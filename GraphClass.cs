using System;
using System.Collections.Generic;
using NLog;


namespace InsightClientLibrary
{
    /// <summary>
    /// Represent a service graph for a uuid
    /// </summary>
    public class ServiceGraph
    {
        /// <value>List of all the elements in the graph</value>
        public List<GraphElement> graphElements;
        /// <value>teh service that the graph was built for</value>
        public Service Service { get; set; }
        /// <value>service status</value>
        public string serviceStatus { get; }
        /// <value>Raw insight element data</value>
        public IqlApiResult IqlApiResult { get; set; }
        /// <value>Array of all the entries in the graph</value>
        public List<ObjectEntry> RouteElements { get; set; }
        /// <value>Array of all the typeAttributes in the graph</value>
        public List<ObjectTypeAttribute> RouteTypeAttributes { get; set; }
        /// <value>Array of int lists, each index related to a routElement, for that index, each list contains indexes in the routeelements that are incoming elements for the route element in that index</value>
        public HashSet<int>[] ElementIncomingElementIndexes { get; set; }
        /// <value>List of the graph sources</value>
        public List<GraphElement> Sources { get; set; }
        /// <value>List of the graph route end elements</value>
        public List<GraphElement> RouteEnd { get; set; }
        /// <value>index in the route element array reffering to the monitoring element of the graph</value>
        public int MonitoringElementIndex { get; set; }
        /// <value>Dictionary containing the names of all the elements in the graph</value>
        public Dictionary<string, int> ServiceElementNameList { get; set; }
        /// <value>Dictionary containing the names of all the atribute types in the graph</value>
        public Dictionary<int, string> ObjectAttributeTypesById;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public NLog.Logger logger;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        /// <value>bool value indicating a succesfull and valid graph build</value>
        public bool constructorSuceeded = false;
        /// <value>bool value indicating a partially succesfull graph build with an invalid member</value>
        public bool ElementMemberIsNull = false;
        private bool debug = false;

        /// <summary>
        /// Constructs a graph containing raw and parsed information of the insight API result
        /// </summary>
        /// <param name="root"> the element list API result</param>
        /// <param name="service">the service get API result</param>
        /// <param name="_debug"> indicated whether the program should run in debug mode</param>
        /// <param name="uuid"> the name of the service to build a graph for</param>
        public ServiceGraph(IqlApiResult root, IqlApiResult service, bool _debug, string uuid)
        {
            debug = _debug;
            InitLogger();
            this.Service = new Service(service, logger,uuid);
            if (root == null || service.objectEntries == null || service.objectTypeAttributes == null)
            {
                return;
            }
            if (root.objectEntries == null || root.objectTypeAttributes == null)
            {
                ElementMemberIsNull = true;
                return;
            }
            this.RouteElements = root.objectEntries;
            this.RouteTypeAttributes = root.objectTypeAttributes;
            this.ElementIncomingElementIndexes = new HashSet<int>[root.objectEntries.Count];
            this.ServiceElementNameList = new Dictionary<string, int>();
            this.ObjectAttributeTypesById = new Dictionary<int, string>();
            this.IqlApiResult = root;
            bool initSuccess = true;
            bool incominElementSuccess = true;
            bool soureSetSuccess = true;
            bool buildGraphSuccess = true;
            try
            {
                logger.Debug("Starting the data initialization......");
                InitData();
                logger.Debug("Data initialization completed");
            }
            catch (Exception e)
            {
                logger.Error("initialization failed " + Service.Name + "\n" + e.Message);
                initSuccess = false;
            }
            try
            {
                logger.Debug("Setting Incoming elements......");
                FindIncomingElements();
                logger.Debug("Incoming elements setting completed");
            }
            catch (Exception e)
            {
                logger.Error("Setting Incoming elements failed " + Service.Name + "\n" + e.Message);
                incominElementSuccess = false;
            }
            try
            {
                logger.Debug("Setting sources......");
                this.Sources = FindSources();
                logger.Debug("source setting completed");
            }
            catch (Exception e)
            {
                logger.Error("Setting sources failed" + "\n" + e.Message);
                soureSetSuccess = false;
            }
            try
            {
                logger.Debug("Starting the graph build......");
                BuildServiceGraph();
                logger.Debug("graph build completed");
            }
            catch (Exception e)
            {
                logger.Error("graph build failed" + "\n" + e.Message);
                buildGraphSuccess = false;
            }

            this.constructorSuceeded = initSuccess && incominElementSuccess && soureSetSuccess && buildGraphSuccess;
        }
        private void InitData()
        {
            for (int i = 0; i < this.RouteElements.Count; i++)
            {
                this.ServiceElementNameList.Add(RouteElements[i].name, i);
                if (RouteElements[i] != null && RouteElements[i].objectType != null && RouteElements[i].objectType.name.Equals("Monitoring"))
                {
                    MonitoringElementIndex = i;
                }
            }

            foreach (var attributeType in IqlApiResult.objectTypeAttributes)
            {
                ObjectAttributeTypesById.Add(attributeType.id, attributeType.name);
            }
        }
        private void InitLogger()
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
            logger = NLog.LogManager.GetCurrentClassLogger();
        }
        private void FindIncomingElements()
        {
            ObjectAttribute attr = null;
            ObjectAttributeValue ObjectAttributeValue = null;
            ObjectEntry referencedobject = null;
            int elementIndex = 0;

            for (int i = 0; i < this.RouteElements.Count; i++)
            {
                if (RouteElements[i].objectType.name.Equals("Monitoring"))
                {
                    MonitoringElementIndex = i;
                }
                for (int j = RouteElements[i].attributes.Length - 1; j >= 0; j--)
                {
                    attr = RouteElements[i].attributes[j];
                    for (int k = 0; k < attr.ObjectAttributeValues.Count; k++)
                    {
                        ObjectAttributeValue = attr.ObjectAttributeValues[k];
                        referencedobject = ObjectAttributeValue.referencedObject;
                        if (referencedobject != null)
                        {
                            if (referencedobject.objectType.parentObjectTypeId == 257)
                            {
                                if (ServiceElementNameList.ContainsKey(referencedobject.name))
                                {
                                    if (ElementIncomingElementIndexes[i] == null)
                                    {
                                        ElementIncomingElementIndexes[i] = new HashSet<int>();
                                    }
                                    elementIndex = ServiceElementNameList[referencedobject.name];
                                    ElementIncomingElementIndexes[i].Add(elementIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<GraphElement> FindSources()
        {
            List<GraphElement> sources = new List<GraphElement>();

            for (int i = 0; i < ElementIncomingElementIndexes.Length; i++)
            {
                if (ElementIncomingElementIndexes[i] == null)
                {
                    sources.Add(new GraphElement(RouteElements[i], 0, this.ObjectAttributeTypesById));
                }
            }

            return sources;
        }

        private void BuildServiceGraph()
        {
            List<GraphElement> tmp = Sources;
            List<GraphElement> allElements = new List<GraphElement>();
            GraphElement[] array = null;
            while (tmp != null)
            {
                this.RouteEnd = tmp;
                array = new GraphElement[tmp.Count];
                tmp.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    if (!allElements.Contains(array[i]))
                    {
                        allElements.Add(array[i]);
                    }
                }
                tmp = BuildNextGraphLevel(tmp, allElements);
            }
            graphElements = allElements;
        }

        private List<GraphElement> BuildNextGraphLevel(List<GraphElement> GraphElementsLevel, List<GraphElement> allElements)
        {

            ObjectEntry incomingElement;
            GraphElement prev;
            GraphElement next;
            List<GraphElement> nextLevelElements = new List<GraphElement>();
            int[] indexes;
            int elementIndex = 0;
            int incomingElementIndex = 0;
            HashSet<int> indexHolder = null;
            GraphElement[] GraphElementsArray = new GraphElement[GraphElementsLevel.Count];
            GraphElementsLevel.CopyTo(GraphElementsArray);
            for (int i = 0; i < RouteElements.Count; i++)
            {
                indexHolder = ElementIncomingElementIndexes[i];
                if (indexHolder != null)
                {
                    indexes = new int[indexHolder.Count];
                    ElementIncomingElementIndexes[i].CopyTo(indexes, 0);

                    for (incomingElementIndex = 0; incomingElementIndex < indexes.Length; incomingElementIndex++)
                    {
                        elementIndex = indexes[incomingElementIndex];
                        incomingElement = RouteElements[elementIndex];
                        for (int j = 0; j < GraphElementsArray.Length; j++)
                        {
                            prev = (GraphElement)GraphElementsArray[j];
                            if (incomingElement.name.Equals(prev.CurrentElement.name))
                            {
                                next = new GraphElement(RouteElements[i], prev.graphLength - 1, this.ObjectAttributeTypesById);
                                if (allElements.Contains(next))
                                {
                                    next = allElements[allElements.IndexOf(next)];
                                }
                                if (!next.IncomingElements.Contains(prev))
                                {
                                    next.AddIncomingElement(prev);
                                }

                                if (!prev.OutgoingElements.Contains(next))
                                {
                                    prev.AddOutgoingElement(next);
                                }
                                nextLevelElements.Add(next);
                            }
                        }
                    }
                }
            }

            if (nextLevelElements.Count == 0)
            {
                return null;
            }
            else return nextLevelElements;
        }

        /// <summary>
        /// print the graph elements by layers, each layer cntains elements of the same length
        /// </summary>
        /// <returns></returns>
        public string PrintGraph()
        {
            string answer = "";
            List<GraphElement> copy = new List<GraphElement>(graphElements);
            List<GraphElement> max = null;

            while (copy.Count > 0)
            {
                max = FindMinLength(copy);
                answer += "Printing level:\n" + max[0].graphLength;
                foreach (var item in max)
                {
                    answer += item.CurrentElement.name + " || ";
                    copy.Remove(item);
                }
                answer += "\n";
            }
            return answer;
        }
        /// <summary>
        /// finds the element/s with shortest distance to this element
        /// </summary>
        /// <param name="currentSpan"> the list of elements in the span we wish to compare</param>
        /// <returns></returns>
        public static List<GraphElement> FindMinLength(List<GraphElement> currentSpan)
        {
            List<GraphElement> element = new List<GraphElement>();
            if (currentSpan == null)
            {
                return null;
            }
            int min = Int32.MaxValue;
            foreach (GraphElement graphElement in currentSpan)
            {
                if (graphElement.graphLength < min)
                {
                    element = new List<GraphElement>();
                    element.Add(graphElement);
                    min = graphElement.graphLength;
                }
                else if (graphElement.graphLength == min)
                {
                    element.Add(graphElement);
                }
            }

            return element;
        }
        /// <summary>
        /// Method to check if an element is active
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsActive(GraphElement element)
        {
            bool ans = true;

            var attributes = element.CurrentElement.attributes;
            foreach (var attribute in attributes)
            {
                var typeName = this.ObjectAttributeTypesById[attribute.id];
                if (typeName.Equals("Active"))
                {
                    return attribute.ObjectAttributeValues[0].displayValue.Equals("Active");
                }
            }
            return ans;
        }
        private bool QueryVerification(string[] queries, Dictionary<string, int> AttributeByIds, Dictionary<int, List<ObjectAttributeValue>> valueList)
        {
            int typeAttrIndex = -1;
            bool answer = false;
            foreach (var query in queries)
            {
                if (AttributeByIds.ContainsKey(query))
                {
                    typeAttrIndex = AttributeByIds[query];
                    if (typeAttrIndex > -1)
                    {
                        List<ObjectAttributeValue> values = valueList[typeAttrIndex];
                        if (values != null && values.Count > 0 && values[0].displayValue != null && !values[0].displayValue.Equals(""))
                        {
                            answer = true;
                        }
                        else
                        {
                            answer = false;
                            break;
                        }
                    }
                    else
                    {
                        answer = false;
                        break;
                    }
                }
                else
                {
                    answer = false;
                    break;
                }
            }
            return answer;
        }
        /// <summary>
        /// Finds lockable graph elements for a given element list
        /// </summary>
        /// <param name="initialElements"></param>
        /// <param name="pop"></param>
        /// <returns></returns>
        public List<GraphElement> getLockableElements(List<GraphElement> initialElements, string pop)
        {
            List<GraphElement> lockableElements = new List<GraphElement>();
            List<GraphElement> elementsToCheck = initialElements;
            List<GraphElement> elementsToFind = initialElements;
            string elementTypeName = "";
            while (elementsToFind.Count > 0)
            {
                elementsToCheck = elementsToFind;
                elementsToFind = new List<GraphElement>();
                foreach (var graphElement in elementsToCheck)
                {
                    var attributes = new List<ObjectAttribute>(graphElement.CurrentElement.attributes);
                    Dictionary<string, int> AttributeByIds = new Dictionary<string, int>();
                    Dictionary<int, List<ObjectAttributeValue>> valueList = new Dictionary<int, List<ObjectAttributeValue>>();
                    foreach (var attribute in attributes)
                    {
                        var typeId = attribute.objectTypeAttributeId;
                        var typeName = graphElement.ObjectAttributeTypesById[typeId];
                        AttributeByIds.Add(typeName, typeId);
                        valueList.Add(typeId, attribute.ObjectAttributeValues);
                    }
                    string department = "";
                    string query = "";
                    int departmentId = AttributeByIds["Department"];
                    List<ObjectAttributeValue> valList = valueList[departmentId];
                    if (valList.Count == 1)
                    {
                        department = valList[0].displayValue;
                    }
                    string[] queries;
                    elementTypeName = graphElement.CurrentElement.objectType.name.ToLower();
                    switch (elementTypeName.ToLower())
                    {
                        case "downlink":
                            if (department.Contains(pop))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "encoding":
                            query = "Encoding Multicast " + pop;
                            queries = (new string[3] { (query + " Main"), (query + " BU"), query });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "muxing":
                            query = "Muxing Output Main Multicast " + pop;
                            queries = (new string[2] { "Muxing Output Main Multicast " + pop, "Muxing Output BU Multicast " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "timeshift":
                            queries = (new string[1] { "Timeshift Multicast " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "mbr":
                            queries = (new string[2] { "Multicast Out Main " + pop, "Multicast Out BU " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "ip to ip gateway":
                            query = "Multicast " + pop;
                            queries = (new string[2] { query + " In", query + " Out" });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "decoding":
                            query = "Muxing Output Main Multicast " + pop;
                            queries = (new string[1] { "Multicast Out " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "channel in a box":
                            query = "Muxing Output Main Multicast " + pop;
                            queries = (new string[1] { "Output Multicast " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "uplink":
                            if (department.Contains(pop))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "fiber video transfer":
                            query = "Muxing Output Main Multicast " + pop;
                            queries = (new string[2] { "Fiber Video Transfer Multicast IP " + pop, "Fiber Video Transfer Backup Multicast IP " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        case "dvb server":
                            query = "Muxing Output Main Multicast " + pop;
                            queries = (new string[2] { "Output Main Multicast " + pop, "Output BU Multicast " + pop });
                            if (QueryVerification(queries, AttributeByIds, valueList))
                            {
                                lockableElements.Add(graphElement);
                            }
                            break;
                        default:
                            if (graphElement != null && graphElement.OutgoingElements != null)
                            {
                                elementsToFind.AddRange(FindMinLength(graphElement.OutgoingElements));
                            }
                            break;
                    }
                }
            }
            return lockableElements;
        }

        /// <summary>
        /// finds the element which is deciding the active source
        /// </summary>
        /// <returns></returns>
        public List<GraphElement> findMinimumCommonElement()
        {
            List<GraphElement> answer = new List<GraphElement>();
            int length = Int32.MaxValue;
            if (Sources.Count == 1)
            {
                answer.Add(Sources[0]);
            }
            else if (Sources.Count > 1)
            {
                foreach (var item in graphElements)
                {
                    var name = item.CurrentElement.name;
                    var index = ServiceElementNameList[name];
                    var incomingIndexes = ElementIncomingElementIndexes[index];
                    if (item.graphLength < length && incomingIndexes != null && incomingIndexes.Count >= Sources.Count)
                    {
                        answer.Add(item);
                        length = item.graphLength;
                    }
                }
            }
            return answer;
        }
    }

}
