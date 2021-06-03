using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Monopolio
{

    public class State
    {
        public const int housesPerHotel = 4;
        public const int maxBuildings = 5;

        public const int initialMoney = 1500;
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

        [JsonIgnore]
        public int AlivePlayers {
            get
            {
                int ans = 0;

                foreach (Player p in Players)
                    if (!p.Bankrupt)
                        ans++;

                return ans;
            }
        }


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

            foreach (Player p in Players)
                p.ResolveCreditor(Players);
        }


        public void Save(string file)
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(file, json);
        }

        #region getters

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

        #endregion

        //when a player lost the game (can't pay)
        void Bankrupcy(Player p)
        {
            int aux = 0;

            foreach (var a in Groups)
            {
                foreach (var b in a.properties)
                {
                    aux += b.Property.buildPrice * b.Buildings / 2;
                    b.Houses = 0;
                    b.Hotels = 0;
                    b.Owner = p.Creditor;
                    //TODO: auction properties if the creditor is the bank (null)
                }
            }

            if (p.Creditor != null)
            {
                p.Creditor.Money += aux;
                p.Creditor.GetOutOfJailFreeCards += p.GetOutOfJailFreeCards;
            }
            else
            {
                //return get-out-of-jail-free cards to their decks
                //TODO: decide to which deck (currently only chance)
                foreach (Card c in board.Chance)
                {
                    if (c.Events.Length > 0 &
                        c.Events[0].Type == Event.EventType.OutOfJailFree)
                    {
                        while (p.GetOutOfJailFreeCards-- > 0)
                            Chance.Add(c);

                        break;
                    }
                }
            }

            p.Bankrupt = true;
        }

        #region gameFlow

        //returns true when 3 doubles have been rolled in a row
        bool DiceRoll()
        {
            if (RepeatTurn && !Players[Turn].Bankrupt)
                RepeatedTurns++;
            else
            {
                do
                    Turn = (Turn + 1) % Players.Length;
                while (Players[Turn].Bankrupt);
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

        void NextTurn()
        {
            //check wether anyone lost in the previous turn
            foreach (var p in Players)
                if (!p.Bankrupt && p.Money < 0)
                    Bankrupcy(p);

            //game over
            if (AlivePlayers <= 1)
                return;

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
                    //TODO: bug fix
                    //if the player contracts debt to pay the fine and then contracts
                    //debt to another player, the debts will get mixed
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
                        if (ps.Owner != null && ps.Owner != Players[Turn])
                        {
                            int rent = Groups[(int)ps.Property.color].Rent(ps);
                            Players[Turn].Give(rent, ps.Owner);
                        }
                        break;

                    case Square.Type.Chance:
                        Card d = Chance.Draw();
                        foreach (Event e in d.Events)
                            Execute(e, Players[Turn]);
                        break;

                    case Square.Type.CommunityChest:
                        d = CommunityChest.Draw();
                        foreach (Event e in d.Events)
                            Execute(e, Players[Turn]);
                        break;

                    case Square.Type.Tax:
                        //TODO: alternative 10% total worth (because
                        //income tax can't cause bankrupcy)
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

        #endregion

        #region executes

        //returns wether the Action was successfully executed
        public bool Execute(Action a)
        {
            //game over
            if (AlivePlayers <= 1)
                return false;

            //dead men tell no tales
            if (a.Player.Bankrupt)
                return false;

            //wait for your turn
            if (a.IsTurnAction && Players[Turn] != a.Player)
                return false;

            switch (a.type)
            {
                case Action.Type.Buy:
                    Square s = board.GetSquare(Players[Turn].Position);

                    if (s.type != Square.Type.Property)
                        return false;

                    PropertyState ps = GetPropertyState(s.property);
                    if (ps.Owner != null || a.Player.Money < s.property.price)
                        return false;

                    a.Player.Money -= s.property.price;
                    ps.Owner = a.Player;
                    break;

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

                case Action.Type.Build:
                    if (a.property.Owner != a.Player
                        || !Groups[(int)a.property.Color].Build(a.property))
                        return false;
                    break;

                case Action.Type.Mortgage:
                    if (a.property.Owner != a.Player
                        || !Groups[(int)a.property.Color].Mortgage(a.property))
                        return false;
                    break;

                case Action.Type.Give:
                    if (a.Player.Money < a.amount)
                        return false;
                    a.Player.Give(a.amount, a.target);
                    break;

                case Action.Type.GiveProperty:
                    if (a.property.Owner != a.Player)
                        return false;
                    //TODO: pay interest if new owner doesn't immediately lift mortgage
                    a.property.Owner = a.target;
                    break;
            }

            if (a.IsTurnAction)
                NextTurn();

            return true;
        }

        void Execute(Event e, Player target)
        {
            switch (e.Type)
            {
                case Event.EventType.GoToJail:
                    board.SendToJail(target);
                    break;

                case Event.EventType.AdvanceToStart:
                    board.AdvanceToStart(target);
                    break;

                case Event.EventType.AdvanceToStation:
                    board.AdvanceToNearest(target, Property.Color.Station);
                    break;

                case Event.EventType.AdvanceTo:
                    board.AdvanceToProperty(target, e.Arg);
                    break;

                case Event.EventType.Walk:
                    board.Walk(target, e.X);
                    break;

                case Event.EventType.Receive:
                    target.Money += e.X;
                    break;

                case Event.EventType.ReceiveFromEach:
                    foreach (Player p in Players)
                        if (p != target)
                            p.Give(e.X, target);
                    break;

                case Event.EventType.PayXRent:
                    Square s = board.GetSquare(target.Position);
                    if (s.type == Square.Type.Property)
                    {
                        PropertyState ps = GetPropertyState(s.property);
                        if (ps.Owner != null && ps.Owner != target)
                        {
                            int rent = e.X * Groups[(int)ps.Color].Rent(ps);
                            target.Give(rent, ps.Owner);
                        }
                    }
                    break;

                case Event.EventType.RepairProperty:
                    int cost = 0;
                    foreach (var a in Groups)
                    {
                        foreach (var b in a.properties)
                        {
                            if (b.Owner == target)
                            {
                                cost += b.Houses * e.X;
                                cost += b.Hotels * e.Y;
                            }
                        }
                    }
                    target.Money -= cost;
                    break;

                case Event.EventType.OutOfJailFree:
                    target.GetOutOfJailFreeCards++;
                    break;

                default:
                    throw new NotImplementedException("Event not implemented");
            }
        }

        #endregion
    }
}