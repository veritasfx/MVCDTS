using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.IO;
using MvcDTS.Models;
using System.Text;

namespace System.Web.Mvc.Html
{
    public static class DataGridHelper
    {
	

        public static string DataGrid<T>(this HtmlHelper helper)
        {
            return DataGrid<T>(helper, null, null);
        }

        public static string DataGrid<T>(this HtmlHelper helper, object data)
        {
            return DataGrid<T>(helper, data, null);
        }

        public static string DataGrid<T>(this HtmlHelper helper, object data, string[] columns)
        {
            //Get items
            var items = (IEnumerable<T>)data;
            if (items == null)
                items = (IEnumerable<T>)helper.ViewData.Model;

            //Get column names 
            if (columns == null)
            {
                columns = typeof(T).GetProperties().Select(p => p.Name).ToArray();
				List<string> temp = columns.ToList();
				temp.Remove("DESCRIPT");
				columns = temp.ToArray();

           }

            //Create HtmlTextWriter 
            var writer = new HtmlTextWriter(new StringWriter());

            //Open table tag 
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            //Render Table Header 
            writer.RenderBeginTag(HtmlTextWriterTag.Thead);
            RenderHeader(helper, writer, columns);
            writer.RenderEndTag();

            // Render table body 
            writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

            int _pageSize = (int)helper.ViewDataContainer.ViewData["PageSize"];
            int i = ((int)helper.ViewDataContainer.ViewData["CurrentPage"] - 1) * _pageSize;

            foreach (var item in items)
            {
                if (item != null)
                    RenderRow<T>(helper, writer, columns, item, ++i);
            }
            writer.RenderEndTag();

            //Close  table tag 
            writer.RenderEndTag();

            //return the string 
            return writer.InnerWriter.ToString();
        }

        private static void RenderHeader(HtmlHelper helper, HtmlTextWriter writer, string[] columns)
        {
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			foreach (var columnName in columns)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Th);

				if (columnName == "No")
				{
					writer.Write(columnName);
				}
				else if (columnName == "SubInv")
				{
					List<string> subInvCodeList = (List<string>)helper.ViewDataContainer.ViewData["SUB_INV_CODEs"];
					string curSubInvCode = (string)helper.ViewDataContainer.ViewData["curSubInvCode"]; 
					List<SelectListItem> si = GetSelectList(helper, subInvCodeList, curSubInvCode);
					var link = columnName + helper.DropDownList("curSubInvCode", si, new { onChange = "this.form.submit()" });
					writer.Write(link);
				}
				else if (columnName == "FamilyName")
				{
					bool showFamilyName = (bool)helper.ViewDataContainer.ViewData["ShowFamilyName"];
					if (!showFamilyName)
					{
						writer.Write("SubInv Description");
					}
					else
					{
						List<string> familyNameList = (List<string>)helper.ViewDataContainer.ViewData["FamilyNames"];
						string curFamilyName = (string)helper.ViewDataContainer.ViewData["curFamilyName"];
						List<SelectListItem> si = GetSelectList(helper, familyNameList, curFamilyName);
						var link = columnName + helper.DropDownList("curFamilyName", si, new { onChange = "this.form.submit()" });
						writer.Write(link);
					}
				}
				else if (columnName == "CountBackDays")
				{
					List<string> headers = (List<string>)helper.ViewDataContainer.ViewData["Headers"];
					headers.RemoveRange(0, 3);
					for(int k = 0; k < headers.Count; k++)
					{
						if (k > 0)
						{
							writer.RenderBeginTag(HtmlTextWriterTag.Th);
						}
						writer.Write(headers[k]);
						if (k < headers.Count - 1)
						{
							writer.RenderEndTag();
						}
					}
				}
				else
				{
					var currentAction = (string)helper.ViewContext.RouteData.Values["action"];
					var head = ReplaceHeader(columnName);
					var link = helper.ActionLink(head, currentAction, new { sort = columnName, searchContent = helper.ViewDataContainer.ViewData["searchContent"] });
					writer.Write(link);
				}
				writer.RenderEndTag();
			}

			writer.RenderEndTag();
        }  

        //  <td>
        //    <%: Html.ActionLink("Edit", "Edit", new { id=item.ID }) %> |
        //    <%: Html.ActionLink("Delete", "Delete", new { id=item.ID })%>
        //  </td>
        public static void RenderRow<T>(HtmlHelper helper, HtmlTextWriter write, string[] columns, T item, int i)
        {
			//List<int> SubInvFamilyNos = (List<int>)helper.ViewDataContainer.ViewData["SubInvFamilyNos"];
			bool showFamilyName = (bool)helper.ViewDataContainer.ViewData["ShowFamilyName"];

			int No = (int)(typeof(T).GetProperty("No").GetValue(item, null) ?? String.Empty);
			string subInv = (string)typeof(T).GetProperty("SubInv").GetValue(item, null) ?? String.Empty;
			if (No == 0 && showFamilyName || subInv == "SUM()")
			{
				string temp = "tr style=\" background-color: #e8eef4;\" ";
				write.RenderBeginTag(temp);
			}
			else
			{
				write.RenderBeginTag(HtmlTextWriterTag.Tr);
			}

            foreach (var columnName in columns)
            {
	            var value = typeof(T).GetProperty(columnName).GetValue(item, null) ?? String.Empty;
				if (columnName == "No")
				{
						string temp = "td style=\" font-weight: 20;\" ";
						write.RenderBeginTag(temp);
						if ((int)value > 0)
						{
							write.Write(helper.Encode(value.ToString()));
						}
						else
						{
						}
						write.RenderEndTag();
	
				}
				else if (columnName == "CountBackDays")
				{
					if (value != null && value != "")
					{
						List<int> tempList = (List<int>)value;
						for (int k = 0; k < tempList.Count; k++ )
						{
							if (k == tempList.Count - 1)
							{
								string temp1 = "td style=\"background-color: #e8eef4\" ";
								write.RenderBeginTag(temp1);
							}
							else if (tempList[k] != 0 && showFamilyName)
							{
								string temp1 = "td style=\" font-weight: bold;\" ";
								write.RenderBeginTag(temp1);
							}
							else 
							{
								write.RenderBeginTag(HtmlTextWriterTag.Td);
							}
							if (!(No == 0 && tempList[k] == 0))
							{
								write.Write(helper.Encode(tempList[k].ToString()));
							}
							write.RenderEndTag();
						}
					}
				}
				else
				{
					write.RenderBeginTag(HtmlTextWriterTag.Td);
					write.Write(helper.Encode(value.ToString()));
					write.RenderEndTag();
				}
               
            }
             write.RenderEndTag();

			if (No == 0 && showFamilyName)
			 {
				 write.RenderBeginTag(HtmlTextWriterTag.Tr);
				 write.RenderBeginTag(HtmlTextWriterTag.Td);
				 write.RenderEndTag();
				 write.RenderEndTag();
			 }

        }


        //private static void RenderHeaderOneLine(HtmlHelper helper, HtmlTextWriter writer, string[] columns)
        //{
        //        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
        //        foreach (var columnName in columns)
        //        {
        //              writer.RenderBeginTag(HtmlTextWriterTag.Th);
        //              SMTVVideoDBEntities _entities = new SMTVVideoDBEntities();
        //              if (columnName == "Category")
        //              {
        //                  List<SelectListItem> si = GetCategorySelectList(helper, _entities);
        //                  var link = columnName + helper.DropDownList("categoryID", si, new { onChange = "this.form.submit()" });
        //                  writer.Write(link);
        //              }
        //              else if (columnName == "YouTubeChannel")
        //              {
        //                  List<SelectListItem> si = GetUploadChannelSelectList(helper, _entities);
        //                  var link = columnName + helper.DropDownList("uploadChannelID", si, new { onChange = "this.form.submit()" });
        //                  writer.Write(link);
        //              }
        //              else if (columnName == "YTUpload")
        //              {
        //                  List<SelectListItem> si = GetYouTubeVideoIDSelectList(helper, _entities);
        //                  var link = columnName + helper.DropDownList("uploaded", si, new { onChange = "this.form.submit()" });
        //                  writer.Write(link);
        //                  writer.Write("Upload | Update");
        //              }
        //              else
        //              {
        //                  var currentAction = (string)helper.ViewContext.RouteData.Values["action"];
        //                  var link = helper.ActionLink(columnName, currentAction, new { sort = columnName, searchContent = helper.ViewDataContainer.ViewData["searchContent"] });
        //                  writer.Write(link);
        //              }
        //              writer.RenderEndTag();
        //        }
        //        writer.RenderEndTag();
        //}

      
        private static string ReplaceHeader(string columnName)
        {
            string newStr = columnName;
            if (columnName == "SMTVVideoID")
            {
                newStr = "SMTV VideoID";
            }
            else if (columnName == "PartNo")
            {
                newStr = "Part No";
            }
            else if (columnName == "SubPartNo")
            {
                newStr = "Sub Part No";
            }
            else if (columnName == "ID")
            {
                newStr = "S/N";
            }
            else if (columnName == "MemberFeeExpiredDate")
            {
                //newStr = "MemberFee Expired Date";
                newStr = "MemberFee Expired by";
            }
            else if (columnName == "IDCardNumber")
            {
                newStr = "ID No";
            }
            else if (columnName == "TotalParts")
            {
                newStr = "Total Parts";
            }
            else if (columnName == "Duration")
            {
                newStr = "Duration (h:m:s)";
            }
            //else if(columnName == "MemberType")
            //{
            //    newStr = "MemberType";
            //}
            else if (columnName == "ContactNo")
            {
                newStr = "Contact No";
            }
            else if (columnName == "ICOrPassportNo")
            {
                newStr = "IC/Passport No";
            }
            //else if (columnName == "UpdateTime")
            //{
            //    newStr = "EditDoneTime";
            //    //newStr = "UpdateTime (UTC)";
            //}
            return newStr;
        }

		//private static List<SelectListItem> GetSubInvCodeSelectList(HtmlHelper helper)
		//{
			
		//    List<string> subInvCodeList = (List<string>)helper.ViewDataContainer.ViewData["SUB_INV_CODEs"];
		//    int curSubInvCodeID = (int)helper.ViewDataContainer.ViewData["curSubInvCodeID"];
		//    int allID = subInvCodeList.Count() + 1;

		//    List<SelectListItem> si = new List<SelectListItem>();
		//    if (curSubInvCodeID == allID)
		//    {
		//        si.Add(new SelectListItem { Text = "All", Value = allID.ToString(), Selected = true });
		//    }
		//    else
		//    {
		//        si.Add(new SelectListItem { Text = "All", Value = allID.ToString() });
		//    }

		//    int j = 1;

		//    foreach (string subInvCode in subInvCodeList)
		//    {
		//        SelectListItem item = new SelectListItem { Text = subInvCode, Value = j.ToString() };
		//        if (curSubInvCodeID == j)
		//        {
		//            item.Selected = true;
		//        }
		//        si.Add(item);
		//        j++;
		//    }
		//    return si;
		//}

    
		private static List<SelectListItem> GetSelectList(HtmlHelper helper, List<string> DisplayList, string currentStr)
		{

			List<SelectListItem> si = new List<SelectListItem>();
			if (currentStr == "All")
			{
				si.Add(new SelectListItem { Text = "All", Value = "All", Selected = true });
			}
			else
			{
				si.Add(new SelectListItem { Text = "All", Value = "All" });
			}

			int j = 1;

			foreach (string displayText in DisplayList)
			{
				SelectListItem item = new SelectListItem { Text = displayText, Value = displayText }; //j.ToString() };
				if (currentStr == displayText)// j)
				{
					item.Selected = true;
				}
				si.Add(item);
				j++;
			}
			return si;
		}



    }
}



 
