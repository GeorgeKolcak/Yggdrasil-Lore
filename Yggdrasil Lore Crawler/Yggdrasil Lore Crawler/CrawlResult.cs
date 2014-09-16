using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    public class CrawlResult
    {
        public Uri URI { get; private set; }

        public string Content { get; private set; }

        public CrawlResult(Uri uri, string content)
        {
            URI = uri;
            Content = content;
        }
    }
}
