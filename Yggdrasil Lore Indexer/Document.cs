using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Indexer
{
    public class Document
    {
        public long ID { get; private set; }

        public Uri URI { get; private set; }

        public string[] Words { get; private set; }

        private int hash;

        public Document(long id, Uri uri, string content)
        {
            ID = id;

            hash = content.GetHashCode();

            URI = uri;

            Words = content.Normalise().ToArray();
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override bool Equals(object obj)
        {
            Document other;
            if ((other = obj as Document) == null)
            {
                return false;
            }

            return hash == other.hash;
        }
    }
}
