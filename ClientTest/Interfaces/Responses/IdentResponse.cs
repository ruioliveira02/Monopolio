using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    public class IdentResponse : Response, IIdentResponse
    {
        /// <summary>
        /// Whether the user was accepted or not
        /// </summary> 
        public bool Accepted { get; set; }

        /// <summary>
        /// The name of the user who made the request
        /// </summary> 
        public string User { get; set; }

        /// <summary>
        /// Executes the actions commanded by the response
        /// </summary>
        public override void Execute()
        {
            Console.WriteLine(string.Format("{0}: {1}", User, Accepted));
        }
    }
}
