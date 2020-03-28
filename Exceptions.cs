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
        public IllegalNameException(string uuid ): base(string.Format("The following UUID: {0} contains illegal charactes, unspported by Insight API.",uuid))
        {
        }
    }

    /// <summary>
    /// Illegal UUID Exception Class
    /// </summary>
    public class InsighClientLibraryUnknownError : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public InsighClientLibraryUnknownError(string objectName) : base(string.Format("The following object: {0}, contained unknown behavior, fatal to the program continuity", objectName))
        {
        }
    }
    /// <summary>
    /// Exception for when the IQl result and its members are null.
    /// Such a case can happen if there is an error with the confluence route of the service due to uncorrect relationships between Inisght objects.
    /// </summary>
    public class CorruptedInsightData : Exception
    {
        /// <summary>
        /// Constructs exception for unsupported UUID convention
        /// </summary>
        public CorruptedInsightData(string uuid) : base(string.Format("The following UUID: {0} is associated with corrupted insight data, and therefore has no legal graph", uuid))
        {
        }
    }

}
