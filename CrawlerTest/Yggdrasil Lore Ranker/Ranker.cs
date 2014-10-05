using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yggdrasil_Lore_Indexer;

namespace Yggdrasil_Lore_Ranker
{
    public class Ranker
    {
        public InvertedIndex Index { get; private set; }

        public Ranker()
        {
            Index = new InvertedIndex();
        }

        public IEnumerable<Uri> ResolveQuery(string query)
        {
            string[] terms = query.Normalise().ToArray();

            IDictionary<string, int> keywordVector = terms.GroupBy(term => term).ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

            string[] keywords = keywordVector.Keys.ToArray();

            IEnumerable<IndexResult> indexResults = Index.RetrieveDocuments(keywords);

            IDictionary<string, double> documentFrequency = Index.GetDocumentFrequency(keywords);

            indexResults = indexResults
                .OrderByDescending(result => ((result.WordCount.Keys.Select(word => result.WordCount[word] * keywordVector[word]).Sum() / //Cosine normalisation
                    (Math.Sqrt(result.WordCount.Keys.Select(keyword => Math.Pow(result.WordCount[keyword], 2.0)).Sum())) * //Cosine normalisation
                    Math.Sqrt(keywordVector.Keys.Select(keyword => Math.Pow(keywordVector[keyword], 2.0)).Sum()))) * result.WordCount.Keys.Select(word => //Cosine normalisation
                    ((Math.Sign(result.WordCount[word]) + Math.Log10(result.WordCount[word])) * documentFrequency[word])).Sum()) //tf-idf
                .ToArray();

            return indexResults.Select(result => result.URI);
        }
    }
}
