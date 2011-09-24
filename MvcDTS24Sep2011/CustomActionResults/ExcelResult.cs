using System;
using System.Web.Mvc;
using System.Data.Linq;
using System.Collections;
using System.IO;
using System.Web.UI.WebControls;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Drawing;
using MvcDTS.Models;

namespace MvcDTS.Controllers
{
    public class ExcelResult : ActionResult
    {
        private DataContext _dataContext;
        private string _fileName;
        private IQueryable _rows;
        private string[] _headers = null;

        private TableStyle _tableStyle;
        private TableItemStyle _headerStyle;
        private TableItemStyle _itemStyle;

        public string FileName
        {
            get { return _fileName; }
        }

        public IQueryable Rows
        {
            get { return _rows; }
        }


        public ExcelResult(DataContext dataContext, IQueryable rows, string fileName)
            :this(dataContext, rows, fileName, null, null, null, null)
        {
        }

        public ExcelResult(DataContext dataContext, string fileName, IQueryable rows, string[] headers)
            : this(dataContext, rows, fileName, headers, null, null, null)
        {
        }

        public ExcelResult(DataContext dataContext, IQueryable rows, string fileName, string[] headers, TableStyle tableStyle, TableItemStyle headerStyle, TableItemStyle itemStyle)
        {
            _dataContext = dataContext;
            _rows = rows;
            _fileName = fileName;
            _headers = headers;
            _tableStyle = tableStyle;
            _headerStyle = headerStyle;
            _itemStyle = itemStyle;

            // provide defaults
            if (_tableStyle == null)
            {
                _tableStyle = new TableStyle();
                _tableStyle.BorderStyle = BorderStyle.Solid;
                _tableStyle.BorderColor = Color.Black;
                _tableStyle.BorderWidth = Unit.Parse("2px");
            }
            if (_headerStyle == null)
            {
                _headerStyle = new TableItemStyle();
                _headerStyle.BackColor = Color.LightGray;
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            // Create HtmlTextWriter
            StringWriter sw = new StringWriter();
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            // Build HTML Table from Items
            if (_tableStyle != null)
                _tableStyle.AddAttributesToRender(tw);
            tw.RenderBeginTag(HtmlTextWriterTag.Table);

            // Generate headers from table
            if (_headers == null)
            {
                _headers = _dataContext.Mapping.GetMetaType(_rows.ElementType).PersistentDataMembers.Select(m => m.Name).ToArray();
            }


            // Create Header Row
            tw.RenderBeginTag(HtmlTextWriterTag.Thead);
            foreach (String header in _headers)
            {
                if (_headerStyle != null)
                    _headerStyle.AddAttributesToRender(tw);
                tw.RenderBeginTag(HtmlTextWriterTag.Th);
                tw.Write(header);
                tw.RenderEndTag();
            }
            tw.RenderEndTag();

            

            // Create Data Rows
            tw.RenderBeginTag(HtmlTextWriterTag.Tbody);
			foreach (DriveInOutViewModel row in _rows)
            {
				//DriveInOutViewModel vm = (DriveInOutViewModel)row;
                tw.RenderBeginTag(HtmlTextWriterTag.Tr);
				string NoStr = null;
				if (row.No > 0)
				{
					NoStr = row.No.ToString();
				}
				RenderRow(tw, NoStr);
				RenderRow(tw, row.SubInv);
				//RenderRow(tw, row.DESCRIPT);
				RenderRow(tw, row.FamilyName);
				//foreach (string header in _headers)
				foreach(int i in row.CountBackDays)
                {
					//strValue = row.GetType().GetProperty(header).GetValue(row, null).ToString();
					RenderRow(tw, i.ToString());
                }
                tw.RenderEndTag();
            }
            tw.RenderEndTag(); // tbody

            tw.RenderEndTag(); // table
            WriteFile(_fileName, "application/ms-excel", sw.ToString());            
        }

		private string RenderRow(HtmlTextWriter tw, string strValue)
		{
			if (!string.IsNullOrEmpty(strValue))
			{
				strValue = ReplaceSpecialCharacters(strValue);
			}
			if (_itemStyle != null)
				_itemStyle.AddAttributesToRender(tw);
			tw.RenderBeginTag(HtmlTextWriterTag.Td);
			tw.Write(HttpUtility.HtmlEncode(strValue));
			tw.RenderEndTag();
			return strValue;
		}


        private static string ReplaceSpecialCharacters(string value)
        {
            value = value.Replace("’", "'");
            value = value.Replace("“", "\"");
            value = value.Replace("”", "\"");
            value = value.Replace("–", "-");
            value = value.Replace("…", "...");
            return value;
        }

        private static void WriteFile(string fileName, string contentType, string content)
        {
            HttpContext context = HttpContext.Current;
            context.Response.Clear();
            context.Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            context.Response.Charset = "";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.ContentType = contentType;
            context.Response.Write(content);
            context.Response.End();
        }
    }
}
