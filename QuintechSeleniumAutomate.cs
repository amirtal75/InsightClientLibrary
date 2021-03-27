
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium;
using NLog;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using System.Diagnostics;

namespace QuintechSeleniumAutomate
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static async void Main(string[] args)
        {
            string testArgs = "*NOC* RF Room Out 27 (^*^) NOC IRD 4;NOC Physical Spec;NOC SED Spec";
            Dictionary<int, int> x = new Dictionary<int, int>();
            int[] asd = new int[128];
            x.Keys.CopyTo(asd, 0);
            for (int i = 0; i < asd.Length; i++)
            {
                var t = asd[i];
            }
            InitLogger(true);
            var startTime = DateTime.Now;
            logger.Debug("Starting log for the Quinteck lock script at: " + DateTime.Now);
            logger.Debug("Number of arguments received: " + args.Length);

            string[] arguments;
            string[] seperator = { " (^*^) " };
            if (args.Length > 0)
            {
                logger.Debug("arggument received: " + args.Length);
                logger.Debug("args[0] = " + args[0]);
                arguments = args[0].Split(seperator, StringSplitOptions.None);
            }
            else
            {
                logger.Debug("No argument received, using default: " + testArgs);
                arguments = testArgs.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            }

            string inputName = arguments[0];
            logger.Debug("Chosen matrix input: " + inputName);

            string[] outputarray = arguments[1].Split(';');
            List<string> outputNames = new List<string>(outputarray);
            foreach (var item in outputNames)
            {
                logger.Debug("Chosen matrix output: " + item);

            }
            try
            {
                using (var driver = new ChromeDriver())
                {
                    driver.Navigate().GoToUrl("https://172.19.19.30/?GetScreen=login");
                    SecurityBypass(driver);
                    Login(driver);
                    driver.Navigate().GoToUrl("https://172.19.19.30/?GetScreen=crosspoints");
                    
                    int tries = 10;
                    Thread.Sleep(1000);
                    while (BugCheck(driver))
                    {
                        Login(driver);
                        driver.Navigate().GoToUrl("https://172.19.19.30/?GetScreen=crosspoints");
                        if (tries == 0)
                        {
                            throw new Exception("Maximum bug fix retries was reached");
                        }
                        tries--;
                    }
                    logger.Debug("input url found " + driver.Url);
                    PressOnInput(driver,inputName);
                    PressOnOutputs(driver, outputNames);
                    SaveCrossPoints(driver);

                    logger.Debug("Run duration: " + (DateTime.Now - startTime));
                }
            }
            catch (WebDriverException e)
            {
                logger.Debug(e.Message);
                logger.Debug(e.StackTrace);
                throw e;
            }
            catch (Exception e)
            {
                logger.Debug(e.Message);
                logger.Debug(e.StackTrace);
                throw e;
            }
        }
        public static void SaveCrossPoints(IWebDriver driver)
        {
            string elementName = "SaveCrossPoints Function:";
            logger.Debug(elementName);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            logger.Debug("Saving crosspoints");
            wait.Until(ExpectedConditions.ElementExists(By.LinkText("Save"))).Click();
        }
        public static void PressOnOutputs(IWebDriver driver, List<string> outputNames)
        {
            string elementName = "PressOnOutput";
            string outputName = "";
            string outputXPath = "";
            string parentXPath = "";
            string xPath_outputNumber = "";
            string outputNumber = "";
            bool found = false;
            int attemptNumber = 0;

            for (int i = 0; i < outputNames.Count; i++)
            {
                outputName = outputNames[i];
                outputXPath = "//td[contains(.,'" + outputName + "')]";
                parentXPath = outputXPath + "/parent::node()";
                xPath_outputNumber = parentXPath + "/*[2]";

                found = false;
                attemptNumber = 0;
                logger.Debug("Bug check from output press");
                BugCheck(driver);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                var outputElement = wait.Until(ExpectedConditions.ElementExists(By.XPath(outputXPath)));

                var parentElement = driver.FindElement(By.XPath(parentXPath));

                string className = parentElement.GetAttribute("class");
                logger.Debug(outputName + " className: " + className);
                
                bool isChecked = className.Equals("checkable checked");
                logger.Debug("Element {0} select status: " + isChecked, outputName);
                if (!isChecked)
                {
                    outputElement.Click();
                    Thread.Sleep(100);
                    isChecked = parentElement.GetAttribute("class").Equals("checkable checked");
                    logger.Debug(outputName + " after click state is: " + isChecked);
                }
            }
        }
        public static int PressOnInput(IWebDriver driver, string inputName)
        {
            string elementName = "PressOnInput Function";
            logger.Debug(elementName);

            string inputXPath = "//td[contains(.,'" + inputName + "')]";
            string parentXPath = inputXPath + "/parent::node()";
            string xPath_inputNumber = parentXPath + "/*[1]";
            string inputNumber = "";

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementExists(By.XPath(inputXPath))).Click();
            logger.Debug("After click");
            inputNumber = driver.FindElement(By.XPath(xPath_inputNumber)).GetAttribute("innerHTML");

            logger.Debug("Wait for output page to load by waiting on the last output in the output table");
            try
            {
                logger.Debug("Trying wait with 4 seconds");
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
                wait.Until(ExpectedConditions.ElementExists(By.Id("Output127")));
            }
            catch (Exception e)
            {
                logger.Debug("Bug check from input press");
                BugCheck(driver);
            }
            logger.Debug("returning from input press");
            return Int32.Parse(inputNumber);
        }
        public static bool BugCheck(IWebDriver driver)
        {
            string elementName = "BugCheck Function";
            logger.Debug(elementName);
            Thread.Sleep(3000);
            var x = "";
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                logger.Debug("Searching login element");
                x = wait.Until(ExpectedConditions.ElementExists(By.XPath("//input[@name='login']"))).TagName;
                logger.Debug("login element found - bug occrured");
                return true;
            }
            catch (WebDriverException e)
            {
                logger.Debug("Bug not occured ,{0} found ",x);
                logger.Debug("Bug not occured ,{0} found ", driver.Url);
                return false;
            }

        }
        public static void Login(IWebDriver driver)
        {
            string elementName = "Login Function";
            logger.Debug(elementName);
            SendUser(driver);
            SendPassword(driver);

            logger.Debug("Clicking login");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));            
            wait.Until(ExpectedConditions.ElementExists(By.XPath("//input[@name='login']"))).Click();

            try
            {
                logger.Debug("Waiting for page to load by waiting on crosspoints element dor 3 seconds");
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.ElementExists(By.LinkText("Crosspoints")));
            }
            catch (WebDriverException e)
            {
                logger.Debug("Page did not load, waiting for bug check");
            }

        }
        public static void SendPassword(IWebDriver driver)
        {
            string elementName = "SendPassword Function:";
            logger.Debug(elementName);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            logger.Debug("Sending password keys");
            wait.Until(ExpectedConditions.ElementExists(By.Id("password"))).SendKeys("mcr");
        }
        public static void SendUser(IWebDriver driver)
        {
            string elementName = "SendUser Function:";
            logger.Debug(elementName);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            logger.Debug("Sending user keys");
            wait.Until(ExpectedConditions.ElementExists(By.Id("username"))).SendKeys("mcr");
        }
        public static void SecurityBypass(IWebDriver driver)
        {
            string elementName = "SecurityBypass Function";
            logger.Debug(elementName);
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                logger.Debug("Clicking advanced button");
                wait.Until(ExpectedConditions.ElementExists(By.Id("details-button"))).Click();
                logger.Debug("Clicking proceed button");
                wait.Until(ExpectedConditions.ElementExists(By.Id("proceed-link"))).Click();
                logger.Debug("Ovverided Security Page");
            }
            catch (WebDriverException)
            {
                logger.Debug("No security screen encountered");
            }
        }
        public static NLog.Logger InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();
            string folder = "D:\\Amir\\";
            var x = DateTime.Now.ToString().Replace('_', ':');
            var y = x.Replace('/', '.');
            string filename = "Quintech Selenium Log - " + y + ".txt";

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
