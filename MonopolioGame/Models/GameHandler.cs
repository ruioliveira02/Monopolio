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

        public event EventHandler DataChanged;

        public GameHandler()
        {
            server = new Server();
            server.NewResponseEvent += ((o, e) => { e.Execute(State); DataChanged(this, new EventArgs()); });
            State = new GameState(null);
        }

        public void Connect(string ip, int port, string username)
        {
            const int maxAttempts = 3;
            //Reset the state of the game
            State = new GameState(null);

            //Subscribe to response event
            server.NewResponseEvent += new EventHandler<Interfaces.Responses.Response>((o, s) => s.Execute(State));

            //TODO:: Reset board state            

            bool[] res = { false, false };
            for (int i = 0; i < maxAttempts && !res[0]; i++)
                server.Connect(ip, port);

            if(res[0])
                for (int i = 0; i < maxAttempts && !res[1]; i++)
                    server.Send(new IdentRequest(username));

            State.Connected = (res[0] && res[1]);
            State.ConnectionAttempt = !State.Connected;
        }
    }
}
