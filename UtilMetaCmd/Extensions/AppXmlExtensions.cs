using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilMetaCmd.Extensions
{
    public static class AppXmlExtensions
    {
        /// <summary>
        /// Retirba o valor da primeira ocorrência da tag no xml repassado
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetXmlTagValue(this string xml, string tag)
        {
            var indexTag = xml.IndexOf("<" + tag + ">");

            if (indexTag < 0)
            {
                return null;
            }

            var indexTagValue = indexTag + tag.Length + 2;
            var indexFechamentoTag = xml.IndexOf("</" + tag + ">");
            var valueLength = indexFechamentoTag - indexTagValue;
            var value = xml.Substring(indexTagValue, valueLength);
            return value;
        }

        public static string RemoveInvalidChars(string strSource)
        {
            return Regex.Replace(strSource, @"[^0-9a-zA-Z=+/]", "");
        }


    }
}
