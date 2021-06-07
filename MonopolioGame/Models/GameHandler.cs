using MonopolioGame.Interfaces.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Models
{
    public class GameHandler
    {
        Server server;
        public GameState State { get; set; }
        public GameHandler()
        {
            server = new Server();
            State = new GameState();
        }

        public void Connect(string ip, int port, string username)
        {
            //Reset the state of the game
            State = new GameState();

            //Subscribe to response event
            server.NewResponseEvent += new EventHandler<Interfaces.Responses.Response>((o, s) => s.Execute(State));

            //TODO:: Reset board state
            server.Connect(ip, port); //TODO :: Use Connect value
            server.Send(new IdentRequest(username));
        }
    }
}
