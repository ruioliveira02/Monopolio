using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    /// <summary>
    /// Represents a card
    /// </summary>
    public class Card
    {
        public string Name { get; }
        public string Text { get; }
        public Event[] Events { get; }
        public bool SelfDestruct { get; } //if true, the card can only be drawn once

        /// <summary>
        /// Creates a Card based on a previously saved Card
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="name">The title of the card</param>
        /// <param name="text">The text of the card</param>
        /// <param name="events">The list of events the card sets off</param>
        /// <param name="selfDestruct">Wether the card should be removed from
        /// the deck after drawn</param>
        [Newtonsoft.Json.JsonConstructor]
        public Card(string name, string text, Event[] events, bool selfDestruct)
        {
            Name = name;
            Text = text;
            Events = events;
            SelfDestruct = selfDestruct;
        }
    }

    /// <summary>
    /// Represents a non-deterministic deck of cards, i.e. the next card to be drawn is
    /// not determined until drawn, with equal probability of any undrawn card being
    /// drawn. After all cards are drawn, the deck is shuffled and the process repeats
    /// </summary>
    public class Deck
    {
        public List<Card> Drawn { get; private set; }
        public List<Card> Undrawn { get; private set; }

        /// <summary>
        /// Creates a non-deterministic Deck of cards
        /// </summary>
        /// <param name="cards">The list of cards in the deck</param>
        public Deck(Card[] cards)
        {
            if (cards.Length == 0)
                throw new ArgumentException("cards must be non-empty.");

            Drawn = new List<Card>();
            Undrawn = new List<Card>(cards);
        }

        /// <summary>
        /// Creates a non-deterministic deck of cards from a previously saved Deck
        /// (used for JSON serialization)
        /// </summary>
        /// <param name="drawn">An array with already drawn cards</param>
        /// <param name="undrawn">An array with the undrawn cards</param>
        [Newtonsoft.Json.JsonConstructor]
        public Deck(Card[] drawn, Card[] undrawn)
        {
            Drawn = new List<Card>(drawn);
            Undrawn = new List<Card>(undrawn);
        }

        /// <summary>
        /// Draws a card from the deck. If the deck has no undrawn cards, the drawn
        /// cards are reshuffled before drawing. The drawn card, if not signaled
        /// with self-destruct, is then marked as drawn and won't be drawn until
        /// shuffling.
        /// </summary>
        /// <returns>The drawn card</returns>
        public Card Draw()
        {
            if (Undrawn.Count == 0)
                Shuffle();

            int index = State.randomizer.Next(Undrawn.Count);
            Card c = Undrawn[index];
            Undrawn.RemoveAt(index);

            if (!c.SelfDestruct)
                Drawn.Add(c);

            return c;
        }

        /// <summary>
        /// Shuffles the drawn cards into the undrawn.
        /// </summary>
        void Shuffle()
        {
            Undrawn.AddRange(Drawn);
            Drawn = new List<Card>();
        }

        /// <summary>
        /// Adds a card to the deck (it is marked as drawn, meaning it will only be
        /// drawn after shuffling)
        /// </summary>
        /// <param name="c">The card to add to the deck</param>
        public void Add(Card c) => Drawn.Add(c);
    }

    /// <summary>
    /// Represents a card event
    /// </summary>
    public struct Event
    {
        public enum EventType
        {
            //Movement
            GoToJail,           //go to jail without passing Start
            AdvanceToStart,     //advance to the Start square and receive 200
            AdvanceToStation,   //advance to the closest station
            AdvanceTo,          //advance to property named Arg (receive 200 at Start)
            Walk,               //walk X spaces forward (if X < 0, walk backwards)

            //Money
            Receive,            //player gets X money. If X < 0, player loses money
            ReceiveFromEach,    //each player gives you X. If X < 0, you give each X
            PayXRent,           //pay X times the rent of where you are (if owned)
            RepairProperty,     //pay X for each house you own and Y for each hotel

            //Get out of jail
            OutOfJailFree,      //the player receives a get-out-of-jail-free card
        }

        public EventType Type { get; }
        public string Arg { get; }
        public int X { get; }
        public int Y { get; }

        /// <summary>
        /// Creates a Card event based on a previously saved Event
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="type">The type of the event</param>
        /// <param name="arg">The string argument of the event</param>
        /// <param name="x">The first integer argument of the event</param>
        /// <param name="y">The second integer argument of the event</param>
        [Newtonsoft.Json.JsonConstructor]
        public Event(EventType type, string arg = null, int x = 0, int y = 0)
        {
            Type = type;
            Arg = arg;
            X = x;
            Y = y;
        }

        public Event(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text mustn't be null");

            List<string> words = Action.WordSplit(text);
            Arg = null;
            X = 0;
            Y = 0;

            switch (words[0])
            {
                case "go_to_jail":
                    Type = EventType.GoToJail;
                    break;

                case "advance_to_start":
                    Type = EventType.AdvanceToStart;
                    break;

                case "advance_to_station":
                    Type = EventType.AdvanceToStation;
                    break;

                case "advance_to":
                    Type = EventType.AdvanceTo;
                    Arg = words[1];
                    break;

                case "walk":
                    Type = EventType.Walk;
                    X = int.Parse(words[1]);
                    break;

                case "receive":
                    Type = EventType.Receive;
                    X = int.Parse(words[1]);
                    break;

                case "receive_from_each":
                    Type = EventType.ReceiveFromEach;
                    X = int.Parse(words[1]);
                    break;

                case "pay_x_rent":
                    Type = EventType.PayXRent;
                    X = int.Parse(words[1]);
                    break;

                case "repair_property":
                    Type = EventType.RepairProperty;
                    X = int.Parse(words[1]);
                    Y = int.Parse(words[2]);
                    break;

                case "out_of_jail_free":
                    Type = EventType.OutOfJailFree;
                    break;

                default:
                    throw new ArgumentException("Event not recognized");
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case EventType.GoToJail: return "go_to_jail";
                case EventType.AdvanceToStart: return "advance_to_start";
                case EventType.AdvanceToStation: return "advance_to_station";
                case EventType.AdvanceTo: return "advance_to " + Arg;
                case EventType.Walk: return "walk " + X;
                case EventType.Receive: return "receive " + X;
                case EventType.ReceiveFromEach: return "receive_from_each " + X;
                case EventType.PayXRent: return "pay_x_rent " + X;
                case EventType.RepairProperty: return "repair_property " + X + Y;
                case EventType.OutOfJailFree: return "out_of_jail_free";
                default: return null;
            }
        }
    }
}