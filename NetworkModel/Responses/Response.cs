using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    /// <summary>
    /// Base class for all the server responses
    /// </summary> 
    public abstract class Response
    {
        /// <summary>
        /// The message to display on the server's console to log this response
        /// </summary> 
        public abstract string Message();
    }
}
