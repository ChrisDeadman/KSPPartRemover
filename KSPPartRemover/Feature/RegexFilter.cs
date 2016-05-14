using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KSPPartRemover.Feature
{
    public class RegexFilter
    {
        public String Pattern { get; }

        public RegexFilter (String pattern)
        {
            this.Pattern = pattern;
        }

        public bool Matches (String str)
        {
            if (String.IsNullOrEmpty (Pattern)) {
                return true;
            }

            return (Pattern.StartsWith ("!"))
                ? !Regex.Match (str, Pattern.Substring (1)).Success
                    : Regex.Match (str, Pattern).Success;
        }

        public IEnumerable<TElement> Apply<TElement> (IEnumerable<TElement> source, Func<TElement, String> selector)
        {
            return source.Where (element => Matches (selector (element)));
        }

        public override String ToString ()
        {
            return Pattern;
        }
    }
}
