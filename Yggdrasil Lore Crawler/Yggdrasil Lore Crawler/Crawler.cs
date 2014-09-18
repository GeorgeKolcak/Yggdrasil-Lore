using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    public class Crawler
    {
        private static readonly Uri[] Seeds = new Uri[]
        {
            new Uri("http://en.wikipedia.org"),
            new Uri("http://www.google.com"),
            new Uri("http://www.amazon.com"),
            new Uri("http://www.bbc.com"),
            new Uri("http://www.aau.dk"),
            new Uri("http://tvtropes.org"),
            new Uri("http://www.yahoo.com"),
            new Uri("http://www.microsoft.com"),
            new Uri("http://stackoverflow.com")
        };

        private Queue<Uri> frontier;

        private Queue<Uri>[] backQueues;
        private IDictionary<string, int> domainBackQueues;
        private SortedDictionary<DateTime, string> backQueueHeap; //This is actually a binary search tree, heap to come...

        private int firstEmptyBackQueueIndex = 0;

        private HashSet<Uri> crawledURIs;

        public Crawler()
        {
            frontier = new Queue<Uri>();
            frontier.Enqueue(new Uri("http://www.fi.muni.cz"));
            frontier.Enqueue(new Uri("http://www.univ-brest.fr"));
            frontier.Enqueue(new Uri("http://www.laposte.fr"));

            backQueues = new Queue<Uri>[256];
            domainBackQueues = new Dictionary<string, int>();
            backQueueHeap = new SortedDictionary<DateTime, string>();

            crawledURIs = new HashSet<Uri>();

            foreach (Uri seed in Seeds)
            {
                EnqueueURI(seed);
                Thread.Sleep(1);
            }
        }

        public IEnumerable<CrawlResult> Crawl(int numberOfResults)
        {
            for (int i = 0; i < numberOfResults; i++)
            {
                //Uri uri = frontier.Dequeue();
                Uri uri = DequeueURI();

                if (!CrawlAllowed(uri))
                {
                    continue;
                }

                WebRequest webRequest = WebRequest.Create(uri);
                WebResponse response;

                try
                {
                    response = webRequest.GetResponse();
                }
                catch (WebException)
                {
                    continue;
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = reader.ReadToEnd();

                    foreach(Uri newLink in ParseHyperlinks(content))
                    {
                        EnqueueURI(newLink);
                    }

                    yield return new CrawlResult(uri, content);
                }
            }
        }

        private Uri DequeueURI()
        {
            KeyValuePair<DateTime, string> oldestAccessedDomain = backQueueHeap.First();
            DateTime nextPossibleAccessTime = oldestAccessedDomain.Key; //The next time we may access this domain
            string domain = oldestAccessedDomain.Value; //Get the domain accessed most in the past

            backQueueHeap.Remove(nextPossibleAccessTime);

            if (nextPossibleAccessTime > DateTime.UtcNow)
            {
                Thread.Sleep((nextPossibleAccessTime - DateTime.UtcNow).Milliseconds);
            }

            int queueIndex = domainBackQueues[domain]; //Retrieve the queue for the domain

            Uri uri = backQueues[queueIndex].Dequeue(); //Get the next URI for the domain

            if (backQueues[queueIndex].Count == 0)
            {
                domainBackQueues.Remove(domain); // Remove the old domain

                Uri newUri = frontier.Dequeue();
                domain = newUri.Host; //We overwrite the previous domain, whose queue was emptied

                while (domainBackQueues.ContainsKey(domain))
                {
                    int oldDomainQueueIndex = domainBackQueues[domain]; //The new domain is actually not new and already has a queue
                    backQueues[oldDomainQueueIndex].Enqueue(newUri);

                    newUri = frontier.Dequeue();
                    domain = newUri.Host;
                }

                domainBackQueues.Add(domain, queueIndex); //Add the new domain

                backQueues[queueIndex].Enqueue(newUri);
            }

            backQueueHeap.Add(DateTime.UtcNow.AddSeconds(3), domain);

            return uri;
        }

        private void EnqueueURI(Uri uri)
        {
            if (crawledURIs.Contains(uri))
            {
                return;
            }

            crawledURIs.Add(uri);

            if (firstEmptyBackQueueIndex < 256)
            {
                string domain = uri.Host;

                if (domainBackQueues.ContainsKey(domain))
                {
                    int queueIndex = domainBackQueues[domain];
                    backQueues[queueIndex].Enqueue(uri);
                }
                else
                {
                    backQueues[firstEmptyBackQueueIndex] = new Queue<Uri>();
                    backQueues[firstEmptyBackQueueIndex].Enqueue(uri);
                    domainBackQueues.Add(domain, firstEmptyBackQueueIndex);
                    backQueueHeap.Add(DateTime.UtcNow, domain);

                    firstEmptyBackQueueIndex++;
                }
            }
            else
            {
                frontier.Enqueue(uri);
            }
        }

        private IEnumerable<Uri> ParseHyperlinks(string html)
        {
            MatchCollection hyperlinkMatches = Regex.Matches(html, "<[^<>]*a[^<>]*href[^<>]*>.*<[^<>]*/a[^<>]*>");

            foreach (Match match in hyperlinkMatches)
            {
                string[] hrefSplit = Regex.Split(match.Value, "href[ \t]*=[ \t]*[\"\']");

                if (hrefSplit.Length <= 1)
                {
                    continue; //This match is false positive.
                }

                string link = hrefSplit[1];

                string[] hrefEnd = Regex.Split(link, "[\"\']");

                link = hrefEnd[0];

                Uri uri = new Uri(link, UriKind.RelativeOrAbsolute);
                
                if (uri.IsAbsoluteUri && !Regex.IsMatch(uri.ToString(), "^javascript"))
                {
                    yield return uri;
                }
                else
                {
                    continue; //TODO
                }
            }
        }

        private bool CrawlAllowed(Uri uri)
        {
            string host = String.Format("http://{0}", uri.Host);

            WebRequest webRequest = WebRequest.Create(String.Format("{0}/robots.txt", host));
            WebResponse response;

            try
            {
                response = webRequest.GetResponse();
            }
            catch (WebException)
            {
                return true;
            }

            using (response)
            {
                using (Stream content = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(content))
                    {
                        AgentPrivileges privileges = null;

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Split('#')[0];
                            if (String.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            string[] lineSplit = line.Split(':');

                            if (lineSplit[0] == "User-agent")
                            {
                                if (privileges != null)
                                {
                                    break;
                                }

                                string agentName = lineSplit[1].Trim();

                                if ((agentName == "*") || Regex.IsMatch(agentName, "^Yggdrasil$"))
                                {
                                    privileges = new AgentPrivileges();
                                }
                            }
                            else if ((privileges != null) && (lineSplit[0] == "Disallow"))
                            {
                                string relativeURL = lineSplit[1].Trim();

                                if (String.IsNullOrWhiteSpace(relativeURL))
                                {
                                    privileges.AddAllowedURL(host + relativeURL);
                                }
                                else
                                {
                                    privileges.AddBlockedURL(host + relativeURL);
                                }
                            }
                            else if ((privileges != null) && (lineSplit[0] == "Allow"))
                            {
                                string relativeURL = lineSplit[1].Trim();

                                privileges.AddAllowedURL(host + relativeURL);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (privileges != null)
                        {
                            return privileges.IsAllowed(uri.ToString());
                        }
                    }
                }
            }

            return true;
        }
    }
}
