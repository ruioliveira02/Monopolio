using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Monopolio
{

    public class State
    {
        public const int initial_money = 1500;
        public const int salary = 200;
        public const int jailFine = 50;

        public static Random randomizer = new Random();

        public static State LoadState(string file)
        {
            string json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<State>(json);
        }


        public Board board { get; } //the board "template"
        public PropertyGroup[] Groups { get; }

        public Player[] Players { get; }
        public int Turn { get; private set; } //which player is currently playing
        public bool RepeatTurn { get; private set; }
        public int RepeatedTurns { get; private set; }

        public int[] Dice { get; } //2-long array with integers from 1 to 6

        public int MiddleMoney { get; private set; }
        public Deck Chance { get; }
        public Deck CommunityChest { get; }


        public State(string[] players) //new game
        {
            Players = new Player[players.Length];

            for (int i = 0; i < players.Length; i++)
                Players[i] = new Player(players[i]);

            board = Board.LoadBoard("default_board.txt");
            Groups = board.GetPropertyGroups();
            Turn = -1;

            Dice = new int[2];
            Dice[0] = 1;
            Dice[1] = 1;
            Chance = new Deck(board.Chance);
            CommunityChest = new Deck(board.CommunityChest);
        }

        [JsonConstructor]
        public State(Board board, PropertyGroup[] groups, Player[] players, int turn,
            bool repeatTurn, int repeatedTurns, int[] dice, int middleMoney,
            Deck chance, Deck communityChest) //saved game
        {
            this.board = board;
            Groups = groups;
            Players = players;
            Turn = turn;
            RepeatTurn = repeatTurn;
            RepeatedTurns = repeatedTurns;
            Dice = dice;
            MiddleMoney = middleMoney;
            Chance = chance;
            CommunityChest = communityChest;

            foreach (var a in Groups)
            {
                foreach (var b in a.properties)
                {
                    b.ResolveOwner(Players);
                    b.ResolveProperty(board);
                }
            }
        }


        public void Save(string file)
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(file, json);
        }

        public PropertyState GetPropertyState(Property property)
            => Groups[(int)property.color].GetPropertyState(property);


        public PropertyState GetPropertyState(string propertyName)
        {
            foreach (var g in Groups)
                foreach (var s in g.properties)
                    if (s.Property.name == propertyName)
                        return s;

            return null;
        }

        public Player GetPlayer(string playerName)
        {
            foreach (Player p in Players)
                if (p.name == playerName)
                    return p;

            return null;
        }


        //returns true when 3 doubles have been rolled in a row
        public bool DiceRoll()
        {
            if (RepeatTurn)
                RepeatedTurns++;
            else
            {
                Turn = (Turn + 1) % Players.Length;
                RepeatedTurns = 0;
            }

            Dice[0] = randomizer.Next(1, 7);
            Dice[1] = randomizer.Next(1, 7);

            RepeatTurn = Dice[0] == Dice[1];
            
            if (RepeatTurn && RepeatedTurns == 2) //3 doubles, send player to jail
            {
                RepeatTurn = false;
                return true;
            }

            return false;
        }

        public void NextTurn()
        {
            if (DiceRoll())
                board.SendToJail(Players[Turn]);
            else if (Players[Turn].InJail >= 0)
            {
                if (Dice[0] == Dice[1])
                {
                    RepeatTurn = false;
                    Players[Turn].InJail = 0;
                }
                else if (Players[Turn].InJail >= 3) //after 3 turns you MUST leave
                {
                    Players[Turn].Money -= jailFine;
                    Players[Turn].InJail = 0;
                }
                else
                    Players[Turn].InJail++;
            }

            if (Players[Turn].InJail == 0)
            {
                Square s = board.Walk(Players[Turn], Dice[0] + Dice[1]);

                switch (s.type)
                {
                    case Square.Type.Property:
                        PropertyState ps = GetPropertyState(s.property);
                        Player owner = ps.Owner;
                        if (owner != null && owner != Players[Turn])
                        {
                            int rent = Groups[(int)ps.Property.color].Rent(ps);
                            Players[Turn].Money -= rent;
                            owner.Money += rent;
                        }
                        break;

                    case Square.Type.Chance:
                        Card d = Chance.Draw();
                        foreach (Event e in d.Events)
                            Execute(e);
                        break;

                    case Square.Type.CommunityChest:
                        d = CommunityChest.Draw();
                        foreach (Event e in d.Events)
                            Execute(e);
                        break;

                    case Square.Type.Tax:
                        Players[Turn].Money -= s.tax;
                        MiddleMoney += s.tax;
                        break;

                    case Square.Type.FreeParking:
                        Players[Turn].Money += MiddleMoney;
                        MiddleMoney = 0;
                        break;

                    case Square.Type.GoToJail:
                        board.SendToJail(Players[Turn]);
                        break;
                }
            }
        }

        //returns wether the Action was successfully executed
        public bool Execute(Action a)
        {
            if (a.IsTurnAction && Players[Turn] != a.Player)
                return false;

            if (a.IsPropertyAction)
            {
                Square s = board.GetSquare(Players[Turn].Position);

                if (s.type != Square.Type.Property)
                    return false;

                PropertyState ps = GetPropertyState(s.property);

                if (a.type == Action.Type.Buy) //buy
                {
                    if (ps.Owner != null)
                        return false;

                    a.Player.Money -= s.property.price;
                    ps.Owner = a.Player;
                }
                else if (!Groups[(int)s.property.color].Build(s.property)) //build
                    return false;
            }
            else
            {
                switch (a.type)
                {
                    case Action.Type.PayJailFine:
                        if (a.Player.InJail == 0 || a.Player.Money < jailFine)
                            return false;
                        a.Player.Money -= jailFine;
                        a.Player.InJail = 0;
                        if (a.Player == Players[Turn] && a.Player.InJail == 1)
                        {
                            NextTurn();
                            return true;
                        }
                        break;

                    case Action.Type.Mortgage:
                        if (a.property.Owner != a.Player)
                            return false;
                        if (!Groups[(int)a.property.Color].Mortgage(a.property))
                            return false;
                        break;

                    case Action.Type.Give:
                        if (a.Player.Money < a.amount)
                            return false;
                        a.Player.Money -= a.amount;
                        a.target.Money += a.amount;
                        break;

                    case Action.Type.GiveProperty:
                        if (a.property.Owner != a.Player)
                            return false;
                        a.property.Owner = a.target;
                        break;
                }
            }

            if (a.IsTurnAction)
                NextTurn();

            return true;
        }

        public void Execute(Event e, Player target)
        {
            switch (e.Type)
            {
                case Event.EventType.GoToJail:
                    board.SendToJail(target);
                    break;

                case Event.EventType.AdvanceToStart:
                    board.AdvanceToStart(target);
                    break;

                case Event.EventType.AdvanceTo:
                    board.AdvanceToProperty(target, e.Arg);
                    break;

                case Event.EventType.Receive:
                    target.Money += e.X;
                    break;

                case Event.EventType.ReceiveFromEach:
                    foreach (Player p in Players)
                        p.Money -= e.X;

                    target.Money += Players.Length * e.X;
                    break;

                //case Event.EventType.PayDoubleRent:

            }
        }
    }
}