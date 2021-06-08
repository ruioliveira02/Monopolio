using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopolio
{
    /// <summary>
    /// Stores all the information about a player (position, money, etc),
    /// excluding properties
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Caracters not allowed in a Player's name
        /// </summary>
        public const string control_caracters = "\"\n";

        /// <summary>
        /// Maximum length of a player's name
        /// </summary>
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

        public bool Bankrupt { get; set; } //true when the player loses

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

        /// <summary>
        /// Creates a new Player object
        /// </summary>
        /// <param name="name">The player's name</param>
        public Player(string name)
        {
            if (name.Length > max_name_length)
                throw new ArgumentException("name mustn't be longer than " + max_name_length + "characters");

            if (name.IndexOfAny(control_caracters.ToCharArray()) != -1)
                throw new ArgumentException("name musn't contain any of the control characters listed");

            this.name = name;
            Money = State.initialMoney;
        }

        /// <summary>
        /// Creates a Player object from a previously saved Player
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="name">The player's name</param>
        /// <param name="money">The player's money</param>
        /// <param name="position">The player's position on the board</param>
        /// <param name="creditorName">The name of the player this player is owing</param>
        /// <param name="inJail">The number of turns the player has been in jail</param>
        /// <param name="getOutOfJailFreeCards">How many get-out-of-jail-free cards the player has</param>
        /// <param name="bankrupt">Wether the player has already lost the game</param>
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

        /// <summary>
        /// Sets the Creditor looking for its name among all players
        /// </summary>
        /// <param name="players">The array with all players</param>
        public void ResolveCreditor(Player[] players)
        {
            if (creditorName != null)
                foreach (var p in players)
                    if (p.name == creditorName)
                        Creditor = p;
        }

        //we assume that the player (this) is not currently in debt
        /// <summary>
        /// Transfers the specified amount of money to another player
        /// It is assumed the player (this) is not in debt (his Money is non-negative)
        /// </summary>
        /// <param name="amount">The specified amount to transfer. Must be non-negative</param>
        /// <param name="p">The player to receive the transfer</param>
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