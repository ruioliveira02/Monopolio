using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monopolio;
using NetworkModel.Responses;

namespace Monopolio_Server.Interfaces.Responses
{
    public class CardDrawResponse : Response, ICardDrawResponse
    {
        public Card Card { get; set; }

        public CardDrawResponse(Card c)
        {
            Card = c;
        }

        public override string Message()
            => string.Format("\"{0}\" card was drawn", Card.Name);
    }
}
