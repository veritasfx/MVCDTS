<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<MvcDTS.Models.DriveInOutViewModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Inventory Aging Report
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Inventory Aging Report</h2>

	<% using (Html.BeginForm())
	{%>
		<table style="border:0">
		<tr>
		<td style="border:0; left: 0">
			<%= Html.ActionLink("Get Excel", "GenerateExcel2", "TS")%>  
			</td>
			<td style="border:0; width:70%"></td>
			<td  style="border:0; right: 0">Show FmailyName? <%: Html.CheckBox("showFamilyName") %>  <input type="submit" value="Refresh" name = "Refresh"  />
			</td>
		</tr>
		</table>
	
		<%=Html.DataGrid<MvcDTS.Models.DriveInOutViewModel>()%>
	<%--	<%=Html.DataGrid<MvcDTS.Models.DriveInOutViewModel>(Model, (string[])ViewData["headers"])%>--%>

  <%} %>

	  <div align="center"  style="margin-top:10px" > 
        <%=Html.PageLink((int)ViewData["CurrentPage"], (int)ViewData["TotalPages"], p => Url.Action("Index", new { page = p, sort = (string)ViewData["SortItem"], curFamilyName = ViewData["curFamilyName"], curSubInvCode = ViewData["curSubInvCode"], searchContent = ViewData["searchContent"], showFamilyName = (bool)ViewData["ShowFamilyName"] }))%>
  </div>

<%--	<table>
        <tr>
		<th></th>
		<% foreach (string header in headers)
	 { %>
			<th> <%: header%></th>
			<%} %>
       </tr>

    <% int index = 0;
    	foreach (var item in Model)
		{%>
         <tr>
		 <td><%: (++index).ToString() %></td>
              <td>
                <%: item.SubInv %>
            </td>              
			<td>
                <%: item.FamilyName%>
            </td>
				<%  for (int i = 0; i < Model.FirstOrDefault().CountBackDays.Count; i++)
		{%>
					<td><%: item.CountBackDays[i].ToString()%></td>
				<%} %>
        </tr>
    
    <% } %>
	
    </table>
--%>

</asp:Content>

