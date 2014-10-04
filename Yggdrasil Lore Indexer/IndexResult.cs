using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Indexer
{
    public class IndexResult
    {
        public Uri URI { get; private set; }

        public IDictionary<string, int> WordCount { get; private set; }

        public IndexResult(Uri uri, IDictionary<string, int> keywords)
        {
            URI = uri;

            WordCount = new Dictionary<string, int>(keywords);
        }
    }
}
