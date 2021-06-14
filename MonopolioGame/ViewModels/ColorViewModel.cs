using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.ViewModels
{
    public class ColorViewModel : ViewModelBase
    {
        public IBrush Color { get; set; }
        public ColorViewModel(IBrush color)
        {
            Color = color;
        }
    }
}
