using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IChatResponse : IResponse
    {
        string Player { get; set; }
        string Msg { get; set; }
    }
}
