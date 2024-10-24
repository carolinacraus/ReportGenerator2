Imports System.Data.SqlClient
Imports ReportGenerator2.Models

Namespace DAL
    Public Class DatabaseHelper
        ' Property to store the formatted start date
        Public Property FormattedPeriodStart As String
        ' Property to store the formatted end date
        Public Property FormattedPeriodEnd As String
        ' Property to store the final query with replaced placeholders
        Public Property FinalQuery As String

        ' Connection string (make sure this matches your Web.config settings)
        Private connectionString As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        ' Method to get associates from the database
        Public Function GetAssociates() As List(Of Associate)
            Dim associates As New List(Of Associate)()

            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Using cmd As New SqlCommand("SELECT CustNum, CustName FROM Associates ORDER BY CustName", conn)
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim associate As New Associate() With {
                            .CustNum = reader.GetInt32(0),
                            .CustName = reader.GetString(1)
                        }
                        associates.Add(associate)
                    End While
                End Using
            End Using

            Return associates
        End Function

        ' Method to get public Premium OmniFunds (OF1, OF2, etc.) for a given customer
        Public Function GetOfConfigIds(custNum As Integer) As List(Of Integer)
            Dim ofConfigIds As New List(Of Integer)()

            ' SQL query to get public Premium OmniFunds
            Dim query As String = "
            SELECT co.ConfigId 
            FROM [OmniFundsV2].dbo.Configurations co 
            WHERE CalculationMethod = 9 
              AND DisplayOnWebSite = 1 
              AND Deleted = 0 
              AND Archived = 0 
              AND Experiment = 0 
              AND co.ConfigID IN (
                  SELECT ConfigId 
                  FROM [OmniFundsV2].dbo.AssociatesOmniFunds 
                  WHERE CustNum = @CustNum
              )
              AND co.ConfigId IN (
                  SELECT ConfigId 
                  FROM [OmniFundsV2].dbo.Configuration_Portfolios 
                  WHERE PortfolioId IN (
                      SELECT ci.ConfigId 
                      FROM [OmniFundsV2].[dbo].[Configuration_ItemCodes] ci 
                      JOIN Configurations co ON ci.configid = co.configid 
                      WHERE itemcode = 5151 
                        AND displayonwebsite = 1
                  )
              )"

            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@CustNum", custNum)
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        ofConfigIds.Add(reader.GetInt32(0)) ' Adding ConfigId to the list
                    End While
                End Using
            End Using

            Return ofConfigIds
        End Function

        ' Method to get custom OmniFunds (C1, C2, etc.) for a given customer
        Public Function GetCustomConfigIds(custNum As Integer) As List(Of Integer)
            Dim customConfigIds As New List(Of Integer)()

            ' SQL query to get custom OmniFunds
            Dim query As String = "
            SELECT co.ConfigId 
            FROM [OmniFundsV2].dbo.Configurations co 
            WHERE CalculationMethod = 9 
              AND DisplayOnWebSite = 0 
              AND Deleted = 0 
              AND Archived = 0 
              AND Experiment = 0 
              AND co.ConfigID IN (
                  SELECT ConfigId 
                  FROM [OmniFundsV2].dbo.AssociatesOmniFunds 
                  WHERE CustNum = @CustNum
              )
              AND co.ConfigID IN (
                  SELECT ConfigId 
                  FROM [OmniFundsV2].dbo.Configuration_Portfolios 
                  WHERE PortfolioId IN (
                      SELECT ci.ConfigId 
                      FROM [OmniFundsV2].[dbo].[Configuration_ItemCodes] ci 
                      JOIN Configurations co ON ci.configid = co.configid 
                      WHERE itemcode = 5151 
                        AND calculationmethod <> 9 
                        AND displayonwebsite = 1
                  )
              )"

            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@CustNum", custNum)
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        customConfigIds.Add(reader.GetInt32(0)) ' Adding ConfigId to the list
                    End While
                End Using
            End Using

            Return customConfigIds
        End Function

        ' Method to get report data based on input dates and customer number
        Public Function GetReportData(periodStart As String, periodEnd As String, custNum As Integer) As List(Of ReportData)
            Dim reportData As New List(Of ReportData)()

            ' Parse the string inputs into DateTime objects
            Dim periodStartDate As DateTime
            Dim periodEndDate As DateTime

            ' Attempt to parse the date strings to DateTime
            If Not DateTime.TryParse(periodStart, periodStartDate) Then
                Throw New ArgumentException("Invalid period start date format.")
            End If

            If Not DateTime.TryParse(periodEnd, periodEndDate) Then
                Throw New ArgumentException("Invalid period end date format.")
            End If

            ' Format the dates as M/d/yyyy and store them in properties as strings
            FormattedPeriodStart = periodStartDate.ToString("M/d/yyyy")
            FormattedPeriodEnd = periodEndDate.ToString("M/d/yyyy")

            ' Get OF and Custom Config IDs
            Dim ofConfigIds As List(Of Integer) = GetOfConfigIds(custNum)
            Dim customConfigIds As List(Of Integer) = GetCustomConfigIds(custNum)

            ' Prepare placeholders for SQL IN clause
            Dim ofPlaceholders As String = If(ofConfigIds.Any(), String.Join(",", ofConfigIds), "NULL")
            Dim customPlaceholders As String = If(customConfigIds.Any(), String.Join(",", customConfigIds), "NULL")

            ' SQL query for the report with IIf and DATEDIFF as per your request
            Dim query As String = "
            SELECT co.Name AS OmniFund,
                   ac.CustNum,
                   ac.FullName,
                   au.Name AS AutoTradeLevel,
                   IIf(re.RegDate < '" & FormattedPeriodStart & "', '" & FormattedPeriodStart & "', re.RegDate) AS BillStartDate,
                   DATEDIFF(day, IIf(re.RegDate < '" & FormattedPeriodStart & "', '" & FormattedPeriodStart & "', re.RegDate), 
                            IIf(re.ExpDate < '" & FormattedPeriodEnd & "', re.ExpDate, '" & FormattedPeriodEnd & "')) + 1 AS NumDays
            FROM [TradeProcessorOFV2].[dbo].[BrokerProducts] bp
            JOIN OmniFundsV2.dbo.Accounts ac ON bp.ProductAccount = ac.AccountID
            JOIN OmniFundsV2.dbo.Configurations co ON co.ConfigID = ac.ConfigID
            LEFT JOIN CustSys.dbo.Register re ON ac.CustNum = re.CustNum
                AND re.RegDate <= '" & FormattedPeriodEnd & "'
                AND re.ExpDate >= '" & FormattedPeriodStart & "'
                AND re.ProdCode IN (21501, 21502, 21503, 21504, 21505, 21506, 21507)
            LEFT JOIN [TradeProcessorOFV2].[dbo].AutoTradeLevels au ON au.ItemCode = re.ProdCode
            WHERE bp.autotrade = 1
              AND bp.BrokerAccount NOT LIKE 'Paper%'
              AND re.RegDate <= '" & FormattedPeriodEnd & "'
              AND (co.ConfigID IN (" & ofPlaceholders & ") OR co.ConfigID IN (" & customPlaceholders & "))
            ORDER BY co.Name, ac.FullName"

            ' Store the final query for debugging or display purposes
            FinalQuery = query

            ' Execute the query
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Using cmd As New SqlCommand(query, conn)
                    ' Execute the query and process results
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim data As New ReportData() With {
                            .OmniFund = reader.GetString(0),
                            .CustNum = reader.GetInt32(1),
                            .FullName = reader.GetString(2),
                            .AutoTradeLevel = reader.GetString(3),
                            .BillStartDate = reader.GetDateTime(4).ToString("M/d/yyyy"),
                            .NumDays = reader.GetInt32(5)
                        }
                        reportData.Add(data)
                    End While
                End Using
            End Using

            Return reportData
        End Function
    End Class
End Namespace
