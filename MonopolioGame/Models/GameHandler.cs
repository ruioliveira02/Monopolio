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
        public Server Server { get; set; }
        public GameState State { get; set; }

        public event EventHandler DataChanged;

        public GameHandler(string username)
        {
            Server = new Server();
            Server.NewResponseEvent += (o, e) =>
            {
                e.Execute(State);
                DataChanged?.Invoke(this, new EventArgs());
            };
            State = new GameState(username);
        }

        public void Connect(string ip, int port, string username)
        {
            
            const int maxAttempts = 3;
            //Reset the state of the game
            State = new GameState(null);
            State.Player = username;

            //TODO:: Reset board state            

            bool[] res = { false, false };
            for (int i = 0; i < maxAttempts && !res[0]; i++)
                res[0] = Server.Connect(ip, port);

            if(res[0])
                for (int i = 0; i < maxAttempts && !res[1]; i++)
                    res[1] = Server.Send(new IdentRequest(username));

            //State.Connected = (res[0] && res[1]);
            State.ConnectionAttempt = !(res[0] && res[1]);
        }

        public void Disconnect() => Server.Disconnect();
    }
}
