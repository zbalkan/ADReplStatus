using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;

namespace ADReplStatus
{
    public class ADREPLDC
    {
        public string Name;

        public string DomainName;

        public bool DiscoveryIssues = false;

        public string Site;

        public string IsGC;

        public string IsRODC;

        public List<ReplicationNeighbor> ReplicationPartners = new List<ReplicationNeighbor>();
    }

}
