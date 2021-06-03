using Network.Attributes;
using Network.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    /// <summary>
    /// Interface for the Identification Responses from the server
    /// </summary>
    public interface IIdentResponse
    {
        /// <summary>
        /// Whether the user was accepted or not
        /// </summary> 
        bool Accepted { get; set; }

        /// <summary>
        /// The name of the user who made the request
        /// </summary> 
        string User { get; set; }
    }
}
