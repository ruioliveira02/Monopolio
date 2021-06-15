using Monopolio;
using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    public class PropertyUpdateResponse : Response, IPropertyUpdateResponse
    {
        public PropertyState PropertyState { get; set; }

        public PropertyUpdateResponse(PropertyState ps)
        {
            PropertyState = ps;
        }

        public override string Message()
            => string.Format("Updated state of property {0}", PropertyState.Name);
    }
}
