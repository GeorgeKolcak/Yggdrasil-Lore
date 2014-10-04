using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Indexer
{
    class PostingsList
    {
        public LinkedList<Tuple<long, int>> Documents { get; private set; }

        public PostingsList()
        {
            Documents = new LinkedList<Tuple<long, int>>();
        }

        public void AddDocument(long id, int occurenceCount)
        {
            if ((Documents.Count == 0) || (Documents.Last.Value.Item1 < id))
            {
                Documents.AddLast(new Tuple<long, int>(id, occurenceCount));
            }
            else
            {
                LinkedListNode<Tuple<long, int>> node = Documents.First;
                while (node.Value.Item1 < id)
                {
                    node = node.Next;
                }

                Documents.AddBefore(node, new Tuple<long, int>(id, occurenceCount));
            }
        }
    }
}
