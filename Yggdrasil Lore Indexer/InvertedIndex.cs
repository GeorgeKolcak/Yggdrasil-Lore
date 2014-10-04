using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Indexer
{
    public class InvertedIndex
    {
        private long nextID;

        private IDictionary<Uri, Document> documentsByURI;
        private IDictionary<long, Document> documentsByID;

        private IDictionary<string, PostingsList> index;

        public InvertedIndex()
        {
            nextID = 0L;

            documentsByURI = new Dictionary<Uri, Document>();
            documentsByID = new Dictionary<long, Document>();

            index = new Dictionary<string, PostingsList>();
        }

        public void NewDocument(Uri uri, string content)
        {
            Document doc = new Document(nextID, uri, content);

            documentsByURI.Add(uri, doc);
            documentsByID.Add(nextID, doc);

            nextID++;

            foreach (IGrouping<string, string> word in doc.Words.GroupBy(w => w))
            {
                AddEntry(word.Key, word.Count(), doc);
            }
        }

        private void AddEntry(string word, int occurenceCount, Document document)
        {
            if (!index.ContainsKey(word))
            {
                index[word] = new PostingsList();
            }

            index[word].AddDocument(document.ID, occurenceCount);
        }

        public IEnumerable<IndexResult> RetrieveDocuments(string query)
        {
            string[] keywords = query.Normalise().ToArray();

            if (keywords.Length == 0)
            {
                return Enumerable.Empty<IndexResult>();
            }

            IEnumerable<PostingsList> keywordEntries = keywords
                .Select(keyword => (index.ContainsKey(keyword) ? index[keyword] : null))
                .Where(x => (x != null))
                .OrderBy(postingList => postingList.Documents.Count);

            if (keywordEntries.Count() == 0)
            {
                return Enumerable.Empty<IndexResult>();
            }

            LinkedList<Tuple<long, int>> matchingDocuments = keywordEntries.Select(postingList => postingList.Documents).Aggregate((postingList1, postingList2) =>
               {
                   LinkedList<Tuple<long, int>> newList = new LinkedList<Tuple<long, int>>();

                   LinkedListNode<Tuple<long, int>> currentNode1 = postingList1.First;
                   LinkedListNode<Tuple<long, int>> currentNode2 = postingList2.First;

                   while ((currentNode1 != null) && (currentNode2 != null))
                   {
                       if (currentNode1.Value.Item1 == currentNode2.Value.Item1)
                       {
                           newList.AddLast(currentNode1.Value);
                           currentNode1 = currentNode1.Next;
                           currentNode2 = currentNode2.Next;
                       }
                       else if (currentNode1.Value.Item1 < currentNode2.Value.Item1)
                       {
                           currentNode1 = currentNode1.Next;
                       }
                       else
                       {
                           currentNode2 = currentNode2.Next;
                       }
                   }

                   return newList;
               });
                
                return matchingDocuments.Select(document => new IndexResult(documentsByID[document.Item1].URI,
                    keywords
                        .Where(keyword => index.ContainsKey(keyword))
                        .ToDictionary(keyword => keyword, keyword => index[keyword].Documents.Single(doc => doc.Item1 == document.Item1).Item2)));
        }
    }
}
