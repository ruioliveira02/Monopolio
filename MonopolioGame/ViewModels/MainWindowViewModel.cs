using Avalonia.Media;
using MonopolioGame.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonopolioGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The list of viewmodels for the list of players
        /// </summary>
        public ObservableCollection<PlayerViewModel> PlayersVM { get; set; }
        public ObservableCollection<PropertyViewModel> PropertiesVM { get; set; }
        public ObservableCollection<SpecialPlaceViewModel> CornersVM { get; set; }
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
            SetBoard();
            SetPlayers();
        }

        public void ConnectCommand()
        {
            string[] split = ServerIp.Split(":");
            if (split.Length != 2 || !int.TryParse(split[1], out _)) //Invalid IP
                return;//TODO:: Do stuff
            else
                handler.Connect(split[0], int.Parse(split[1]), Username);
        }

        private void SetBoard()
        {
            var temp = new ObservableCollection<PropertyViewModel>();
            for (int i = 0; i < 40; i++)
            {
                if (i % 10 == 0)
                    continue;

                IBrush c = (i % 2 == 0) ? new SolidColorBrush(Colors.Red, 1) : new SolidColorBrush(Colors.Pink, 1);
                var v = new PropertyViewModel(c, i, i.ToString(), 1000);
                v.Players.Add(new ColorViewModel(Brushes.RosyBrown));
                v.Players.Add(new ColorViewModel(Brushes.Pink));
                temp.Add(v);
            }

            PropertiesVM = temp;

            var temp2 = new ObservableCollection<SpecialPlaceViewModel>();
            temp2.Add(new SpecialPlaceViewModel(Brushes.DarkBlue, 10, 0));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Orange, 0, 0));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Lime, 0, 10));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Gray, 10, 10));

            CornersVM = temp2;
        }


        private void SetPlayers()
        {
            ObservableCollection<PlayerViewModel> temp = new ObservableCollection<PlayerViewModel>();
            temp.Add(new PlayerViewModel(true, false, false, "Vasques", Brushes.Green, 10));
            temp.Add(new PlayerViewModel(false, true, false, "Bace", Brushes.DarkBlue, -1000));
            temp.Add(new PlayerViewModel(false, false, true, "Manela", Brushes.Pink, 100000)); //Roubou o dinheiro todo ao Vasques e foi se embora

            PlayersVM = temp;
        }
    }
}
