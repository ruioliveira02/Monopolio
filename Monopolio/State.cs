using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{

    public class State
    {
        public const int initial_money = 1500;
        public const int salary = 200;
        public const int jailFine = 50;

        public static Random randomizer = new Random();


        readonly Board board; //the board "template"

        Player[] players;
        int turn; //which player is currently playing
        bool repeat_turn;
        int repeated_turns;

        PropertyGroup[] groups;
        //cartas (que já saíram? ou todas, já baralhadas?)
        int middle_money;

        //2-long array with integers from 1 to 6
        public int[] Dice { get; }

        public State(string[] players) //new game
        {
            this.players = new Player[players.Length];

            for (int i = 0; i < players.Length; i++)
                this.players[i] = new Player(players[i]);

            board = new Board("default_board.txt"); //TODO
            groups = board.GetPropertyGroups();
            turn = -1;

            Dice = new int[2];
            Dice[0] = 1;
            Dice[1] = 1;
        }

        public State(string file) //saved game
        {
            
        }


        public void Save(string file) //json?
        {
            
        }

        public PropertyState GetPropertyState(Property property)
            => groups[(int)property.color].GetPropertyState(property);


        public PropertyState GetPropertyState(string propertyName)
        {
            foreach (var g in groups)
                foreach (var s in g.properties)
                    if (s.property.name == propertyName)
                        return s;

            return null;
        }

        public Player GetPlayer(string playerName)
        {
            foreach (Player p in players)
                if (p.name == playerName)
                    return p;

            return null;
        }

        public void SetOwner(Property property, Player player)
        {
            GetPropertyState(property).Owner = player;
        }


        //returns true when 3 doubles have been rolled in a row
        public bool DiceRoll()
        {
            if (repeat_turn)
                repeated_turns++;
            else
            {
                turn = (turn + 1) % players.Length;
                repeated_turns = 0;
            }

            Dice[0] = randomizer.Next(1, 7);
            Dice[1] = randomizer.Next(1, 7);

            repeat_turn = Dice[0] == Dice[1];
            
            if (repeat_turn && repeated_turns == 2) //3 doubles, send player to jail
            {
                repeat_turn = false;
                return true;
            }

            return false;
        }

        public void NextTurn()
        {
            if (DiceRoll())
                board.SendToJail(players[turn]);
            else if (players[turn].InJail >= 0)
            {
                if (Dice[0] == Dice[1])
                {
                    repeat_turn = false;
                    players[turn].InJail = 0;
                }
                else if (players[turn].InJail >= 3) //after 3 turns you MUST leave
                {
                    players[turn].Money -= jailFine;
                    players[turn].InJail = 0;
                }
                else
                    players[turn].InJail++;
            }

            if (players[turn].InJail == 0)
            {
                Square s = board.Walk(players[turn], Dice[0] + Dice[1]);

                switch (s.type)
                {
                    case Square.Type.Jail:
                        break;

                    case Square.Type.Property:
                        PropertyState ps = GetPropertyState(s.property);
                        Player owner = ps.Owner;
                        if (owner != null && owner != players[turn])
                        {
                            int rent = groups[(int)ps.property.color].Rent(ps);
                            players[turn].Money -= rent;
                            owner.Money += rent;
                        }
                        break;

                    case Square.Type.GoToJail:
                        board.SendToJail(players[turn]);
                        break;

                    //TODO
                }
            }
        }

        //returns wether the Action was successfully executed
        public bool Execute(Action a)
        {
            if (a.IsTurnAction && players[turn] != a.Player)
                return false;

            if (a.IsPropertyAction)
            {
                Square s = board.GetSquare(players[turn].Position);

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
                else if (!groups[(int)s.property.color].Build(s.property)) //build
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
                        if (a.Player == players[turn] && a.Player.InJail == 1)
                        {
                            NextTurn();
                            return true;
                        }
                        break;

                    case Action.Type.Mortgage:
                        if (a.property.Owner != a.Player)
                            return false;
                        if (!groups[(int)a.property.Color].Mortgage(a.property))
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
    }
}