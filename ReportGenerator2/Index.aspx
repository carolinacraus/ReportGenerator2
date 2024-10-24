<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Index.aspx.vb" Inherits="ReportGenerator2.Index" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Report Generator</title>
    <style>
        /* General Body Styling */
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            color: #333;
            margin: 0;
            padding: 0;
        }

        /* Container to center content and add spacing */
        .container {
            display: flex;
            flex-direction: column;
            align-items: center; /* Center horizontally */
            justify-content: center; /* Center vertically */
            margin-top: 50px; /* Space at the top */
        }

        /* Input Section Styling */
        .input-section {
            background-color: #fff;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
            margin-bottom: 30px; /* Space between input section and report */
            width: 80%; /* Adjust width of input box */
            max-width: 600px;
        }

        /* Label and Input Alignment */
        label {
            display: block;
            margin-top: 10px;
            font-weight: bold;
        }

        input[type="text"], select {
            width: 100%;
            padding: 10px;
            margin-top: 5px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }

        /* Button Styling */
        .input-section button, .input-section .aspNetButton {
            background-color: #007BFF;
            color: white;
            border: none;
            padding: 10px 20px;
            margin-top: 15px;
            cursor: pointer;
            border-radius: 5px;
            font-size: 16px;
        }

        .input-section button:hover, .input-section .aspNetButton:hover {
            background-color: #0056b3;
        }

        /* Message Label Styling */
        .input-section .error-message {
            color: red;
            margin-top: 15px;
            display: block;
        }

        /* GridView Section Styling */
        .gridview-section {
            margin-top: 30px;
            width: 90%;
            max-width: 800px;
        }

        .gridview-section table {
            border-collapse: collapse;
            width: 100%;
            background-color: #fff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
        }

        .gridview-section th, .gridview-section td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }

        .gridview-section th {
            background-color: #007BFF;
            color: white;
        }

        .gridview-section tr:nth-child(even) {
            background-color: #f2f2f2;
        }

        .gridview-section td {
            font-size: 14px;
        }

        /* Mobile Friendly Adjustments */
        @media (max-width: 768px) {
            .input-section, .gridview-section {
                width: 95%;
            }
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <!-- Input Section -->
            <div class="input-section">
                <h2>OmniFunds Report</h2>
                <label for="txtPeriodStart">Period Start:</label>
                <asp:TextBox ID="txtPeriodStart" runat="server"></asp:TextBox>
                <br />
                <label for="txtPeriodEnd">Period End:</label>
                <asp:TextBox ID="txtPeriodEnd" runat="server"></asp:TextBox>
                <br />
                <label for="ddlCustNum">Select Customer:</label>
                <asp:DropDownList ID="ddlCustNum" runat="server"></asp:DropDownList>
                <br />
                <asp:Button ID="btnGenerate" runat="server" Text="Generate Report" CssClass="aspNetButton" />
                <asp:Button ID="btnDownloadCSV" runat="server" Text="Download CSV" CssClass="aspNetButton" />
                <br /><br />
                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>
            </div>

            <!-- GridView Section -->
            <div class="gridview-section">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="True" CssClass="gridview" />
            </div>
        </div>
    </form>
</body>
</html>
