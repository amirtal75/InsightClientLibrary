using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using NLog;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Net;

namespace InsightClientLibrary
{
#pragma warning disable CS1591
    public class InsightClient
#pragma warning restore CS1591
    {
        /// <summary>
        /// This class contains a variety of methods meant to perform requests from and to Insight.
        /// In case the insight version will be updated, the following must adjusted accordingly:
        ///     1. The function GetInsightObjectTypeList, uses the insight API query: https://jira.mx1.com/rest/insight/1.0/objectschema/3/objecttypes/flat
        ///     2. The function 
        /// </summary>
        private bool debug = false;
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
            try
            {
                CreateSchemaGraph(3, "dataminer_muc");
            }
            catch (InsightUserAthenticationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Constructs a client to communicate with insight
        /// </summary>
        /// Constructs a client to communicate with insight
        /// <param name="debug"> will run the program with debug messages accordingly</param>
        /// <param name="username"> the username for insight login</param>
        /// <param name="password"> the password for insight login</param>
        /// <param name="PopName"> the name of the pop that creates this client</param>
        public InsightClient(bool debug, string PopName, string username, string password)
        {
            InsightRestClient = GetClient(username, password);
            logger = NLog.LogManager.GetCurrentClassLogger();

            if (InsightRestClient != null)
            {
                try
                {
                    RestRequest request = new RestRequest("objectschema/3/objecttypes/flat", Method.GET);
                    var response = InsightRestClient.Execute(request);
                    if (response.ErrorException != null)
                    {
                        logger.Error("Client failed Insight validity test, due to the following exception:\n" + response.ErrorException.Message);
                        logger.Error("Stack Trace:\n" + response.ErrorException.StackTrace);
                        throw new RestSharpException(response.ErrorException.Message);
                    }
                    else if (!response.IsSuccessful)
                    {
                        logger.Error("Client failed Insight validity test, due to the following details:\n" + response.Content + "\nStatus code:" + response.StatusCode);
                        throw new UnsuccessfullResponseException("Status code: " + response.StatusCode + ", Reponse content: " + response.Content);
                    }
                    else
                    {
                        POPName = PopName;
                        CreateSchemaGraph(3, username);
                    }

                }
                catch (InsightUserAthenticationException e)
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
                    logger.Fatal("Client failed Insight validity test, due to a unknown issue");
                    throw new Exception(e.Message + "|" + e.StackTrace);
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
        /// Goes over all the elements in the graph, for each element check the incoming elements in Insight database and in the built graph route.
        /// If every grapgh element has the same incoming elements in both the graph route and Insight database, we can deduct that the graph is valid
        /// </summary>
        /// <param name="graph">the graph to check for a valid route</param>
        /// <returns>true if the graph route is valid, false otherwise</returns>
        public bool VerifyValidRoute(ServiceGraph graph)
        {
            bool ans = true;
            List<string> graphElementNames = new List<string>(graph.ServiceElementNameList.Keys);
            List<string> insightIncomingElementNames = new List<string>();
            List<string> graphElementIncomingElementNames = new List<string>();

            // gather all name of the graph element list
            foreach (GraphElement graphElement in graph.graphElements)
            {
                insightIncomingElementNames = new List<string>();
                graphElementIncomingElementNames = new List<string>();
                bool modified = false;
                var modifiedElementName = Tools.ModifyUnspportedInsightNameConvention(graphElement.CurrentElement.name, forbiddenInsightApiQuerySymbols);
                string nameEqualizer = "=";
                if (!modifiedElementName.Equals(graphElement.CurrentElement.name))
                {
                    nameEqualizer = "LIKE";
                }
                string equalizer = "=";
                if (graph.modifiedUUID)
                {
                    equalizer = "LIKE";
                }

                string modifiedServiceName = Tools.ModifyUnspportedInsightNameConvention(graph.Service.Name, forbiddenInsightApiQuerySymbols);
                string[] getGroups = new string[2] { "", "" };
                string groups = InsightGetByIqlObjectIdGroup(getGroups, "Element", "", modified);
                string query = "object HAVING outboundReferences(Name " + equalizer + " " + modifiedServiceName + ") AND " + groups + "object HAVING inboundReferences(Name " + nameEqualizer + " " + modifiedElementName + ")";
                var InsightIncomingElements = InsightGetByGeneralIqlQuery(query);
                List<ObjectEntry> elementEntries = new List<ObjectEntry>();
                foreach (var entry in InsightIncomingElements.objectEntries)
                {
                    if (!entry.name.Equals(graphElement.CurrentElement.name))
                    {
                        foreach (var attribute in entry.attributes)
                        {
                            if (attribute.objectTypeAttributeId == 1781 && attribute.ObjectAttributeValues != null && attribute.ObjectAttributeValues.Count > 0)
                            {
                                foreach (var attributeValue in attribute.ObjectAttributeValues)
                                {
                                    if (attributeValue.displayValue.Equals(graph.Service.Name))
                                    {
                                        elementEntries.Add(entry);
                                    }
                                }
                            }
                        }

                    }
                }

                
                InsightIncomingElements.objectEntries = elementEntries;
                var GraphRouteIncomingElements = graphElement.IncomingElements;
                if (!Tools.IsValidIqlResult(InsightIncomingElements))
                {
                    logger.Debug("Error in the insight response received, assuming valid graph build");
                    return true;
                }
                else if (InsightIncomingElements.objectEntries.Count != GraphRouteIncomingElements.Count)
                {
                    logger.Fatal("The graph element: " + graphElement.CurrentElement.name + " has " + InsightIncomingElements.objectEntries.Count + " incoming elements, while the insight has " + GraphRouteIncomingElements.Count + " incoming elements");
                    return false;
                }
                else
                {
                    // gather all name of the insight incoming element list and graph incoming element list
                    foreach (var entry in InsightIncomingElements.objectEntries)
                    {
                        insightIncomingElementNames.Add(entry.name);
                    }
                    foreach (var incomingGraphElement in graphElement.IncomingElements)
                    {
                        graphElementIncomingElementNames.Add(incomingGraphElement.CurrentElement.name);
                    }

                    // According to the Discreet mathematics law of equality, if two sets A,B where A<=b and b<=A then A=B.
                    // this is checked below for the incoming element found in inisght in comparison to the realtionship built in the graph route
                    foreach (var incomingGraphElementName in graphElementIncomingElementNames)
                    {
                        if (insightIncomingElementNames.Count > 0 && !insightIncomingElementNames.Contains(incomingGraphElementName))
                        {
                            logger.Error("The graph element: {0} has an extra element: {1}", graphElement.CurrentElement.name, incomingGraphElementName);
                            return false;
                        }
                    }
                    foreach (var incomingInsightElementName in insightIncomingElementNames)
                    {
                        if (graphElementIncomingElementNames.Count > 0 && !graphElementIncomingElementNames.Contains(incomingInsightElementName))
                        {
                            logger.Error("The graph element: {0} has a missing element: {1}", graphElement.CurrentElement.name, incomingInsightElementName);
                            return false;
                        }
                    }
                }
            }
            return ans;
        }
        /// <summary>
        /// return the object schema divided into type groups
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<ObjectType>> GetObjectGroups()
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
                bool modified = false; ;
                // check if the uuid contains illegal characters and remove them
                uuid = Tools.ModifyUnspportedInsightNameConvention(uuid, forbiddenInsightApiQuerySymbols);
                if (uuid[0] == '"')
                {
                    string tmp = uuid.Substring(1, uuid.Length - 2);
                    modified = !tmp.Equals(originalUUID);
                }
                else modified = !uuid.Equals(originalUUID);
                logger.Debug("The original uuid provided for this build: {0}", originalUUID);
                logger.Debug("The modified uuid created for this build: {0}", uuid);

                // Check if the IQL result are legal
                IqlApiResult serviceResult = GetInsightObjectByName(uuid, "Root", "Service", modified);
                IqlApiResult elementResult = GetInsightOutBoundByObjectName(uuid, "Element", modified);
                if (!Tools.IsValidIqlResult(serviceResult) || !Tools.IsValidIqlResult(elementResult) || serviceResult.objectEntries.Count == 0)
                {
                    throw new CorruptedInsightDataException(uuid);
                }
                // if there is more than one service or none that are matching the given uuid, 
                // it must mean that the uuid contains an illegal naming conevtion, which gave false positive results after the name modification
                if (serviceResult.objectEntries.Count > 1)
                {
                    Tools.UniquenessLostFix(originalUUID, ref serviceResult, ref elementResult);
                    //throw new IllegalNameException(uuid + " Uniquness lost!!");
                }

                // From here the code logic starts
                ServiceGraph graph = new ServiceGraph(elementResult, serviceResult, debug, originalUUID, modified);
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
                logger.Fatal("Unknwon error: \n" + e.Message + "|" + e.StackTrace);
                throw e;
            }
        }
        /// <summary>
        /// Goes over the schema data from insight and create groups to an accesible member
        /// </summary>
        private void CreateSchemaGraph(int schemaId, string username)
        {
            ObjectGroups = new Dictionary<string, List<ObjectType>>();
            ObjectType[] objectTypeList;
            try
            {
                objectTypeList = GetInsightObjectTypeList(schemaId, username);
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
            catch (InsightUserAthenticationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                logger.Fatal("Uknown exception: \n" + e.Message + "|" + e.StackTrace);
                throw e;
            }
        }
        /// <summary>
        ///  Gets all object types conatained in a schema
        /// </summary>
        /// <returns>array of <see cref="ObjectType"/>ObjectType/></returns>
        public ObjectType[] GetInsightObjectTypeList(int schemaID, string username)
        {
            try
            {
                string schemaquery = "objectschema/" + schemaID + "/objecttypes/flat";
                RestRequest request = new RestRequest(schemaquery, Method.GET);
                var response = InsightRestClient.Execute(request);
                string statusCode = response.StatusCode.ToString();
                if (statusCode.Equals("Unauthorized"))
                {
                    throw new InsightUserAthenticationException(username);
                }
                AuthenticationTest = statusCode;
                return JsonConvert.DeserializeObject<ObjectType[]>(response.Content);
            }
            catch (InsightUserAthenticationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                logger.Fatal("Fatal error while communicating with Insight API:\n" + e.Message + "|" + e.StackTrace);
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
                InsightRestClient.Timeout = 60000;
                InsightRestClient.AddDefaultHeader("User-Agent", "DataMiner");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                return InsightRestClient;
            }
            catch (Exception e)
            {
                logger.Error("Error with creating the web client\n" + e.Message + "|" + e.StackTrace);
                throw new Exception();
            }
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for an object matching the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="group">Object type group that the object belons to in insight</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightObjectByName(string name, string group, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "Name = " + name + ")", "Name LIKE " + name + ")" };
            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, group, "", nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for an object matching the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="group">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightObjectByName(string name, string group, string groupMemeber, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "(Name = " + name + ")", "(Name LIKE " + name + ")" };
            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, group, groupMemeber, nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects reffering to the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightOutBoundByObjectName(string name, string groupName, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "object HAVING outboundReferences(Name = " + name + ")", "object HAVING outboundReferences(Name LIKE " + name + ")" };
            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, "", nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects reffering to the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightOutBoundByObjectName(string name, string groupName, string groupMemeber, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "object HAVING outboundReferences(Name = " + name + ")", "object HAVING outboundReferences(Name LIKE " + name + ")" };
            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, groupMemeber, nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects refferd by the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightinBoundByObjectName(string name, string groupName, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "object HAVING inboundReferences(Name = " + name + ")", "object HAVING inboundReferences(Name LIKE " + name + ")" };

            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, "", nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Initiates an API call to insight to receive a response for objects refferd by the given name.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <param name="groupName">Object type group that the object belongs to in insight database</param>
        /// <param name="groupMemeber">name of the member in the insight type groug that the name belongs to</param>
        /// <param name="nameWasModified"> Indicates whether the object name was modified due to illegal Insight naming convetion</param>
        /// <returns>Insight object containing the data from Insight</returns>
        public IqlApiResult GetInsightinBoundByObjectName(string name, string groupName, string groupMemeber, bool nameWasModified)
        {
            string[] insightElementQuery = new string[2] { "object HAVING inboundReferences(Name = " + name + ")", "object HAVING inboundReferences(Name LIKE " + name + ")" };
            string queryWithGroups = InsightGetByIqlObjectIdGroup(insightElementQuery, groupName, groupMemeber, nameWasModified);
            return InsightGetByGeneralIqlQuery(queryWithGroups);
        }
        /// <summary>
        /// Using the group details received, deduces the id group the insight request will use to receive a response for the request
        /// </summary>
        /// <returns>Insight object containing the data from Insight</returns>
        private string InsightGetByIqlObjectIdGroup(string[] iqlQuery, string groupName, string groupMemeber, bool nameWasModified)
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
            string insightQueryEqual = "objectTypeId IN " + objectIdGroup + " AND " + iqlQuery[0];
            string insightQueryLike = "objectTypeId IN " + objectIdGroup + " AND " + iqlQuery[1];
            if (nameWasModified)
            {
                return insightQueryLike;
            }
            else return insightQueryEqual;
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
                logger.Error(e.Message + "|" + e.StackTrace);
                return null;
            }
        }
    }
}