using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    /// <summary>
    /// Base class for all the server responses
    /// </summary> 
    public abstract class Response : IResponse
    {
        /// <summary>
        /// The message to display on the server's console to log this response
        /// </summary> 
        public abstract string Message();
    }
}
