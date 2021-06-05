using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    interface ISkipResponse : IResponse
    {
        string NextPlayer { get; set; }
    }
}
