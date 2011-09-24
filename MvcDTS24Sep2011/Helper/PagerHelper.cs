using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace System.Web.Mvc.Html
{
    public static class PageLinkHelper
    {
        //public static string PageLink(this HtmlHelper html, int currentPage, int totalPages, Func<int, string> pageUrl)
        //{
        //     var diff = 1;
        //    StringBuilder result = new StringBuilder();
        //    var TotalPages = totalPages;

        //    if (currentPage < 1)
        //        currentPage = 1;

        //    if ((currentPage + diff) < totalPages)
        //        totalPages = currentPage + diff;
        //    var startPage = 1;
        //    if ((currentPage - diff) > startPage)
        //        startPage = currentPage - diff;

        //    if ((currentPage - diff) > 1)
        //    {
        //        TagBuilder tag3 = new TagBuilder("a");
        //        tag3.Attributes.Add("href", pageUrl(1));
        //        tag3.InnerHtml = "1";
        //        result.AppendLine(tag3.ToString());
        //    }

        //    if ((currentPage - (diff + 1)) > 1)
        //    {
        //        TagBuilder tag2 = new TagBuilder("a");
        //        tag2.Attributes.Add("href", pageUrl(currentPage - (diff + 1)));
        //        tag2.InnerHtml = "...";
        //        result.AppendLine(tag2.ToString());
        //    }

        //    for (int i = startPage; i <= totalPages; i++)
        //    {
        //        TagBuilder tag = new TagBuilder("a");
        //        //tag.Attributes.Add("href", pageUrl(i));
        //        tag.InnerHtml = i.ToString();
        //        if (i == currentPage)
        //        {
        //             tag.AddCssClass("pageSelected");
        //        }
        //        else
        //        {
        //               tag.Attributes.Add("href", pageUrl(i));
        //               if (i == startPage)
        //               {
        //                   tag.InnerHtml = "Prev";
        //               }
        //               else
        //               {
        //                   tag.InnerHtml = "Next";
        //               }
        //        }
        //        result.AppendLine(tag.ToString());
        //    }

        //    if ((currentPage + (diff + 1)) < TotalPages)
        //    {
        //        TagBuilder tag2 = new TagBuilder("a");
        //        tag2.Attributes.Add("href", pageUrl(currentPage + (diff + 1)));
        //        tag2.InnerHtml = "...";
        //        result.AppendLine(tag2.ToString());
        //    }

        //    if ((currentPage + diff) < TotalPages)
        //    {
        //        TagBuilder tag3 = new TagBuilder("a");
        //        tag3.Attributes.Add("href", pageUrl(TotalPages));
        //        tag3.InnerHtml = TotalPages.ToString();
        //        result.AppendLine(tag3.ToString());
        //    }
        //    return result.ToString();
        //}

        public static string PageLink(this HtmlHelper html, int currentPage, int totalPages, Func<int, string> pageUrl)
        {
             StringBuilder result = new StringBuilder();
   
            if (currentPage < 1)
                currentPage = 1;

            for (int i = 1; i <= totalPages; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                tag.InnerHtml = i.ToString();
                if (i == currentPage)
                {
                    tag.AddCssClass("pageSelected");
                }
                else
                {
                    tag.Attributes.Add("href", pageUrl(i));
                    tag.InnerHtml = i.ToString();
                 }
                result.AppendLine(tag.ToString());
            }

             return result.ToString();
        }

    }

    public static class HtmlHelpers
    {
        public static string Truncate(this HtmlHelper helper, string input, int length)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Length <= length)
                {
                    return input;
                }
                else
                {
                    return input.Substring(0, length) + "...";
                }
            }
            else
                return null;
        }
    }
}

