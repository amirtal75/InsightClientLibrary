using System;
using System.Collections.Generic;
using NLog;

namespace InsightClientLibrary
{
    /// <summary>
    /// Class with methods used to extract dta for locking elements in a corresponding equipment
    /// </summary>
    public class ServiceLocker
    {
        #pragma warning disable CS1591
        public NLog.Logger logger;
        #pragma warning restore CS1591
        #pragma warning disable CS1591
        public string pop;
        #pragma warning restore CS1591
        #pragma warning disable CS1591
        public InsightClient client;
        #pragma warning restore CS1591
        private string LastLockedUUID;
        #pragma warning disable CS1591
        public bool debug = false;
        #pragma warning restore CS1591
        #pragma warning disable CS1591
        public ServiceGraph LastSerachedUuidGraph;
        #pragma warning restore CS1591
        /// <summary>
        /// Constructor for getting a service locker.
        /// </summary>
        /// <param name="_debug"> should the program run in debug mode</param>
        /// <param name="pop">the name of the pop the script runs for</param>
        /// <param name="client"> the client which will be used to perform the actions</param>
        public ServiceLocker(bool _debug, string pop, InsightClient client)
        {
            LastLockedUUID = "";
            LastSerachedUuidGraph = null;
            debug = _debug;
            InitLogger(debug);
            this.client = client;
            this.pop = pop;

        }

        #pragma warning disable CS1572 // XML comment has a param tag, but there is no parameter by that name
        /// <summary>
        /// Constructor for getting a service locker.
        /// </summary>
        /// <param name="_debug"> should the program run in debug mode</param>
        /// <param name="pop">the name of the pop the script runs for</param>
        public ServiceLocker(bool _debug, string pop)
        {
            LastLockedUUID = "";
            LastSerachedUuidGraph = null;
            debug = _debug;
            InitLogger(debug);
            this.client = new InsightClient(debug,pop);
            this.pop = pop;

        }
        /// <summary>
        /// Get a lock element for each source that exists for the given uuid
        /// </summary>
        /// <param name="uuid"> the UUID that we want to lock a source for</param>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <returns></returns>
        public List<LockableElement> GetSourceLockElement(string uuid, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel)
        {
            LastLockedUUID = uuid;
            List<LockableElement> answer = new List<LockableElement>();
            ServiceGraph graph = client.GetServiceGraph(uuid);
            LastSerachedUuidGraph = graph;
            if (graph == null || graph.Sources == null)
            {
                return null;
            }
            foreach (var source in graph.Sources)
            {
                if (source != null)
                {
                    LockableElement toLock = GetLockElement(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, graph, source);
                    if (toLock != null)
                    {
                        answer.Add(toLock);
                    }
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
        /// <param name="elementToLock"></param>
        /// <returns></returns>
        public LockableElement GetLockElement(string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, ServiceGraph graph, GraphElement elementToLock)
        {
            LockableElement answer = null;

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


            string elementType = elementToLock.CurrentElement.objectType.name;
            string elementName = elementToLock.CurrentElement.name;
            logger.Debug("The Element to create a lockable for is a {0} element, with the name: {1}", elementType, elementName);

            foreach (var attribute in elementToLock.CurrentElement.attributes)
            {
                if (attribute.ObjectAttributeValues != null && attribute.ObjectAttributeValues.Count > 0)
                {
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
                logger.Debug("Given Element: {0}, Multilockable returned.", elementType);
                multiLockable = new MultiLockableElement(multicastEHAMain, multicastEHABackup, sourceIPMain, sourceIPBackup, downlinkFrequency, modulation, downlinkPolarity,
                                downlinkSatellite, downlinkSymbolRate, destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop.ToLower(), serviceID, elementType, elementName);
                answer = multiLockable;
                logger.Debug("\nlockable element parameters:\n" + multiLockable.ToString());

            }
            else if (elementType.Equals("Uplink"))
            {
                logger.Debug("Given Element: {0}, RFLockable returned.", elementType);
                rFLockable = new RFLockableElement(downlinkFrequency, modulation, downlinkPolarity, downlinkSatellite, downlinkSymbolRate, destinationIrdManagmentIp, 
                                                    destinationIrdName, destinationIrdModel, pop.ToLower(), serviceID, elementType, elementName);
                answer = rFLockable;
                logger.Debug("\nlockable element parameters:\n" + rFLockable.ToString());
            }
            else
            {
                logger.Debug("Given Element: {0}, IPLockable returned.", elementType);
                if (pop.ToLower().Equals("eha"))
                {
                    iPLockable = new IPLockableElement(multicastEHAMain, multicastEHABackup, sourceIPMain, sourceIPBackup, destinationIrdName, destinationIrdName, destinationIrdModel, pop.ToLower(), serviceID, elementType, elementName);
                }
                else if (pop.ToLower().Equals("muc"))
                {
                    iPLockable = new IPLockableElement(multicastMUC, "", sourceIPMain, sourceIPBackup, destinationIrdName, destinationIrdName, destinationIrdModel, pop.ToLower(), serviceID, elementType, elementName);
                }
                answer = iPLockable;
                logger.Debug("\nlockable element parameters:\n" + iPLockable.ToString());
            }
            return answer;
        }
        private void InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"D:\Amir\Log.txt" }
            ;

            // Rules for mapping loggers to targets            
            if (debug)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            }
            else config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);


            // Apply config           
            NLog.LogManager.Configuration = config;
            logger = NLog.LogManager.GetCurrentClassLogger(); logger.Info("Log Start");
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
        /// <param name="downlinkSymbolRate"></param>
        /// <param name="destinationIrdManagmentIp"></param>
        /// <param name="destinationIrdName"></param>
        /// <param name="destinationIrdModel"></param>
        /// <param name="pop"></param>
        /// <param name="serviceID"></param>
        /// <param name="elementName"></param>
        /// <param name="elementType"></param>
        public RFLockableElement(string downlinkFrequency, string modulation, string downlinkPolarity, string downlinkSatellite, string downlinkSymbolRate,
                                 string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop, string serviceID, string elementName, string elementType) :
                                        base(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, elementName, elementType)
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

        }
        string downlinkFrequency { get; set; }
        List<string> downlinkLocalOsscilator { get; set; }
        string modulation { get; set; }
        string downlinkPolarity { get; set; }
        string downlinkSatellite { get; set; }
        string downlinkSymbolRate { get; set; }
        string downlinkFEC { get; set; }
        string downlinkRollOff { get; set; }
        string transponder { get; set; }

#pragma warning disable CS1591
        public override bool Equals(object obj)
#pragma warning restore CS1591
        {
            return obj is RFLockableElement element &&
                   base.Equals(obj) &&
                   downlinkFrequency == element.downlinkFrequency &&
                   modulation == element.modulation &&
                   downlinkPolarity == element.downlinkPolarity &&
                   downlinkSatellite == element.downlinkSatellite &&
                   downlinkSymbolRate == element.downlinkSymbolRate;
        }

#pragma warning disable CS1591
        public override int GetHashCode()
#pragma warning restore CS1591
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

#pragma warning disable CS1591
        public override string ToString()
#pragma warning restore CS1591
        {
            return base.ToString() + 
                "downlinkFrequency: " + downlinkFrequency + "\n" +
                "downlinkLocalOsscilator: " + downlinkLocalOsscilator + "\n" +
                "modulation: " + modulation + "\n" +
                "downlinkPolarity: " + downlinkPolarity + "\n" +
                "downlinkSatellite: " + downlinkSatellite + "\n" +
                "downlinkSymbolRate: " + downlinkSymbolRate + "\n";
        }
    }

    /// <summary>
    /// IPLockableElement is a class for locking an IP input
    /// </summary>
    public class IPLockableElement : LockableElement
    {
#pragma warning disable CS1591
        public IPLockableElement(string multicastMain, string multicastBackup, string sourceIpMain, string sourceIpBU, string destinationIrdManagmentIp,
#pragma warning restore CS1591
            string destinationIrdName, string destinationIrdModel, string pop, string serviceID, string elementName, string elementType) :
                                        base(destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, elementName, elementType)
        {
            this.multicastMain = multicastMain;
            this.multicastBackup = multicastBackup;
            this.sourceIpMain = sourceIpMain;
            this.sourceIpBU = sourceIpBU;
        }
        string multicastMain { get; set; }
        string multicastBackup { get; set; }
        string sourceIpMain { get; set; }
        string sourceIpBU { get; set; }

#pragma warning disable CS1591
        public override bool Equals(object obj)
#pragma warning restore CS1591
        {
            return obj is IPLockableElement element &&
                   base.Equals(obj) &&
                   multicastMain == element.multicastMain &&
                   multicastBackup == element.multicastBackup &&
                   sourceIpMain == element.sourceIpMain &&
                   sourceIpBU == element.sourceIpBU;
        }

#pragma warning disable CS1591
        public override int GetHashCode()
#pragma warning restore CS1591
        {
            int hashCode = -704129115;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastBackup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpBU);
            return hashCode;
        }

#pragma warning disable CS1591
        public override string ToString()
#pragma warning restore CS1591
        {
            return base.ToString() +
                "multicastMain: " + multicastMain + "\n" +
                "multicastBackup: " + multicastBackup + "\n" +
                "sourceIpMain: " + sourceIpMain + "\n" +
                "sourceIpBU: " + sourceIpBU + "\n";
        }
    }
    /// <summary>
    /// MultiLockableElement is a class for locking an element with noth an IP and RF parameters, like a downlink element with an outgoing multicast
    /// </summary>
    public class MultiLockableElement : RFLockableElement
    {
#pragma warning disable CS1591
        public MultiLockableElement(string multicastMain, string multicastBackup, string sourceIpMain, string sourceIpBU, string downlinkFrequency, string modulation, string downlinkPolarity,
#pragma warning restore CS1591
                                    string downlinkSatellite, string downlinkSymbolRate, string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop,
                                    string serviceID, string elementName, string elementType) :
                                        base(downlinkFrequency, modulation, downlinkPolarity, downlinkSatellite, downlinkSymbolRate,
                                        destinationIrdManagmentIp, destinationIrdName, destinationIrdModel, pop, serviceID, elementName, elementType)
        {
            this.multicastMain = multicastMain;
            this.multicastBackup = multicastBackup;
            this.sourceIpMain = sourceIpMain;
            this.sourceIpBU = sourceIpBU;

        }

        string multicastMain { get; set; }
        string multicastBackup { get; set; }
        string sourceIpMain { get; set; }
        string sourceIpBU { get; set; }

#pragma warning disable CS1591
        public override bool Equals(object obj)
#pragma warning restore CS1591
        {
            return obj is MultiLockableElement element &&
                   base.Equals(obj) &&
                   multicastMain == element.multicastMain &&
                   multicastBackup == element.multicastBackup &&
                   sourceIpMain == element.sourceIpMain &&
                   sourceIpBU == element.sourceIpBU;
        }

#pragma warning disable CS1591
        public override int GetHashCode()
#pragma warning restore CS1591
        {
            int hashCode = -704129115;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(multicastBackup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpMain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(sourceIpBU);
            return hashCode;
        }

#pragma warning disable CS1591
        public override string ToString()
#pragma warning restore CS1591
        {
            return base.ToString() +
                "multicastMain: " + multicastMain + "\n" +
                "multicastBackup: " + multicastBackup + "\n" +
                "sourceIpMain: " + sourceIpMain + "\n" +
                "sourceIpBU: " + sourceIpBU + "\n";
        }
    }
    /// <summary>
    /// abstract base class for a lockable element
    /// </summary>
    public abstract class LockableElement
    {
#pragma warning disable CS1591
        protected LockableElement(string destinationIrdManagmentIp, string destinationIrdName, string destinationIrdModel, string pop, string serviceID, string elementName, string elementType)
#pragma warning restore CS1591
        {
            this.destinationIrdManagmentIp = destinationIrdManagmentIp;
            this.destinationIrdName = destinationIrdName;
            this.destinationIrdModel = destinationIrdModel;
            this.pop = pop;
            this.serviceID = serviceID;
        }
        string pop { get; set; }
        string destinationIrdManagmentIp { get; set; }
        string elementName { get; set; }
        string elementType { get; set; }
        string destinationIrdName { get; set; }
        string destinationIrdModel { get; set; }
        string serviceID { get; set; }

#pragma warning disable CS1591
        public override bool Equals(object obj)
#pragma warning restore CS1591
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

#pragma warning disable CS1591
        public override int GetHashCode()
#pragma warning restore CS1591
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

#pragma warning disable CS1591
        public override string ToString()
#pragma warning restore CS1591
        {
            return "Element Name: " + elementName + "\n" +
                "Element Type: " + elementType + "\n" +
                "POP: " + pop + "\n" +
                "Receiver Name: " + destinationIrdName + "\n" +
                "Receiver Model: " + destinationIrdModel + "\n" +
                "Receiver Managment IP: " + destinationIrdManagmentIp + "\n";
        }
    }
}
