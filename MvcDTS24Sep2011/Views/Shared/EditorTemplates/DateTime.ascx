<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DateTime?>" %>

<%string name = ViewData.TemplateInfo.HtmlFieldPrefix;%>
<%string id = name.Replace(".", "_");%>

    <%= Html.TextBox("", ViewData.TemplateInfo.FormattedModelValue,  new { @class="datetimePicker" }) %>
    <%= Html.ValidationMessageFor(model => model) %>


<script type="text/javascript">
     $(function () {
         //$(document).ready(function () {
     	$("#<%=id%>").datetimepicker({ showOn: 'both', buttonImage: "../../images/icon_calendar.gif", dateFormat: 'dd mmm yyyy H:MM', changeYear: true, changeMonth: true });
	 });

</script>    
      
