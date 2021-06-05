using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    interface IBuildResponse : IResponse //TODO :: Ver o que meter aqui
    {
        string Player { get; set; }
        int Property { get; set; }
        bool Accepted { get; set; }
        int Cost { get; set; }
    }
}
