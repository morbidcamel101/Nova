<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LocationStatistics.aspx.cs" Inherits="NovaService.LocationStatistics" %>

<%@ Register assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" namespace="System.Web.UI.DataVisualization.Charting" tagprefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="background-color:#B3CDD3; margin: 0">
    <form id="form1" runat="server">
    <div style="background-color:#B3CDD3; width: 100%; height: 100%; position: relative;"  >
    
        <asp:Chart ID="LocationChart" runat="server" DataSourceID="NovaDemo" 
            Height="457px" onload="LocationChart_Load" Width="617px" 
            BackColor="179, 205, 211">
            <series>
                <asp:Series ChartType="Pie" Name="CustomerCountSeries" 
            IsValueShownAsLabel="True" Legend="Location" MarkerColor="Red" 
            XValueMember="State" XValueType="String" YValueMembers="CustomerCount" 
            Color="#B3CDD3">
                <SmartLabelStyle Enabled="False" /></asp:Series>
            </series>
            <chartareas>
                <asp:ChartArea Name="ChartArea1" BackColor="#B3CDD3">
                <AxisY IsLabelAutoFit="False"><LabelStyle Angle="90" /></AxisY><AxisX 
            IsLabelAutoFit="False"><LabelStyle Angle="90" /></AxisX></asp:ChartArea>
            </chartareas>
        <Legends><asp:Legend Name="Location" Title="Location"></asp:Legend></Legends></asp:Chart>
        <asp:SqlDataSource ID="NovaDemo" runat="server" 
            ConnectionString="<%$ ConnectionStrings:NovaDemoConnectionString %>" 
            SelectCommand="Customers.sp_GetCustomerByLocationSummary" 
            SelectCommandType="StoredProcedure">
            <SelectParameters>
                <asp:QueryStringParameter DefaultValue="1" Name="LocationMode" 
                    QueryStringField="LocationMode" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    
    </div>
    </form>
</body>
</html>
