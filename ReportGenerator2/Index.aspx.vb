Imports System.IO
Imports System.Text
Imports ReportGenerator2.DAL
Imports ReportGenerator2.Models

Partial Class Index
    Inherits System.Web.UI.Page

    ' Instance of the DatabaseHelper class to interact with the database
    Private dbHelper As New DatabaseHelper()

    ' This method is triggered when the page loads
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadAssociates()
        End If
    End Sub

    ' Load associates for the dropdown list
    Private Sub LoadAssociates()
        Dim associates As List(Of Associate) = dbHelper.GetAssociates()
        ddlCustNum.DataSource = associates
        ddlCustNum.DataTextField = "CustName"
        ddlCustNum.DataValueField = "CustNum"
        ddlCustNum.DataBind()

        ddlCustNum.Items.Insert(0, New ListItem("-- Select Customer --", "0"))
    End Sub

    ' Generate Report button click event
    Protected Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        If ddlCustNum.SelectedValue = "0" Then
            lblMessage.Text = "Please select a customer."
            Return
        End If

        Dim periodStart As String = txtPeriodStart.Text
        Dim periodEnd As String = txtPeriodEnd.Text
        Dim custNum As Integer = Integer.Parse(ddlCustNum.SelectedValue)

        ' Fetch report data from the database
        Dim reportData As List(Of ReportData) = dbHelper.GetReportData(periodStart, periodEnd, custNum)

        ' Bind the fetched data to the GridView
        GridView1.DataSource = reportData
        GridView1.DataBind()

        If reportData.Count = 0 Then
            lblMessage.Text = "No data found for the selected period and customer."
        Else
            lblMessage.Text = ""  ' Clear the message if data is found
        End If
    End Sub

    ' Download CSV button click event
    Protected Sub btnDownloadCSV_Click(sender As Object, e As EventArgs)
        ' Ensure the GridView contains data before generating CSV
        If GridView1.Rows.Count = 0 Then
            lblMessage.Text = "No data available for download."
            Return
        End If

        ' Create the CSV data
        Dim csvData As StringBuilder = New StringBuilder()

        ' Add header row
        For Each cell As TableCell In GridView1.HeaderRow.Cells
            csvData.Append(cell.Text + ",")
        Next
        csvData.AppendLine()

        ' Add data rows
        For Each row As GridViewRow In GridView1.Rows
            For Each cell As TableCell In row.Cells
                csvData.Append(cell.Text + ",")
            Next
            csvData.AppendLine()
        Next

        ' Send the CSV file as a response
        Response.Clear()
        Response.Buffer = True
        Response.AddHeader("content-disposition", "attachment;filename=Report.csv")
        Response.Charset = ""
        Response.ContentType = "application/text"
        Response.Output.Write(csvData.ToString())
        Response.Flush()
        Response.End()
    End Sub
End Class
