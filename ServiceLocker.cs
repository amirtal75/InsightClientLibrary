using System;
using System.Collections.Generic;
using NLog;

namespace InsightClientLibrary
{
    /// <summary>
    /// Class with methods used to extract data for locking elements in a corresponding equipment
    /// </summary>
    public class ServiceLocker
    {

        private NLog.Logger logger;
        private string pop;
        private InsightClient client;
        private string LastLockedUUID;
        private bool debug = false;
        private ServiceGraph lastSerachedUuidGraph;
        private bool clientAuthenticated = true;

        /// <summary>
        /// Constructor for getting a service locker.
        /// </summary>
        /// <param name="_debug"> should the program run in debug mode</param>
        /// <param name="pop">the name of the pop the script runs for</param>
        /// <param name="username">the username for Insight login</param>
        /// <param name="password">the password for Insight login</param>
        public ServiceLocker(bool _debug, string pop, string username, string password)
        {
            LastLockedUUID1 = "";
            LastSerachedUuidGraph = null;
            Debug = _debug;
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.Client = new InsightClient(Debug, pop, username, password);
            this.Pop = pop;
        }
        /// <summary>
        /// The pop the service is bieng locked for
        /// </summary>
        public string Pop { get => pop; set => pop = value; }
        /// <summary>
        /// the insight client created for the use of the service locker
        /// </summary>
        public InsightClient Client { get => client; set => client = value; }
        /// <summary>
        /// the last uuid locked by this locker
        /// </summary>
        public string LastLockedUUID1 { get => LastLockedUUID; set => LastLockedUUID = value; }
        /// <summary>
        /// Whether the service locker runs in debug mode
        /// </summary>
        public bool Debug { get => debug; set => debug = value; }
        /// <summary>
        /// the graph built for the last loced uuid
        /// </summary>
        public ServiceGraph LastSerachedUuidGraph { get => lastSerachedUuidGraph; set => lastSerachedUuidGraph = value; }
        /// <summary>
        /// Whether the credentials provided for the client are valid Insight credentials.
        /// </summary>
        public bool ClientAuthenticated { get => clientAuthenticated; set => clientAuthenticated = value; }
        /// <summary>
        /// Constructor for getting a service locker.
        /// </summary>
        /// <param name="_debug"> should the program run in debug mode</param>
        /// <param name="pop">the name of the pop the script runs for</param>
        public ServiceLocker(bool _debug, string pop)
        {
            LastLockedUUID1 = "";
            LastSerachedUuidGraph = null;
            Debug = _debug;
            logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                this.Pop = pop;
                this.Client = new InsightClient(Debug, pop);
                if (!Client.AuthenticationTest.Equals("OK"))
                {
                    ClientAuthenticated = false;
                    return;
                }
            }
            catch (InsightUserAthenticationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                logger.Fatal("Crash due to unknown issue in Service locker: |" + e.Message + "|" + e.StackTrace);
                throw e;
            }


        }


        private List<LockableElement> GetSourceLockElement(string uuid, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, GraphElement source)
        {
            List<GraphElement> list = new List<GraphElement>();
            List<LockableElement> answer = new List<LockableElement>();
            List<LockableElement> toremove = new List<LockableElement>();

            try
            {
                if (lastSerachedUuidGraph == null || lastSerachedUuidGraph.Sources == null)
                {
                    return null;
                }
                List<GraphElement> graphLockables = new List<GraphElement>();
                list = new List<GraphElement>();
                list.Add(source);
                if (pop.Equals(""))
                {
                    graphLockables.AddRange(lastSerachedUuidGraph.getLockableElements(list, "EHA"));
                    graphLockables.AddRange(lastSerachedUuidGraph.getLockableElements(list, "MUC"));
                }
                else graphLockables.AddRange(lastSerachedUuidGraph.getLockableElements(list, Pop));

                List<GraphElement> nextLockables = new List<GraphElement>(graphLockables);
                List<LockableElement> toLock;
                while (nextLockables.Count > 0)
                {
                    graphLockables = nextLockables;
                    nextLockables = new List<GraphElement>();
                    toremove = new List<LockableElement>();
                    foreach (var lockable in graphLockables)
                    {
                        toLock = GetLockElement(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, lastSerachedUuidGraph, source, lockable);
                        if (toLock != null)
                        {
                            answer.AddRange(toLock);
                        }
                    }
                    foreach (var lockableElement in answer)
                    {
                        if (!ValidLockElement(lockableElement))
                        {
                            toremove.Add(lockableElement);
                            if (lockableElement.lockableElement.OutgoingElements != null && lockableElement.lockableElement.OutgoingElements.Count > 0)
                            {
                                nextLockables.AddRange(lastSerachedUuidGraph.FindMinLength(lockableElement.lockableElement.OutgoingElements));
                            }
                        }
                    }
                    foreach (var item in toremove)
                    {
                        answer.Remove(item);
                    }
                    if (answer.Count > 0)
                    {
                        return answer;
                    }
                }
            }
            catch (IllegalNameException e)
            {
                logger.Error(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (InsighClientLibraryUnknownErrorException e)
            {
                logger.Fatal(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (CorruptedInsightDataException e)
            {
                logger.Error(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (RestSharpException e)
            {
                logger.Fatal(e.Message + "|" + e.StackTrace);
                throw new RestSharpException(e.Message + "|" + e.StackTrace);
            }
            catch (UnsuccessfullResponseException e)
            {
                logger.Error(e.Message + "|" + e.StackTrace);
                throw new UnsuccessfullResponseException(e.Message + "|" + e.StackTrace);
            }
            catch (Exception e)
            {
                logger.Fatal("Unknwon error: |" + e.Message + "|" + e.StackTrace);
                throw e;
            }


            return answer;
        }

        /// <summary>
        /// Get a lock element for each source that exists for the given uuid
        /// </summary>
        /// <param name="uuid"> the UUID that we want to lock a source for</param>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <returns></returns>
        public Dictionary<GraphElement, List<LockableElement>> GetSourcesLockElement(string uuid, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel)
        {
            LastLockedUUID = uuid;
            try
            {
                ServiceGraph graph = client.GetServiceGraph(uuid);
                if (graph == null)
                {
                    logger.Error("the graph for: {0} is null", uuid);
                    return null;
                }
                lastSerachedUuidGraph = graph;
                Dictionary<GraphElement, List<LockableElement>> answer = new Dictionary<GraphElement, List<LockableElement>>();
                List<LockableElement> tmp = null;
                foreach (var source in graph.Sources)
                {
                    tmp = GetSourceLockElement(uuid, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, source);
                    if (tmp.Count > 0)
                    {
                        answer.Add(source, tmp);
                    }
                }
                return answer;
            }
            catch (IllegalNameException e)
            {
                logger.Error(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (InsighClientLibraryUnknownErrorException e)
            {
                logger.Fatal(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (CorruptedInsightDataException e)
            {
                logger.Error(e.Message + "|" + e.StackTrace);
                throw e;
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message + "|" + e.StackTrace);
                throw e;
            }

        }
        private static bool ValidLockElement(LockableElement toLock)
        {
            if (toLock == null)
            {
                return false;
            }
            bool answer = false;
            bool legalMulticast = false;
            bool downlinkSymbolRate = true;
            bool modulation = true;
            bool transponder = true;
            bool downlinkPolarity = true;
            bool downlinkSatellite = true;
            if (toLock.GetType() == typeof(IPLockableElement))
            {
                IPLockableElement iPLockable = (IPLockableElement)toLock;
                answer = Tools.LegalIPV4(iPLockable.multicastMain) || Tools.LegalIPV4(iPLockable.multicastBackup);
            }
            else if (toLock.GetType() == typeof(RFLockableElement))
            {
                RFLockableElement rfLockable = (RFLockableElement)toLock;
                downlinkSymbolRate = !rfLockable.downlinkSymbolRate.Equals("");
                modulation = !rfLockable.modulation.Equals("");
                transponder = !rfLockable.transponder.Equals("");
                downlinkPolarity = !rfLockable.downlinkPolarity.Equals("");
                downlinkSatellite = !rfLockable.downlinkSatellite.Equals("");

                if (toLock.GetType() == typeof(MultiLockableElement))
                {
                    MultiLockableElement multiLockable = (MultiLockableElement)toLock;
                    legalMulticast = Tools.LegalIPV4(multiLockable.multicastMain) || Tools.LegalIPV4(multiLockable.multicastBackup);
                    answer = legalMulticast || (downlinkSymbolRate && modulation && transponder && downlinkPolarity && downlinkSatellite);
                }
                else
                {
                    answer = downlinkSymbolRate && modulation && transponder && downlinkPolarity && downlinkSatellite;
                }
            }
            return answer;
        }

        /// <summary>
        /// parse the given data to lockable and readable parameters
        /// </summary>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="elementToLock"></param>
        /// <returns></returns>
        public List<LockableElement> GetLockElement(string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, ServiceGraph graph, GraphElement source, GraphElement elementToLock)
        {
            List<LockableElement> answer = new List<LockableElement>();

            string serviceID = "";
            string attributeName = "";

            // IP lock parameters
            string multicastEHAMain = "";
            string multicastEHABackup = "";
            string multicastMUC = "";
            string multicastMUCBackup = "";
            string sourceIPMain = "";
            string sourceIPBackup = "";

            // RF lock parameters
            string downlinkFrequency = "";
            string modulation = "";
            string downlinkSymbolRate = "";
            string downlinkSatellite = "";
            string downlinkPolarity = "";
            string department = "";
            string transponder = "";
            string rollOff = "";
            string FEC = "";
            string ElementStatus = "";

            RFLockableElement rFLockable = null;
            IPLockableElement iPLockable = null;
            MultiLockableElement multiLockable = null;


            multicastEHAMain = "";
            multicastEHABackup = "";
            multicastMUC = "";
            multicastMUCBackup = "";
            sourceIPMain = "";
            sourceIPBackup = "";

            // RF lock parameters
            downlinkFrequency = "";
            modulation = "";
            downlinkSymbolRate = "";
            downlinkSatellite = "";
            downlinkPolarity = "";
            department = "";
            serviceID = "";
            attributeName = "";

            Dictionary<int, string> attributeTypeNames = new Dictionary<int, string>();

            string elementType = elementToLock.CurrentElement.objectType.name;
            string elementName = elementToLock.CurrentElement.name;
            logger.Debug("The Element to create a lockable for is a {0} element, with the name: {1}", elementType, elementName);

            foreach (var attribute in elementToLock.CurrentElement.attributes)
            {
                if (attribute.ObjectAttributeValues != null && attribute.ObjectAttributeValues.Count > 0)
                {
                    attributeName = elementToLock.ObjectAttributeTypesById[attribute.objectTypeAttributeId];
                    if (attributeName.Equals("Element Status"))
                    {
                        ElementStatus = attribute.ObjectAttributeValues[0].displayValue;
                    }
                    switch (elementType.ToLower())
                    {
                        case "downlink":
                            switch (attributeName)
                            {
                                case "Downlink Frequency":
                                    downlinkFrequency = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Standard/Modulation":
                                    modulation = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Symbol Rate (MSymb/s)":
                                    downlinkSymbolRate = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Satellite":
                                    downlinkSatellite = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Transponder":
                                    transponder = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink FEC":
                                    FEC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Rolloff":
                                    rollOff = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Polarity":
                                    downlinkPolarity = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Department":
                                    department = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Multicast EHA Main":
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Multicast EHA BU":
                                    multicastEHABackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Multicast MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Service ID":
                                    serviceID = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "monitoring":
                            switch (attributeName)
                            {
                                case "Monitoring Multicast EHA": // belongs to monitoring element
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Multicast MUC":// belongs to monitoring element
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Satellite":// belongs to monitoring element
                                    downlinkSatellite = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Transponder":
                                    transponder = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring FEC":
                                    FEC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Rolloff":
                                    rollOff = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Frequency":// belongs to monitoring element
                                    downlinkFrequency = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Simbolrate":// belongs to monitoring element
                                    downlinkSymbolRate = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Standard":// belongs to monitoring element
                                    modulation = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Monitoring Polarity":// belongs to monitoring element
                                    downlinkPolarity = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Program Number":// belongs to monitoring element
                                    serviceID = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "upink":
                            switch (attributeName)
                            {
                                case "Uplink Satellite":// belongs to uplink element
                                    downlinkSatellite = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Polarity":
                                    downlinkPolarity = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Downlink Frequency (MHz)":// belongs to uplink element
                                    downlinkFrequency = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Uplink Symbol Rate (MSymb/s)":// belongs to uplink element
                                    downlinkSymbolRate = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Uplink Standard/Modulation":// belongs to uplink element
                                    modulation = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Uplink Service ID":// belongs to uplink element
                                    serviceID = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Transponder":
                                    transponder = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Uplink FEC":
                                    FEC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Uplink Rolloff":
                                    rollOff = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case "encoding":
                            switch (attributeName)
                            {
                                case "Encoding Multicast EHA Main":
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Encoding Multicast EHA BU":
                                    multicastEHABackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Encoding Multicast MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Service ID": // belongs to the encoding element
                                    serviceID = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "muxing":
                            switch (attributeName)
                            {
                                case "Muxing Output Main Multicast EHA":
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Muxing Output BU Multicast EHA":
                                    multicastEHABackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Muxing Output Main Multicast MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Muxing Output BU Multicast MUC":
                                    multicastMUCBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Muxing Main Source IP":
                                    sourceIPMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Muxing BU Source IP":
                                    sourceIPBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "timeshift":
                            switch (attributeName)
                            {
                                case "Timeshift Multicast EHA":
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Timeshift Multicast MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "mbr":
                            switch (attributeName)
                            {
                                case "Multicast Out Main EHA": // belongs to MBR element
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Multicast Out BU EHA":// belongs to MBR element
                                    multicastEHABackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Multicast Out Main MUC":// belongs to MBR element
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Multicast Out BU MUC":// belongs to MBR element
                                    multicastMUCBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "ip to ip gateway":
                            switch (attributeName)
                            {
                                case "Multicast EHA Out": // belongs to ip to ip gateway element
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Multicast MUC Out":// belongs to ip to ip gateway element
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Source IP Out":// belongs to ip to ip gateway element
                                    sourceIPMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "decoding":
                            switch (attributeName)
                            {
                                case "Multicast Out EHA": // belongs to decoding element
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Multicast Out MUC":// belongs to decoding element
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "channel in a box":
                            switch (attributeName)
                            {
                                case "Output Multicast EHA": // belongs to channel in a box element
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Output Multicast MUC":// belongs to channel in a box element
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "fiber video transfer":
                            switch (attributeName)
                            {
                                case "Fiber Video Transfer Multicast IP EHA":
                                    multicastEHAMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Fiber Video Transfer Backup Multicast IP EHA":
                                    multicastEHABackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Fiber Video Transfer Multicast IP MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Fiber Video Transfer Backup Multicast IP MUC":
                                    multicastMUCBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "dvb server":
                            switch (attributeName)
                            {
                                case "Output Main Multicast MUC":
                                    multicastMUC = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Output Backup Multicast MUC":
                                    multicastMUCBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Output Main Source IP":
                                    sourceIPMain = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                case "Output Backup Source IP":
                                    sourceIPBackup = attribute.ObjectAttributeValues[0].displayValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (elementType.Equals("Downlink") || elementType.Equals("Monitoring"))
            {
                if (Pop.ToLower().Equals("eha") || Pop.Equals(""))
                {
                    multiLockable = new MultiLockableElement(multicastEHAMain, multicastEHABackup, sourceIPMain, sourceIPBackup, downlinkFrequency, modulation, downlinkPolarity,
                                                    downlinkSatellite, transponder, FEC, rollOff, downlinkSymbolRate, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, Pop.ToLower(), serviceID, source, elementToLock);
                    answer.Add(multiLockable);
                }
                if (Pop.ToLower().Equals("muc") || Pop.Equals(""))
                {
                    multiLockable = new MultiLockableElement(multicastMUC, multicastMUCBackup, sourceIPMain, sourceIPBackup, downlinkFrequency, modulation, downlinkPolarity,
                                downlinkSatellite, transponder, FEC, rollOff, downlinkSymbolRate, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, Pop.ToLower(), serviceID, source, elementToLock);
                    answer.Add(multiLockable);
                }
                logger.Debug("lockable element parameters:|" + multiLockable.ToString());
            }
            else if (elementType.Equals("Uplink"))
            {
                logger.Debug("Given Element: {0}, RFLockable returned.", elementType);
                rFLockable = new RFLockableElement(downlinkFrequency, modulation, downlinkPolarity, downlinkSatellite, transponder, rollOff, FEC, downlinkSymbolRate, destinationIrdManagmentIp,
                                                    destinationIrdName, destinationIrdModel, Pop.ToLower(), serviceID, source, elementToLock);
                answer.Add(rFLockable);
                logger.Debug("lockable element parameters:|" + rFLockable.ToString());
            }
            else
            {
                logger.Debug("Given Element: {0}, IPLockable returned.", elementType);
                if (Pop.ToLower().Equals("eha") || Pop.Equals(""))
                {
                    iPLockable = new IPLockableElement(multicastEHAMain, multicastEHABackup, sourceIPMain, sourceIPBackup, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, Pop.ToLower(), serviceID, source, elementToLock);
                    answer.Add(iPLockable);
                }
                if (Pop.ToLower().Equals("muc") || Pop.Equals(""))
                {
                    iPLockable = new IPLockableElement(multicastMUC, multicastMUCBackup, sourceIPMain, sourceIPBackup, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, Pop.ToLower(), serviceID, source, elementToLock);
                    answer.Add(iPLockable);
                }
                logger.Debug("lockable element parameters:|" + iPLockable.ToString());
            }
            return answer;
        }
    }
    /// <summary>
    /// RFLockableElement is a class for locking an RF input
    /// </summary>
    public class RFLockableElement : LockableElement
    {
        /* this constructor is unnecessary for almost all familiar receivers, basic constructor is recommended
        public RFLockableElement(string downlinkFrequency, string downlinkLocalOsscilator, string modulation, string downlinkPolarity, string downlinkSatellite, string downlinkSymbolRate, string downlinkFEC, string downlinkRollOff, string transponder, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel)
        {
            this.downlinkFrequency = downlinkFrequency;
            this.downlinkLocalOsscilator = downlinkLocalOsscilator;
            Modulation = modulation;
            this.downlinkPolarity = downlinkPolarity;
            this.downlinkSatellite = downlinkSatellite;
            this.downlinkSymbolRate = downlinkSymbolRate;
            this.downlinkFEC = downlinkFEC;
            this.downlinkRollOff = downlinkRollOff;
            this.transponder = transponder;
            this.destinationIrdManagmentIp = destinationIrdManagmentIp;
            this.destinationIrdName = destinationIrdName;
            this.destinationIrdModel = destinationIrdModel;
        }
        */
        /// <summary>
        /// Constructor for an RF lock, parameters are self explanatory
        /// </summary>
        /// <param name="downlinkFrequency"></param>
        /// <param name="modulation"></param>
        /// <param name="downlinkPolarity"></param>
        /// <param name="downlinkSatellite"></param>
        /// <param name="transponder"></param>
        /// <param name="downlinkFEC"></param>
        /// <param name="downlinkRollOff"></param>
        /// <param name="downlinkSymbolRate"></param>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <param name="pop"></param>
        /// <param name="serviceID"></param>
        /// <param name="source"></param>
        /// <param name="lockableElement"></param>
        public RFLockableElement(string downlinkFrequency, string modulation, string downlinkPolarity, string downlinkSatellite, string transponder, string downlinkFEC, string downlinkRollOff, string downlinkSymbolRate,
                                 string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop, string serviceID, GraphElement source, GraphElement lockableElement) :
                                        base(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, source, lockableElement)
        {
            this.downlinkLocalOsscilator = new List<string>();
            try
            {
                int osscil = Convert.ToInt32(downlinkLocalOsscilator);
                int frequency = Convert.ToInt32(downlinkFrequency);
                if (frequency > 3050 && frequency < 4200)
                {
                    this.downlinkLocalOsscilator.Add("5150");
                }
                else if (frequency > 10700 && frequency < 11850 && osscil == 9750)
                {
                    this.downlinkLocalOsscilator.Add("9750");
                }
                else if (frequency > 10950 && frequency < 12100 && osscil == 10000)
                {
                    this.downlinkLocalOsscilator.Add("10000");
                }
                else if (frequency > 11550 && frequency < 12700 && osscil == 10600)
                {
                    this.downlinkLocalOsscilator.Add("10600");
                }
                else if (frequency > 11700 && frequency < 12850 && osscil == 10700)
                {
                    this.downlinkLocalOsscilator.Add("10700");
                }
            }
            catch (Exception)
            {
                downlinkLocalOsscilator.Add("N/A");
            }
            this.downlinkFrequency = downlinkFrequency;
            this.modulation = modulation;
            this.downlinkPolarity = downlinkPolarity;
            this.downlinkSatellite = downlinkSatellite;
            this.downlinkSymbolRate = downlinkSymbolRate;
            this.downlinkFEC = downlinkFEC;
            this.downlinkRollOff = downlinkRollOff;
            this.transponder = transponder;
        }
        /// <summary> </summary>
        public string downlinkFrequency { get; set; }
        /// <summary> </summary>
        public List<string> downlinkLocalOsscilator { get; set; }
        /// <summary> </summary>
        public string modulation { get; set; }
        /// <summary> </summary>
        public string downlinkPolarity { get; set; }
        /// <summary> </summary>
        public string downlinkSatellite { get; set; }
        /// <summary> </summary>
        public string downlinkSymbolRate { get; set; }
        /// <summary> </summary>
        public string downlinkFEC { get; set; }
        /// <summary> </summary>
        public string downlinkRollOff { get; set; }
        /// <summary> </summary>
        public string transponder { get; set; }
        /// <summary> </summary>
        public override bool Equals(object obj)

        {
            return obj is RFLockableElement element &&
                   base.Equals(obj) &&
                   downlinkFrequency == element.downlinkFrequency &&
                   modulation == element.modulation &&
                   downlinkPolarity == element.downlinkPolarity &&
                   downlinkSatellite == element.downlinkSatellite &&
                   downlinkSymbolRate == element.downlinkSymbolRate;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()

        {
            int hashCode = -701880288;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(downlinkFrequency);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(modulation);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(downlinkPolarity);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(downlinkSatellite);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(downlinkSymbolRate);
            return hashCode;
        }

        /// <summary> </summary>
        public override string ToString()

        {
            return base.ToString() +
                "downlinkFrequency: " + downlinkFrequency + "|" +
                "downlinkLocalOsscilator: " + downlinkLocalOsscilator + "|" +
                "modulation: " + modulation + "|" +
                "downlinkPolarity: " + downlinkPolarity + "|" +
                "downlinkSatellite: " + downlinkSatellite + "|" +
                "downlinkSymbolRate: " + downlinkSymbolRate + "|";
        }
    }

    /// <summary>
    /// IPLockableElement is a class for locking an IP input
    /// </summary>
    public class IPLockableElement : LockableElement
    {
        /// <summary> </summary>
        public IPLockableElement(string multicastMain, string multicastBackup, string sourceIpMain, string sourceIpBU, string destinationIrdManagmentIp,

            string destinationIrdName, string destinationIrdModel, string pop, string serviceID, GraphElement source, GraphElement lockableElement) :
                                        base(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, source, lockableElement)
        {
            this.multicastMain = multicastMain;
            this.multicastBackup = multicastBackup;
            this.sourceIpMain = sourceIpMain;
            this.sourceIpBU = sourceIpBU;
        }
        /// <summary> </summary>
        public string multicastMain { get; set; }
        /// <summary> </summary>
        public string multicastBackup { get; set; }
        /// <summary> </summary>
        public string sourceIpMain { get; set; }
        /// <summary> </summary>
        public string sourceIpBU { get; set; }

        /// <summary> </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is IPLockableElement element &&
                   base.Equals(obj) &&
                   multicastMain == element.multicastMain &&
                   multicastBackup == element.multicastBackup &&
                   sourceIpMain == element.sourceIpMain &&
                   sourceIpBU == element.sourceIpBU;
        }
        /// <summary> </summary>
        /// <returns></returns>
        public override int GetHashCode()

        {
            int hashCode = -704129115;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastBackup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpBU);
            return hashCode;
        }
        /// <summary> </summary>
        public override string ToString()

        {
            return base.ToString() +
                "multicastMain: " + multicastMain + "|" +
                "multicastBackup: " + multicastBackup + "|" +
                "sourceIpMain: " + sourceIpMain + "|" +
                "sourceIpBU: " + sourceIpBU + "|";
        }
    }
    /// <summary>
    /// MultiLockableElement is a class for locking an element with noth an IP and RF parameters, like a downlink element with an outgoing multicast
    /// </summary>
    public class MultiLockableElement : RFLockableElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MultiLockableElement(string multicastMain, string multicastBackup, string sourceIpMain, string sourceIpBU, string downlinkFrequency, string modulation, string downlinkPolarity,
                                    string downlinkSatellite, string transponder, string FEC, string rollOff, string downlinkSymbolRate, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop,
                                    string serviceID, GraphElement source, GraphElement elementToLock) :
                                        base(downlinkFrequency, modulation, downlinkPolarity, downlinkSatellite, transponder, FEC, rollOff, downlinkSymbolRate,
                                        destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, source, elementToLock)
        {
            this.multicastMain = multicastMain;
            this.multicastBackup = multicastBackup;
            this.sourceIpMain = sourceIpMain;
            this.sourceIpBU = sourceIpBU;

        }
        /// <summary> </summary>
        public string multicastMain { get; set; }
        /// <summary> </summary>
        public string multicastBackup { get; set; }
        /// <summary> </summary>
        public string sourceIpMain { get; set; }
        /// <summary> </summary>
        public string sourceIpBU { get; set; }

        /// <summary> </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is MultiLockableElement element &&
                   base.Equals(obj) &&
                   multicastMain == element.multicastMain &&
                   multicastBackup == element.multicastBackup &&
                   sourceIpMain == element.sourceIpMain &&
                   sourceIpBU == element.sourceIpBU;
        }
        /// <summary> </summary>
        /// <returns></returns>
        public override int GetHashCode()

        {
            int hashCode = -704129115;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastBackup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpBU);
            return hashCode;
        }
        /// <summary> </summary>
        /// <returns></returns>
        public override string ToString()

        {
            return base.ToString() +
                "multicastMain: " + multicastMain + "|" +
                "multicastBackup: " + multicastBackup + "|" +
                "sourceIpMain: " + sourceIpMain + "|" +
                "sourceIpBU: " + sourceIpBU + "|";
        }
    }
    /// <summary>
    /// abstract base class for a lockable element
    /// </summary>
    public abstract class LockableElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <param name="pop"></param>
        /// <param name="serviceID"></param>
        /// <param name="source"></param>
        /// <param name="_lockableElement"></param>
        protected LockableElement(string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop, string serviceID, GraphElement source, GraphElement _lockableElement)

        {
            this.destinationIrdManagmentIp = destinationIrdManagmentIp;
            this.destinationIrdName = destinationIrdName;
            this.destinationIrdModel = destinationIrdModel;
            this.pop = pop;
            this.sourceElement = source;
            this.lockableElement = _lockableElement;
            this.serviceID = serviceID;
            this.elementName = lockableElement.CurrentElement.name;
            this.elementType = lockableElement.CurrentElement.objectType.name;
        }
        /// <summary>
        /// 
        /// </summary>
        public GraphElement sourceElement { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public GraphElement lockableElement { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pop { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string destinationIrdManagmentIp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string elementName { get; set; }
        /// <summary> </summary>
        public string elementType { get; set; }
        /// <summary> </summary>
        public string destinationIrdName { get; set; }
        /// <summary> </summary>
        public string destinationIrdModel { get; set; }
        /// <summary> </summary>
        public string serviceID { get; set; }
        /// <summary> </summary>
        public override bool Equals(object obj)
        {
            return obj is LockableElement element &&
                   pop == element.pop &&
                   destinationIrdManagmentIp == element.destinationIrdManagmentIp &&
                   elementName == element.elementName &&
                   elementType == element.elementType &&
                   destinationIrdName == element.destinationIrdName &&
                   destinationIrdModel == element.destinationIrdModel &&
                   serviceID == element.serviceID;
        }
        /// <summary> </summary>
        public override int GetHashCode()
        {
            int hashCode = 453849517;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(pop);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(destinationIrdManagmentIp);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(elementName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(elementType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(destinationIrdName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(destinationIrdModel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(serviceID);
            return hashCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Element Name: " + elementName + "|" +
                "Element Type: " + elementType + "|" +
                "Source Element Name: " + sourceElement.CurrentElement.name + "|" +
                "Source Element Type: " + sourceElement.CurrentElement.objectType.name + "|" +
                "POP: " + pop + "|" +
                "Receiver Name: " + destinationIrdName + "|" +
                "Receiver Model: " + destinationIrdModel + "|" +
                "Receiver Managment IP: " + destinationIrdManagmentIp + "|";
        }
    }
}