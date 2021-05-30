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
            //turn actions (require that it is the player's turn and finish the turn on successful completion)
            Skip,           //skips the player's turn
            Buy,            //buy the property the player is standing on
            Build,          //build a house/hotel on the property the player is standing on

            //has his own category (he's kind of a loner)
            PayJailFine,    //the player pays his fine and leaves the jail with the dice he rolled before.
                            //if executed in the same turn the player was sent to jail it terminates his turn

            //instant actions (can be performed at any time, by any player)
            Mortgage,       //return a property to the bank for half of its price. If there is a building
                            //on the property it is mortgaged instead (respecting the building rules in reverse)
            Give,           //give another player a certain amount of money
            GiveProperty,   //give another player a certain property. the property must not have any buildings
            //GiveGetOutOfJail
        }

        public Player Player { get; }
        public readonly Type type;

        //arguments
        public readonly Player target;
        public readonly PropertyState property;
        public readonly int amount;

        public bool IsTurnAction { get => type < Type.Give; }

        //turn actions that require the player to be on a property space
        public bool IsPropertyAction { get => IsTurnAction && type >= Type.Buy; }

        //action must be formatted as:
        //instruction "player_arg" "property_arg" int_arg
        //(args are optional, but are expected for respective instructions)
        public Action(State state, string playerName, string action)
        {
            Player = state.GetPlayer(playerName);
            List<string> words = WordSplit(action);

            using (var word = words.GetEnumerator())
            {
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
                case Type.Mortgage:     return "mortgage \"" + property.Name + "\"";
                case Type.Give:         return "give \"" + target.name + "\" " + amount;
                case Type.GiveProperty: return "give_property \"" + target.name + "\" \"" + property.Name + "\"";
                default:                return null;
            }
        }

        public List<string> WordSplit(string s)
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
                        ans.Add(s.Substring(prev, i));
                        prev = i;
                    }
                    else if (s[prev] == ' ')
                        prev = i;
                }
            }

            if (quotes)
                throw new ArgumentException("Invalid string (unclosed quotes)");

            if (s[prev] != ' ')
                ans.Add(s.Substring(prev, s.Length));

            return ans;
        }
    }
}