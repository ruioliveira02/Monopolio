using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IMortgageRequest : IRequest
    {
        int Property { get; set; }
    }
}
