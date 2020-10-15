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
using System.Diagnostics;

namespace SlemiumTest2
{
    public class Class1
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static IWebDriver driver = null;
        static int maxRetryAttempts = 100;

        static void Main(string[] args)
        {
            string inputName = "*NOC* RF Room Out 27";
            List<string> outputNames = new List<string>();
            outputNames.Add("NOC IRD 4");
            outputNames.Add("NOC IRD 3");
            outputNames.Add("NOC IRD 2");
            outputNames.Add("NOC IRD 1");
            InitLogger(true);

            if (driver == null)
            {
                try
                {
                    driver = new ChromeDriver();

                    // Step 1 - Navigate to Quintech matrix
                    driver.Navigate().GoToUrl("https://172.19.19.30/?GetScreen=login");
                    logger.Debug(" Navigated to Quintech");

                    // Step 2 - Check for security screen
                    SecurityBypass(driver);

                    // Step 3 - Quintech has a bug that when you login and press on the crosspoints page button, it logs you out of the system.
                    // In this step we will try to perform the login 10 times before giving an exception
                    Login(driver);
                    GoToCrosspointsListPage(driver);

                    logger.Debug("Bug loop");
                    int times = 0;
                    bool bugFound = true;
                    while (times < 10 && bugFound)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            var loginElem = driver.FindElement(By.XPath("//input[@name='login']"));
                            times++;
                            logger.Debug("Bug loop2 time: " + times);
                            Login(driver);
                        }
                        catch (Exception)
                        {
                            bugFound = false;
                        }
                    }

                    // Step 4 -  Select Deired Input
                    PressOnInput(driver, inputName);

                    // Step 5 - Select desired outputs
                    PressOnOutputs(driver, outputNames);

                    // Step 6 - Save the cross points
                    SaveCrossPoints(driver);
                }
                catch (OpenQA.Selenium.DriverServiceNotFoundException notFound)
                {
                    Console.WriteLine(notFound);
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void PressOnInput(IWebDriver driver, string inputName)
        {
            string inputXPath = "//td[contains(.,'" + inputName + "')]";
            string parentXPath = inputXPath + "/parent::node()";
            string elementName = "PressOnInput";
            string xPath_inputNumber = parentXPath + "/*[1]";
            string inputNumber = "";
            string nameOfEditPage = "";
            int inNumber = 0;

            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = inputName;
                    Thread.Sleep(1000);
                    if (driver.Url.Equals("https://172.19.19.30/?GetScreen=crosspoints"))
                    {
                        var inputElem = driver.FindElement(By.XPath(inputXPath));
                        inputElem.Click();
                        inputNumber = driver.FindElement(By.XPath(xPath_inputNumber)).GetAttribute("innerHTML");
                        inNumber = Int32.Parse(inputNumber);
                        inNumber--;
                        nameOfEditPage = "https://172.19.19.30/?GetScreen=editinput&input=" + inNumber;

                        logger.Debug("input check loop");
                        logger.Debug("edit page name: " + nameOfEditPage);
                        logger.Debug("input x path: " + inputXPath);
                        
                        logger.Debug("");
                        logger.Debug("");
                        logger.Debug("");
                        logger.Debug("");
                        int attemp = 0;
                        while (!driver.Url.Equals(nameOfEditPage))
                        {

                            attemp++;
                            logger.Debug("attemp number: " + attemp);
                            Thread.Sleep(500);
                            if (attemp == 10)
                            {
                                logger.Debug("attempt 10 exception");
                                throw new Exception("Cannot enter matrix after pressing input and waiting for 5 seconds");
                            }
                        }
                        logger.Debug("input number: {0}, named {1} was pressed", inputNumber, inputName);
                        found = true;
                    }      
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }

            found = false;
            attemptNumber = 0;
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
                while (!found)
                {
                    try
                    {
                        elementName = outputName;
                        var outputElem = driver.FindElement(By.XPath(outputXPath));
                        outputNumber = driver.FindElement(By.XPath(xPath_outputNumber)).GetAttribute("innerHTML");
                        int outNumber = Int32.Parse(outputNumber);
                        logger.Debug("selecting output: " + outputName);
                        
                        logger.Debug(outputName + " is not selected for input 27");
                        string className = driver.FindElement(By.XPath(parentXPath)).GetAttribute("class");
                        logger.Debug(outputName + " className: " + className);
                        bool isChecked = className.Equals("checkable checked");
                        if (!isChecked)
                        {
                            logger.Debug(outputName + " is not selected for input 27");
                            outputElem.Click();
                            logger.Debug(outputName + " after click state is: " + isChecked);
                        }
                        else
                        {
                            logger.Debug(outputName + " is already selected");
                        }
                        found = true;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException e)
                    {
                        attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e.StackTrace);
                        logger.Fatal(e.Message);
                    }
                }
            }
        }

        public static void SaveCrossPoints(IWebDriver driver)
        {
            string elementName = "SaveCrossPoints";

            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = "saveElem";
                    var saveElem = driver.FindElement(By.LinkText("Save"));
                    saveElem.Click();
                    int attemp = 0;
                    while (!driver.Url.Equals("https://172.19.19.30/?GetScreen=crosspoints"))
                    {
                        attemp++;
                        Thread.Sleep(500);
                        if (attemp == 10)
                        {
                            throw new Exception("Cannot enter matrix after pressing login and waiting for 5 seconds");
                        }
                    }
                    logger.Debug("saveElem completed");
                    found = true;
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }

            found = false;
            attemptNumber = 0;
        }
        public static void GoToCrosspointsListPage(IWebDriver driver)
        {
            string elementName = "GoToCrosspointsListPage";

            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = "crosspointListElem";
                    var crosspointListElem = driver.FindElement(By.LinkText("Crosspoints"));
                    crosspointListElem.Click();
                    logger.Debug("Crosspoints completed");
                    found = true;
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }

            found = false;
            attemptNumber = 0;
        }
        public static void SendUser(IWebDriver driver, string elementName)
        {
            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = "usernameElem";
                    var usernameElem = driver.FindElement(By.Id("username"));
                    usernameElem.SendKeys("mcr");
                    logger.Debug("typed the username");
                    found = true;
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);

                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }
        }
        public static void SendPassword(IWebDriver driver, string elementName)
        {
            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = "passwordElem";
                    var passwordElem = driver.FindElement(By.Id("password"));
                    passwordElem.SendKeys("mcr");
                    logger.Debug("typed the password");
                    found = true;
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }
        }
        public static void Login(IWebDriver driver)
        {
            string elementName = "Login Function";

            
            SendUser(driver, "usernameElem");
            SendPassword(driver, "passwordElem");

            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    elementName = "loginElem";
                    logger.Debug("Sleeping 1000");
                    Thread.Sleep(1000);
                    logger.Debug("Befor Find");
                    var loginElem = driver.FindElement(By.XPath("//input[@name='login']"));
                    logger.Debug("Befor click");
                    loginElem.Click();
                    logger.Debug("Sleeping 3000");
                    Thread.Sleep(3000);
                    logger.Debug("Befor attempt login loop");
                    int attemp = 0;
                    while (!driver.Url.Equals("https://172.19.19.30/?GetScreen=summary"))
                    {
                        attemp++;
                        logger.Debug("Befor attempt login,attempt: " + attemp);
                        SendUser(driver, "usernameElem");
                        SendPassword(driver, "passwordElem");
                        loginElem.Click();
                        Thread.Sleep(3000);
                        if (attemp == 10)
                        {
                            throw new Exception("Cannot enter matrix after pressing login and waiting for 5 seconds");
                        }
                    }
                    logger.Debug("login completed");
                    found = true;
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }
            logger.Debug("Sleeping 5000");
            Thread.Sleep(6000);
            found = false;
            attemptNumber = 0;
        }
        public static void SecurityBypass(IWebDriver driver)
        {
            string elementName = "SecurityBypass Function";

            bool found = false;
            int attemptNumber = 0;
            while (!found)
            {
                try
                {
                    var adavancedButtonElem = driver.FindElement(By.Id("details-button"));
                    elementName = "adavancedButtonElem";
                    adavancedButtonElem.Click();
                    logger.Debug("Encountered security screen... Activating Bypass");
                    found = true;
                    logger.Debug("pressed advanced");
                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    logger.Debug("No security screen encountered");
                    return;
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }

            found = false;
            attemptNumber = 0;
            while (!found)
            {
                try
                {
                    Thread.Sleep(500);
                    var proceedLinkElem = driver.FindElement(By.Id("proceed-link"));
                    elementName = "proceedLinkElem";
                    proceedLinkElem.Click();
                    found = true;
                    logger.Debug("Ovverided Security Page");

                }
                catch (OpenQA.Selenium.NoSuchElementException e)
                {
                    attemptNumber = logCatchElementNotFound(attemptNumber, e, elementName);
                }
                catch (Exception e)
                {
                    logger.Fatal(e.StackTrace);
                    logger.Fatal(e.Message);
                }
            }
        }
        public static NLog.Logger InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"D:\Amir\SeleniumLog.txt" };

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
        public static int logCatchElementNotFound(int attemptNumber, OpenQA.Selenium.NoSuchElementException e, string elementName)
        {
            // sleep to let the page finish loading
            Thread.Sleep(100);
            logger.Debug("HTML Element " + elementName + " not found, attempt number: " + attemptNumber);
            logger.Error(e.Message);
            logger.Error(e.StackTrace);

            // check if max retries were made
            if (attemptNumber > maxRetryAttempts)
            {
                throw new OpenQA.Selenium.NoSuchElementException("Max retry was reached!!!");
            }

            return ++attemptNumber;
        }
    }
}