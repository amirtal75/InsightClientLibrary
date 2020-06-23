# InsightClientLibrary
This repository represents a part of a multi system automation project.
This specific code contains a client for communicating and extracting information from Atlassian Jira database (Insight).
There are several levels of data extraction meant for two levels of users:
* User with no programming exprience to get pre-processed determined logical implementations of the data.
* User with SQL knowledge to perform custom SQL queries in fron of Insight API.

In addition to the above, the graph class was written create a graph describing a real time transmission chain with equipments related to Radio, Video and Network transmissions, and apply required logic and data extraction according to custom operational requirments.

The second part of the project was written in C# directly to an operational Skyline Dataminer system which communicates with various equipments over SNMP/SNMPV2 protocols.
Various design patterns were implemented to process real time information, gather statistical information and automate common user actions. 
