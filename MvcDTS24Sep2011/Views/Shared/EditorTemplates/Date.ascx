<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%string name = ViewData.TemplateInfo.HtmlFieldPrefix;%>
<% string id = name.Replace(".", "_");%>

    <%= Html.TextBox("", ViewData.TemplateInfo.FormattedModelValue,  new { @class="datePicker" }) %>


 <script type="text/javascript">
     $(function () {
//         $(".datePicker").datepicker();
         $(".datePicker").datepicker({ showOn: 'both', dateFormat: 'd M yy', changeYear: true, changeMonth: true }); 
     });
</script>


