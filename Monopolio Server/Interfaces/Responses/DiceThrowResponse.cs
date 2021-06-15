using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    public class DiceThrowResponse : Response, IDiceThrowResponse
    {
        public int[] Dice { get; set; }

        public DiceThrowResponse(int[] dice)
        {
            if (dice.Length != 2)
                throw new ArgumentException("dice must be length 2");

            foreach (int d in dice)
                if (d <= 0 || d > 6)
                    throw new ArgumentException("Each dice must be between 1 and 6");

            Dice = dice;
        }

        public override string Message()
            => string.Format("Dice roll: {0} {1}", Dice[0], Dice[1]);
    }
}
