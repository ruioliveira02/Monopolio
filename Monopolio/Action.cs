using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    //represents an action a player can choose to perform. Things like paying rent after
    //landing on someone else's property is automatic, and so isn't considered an Action
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
        public readonly Type type;

        //arguments
        public readonly Player target;
        public readonly PropertyState property;
        public readonly int amount;

        public bool IsTurnAction { get => type == Type.Skip || type == Type.Buy
                || type == Type.Build; }

        public bool IsGetOutOfJail { get => type == Type.PayJailFine
                || type == Type.UseGetOutOfJailFreeCard; }

        //action must be formatted as:
        //instruction "player_arg" "property_arg" int_arg
        //(args are optional, but are expected for respective instructions)
        public Action(State state, string playerName, string action)
        {
            Player = state.GetPlayer(playerName);
            List<string> words = WordSplit(action);

            using (var word = words.GetEnumerator())
            {
                word.MoveNext();
                switch (word.Current)
                {
                    case "skip":
                        type = Type.Skip;
                        break;

                    case "buy":
                        type = Type.Buy;
                        break;

                    case "build":
                        type = Type.Build;
                        break;

                    case "pay_jail_fine":
                        type = Type.PayJailFine;
                        break;

                    case "use_get_out_of_jail_free_card":
                        type = Type.UseGetOutOfJailFreeCard;
                        break;

                    case "mortgage":
                        type = Type.Mortgage;
                        word.MoveNext();
                        property = state.GetPropertyState(word.Current);
                        break;

                    case "give":
                        type = Type.Give;
                        word.MoveNext();
                        target = state.GetPlayer(word.Current);
                        word.MoveNext();
                        amount = int.Parse(word.Current);
                        break;

                    case "give_property":
                        type = Type.Give;
                        word.MoveNext();
                        target = state.GetPlayer(word.Current);
                        word.MoveNext();
                        property = state.GetPropertyState(word.Current);
                        break;

                    case "give_get_out_of_jail_free_card":
                        type = Type.GiveGetOutOfJailFreeCard;
                        break;

                    default:
                        throw new ArgumentException("Action not recognized");
                }
            }
        }

        public override string ToString()
        {
            switch (type)
            {
                case Type.Skip:         return "skip";
                case Type.Buy:          return "buy";
                case Type.Build:        return "build_house";
                case Type.PayJailFine:  return "pay_jail_fine";
                case Type.UseGetOutOfJailFreeCard:  return "use_get_out_of_jail_free_card";
                case Type.Mortgage:     return "mortgage \"" + property.Name + "\"";
                case Type.Give:         return "give \"" + target.name + "\" " + amount;
                case Type.GiveProperty: return "give_property \"" + target.name + "\" \"" + property.Name + "\"";
                case Type.GiveGetOutOfJailFreeCard: return "give_get_out_of_jail_free_card";
                default:                return null;
            }
        }

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