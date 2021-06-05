using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IBuyResponse : IResponse
    {
        string Player { get; set; }
        bool Accepted { get; set; }
        int Cost { get; set; }
    }
}
