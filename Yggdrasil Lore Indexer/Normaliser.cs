using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Indexer
{
    public static class Normaliser
    {
        private static char[] Separators = new char[]
        {
            ' ', '\r', '\n', '\t', //Whitespace
            '.', ',', ':', ';', '"', '"', '?', '!', //Interpunction
            '(', ')', '{', '}', '[', ']', '<', '>', //Parentheses
            '&', '^', '~', '°', '#', '$', '`', '%', '@', '*', '+', '=', '/', '\\', '|' //Other Special Characters
        };

        private static string[] StopWords = new string[]
        {
            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at", "be", "because", "been", "before", "being", "below", "between", "both",
            "but", "by", "can't", "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during", "each", "few", "for", "from", "further", "had", "hadn't",
            "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm",
            "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most", "mustn't", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only",
            "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so", "some", "such", "than", "that",
            "that's", "the", "their", "theirs", "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too",
            "under", "until", "up", "very", "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which", "while", "who",
            "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't", "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
        };

        public static IEnumerable<string> Normalise(this string str)
        {
            return str.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.ToLowerInvariant())
                .Where(word => !StopWords.Contains(word))
                .Select(word =>
                    {
                        Stemmer stemmer = new Stemmer();
                        stemmer.add(word.ToCharArray(0, word.Length), word.Length);
                        stemmer.stem();
                        return stemmer.ToString();
                    });
        }
    }
}
