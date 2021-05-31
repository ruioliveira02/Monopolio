using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    public class Card
    {
        public string Name { get; }
        public string Text { get; }
        public Event[] Events { get; }
        public bool SelfDestruct { get; } //if true, the card can only be drawn once

        [Newtonsoft.Json.JsonConstructor]
        public Card(string name, string text, Event[] events, bool selfDestruct)
        {
            Name = name;
            Text = text;
            Events = events;
            SelfDestruct = selfDestruct;
        }
    }

    public class Deck
    {
        List<Card> drawn;
        List<Card> undrawn;

        public Deck(Card[] cards)
        {
            if (cards.Length == 0)
                throw new ArgumentException("cards must be non-empty.");

            drawn = new List<Card>();
            undrawn = new List<Card>(cards);
        }

        [Newtonsoft.Json.JsonConstructor]
        public Deck(Card[] drawn, Card[] undrawn)
        {
            this.drawn = new List<Card>(drawn);
            this.undrawn = new List<Card>(undrawn);
        }

        public Card Draw()
        {
            if (undrawn.Count == 0)
                Shuffle();

            int index = State.randomizer.Next(undrawn.Count);
            Card c = undrawn[index];
            undrawn.RemoveAt(index);

            if (!c.SelfDestruct)
                drawn.Add(c);

            return c;
        }

        void Shuffle()
        {
            undrawn = drawn;
            drawn = new List<Card>();
        }
    }

    public struct Event
    {
        public enum EventType
        {
            //Movement
            GoToJail,       //go to jail without passing Start
            AdvanceToStart, //advance to the Start square and receive 200
            AdvanceTo,      //advance to property named Arg (receive 200 passing Start)

            //Money
            Receive,        //player gets X money. If X is negative, player loses money
            ReceiveFromEach,//each player gives you X money. If X < 0, you give each X
            PayDoubleRent,  //pay twice the rent of the square you are on (if owned)
            RepairProperty, //pay X for each house on your property and Y for each hotel
        }

        public EventType Type { get; }
        public string Arg { get; }
        public int X { get; }
        public int Y { get; }

        public Event(EventType type, string arg, int x = 0, int y = 0)
        {
            Type = type;
            Arg = arg;
            X = x;
            Y = y;
        }
    }
}