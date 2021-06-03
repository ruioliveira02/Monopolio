using Newtonsoft.Json;
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
        int money;
        public int Position { get; set; } //position on the board

        [JsonIgnore]
        public Player Creditor { get; set; }

        public string CreditorName { get => Creditor?.name; }
        readonly string creditorName;

        public int InJail { get; set; } //number of turns the player has been in jail
        public int GetOutOfJailFreeCards { get; set; } //wether the player has the "Get Out of Jail Free Card"

        public bool Bankrupt { get; set; } //false when the player loses

        public int Money
        {
            get => money; set
            {
                if (Creditor != null)
                {
                    int gain = value - money; //we assume, if the player is in debt, he can only gain money
                    Creditor.Money += Math.Min(gain, -money);

                    if (value >= 0)
                        Creditor = null;
                }

                money = value;
            }
        }

        public Player(string name)
        {
            if (name.Length > max_name_length)
                throw new ArgumentException("name mustn't be longer than " + max_name_length + "characters");

            if (name.IndexOfAny(control_caracters.ToCharArray()) != -1)
                throw new ArgumentException("name musn't contain any of the control characters listed");

            this.name = name;
            Money = State.initialMoney;
        }

        [JsonConstructor]
        public Player(string name, int money, int position, string creditorName,
            int inJail, int getOutOfJailFreeCards, bool bankrupt)
        {
            this.name = name;
            this.money = money;
            Position = position;
            this.creditorName = creditorName;
            InJail = inJail;
            GetOutOfJailFreeCards = getOutOfJailFreeCards;
            Bankrupt = bankrupt;
        }

        public void ResolveCreditor(Player[] players)
        {
            if (creditorName != null)
                foreach (var p in players)
                    if (p.name == creditorName)
                        Creditor = p;
        }

        //amount > 0
        //we assume that the player (this) is not currently in debt
        public void Give(int amount, Player p)
        {
            if (money < amount)
            {
                Creditor = p;
                p.Money += money;
            }
            else
                p.Money += amount;

            money -= amount;
        }
    }
}