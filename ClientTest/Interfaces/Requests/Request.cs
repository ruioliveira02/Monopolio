using NetworkModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    public abstract class Request : IRequest
    {
        /// <summary>
        /// The name of the sender of the request
        /// </summary> 
        public string SenderID { get; set; }

        /// <summary>
        /// Whether or not the request was accepted
        /// </summary> 
        public bool Accepted { get; set; }

        /// <summary>
        /// Returns the JSON string of the class
        /// 
        /// <returns>The JSON string of the class</returns>
        /// </summary> 
        public virtual string ToJSON()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
