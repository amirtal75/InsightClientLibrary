using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Excel = Microsoft.Office.Interop.Excel;

namespace InsightClientLibrary
{
    /// <summary>
    /// Represent the majority of the field in a service found in the insight database
    /// </summary>
    public class Service
    {
        private NLog.Logger logger;
        /// <summary> </summary>
        public Dictionary<string, int> AttributeNamesByIds;

        /// <summary> </summary>
        public Dictionary<int, string> AttributeIdsByNames;

        /// <summary> </summary>
        public Dictionary<int, ObjectAttribute> AttributeByIds;

        /// <summary> </summary>
        public Dictionary<int, ObjectTypeAttribute> AttributeTypesByIds;

        /// <summary> </summary>
        public string Key { get; set; }

        /// <summary> </summary>
        public string Name { get; set; }

        /// <summary> </summary>
        public string ConfluenceURL { get; set; }

        /// <summary> </summary>
        public string Status { get; set; }

        /// <summary> </summary>
        public string BroadcastTime { get; set; }

        /// <summary> </summary>
        public bool ClientVPN { get; set; }

        /// <summary> </summary>
        public string UUID { get; set; }

        /// <summary> </summary>
        public string ServiceOwner { get; set; }

        /// <summary> </summary>
        public string ContentType { get; set; }

        /// <summary> </summary>
        public string ServiceCategory { get; set; }

        /// <summary> </summary>
        public string DistributionChannel { get; set; }

        /// <summary> </summary>
        public string ServiceGroupe { get; set; }

        /// <summary> </summary>
        public string ServiceRole { get; set; }

        /// <summary> </summary>
        public string ServiceRemarks { get; set; }

        /// <summary> </summary>
        public string StartofService { get; set; }

        /// <summary> </summary>
        public string EndofService { get; set; }

        /// <summary> </summary>
        public string ServiceDocumentationApproved { get; set; }


        /// <summary> </summary>
        public Service(ObjectEntry selectedEntry, List<ObjectTypeAttribute> objectTypeAttributes, string originalUUID)

        {
            AttributeNamesByIds = new Dictionary<string, int>();
            AttributeByIds = new Dictionary<int, ObjectAttribute>();
            AttributeIdsByNames = new Dictionary<int, string>();
            AttributeTypesByIds = new Dictionary<int, ObjectTypeAttribute>();
            foreach (var attribute in selectedEntry.attributes)
            {
                AttributeByIds.Add(attribute.objectTypeAttributeId, attribute);
            }
            foreach (var typeAttribute in objectTypeAttributes)
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
    /// <summary>
    /// Class to avoid recycle code, contains useful methods
    /// </summary>
    public class Tools
    {
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
                        uuidBuffer += uuid + "\n";
                    }
                }
            }
            xlWorkbook.Close(0);
            xlApp.Quit();
            return uuidBuffer;
        }
        /// <summary>
        /// Check if the IQl result and its members are not null.
        /// Such a case can happen if there is an error with the confluence route of the service due to uncorrect relationships between Inisght objects.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsValidIqlResult(IqlApiResult result)
        {
            return (result != null && (result.objectEntries != null) && (result.objectTypeAttributes != null));
        }
        /// <summary>
        /// This function will be used when the uuid was modified due to the presence of illegal Insight APi symbols and the iql service query returned more than one object entry.
        /// meaning the uuid modified caused loss of uniquencess and returned multiple matching services.
        ///this function removes unrelated services and elements in order to construct a correct graph.
        /// </summary>
        /// <param name="originalUUID"></param>
        /// <param name="serviceResult"></param>
        /// <param name="elementResult"></param>
        public static void UniquenessLostFix(string originalUUID, ref IqlApiResult serviceResult, ref IqlApiResult elementResult)
        {
            List<Service> services = new List<Service>();
            List<ObjectEntry> CorrectService = new List<ObjectEntry>();
            foreach (var entry in serviceResult.objectEntries)
            {
                services.Add(new Service(entry, serviceResult.objectTypeAttributes, ""));
            }
            for (int i = 0; i < serviceResult.objectEntries.Count; i++)
            {
                if (serviceResult.objectEntries[i].name.Equals(originalUUID))
                {
                    CorrectService.Add(serviceResult.objectEntries[i]);
                    break;
                }
            }
            serviceResult.objectEntries = CorrectService;


            List<ObjectEntry> elementEntries = new List<ObjectEntry>();
            foreach (var entry in elementResult.objectEntries)
            {
                foreach (var attribute in entry.attributes)
                {
                    if (attribute.objectTypeAttributeId == 1781 && attribute.ObjectAttributeValues != null && attribute.ObjectAttributeValues.Count > 0)
                    {
                        foreach (var attributeValue in attribute.ObjectAttributeValues)
                        {
                            if (attributeValue.displayValue.Equals(originalUUID))
                            {
                                elementEntries.Add(entry);
                            }
                        }
                    }
                }
            }

            elementResult.objectEntries = elementEntries;
        }
        /// <summary>
        /// Checks the given name for illegal Insight API symbols.
        /// </summary>
        /// <param name="name">the name to check for valid characters</param>
        /// <param name="forbiddenInsightApiQuerySymbols">the the insight API forbideen symbols</param>
        /// <returns>modified name with no illegal characters, will be an empty string in case too many invalid characters exist</returns>
        public static string ModifyUnspportedInsightNameConvention(string name, string forbiddenInsightApiQuerySymbols)
        {
            string nameToModify = name;
            bool hasForbidden = true;

            while (hasForbidden)
            {
                foreach (char forbiddenSymbol in forbiddenInsightApiQuerySymbols)
                {
                    if (nameToModify.Contains(forbiddenSymbol.ToString()))
                    {
                        int forbiddenIndex = nameToModify.IndexOf(forbiddenSymbol);
                        if (forbiddenIndex < nameToModify.Length - 1)
                        {
                            nameToModify = nameToModify.Substring(forbiddenIndex + 1);
                        }
                        else nameToModify = "";
                    }
                }
                hasForbidden = false;
                foreach (char ch in nameToModify)
                {
                    if (forbiddenInsightApiQuerySymbols.Contains(ch.ToString()))
                    {
                        hasForbidden = true;
                    }
                }
            }
            if (nameToModify.Equals(""))
            {
                throw new IllegalNameException(name);
            }
            else
            {
                if (nameToModify.Contains(" "))
                {
                    var s = '"'.ToString();
                    nameToModify = s + nameToModify + s;
                }
                return nameToModify;
            }

        }
        /// <summary>
        /// Checks if the given string is a legal IPV4 adress.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool LegalIPV4(string ip)
        {
            bool answer = true;
            // check that the string is of the form XXX.XXX.XXX.XXX
            if (!ip.Contains("."))
            {
                return false;
            }
            string[] octats = ip.Split('.');
            // check if each octat contains a maximum of three number characters
            for (int i = 0; i < octats.Length; i++)
            {
                if (octats[i].Length > 3)
                {
                    return false;
                }
                else
                {
                    foreach (char ch in octats[i])
                    {
                        if (ch < '0' || ch > '9')
                        {
                            return false;
                        }
                    }
                }
            }
            return answer;
        }

    }
    /// <summary>
    /// Represents an element in a graph, akeen to a node in a bydirectional linked list
    /// </summary>
    public class GraphElement
    {
        /// <summary> </summary>
        public GraphElement(ObjectEntry currentElement, int incomingLength, Dictionary<int, string> ObjectAttributeTypesById)
        {
            this.CurrentElement = currentElement;
            this.OutgoingElements = new List<GraphElement>();
            this.IncomingElements = new List<GraphElement>();
            this.graphLength = incomingLength + 1;
            this.ObjectAttributeTypesById = ObjectAttributeTypesById;
        }
        /// <summary> </summary>
        public Dictionary<int, string> ObjectAttributeTypesById;

        /// <summary> </summary>
        public ObjectEntry CurrentElement { get; set; }

        /// <summary> </summary>
        public List<GraphElement> IncomingElements { get; set; }

        /// <summary> </summary>
        public List<GraphElement> OutgoingElements { get; set; }

        /// <summary> </summary>
        public int graphLength { get; set; }

        /// <summary> </summary>
        public void AddOutgoingElement(GraphElement outgoing)

        {
            OutgoingElements.Add(outgoing);
        }
        /// <summary> </summary>
        public void AddIncomingElement(GraphElement incoming)

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

        /// <summary> </summary>
        public override bool Equals(object obj)

        {
            return obj is GraphElement element &&
                CurrentElement.objectKey.Equals(element.CurrentElement.objectKey);

        }

        /// <summary> </summary>
        public override int GetHashCode()

        {
            var hashCode = -384246513;
            hashCode = hashCode * -1521134295 + EqualityComparer<ObjectEntry>.Default.GetHashCode(CurrentElement);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<GraphElement>>.Default.GetHashCode(IncomingElements);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<GraphElement>>.Default.GetHashCode(OutgoingElements);
            return hashCode;
        }

        /// <summary> </summary>
        public void Update()

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
    /// <summary> </summary>
    public class ApplicationEntry

    {
        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public string type { get; set; }

        /// <summary> </summary>
        public string typeValue { get; set; }

        /// <summary> </summary>
        public string additionalValue { get; set; }

    }
    /// <summary> </summary>
    public class ConfluenceSpace

    {
        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public string type { get; set; }

        /// <summary> </summary>
        public string typeValue { get; set; }

        /// <summary> </summary>
        public string additionalValue { get; set; }

    }
    /// <summary> </summary>
    public class ObjectTypeAttribute

    {
        /// <summary> </summary>
        public string additionalValue { get; set; }

        /// <summary> </summary>
        public DateTime created { get; set; }

        /// <summary> </summary>
        public DefaultType defaultType { get; set; }

        /// <summary> </summary>
        public string description { get; set; }

        /// <summary> </summary>
        public bool hidden { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public bool includeChildObjectTypes { get; set; }

        /// <summary> </summary>
        public string iql { get; set; }

        /// <summary> </summary>
        public bool Label { get; set; }

        /// <summary> </summary>
        public int maximumCardinality { get; set; }

        /// <summary> </summary>
        public int minimumCardinality { get; set; }

        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public int ObjectTypeId { get; set; }

        /// <summary> </summary>
        public string options { get; set; }

        /// <summary> </summary>
        public int position { get; set; }

        /// <summary> </summary>
        public int referenceObjectTypeId { get; set; }

        /// <summary> </summary>
        public int referenceTypeId { get; set; }

        /// <summary> </summary>
        public string regexValidation { get; set; }

        /// <summary> </summary>
        public bool removable { get; set; }

        /// <summary> </summary>
        public string suffix { get; set; }

        /// <summary> </summary>
        public bool summable { get; set; }

        /// <summary> </summary>
        public int type { get; set; }

        /// <summary> </summary>
        public string typevalue { get; set; }

        /// <summary> </summary>
        public bool uniqueAttribute { get; set; }

        /// <summary> </summary>
        public DateTime updated { get; set; }

        /// <summary> </summary>
        public ConfluenceSpace confluenceAddValue { get; set; }

        /// <summary> </summary>
        public ApplicationEntry confluenceTypeValue { get; set; }

        /// <summary> </summary>
        public bool editable { get; set; }

        /// <summary> </summary>
        public bool objectAttributeExists { get; set; }

        /// <summary> </summary>
        public ObjectType referenceObjectType { get; set; }

        /// <summary> </summary>
        public ReferenceType referenceType { get; set; }


        /// <summary> </summary>
        public bool sortable { get; set; }


        /// <summary> </summary>
        public bool system { get; set; }


        /// <summary> </summary>
        public string typeValue { get; set; }


    }
    /// <summary> </summary>
    public class ObjectType

    {
        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public int type { get; set; }

        /// <summary> </summary>
        public Icon icon { get; set; }

        /// <summary> </summary>
        public int position { get; set; }

        /// <summary> </summary>
        public DateTime created { get; set; }

        /// <summary> </summary>
        public DateTime updated { get; set; }

        /// <summary> </summary>
        public int objectCount { get; set; }

        /// <summary> </summary>
        public int parentObjectTypeId { get; set; }

        /// <summary> </summary>
        public int objectSchemaId { get; set; }

        /// <summary> </summary>
        public bool inherited { get; set; }

        /// <summary> </summary>
        public bool abstractObjectType { get; set; }

        /// <summary> </summary>
        public bool parentObjectTypeInherited { get; set; }

        /// <summary> </summary>
        public string description { get; set; }


    }
    /// <summary> </summary>
    public class Icon

    {
        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public string url16 { get; set; }

        /// <summary> </summary>
        public string url48 { get; set; }

    }
    /// <summary> </summary>
    public class _Links

    {
        /// <summary> </summary>
        public string self { get; set; }

    }
    /// <summary> </summary>
    public class ObjectEntry

    {
        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int objectTypeId { get; set; }

        /// <summary> </summary>
        public string label { get; set; }

        /// <summary> </summary>
        public string objectKey { get; set; }

        /// <summary> </summary>
        public Avatar avatar { get; set; }

        /// <summary> </summary>
        public ObjectType objectType { get; set; }

        /// <summary> </summary>
        public string created { get; set; }

        /// <summary> </summary>
        public string updated { get; set; }

        /// <summary> </summary>
        public bool hasAvatar { get; set; }

        /// <summary> </summary>
        public long timestamp { get; set; }

        /// <summary> </summary>
        public ObjectAttribute[] attributes { get; set; }

        /// <summary> </summary>
        public _Links _links { get; set; }

        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public ObjectHistory objectHistory { get; set; }

        /// <summary> </summary>
        public Comment comment { get; set; }

        /// <summary> </summary>
        public ObjectJiraIssue jiraIssue { get; set; }

        /// <summary> </summary>
        public ObjectWatch objectWatch { get; set; }


        /// <summary> </summary>
        public override bool Equals(object obj)

        {
            return obj is ObjectEntry entry &&
                   objectKey == entry.objectKey &&
                   name == entry.name;
        }

        /// <summary> </summary>
        public override int GetHashCode()

        {
            var hashCode = -343287073;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(objectKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            return hashCode;
        }

        /// <summary> </summary>
        public override string ToString()

        {
            return objectKey + ": " + name;
        }
    }
    /// <summary> </summary>
    public class ObjectJiraIssue

    {
        /// <summary> </summary>
        public int customFieldId { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int jiraIssueId { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

    }
    /// <summary> </summary>
    public class Comment

    {
        /// <summary> </summary>
        public string author { get; set; }

        /// <summary> </summary>
        public string comment { get; set; }

        /// <summary> </summary>
        public DateTime created { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int role { get; set; }

        /// <summary> </summary>
        public DateTime updated { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

    }
    /// <summary> </summary>
    public class ObjectWatch

    {
        /// <summary> </summary>
        public string userKey { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

    }
    /// <summary> </summary>
    public class ObjectAttachment

    {
        /// <summary> </summary>
        public string author { get; set; }

        /// <summary> </summary>
        public string comment { get; set; }

        /// <summary> </summary>
        public DateTime created { get; set; }

        /// <summary> </summary>
        public string filename { get; set; }

        /// <summary> </summary>
        public int filesize { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public string mimeType { get; set; }

        /// <summary> </summary>
        public string nameInFileSystem { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

    }
    /// <summary> </summary>
    public class ObjectHistory

    {
        /// <summary> </summary>
        public string ActorUserKey { get; set; }

        /// <summary> </summary>
        public string AffectedAttribute { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public string insightVersion { get; set; }

        /// <summary> </summary>
        public string newKeyValues { get; set; }

        /// <summary> </summary>
        public string newValue { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

        /// <summary> </summary>
        public int ObjectTypeAttributeId { get; set; }

        /// <summary> </summary>
        public string oldKeyValues { get; set; }

        /// <summary> </summary>
        public string oldValue { get; set; }

        /// <summary> </summary>
        public int type { get; set; }

    }
    /// <summary> </summary>
    public class Avatar

    {
        /// <summary> </summary>
        public string url16 { get; set; }

        /// <summary> </summary>
        public string url48 { get; set; }

        /// <summary> </summary>
        public string url72 { get; set; }

        /// <summary> </summary>
        public string url144 { get; set; }

        /// <summary> </summary>
        public string url288 { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

    }
    /// <summary> </summary>
    public class DefaultType

    {
        /// <summary> </summary>
        public int typeId { get; set; }

        /// <summary> </summary>
        public string typeName { get; set; }

    }
    /// <summary> </summary>
    public class ObjectAttribute

    {
        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int objectId { get; set; }

        /// <summary> </summary>
        public int objectTypeAttributeId { get; set; }

        /// <summary> </summary>
        public List<ObjectAttributeValue> ObjectAttributeValues { get; set; }

        /// <summary> </summary>
        public bool hidden { get; set; }

    }

    /// <summary> </summary>
    public class ObjectAttributeValue

    {
        /// <summary> </summary>
        public string value { get; set; }

        /// <summary> </summary>
        public string displayValue { get; set; }

        /// <summary> </summary>
        public string additionalValue { get; set; }

        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public int objectAttributeId { get; set; }

        /// <summary> </summary>
        public int referencedObjectId { get; set; }

        /// <summary> </summary>
        public ObjectEntry referencedObject { get; set; }


    }
    /// <summary> </summary>
    public class ReferenceType

    {
        /// <summary> </summary>
        public int id { get; set; }

        /// <summary> </summary>
        public string name { get; set; }

        /// <summary> </summary>
        public string color { get; set; }

        /// <summary> </summary>
        public string url116 { get; set; }

        /// <summary> </summary>
        public bool removable { get; set; }

        /// <summary> </summary>
        public int objectSchemaId { get; set; }

    }
    /// <summary> </summary>
    public class IqlApiResult

    {
        /// <summary> </summary>
        public List<ObjectEntry> objectEntries { get; set; }

        /// <summary> </summary>
        public List<ObjectTypeAttribute> objectTypeAttributes { get; set; }

        /// <summary> </summary>
        public int objectTypeId { get; set; }

        /// <summary> </summary>
        public bool objectTypeIsInherited { get; set; }

        /// <summary> </summary>
        public bool abstractObjectType { get; set; }

        /// <summary> </summary>
        public int totalFilterCount { get; set; }

        /// <summary> </summary>
        public int startIndex { get; set; }

        /// <summary> </summary>
        public int toIndex { get; set; }

        /// <summary> </summary>
        public int pageObjectSize { get; set; }

        /// <summary> </summary>
        public int pageNumber { get; set; }

        /// <summary> </summary>
        public string orderWay { get; set; }

        /// <summary> </summary>
        public string iql { get; set; }

        /// <summary> </summary>
        public bool iqlSearchResult { get; set; }

        /// <summary> </summary>
        public bool conversionPossible { get; set; }

        /// <summary> </summary>
        public int pageSize { get; set; }

    }

}