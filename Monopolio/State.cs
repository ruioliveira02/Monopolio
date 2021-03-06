using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Monopolio
{
    /// <summary>
    /// Represents the current game state
    /// </summary>
    public class State
    {
        public const string Extension = ".mpy";

        public const int housesPerHotel = 4;
        public const int maxBuildings = 5;

        public const int initialMoney = 1500;
        public const int salary = 200;
        public const int jailFine = 50;

        #region handlers

        public delegate void TurnUpdate(int turn);

        //dice: length 2 with numbers from 1 to 6
        public delegate void DiceThrow(int[] dice);

        //deck: 1 -> chance | 2 -> community chest
        public delegate void CardDraw(Card card, int deck);

        public delegate void MiddleMoneyUpdate(int middleMoney);

        public delegate void PlayerUpdate(Player p);

        public delegate void PropertyUpdate(PropertyState ps);

        #endregion

        public static Random randomizer = new Random();

        /// <summary>
        /// Loads a state in JSON format from the specified file
        /// </summary>
        /// <param name="file">The specified file</param>
        /// <returns>The stored game state</returns>
        public static State LoadState(string file)
        {
            string json = File.ReadAllText(file);
            State state = JsonConvert.DeserializeObject<State>(json);

            foreach (var a in state.Groups)
            {
                foreach (var b in a.properties)
                {
                    b.ResolveOwner(state.Players);
                    b.ResolveProperty(state.board);
                }
            }

            foreach (Player p in state.Players)
                p.ResolveCreditor(state.Players);

            return state;
        }


        public Board board { get; } //the board "template"
        public PropertyGroup[] Groups { get; }

        public Player[] Players { get; }
        public int Turn { get; set; } //which player is currently playing
        public bool RepeatTurn { get; set; }
        public int RepeatedTurns { get; set; }

        public int[] Dice { get; } //2-long array with integers from 1 to 6

        private int _middleMoney;
        public int MiddleMoney { get => _middleMoney; private set {
                _middleMoney = value;
                MiddleMoneyUpdateHandler?.Invoke(_middleMoney);
            } }

        public Deck Chance { get; }
        public Deck CommunityChest { get; }

        #region handlers

        [JsonIgnore]
        public TurnUpdate TurnUpdateHandler { get; set; }
        [JsonIgnore]
        public DiceThrow DiceThrowHandler { get; set; }
        [JsonIgnore]
        public CardDraw CardDrawHandler { get; set; }
        [JsonIgnore]
        public MiddleMoneyUpdate MiddleMoneyUpdateHandler { get; set; }
        [JsonIgnore]
        public PlayerUpdate PlayerUpdateHandler { get; set; }
        [JsonIgnore]
        public PropertyUpdate PropertyUpdateHandler { get; set; }

        #endregion

        /// <summary>
        /// Creates a new game state (new game)
        /// </summary>
        /// <param name="board">The board template</param>
        /// <param name="players">The playesr's names</param>
        public State(Board board, string[] players)
        {
            Players = new Player[players.Length];

            for (int i = 0; i < players.Length; i++)
                Players[i] = new Player(players[i]);

            this.board = board;
            Groups = board.GetPropertyGroups();
            Turn = -1;

            Dice = new int[2];
            Dice[0] = 1;
            Dice[1] = 1;
            Chance = new Deck(this.board.Chance);
            CommunityChest = new Deck(this.board.CommunityChest);
        }

        /// <summary>
        /// Creates a game state from a previosly saved game
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="groups"></param>
        /// <param name="players"></param>
        /// <param name="turn"></param>
        /// <param name="repeatTurn"></param>
        /// <param name="repeatedTurns"></param>
        /// <param name="dice"></param>
        /// <param name="middleMoney"></param>
        /// <param name="chance"></param>
        /// <param name="communityChest"></param>
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
            _middleMoney = middleMoney;
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

        /// <summary>
        /// Saves a game state to a file
        /// </summary>
        /// <param name="file">The file</param>
        public void Save(string file)
        {
            JsonSerializerSettings s = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                Formatting = Formatting.None

            };
            string json = JsonConvert.SerializeObject(this, s);
            File.WriteAllText(file, json);
        }

        /// <summary>
        /// Assigns both dice a random integer value from 1 to 6
        /// and calls the DiceThrowHandler, if set
        /// </summary>
        public void ThrowDice()
        {
            Dice[0] = randomizer.Next(1, 7);
            Dice[1] = randomizer.Next(1, 7);
            DiceThrowHandler?.Invoke(Dice);
        }

        #region getters

        /// <summary>
        /// Retrieves the state of a given property (searches all properties)
        /// </summary>
        /// <param name="property">The given property</param>
        /// <returns>The corresponding state</returns>
        public PropertyState GetPropertyState(Property property)
            => Groups[(int)property.color].GetPropertyState(property);

        /// <summary>
        /// Retrieves the state of a given property (searches all properties)
        /// </summary>
        /// <param name="propertyName">The given property's name</param>
        /// <returns>The corresponding state</returns>
        public PropertyState GetPropertyState(string propertyName)
        {
            foreach (var g in Groups)
                foreach (var s in g.properties)
                    if (s.Property.name == propertyName)
                        return s;

            return null;
        }

        /// <summary>
        /// Given a player's name, retrieves his Player object
        /// </summary>
        /// <param name="playerName">The player's name</param>
        /// <returns>The corresponding Player object</returns>
        public Player GetPlayer(string playerName)
        {
            foreach (Player p in Players)
                if (p.name == playerName)
                    return p;

            return null;
        }

        [JsonIgnore]
        public int AlivePlayers
        {
            get
            {
                int ans = 0;

                foreach (Player p in Players)
                    if (!p.Bankrupt)
                        ans++;

                return ans;
            }
        }

        [JsonIgnore]
        public Player Winner
        {
            get
            {
                Player winner = null;

                foreach (Player p in Players)
                {
                    if (!p.Bankrupt)
                    {
                        if (winner == null)
                            winner = p;
                        else
                            return null;
                    }
                }

                return winner;
            }
        }

        #endregion

        #region gameFlow

        void CallUpdaters()
        {
            foreach (Player p in Players)
            {
                if (p.updated)
                {
                    PlayerUpdateHandler?.Invoke(p);
                    p.updated = false;
                }
            }

            foreach (PropertyGroup pg in Groups)
            {
                foreach (PropertyState ps in pg.properties)
                {
                    if (ps.updated)
                    {
                        PropertyUpdateHandler?.Invoke(ps);
                        ps.updated = false;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        /// <returns>True if successful. False if the game is already ongoing</returns>
        public bool Start()
        {
            if (Turn != -1)
                return false;

            NextTurn();
            return true;
        }

        /// <summary>
        /// Starts the next turn.
        /// Sets the turn and rolls the dice.
        /// </summary>
        /// <returns>True if a double is rolled 3 times in a row
        /// for the same player</returns>
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

            TurnUpdateHandler?.Invoke(Turn);
            ThrowDice();
            RepeatTurn = Dice[0] == Dice[1];

            if (RepeatTurn && RepeatedTurns == 2) //3 doubles, send player to jail
            {
                RepeatTurn = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Plays the next turn
        /// </summary>
        void NextTurn()
        {
            //check wether anyone lost in the previous turn
            foreach (var p in Players)
                if (!p.Bankrupt && p.Money < 0)
                    Bankruptcy(p);

            //game over
            if (AlivePlayers <= 1)
                return;

            if (DiceRoll())
                board.SendToJail(Players[Turn]);
            else if (Players[Turn].InJail > 0)
            {
                if (Dice[0] == Dice[1])
                {
                    RepeatTurn = false;
                    Players[Turn].InJail = 0;
                }
                else if (Players[Turn].InJail > 3) //after 3 turns in jail you MUST leave
                {
                    //TODO: bug fix
                    //if the player contracts debt to pay the fine and then contracts
                    //debt to another player, the debts will get mixed up
                    Players[Turn].Money -= jailFine;
                    Players[Turn].InJail = 0;
                }
                else
                    Players[Turn].InJail++;
            }

            if (Players[Turn].InJail == 0)
            {
                board.Walk(Players[Turn], Dice[0] + Dice[1]);
                CallUpdaters();
                CalculateSquare(Players[Turn]);
            }

            CallUpdaters();
        }

        /// <summary>
        /// Calculates the result of the specified player falling into
        /// his current position (ex: if the player is on a chance square,
        /// a chance card is drawn)
        /// </summary>
        /// <param name="p">The specified player</param>
        void CalculateSquare(Player p)
        {
            Square s = board.GetSquare(p.Position);

            switch (s.type)
            {
                case Square.Type.Property:
                    PropertyState ps = GetPropertyState(s.property);
                    if (ps.Owner != null && ps.Owner != p)
                    {
                        int rent = Groups[(int)ps.Property.color].Rent(ps, this);
                        p.Give(rent, ps.Owner);
                    }
                    break;

                case Square.Type.Chance:
                    Card d = Chance.Draw();
                    CardDrawHandler?.Invoke(d, 1);
                    foreach (Event e in d.Events)
                        Execute(e, p);
                    break;

                case Square.Type.CommunityChest:
                    d = CommunityChest.Draw();
                    CardDrawHandler?.Invoke(d, 2);
                    foreach (Event e in d.Events)
                        Execute(e, p);
                    break;

                case Square.Type.Tax:
                    //TODO: alternative 10% total worth (because
                    //income tax can't cause bankrupcy)
                    p.Money -= s.tax;
                    MiddleMoney += s.tax;
                    break;

                case Square.Type.FreeParking:
                    p.Money += MiddleMoney;
                    MiddleMoney = 0;
                    break;

                case Square.Type.GoToJail:
                    board.SendToJail(p);
                    //TODO: 2 motions as 1 ?????
                    break;
            }
        }

        //when a player loses the game (can't pay)
        /// <summary>
        /// Sells/returns to the bank all the specified player's assets
        /// (including get-out-of-jail-free cards) and sets him as bankrupt
        /// </summary>
        /// <param name="p">The specified player</param>
        void Bankruptcy(Player p)
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

        #endregion

        #region executes

        /// <summary>
        /// Executes the given player action.
        /// </summary>
        /// <param name="a">The given action</param>
        /// <returns>True if the Action was successfully executed</returns>
        public bool Execute(Action a, Player player)
        {
            //it ain't started yet
            if (Turn == -1)
                return false;

            //game over
            if (AlivePlayers <= 1)
                return false;

            //dead men tell no tales
            if (player.Bankrupt)
                return false;

            //wait for your turn
            if (a.IsTurnAction && player != Players[Turn])
                return false;

            //can't get out if you're already out
            if (a.IsGetOutOfJail && player.InJail == 0)
                return false;

            switch (a.type)
            {
                case Action.Type.Skip:
                    NextTurn();
                    return true;

                case Action.Type.Buy:
                    Square s = board.GetSquare(Players[Turn].Position);

                    if (s.type != Square.Type.Property)
                        return false;

                    PropertyState ps = GetPropertyState(s.property);
                    if (ps.Owner != null || player.Money < s.property.price)
                        return false;

                    player.Money -= s.property.price;
                    ps.Owner = player;
                    break;

                case Action.Type.Build:
                    if (a.property.Owner != player
                        || !Groups[(int)a.property.Color].Build(a.property))
                        return false;
                    break;

                case Action.Type.PayJailFine:
                    if (player.Money < jailFine)
                        return false;
                    player.Money -= jailFine;
                    if (player == Players[Turn] && player.InJail == 1)
                    {
                        player.InJail = 0;
                        CallUpdaters();
                        NextTurn();
                        return true;
                    }
                    break;

                case Action.Type.UseGetOutOfJailFreeCard:
                    if (player.GetOutOfJailFreeCards == 0)
                        return false;
                    player.GetOutOfJailFreeCards--;
                    if (player == Players[Turn] && player.InJail == 1)
                    {
                        player.InJail = 0;
                        CallUpdaters();
                        NextTurn();
                        return true;
                    }
                    break;

                case Action.Type.Mortgage:
                    if (a.property.Owner != player
                        || !Groups[(int)a.property.Color].Mortgage(a.property))
                        return false;
                    break;

                case Action.Type.Give:
                    if (a.amount < 0 || player.Money < a.amount)
                        return false;
                    player.Give(a.amount, a.target);
                    break;

                case Action.Type.GiveProperty:
                    if (a.property.Owner != player)
                        return false;
                    //TODO: pay interest if new owner doesn't immediately lift mortgage
                    a.property.Owner = a.target;
                    break;

                case Action.Type.GiveGetOutOfJailFreeCard:
                    if (player.GetOutOfJailFreeCards == 0)
                        return false;
                    player.GetOutOfJailFreeCards--;
                    a.target.GetOutOfJailFreeCards++;
                    break;
            }

            if (a.IsGetOutOfJail)
            {
                player.InJail = 0;
                board.Walk(player, Dice[0] + Dice[1]);
                CallUpdaters();
                CalculateSquare(player);
            }

            CallUpdaters();
            return true;
        }

        /// <summary>
        /// Executes the event on the target player
        /// </summary>
        /// <param name="e">The event</param>
        /// <param name="target">The target player</param>
        public void Execute(Event e, Player target)
        {
            switch (e.Type)
            {
                case Event.EventType.GoToJail:
                    board.SendToJail(target);
                    break;

                case Event.EventType.AdvanceToStart:
                    board.AdvanceToStart(target); //the 200€ salary is given by the board
                    break;

                case Event.EventType.AdvanceToStation:
                    board.AdvanceToNearest(target, Property.Color.Station);
                    CalculateSquare(target);
                    break;

                case Event.EventType.AdvanceTo:
                    board.AdvanceToProperty(target, e.Arg);
                    CalculateSquare(target);
                    break;

                case Event.EventType.Walk:
                    board.Walk(target, e.X);
                    CalculateSquare(target);
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
                            int rent = e.X * Groups[(int)ps.Color].Rent(ps, this);
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

            CallUpdaters();
        }

        #endregion
    }
}