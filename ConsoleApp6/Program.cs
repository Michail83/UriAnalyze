using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var analyser = new UriAnalyzer();
            var incoming = new string[] {
                "asdf www.hommits..by", "-123 12www.hommits.com", "123 hommits*++&.com.hoMMits.by",
                "345 www.hom--mits.uk", "754 hommits.by", "98 www.biz.hommits.by",
                "234 hTtps://hommits.com", "19 https://www.hommits.com", "234 http://hommits.by",
                "234 http://hom-mits.by", "111 httP://hom-mits22.by"
            };

            var addingResult = analyser.AddData(incoming);

            Console.WriteLine($"valid result =  {addingResult.CountOfValidResult}, has error = {addingResult.HasErrors}");
            if (addingResult.HasErrors)
            {
                foreach (var error in addingResult.ErrorList)
                {
                    Console.WriteLine($"{error.ErrorString}");
                    foreach (var item in error.ErrorsList)
                    {
                        Console.WriteLine($"{item}");
                    }
                    
                }
                Console.WriteLine();
            }
            var result = analyser.GetVisitsByDomainLevel(1);
            foreach (var item in result)
            {
                Console.WriteLine($"{item.Key}  has  {item.Value} visits");
            }
            Console.WriteLine();




        }
    }
    public class UriAnalyzer
    {
        private List<MyUri> myUriList = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incomingData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Result AddData(string[] incomingData)
        {
            if (incomingData ==null)
            {
                throw new ArgumentNullException();
            }

            List< Error> errors = new ();
            foreach (var itemString in incomingData)
            {
                Error error = new();

                var stringRow = itemString.Split(" ");
                if (!int.TryParse(stringRow[0], out int number) || number < 0)
                {
                    error.ErrorString = itemString;
                    error.ErrorsList.Add("!!!wrong number of visits");                    
                }

                string checkUriPattern =
                    @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,6}(:[0-9]{1,5})?(\/.*)?$";
                Regex regexCheckUri = new(checkUriPattern, RegexOptions.IgnoreCase);
                if (!regexCheckUri.IsMatch(stringRow[1]))
                {
                    error.ErrorString = itemString;
                    error.ErrorsList.Add("!!!wrong URI");
                }
                if (error.HasError)
                {
                    errors.Add(error);
                    continue;
                }


                MyUri myUri = new()
                {
                    Number = number
                };
                string schemePattern = "^https{0,1}://";
                Regex regexScheme = new Regex(schemePattern, RegexOptions.IgnoreCase);
                var scheme = regexScheme.Match(stringRow[1]).Value;
                if (!string.IsNullOrEmpty(scheme))
                {
                    myUri.Scheme = scheme.ToLowerInvariant();
                    stringRow[1] = Regex.Replace(stringRow[1], schemePattern, "");
                }

                string wwwPattern = "^www";
                Regex regexWWW = new Regex(wwwPattern, RegexOptions.IgnoreCase);
                var www = regexWWW.Match(stringRow[1]).Value;
                if (!string.IsNullOrEmpty(www))
                {
                    myUri.WWW = www;
                    stringRow[1] = Regex.Replace(stringRow[1], "^www\\.", "");
                }

                myUri.Host = stringRow[1].ToLowerInvariant();
                myUriList.Add(myUri);
            }
            return new Result(myUriList.Count, errors);
        }


        /// <summary>
        /// Return  if don't have any data or whitespace if no matches
        /// </summary>
        /// <param name="level"> how many domain levels use to compare, must be in range 1 - 63 </param>
        /// 
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Dictionary<string, int> GetVisitsByDomainLevel(int level=2)
        {
            return GetVisits(GetPatternByDomainLevel(level));
        }


        internal Dictionary<string, int> GetVisits(string pattern)

        {
            Dictionary<string, int> result = new();

            Regex regex = new(pattern, RegexOptions.IgnoreCase);

            if (myUriList.Count < 1)
            {
                return null;
            }
            foreach (var myUri in myUriList)
            {
                var key = regex.Match(myUri.Host).Value;
                if (string.IsNullOrEmpty(key)) continue;

                if (result.ContainsKey(key))
                {
                    result[key] += myUri.Number;
                }
                else
                {
                    result[key] = myUri.Number;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetPatternByDomainLevel(int level)
        {
            if (1 > level && level > 63) throw new ArgumentOutOfRangeException("level between 1 and 63");
            StringBuilder builder = new();
            for (int i = 1; i < level; i++)
            {
                builder.Append(@"[a-z0-9-]{1,}[\.]{1}");
            }
            builder.Append("[a-z]{2,}$");
            return builder.ToString();
        }
    }

    public class MyUri
    {
        public string Scheme { get; set; } = string.Empty;
        public bool HasSheme { get { return !string.IsNullOrEmpty(Scheme); } }
        public string WWW { get; set; } = string.Empty;
        public bool HasWWW { get { return !string.IsNullOrEmpty(WWW); } }
        public string Host { get; set; }
        public int Number { get; set; }

    }
    public class Result
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="validResultCount"> must be positive</param>
        /// <param name="errors"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Result(int validResultCount, List< Error> errors = null)
        {
            if (validResultCount < 0) throw new ArgumentOutOfRangeException();
            if (errors != null)  ErrorList = new List<Error>(errors);

            CountOfValidResult = validResultCount;
        }

        public List<Error> ErrorList { get; private set; }
        public bool HasErrors => ErrorList != null && ErrorList.Count > 0;
        public int CountOfValidResult { get; private set; }
        public bool HasValidResult => CountOfValidResult > 0;
    }

    public class Error 
        {
        public string ErrorString { get; set; }
        public List<string> ErrorsList { get; set; } = new();
        public bool HasError { get => ErrorsList.Count > 0; }

    }
}



