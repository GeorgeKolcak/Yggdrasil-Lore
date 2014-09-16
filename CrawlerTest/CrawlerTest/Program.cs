using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yggdrasil_Lore_Crawler;

namespace CrawlerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfPages = Int32.Parse(Console.ReadLine());

            Crawler crawler = new Crawler();

            foreach (CrawlResult result in crawler.Crawl(numberOfPages))
            {
                Console.WriteLine(result.URI.ToString());
            }

            Console.ReadKey();
        }
    }
}
