using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using NLog;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;


namespace InsightClientLibrary
{
        #pragma warning disable CS1591
    public class InsightClient
        #pragma warning restore CS1591
    {
        /// <summary>
        /// This class contain methods meant to perform requests from and to Insight.
        /// In case the insight version will be updated, the following must adjusted accordingly:
        ///     1. The function GetInsightObjectTypeList, uses the insight API query: https://jira.mx1.com/rest/insight/1.0/objectschema/3/objecttypes/flat
        ///     2. The function 
        /// </summary>
        private bool debug =  false;
        private NLog.Logger logger;
        /// <value> This value will prevent the code from working with equipments of other departments </value>
        public string POPName;
        /// <value>this value contains all object groups in the schema</value>
        private Dictionary<string, List<ObjectType>> ObjectGroups;
        private RestClient InsightRestClient;
        /// <value> This value describes symbols that Insight API considers illegal in a request </value>
        public string forbiddenInsightApiQuerySymbols = "<>#+,&";
        /// <summary>
        /// This is the Insight API API server adress.
        /// Should the insight command structure change, you need to modify this field for continuing support in future versions.
        /// </summary>
        public readonly string InsightApiServerAdress = "https://jira.mx1.com/rest/insight/1.0/";
        /// <summary>
        /// This is the Insight API command for getting the object types in a schema.
        /// Should the insight command structure change, you need to modify this field for continuing support in future versions.
        /// </summary>
        public readonly string schemaResource = "objectschema/3/objecttypes/flat";
        /// <summary>
        /// This is the Insight API command for getting an IQL response.
        /// Should the insight command structure change, you need to modify this field for continuing support in future versions.
        /// </summary>
        public readonly string iqlResource = "iql/objects?objectSchemaId=3&iql=";
        /// <value> This value indicates if an authentication error occured while creating an insight client </value>
        public string AuthenticationTest = "";
        /// <summary>
        /// Constructs a client to communicate with insight
        /// </summary>
        /// <param name="_debug"> will run the program with debug messages accordingly</param>
        /// <param name="PopName"> the name of the pop that creates this client</param>
        public InsightClient(bool _debug, string PopName)
        {
            POPName = PopName;
            debug = _debug;
            logger = NLog.LogManager.GetCurrentClassLogger();
            InsightRestClient = GetDefaultClient();
            CreateSchemaGraph(3);
        }
        /// <summary>
        /// Constructs a client to communicate with insight
        /// </summary>
        /// Constructs a client to communicate with insight
        /// <param name="debug"> will run the program with debug messages accordingly</param>
        /// <param name="username"> the username for insight login</param>
        /// <param name="password"> the password for insight login</param>
        /// <param name="PopName"> the name of the pop that creates this client</param>
        public InsightClient(bool debug,string PopName, string username, string password)
        {
            InsightRestClient = GetClient(username, password);
            logger = NLog.LogManager.GetCurrentClassLogger();

            if (InsightRestClient != null)
            {
                try
                {
                    RestRequest request = new RestRequest("objectschema/3/objecttypes/flat",Method.GET);
                    var response = InsightRestClient.Execute(request);
                    if (response.ErrorException != null)
                    {
                        logger.Error("Client failed Insight validity test, due to the following exception:\n" + response.ErrorException.Message);
                        logger.Error("Stack Trace:\n" + response.ErrorException.StackTrace);
                        throw new Exception();
                    }
                    else if (!response.IsSuccessful)
                    {
                        logger.Error("Client failed Insight validity test, due to the following details:\n" + response.Content + "\nStatus code:" + response.StatusCode);
                        throw new Exception();
                    }
                    else
                    {
                        POPName = PopName;
                        CreateSchemaGraph(3);
                    }

                }
                catch (Exception e)
                {
                    logger.Error("Client failed Insight validity test, due to a unknown issue");
                    throw new Exception(e.Message);
                }
            }
            else
            {
                string message = "The supplied rest client was null, please consider using the minimized constructor which uses the default client";
                logger.Error(message);
                throw new Exception(message);
            }
        }
        /// <summary>
        /// Creates a logger for a class
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        public NLog.Logger InitLogger(bool debug)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"D:\Amir\Log.txt" };

            // Rules for mapping loggers to targets            
            if (debug)
            {
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            }
            else config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);


            // Apply config           
            NLog.LogManager.Configuration = config;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            return logger;
        }
        /// <summary>
        /// Goes over all the elements in the graph, for each element check the incoming elements in Insight database and in the built graph route.
        /// If every grapgh element has the same incoming elements in both the graph route and Insight database, we can deduct that the graph is valid
        /// </summary>
        /// <param name="graph">the graph to check for a valid route</param>
        /// <returns>true if the graph route is valid, false otherwise</returns>
        public bool VerifyValidRoute(ServiceGraph graph)
        {
            bool ans = true;
            List<string> graphElementNames = new List<string>();
            List<string> insightElementNames = new List<string>();
            // gather all name of the graph element list
            foreach (GraphElement graphElement in graph.graphElements)
            {
                graphElementNames = new List<string>();
                insightElementNames = new List<string>();
                var InsightIncomingElements = GetInsightinBoundByObjectName(graphElement.CurrentElement.name, "Element");
                var GraphRouteIncomingElements = graphElement.IncomingElements;
                foreach (GraphElement incomingElement in graph.graphElements)
                {
                    graphElementNames.Add(incomingElement.CurrentElement.name);
                }
                if (InsightIncomingElements == null || InsightIncomingElements.objectEntries == null || InsightIncomingElements.objectTypeAttributes == null)
                {
                    logger.Debug("Error in the insight response received");
                    return false;
                }
                else if (InsightIncomingElements.objectEntries.Count != GraphRouteIncomingElements.Count)
                {
                    logger.Debug("The graph element: {0} has {1] incoming elements, while the insight has {2} incoming elements", graphElement.CurrentElement.name, InsightIncomingElements.objectEntries.Count, GraphRouteIncomingElements.Count);
                    return false;
                }
                else
                {
                    // gather all name of the insight element list
                    foreach (var entry in InsightIncomingElements.objectEntries)
                    {
                        insightElementNames.Add(entry.name);
                    }
                    if (!insightElementNames.Contains(graphElement.CurrentElement.name))
                    {
                        logger.Debug("The graph element: {0}, does not exist in the insight incoming element list", graphElement.CurrentElement.name);
                        return false;
                    }
                    else
                    {
                        // we checked that the graph element exists in the InsightIncomingElements list.
                        // if every element in the InsightIncomingElements list is also contained in the graph element list.
                        // then accorsing to the rule of by directional containment the two lists are equal.
                        foreach (var entry in InsightIncomingElements.objectEntries)
                        {
                            if (!graphElementNames.Contains(entry.name))
                            {
                                logger.Debug("The graph element: {0} has a missing element: {1}", graphElement.CurrentElement.name, entry.name);
                                return false;
                            }
                        }
                    }
                }
            }
            return ans;
        }
        #pragma warning disable CS1591
        public Dictionary<string, List<ObjectType>> GetObjectGroups()
        #pragma warning restore CS1591
        {
            return ObjectGroups;
        }
        
        /// <summary>
        /// The insight client will search for the elemets related to the uuid.
        /// </summary>
        /// <param name="uuid"> The uuid that we want to get a graph/route for</param>
        /// <returns>Grapgh which represents the service element route</returns>
        public ServiceGraph GetServiceGraph(string uuid)
        {
            string originalUUID = uuid;
            try
            {
                // check if the uuid contains illegal characters and remove them
                uuid = Tools.ModifyUnspportedInsightNameConvention(uuid, forbiddenInsightApiQuerySymbols);
                logger.Debug("The original uuid provided for this build: {0}", originalUUID);
                logger.Debug("The modified uuid created for this build: {0}", uuid);

                // Check if the IQL result are legal
                IqlApiResult serviceResult = GetInsightObjectByName(uuid, "Root", "Service");
                IqlApiResult elementResult = GetInsightOutBoundByObjectName(uuid, "Element");
                if (!Tools.IsValidIqlResult(serviceResult) || !Tools.IsValidIqlResult(elementResult))
                {
                    throw new CorruptedInsightData(uuid);
                }
                // if there is more than one service or none that are matching the given uuid, 
                // it must mean that the uuid contains an illegal naming conevtion, which gave false positive results after the name modification
                if (serviceResult.objectEntries.Count != 1)
                {
                    throw new IllegalNameException(uuid);
                }

                // From here the code logic starts
                ServiceGraph graph = new ServiceGraph(elementResult, serviceResult, debug, uuid);
                if (graph != null && graph.constructorSuceeded)
                {
                    return graph;
                }
                else
                {
                    logger.Error("graph construction failed");
                    return null;
                }
            }
            catch (IllegalNameException e)
            {
                logger.Error(e.Message);
                throw e;
            }
            catch (InsighClientLibraryUnknownError e)
            {
                logger.Fatal(e.Message);
                throw e;
            }
            catch (CorruptedInsightData e)
            {
                logger.Error(e.Message);
                throw e;
            }
        }
        /// <summary>
        /// Goes over the schema data from insight and create groups to an accesible member
        /// </summary>
        private void CreateSchemaGraph(int schemaId)
        {
            ObjectGroups = new Dictionary<string, List<ObjectType>>();
            ObjectType[] objectTypeList = GetInsightObjectTypeList(schemaId);
            if (!AuthenticationTest.Equals("OK"))
            {
                return;
            }
            List<ObjectType> objectList = new List<ObjectType>(objectTypeList);
            ObjectType root = null;
            List<ObjectType> remaining = new List<ObjectType>();
            foreach (var objectType in objectList)
            {
                if (!objectType.parentObjectTypeInherited)
                {
                    root = objectType;
                    ObjectGroups.Add("SuperRoot", new List<ObjectType>());
                    ObjectGroups["SuperRoot"].Add(root);
                }
                else remaining.Add(objectType);
            }

            objectList = remaining;
            remaining = new List<ObjectType>();
            List<ObjectType> childGroup; ;
            Dictionary<string, List<ObjectType>> NewObjectGroups;

            while (objectList.Count > 1)
            {
                remaining = new List<ObjectType>();
                childGroup = new List<ObjectType>();
                NewObjectGroups = new Dictionary<string, List<ObjectType>>();
                foreach (var parentGroup in ObjectGroups.Values)
                {
                    foreach (ObjectType potentialParent in parentGroup)
                    {
                        foreach (ObjectType potentialChild in objectList)
                        {
                            int childParentTypeId = potentialChild.parentObjectTypeId;
                            if (potentialParent.id == potentialChild.parentObjectTypeId && !potentialChild.name.Equals("CSV Import"))
                            {
                                if (ObjectGroups.ContainsKey(potentialParent.name))
                                {
                                    ObjectGroups[potentialParent.name].Add(potentialChild);
                                }
                                else
                                {
                                    if (NewObjectGroups.ContainsKey(potentialParent.name))
                                    {
                                        NewObjectGroups[potentialParent.name].Add(potentialChild);
                                    }
                                    else
                                    {
                                        childGroup.Add(potentialChild);
                                        NewObjectGroups.Add(potentialParent.name, childGroup);
                                        childGroup = new List<ObjectType>();
                                    }

                                }
                            }
                            else
                            {
                                remaining.Add(potentialChild);
                            }
                        }
                        objectList = remaining;
                        remaining = new List<ObjectType>();
                    }
                }
                foreach (var item in NewObjectGroups)
                {
                    ObjectGroups.Add(item.Key, item.Value);
                }

            }
        }
        /// <summary>
        ///  Gets all object types conatained in a schema
        /// </summary>
        /// <returns>array of <see cref="ObjectType"/>ObjectType/></returns>
        public ObjectType[] GetInsightObjectTypeList(int schemaID)
        {
            try
            {
                string schemaquery = "objectschema/" + schemaID + "/objecttypes/flat";
                RestRequest request = new RestRequest(schemaquery, Method.GET);
                var response = InsightRestClient.Execute(request);
                string statusCode = response.StatusCode.ToString();
                if (statusCode.Equals("Unauthorized"))
                {
                    AuthenticationTest = "Unauthorized";
                    throw new Exception("The user set for this operation in not authorized with insight");
                }
                AuthenticationTest = statusCode;
                return JsonConvert.DeserializeObject<ObjectType[]>(response.Content);
            }
            catch (Exception e)
            {
                logger.Fatal("Fatal error while communicating with Insight API:\n" + e.Message);
                return null;
            }
            
        }
        /// <summary>
        /// return a lient using the credentails of the MUC generic user
        /// </summary>
        /// <returns></returns>
        public RestClient GetDefaultClient()
        {
            return GetClient("dataminer_muc", "SAz{ 2YY3SQeThh }:");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"> Insight valid username</param>
        /// <param name="password"> Insight valid password</param>
        /// <returns> A new rest client initialized using the given parameters</returns>
        public RestClient GetClient(string username, string password)
        {
            try
            {
                InsightRestClient = new RestClient();
                InsightRestClient.BaseUrl = new Uri(InsightApiServerAdress);
                InsightRestClient.Authenticator = new HttpBasicAuthenticator(username, password);
                InsightRestClient.Timeout = 30000;
                InsightRestClient.AddDefaultHeader("User-Agent", "DataMiner");
                return InsightRestClient;
            }
            catch (Exception e)
            {
                logger.Error("Error with creating the web client\n" + e.Message);
                throw new Exception();
            }
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for an object matching the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="group">Object type group that the object belons to in insight</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightObjectByName(string name, string group)
        {
            string insightElementQuery = "(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, group, "");
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for an object matching the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="group">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightObjectByName(string name, string group, string groupMemeber)
        {
            string insightElementQuery = "(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, group, groupMemeber);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects reffering to the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightOutBoundByObjectName(string name, string groupName)
        {
            string insightElementQuery = "object HAVING outboundReferences(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, "");
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects reffering to the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightOutBoundByObjectName(string name, string groupName, string groupMemeber)
        {
            string insightElementQuery = "object HAVING outboundReferences(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, groupMemeber);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects refferd by the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightinBoundByObjectName(string name, string groupName)
        {
            string insightElementQuery = "object HAVING inboundReferences(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, "");
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects refferd by the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightinBoundByObjectName(string name, string groupName, string groupMemeber)
        {
            string insightElementQuery = "object HAVING inboundReferences(Name LIKE " + name + ")";
            return InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, groupMemeber);
        }
        /// <summary>
        /// Using the group details received, deduces the id group the insight request will use to receive a response for the request
        /// </summary>
        /// <returns>Insight object containing the data from Insight</returns>
        private IqlApiResult InsightGetByIqlObjectIdGroup(string iqlQuery, string groupName, string groupMemeber)
        {
            var list = ObjectGroups[groupName];
            string objectIdGroup = "(";
            foreach (var item in list)
            {
                if (groupMemeber.Equals(""))
                {
                    objectIdGroup += item.id + ",";
                }
                else if (item.name.Equals(groupMemeber))
                {
                    objectIdGroup += item.id + ",";
                }
            }
            if (objectIdGroup[objectIdGroup.Length - 1] == ',')
            {
                objectIdGroup = objectIdGroup.Substring(0, objectIdGroup.Length - 1);
            }
            objectIdGroup += ")";
            string insightQuery = "objectTypeId IN " + objectIdGroup + " AND " + iqlQuery;
            return InsightGetByGeneralIqlQuery(insightQuery);
        }
        /// <summary>
        /// An advanced user can directly enter an IQL Insight valid query and receive a valid response
        /// </summary>
        /// <param name="iqlQuery"> The IQL query for the API call</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult InsightGetByGeneralIqlQuery(string iqlQuery)
        {
            var response = ExecuteGetResponse(iqlResource + iqlQuery);
            return JsonConvert.DeserializeObject<IqlApiResult>(response.Content);
        }
        /// <summary>
        /// Excecute a get query for the gicen resource.
        /// Appropriate debug meesages exists for troublshooting.
        /// </summary>
        /// <param name="resource">the insight query</param>
        /// <returns>returns a response conatining the data according to the request</returns>
        private IRestResponse ExecuteGetResponse(string resource)
        {
            RestRequest request = new RestRequest(resource, Method.GET);
            try
            {
                logger.Debug("insightQuery: " + InsightRestClient.BaseUrl + request.Resource);
                var response = InsightRestClient.Execute(request);
                if (response == null)
                {
                    logger.Error("Client executed the insight request but got null response");
                }
                else if (response.ErrorException != null)
                {
                    logger.Error("Client execute the insight request due to Restsharp exception:");
                    logger.Error(response.ErrorException.Message);
                }
                else if (!response.IsSuccessful)
                {
                    logger.Error("Client failed to execute the insight request due to unhandled Restsharp exception");
                }
                else logger.Debug("Client executed the insight request and got a response");


                logger.Debug("the response result was successfull with the status code" + response.StatusCode);
                return response;

            }
            catch (Exception e)
            {
                logger.Error("Client failed to execute the insight request due to unknown exception");
                logger.Error(e.Message);
                return null;
            }
        }
    }
}
