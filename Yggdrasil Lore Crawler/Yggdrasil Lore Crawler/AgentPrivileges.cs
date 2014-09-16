using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    class AgentPrivileges
    {
        private IList<Uri> blockedURIs;
        private IList<Uri> allowedURIs;

        public AgentPrivileges()
        {
            blockedURIs = new List<Uri>();
            allowedURIs = new List<Uri>();
        }

        public void AddBlockedURI(Uri uri)
        {
            blockedURIs.Add(uri);
        }

        public void AddAllowedURI(Uri uri)
        {
            allowedURIs.Add(uri);
        }

        public bool IsAllowed(Uri uri)
        {
            bool allowed = true;

            foreach (Uri blockedURI in blockedURIs)
            {
                if (uri.ToString().StartsWith(blockedURI.ToString()))
                {
                    allowed = false;
                    break;
                }
            }

            if (!allowed)
            {
                foreach(Uri allowedURI in allowedURIs)
                {
                    if (uri.ToString().StartsWith(allowedURI.ToString()))
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
