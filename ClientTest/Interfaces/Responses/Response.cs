using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    /// <summary>
    /// Client side implementation of server responses
    /// </summary>
    public abstract class Response : IResponse
    {
        /// <summary>
        /// Executes the actions commanded by the response
        /// </summary>
        public abstract void Execute();
    }
}
