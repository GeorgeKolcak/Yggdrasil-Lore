using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    public class Crawler
    {
        private static readonly Uri[] Seeds = new Uri[]
        {
            new Uri("http://www.wikipedia.org"),
            new Uri("http://www.google.com"),
            new Uri("http://www.amazon.com"),
            new Uri("http://www.bbc.com"),
            new Uri("http://www.aau.dk"),
            new Uri("http://tvtropes.org"),
            new Uri("http://www.yahoo.com"),
            new Uri("http://www.microsoft.com")
        };

        private Queue<Uri> frontier;

        public Crawler()
        {
            frontier = new Queue<Uri>(Seeds);
        }

        public IEnumerable<CrawlResult> Crawl(int numberOfResults)
        {
            for (int i = 0; i < numberOfResults; i++)
            {
                Uri uri = frontier.Dequeue();

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
                        frontier.Enqueue(newLink);
                    }

                    yield return new CrawlResult(uri, content);
                }
            }
        }

        private IEnumerable<Uri> ParseHyperlinks(string html)
        {
            MatchCollection hyperlinkMatches = Regex.Matches(html, "<[^<]*a[^>]*href.*>.*<[^<]*/a[^>]*>");

            foreach (Match match in hyperlinkMatches)
            {
                string[] hrefSplit = Regex.Split(match.Value, "href[ \t]*=[ \t]*\"");

                if (hrefSplit.Length <= 1)
                {
                    hrefSplit = Regex.Split(match.Value, "href[ \t]*=[ \t]*\'");
                }

                if (hrefSplit.Length <= 1)
                {
                    continue; //The link is probably written in some broken fashion, so let's ignore it.
                }

                string link = hrefSplit[1];

                string[] hrefEnd = link.Split('\"');

                if (hrefEnd.Length <= 1)
                {
                    hrefEnd = link.Split('\'');
                }

                link = hrefEnd[0];

                Uri uri = new Uri(link, UriKind.RelativeOrAbsolute);
                
                if (uri.IsAbsoluteUri)
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
            string host = uri.Host;

            WebRequest webRequest = WebRequest.Create(String.Format("http://{0}/robots.txt", host));
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

                                if (Regex.IsMatch(agentName, "^Yggdrasil$"))
                                {
                                    privileges = new AgentPrivileges();
                                }
                            }
                            else if ((privileges != null) && (lineSplit[0] == "Disallow"))
                            {
                                string relativeURL = lineSplit[1].Trim();

                                if (String.IsNullOrWhiteSpace(relativeURL))
                                {
                                    privileges.AddAllowedURI(new Uri(host));
                                }
                                else
                                {
                                    privileges.AddBlockedURI(new Uri(host + relativeURL));
                                }
                            }
                            else if ((privileges != null) && (lineSplit[0] == "Allow"))
                            {
                                privileges.AddAllowedURI(new Uri(host + lineSplit[1].Trim()));
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (privileges != null)
                        {
                            return privileges.IsAllowed(uri);
                        }
                    }
                }
            }

            return true;
        }
    }
}
