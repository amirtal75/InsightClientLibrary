using System;
using System.Net;
using SnmpSharpNet;
using NLog;
using System.Collections.Generic;
using System.Collections.Specialized;
// Based on snmpshart for .Net
// http://www.snmpsharpnet.com
namespace CiscoSnmpGet
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            InitLogger(true);
            AuthenticationDigests authDigest = AuthenticationDigests.None;
            PrivacyProtocols privacyprotocol = PrivacyProtocols.None;
            string ip = "172.19.27.8";
            int port = 161;
            VbCollection vbCollection = null;
            string community = "public";
            UdpTarget target = null;
            int version = 2;
            SnmpV2Packet result2 = null;
            int ErrorStatus = -1;
            int ErrorIndex = -1;

            string sidbase2 = "1.3.6.1.4.1.1429.2.2.6.5.12.3.1.2.";
            string NameBase = "1.3.6.1.4.1.1429.2.2.6.5.12.3.1.4.";
            string sidbase = "1.3.6.1.4.1.8813.2.366";
            try
            {
                String snmpAgent = "172.19.27.8";
                String snmpCommunity = "public";
                SimpleSnmp snmp = new SimpleSnmp(snmpAgent, snmpCommunity);
                // Create a request Pdu
                Pdu pdu = new Pdu();
                pdu.Type = PduType.GetNext;
                List<string> lst1 = new List<string>();
                List<string> lst2 = new List<string>();

                string sid = "";
                string channelName = "";

                Dictionary<string, string> channelList = new Dictionary<string, string>();
                Dictionary<Oid, AsnType> result = null;

                for (int i = 0; i < 1; i++)
                {
                    sid = "";
                    channelName = "";
                    result = snmp.GetNext(SnmpVersion.Ver2,new string[]{ (sidbase + i) });
                    if (result == null)
                    {
                        logger.Debug("Request failed.");
                    }
                    else
                    {
                        foreach (KeyValuePair<Oid, AsnType> entry in result)
                        {
                            if (entry.Key.ToString().Contains(sidbase))
                            {
                                sid = entry.Value.ToString();
                            }
                            else i = 100;
                        }
                    }
                    result = snmp.GetNext(SnmpVersion.Ver2, new string[] { (NameBase + i) });
                    if (result == null)
                    {
                        logger.Debug("Request failed.");
                    }
                    else
                    {
                        foreach (KeyValuePair<Oid, AsnType> entry in result)
                        {
                            if (entry.Key.ToString().Contains(NameBase))
                            {
                                channelName = entry.Value.ToString();
                            }
                        }
                    }

                    if (!sid.Equals("") && !channelName.Equals(""))
                    {
                        channelList.Add(sid, channelName);
                    }
                }

                foreach (var item in channelList)
                {
                    logger.Debug(item.Key + ":" + item.Value);
                }
            }
            catch (Exception e)
            {

                logger.Debug(e.Message);
                logger.Debug(e.StackTrace);

            }
            /*
            try
            {
                VbCollection coll = new VbCollection();
                coll.Add("1.3.6.1.4.1.1429.2.2.6.5.2.1.1.1");
                coll.Add("1.3.6.1.4.1.1429.2.2.6.5.2.1.1.2");
                Pdu pdu = new Pdu(coll, PduType.GetBulk, 0);
                pdu.MaxRepetitions = 10;
                String snmpAgent = "172.19.54.30";
                String snmpCommunity = "public";
                SimpleSnmp snmp = new SimpleSnmp(snmpAgent, snmpCommunity);
                for (int i = 0; i < 1; i++)
                {
                    Dictionary<Oid, AsnType> result = snmp.GetBulk(pdu);
                    
                    if (result == null)
                    {
                        logger.Debug("Request failed.");
                    }
                    else
                    {
                        logger.Debug("result size: " + result.Count);
                        string filename = "D:\\Amir\\SNMP " + i + ".txt";
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filename))
                        {
                            foreach (KeyValuePair<Oid, AsnType> entry in result)
                            {
                                var x = entry.Value;
                                file.WriteLine("{0} = {1}: {2}", entry.Key.ToString(), SnmpConstants.GetTypeName(entry.Value.Type),
                                entry.Value.ToString());
                            }
                        }

                    }
                }
                
               
            }
            catch (Exception e)
            {

                logger.Debug(e.Message);
                logger.Debug(e.StackTrace);
            }
            */
            /*try
            {
                IpAddress ipa = new IpAddress(ip);
                target = new UdpTarget((IPAddress)ipa);
                target.Port = port;
                // Construct a Protocol Data Unit (PDU)
                Pdu pdu = new Pdu(PduType.Get);
                // Set the request type (default is Get)
                // pdu.Type = PduType.Get;
                // Add variables you wish to query
                pdu.VbList.Add(oid); //oid);
                OctetString communityOctet = new OctetString(community);
                AgentParameters param = new AgentParameters(communityOctet);

                
                
                param.Version = SnmpVersion.Ver2;
                logger.Debug("before get");
                result2 = (SnmpV2Packet)target.Request(pdu, param);
                logger.Debug("after get");

                vbCollection = result2.Pdu.VbList;
                ErrorStatus = result2.Pdu.ErrorStatus;
                ErrorIndex = result2.Pdu.ErrorIndex;
               
                if (ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    logger.Debug("Error in SNMP reply. Error {0} index {1}",
                        ErrorStatus, ErrorIndex);
                }
                else
                {
                    if (vbCollection != null)
                    {
                        foreach (Vb v in vbCollection)
                        {
                            logger.Debug("OID={0}\r\nType={1}\r\nValue:{2}",
                                v.Oid.ToString(),
                                SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
                        }
                    }
                    else logger.Debug("VB COLLECTION IS NULL");
                }
                target.Close();
            }
            catch (Exception ex)
            {
                if (target != null)
                    target.Close();
                logger.Debug("Exception: " + ex.Message + " type: " + ex.GetType().ToString());
            }
                */
            /*
            string oid = "";
            string ip = "";
            int port = 161;
            string securityuser = "";
            string authenticationdigest = "";
            AuthenticationDigests authDigest = AuthenticationDigests.None;
            string authenticationpassword = "";
            PrivacyProtocols privacyprotocol = PrivacyProtocols.None;
            string privacyprotocolstring = "";
            string privacyprotocolpassword = "";
            string community = "";
            int version = -1;
            SnmpV3Packet result3;
            SnmpV1Packet result1;
            SnmpV2Packet result2;
            
            int ErrorStatus = -1;
            int ErrorIndex = -1;
            if (args.Length == 0)
            {
                Console.WriteLine("Specify arguments:\r\n" +
                "-v:version (Example: -v:1,-v:2c, -v:3)\r\n" +
                "-r:IP (Example: -r:192.168.1.0)\r\n" +
                "-p:port (Optional, default 161, example: -p:555\r\n" +
                "-o:OID (Example: -o:1.3.6.1.2.1.1.2.0" +
                "-c:community_string (Example: -c:public)\r\n" +
                "-ap:AuthenticationDigest SHA/MD5(Example: -ap:SHA)\r\n" +
                "-aw:AuthenticationPassword\r\n" +
                "-pp:PrivacyProtocol DES/AES128/AES192/AES256/TripleDES (Example: -pp:AES128)\r\n" +
                "-pp:PrivacyPassword\r\n" +
                "-sn:SecurityName");
                return;
            }
            else
            {
                foreach (string arg in args)
                {
                    if (arg.IndexOf("-v:") == 0)
                    {
                        if (arg.Substring(3) == "1")
                            version = 1;
                        if (arg.Substring(3) == "2c")
                            version = 2;
                        if (arg.Substring(3) == "3")
                            version = 3;
                    }
                    if (arg.IndexOf("-o:") == 0)
                    {
                        oid = arg.Substring(3);
                    }
                    if (arg.IndexOf("-r:") == 0)
                    {
                        ip = arg.Substring(3);
                    }
                    if (arg.IndexOf("-sn:") == 0)
                    {
                        securityuser = arg.Substring(4);
                    }
                    if (arg.IndexOf("-c:") == 0)
                    {
                        community = arg.Substring(3);
                    }
                    if (arg.IndexOf("-p:") == 0)
                    {
                        string ports = arg.Substring(3);
                        try
                        {
                            port = Int32.Parse(ports);
                        }
                        catch (Exception ex)
                        {
                            port = -1;
                            Console.WriteLine("Not correct port value: " + ports);
                            break;
                        }
                    }
                    if (arg.IndexOf("-ap:") == 0)
                    {
                        authenticationdigest = arg.Substring(4);
                        if (String.Compare(authenticationdigest, "SHA", true) == 0)
                            authDigest = AuthenticationDigests.SHA1;
                        if (String.Compare(authenticationdigest, "MD5", true) == 0)
                            authDigest = AuthenticationDigests.MD5;
                    }
                    if (arg.IndexOf("-aw:") == 0)
                    {
                        authenticationpassword = arg.Substring(4);
                    }
                    if (arg.IndexOf("-pp:") == 0)
                    {
                        privacyprotocolstring = arg.Substring(4);
                        if (String.Compare(privacyprotocolstring, "DES", true) == 0)
                            privacyprotocol = PrivacyProtocols.DES;
                        if (String.Compare(privacyprotocolstring, "AES128", true) == 0)
                            privacyprotocol = PrivacyProtocols.AES128;
                        if (String.Compare(privacyprotocolstring, "AES192", true) == 0)
                            privacyprotocol = PrivacyProtocols.AES192;
                        if (String.Compare(privacyprotocolstring, "AES256", true) == 0)
                            privacyprotocol = PrivacyProtocols.AES256;
                        if (String.Compare(privacyprotocolstring, "TripleDES", true) == 0)
                            privacyprotocol = PrivacyProtocols.TripleDES;
                    }
                    if (arg.IndexOf("-pw:") == 0)
                    {
                        privacyprotocolpassword = arg.Substring(4);
                    }
                }
            }
            if (port == -1)
            {
                Console.WriteLine("Not valid port: " + port.ToString());
                return;
            }
            if ((version < 1) || (version > 3))
            {
                Console.WriteLine("Not valid version: ");
                return;
            }
            */


        }
        public static NLog.Logger InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();
            string folder = "D:\\Amir\\";
            var x = DateTime.Now.ToString().Replace('_', ':');
            var y = x.Replace('/', '.');
            string filename = "SNMP - " + y + ".txt";

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = folder + filename };

            // Rules for mapping loggers to targets            
            if (debug)
            {
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
            }
            else config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);

            //NLog.LogManager.Setup().SetupInternalLogger(s =>
            //s.SetMinimumLogLevel(LogLevel.Trace).LogToFile(@"D:\Amir\Hello.txt"));
            // Apply config           
            NLog.LogManager.Configuration = config;
            return NLog.LogManager.GetCurrentClassLogger();

        }
    }
}


