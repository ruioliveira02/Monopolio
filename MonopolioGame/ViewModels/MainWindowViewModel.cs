using Avalonia.Media;
using Monopolio;
using MonopolioGame.Interfaces.Requests;
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
        #region Properties
        /// <summary>
        /// The list of viewmodels for the list of players
        /// </summary>
        private ObservableCollection<PlayerViewModel> _playersVM; 
        public ObservableCollection<PlayerViewModel> PlayersVM
        {
            get => _playersVM;
            set
            {
                _playersVM = value;
                Raise(this, nameof(PlayersVM));
            }
        }


        private ObservableCollection<PropertyViewModel> _propertiesVM;
        public ObservableCollection<PropertyViewModel> PropertiesVM
        {
            get => _propertiesVM;
            set
            {
                _propertiesVM = value;
                Raise(this, nameof(PropertiesVM));
            }
        }

        private ObservableCollection<SpecialPlaceViewModel> _cornersVM;
        public ObservableCollection<SpecialPlaceViewModel> CornersVM 
        { 
            get => _cornersVM;
            set
            {
                _cornersVM = value;
                Raise(this, nameof(CornersVM));
            }
        }


        private string _chat;
        public string Chat
        {
            get => _chat;
            set
            {
                _chat = value;
                Raise(this, nameof(Chat));
            }
        }

        private string _chatMessage;
        public string ChatMessage
        {
            get => _chatMessage;
            set
            {
                _chatMessage = value;
                Raise(this, nameof(ChatMessage));
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                Raise(this, nameof(Username));
            }
        }

        private string _serverIp;
        public string ServerIp // "2.80.236.204:25565"
        {
            get => _serverIp;
            set
            {
                _serverIp = value;
                Raise(this, nameof(ServerIp));
            }
        }

        /// <summary>
        /// DO NOT TOUCH THIS!!! EVER!!!
        /// Use the individual screen setters instead
        /// (changing one to true will change the others to false)
        /// </summary>
        private int _screen;    //-1 -> undefined
                                //0  -> login
                                //1  -> game
                                //2  -> error

        public bool LoginScreen
        {
            get => _screen == 0;
            set
            {
                if (value)
                {
                    _screen = 0;
                    Raise(this, nameof(LoginScreen));
                }
                else if (_screen == 0)
                    _screen = -1;
            }
        }
        public bool GameScreen
        {
            get => _screen == 1;
            set
            {
                if (value)
                {
                    _screen = 1;
                    Raise(this, nameof(GameScreen));
                }
                else if (_screen == 0)
                    _screen = -1;
            }
        }

        #region gameScreenProperties

        private int _selectedSquareIndex;
        public int SelectedSquareIndex
        {
            get => _selectedSquareIndex;
            set
            {
                if (_selectedSquareIndex != value)
                {
                    _selectedSquareIndex = value;
                    Raise(this, nameof(PropertySelected));
                    Raise(this, nameof(ChanceSelected));
                    Raise(this, nameof(CommunityChestSelected));
                    Raise(this, nameof(TaxSelected));
                    Raise(this, nameof(StartSelected));
                    Raise(this, nameof(JailSelected));
                    Raise(this, nameof(FreeParkingSelected));
                    Raise(this, nameof(GoToJailSelected));
                }
            }
        }
        public Square? SelectedSquare { get => Handler?.State?.CurrentState?.board?.GetSquare(SelectedSquareIndex); }

        public bool PropertySelected { get => SelectedSquare?.type == Square.Type.Property; }
        public bool ChanceSelected { get => SelectedSquare?.type == Square.Type.Chance; }
        public bool CommunityChestSelected { get => SelectedSquare?.type == Square.Type.CommunityChest; }
        public bool TaxSelected { get => SelectedSquare?.type == Square.Type.Tax; }
        public bool StartSelected { get => SelectedSquare?.type == Square.Type.Start; }
        public bool JailSelected { get => SelectedSquare?.type == Square.Type.Jail; }
        public bool FreeParkingSelected { get => SelectedSquare?.type == Square.Type.FreeParking; }
        public bool GoToJailSelected { get => SelectedSquare?.type == Square.Type.GoToJail; }

        #endregion

        public bool ErrorScreen
        {
            get => _screen == 2;
            set
            {
                if (value)
                {
                    _screen = 2;
                    Raise(this, nameof(ErrorScreen));
                }
                else if (_screen == 0)
                    _screen = -1;
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                Raise(this, nameof(ErrorMessage));
            }
        }
        

        private bool _connectionAttemptedText;
        public bool ConnectionAttemptedText
        {
            get => _connectionAttemptedText; 
            set
            { 
                _connectionAttemptedText = value;
                Raise(this, nameof(ConnectionAttemptedText));
            }
        }


        GameHandler Handler { get; set; }
        #endregion
        public MainWindowViewModel()
        {
            Username = "Anta";
            ServerIp = "2.80.236.204:25565";
            LoginScreen = true;
            GameScreen = false;
            ConnectionAttemptedText = false;
            SetBoard();
            SetPlayers();
            Handler = new GameHandler("");
            Handler.DataChanged += ((s, e) => UpdateData());
        }

        #region Commands
        public void ConnectCommand()
        {
            string[] split = ServerIp.Split(":");
            if (split.Length != 2 || !int.TryParse(split[1], out _)) //Invalid IP
                return;//TODO:: Do stuff
            else
                Handler.Connect(split[0], int.Parse(split[1]), Username);

            UpdateData();
        }

        public void ErrorOK()
        {
            //Return to login screen
            Handler.Disconnect();
            LoginScreen = true;
        }

        public void Closing()
        {
            Handler.Disconnect();
        }

        public void SendChatMessage()
        {
            Handler.Server.Send(new ChatRequest(Username, ChatMessage));
            ChatMessage = "";
        }
        #endregion
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
            LoginScreen = !Handler.State.Connected;
            GameScreen = Handler.State.Connected;
            ConnectionAttemptedText = Handler.State.ConnectionAttempt;
            Chat = Handler.State.Chat;

            if (Handler.State.CurrentState == null)
                return;

            UpdatePlayers();
            UpdatePlayersProperties();
        }

        protected void UpdatePlayers()
        {
            ObservableCollection<PlayerViewModel> temp = new ObservableCollection<PlayerViewModel>();

            foreach(Player player in Handler.State.CurrentState.Players)
            {
                temp.Add(new PlayerViewModel(player, player.name == Username, false, false));
            }
            temp[Handler.State.CurrentState.Turn].IsCurrentTurn = true;

            PlayersVM = temp;
        }

        protected void UpdatePlayersProperties()
        {
            ObservableCollection<PropertyViewModel> temp = PropertiesVM;

            foreach(PropertyViewModel property in temp)
            {
                property.Players = new ObservableCollection<ColorViewModel>();
            }

            foreach(Player player in Handler.State.CurrentState.Players)
            {
                temp[player.Position].Players.Add(new ColorViewModel(Brushes.Red)); //TODO:: Change to player image
            }

            PropertiesVM = temp;
        }
    }
}
