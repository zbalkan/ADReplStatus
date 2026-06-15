using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ADReplStatus
{
    public interface IProgressReporter
    {
        void ReportProgress(int percent, string message);
        bool AskFallbackToAutomaticDiscovery(string dcName);
    }

    public class ReplicationService
    {
        private static readonly int ReachabilityTimeoutMs = 3000;

        public List<ADREPLDC> DiscoverReplicationStatus(IProgressReporter reporter)
        {
            var state = AppState.Instance;
            Forest forest = null;

            try
            {
                if (state.UseUserDomainController)
                {
                    Logger.Log($"Attempting forest discovery via user specified domain controller {state.UserDomainController}");

                    DirectoryContext dcContext;
                    if (state.Username.Length > 0)
                    {
                        reporter.ReportProgress(0, $"Attempting to connect to {state.UserDomainController} with alternate user {state.Username}.");
                        dcContext = new DirectoryContext(DirectoryContextType.DirectoryServer, state.UserDomainController, state.Username, state.Password);
                    }
                    else
                    {
                        reporter.ReportProgress(0, $"Attempting to connect to {state.UserDomainController} as currently logged-on user.");
                        dcContext = new DirectoryContext(DirectoryContextType.DirectoryServer, state.UserDomainController);
                    }

                    try
                    {
                        DomainController domainController = DomainController.GetDomainController(dcContext);
                        forest = domainController.Forest;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to connect to specified DC {state.UserDomainController}: {ex.Message}");
                        if (reporter.AskFallbackToAutomaticDiscovery(state.UserDomainController))
                        {
                            Logger.Log("User chose to fall back to automatic forest discovery");
                            DirectoryContext forestContext;
                            if (state.Username.Length > 0)
                            {
                                reporter.ReportProgress(0, $"Attempting to connect to forest {state.ForestName} with alternate user {state.Username}.");
                                forestContext = new DirectoryContext(DirectoryContextType.Forest, state.ForestName, state.Username, state.Password);
                            }
                            else
                            {
                                reporter.ReportProgress(0, $"Attempting to connect to forest {state.ForestName} as currently logged-on user.");
                                forestContext = new DirectoryContext(DirectoryContextType.Forest, state.ForestName);
                            }
                            forest = Forest.GetForest(forestContext);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    DirectoryContext forestContext;
                    if (state.Username.Length > 0)
                    {
                        reporter.ReportProgress(0, $"Attempting to connect to forest {state.ForestName} with alternate user {state.Username}.");
                        forestContext = new DirectoryContext(DirectoryContextType.Forest, state.ForestName, state.Username, state.Password);
                    }
                    else
                    {
                        reporter.ReportProgress(0, $"Attempting to connect to forest {state.ForestName} as currently logged-on user.");
                        forestContext = new DirectoryContext(DirectoryContextType.Forest, state.ForestName);
                    }

                    forest = Forest.GetForest(forestContext);
                }
            }
            catch (Exception ex)
            {
                if (state.UseUserDomainController)
                {
                    reporter.ReportProgress(0, $"ERROR:Unable to find AD forest:{state.ForestName}\nUsing user specified target domain controller:{state.UserDomainController}\n{ex.Message}\n");
                }
                else
                {
                    reporter.ReportProgress(0, $"ERROR:Unable to find AD forest:{state.ForestName}\n{ex.Message}\n\nYou probably need to manually enter the forest using the button.");
                }
                return null;
            }

            DomainCollection domainCollection = forest.Domains;

            reporter.ReportProgress(0, $"Found {domainCollection.Count} domains in forest {forest.Name}.");

            int numDCs = 0;
            foreach (Domain domain in domainCollection)
            {
                numDCs += domain.DomainControllers.Count;
            }

            var results = new ConcurrentBag<ADREPLDC>();
            int completedDCs = 0;

            foreach (Domain domain in domainCollection)
            {
                var dcList = domain.DomainControllers.Cast<DomainController>().ToList();
                string domainName = domain.Name;

                Parallel.ForEach(dcList, dc =>
                {
                    var adrepldc = new ADREPLDC
                    {
                        Name = dc.Name,
                        DomainName = domainName
                    };

                    if (!IsDCReachable(dc.Name))
                    {
                        reporter.ReportProgress(0, $"DC {adrepldc.Name} is unreachable (TCP port 389 check failed).");

                        adrepldc.Site = "Unknown";
                        adrepldc.IsGC = "Unknown";
                        adrepldc.IsRODC = "Unknown";
                        adrepldc.DiscoveryIssues = true;

                        results.Add(adrepldc);

                        int skipped = Interlocked.Increment(ref completedDCs);
                        int skipPercent = (int)((float)skipped / (float)numDCs * 100);
                        reporter.ReportProgress(skipPercent, "UPDATEPERCENT");
                        return;
                    }

                    try
                    {
                        adrepldc.Site = dc.SiteName;
                    }
                    catch (Exception ex)
                    {
                        reporter.ReportProgress(0, $"Failed to contact DC {adrepldc.Name} and fetch site name:{ex.Message}");
                        adrepldc.Site = "Unknown";
                        adrepldc.DiscoveryIssues = true;
                    }

                    if (!adrepldc.DiscoveryIssues)
                    {
                        try
                        {
                            adrepldc.IsGC = dc.IsGlobalCatalog().ToString();
                        }
                        catch (Exception ex)
                        {
                            reporter.ReportProgress(0, $"Failed to contact DC {adrepldc.Name} and determine global catalog status:{ex.Message}");
                            adrepldc.IsGC = "Unknown";
                            adrepldc.DiscoveryIssues = true;
                        }
                    }
                    else
                    {
                        adrepldc.IsGC = "Unknown";
                    }

                    if (!adrepldc.DiscoveryIssues)
                    {
                        try
                        {
                            using (var directoryEntry = new DirectoryEntry("LDAP://" + dc.Name))
                            {
                                using (var search = new DirectorySearcher(directoryEntry))
                                {
                                    search.ClientTimeout = new TimeSpan(0, 0, 5);
                                    search.Filter = $"(samaccountname={dc.Name.Split('.')[0]}$)";
                                    search.PropertiesToLoad.Add("msDS-isRODC");

                                    SearchResult result = search.FindOne();

                                    if (result == null || result.Properties["msDS-isRODC"].Count == 0)
                                    {
                                        throw new Exception("msDS-isRODC attribute not found!");
                                    }

                                    adrepldc.IsRODC = ((bool)result.Properties["msDS-isRODC"][0]).ToString();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            reporter.ReportProgress(0, $"Failed to determine RODC status for {dc.Name}:{ex.Message}");
                            adrepldc.IsRODC = "Unknown";
                            adrepldc.DiscoveryIssues = true;
                        }
                    }
                    else
                    {
                        adrepldc.IsRODC = "Unknown";
                    }

                    if (!adrepldc.DiscoveryIssues)
                    {
                        try
                        {
                            foreach (ReplicationNeighbor partner in dc.GetAllReplicationNeighbors())
                            {
                                adrepldc.ReplicationPartners.Add(partner);
                            }
                        }
                        catch (Exception ex)
                        {
                            reporter.ReportProgress(0, $"Failed to determine replication neighbors and repl status for {dc.Name}:{ex.Message}");
                            adrepldc.DiscoveryIssues = true;
                        }
                    }

                    results.Add(adrepldc);

                    int done = Interlocked.Increment(ref completedDCs);
                    int percent = (int)((float)done / (float)numDCs * 100);
                    reporter.ReportProgress(percent, "UPDATEPERCENT");
                });
            }

            return results.ToList();
        }

        public static bool IsDCReachable(string hostname, int port = 389)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(hostname, port, null, null);
                    bool connected = result.AsyncWaitHandle.WaitOne(ReachabilityTimeoutMs);
                    if (connected)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
