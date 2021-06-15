using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    /// <summary>
    /// Represents a player action/input in the game. Paying rent and othe mandatory plays are
    /// not choices (are automaticaly performed), therefore acn't be represented as Actions
    /// </summary>
    public class Action
    {
        public enum Type
        {
            //turn actions (require that it is the player's turn)
            Skip,           //end the player's turn
            Buy,            //buy the property the player is standing on
            Build,          //build a house/hotel on target property

            //get out of jail actions (if executed in the same turn the player went to jail, his turn ends)
            PayJailFine,    //the player pays his fine and leaves the jail with the dice he rolled before.
            UseGetOutOfJailFreeCard,    //as above, but only if the player has a card (it goes back to its deck)

            //instant actions (can be performed at any time, by any player)
            Mortgage,       //return a property to the bank for half of its price. If there is a building
                            //on the property it is mortgaged instead (respecting the building rules in reverse)
            Give,           //give another player a certain amount of money
            GiveProperty,   //give another player a certain property. the property must not have any buildings
            GiveGetOutOfJailFreeCard    //you're a big boy, you can figure it out
        }

        public Player Player { get; }
        public Type type;

        //arguments
        public Player target;
        public PropertyState property;
        public int amount;

        public bool IsTurnAction
        {
            get => type == Type.Skip || type == Type.Buy || type == Type.Build;
        }

        public bool IsGetOutOfJail
        {
            get => type == Type.PayJailFine || type == Type.UseGetOutOfJailFreeCard;
        }

        //action must be formatted as:
        //instruction "player_arg" "property_arg" int_arg
        //(args are optional, but are expected for respective instructions)

        /// <summary>
        /// Creates a player action from a string
        /// </summary>
        /// <param name="state">The current game state</param>
        /// <param name="playerName">The name of the player executing the action</param>
        /// <param name="action">The action</param>
        public Action(State state, string playerName, string action)
        {
            if (action == null)
                throw new ArgumentNullException("action mustn't be null");

            Player = state.GetPlayer(playerName);

            if (Player == null)
                throw new ArgumentException("Player not recognized");

            List<string> words = WordSplit(action);

            switch (words[0])
            {
                case "skip":
                    type = Type.Skip;
                    break;

                case "buy":
                    type = Type.Buy;
                    break;

                case "build":
                    type = Type.Build;
                    property = state.GetPropertyState(words[1]);
                    if (property == null)
                        throw new ArgumentException("Property not recognized");
                    break;

                case "pay_jail_fine":
                    type = Type.PayJailFine;
                    break;

                case "use_get_out_of_jail_free_card":
                    type = Type.UseGetOutOfJailFreeCard;
                    break;

                case "mortgage":
                    type = Type.Mortgage;
                    property = state.GetPropertyState(words[1]);
                    if (property == null)
                        throw new ArgumentException("Property not recognized");
                    break;

                case "give":
                    type = Type.Give;
                    target = state.GetPlayer(words[1]);
                    amount = int.Parse(words[2]);
                    if (target == null)
                        throw new ArgumentException("Player not recognized");
                    break;

                case "give_property":
                    type = Type.Give;
                    target = state.GetPlayer(words[1]);
                    property = state.GetPropertyState(words[2]);
                    if (target == null)
                        throw new ArgumentException("Player not recognized");
                    if (property == null)
                        throw new ArgumentException("Property not recognized");
                    break;

                case "give_get_out_of_jail_free_card":
                    type = Type.GiveGetOutOfJailFreeCard;
                    target = state.GetPlayer(words[1]);
                    if (target == null)
                        throw new ArgumentException("Player not recognized");
                    break;

                default:
                    throw new ArgumentException("Action not recognized");
            }
        }

        /// <summary>
        /// Converts the action to a string, effectively reversing the constructor
        /// </summary>
        /// <returns>The action, as a string</returns>
        public override string ToString()
        {
            switch (type)
            {
                case Type.Skip: return "skip";
                case Type.Buy: return "buy";
                case Type.Build: return "build_house";
                case Type.PayJailFine: return "pay_jail_fine";
                case Type.UseGetOutOfJailFreeCard: return "use_get_out_of_jail_free_card";
                case Type.Mortgage: return "mortgage \"" + property.Name + "\"";
                case Type.Give: return "give \"" + target.name + "\" " + amount;
                case Type.GiveProperty: return "give_property \"" + target.name + "\" \"" + property.Name + "\"";
                case Type.GiveGetOutOfJailFreeCard: return "give_get_out_of_jail_free_card \"" + target.name + "\"";
                default: return null;
            }
        }

        /// <summary>
        /// Splits the string into words, keeping the spaces inside quotes.
        /// Ex: 'give_property "bace" "Castelo do Queijo"' turns into [ 'give_property', 'bace',
        /// 'Castelo do Queijo' ]
        /// Throws an error if unclosed quotes are found
        /// </summary>
        /// <param name="s">The string to split</param>
        /// <returns>The list of words</returns>
        public static List<string> WordSplit(string s)
        {
            List<string> ans = new List<string>();
            bool quotes = false;
            int prev = 0;
            s = s.Trim();

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '"')
                {
                    quotes = !quotes;
                    prev = i + 1;
                }
                else if (!quotes)
                {
                    if (s[i] == ' ' && s[prev] != ' ')
                    {
                        ans.Add(s.Substring(prev, i - prev));
                        prev = i;
                    }
                    else if (s[prev] == ' ')
                        prev = i;
                }
            }

            if (quotes)
                throw new ArgumentException("Invalid string (unclosed quotes)");

            if (s[prev] != ' ')
                ans.Add(s.Substring(prev, s.Length - prev));

            return ans;
        }
    }
}