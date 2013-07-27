using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaBrowser.Theater.DirectShow
{
    public class StringFunctions
    {
        public static string CleanWebCharacters(string input)
        {
            try
            {
                input = input.Replace("&#x22;", "\"").Replace("&#x26;", "&").Replace("&#x27;", "'");
                input = input.Replace("&#xD6;", "Ö").Replace("&#xF6;", "ö").Replace("&#xF4;", "ô");
                input = input.Replace("&#xF3;", "ó").Replace("&#xE9;", "é").Replace("&#47;", "/");
                input = input.Replace("&#xDF;", "ß").Replace("&#xE8;", "è").Replace("&#xE0;", "à");
                input = input.Replace("&#x27;", "'").Replace("&#xEE;", "î").Replace("&#xE4;", "ä");
                input = input.Replace("&#x2022;", "•").Replace("&#x00E1;", "á").Replace("&#x00C1;", "Á").Replace("&#x00E2;", "â").Replace("&#x00C2;", "Â").Replace("&#x00E0;", "à").Replace("&#x00C0;", "À").Replace("&#x00E5;", "å").Replace("&#x00C5;", "Å").Replace("&#x00E3;", "ã").Replace("&#x00C3;", "Ã").Replace("&#x00E4;", "ä").Replace("&#x00C4;", "Ä").Replace("&#x00E6;", "æ").Replace("&#x00C6;", "Æ").Replace("&#x00E7;", "ç").Replace("&#x00C7;", "Ç").Replace("&#x00F0;", "ð").Replace("&#x00E9;", "Ð").Replace("&#x00E9;", "é").Replace("&#x00C9;", "É").Replace("&#x00EA;", "ê").Replace("&#x00CA;", "Ê").Replace("&#x00E8;", "è").Replace("&#x00C8;", "È").Replace("&#x00EB;", "ë").Replace("&#x00CB;", "Ë").Replace("&#x00ED;", "í").Replace("&#x00CD;", "Í").Replace("&#x00EE;", "î").Replace("&#x00CE;", "Î").Replace("&#x00EC;", "ì").Replace("&#x00CC;", "Ì").Replace("&#x00EF;", "ï").Replace("&#x00CF;", "Ï").Replace("&#x00F1;", "ñ").Replace("&#x00D1;", "Ñ").Replace("&#x00F3;", "ó").Replace("&#x00D3;", "Ó").Replace("&#x00F4;", "ô").Replace("&#x00D4;", "Ô").Replace("&#x00F2;", "ò").Replace("&#x00D2;", "Ò").Replace("&#x00F8;", "ø").Replace("&#x00D8;", "Ø").Replace("&#x00F5;", "õ").Replace("&#x00D5;", "Õ").Replace("&#x00F6;", "ö").Replace("&#x00D6;", "Ö").Replace("&#x00DF;", "ß").Replace("&#x00FE;", "þ").Replace("&#x00DE;", "Þ").Replace("&#x00FA;", "ú").Replace("&#x00DA;", "Ú").Replace("&#x00FB;", "û").Replace("&#x00DB;", "Û").Replace("&#x00F9;", "ù").Replace("&#x00D9;", "Ù").Replace("&#x00FC;", "ü").Replace("&#x00DC;", "Ü").Replace("&#x00FD;", "ý").Replace("&#x00DD;", "Ý").Replace("&#x00FF;", "Ÿ").Replace("&#x00A6;", "¦").Replace("&#x00A2;", "¢").Replace("&#x00A9;", "©").Replace("&#x00A4;", "¤").Replace("&#x2020;", "†").Replace("&#x2021;", "‡").Replace("&#x00B0;", "°").Replace("&#x00F7;", "÷").Replace("&#x2193;", "?").Replace("&#x2026;", "…").Replace("&#x00A1;", "¡").Replace("&#x00BF;", "¿").Replace("&#x00AB;", "«").Replace("&#x2190;", "?").Replace("&#x201C;", "“").Replace("&#x201E;", "„").Replace("&#x2018;", "‘").Replace("&#x201A;", "‚").Replace("&#x2014;", "-").Replace("&#x00B7;", "·").Replace("&#x00A0;", " ").Replace("&#x2013;", "-").Replace("&#x00B6;", "¶").Replace("&#x00B1;", "±").Replace("&#x00A3;", "£").Replace("&#x2032;", "?").Replace("&#x2033;", "?").Replace("&#x00BB;", "»").Replace("&#x2192;", "?").Replace("&#x201D;", "”").Replace("&#x00AE;", "®").Replace("&#x201D;", "“").Replace("&#x2019;", "’").Replace("&#x201F;", "‘").Replace("&#x00A7;", "§").Replace("&#x00D7;", "×").Replace("&#x2122;", "™").Replace("&#x2191;", "?").Replace("&#x00A5;", "¥");
                input = input.Replace("&#x96;", "-").Replace("&#x97;", "-").Replace("&#x2013;", "-").Replace("&#x2014;", "-");
                input = input.Replace("&quot;", "\"").Replace("&#x201C;", "\"").Replace("&#x201D;", "\"").Replace("&#x201E;", "\"");
                input = input.Replace("&apos;", "'").Replace("&#x2018;", "'").Replace("&#x2019;", "'").Replace("&#x201A;", "'");
                input = input.Replace("&gt;", ">").Replace("&#x203A;", ">");
                input = input.Replace("&lt;", "<").Replace("&#x2039;", "<");
                input = Regex.Replace(input, "\u2026", "...");
                input = Regex.Replace(input, "\u02C6", "^");
                input = Regex.Replace(input, "[\u02DC|\u00A0]", " ");
                input = Regex.Unescape(input);
            }
            catch
            {
            }
            return input;
        }

        public static bool containsIllegalCharacters(string searchText)
        {
            if (searchText.Contains("/") || searchText.Contains("\\") || searchText.Contains(":") || searchText.Contains("?") || searchText.Contains("\"") || searchText.Contains("<") || searchText.Contains(">") || searchText.Contains("|") || searchText.Contains("*"))
            {
                return true;
            }
            return false;
        }

        public static string replaceIllegalCharacters(string input, string replaceWith)
        {
            String T2 = input.Replace("\\", replaceWith);
            T2 = input.Replace("/", replaceWith);
            input = T2.Trim();
            T2 = input.Replace(":", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("?", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("\"", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("<", replaceWith);
            input = T2.Trim();
            T2 = input.Replace(">", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("|", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("*", replaceWith);
            input = T2.Trim();
            T2 = input.Replace("  ", " ");
            input = T2.Trim();
            return input;
        }

        public static bool containsRomanNumerals(string searchText)
        {
            StringBuilder sb = new StringBuilder();
            String C3 = searchText.Trim();
            sb.Append(C3 + " ");
            String sText = sb.ToString().ToLower();
            String[] rNumbers = {" i", " ii", " iii", " iv", " v", " vi", " vii", " viii", " ix", " x",
                               " xi", " xii", " xiii", " xiv", " xv", " xvi", " xvii", " xviii", " xix", " xx"};
            bool rNumerals = false;
            foreach (string s in rNumbers)
            {
                if (sText.Contains(s))
                {
                    rNumerals = true;
                    break;
                }
            }
            return rNumerals;
        }

        public static string RomanNumberToText(string romanNumber)
        {
            StringBuilder sb = new StringBuilder();
            String C3 = romanNumber.Trim();
            sb.Append(C3 + " ");
            String PosterName2 = sb.ToString().ToLower();
            C3 = PosterName2.Replace(" i ", " one ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" ii ", " two ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" iii ", " three ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" iv ", " four ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" v ", " five ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" vi ", " six ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" vii ", " seven ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" viii ", " eight ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" ix ", " nine ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" x ", " ten ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xi ", " eleven ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xii ", " tweleve ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xiii ", " thirteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xiv ", " fourteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xv ", " fifteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xvi ", " sixteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xvii ", " seventeen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xviii ", " eightteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xix ", " nineteen ").ToLower();
            PosterName2 = C3;
            C3 = PosterName2.Replace(" xx ", " twenty ").ToLower();
            PosterName2 = C3.Trim();
            return PosterName2;
        }

        public static string YearToText(int n)
        {
            String Year = Convert.ToString(n);
            String fistTwoDigits = Year.Substring(0, 2);
            int i1 = Convert.ToInt16(fistTwoDigits);
            fistTwoDigits = NumberToText(i1);
            String secondTwoDigits = Year.Substring(2, 2);
            int i2 = Convert.ToInt16(secondTwoDigits);
            secondTwoDigits = NumberToText(i2);
            String textYear = (fistTwoDigits + " " + secondTwoDigits);
            return textYear;
        }

        public static string SplitAbbreviationsWithSpace(string stringText)
        {
            string[] split = stringText.Split(new Char[] { ' ' });
            List<string> newString = new List<string>();
            List<string> charList = new List<string>();
            foreach (string s in split)
            {
                if (StringFunctions.HasVowels(s) == true
                    || StringFunctions.IsNumeric(s) == true
                    || StringFunctions.HasNumbers(s) == true)
                {
                    newString.Add(s);
                }
                else
                {
                    s.Replace(".", "");
                    foreach (char c1 in s)
                    {
                        newString.Add(c1.ToString());
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (string t2 in newString)
            {
                sb.Append(t2 + " ");
            }
            return sb.ToString().Trim();
        }

        public static string IndividualNumToText(string t)
        {
            string[] split = t.Split(new Char[] { ' ' });
            List<string> newString = new List<string>();
            List<string> charList = new List<string>();
            foreach (string s in split)
            {
                try
                {
                    int n = Convert.ToInt16(s);
                    foreach (char c1 in s)
                    {
                        string cString = Convert.ToString(c1);
                        charList.Add(cString);
                    }
                    foreach (string c in charList)
                    {
                        if (c == "0")
                        {
                            newString.Add("zero");
                        }
                        if (c == "1")
                        {
                            newString.Add("one");
                        }
                        if (c == "2")
                        {
                            newString.Add("two");
                        }
                        if (c == "3")
                        {
                            newString.Add("three");
                        }
                        if (c == "4")
                        {
                            newString.Add("four");
                        }
                        if (c == "5")
                        {
                            newString.Add("five");
                        }
                        if (c == "6")
                        {
                            newString.Add("six");
                        }
                        if (c == "7")
                        {
                            newString.Add("seven");
                        }
                        if (c == "8")
                        {
                            newString.Add("eight");
                        }
                        if (c == "9")
                        {
                            newString.Add("nine");
                        }
                    }
                }
                catch
                {
                    newString.Add(s);
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (string t2 in newString)
            {
                sb.Append(t2 + " ");
            }
            return sb.ToString();
        }

        public static string NumberToText(int n)
        {
            if (n < 0)
                return "Minus " + NumberToText(-n);
            else if (n == 0)
                return "";
            else if (n <= 19)
                return new string[] {"One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", 
         "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", 
         "Seventeen", "Eighteen", "Nineteen"}[n - 1] + " ";
            else if (n <= 99)
                return new string[] {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", 
         "Eighty", "Ninety"}[n / 10 - 2] + " " + NumberToText(n % 10);
            else if (n <= 199)
                return "One Hundred " + NumberToText(n % 100);
            else if (n <= 999)
                return NumberToText(n / 100) + "Hundred " + NumberToText(n % 100);
            else if (n <= 1999)
                return "One Thousand " + NumberToText(n % 1000);
            else if (n <= 999999)
                return NumberToText(n / 1000) + "Thousand " + NumberToText(n % 1000);
            else if (n <= 1999999)
                return "One Million " + NumberToText(n % 1000000);
            else if (n <= 999999999)
                return NumberToText(n / 1000000) + "Million " + NumberToText(n % 1000000);
            else if (n <= 1999999999)
                return "One Billion " + NumberToText(n % 1000000000);
            else
                return NumberToText(n / 1000000000) + "Billion " + NumberToText(n % 1000000000);
        }

        public static string NthNumberToText(int n)
        {
            if (n < 0)
                return "Minus " + NthNumberToText(-n);
            else if (n == 0)
                return "";
            else if (n <= 19)
                return new string[] {"First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", 
         "Nineth", "Tenth", "Eleventh", "Twelfth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", 
         "Seventeenth", "Eighteenth", "Nineteenth"}[n - 1] + " ";
            else if (n <= 99)
                return new string[] {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", 
         "Eighty", "Ninety"}[n / 10 - 2] + " " + NthNumberToText(n % 10);
            else if (n <= 199)
                return "One Hundred " + NthNumberToText(n % 100);
            else if (n <= 999)
                return NthNumberToText(n / 100) + "Hundred " + NthNumberToText(n % 100);
            else if (n <= 1999)
                return "One Thousand " + NthNumberToText(n % 1000);
            else if (n <= 999999)
                return NthNumberToText(n / 1000) + "Thousand " + NthNumberToText(n % 1000);
            else if (n <= 1999999)
                return "One Million " + NthNumberToText(n % 1000000);
            else if (n <= 999999999)
                return NthNumberToText(n / 1000000) + "Million " + NthNumberToText(n % 1000000);
            else if (n <= 1999999999)
                return "One Billion " + NthNumberToText(n % 1000000000);
            else
                return NthNumberToText(n / 1000000000) + "Billion " + NthNumberToText(n % 1000000000);
        }

        public static string GetSortTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return string.Empty;
            }
            if ((((title.StartsWith("A ", StringComparison.InvariantCultureIgnoreCase) || title.StartsWith("An ", StringComparison.InvariantCultureIgnoreCase)) || (title.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase) || title.StartsWith("Le ", StringComparison.InvariantCultureIgnoreCase))) || ((title.StartsWith("Les ", StringComparison.InvariantCultureIgnoreCase) || title.StartsWith("Das ", StringComparison.InvariantCultureIgnoreCase)) || title.StartsWith("Das ", StringComparison.InvariantCultureIgnoreCase))) || title.StartsWith("La ", StringComparison.InvariantCultureIgnoreCase))
            {
                return title.Substring(title.IndexOf(" ") + 1);
            }
            return title;
        }

        public class StringProcessing
        {
            ////Processing with strings
            //Use for small to medium strings

            //Swaps the cases in a string
            //word -> WORD
            //Word -> wORD
            //WoRd -> wOrD
            public static string SwapCases(string input)
            {
                string ret = "";
                for (int i = 0; i < input.Length; i++)
                {
                    if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToUpper(), false) == 0)
                        ret += input.Substring(i, 1).ToLower();
                    else
                        ret += input.Substring(i, 1).ToUpper();
                }
                return ret;
            }

            //Alternates cases between letters of a string, letting the user pick if the first letter is capitalized
            public static string AlternateCases(string input, bool firstIsUpper)
            {
                string ret = "";
                for (int i = 0; i < input.Length; i++)
                {
                    if (firstIsUpper)
                        ret += input.Substring(i, 1).ToUpper();
                    else
                        ret += input.Substring(i, 1).ToLower();

                    firstIsUpper = !firstIsUpper;
                }

                return ret;
            }

            //Removes vowels from a word
            //remove -> rmv
            public static string RemoveVowels(string input)
            {
                string ret = "";
                string currentLetter;
                for (int i = 0; i < input.Length; i++)
                {
                    currentLetter = input.Substring(i, 1);

                    if (string.Compare(currentLetter, "a", true) != 0 &&
                        string.Compare(currentLetter, "e", true) != 0 &&
                        string.Compare(currentLetter, "i", true) != 0 &&
                        string.Compare(currentLetter, "o", true) != 0 &&
                        string.Compare(currentLetter, "u", true) != 0)
                    {
                        //Not a vowel, add it
                        ret += currentLetter;
                    }
                }
                return ret;
            }

            //Removes consinents from a word
            //remove -> eoe
            public static string KeepVowels(string input)
            {
                string ret = "";
                string currentLetter;
                for (int i = 0; i < input.Length; i++)
                {
                    currentLetter = input.Substring(i, 1);

                    if (string.Compare(currentLetter, "a", true) == 0 ||
                        string.Compare(currentLetter, "e", true) == 0 ||
                        string.Compare(currentLetter, "i", true) == 0 ||
                        string.Compare(currentLetter, "o", true) == 0 ||
                        string.Compare(currentLetter, "u", true) == 0)
                    {
                        //A vowel, add it
                        ret += currentLetter;
                    }
                }
                return ret;
            }

            //Returns an array converted into a string
            public static string ArrayToString(Array input, string separator)
            {
                string ret = "";
                for (int i = 0; i < input.Length; i++)
                {
                    ret += input.GetValue(i).ToString();
                    if (i != input.Length - 1)
                        ret += separator;
                }
                return ret;
            }

            //Inserts a separator after every letter
            //hello, - -> h-e-l-l-o
            public static string InsertSeparator(string input, string separator)
            {
                string ret = "";
                for (int i = 0; i < input.Length; i++)
                {
                    ret += input.Substring(i, 1);
                    if (i != input.Length - 1)
                        ret += separator;
                }
                return ret;
            }

            //Inserts a separator after every Count letters
            //hello, -, 2 -> he-ll-o
            public static string InsertSeparatorEvery(string input, string separator, int count)
            {
                string ret = "";
                for (int i = 0; i < input.Length; i++)
                {
                    if (i + count < input.Length)
                        ret += input.Substring(i, count);
                    else
                        ret += input.Substring(i);

                    if (i != input.Length - 1)
                        ret += separator;
                }
                return ret;
            }

            //Reverses a string
            //Hello -> olleH
            public static string Reverse(string input)
            {
                string ret = "";
                for (int i = input.Length - 1; i >= 0; i--)
                {
                    ret += input.Substring(i, 1);
                }
                return ret;
            }
        }

        public class StringBuilderProcessing
        {
            ////Processing with StringBuilder
            //Use for small to medium strings

            //Swaps the cases in a string
            //word -> WORD
            //Word -> wORD
            //WoRd -> wOrD
            public static string SwapCases(string input)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToUpper(), false) == 0)
                        ret.Append(input.Substring(i, 1).ToLower());
                    else
                        ret.Append(input.Substring(i, 1).ToUpper());
                }
                return ret.ToString();
            }

            //Alternates cases between letters of a string, letting the user pick if the first letter is capitalized
            public static string AlternateCases(string input, bool firstIsUpper)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    if (firstIsUpper)
                        ret.Append(input.Substring(i, 1).ToUpper());
                    else
                        ret.Append(input.Substring(i, 1).ToLower());

                    firstIsUpper = !firstIsUpper;
                }

                return ret.ToString();
            }

            //Removes vowels from a word
            //remove -> rmv
            public static string RemoveVowels(string input)
            {
                StringBuilder ret = new StringBuilder();
                string currentLetter;
                for (int i = 0; i < input.Length; i++)
                {
                    currentLetter = input.Substring(i, 1);

                    if (string.Compare(currentLetter, "a", true) != 0 &&
                        string.Compare(currentLetter, "e", true) != 0 &&
                        string.Compare(currentLetter, "i", true) != 0 &&
                        string.Compare(currentLetter, "o", true) != 0 &&
                        string.Compare(currentLetter, "u", true) != 0)
                    {
                        //Not a vowel, add it
                        ret.Append(currentLetter);
                    }
                }
                return ret.ToString();
            }

            //Removes vowels from a word
            //remove -> eoe
            public static string KeepVowels(string input)
            {
                StringBuilder ret = new StringBuilder();
                string currentLetter;
                for (int i = 0; i < input.Length; i++)
                {
                    currentLetter = input.Substring(i, 1);

                    if (string.Compare(currentLetter, "a", true) == 0 ||
                        string.Compare(currentLetter, "e", true) == 0 ||
                        string.Compare(currentLetter, "i", true) == 0 ||
                        string.Compare(currentLetter, "o", true) == 0 ||
                        string.Compare(currentLetter, "u", true) == 0)
                    {
                        //A vowel, add it
                        ret.Append(currentLetter);
                    }
                }
                return ret.ToString();
            }

            //Returns an array converted into a string
            public static string ArrayToString(Array input, string separator)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    ret.Append(input.GetValue(i).ToString());
                    if (i != input.Length - 1)
                        ret.Append(separator);
                }
                return ret.ToString();
            }

            //Inserts a separator after every letter
            //hello, - -> h-e-l-l-o
            public static string InsertSeparator(string input, string separator)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    ret.Append(input.Substring(i, 1));
                    if (i != input.Length - 1)
                        ret.Append(separator);
                }
                return ret.ToString();
            }

            //Inserts a separator after every Count letters
            //hello, -, 2 -> he-ll-o
            public static string InsertSeparatorEvery(string input, string separator, int count)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    if (i + count < input.Length)
                        ret.Append(input.Substring(i, count));
                    else
                        ret.Append(input.Substring(i));

                    if (i != input.Length - 1)
                        ret.Append(separator);
                }
                return ret.ToString();
            }

            //Reverses a string
            //Hello -> olleH
            public static string Reverse(string input)
            {
                StringBuilder ret = new StringBuilder();
                for (int i = input.Length - 1; i >= 0; i--)
                {
                    ret.Append(input.Substring(i, 1));
                }
                return ret.ToString();
            }
        }

        //Capitalizes a word or sentence
        //word -> Word
        //OR
        //this is a sentence -> This is a sentence
        public static string Capitalize(string input)
        {
            if (input.Length == 0) return "";
            if (input.Length == 1) return input.ToUpper();

            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        //Checks whether a word or sentence is capitalized
        //Word -> True
        //OR
        //This is a sentence -> True
        public static bool IsCapitalized(string input)
        {
            if (input.Length == 0) return false;
            return string.Compare(input.Substring(0, 1), input.Substring(0, 1).ToUpper(), false) == 0;
        }

        //Checks whether a string is in all lower case
        //word -> True
        //Word -> False
        public static bool IsLowerCase(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToLower(), false) != 0)
                    return false;
            }
            return true;
        }

        //Checks whether a string is in all upper case
        //word -> False
        //Word -> False
        //WORD -> True
        public static bool IsUpperCase(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToUpper(), false) != 0)
                    return false;
            }
            return true;
        }

        //Alternates cases between letters of a string, first letter's case stays the same
        //Hi -> Hi
        //longstring -> lOnGsTrInG
        public static string AlternateCases(string input)
        {
            if (input.Length == 0) return "";
            if (input.Length == 1) return input; //Cannot automatically alternate
            bool firstIsUpper = string.Compare(input.Substring(0, 1), input.Substring(0, 1).ToUpper(), false) != 0;
            string ret = input.Substring(0, 1);
            for (int i = 1; i < input.Length; i++)
            {
                if (firstIsUpper)
                    ret += input.Substring(i, 1).ToUpper();
                else
                    ret += input.Substring(i, 1).ToLower();

                firstIsUpper = !firstIsUpper;
            }

            return ret;
        }

        //Checks to see if a string has alternate cases
        //lOnGsTrInG -> True
        public static bool IsAlternateCases(string input)
        {
            if (input.Length <= 1) return false;

            bool lastIsUpper = string.Compare(input.Substring(0, 1), input.Substring(0, 1).ToUpper(), false) == 0;

            for (int i = 1; i < input.Length; i++)
            {
                if (lastIsUpper)
                {
                    if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToLower(), false) != 0)
                        return false;
                }
                else
                {
                    if (string.Compare(input.Substring(i, 1), input.Substring(i, 1).ToUpper(), false) != 0)
                        return false;
                }

                lastIsUpper = !lastIsUpper;
            }

            return true;
        }

        //Counts total number of a char or chars in a string
        //hello, l -> 2
        //hello, el -> 1
        public static int CountTotal(string input, string chars, bool ignoreCases)
        {
            int count = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (!(i + chars.Length > input.Length) &&
                    string.Compare(input.Substring(i, chars.Length), chars, ignoreCases) == 0)
                {
                    count++;
                }
            }
            return count;
        }

        //Checks to see if a string contains vowels
        //hello -> True
        //rmv -> False
        public static bool HasVowels(string input)
        {
            string currentLetter;
            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input.Substring(i, 1);

                if (string.Compare(currentLetter, "a", true) == 0 ||
                  string.Compare(currentLetter, "e", true) == 0 ||
                  string.Compare(currentLetter, "i", true) == 0 ||
                  string.Compare(currentLetter, "o", true) == 0 ||
                  string.Compare(currentLetter, "u", true) == 0)
                {
                    //A vowel found
                    return true;
                }
            }

            return false;
        }

        //Checks if string is nothing but spaces
        //"        " -> True
        public static bool IsSpaces(string input)
        {
            if (input.Length == 0) return false;
            return input.Replace(" ", "").Length == 0;
        }

        //Checks if the string has all the same letter/number
        //aaaaaaaaaaaaaaaaaaa -> true
        //aaaaaaaaaaaaaaaaaab -> false
        public static bool IsRepeatedChar(string input)
        {
            if (input.Length == 0) return false;
            return input.Replace(input.Substring(0, 1), "").Length == 0;
        }

        //Checks if string has only numbers
        //12453 -> True
        //234d3 -> False
        public static bool IsNumeric(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!(Convert.ToInt32(input[i]) >= 48 && Convert.ToInt32(input[i]) <= 57))
                {
                    //Not integer value
                    return false;
                }
            }
            return true;
        }

        //Checks if the string contains numbers
        //hello -> False
        //h3llo -> True
        public static bool HasNumbers(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, "\\d+");
        }

        //Checks if string is numbers and letters
        //Test1254 -> True
        //$chool! -> False
        public static bool IsAlphaNumberic(string input)
        {
            char currentLetter;
            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input[i];

                if (!(Convert.ToInt32(currentLetter) >= 48 && Convert.ToInt32(currentLetter) <= 57) &&
                    !(Convert.ToInt32(currentLetter) >= 65 && Convert.ToInt32(currentLetter) <= 90) &&
                    !(Convert.ToInt32(currentLetter) >= 97 && Convert.ToInt32(currentLetter) <= 122))
                {
                    //Not a number or a letter
                    return false;
                }
            }
            return true;
        }

        //Checks if a string contains only letters
        //Hi -> True
        //Hi123 -> False
        public static bool isLetters(string input)
        {
            char currentLetter;
            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input[i];

                if (!(Convert.ToInt32(currentLetter) >= 65 && Convert.ToInt32(currentLetter) <= 90) &&
                    !(Convert.ToInt32(currentLetter) >= 97 && Convert.ToInt32(currentLetter) <= 122))
                {
                    //Not a letter
                    return false;
                }
            }
            return true;
        }

        // Returns the initials of a name or sentence
        //capitalize - whether to make intials capitals
        //includeSpace - to return intials separated (True - J. S. or False - J.S.)
        //John Smith -> J. S. or J.S.
        public static string GetInitials(string input, bool capitalize, bool includeSpace)
        {
            string[] words = input.Split(new char[] { ' ' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    if (capitalize)
                        words[i] = words[i].Substring(0, 1).ToUpper() + ".";
                    else
                        words[i] = words[i].Substring(0, 1) + ".";
            }

            if (includeSpace)
                return string.Join(" ", words);
            else
                return string.Join("", words);
        }

        //Capitalizes the first letter of every word
        //the big story -> The Big Story
        public static string GetTitle(string input)
        {
            string[] words = input.Split(new char[] { ' ' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
            }

            return string.Join(" ", words);
        }

        //Very much like the GetTitle function, capitalizes the first letter of every word
        //Additional function is, mcdonald -> McDonald, machenry -> MacHenry
        //Credits to ShutlOrbit (http://www.thirdstagesoftware.com) from CodeProject
        public static string GetNameCasing(string input)
        {
            string[] words = input.Split(new char[] { ' ' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
                    if (words[i].StartsWith("Mc") && words[i].Length > 2)
                        words[i] = words[i].Substring(0, 2) + words[i].Substring(2, 1).ToUpper() + words[i].Substring(3);
                    else if (words[i].StartsWith("Mac") && words[i].Length > 3)
                        words[i] = words[i].Substring(0, 3) + words[i].Substring(3, 1).ToUpper() + words[i].Substring(4);
                }
            }
            return string.Join(" ", words);
        }

        // Checks whether the first letter of each word is capitalized
        //The Big Story -> True
        //The big story -> False
        public static bool IsTitle(string input)
        {
            string[] words = input.Split(new char[] { ' ' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    if (string.Compare(words[i].Substring(0, 1).ToUpper(), words[i].Substring(0, 1), false) != 0)
                        return false;
            }
            return true;
        }

        //Checks if string is a valid email address format
        //name@place.com -> True
        //hahaimfaking -> False
        //(Function works assuming the last part is no bigger than 3 letters (com, net, org))
        public static bool IsEmailAddress(string input)
        {
            if (input.IndexOf('@') != -1)
            {
                int indexOfDot = input.LastIndexOf('.');
                if (input.Length - indexOfDot > 0 && input.Length - indexOfDot <= 4)
                    return true;
            }
            return false;
        }

        //Returns all the locations of a char in a string
        //Hello, l -> 2, 3
        //Hello, o -> 4
        //Bob, 1 -> -1
        public static int[] IndexOfAll(string input, string chars)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input.Substring(i, 1) == chars)
                    indices.Add(i);
            }

            if (indices.Count == 0)
                indices.Add(-1);

            return indices.ToArray();
        }

        //Return a rating for how strong the string is as a password
        //Max rating is 100
        //Credits for original function to D. Rijmenants
        //If there are problems with copyright contact us and we will delete it
        public static int PasswordStrength(string input)
        {
            double total = input.Length * 3;
            bool hasUpperCase = false;
            bool hasLowerCase = false;

            char currentLetter;
            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input[i];
                if (Convert.ToInt32(currentLetter) >= 65 && Convert.ToInt32(currentLetter) <= 92)
                    hasUpperCase = true;

                if (Convert.ToInt32(currentLetter) >= 97 && Convert.ToInt32(currentLetter) <= 122)
                    hasLowerCase = true;
            }

            if (hasUpperCase && hasLowerCase) total *= 1.2;

            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input[i];
                if (Convert.ToInt32(currentLetter) >= 48 && Convert.ToInt32(currentLetter) <= 57) //Numbers
                    if (hasUpperCase && hasLowerCase) total *= 1.4;
            }

            for (int i = 0; i < input.Length; i++)
            {
                currentLetter = input[i];
                if ((Convert.ToInt32(currentLetter) <= 47 && Convert.ToInt32(currentLetter) >= 123) ||
                    (Convert.ToInt32(currentLetter) >= 58 && Convert.ToInt32(currentLetter) <= 64)) //symbols
                {
                    total *= 1.5;
                    break;
                }
            }

            if (total > 100.0) total = 100.0;

            return (int)total;
        }

        //Gets the char in a string at a given position, but counting from right to left
        //string, 0 -> g
        public static char CharRight(string input, int index)
        {
            if (input.Length - index - 1 >= input.Length ||
                input.Length - index - 1 < 0)
                return new char();

            string str = input.Substring(input.Length - index - 1, 1);
            return str[0];
        }

        //Gets the char in a string from a starting position plus the index
        //string, 3, 1 -> n
        public static char CharMid(string input, int startingIndex, int countIndex)
        {
            if (startingIndex + countIndex < input.Length)
            {
                string str = input.Substring(startingIndex + countIndex, 1);
                return str[0];
            }
            else
                return new char();
        }

        //Function that works the same way as the default Substring, but
        //it takes Start and End (exclusive) parameters instead of Start and Length
        //hello, 1, 3 -> el
        public static string SubstringEnd(string input, int start, int end)
        {
            if (start > end) //Flip the values
            {
                start ^= end;
                end = start ^ end;
                start ^= end;
            }

            if (end > input.Length) end = input.Length; //avoid errors

            return input.Substring(start, end - start);

        }

        //Splits strings, but leaves anything within quotes
        //(Has issues with nested quotes
        //This is a "very long" string ->
        //This
        //is
        //a
        //very long
        //string
        public static string[] SplitQuotes(string input, bool ignoreQuotes, string separator)
        {
            if (ignoreQuotes)
                return input.Split(separator.ToCharArray());
            else
            {
                string[] words = input.Split(separator.ToCharArray());
                List<string> newWords = new List<string>();

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].StartsWith('"'.ToString()))
                    {
                        List<string> linked = new List<string>();
                        for (int y = i; y < words.Length; y++)
                        {
                            if (words[y].EndsWith('"'.ToString()))
                            {
                                linked.Add(words[y].Substring(0, words[y].Length - 1));
                                i = y;
                                break;
                            }
                            else
                            {
                                if (words[y].StartsWith('"'.ToString()))
                                    linked.Add(words[y].Substring(1));
                            }
                        }
                        newWords.Add(string.Join(separator, linked.ToArray()));
                        linked.Clear();
                    }
                    else
                        newWords.Add(words[i]);
                }
                return newWords.ToArray();
            }
        }
    }
}