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

        [JsonIgnore]    //is marked as false after every execute cycle
        public bool updated;

        private int money;
        private int _position;  //position on the board
        private int _inJail;    //number of turns the player has been in jail
        private int _cards;     //the number of get-out-of-jail-free cards the player has
        private bool _bankrupt; //true when the player loses

        [JsonIgnore]
        public Player Creditor { get; set; }

        public string CreditorName { get => Creditor?.name; }
        readonly string creditorName;

        #region getters

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

                if (money != value)
                    updated = true;
                money = value;
            }
        }

        public int Position { get => _position; set {
                if (_position != value)
                    updated = true;
                _position = value;
            } }

        public int InJail { get => _inJail; set {
                if (_inJail != value)
                    updated = true;
                _inJail = value;
            } }

        public int GetOutOfJailFreeCards { get => _cards; set {
                if (_cards != value)
                    updated = true;
                _cards = value;
            } }

        public bool Bankrupt { get => _bankrupt; set {
                if (_bankrupt != value)
                    updated = true;
                _bankrupt = value;
            } }

        #endregion

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
            money = State.initialMoney;
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
            _position = position;
            this.creditorName = creditorName;
            _inJail = inJail;
            _cards = getOutOfJailFreeCards;
            _bankrupt = bankrupt;
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

            if (amount != 0)
                updated = true;
        }
    }
}