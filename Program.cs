using System;
using InsightClientLibrary;
using Excel = Microsoft.Office.Interop.Excel;
using NLog;
using System.Collections.Generic;

namespace DMSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.LogManager.ThrowConfigExceptions = true;
            string pop = "";
            bool debug = true;
            NLog.Logger logger = InitLogger(debug);
            logger.Debug("Simuldation started at: {0}", DateTime.Now);
            ServiceLocker serviceLocker = null;
            
            int retry = 3;
            while (retry > 0)
            {
                try
                {
                    serviceLocker = new ServiceLocker(debug, pop);
                    if (serviceLocker.ErrorException != null)
                    {
                        if (serviceLocker.ErrorException.GetType().Equals(typeof(InsightUserAthenticationException)))
                        {
                            Console.WriteLine(serviceLocker.ErrorException.Message + serviceLocker.ErrorException.StackTrace);
                        }
                    }
                }
                catch (InsightUserAthenticationException e)
                {
                    if (retry > 1)
                    {
                        Engine.GenerateInformation(e+ ", retrying");
                    }
                    else if (retry == 1)
                    {
                        Engine.GenerateInformation(e + ", please enter your password");
                    }
                }
                --retry;
            }

            try
            {
                // here need to ask user to add crdentials
                retry = 3;
                string username = "amir.tal";
                string password = "At123456!";
                while (retry > 0)
                {
                    try
                    {
                        serviceLocker = new ServiceLocker(debug, pop, username, password);
                        break;
                    }
                    catch (InsightUserAthenticationException e)
                    {
                        if (retry > 1)
                        {
                            Engine.GenerateInformation(e + ", kindly try to re enter your password");
                        }
                        else if (retry == 1)
                        {
                            Engine.GenerateInformation(e + ", kindly reach out to your IT admin");
                            return;
                        }
                        --retry;
                    }
                }

                if (!serviceLocker.ClientAuthenticated)
                {
                    Engine.GenerateInformation("Your user is no longer authenticated");
                    return;
                }
                string IRDManagment = "172.19.54.29";
                string IRDModel = "Cisco D9854";
                string IRDName = "NOC IRD 3 Cisco";
                /*
                var sport = serviceLocker.Client.GetServiceGraph("SPORT2HDP1-EHA-CELL");
                Console.WriteLine(sport.PrintGraph());

                var xa = serviceLocker.Client.GetServiceGraph("ACCNETWORKEASTHD-HWL-VUAVCD");
                Console.WriteLine(xa.PrintGraph());
               
                var xb = serviceLocker.GetSourcesLockElement("#AB1-SES-5-4.971","","","");
                Console.WriteLine(serviceLocker.LastSerachedUuidGraph.PrintGraph());
                Console.WriteLine(serviceLocker.Client.VerifyValidRoute(serviceLocker.LastSerachedUuidGraph));
                 */
                string[] uuidList = Tools.ReadNameFromExcelFile("export.xlsx", 2, 2).Split('\n');
                int i = 0;
                foreach (var uuid in uuidList)
                {
                    try
                    {
                        ++i;
                        Engine.GenerateInformation("current index: " + i);
                        logger.Debug("Index: " + i);
                        var x = serviceLocker.GetSourcesLockElement(uuid, IRDManagment, IRDName, IRDModel);
                        var y = serviceLocker.LastSerachedUuidGraph;
                        if (debug)
                        {
                            bool validRoute = serviceLocker.Client.VerifyValidRoute(y);
                            if (!validRoute)
                            {
                                logger.Fatal("The program created a wrong route for the uuid: {0}", uuid);
                                throw new Exception("The program created a wrong route for the uuid: " + uuid);
                            }
                        }
                        
                        if (x == null)
                        {
                            logger.Error("There was an error with the program, please try again\nthe programmer was notified");
                            // add email to self
                        }
                        foreach (var item in x.Values)
                        {
                            foreach (var item2 in item)
                            {
                                logger.Debug(item2.ToString());
                            }
                        }
                    }
                    catch (IllegalNameException e)
                    {
                        logger.Error(e.Message);
                        Engine.GenerateInformation(e.Message);
                    }
                    catch (InsighClientLibraryUnknownErrorException e)
                    {
                        logger.Fatal(e.Message);
                        Engine.GenerateInformation(e.Message);
                    }
                    catch (CorruptedInsightDataException e)
                    {
                        logger.Error(e.Message);
                        Engine.GenerateInformation(e.Message);
                    }
                    catch (RestSharpException e)
                    {
                        logger.Error(e.Message);
                        Engine.GenerateInformation(e.Message); ;
                    }
                    catch (UnsuccessfullResponseException e)
                    {
                        logger.Error(e.Message);
                        Engine.GenerateInformation(e.Message);
                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e.Message);
                        logger.Fatal(e.StackTrace);
                        Engine.GenerateInformation("Program failed due to an unknown error, the programmer was informed");
                        // add mail
                    }
                }
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
                Engine.GenerateInformation("Program failed due to an unknown error, the programmer was informed");
                // add mail
            }
        }
        /// <summary>
        /// Creates a logger for a class
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static NLog.Logger InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"D:\Amir\Log.txt" };

            // Rules for mapping loggers to targets            
            if (debug)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            }
            else config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            //NLog.LogManager.Setup().SetupInternalLogger(s =>
                            //s.SetMinimumLogLevel(LogLevel.Trace).LogToFile(@"D:\Amir\Hello.txt"));
            // Apply config           
            NLog.LogManager.Configuration = config;
            return NLog.LogManager.GetCurrentClassLogger();
            
        }
    }

    class Engine
    {
        public Engine()
        {
        }

        public static void GenerateInformation(string toPrint)
        {
            Console.WriteLine(toPrint);
        }
    }
}
