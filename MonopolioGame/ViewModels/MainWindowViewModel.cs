using Avalonia.Media;
using Monopolio;
using MonopolioGame.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace MonopolioGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The list of viewmodels for the list of players
        /// </summary>
        private ObservableCollection<PlayerViewModel> _playersVM; 
        public ObservableCollection<PlayerViewModel> PlayersVM
        {
            get
            {
                return _playersVM;
            }
            set
            {
                _playersVM = value;
                Raise(this, nameof(PlayersVM));
            }
        }


        private ObservableCollection<PropertyViewModel> _propertiesVM;
        public ObservableCollection<PropertyViewModel> PropertiesVM
        {
            get
            {
                return _propertiesVM;
            }
            set
            {
                _propertiesVM = value;
                Raise(this, nameof(PropertiesVM));
            }
        }

        private ObservableCollection<SpecialPlaceViewModel> _cornersVM;
        public ObservableCollection<SpecialPlaceViewModel> CornersVM 
        { 
            get
            {
                return _cornersVM;
            }
            set
            {
                _cornersVM = value;
                Raise(this, nameof(CornersVM));
            }
        }


        private string _chat;
        public string Chat
        {
            get
            {
                return _chat;
            }
            set
            {
                _chat = value;
                Raise(this, nameof(Chat));
            }
        }

        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                Raise(this, nameof(Username));
            }
        }

        private string _serverIp;
        public string ServerIp // "2.80.236.204:25565"
        {
            get
            {
                return _serverIp;
            }
            set
            {
                _serverIp = value;
                Raise(this, nameof(ServerIp));
            }
        }

        private bool _loginScreen;
        public bool LoginScreen
        {
            get
            {
                return _loginScreen;
            }
            set
            {
                _loginScreen = value;
                Raise(this, nameof(LoginScreen));
            }
        }

        private bool _gameScreen;
        public bool GameScreen
        {
            get
            {
                return _gameScreen;
            }
            set
            {
                _gameScreen = value;
                Raise(this, nameof(GameScreen));
            }
        }

        private bool _connectionAttemptedText;
        public bool ConnectionAttemptedText
        {
            get
            { 
                return _connectionAttemptedText; 
            }
            set
            { 
                _connectionAttemptedText = value;
                Raise(this, nameof(ConnectionAttemptedText));
            }
        }


        GameHandler handler = new GameHandler();

        public MainWindowViewModel()
        {
            Username = "Anta";
            ServerIp = "2.80.236.204:25565";
            LoginScreen = true;
            GameScreen = false;
            ConnectionAttemptedText = false;
            SetBoard();
            SetPlayers();

            handler.DataChanged += ((s,e) => UpdateData());
        }
        

        public void ConnectCommand()
        {
            string[] split = ServerIp.Split(":");
            if (split.Length != 2 || !int.TryParse(split[1], out _)) //Invalid IP
                return;//TODO:: Do stuff
            else
                handler.Connect(split[0], int.Parse(split[1]), Username);

            UpdateData();
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

        protected void UpdateData()
        {
            LoginScreen = !handler.State.Connected;
            GameScreen = handler.State.Connected;
            ConnectionAttemptedText = handler.State.ConnectionAttempt;
            Chat = handler.State.Chat;

            if (handler.State.CurrentState == null)
                return;

            UpdatePlayers();
            UpdatePlayersProperties();
        }

        protected void UpdatePlayers()
        {
            ObservableCollection<PlayerViewModel> temp = new ObservableCollection<PlayerViewModel>();

            foreach(Player player in handler.State.CurrentState.Players)
            {
                temp.Add(new PlayerViewModel(player, player.name == Username, false, false));
            }
            temp[handler.State.CurrentState.Turn].IsCurrentTurn = true;

            PlayersVM = temp;
        }

        protected void UpdatePlayersProperties()
        {
            ObservableCollection<PropertyViewModel> temp = PropertiesVM;

            foreach(PropertyViewModel property in temp)
            {
                property.Players = new ObservableCollection<ColorViewModel>();
            }

            foreach(Player player in handler.State.CurrentState.Players)
            {
                temp[player.Position].Players.Add(new ColorViewModel(Brushes.Red)); //TODO:: Change to player image
            }

            PropertiesVM = temp;
        }
    }
}
