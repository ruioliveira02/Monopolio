using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IMortgageResponse
    {
        string Player { get; set; }
        int Property { get; set; }
        bool HasBuilding { get; set; }
        int FinancialGain { get; set; }
    }
}
