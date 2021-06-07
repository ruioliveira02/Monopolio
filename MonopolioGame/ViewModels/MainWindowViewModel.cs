using MonopolioGame.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolioGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public string Username { get; set; }
        public string ServerIp { get; set; }  // "2.80.236.204:25565"

        public bool LoginScreen { get; set; }
        public bool GameScreen { get; set; }

        GameHandler handler = new GameHandler();

        public MainWindowViewModel()
        {
            Username = "Anta";
            ServerIp = "2.80.236.204:25565";
            LoginScreen = true;
            GameScreen = false;
        }

        public void ConnectCommand()
        {
            string[] split = ServerIp.Split(":");
            if (split.Length != 2 || !int.TryParse(split[1], out int n)) //Invalid IP
                return;//TODO:: Do stuff
            else
                handler.Connect(split[0], int.Parse(split[1]), Username);
        }
    }
}
