using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InsightClientLibrary
{
    /// <summary>
    /// Illegal UUID Exception Class
    /// </summary>
    public class IllegalNameException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public IllegalNameException(string uuid) : base(string.Format("The following UUID: {0} contains illegal charactes, unspported by Insight API.", uuid))
        {
        }
    }

    /// <summary>
    /// Illegal UUID Exception Class
    /// </summary>
    public class InsighClientLibraryUnknownErrorException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public InsighClientLibraryUnknownErrorException(string objectName) : base(string.Format("The following object: {0}, contained unknown behavior, fatal to the program continuity", objectName))
        {
        }
    }
    /// <summary>
    /// Exception for when the IQl result and its members are null.
    /// Such a case can happen if there is an error with the confluence route of the service due to uncorrect relationships between Inisght objects.
    /// </summary>
    public class CorruptedInsightDataException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public CorruptedInsightDataException(string uuid) : base(string.Format("The following UUID: {0} is associated with corrupted insight data, and therefore has no legal graph", uuid))
        {
        }
    }
    /// <summary>
    /// Exception to be thrown when the credentails cannot be authenticated in front of Insight database
    /// </summary>
    public class InsightUserAthenticationException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public InsightUserAthenticationException(string username) : base(string.Format("The following Username: {0} failed the insight authentication", username))
        {
        }
    }
    /// <summary>
    /// In case in the future i will want to raise duplicate element exception
    /// </summary>
    public class DuplicateElementException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public DuplicateElementException(string name) : base(string.Format("The following element: {0} is duplicated", name))
        {
        }
    }
    /// <summary>
    /// To raise when getting restsharp exception
    /// </summary>
    public class RestSharpException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public RestSharpException(string message) : base(string.Format("RestSharp Exception: ", message))
        {
        }
    }
    /// <summary>
    /// To raise when getting restsharp exception
    /// </summary>
    public class UnsuccessfullResponseException : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public UnsuccessfullResponseException(string message) : base(string.Format("Unsuccessfull Response: \n", message))
        {
        }
    }

}