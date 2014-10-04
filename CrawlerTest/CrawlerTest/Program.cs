using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yggdrasil_Lore_Crawler;
using Yggdrasil_Lore_Indexer;

namespace CrawlerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            InvertedIndex index = new InvertedIndex();

            int numberOfPages = Int32.Parse(Console.ReadLine());

            Crawler crawler = new Crawler();

            int i = 1;
            foreach (CrawlResult result in crawler.Crawl(numberOfPages))
            {
                Console.WriteLine("{0:00000}: {1}", i, result.URI.ToString());
                i++;

                index.NewDocument(result.URI, result.Content);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            string query;
            while ((query = Console.ReadLine()) != null)
            {
                foreach (IndexResult result in index.RetrieveDocuments(query))
                {
                    Console.WriteLine(result.URI);
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
