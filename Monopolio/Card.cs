﻿using System;
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

        public void Add(Card c) => undrawn.Add(c);
    }

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

            //Anti-Prision
            OutOfJailFree,      //the player receives a get-out-of-jail-free card
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