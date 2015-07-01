using BCD.Common;
using BCD.DataAccessLayer;
using Downloads.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BCD.Generic
{
    public class GenericFileTypeClass : SearchBase
    {
        public CompanyInsight_DevEntities context;// = new ConferenceEntities();
        public string ISSBN;
        public string pubStePtrn;
        public string Scenario;
        public virtual void FileTypeProcessData(string webPage)
        {

            throw new NotImplementedException();
        }
        public string convertSingleSpaces(string Data)
        {
            Data = Regex.Replace(Data, @"\t|\n|\r", "");
            Data = Regex.Replace(Data, @"\s+", " ");
            return Data;
        }
        public string TrimData(string txtData)
        {
            if (txtData.Trim().Length > 0)
            {
                for (int i = 0; i < txtData.Length; i++)
                {
                    txtData = txtData.Trim().TrimStart(new char[] { '-', '.', ':', ',', '&', '*', ';' }).Trim();
                    txtData = txtData.Trim().TrimEnd(new char[] { '-', '.', ':', ',', '&', '*', '(', ';' }).Trim();

                }
            }
            return txtData;
        }
        public string GetFirstWord(string webPageContent)
        {

            var matches = Regex.Matches(webPageContent, @"\w+[^\s]*\w+|\w");
            foreach (var match in matches)
            {
                if (match.ToString().Trim().Length > 1)
                {
                    return match.ToString();

                }


            }
            return string.Empty;

        }
        public String TitleCaseString(String s)
        {
            if (s == null || s.Trim().Length == 0) return s;

            String[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = "";
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1).ToLower();
                }
                words[i] = firstChar + rest;
            }
            return String.Join(" ", words);
        }
    }
}
