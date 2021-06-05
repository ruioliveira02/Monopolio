using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IPayJailFineResponse : IResponse
    {
        string Player { get; set; }
        int Cost { get; set; }
    }
}
