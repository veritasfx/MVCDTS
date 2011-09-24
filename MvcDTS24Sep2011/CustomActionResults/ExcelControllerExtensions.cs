using System;
using System.Web.Mvc;
using System.Data.Linq;
using System.Collections;
using System.Web.UI.WebControls;
using System.Linq;

namespace MvcDTS.Controllers
{
    public static class ExcelControllerExtensions
    {

        public static ActionResult Excel
        (
            this Controller controller,
            DataContext dataContext,
            IQueryable rows,
            string fileName
        )
        {
            return new ExcelResult(dataContext, rows, fileName, null, null, null, null);
        }

        public static ActionResult Excel
        (
            this Controller controller,
            DataContext dataContext,
            IQueryable rows,
            string fileName,
            string[] headers
        )
        {
            return new ExcelResult(dataContext, rows, fileName, headers, null, null, null);
        }

        public static ActionResult Excel
        (
            this Controller controller, 
            DataContext dataContext,
            IQueryable rows, 
            string fileName, 
            string[] headers, 
            TableStyle tableStyle, 
            TableItemStyle headerStyle,
            TableItemStyle itemStyle
        )
        {
            return new ExcelResult(dataContext, rows, fileName, headers, tableStyle, headerStyle, itemStyle);
        }

    }
}
