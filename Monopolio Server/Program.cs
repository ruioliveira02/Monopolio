using Monopolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Monopolio_Server
{
    /// <summary>
    /// Class containing the main.
    /// </summary> 
    class Program
    {
        /// <summary>
        /// The entry point of the application
        /// </summary> 
        /// 
        /// <param name="args">The program's arguments</param>
        static void Main(string[] args)
        {
            string board = "default_board.json";

            Console.Write("Game: ");
            string game = Console.ReadLine() + ".json";

            if (File.Exists(game))
                Server.Run(State.LoadState(game));
            else
                Server.Run(Board.LoadBoard(board));

            if (Server.State != null)
            {
                Console.WriteLine("Saving game...");
                Server.State.Save(game);
            }
            
            Console.WriteLine("Press any key to close");
            Console.ReadKey();


            /*
            string[] players = { "bace", "vasques", "manela", "sid" };
            State s = new State("default_board.json", players);
            s.DiceThrowHandler = new State.DiceThrow((int[] dice) =>
            {
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("Dice roll: " + dice[0] + " / " + dice[1]);
            });
            s.PlayerMoveHandler = new State.PlayerMove((Player p, int start, bool direct)
                => Console.WriteLine(p.name + " moves " + (direct ? "directly " : "")
                + "from " + start + " to " + p.Position)
            );
            s.CardDrawHandler = new State.CardDraw((Card c, int deck) =>
            {
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("Card draw: " + c.Name);
                Console.WriteLine(c.Text);
            });
            s.Start();

            while (s.AlivePlayers > 1)
            {
                //PRINT STATE
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("Turn: " + s.Players[s.Turn].name);
                foreach (Player p in s.Players)
                    Console.WriteLine(p.name + ": " + p.Money + " / " + p.Position
                        + " / " + p.GetOutOfJailFreeCards);

                //INPUT
                bool result = false;
                do
                {
                    string input = Console.ReadLine();

                    if (input == "save")
                    {
                        s.Save("game.mpy");
                        continue;
                    }

                    string player = input.Substring(0, input.IndexOf(' '));
                    string action = input.Substring(input.IndexOf(' ') + 1);
                    result = s.Execute(new Monopolio.Action(s, player, action));

                    if (!result)
                        Console.WriteLine("Action denied");
                }
                while (!result);
            }

            //END OF GAME
            Console.WriteLine("-------------------------------------");
            Player winner = s.Winner;
            if (winner == null)
                Console.WriteLine("It's a draw!");
            else
                Console.WriteLine(winner.name + " wins!");
            */
        }
    }
}