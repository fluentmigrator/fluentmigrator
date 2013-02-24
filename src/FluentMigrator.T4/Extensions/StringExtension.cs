using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    static class StringExtension
    {
        //  using System.Text.RegularExpressions;

        /// <summary>
        ///  Regular expression built for C# on: sab, dic 1, 2012, 11.00.55 
        ///  Using Expresso Version: 3.0.3634, http://www.ultrapico.com
        ///  
        ///  check guid
        ///  
        ///  A description of the regular expression:
        ///  
        ///  Match expression but don't capture it. [\'], between 0 and 1 repetitions
        ///      Literal '
        ///  [1]: A numbered capture group. [[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}]
        ///      [0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}
        ///          Any character in this class: [0-9a-fA-F], exactly 8 repetitions
        ///          Literal -
        ///          Any character in this class: [0-9a-fA-F], exactly 4 repetitions
        ///          Literal -
        ///          Any character in this class: [0-9a-fA-F], exactly 4 repetitions
        ///          Literal -
        ///          Any character in this class: [0-9a-fA-F], exactly 4 repetitions
        ///          Literal -
        ///          Any character in this class: [0-9a-fA-F], exactly 12 repetitions
        ///  Match expression but don't capture it. [\'], between 0 and 1 repetitions
        ///      Literal '
        ///  
        ///
        /// </summary>
        public static Regex isGuid = new Regex(
              "(?:\\'){0,1}([0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4" +
              "}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12})(?:\\'){0,1}",
            RegexOptions.Multiline
            | RegexOptions.Compiled
            );



        //// Replace the matched text in the InputText using the replacement pattern
        // string result = isGuid.Replace(InputText,isGuidReplace);

        //// Split the InputText wherever the regex matches
        // string[] results = isGuid.Split(InputText);

        //// Capture the first Match, if any, in the InputText
        // Match m = isGuid.Match(InputText);

        //// Capture all Matches in the InputText
        // MatchCollection ms = isGuid.Matches(InputText);

        //// Test to see if there is a match in the InputText
        // bool IsMatch = isGuid.IsMatch(InputText);

        //// Get the names of all the named and numbered capture groups
        // string[] GroupNames = isGuid.GetGroupNames();

        //// Get the numbers of all the named and numbered capture groups
        // int[] GroupNumbers = isGuid.GetGroupNumbers();





        internal static bool IsGuid(this string candidate, out Guid output)
        {
            output = Guid.Empty;
            if (string.IsNullOrEmpty(candidate))
                return false;

            bool isValid = false;

            if (candidate != null)
            {


                var m = isGuid.Match(candidate);
                if (m != null && m.Success)
                {

                    output = new Guid(string.Format("{{{0}}}", m.Groups[1].Value));
                    isValid = true;

                }

            }

            return isValid;

        }

        static internal string Clean(this string source, char start)
        {
            return source.Clean(start, start);
        }

        static internal string Clean(this string source, char start, char end)
        {
            if (string.IsNullOrEmpty(source) == false)
            {
                source = source.Trim();
                if (source.Length > 0)
                {
                    var trimStart = -1;
                    var trimEnd = -1;
                    var chars = source.ToCharArray();
                    var i = 0;
                    var y = chars.Length - 1;
                    while (chars[i++] == start && chars[y--] == end)
                    {
                        trimStart = i;
                        trimEnd = y;
                    }
                    var trimSize = trimEnd - trimStart + 1;
                    if (trimStart > -1 && trimSize > 0)
                        source = source.Substring(trimStart, trimSize);
                }
            }
            return source;
        }


        static internal string CleanBracket(this string source)
        {
            return source.Clean('(', ')');
        }
    }
}
