using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.ViewModels
{
    public class SpecialPlaceViewModel : ViewModelBase
    {
        public IBrush Color { get; set; } //TODO:: Change to image
        public int Row { get; set; }
        public int Column { get; set; }

        public ObservableCollection<IBrush> Players { get; set; }

        public SpecialPlaceViewModel(IBrush color, int row, int column)
        {
            Color = color;
            Row = row;
            Column = column;
            Players = new ObservableCollection<IBrush>(); 
        }
    }
}
