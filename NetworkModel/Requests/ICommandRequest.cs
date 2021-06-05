using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface ICommandRequest : IRequest
    {
        string Command { get; set; }
    }
}
