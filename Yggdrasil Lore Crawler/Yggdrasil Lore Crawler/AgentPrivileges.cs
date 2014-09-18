using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    class AgentPrivileges
    {
        private IList<string> blockedURLs;
        private IList<string> allowedURLs;

        public AgentPrivileges()
        {
            blockedURLs = new List<string>();
            allowedURLs = new List<string>();
        }

        public void AddBlockedURL(string url)
        {
            blockedURLs.Add(url.Replace("*", ".*").Replace("?", "\\?"));
        }

        public void AddAllowedURL(string url)
        {
            allowedURLs.Add(url.Replace("*", ".*").Replace("?", "\\?"));
        }

        public bool IsAllowed(string url)
        {
            bool allowed = true;

            foreach (string blockedURL in blockedURLs)
            {
                if (Regex.IsMatch(url, blockedURL))
                {
                    allowed = false;
                    break;
                }
            }

            if (!allowed)
            {
                foreach(string allowedURL in allowedURLs)
                {
                    if (Regex.IsMatch(url, allowedURL))
                    {
                        allowed = true;
                        break;
                    }
                }
            }

            return allowed;
        }
    }
}
