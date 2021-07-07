using Avalonia.Media;
using Monopolio;
using MonopolioGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.ViewModels
{
    public class PropertySelectedViewModel : ViewModelBase
    {
        #region Properties
        private GameState State;

        private IBrush _color;
        public IBrush Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                Raise(this, nameof(Color));
            }
        }

        bool _visible;
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                Raise(this, nameof(Visible));
            }
        }

        string _propertyName;
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                _propertyName = value;
                Raise(this, nameof(PropertyName));
            }
        }

        string _owner;
        public string Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
                Raise(this, nameof(Owner));
            }
        }

        string _price;
        public string Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                Raise(this, nameof(Price));
            }
        }

        string _rent;
        public string Rent
        {
            get
            {
                return _rent;
            }
            set
            {
                _rent = value;
                Raise(this, nameof(Rent));
            }
        }

        string _houses;
        public string Houses
        {
            get
            {
                return _houses;
            }
            set
            {
                _houses = value;
                Raise(this, nameof(Houses));
            }
        }

        string _sellOption;
        public string SellOption
        {
            get
            {
                return _sellOption;
            }
            set
            {
                _sellOption = value;
                Raise(this, nameof(SellOption));
            }
        }

        bool _buildVisible;
        public bool BuildVisible
        {
            get
            {
                return _buildVisible;
            }
            set
            {
                _buildVisible = value;
                Raise(this, nameof(BuildVisible));
            }
        }
        #endregion

        public PropertySelectedViewModel(GameState state)
        {
            State = state;
            PropertyName = "";
            Owner = "";
            Price = "";
            Rent = "";
            Houses = "";
            SellOption = "";
            BuildVisible = false;
            Visible = false;
            Color = Brushes.White;
        }

        public PropertySelectedViewModel(GameState state, PropertyState property)
        {
            Visible = true;
            State = state;
            ChangeProperty(property);
        }

        public void ChangeProperty(PropertyState property)
        {
            Visible = true;
            PropertyName = property.Name;
            Color = ColorConverter(property.Color);

            Owner = property.OwnerName;
            if (property.Mortgaged)
                Owner += " (Hipotecada)";


            Price = string.Format("Preço: {0} €", property.Property.price);
            Rent = string.Format("Renda atual: {0} €", property.Property.rent); //TODO:: Atualizar renda?????
            Houses = string.Format("{0} casas + {1} hotéis", property.Houses, property.Hotels);
            SellOption = (Owner == State.Player) ? "Vender / Hipotecar" : "Comprar";
            BuildVisible = (Owner == State.Player);
        }

        public static IBrush ColorConverter(Property.Color color)
        {
            return color switch
            {
                Property.Color.Brown => Brushes.Brown,
                Property.Color.Cyan => Brushes.Cyan,
                Property.Color.Pink => Brushes.Pink,
                Property.Color.Orange => Brushes.Orange,
                Property.Color.Red => Brushes.Red,
                Property.Color.Yellow => Brushes.Yellow,
                Property.Color.Green => Brushes.Green,
                Property.Color.Blue => Brushes.Blue,
                Property.Color.Station => Brushes.White,
                Property.Color.Utility => Brushes.White,
                _ => throw new ArgumentException("The view model cannot be used with corner squares"),
            };
        }
    }
}
