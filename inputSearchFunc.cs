using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ConsoleApp1
{
    class Program
    {
        public static NLog.Logger logger = InitLogger(true);
        static void Main(string[] args)
        {
            string path = "D:\\sources.txt";
            StreamReader sr = new StreamReader(path);
            string[] lines = sr.ReadToEnd().Split(new string[]{ "\r\n" },StringSplitOptions.None);
            List<string[]> compnents = new List<string[]>();
            List<string> fullInputNames = new List<string>();
            List<string> nickNames = new List<string>();
            Dictionary<string, string> fullnameToNickname = new Dictionary<string, string>();
            foreach (var item in lines)
            {
                var array = item.Split('\t');
                
                if (!array[1].Equals("Free"))
                {
                    compnents.Add(array);
                    nickNames.Add(array[1]);
                    fullInputNames.Add(array[2]);
                    fullnameToNickname.Add(array[2], array[1]);
                }
                
            }

            string sattelite = "Eutelsat 7B (7 E)";
            List<string> localosilators = new List<string>( new string[]{ "10600", "9750", "10750", "10000", "5150" });
            string polarity = "Vertical";

            var result = inputSearch(sattelite,polarity, localosilators, fullInputNames);
            if (!result.Equals(""))
            {
                Console.WriteLine("found and RF input called: " + fullnameToNickname[result]);
            }
        }

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

        public static string inputSearch(string satellite, string polarity, List<string> localosilators, List<string> inputs)
        {
            string inputFound = "";

            satellite = satellite.Trim();
            var originalSattelite = satellite;

            int indexOfParentasis = satellite.LastIndexOf('(');
            var position = satellite.Substring(indexOfParentasis);
            satellite = satellite.Substring(0, indexOfParentasis - 1);

            bool satteliteNameMatch = true;
            bool foundInput = false;
            for (int i = 0; i < inputs.Count && !foundInput; i++)
            {
                if (i == 70)
                {
                    var x = "stop here";
                }

                var values = inputs[i].Split(new string[] { " | " }, StringSplitOptions.None);
                if (values.Length >= 3)
                {
                    var inputListSattelite = values[0].Trim();
                    var inputListOsscilator = values[2];
                    var inputListPolarity = values[1];
                    string[] listOfSatteliteStrings = null;

                    if (inputListSattelite.Contains(" / "))
                    {
                        listOfSatteliteStrings = inputListSattelite.Split(new string[] { " / " }, StringSplitOptions.None);
                    }
                    else listOfSatteliteStrings = new string[] { inputListSattelite };

                    foreach (var inputSatteliteString in listOfSatteliteStrings)
                    {
                        if (inputSatteliteString.Contains("("))
                        {
                            indexOfParentasis = inputSatteliteString.LastIndexOf('(');
                            var inputListposition = inputSatteliteString.Substring(indexOfParentasis);
                            var sattelitePart = inputSatteliteString.Substring(0, indexOfParentasis - 1);


                            if (!inputSatteliteString.Contains("&"))
                            {
                                satteliteNameMatch = satellite.Equals(sattelitePart);
                            }
                            else
                            {
                                if (position.Equals(inputListposition))
                                {
                                    List<string> satelliteArray = new List<string>();
                                    satelliteArray.AddRange(inputSatteliteString.Split('&'));
                                    satelliteArray[satelliteArray.Count - 1] = satelliteArray[satelliteArray.Count - 1].Split(new string[] { " (" }, StringSplitOptions.None)[0];
                                    var satelliteSplit = satelliteArray[0].Split(' ');
                                    var length = satelliteArray[0].Length - satelliteSplit[satelliteSplit.Length - 1].Length;
                                    var satteliteInitial = satelliteArray[0].Substring(0, length);

                                    satteliteNameMatch = satellite.Equals(satelliteArray[0]);

                                    for (int j = 1; j < satelliteArray.Count; j++)
                                    {
                                        bool sec = satellite.Equals(satteliteInitial + satelliteArray[j]);
                                        satteliteNameMatch = satteliteNameMatch || sec;
                                        
                                    }
                                }
                            }
                            if (localosilators.Contains(inputListOsscilator) && polarity.Equals(inputListPolarity) && satteliteNameMatch)
                            {
                                return inputs[i];
                            }
                        }
                        else
                        {
                            // in the future, if we know the left and right limit of a mobile dish we would be able to extend the code.
                        }
                    }
                    
                }
            }

            return inputFound;
        }
       /* public static string inputSearch2(string satellite, string polarity, List<string> localosilators, List<string> inputs)
        {
            string scriptParamSatellite = satellite.Trim(); ;
            string scriptParamPolarity = polarity.Trim(); ;
            // formatted in the following way: OSC-1 | OSC-2 |.....| OSC-N
            string scriptParamLocalosilator = localosilators;

            logger.Debug("In input search Function");
          
            logger.Debug("Satellite Parameter: " + scriptParamSatellite);
            logger.Debug("Polarity Parameter: " + scriptParamPolarity);
            logger.Debug("Osscilators string: " + scriptParamLocalosilator);

            string[] delimiter = new string[1] { " | " };
            List<string> OsscilatorSplit = new List<string>(scriptParamLocalosilator.Split(delimiter, System.StringSplitOptions.None));
            for (int i = 0; i < OsscilatorSplit.Count; i++)
            {
                logger.Debug("osl " + i.ToString() + ": " + OsscilatorSplit[i]);
            }

            List<string> matrixInputSatelliteList = new List<string>();
            List<string> inputSplitResult = new List<string>();
            List<string> satelliteNameSplit = new List<string>();
            string inputSearchResult = "";
            int indexOfLastLeftParentasis = 0;
            int indexOfLastLeftSpace = 0;
            string matrixInputPosition = "";
            string matrixInputSatelliteName = "";
            string matrixInputPolarity = "";
            string matrixInputOsscilator = "";
            string satelliteName = "";
            bool found = false;
            bool satteliteNameMatch = true;
            for (int i = 1; i <= inputs.Count && !found; i++)
            {
                logger.Debug("index: " + i + "\n");
                found = false;
                satteliteNameMatch = true;
                matrixInputSatelliteList = new List<string>();

                inputSearchResult = inputs[i];
                logger.Debug("Current Index RF: " + inputSearchResult);
                inputSplitResult = new List<string>(inputSearchResult.Split(delimiter, System.StringSplitOptions.None));

                satteliteNameMatch = (inputSplitResult.Count == 3);
                if (satteliteNameMatch)
                {
                    matrixInputSatelliteName = inputSplitResult[0].Trim();
                    logger.Debug("Satellite Name Result: " + matrixInputSatelliteName);
                    satteliteNameMatch = (matrixInputSatelliteName.Equals(scriptParamSatellite));
                }
                if (satteliteNameMatch)
                {
                    matrixInputPolarity = inputSplitResult[1].Trim();
                    logger.Debug("Polarity Result: " + matrixInputPolarity);
                    logger.Debug("Polarity Param: " + scriptParamPolarity);
                    satteliteNameMatch = (matrixInputPolarity.Equals(scriptParamPolarity));
                }
                if (satteliteNameMatch)
                {
                    matrixInputOsscilator = inputSplitResult[2].Trim();
                    logger.Debug("Osscilator Result: " + matrixInputOsscilator);
                    satteliteNameMatch = (OsscilatorSplit.Contains(matrixInputOsscilator));
                }
                if (satteliteNameMatch)
                {
                    matrixInputPosition = matrixInputSatelliteName.Substring(indexOfLastLeftParentasis);
                    logger.Debug("matrix Input Position: " + matrixInputPosition.ToString());
                    if (matrixInputSatelliteName.Contains("&"))
                    {
                        logger.Debug("Contains &");
                        satelliteNameSplit = new List<string>(matrixInputSatelliteName.Split('&'));
                        satelliteName = satelliteNameSplit[0];

                        indexOfLastLeftSpace = satelliteName.LastIndexOf(' ');
                        if (indexOfLastLeftSpace > 0)
                        {
                            satelliteNameSplit[0] = satelliteName.Substring(indexOfLastLeftSpace + 1);
                            satelliteName = satelliteName.Substring(0, indexOfLastLeftSpace - 1);
                            for (int j = 0; j < matrixInputSatelliteList.Count; j++)
                            {
                                matrixInputSatelliteList.Add(satelliteName + satelliteNameSplit[j]);
                            }
                            if (matrixInputSatelliteList.Contains(scriptParamSatellite))
                            {
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    return inputs[i];
                }
            }

            logger.Debug("End Script!!!");
            return "";

        }
       */
        public static string[] OpenFile(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);
            return sr.ReadToEnd().Split('\n');
        }
    }
}
