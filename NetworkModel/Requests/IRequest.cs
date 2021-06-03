
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    /// <summary>
    /// Interface for a generic Request
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// The name of the client who sent the request
        /// </summary> 
        string SenderID { get; set; }

        /// <summary>
        /// Whether the request was accepted or not
        /// </summary> 
        bool Accepted { get; set; }
    }
}
