using System;
using System.Collections.Generic;
using System.Text;
using Monopolio;

namespace NetworkModel.Responses
{
    interface ICardDrawResponse : IResponse
    {
        //player?

        //só título e texto?
        Card Card { get; set; }
    }
}
