using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopolio
{
    public class Player
    {
        public const string control_caracters = "\"\n";
        public const int max_name_length = 16;

        //cannot contain any of the caracters listed above
        public readonly string name;
        public int Money { get; set; }
        public int Position { get; set; } //position on the board

        public int InJail { get; set; } //number of turns the player has been in jail
        public int GetOutOfJailFreeCards { get; set; } //wether the player has the "Get Out of Jail Free Card"
        //a jogar (falso se já perdeu)

        public Player(string name)
        {
            if (name.Length > max_name_length)
                throw new ArgumentException("name mustn't be longer than " + max_name_length + "characters");

            if (name.IndexOfAny(control_caracters.ToCharArray()) != -1)
                throw new ArgumentException("name musn't contain any of the control characters listed");

            this.name = name;
            Money = State.initial_money;
        }

        [Newtonsoft.Json.JsonConstructor]
        public Player(string name, int money, int position, int inJail, int getOutOfJailFreeCards)
        {
            this.name = name;
            Money = money;
            Position = position;
            InJail = inJail;
            GetOutOfJailFreeCards = getOutOfJailFreeCards;
        }
    }
}