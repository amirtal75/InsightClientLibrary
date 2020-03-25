using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace InsightClientLibrary
{
    /// <summary>
    /// Represent teh majority of the field in a service found in the insight database
    /// </summary>
    public class Service
    {
        private NLog.Logger logger;
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<string, int> AttributeNamesByIds;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<int, string> AttributeIdsByNames;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<int, ObjectAttribute> AttributeByIds;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<int, ObjectTypeAttribute> AttributeTypesByIds;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Key { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ConfluenceURL { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Status { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string BroadcastTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool ClientVPN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string UUID { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceOwner { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ContentType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceCategory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string DistributionChannel { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceGroupe { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceRole { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceRemarks { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string StartofService { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string EndofService { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ServiceDocumentationApproved { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Service(IqlApiResult service, NLog.Logger logger, string originalUUID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (service != null && service.objectEntries != null && service.objectEntries.Count > 0)
            {
                ObjectEntry selectedEntry = service.objectEntries[0];
                if (service.objectEntries.Count > 1)
                {
                    foreach (var entry in service.objectEntries)
                    {
                        string name = entry.label;
                        if (name.Length - originalUUID.Length >=0 && name.Length - originalUUID.Length <= 0)
                        {
                            selectedEntry = entry; 
                        }
                    }
                }
                AttributeNamesByIds = new Dictionary<string, int>();
                AttributeByIds = new Dictionary<int, ObjectAttribute>();
                AttributeIdsByNames = new Dictionary<int, string>();
                AttributeTypesByIds = new Dictionary<int, ObjectTypeAttribute>();
                foreach (var attribute in selectedEntry.attributes)
                {
                    AttributeByIds.Add(attribute.objectTypeAttributeId, attribute);
                }
                foreach (var typeAttribute in service.objectTypeAttributes)
                {
                    AttributeNamesByIds.Add(typeAttribute.name, typeAttribute.id);
                    AttributeIdsByNames.Add(typeAttribute.id, typeAttribute.name);
                    AttributeTypesByIds.Add(typeAttribute.id, typeAttribute);
                    bool hasThisAttribute = AttributeByIds.ContainsKey(typeAttribute.id);
                    ObjectAttribute attribute = AttributeByIds[typeAttribute.id];
                    List<ObjectAttributeValue> attributeValues = attribute.ObjectAttributeValues;

                    // if all above values are valid then set fields
                    if (typeAttribute != null && AttributeByIds != null && hasThisAttribute && attribute != null
                        && attributeValues != null && attributeValues.Count > 0 && attributeValues[0] != null)
                    {
                        switch (typeAttribute.name)
                        {
                            case "Key":
                                Key = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Name":
                                Name = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Confluence URL":
                                ConfluenceURL = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Status":
                                Status = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Broadcast Time":
                                BroadcastTime = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Client VPN":
                                ClientVPN = Convert.ToBoolean(typeAttribute.Label);
                                break;
                            case "UUID/TKSID":
                                UUID = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Owner":
                                ServiceOwner = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Content Type":
                                ContentType = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Category":
                                ServiceCategory = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Distribution Channel":
                                DistributionChannel = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Role":
                                ServiceRole = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Remarks(Customer Service)":
                                ServiceRemarks = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Start of Service":
                                StartofService = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "End of Service":
                                EndofService = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            case "Service Documentation Approved":
                                EndofService = AttributeByIds[typeAttribute.id].ObjectAttributeValues[0].displayValue;
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
        }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void InitLogger(bool debug)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

    }
    /// <summary>
    /// Class to avoid recycle code, contains useful methods
    /// </summary>
    public class Tools
    {
        public Tools()
        {
        }
        /// <summary>
        /// Method to extract list of names from an excel file
        /// </summary>
        /// <param name="excelFilenameXlsx">Name of the file</param>
        /// <param name="nameColoumn">the number of the cloumn containing the names in the excel file</param>
        /// <param name="uuidstartingRow">the rwo from which the count begins from, for instance the first row can be a subject row</param>
        /// <returns></returns>
        public static String ReadNameFromExcelFile(string excelFilenameXlsx, int nameColoumn, int uuidstartingRow)
        {
            String uuidBuffer = "";

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Path.Combine(Directory.GetCurrentDirectory(), excelFilenameXlsx), 0, true, 5, "", "", false, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            Excel._Worksheet xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            string uuid = "";
            if (xlApp == null || xlWorkbook == null || xlWorksheet == null || xlRange == null)
            {
                return "";
            }
            else
            {
                for (int i = uuidstartingRow; i <= xlRange.Rows.Count; i++)
                {

                    var val = ((Excel.Range)xlRange.Cells[i, nameColoumn]);
                    //write the value to the console
                    if (val != null && val.Value2 != null)
                    {
                        uuid = val.Value2.ToString();
                        if (uuid.Contains("+"))
                        {
                            uuid = uuid.Substring(uuid.IndexOf('+') + 1);
                        }
                        uuid = "\"" + uuid + "\"";
                        uuidBuffer += uuid;
                    }
                }
            }

            return uuidBuffer;
        }
        /// <summary>
        /// Creates a logger for a class
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static NLog.Logger InitLogger(bool debug, string className)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "Log.txt" };

            // Rules for mapping loggers to targets            
            if (debug)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            }
            else config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);


            // Apply config           
            NLog.LogManager.Configuration = config;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            //NLog.Logger logger = NLog.LogManager.GetLogger(className);
            return logger;
        }

    }
    /// <summary>
    /// Represents an element in a graph, akeen to a node in a bydirectional linked list
    /// </summary>
    public class GraphElement
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public GraphElement(ObjectEntry currentElement, int incomingLength, Dictionary<int, string> ObjectAttributeTypesById)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            this.CurrentElement = currentElement;
            this.OutgoingElements = new List<GraphElement>();
            this.IncomingElements = new List<GraphElement>();
            this.graphLength = incomingLength + 1;
            this.ObjectAttributeTypesById = ObjectAttributeTypesById;
        }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<int, string> ObjectAttributeTypesById;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectEntry CurrentElement { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<GraphElement> IncomingElements { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<GraphElement> OutgoingElements { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int graphLength { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void AddOutgoingElement(GraphElement outgoing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            OutgoingElements.Add(outgoing);
        }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void AddIncomingElement(GraphElement incoming)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            IncomingElements.Add(incoming);
            int maxLength = Math.Max(incoming.graphLength + 1, this.graphLength);
            if (maxLength > this.graphLength)
            {
                this.graphLength = maxLength;
                foreach (var item in OutgoingElements)
                {
                    item.Update();
                }
            }

        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override bool Equals(object obj)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return obj is GraphElement element &&
                CurrentElement.objectKey.Equals(element.CurrentElement.objectKey);

        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override int GetHashCode()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var hashCode = -384246513;
            hashCode = hashCode * -1521134295 + EqualityComparer<ObjectEntry>.Default.GetHashCode(CurrentElement);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<GraphElement>>.Default.GetHashCode(IncomingElements);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<GraphElement>>.Default.GetHashCode(OutgoingElements);
            return hashCode;
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Update()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            int max = this.graphLength;
            int oldLength = this.graphLength;
            // Update the graph length to the new value by calculating the max value in comparison to the incoming and outgoing elements
            foreach (var item in IncomingElements)
            {
                max = Math.Max(max, item.graphLength + 1);
            }
            // if the graph lenght was changed then we need to update all outgoing elements with the change to the new length
            if (max != oldLength)
            {
                this.graphLength = max;
                foreach (var item in OutgoingElements)
                {
                    item.Update();
                }
            }
        }
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ApplicationEntry
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string typeValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string additionalValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ConfluenceSpace
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string typeValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string additionalValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectTypeAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string additionalValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime created { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DefaultType defaultType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool hidden { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool includeChildObjectTypes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string iql { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool Label { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int maximumCardinality { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int minimumCardinality { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int ObjectTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string options { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int position { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int referenceObjectTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int referenceTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string regexValidation { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool removable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string suffix { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool summable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string typevalue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool uniqueAttribute { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime updated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ConfluenceSpace confluenceAddValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ApplicationEntry confluenceTypeValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool editable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool objectAttributeExists { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectType referenceObjectType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ReferenceType referenceType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool sortable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool system { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string typeValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectType
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Icon icon { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int position { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime created { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime updated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int parentObjectTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectSchemaId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool inherited { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool abstractObjectType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool parentObjectTypeInherited { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Icon
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url16 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url48 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class _Links
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string self { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectEntry
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string label { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string objectKey { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Avatar avatar { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectType objectType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string created { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string updated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool hasAvatar { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public long timestamp { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectAttribute[] attributes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public _Links _links { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectHistory objectHistory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Comment comment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectJiraIssue jiraIssue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectWatch objectWatch { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override bool Equals(object obj)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return obj is ObjectEntry entry &&
                   objectKey == entry.objectKey &&
                   name == entry.name;
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override int GetHashCode()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var hashCode = -343287073;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(objectKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            return hashCode;
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return objectKey + ": " + name;
        }
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectJiraIssue
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int customFieldId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int jiraIssueId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Comment
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string author { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string comment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime created { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int role { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime updated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectWatch
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string userKey { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectAttachment
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string author { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string comment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime created { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string filename { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int filesize { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string mimeType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string nameInFileSystem { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectHistory
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string ActorUserKey { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string AffectedAttribute { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string insightVersion { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string newKeyValues { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string newValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int ObjectTypeAttributeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string oldKeyValues { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string oldValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Avatar
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url16 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url48 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url72 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url144 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url288 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DefaultType
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int typeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string typeName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectTypeAttributeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<ObjectAttributeValue> ObjectAttributeValues { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool hidden { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ObjectAttributeValue
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string displayValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string additionalValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectAttributeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int referencedObjectId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ObjectEntry referencedObject { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ReferenceType
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string color { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string url116 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool removable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectSchemaId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class IqlApiResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<ObjectEntry> objectEntries { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<ObjectTypeAttribute> objectTypeAttributes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int objectTypeId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool objectTypeIsInherited { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool abstractObjectType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int totalFilterCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int startIndex { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int toIndex { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int pageObjectSize { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int pageNumber { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string orderWay { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string iql { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool iqlSearchResult { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool conversionPossible { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int pageSize { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

}
