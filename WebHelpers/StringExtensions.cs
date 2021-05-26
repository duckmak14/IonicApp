using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Common
{
    public static class StringExtensions
    {
        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        private delegate void ActionLine(TextWriter textWriter, string line);

        #region Char extensions

        [DebuggerStepThrough]
        public static int ToInt(this char value)
        {
            if ((value >= '0') && (value <= '9'))
            {
                return (value - '0');
            }
            if ((value >= 'a') && (value <= 'f'))
            {
                return ((value - 'a') + 10);
            }
            if ((value >= 'A') && (value <= 'F'))
            {
                return ((value - 'A') + 10);
            }
            return -1;
        }

        

         

        #endregion

        #region String extensions

        [DebuggerStepThrough]
        public static T ToEnum<T>(this string value, T defaultValue)
        {
            if (!value.HasValue())
            {
                return defaultValue;
            }
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        [DebuggerStepThrough]
        public static string ToSafe(this string value, string defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }
            return (defaultValue ?? String.Empty);
        }

        [DebuggerStepThrough]
        public static string EmptyNull(this string value)
        {
            return (value ?? string.Empty).Trim();
        }

        [DebuggerStepThrough]
        public static string NullEmpty(this string value)
        {
            return (string.IsNullOrEmpty(value)) ? null : value;
        }

        /// <summary>
        /// Formats a string to an invariant culture
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string FormatInvariant(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.InvariantCulture, format, objects);
        }

        /// <summary>
        /// Formats a string to the current culture.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string FormatCurrent(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentCulture, format, objects);
        }

        /// <summary>
        /// Formats a string to the current UI culture.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string FormatCurrentUI(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentUICulture, format, objects);
        }

        [DebuggerStepThrough]
        public static string FormatWith(this string format, params object[] args)
        {
            return FormatWith(format, CultureInfo.CurrentCulture, args);
        }

        [DebuggerStepThrough]
        public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, format, args);
        }

        /// <summary>
        /// Determines whether this instance and another specified System.String object have the same value.
        /// </summary>
        /// <param name="instance">The string to check equality.</param>
        /// <param name="comparing">The comparing with string.</param>
        /// <returns>
        /// <c>true</c> if the value of the comparing parameter is the same as this string; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsCaseSensitiveEqual(this string value, string comparing)
        {
            return string.CompareOrdinal(value, comparing) == 0;
        }

        /// <summary>
        /// Determines whether this instance and another specified System.String object have the same value.
        /// </summary>
        /// <param name="instance">The string to check equality.</param>
        /// <param name="comparing">The comparing with string.</param>
        /// <returns>
        /// <c>true</c> if the value of the comparing parameter is the same as this string; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsCaseInsensitiveEqual(this string value, string comparing)
        {
            return string.Compare(value, comparing, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the string is null, empty or all whitespace.
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsEmpty(this string value)
        {

            if (value == null || value.Length == 0)
                return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the string is all white space. Empty string will return false.
        /// </summary>
        /// <param name="s">The string to test whether it is all white space.</param>
        /// <returns>
        /// 	<c>true</c> if the string is all white space; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsWhiteSpace(this string value)
        {
            Guard.ArgumentNotNull(value, "value");

            if (value.Length == 0)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        [DebuggerStepThrough]
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <remarks>codehint: sm-edit</remarks>
        /// <remarks>to get equivalent result to PHPs md5 function call Hash("my value", false, false).</remarks>
        [DebuggerStepThrough]
        public static string Hash(this string value, bool toBase64 = false, bool unicode = false)
        {
            Guard.ArgumentNotEmpty(value, "value");

            using (MD5 md5 = MD5.Create())
            {
                byte[] data = null;
                if (unicode)
                    data = Encoding.Unicode.GetBytes(value);
                else
                    data = Encoding.ASCII.GetBytes(value);

                if (toBase64)
                {
                    byte[] hash = md5.ComputeHash(data);
                    return Convert.ToBase64String(hash);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    byte[] hashBytes = md5.ComputeHash(data);
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2").ToLower());
                    }

                    return sb.ToString();
                }
            }
        }

        [DebuggerStepThrough]
        public static bool IsWebUrl(this string value)
        {
            return !String.IsNullOrEmpty(value) && RegularExpressions.IsWebUrl.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static bool IsEmail(this string value)
        {
            return !String.IsNullOrEmpty(value) && RegularExpressions.IsEmail.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static bool IsNumeric(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            return !RegularExpressions.IsNotNumber.IsMatch(value) &&
                   !RegularExpressions.HasTwoDot.IsMatch(value) &&
                   !RegularExpressions.HasTwoMinus.IsMatch(value) &&
                   RegularExpressions.IsNumeric.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static bool IsAlpha(this string value)
        {
            return RegularExpressions.IsAlpha.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static bool IsAlphaNumeric(this string value)
        {
            return RegularExpressions.IsAlphaNumeric.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static string Truncate(this string value, int maxLength)
        {
            return Truncate(value, maxLength, "...");
        }

        [DebuggerStepThrough]
        public static string Truncate(this string value, int maxLength, string suffix)
        {
            Guard.ArgumentNotNull(suffix, "suffix");
            Guard.ArgumentIsPositive(maxLength, "maxLength");

            int subStringLength = maxLength - suffix.Length;

            if (subStringLength <= 0)
                throw Error.Argument("maxLength", "Length of suffix string is greater or equal to maximumLength");

            if (value != null && value.Length > maxLength)
            {
                string truncatedString = value.Substring(0, subStringLength);
                // incase the last character is a space
                truncatedString = truncatedString.Trim();
                truncatedString += suffix;

                return truncatedString;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Determines whether the string contains white space.
        /// </summary>
        /// <param name="s">The string to test for white space.</param>
        /// <returns>
        /// 	<c>true</c> if the string contains white space; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool ContainsWhiteSpace(this string value)
        {
            Guard.ArgumentNotNull(value, "value");

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Ensures the target string ends with the specified string.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        /// <returns>The target string with the value string at the end.</returns>
        [DebuggerStepThrough]
        public static string EnsureEndsWith(this string value, string endWith)
        {
            Guard.ArgumentNotNull(value, "value");
            Guard.ArgumentNotNull(endWith, "endWith");

            if (value.Length >= endWith.Length)
            {
                if (string.Compare(value, value.Length - endWith.Length, endWith, 0, endWith.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return value;

                string trimmedString = value.TrimEnd(null);

                if (string.Compare(trimmedString, trimmedString.Length - endWith.Length, endWith, 0, endWith.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return value;
            }

            return value + endWith;
        }

        [DebuggerStepThrough]
        public static int? GetLength(this string value)
        {
            if (value == null)
                return null;
            else
                return value.Length;
        }

        [DebuggerStepThrough]
        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        [DebuggerStepThrough]
        public static string UrlDecode(this string value)
        {
            return HttpUtility.UrlDecode(value);
        }

        [DebuggerStepThrough]
        public static string AttributeEncode(this string value)
        {
            return HttpUtility.HtmlAttributeEncode(value);
        }

        [DebuggerStepThrough]
        public static string HtmlEncode(this string value)
        {
            return HttpUtility.HtmlEncode(value);
        }

        [DebuggerStepThrough]
        public static string HtmlDecode(this string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        [DebuggerStepThrough]
        public static string RemoveHtml(this string value)
        {
            return RemoveHtmlInternal(value, null);
        }

        public static string RemoveHtml(this string value, ICollection<string> removeTags)
        {
            return RemoveHtmlInternal(value, removeTags);
        }

        private static string RemoveHtmlInternal(string s, ICollection<string> removeTags)
        {
            List<string> removeTagsUpper = null;
            if (removeTags != null)
            {
                removeTagsUpper = new List<string>(removeTags.Count);

                foreach (string tag in removeTags)
                {
                    removeTagsUpper.Add(tag.ToUpperInvariant());
                }
            }

            return RegularExpressions.RemoveHTML.Replace(s, delegate(Match match)
            {
                string tag = match.Groups["tag"].Value.ToUpperInvariant();

                if (removeTagsUpper == null)
                    return string.Empty;
                else if (removeTagsUpper.Contains(tag))
                    return string.Empty;
                else
                    return match.Value;
            });
        }

        /// <summary>
        /// Replaces pascal casing with spaces. For example "CustomerId" would become "Customer Id".
        /// Strings that already contain spaces are ignored.
        /// </summary>
        /// <param name="input">String to split</param>
        /// <returns>The string after being split</returns>
        [DebuggerStepThrough]
        public static string SplitPascalCase(this string value)
        {
            //return Regex.Replace(input, "([A-Z][a-z])", " $1", RegexOptions.Compiled).Trim();
            StringBuilder sb = new StringBuilder();
            char[] ca = value.ToCharArray();
            sb.Append(ca[0]);
            for (int i = 1; i < ca.Length - 1; i++)
            {
                char c = ca[i];
                if (char.IsUpper(c) && (char.IsLower(ca[i + 1]) || char.IsLower(ca[i - 1])))
                {
                    sb.Append(" ");
                }
                sb.Append(c);
            }
            if (ca.Length > 1)
            {
                sb.Append(ca[ca.Length - 1]);
            }

            return sb.ToString();
        }
        /// <remarks>codehint: sm-add</remarks>
        [DebuggerStepThrough]
        public static string[] SplitSafe(this string value, string separator)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];
            return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }
         

        [DebuggerStepThrough]
        public static string ToCamelCase(this string instance)
        {
            char ch = instance[0];
            return (ch.ToString().ToLowerInvariant() + instance.Substring(1));
        }

        [DebuggerStepThrough]
        public static string ReplaceNewLines(this string value, string replacement)
        {
            StringReader sr = new StringReader(value);
            StringBuilder sb = new StringBuilder();

            bool first = true;

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (first)
                    first = false;
                else
                    sb.Append(replacement);

                sb.Append(line);
            }

            return sb.ToString();
        }
 
 

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string enclosedIn)
        {
            return value.IsEnclosedIn(enclosedIn, StringComparison.CurrentCulture);
        }

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string enclosedIn, StringComparison comparisonType)
        {
            if (String.IsNullOrEmpty(enclosedIn))
                return false;

            if (enclosedIn.Length == 1)
                return value.IsEnclosedIn(enclosedIn, enclosedIn, comparisonType);

            if (enclosedIn.Length % 2 == 0)
            {
                int len = enclosedIn.Length / 2;
                return value.IsEnclosedIn(
                    enclosedIn.Substring(0, len),
                    enclosedIn.Substring(len, len),
                    comparisonType);

            }

            return false;
        }

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string start, string end)
        {
            return value.IsEnclosedIn(start, end, StringComparison.CurrentCulture);
        }

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string start, string end, StringComparison comparisonType)
        {
            return value.StartsWith(start, comparisonType) && value.EndsWith(end, comparisonType);
        }

        public static string RemoveEncloser(this string value, string encloser)
        {
            return value.RemoveEncloser(encloser, StringComparison.CurrentCulture);
        }

        public static string RemoveEncloser(this string value, string encloser, StringComparison comparisonType)
        {
            if (value.IsEnclosedIn(encloser, comparisonType))
            {
                int len = encloser.Length / 2;
                return value.Substring(
                    len,
                    value.Length - (len * 2));
            }

            return value;
        }

        public static string RemoveEncloser(this string value, string start, string end)
        {
            return value.RemoveEncloser(start, end, StringComparison.CurrentCulture);
        }

        public static string RemoveEncloser(this string value, string start, string end, StringComparison comparisonType)
        {
            if (value.IsEnclosedIn(start, end, comparisonType))
                return value.Substring(
                    start.Length,
                    value.Length - (start.Length + end.Length));

            return value;
        }

        // codehint: sm-add (begin)

        /// <summary>Debug.WriteLine</summary>
        /// <remarks>codehint: sm-add</remarks>
        [DebuggerStepThrough]
        public static void Dump(this string value)
        {
            Debug.WriteLine(value);
        }

        /// <summary>Smart way to create a HTML attribute with a leading space.</summary>
        /// <remarks>codehint: sm-add</remarks>
        /// <param name="name">Name of the attribute.</param>
        public static string ToAttribute(this string value, string name, bool htmlEncode = true)
        {
            if (value == null  )
                return "";

            if (value == "" && name != "value" && !name.StartsWith("data"))
                return "";

            if (name == "maxlength" && (value == "" || value == "0"))
                return "";

            if (name == "checked" || name == "disabled" || name == "multiple")
            {
                if (value == "" || string.Compare(value, "false", true) == 0)
                    return "";
                value = (string.Compare(value, "true", true) == 0 ? name : value);
            }

            if (name.StartsWith("data"))
                name = name.Insert(4, "-");

            return string.Format(" {0}=\"{1}\"", name, htmlEncode ? HttpUtility.HtmlEncode(value) : value);
        }

        /// <summary>Appends grow and uses delimiter if the string is not empty.</summary>
        /// <remarks>codehint: sm-add</remarks>
        [DebuggerStepThrough]
        public static string Grow(this string value, string grow, string delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return (string.IsNullOrEmpty(grow) ? "" : grow);

            if (string.IsNullOrEmpty(grow))
                return (string.IsNullOrEmpty(value) ? "" : value);

            return string.Format("{0}{1}{2}", value, delimiter, grow);
        }

        /// <summary>Returns n/a if string is empty else self.</summary>
        /// <remarks>codehint: sm-add</remarks>
        [DebuggerStepThrough]
        public static string NaIfEmpty(this string value)
        {
            return (value.HasValue() ? value : "n/a");
        }

        /// <summary>Replaces substring with position x1 to x2 by replaceBy.</summary>
        /// <remarks>codehint: sm-add</remarks>
        [DebuggerStepThrough]
        public static string Replace(this string value, int x1, int x2, string replaceBy = null)
        {
            if (value.HasValue() && x1 > 0 && x2 > x1 && x2 < value.Length)
            {
                return value.Substring(0, x1) + (replaceBy == null ? "" : replaceBy) + value.Substring(x2 + 1);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static string TrimSafe(this string value)
        {
            return (value.HasValue() ? value.Trim() : value);
        }

        [DebuggerStepThrough]
        public static string Prettify(this string value, bool allowSpace = false, char[] allowChars = null)
        {
            string res = "";
            try
            {
                if (value.HasValue())
                {
                    StringBuilder sb = new StringBuilder();
                    bool space = false;
                    char ch;

                    for (int i = 0; i < value.Length; ++i)
                    {
                        ch = value[i];

                        if (ch == ' ' || ch == '-')
                        {
                            if (allowSpace && ch == ' ')
                                sb.Append(' ');
                            else if (!space)
                                sb.Append('-');
                            space = true;
                            continue;
                        }

                        space = false;

                        if ((ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 90) || (ch >= 97 && ch <= 122))
                        {
                            sb.Append(ch);
                            continue;
                        }

                        if (allowChars != null && allowChars.Contains(ch))
                        {
                            sb.Append(ch);
                            continue;
                        }

                        switch (ch)
                        {
                            case '_': sb.Append(ch); break;

                            case 'ä': sb.Append("ae"); break;
                            case 'ö': sb.Append("oe"); break;
                            case 'ü': sb.Append("ue"); break;
                            case 'ß': sb.Append("ss"); break;
                            case 'Ä': sb.Append("AE"); break;
                            case 'Ö': sb.Append("OE"); break;
                            case 'Ü': sb.Append("UE"); break;

                            case 'é':
                            case 'è':
                            case 'ê': sb.Append('e'); break;
                            case 'á':
                            case 'à':
                            case 'â': sb.Append('a'); break;
                            case 'ú':
                            case 'ù':
                            case 'û': sb.Append('u'); break;
                            case 'ó':
                            case 'ò':
                            case 'ô': sb.Append('o'); break;
                        }	// switch
                    }	// for

                    if (sb.Length > 0)
                    {
                        res = sb.ToString().Trim(new char[] { ' ', '-' });

                        Regex pat = new Regex(@"(-{2,})");		// remove double SpaceChar
                        res = pat.Replace(res, "-");
                        res = res.Replace("__", "_");
                    }
                }
            }
            catch (Exception exp)
            {
                 
            }
            return (res.Length > 0 ? res : "null");
        }

        public static string SanitizeHtmlId(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(value.Length);
            int index = value.IndexOf("#");
            int num2 = value.LastIndexOf("#");
            if (num2 > index)
            {
                ReplaceInvalidHtmlIdCharacters(value.Substring(0, index), builder);
                builder.Append(value.Substring(index, (num2 - index) + 1));
                ReplaceInvalidHtmlIdCharacters(value.Substring(num2 + 1), builder);
            }
            else
            {
                ReplaceInvalidHtmlIdCharacters(value, builder);
            }
            return builder.ToString();
        }

        private static bool IsValidHtmlIdCharacter(char c)
        {
            bool invalid = (c == '?' || c == '!' || c == '#' || c == '.' || c == ' ' || c == ';' || c == ':');
            return !invalid;
        }

        private static void ReplaceInvalidHtmlIdCharacters(string part, StringBuilder builder)
        {
            for (int i = 0; i < part.Length; i++)
            {
                char c = part[i];
                if (IsValidHtmlIdCharacter(c))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('_');
                }
            }
        }

        public static string Sha(this string value)
        {
            if (value.HasValue())
            {
                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    byte[] data = Encoding.ASCII.GetBytes(value);
                    StringBuilder sb = new StringBuilder();

                    foreach (byte b in sha1.ComputeHash(data))
                        sb.Append(b.ToString("x2"));

                    return sb.ToString();
                }
            }
            return "";
        }

        [DebuggerStepThrough]
        public static bool IsMatch(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.IsMatch(input, pattern, options);
        }

        [DebuggerStepThrough]
        public static bool IsMatch(this string input, string pattern, out Match match, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            match = Regex.Match(input, pattern, options);
            return match.Success;
        }

        public static string RegexRemove(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, string.Empty, options);
        }

        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }

        [DebuggerStepThrough]
        public static string ToValidFileName(this string input, string replacement = "-")
        {
            return input.ToValidPathInternal(false, replacement);
        }

        [DebuggerStepThrough]
        public static string ToValidPath(this string input, string replacement = "-")
        {
            return input.ToValidPathInternal(true, replacement);
        }

        private static string ToValidPathInternal(this string input, bool isPath, string replacement)
        {
            var result = input.ToSafe();

            char[] invalidChars = isPath ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars();

            foreach (var c in invalidChars)
            {
                result = result.Replace(c.ToString(), replacement ?? "-");
            }

            return result;
        }

        [DebuggerStepThrough]
        public static int[] ToIntArray(this string s)
        {
            return Array.ConvertAll(s.SplitSafe(","), v => int.Parse(v));
        }

        [DebuggerStepThrough]
        public static bool ToIntArrayContains(this string s, int value, bool defaultValue)
        {
            var arr = s.ToIntArray();
            if (arr == null || arr.Count() <= 0)
                return defaultValue;
            return arr.Contains(value);
        }

        [DebuggerStepThrough]
        public static string RemoveInvalidXmlChars(this string s)
        {
            if (s==null || string.IsNullOrEmpty(s))
                return s;

            return Regex.Replace(s, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
        }

        // codehint: sm-add (end)
        #endregion

        #region Helper
 
  

        #endregion
    }
}
