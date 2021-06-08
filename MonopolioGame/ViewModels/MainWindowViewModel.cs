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
            var temp = new ObservableCollection<PropertyViewModel>();
            for(int i = 0; i < 40; i++)
            {
                if (i % 10 == 0)
                    continue;

                IBrush c = (i % 2 == 0) ? new SolidColorBrush(Colors.Red, 1) : new SolidColorBrush(Colors.Pink, 1);

                temp.Add(new PropertyViewModel(c, i, i.ToString(), 1000));
            }

            PropertiesVM = temp;

            var temp2 = new ObservableCollection<SpecialPlaceViewModel>();
            temp2.Add(new SpecialPlaceViewModel(Brushes.DarkBlue, 10, 0));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Orange, 0, 0));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Lime, 0, 10));
            temp2.Add(new SpecialPlaceViewModel(Brushes.Gray, 10, 10));

            CornersVM = temp2;
        }

        public void ConnectCommand()
        {
            string[] split = ServerIp.Split(":");
            if (split.Length != 2 || !int.TryParse(split[1], out _)) //Invalid IP
                return;//TODO:: Do stuff
            else
                handler.Connect(split[0], int.Parse(split[1]), Username);
        }
    }
}
