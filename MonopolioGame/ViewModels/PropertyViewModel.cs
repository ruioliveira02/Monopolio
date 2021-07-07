using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.ViewModels
{
    public class PropertyViewModel : ViewModelBase
    {
        public int Index { get; set; }
        public IBrush Color { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string Name { get; set; }

        public string Cost { get; set; }

        public ObservableCollection<ColorViewModel> Players { get; set; }
        public IBrush ColorPlayer { get; set; }
        public PropertyViewModel(IBrush color, int index, string name, int cost)
        {
            Index = index;
            Players = new ObservableCollection<ColorViewModel>();
            Color = color;

            if(index < 10)
            {
                Row = 10;
                Column = 10 - index;
            }
            else if(index < 20)
            {
                Row = 20 - index;
                Column = 0;
            }
            else if(index < 30)
            {
                Row = 0;
                Column = index - 20;
            }
            else
            {
                Row = index - 30;
                Column = 10;
            }
            Name = name;
            Cost = string.Format("{0} €", cost.ToString());
            ColorPlayer = Brushes.Red;
            return;
        }
    }
}
