﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Markdig;

namespace NHS111.Utils.Extensions
{
    public static class StringExtension
    {
        public static HttpResponseMessage AsHttpResponse(this string s)
        {
            return new HttpResponseMessage { Content = new StringContent(s) };
        }

        public static string FirstToUpper(this string s)
        {
            if (s == null) 
                return null;
            
            if (s.Length > 1) 
                return char.ToUpper(s[0]) + s.Substring(1);
            
            return s.ToUpper();
        }

        public static string ToTitleCase(this string s) {
            if (string.IsNullOrEmpty(s))
                return s;

            s = s.ToLower();
            return FirstToUpper(s);
        }

        public static string ParseForMarkdown(this string s, HtmlGenericControl enclosingTag)
        {
            if(s.StartsWith("!markdown!"))
                return Markdown.ToHtml(s.Replace("!markdown!", string.Empty).Replace("/r/n", Environment.NewLine));

            enclosingTag.InnerHtml = s;
            return GenerateHtml(enclosingTag);
        }

        private static string GenerateHtml(HtmlGenericControl tag)
        {
            var generatedHtml = new StringBuilder();
            using (var htmlStringWriter = new StringWriter(generatedHtml))
            {
                using (var htmlTextWriter = new HtmlTextWriter(htmlStringWriter))
                {
                    tag.RenderControl(htmlTextWriter);
                    return generatedHtml.ToString();
                }
            }
        }
    }
}