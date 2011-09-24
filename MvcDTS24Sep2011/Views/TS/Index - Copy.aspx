<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcApplication2.Models.ReportViewMode>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


    <h2>TS Drive In Out Report</h2>

    <% Html.EnableClientValidation(); %>
    <% using (Html.BeginForm()) {%>
      <%: Html.ValidationSummary(true) %>
         <div align="center">
                              
                                    Start : <%: Html.EditorFor(model => model.dayStart, "Date")%>    &nbsp;
                                    End :   <%: Html.EditorFor(model => model.dayEnd, "Date")%>      &nbsp;
                <input type="submit" value="Search"   class ="buttonsearch"/>
          </div>
	
    <table style=" margin-top:20px; margin-left : auto; margin-right : auto;  ">
        <tr>
            <th>
                SubInv
            </th>
            <th>
                DriveIns
            </th>
            <th>
                DriveOuts
            </th>
            <th>
                DriveInStocks
            </th>
        </tr>
		
    <% for (int i = 0; i < Model.dioVM.Count(); i++ )
	   { %>
    
        <tr>
             <td>
                <%: Model.dioVM[i].SubInv%>
            </td>
            <td>
                <%:  Model.dioVM[i].DriveIns%>
            </td>
            <td>
                <%:  Model.dioVM[i].DriveOuts%>
            </td>
            <td>
                <%:  Model.dioVM[i].DriveInStocks%>
            </td>
        </tr>
    
    <% } %>

    </table>

	<% } %>
 

</asp:Content>

