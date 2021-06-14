using System;
using System.Collections.Generic;
using System.Text;
using Monopolio;

namespace NetworkModel.Responses
{
    interface IPropertyUpdateResponse : IResponse
    {
        PropertyState PropertyState { get; set; }
    }
}
