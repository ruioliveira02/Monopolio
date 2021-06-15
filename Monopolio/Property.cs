using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    /// <summary>
    /// Represents one property. Does not contain information regading the state of
    /// the property during the game
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Caracters not allowed in a Property's name
        /// </summary>
        public const string control_caracters = "\"\n";

        //IMPORTANT!!! This enum doensn't support non-default values
        public enum Color
        {
            Brown,
            Cyan,
            Pink,
            Orange,
            Red,
            Yellow,
            Green,
            Blue,
            Station,
            Utility,
        }


        public readonly Color color;
        public readonly string name; //mustn't contain the listed control characters
        public readonly int price;
        public readonly int buildPrice; //if 0, building is not allowed (stations/utilities)
        public readonly int[] rent;

        /// <summary>
        /// Creates a Property object from a previously saved Porperty
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="color">The color of the proeprty</param>
        /// <param name="name">The name of the property (mustn't contain control caracters,
        /// otherwise an error is thrown)</param>
        /// <param name="price">The price of the property</param>
        /// <param name="buildPrice">The preice of building in the property</param>
        /// <param name="rent">A 6-long array, containing the rent when no buildings are
        /// placed, when 1-4 houses are placed and when 1 hotel is placed</param>
        [JsonConstructor]
        public Property(Color color, string name, int price, int buildPrice, int[] rent)
        {
            if (name.IndexOfAny(control_caracters.ToCharArray()) != -1)
                throw new ArgumentException("name musn't contain any of the control characters listed");

            this.color = color;
            this.name = name;
            this.price = price;
            this.buildPrice = buildPrice;
            this.rent = rent;
        }

        /// <summary>
        /// Calculates the rent of the property, given a number of buildings
        /// </summary>
        /// <param name="buildings">The number of buildings (1 hotel = 5 houses)</param>
        /// <returns>The corresponding rent</returns>
        public int Rent(int buildings) => rent[buildings];
    }

    /// <summary>
    /// Represents the state of a property during the game
    /// </summary>
    public class PropertyState
    {
        [JsonIgnore]
        public Property Property { get; private set; }

        [JsonIgnore]
        public bool updated;

        private int _houses;
        private int _hotels;
        private Player _owner;
        private bool _mortgaged;

        #region getters

        public int Houses { get => _houses; set {
                if (_houses != value)
                    updated = true;
                _houses = value;
            } }
        public int Hotels { get => _hotels; set
            {
                if (_hotels != value)
                    updated = true;
                _hotels = value;
            }
        }

        [JsonIgnore]
        public Player Owner { get => _owner; set
            {
                if (_owner != value)
                    updated = true;
                _owner = value;
            }
        }
        public bool Mortgaged { get => _mortgaged; set
            {
                if (_mortgaged != value)
                    updated = true;
                _mortgaged = value;
            }
        }

        //used for json serializing (to avoid wrong references and stuff. You don't want to know)
        public string OwnerName { get => _owner?.name; }
        readonly string ownerName;

        public string Name { get => Property.name; }
        readonly string name;

        [JsonIgnore]
        public Property.Color Color { get => Property.color; }

        [JsonIgnore]
        public int Buildings { get => Houses + 5 * Hotels; } //since 4 houses are removed when 
                                                             //upgrading to a hotel, 1 hotel = 5 houses

        #endregion

        /// <summary>
        /// Creates an empty property state for a given property (new game)
        /// </summary>
        /// <param name="property">The given property</param>
        public PropertyState(Property property)
        {
            Property = property;
            _houses = 0;
            _hotels = 0;
        }

        /// <summary>
        /// Creates a property state from a previously saved state
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="ownerName">The owner's name</param>
        /// <param name="mortgaged">Wether the property is mortgaged</param>
        /// <param name="houses">The number of houses on the property</param>
        /// <param name="hotels">The number of hotels on the property</param>
        [JsonConstructor]
        public PropertyState(string propertyName, string ownerName,
            bool mortgaged, int houses, int hotels) //saved game
        {
            this.name = propertyName;
            this.ownerName = ownerName;
            _mortgaged = mortgaged;
            _houses = houses;
            _hotels = hotels;
        }

        /// <summary>
        /// Sets the Owner, searching from among the given players the one with the correct name.
        /// If the owner wasn't found, an error is thrown
        /// </summary>
        /// <param name="players">The given array of players</param>
        public void ResolveOwner(Player[] players)
        {
            if (ownerName == null)
                return;

            foreach (Player p in players)
            {
                if (ownerName == p.name)
                {
                    _owner = p;
                    return;
                }
            }

            throw new Exception("ups");
        }

        /// <summary>
        /// Sets the Property, searching in the board the one with the correct name.
        /// If the property wasn't found, Property is set as null
        /// </summary>
        /// <param name="board">The board</param>
        public void ResolveProperty(Board board)
            => Property = board.GetProperty(name);

        /// <summary>
        /// Increases the number of buildings, following standart monopoly rules
        /// (no buildings -> 1 house -> 2 houses -> 3 houses -> 4 houses -> 1 hotel)
        /// </summary>
        /// <returns>False if the proeprty already has the maximum allowed number of buildings,
        /// in which case no action is performed. True otherwise</returns>
        public bool Upgrade()
        {
            if (Buildings >= State.maxBuildings)
                return false;

            if (Houses == State.housesPerHotel)
            {
                Houses = 0;
                Hotels++;
            }
            else
                Hotels++;

            return true;
        }

        /// <summary>
        /// Removes a building from the property, effectively reversing a call to Upgrade().
        /// </summary>
        /// <returns>False if there are no buildings to remove, true otherwise</returns>
        public bool Downgrade()
        {
            if (Houses == 0)
            {
                if (Hotels == 0)
                    return false;
                else
                {
                    Houses = 4;
                    Hotels--;
                }
            }
            else
                Houses--;

            return true;
        }
    }

    /// <summary>
    /// Represents a group of property states during the game. Relevant because multiple
    /// operations over properties require knowlage about the whole group to be executed
    /// </summary>
    public struct PropertyGroup
    {
        public readonly PropertyState[] properties;

        /// <summary>
        /// Creates a new group of properties (new game)
        /// </summary>
        /// <param name="numberOfProperties">The number of properties the group contains</param>
        public PropertyGroup(int numberOfProperties)
        {
            properties = new PropertyState[numberOfProperties];
        }

        /// <summary>
        /// Creates a property group based on a previously loaded property group
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="properties">The array with the properties in the group
        /// (should all have the same color)</param>
        [JsonConstructor]
        public PropertyGroup(PropertyState[] properties) //saved game
        {
            this.properties = properties;
        }

        #region auxProperties

        [JsonIgnore]
        public Player Owner {
            get
            {
                bool set = false;
                Player ans = null;

                foreach (PropertyState ps in properties)
                {
                    if (!set)
                    {
                        ans = ps.Owner;
                        set = true;
                    }  
                    else if (ans != ps.Owner)
                        return null;
                }

                return ans;
            }
        }

        [JsonIgnore]
        public bool Mortgaged {
            get
            {
                bool ans = false;

                foreach (var ps in properties)
                    ans |= ps.Mortgaged;

                return ans;
            }
        }

        [JsonIgnore]
        public int Buildings {
            get
            {
                int ans = 0;

                foreach (var ps in properties)
                    ans += ps.Buildings;

                return ans;
            }
        }

        [JsonIgnore]
        public bool EvenBuilding {
            get
            {
                int[] b = new int[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                    b[i] = properties[i].Buildings;

                int min = 0, max = 0;

                for (int i = 1; i < properties.Length; i++)
                {
                    if (b[i] < b[min])
                        min = i;
                    else if (b[i] > b[max])
                        max = i;
                }

                return b[max] - b[min] <= 1;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the state of the given property.
        /// If the property isn't in the group, null is returned (compared by reference)
        /// </summary>
        /// <param name="property">The given property</param>
        /// <returns>The state of the property, or null if no match was found</returns>
        public PropertyState GetPropertyState(Property property)
        {
            foreach (var ps in properties)
                if (ps.Property == property)
                    return ps;

            return null;
        }

        /// <summary>
        /// Calculates the rent of a property, based on its state and the current state
        /// of the game.
        /// </summary>
        /// <param name="ps">The state of the property</param>
        /// <param name="s">The current game state</param>
        /// <returns>The rent of the property</returns>
        public int Rent(PropertyState ps, State s)
        {
            if (ps.Mortgaged)
                return 0;
            else if (ps.Color == Property.Color.Station)
            {
                int stations = 0;

                foreach (var ps2 in properties)
                    if (ps2.Owner == ps.Owner)
                        stations++;

                return 25 * (1 << (stations - 1)); // 25 / 50 / 100 / 200
            }
            else if (ps.Color == Property.Color.Utility)
            {
                //TODO: check
                s.ThrowDice();
                int dice = s.Dice[0] + s.Dice[1];
                return Owner == null ? 4 * dice : 10 * dice;
            }
            else
            {
                int b = Buildings;

                if (Owner != null && b == 0)
                    return 2 * ps.Property.Rent(b);
                else
                    return ps.Property.Rent(b);
            }
        }

        #region builds

        /// <summary>
        /// Increases the number of buildings on the given property by 1, or unmorgages the
        /// property if it is currently mortgaged (provided the owner has enough money)
        /// </summary>
        /// <param name="property">The given property</param>
        /// <param name="checkOnly">If true, no changes are made to the state, only 
        /// calculating the return value</param>
        /// <returns>False when:
        /// -the owner can't pay
        /// -the group isn't a monopoly
        /// -the property already has the maximum allowed number of buildings (1 hotel = 5 houses)
        /// -the build doesn't respect the even building rule: in a group, the difference
        ///  between the maximum and minimum number of buildings must be 1 or less 
        /// -the property isn't mortgaged and the buildPrice of the property is 0
        ///  (signaling that building on the property is not allowed)
        /// </returns>
        public bool Build(PropertyState property, bool checkOnly = false)
        {
            if (Mortgaged)
            {
                int cost = (int)(0.55 * property.Property.price);

                if (property.Owner.Money < cost)
                    return false;

                property.Owner.Money -= cost;
                return true;
            }

            if (Owner == null)
                return false;

            bool aux = property.Upgrade();
            bool ans = aux && EvenBuilding && property.Owner.Money >= property.Property.buildPrice;

            if (checkOnly && aux)
                property.Downgrade();

            if (!checkOnly && ans)
                property.Owner.Money -= property.Property.buildPrice;

            return ans;
        }

        //If the property has houses/hotels, one of them is returned to the bank.
        //Otherwise, the property is mortgaged. TODO: change?
        /// <summary>
        /// If the property has buildings, one is sold back to the bank. (If its sale isn't
        /// against the even building rule). Otherwise, the property is mortgaged.
        /// </summary>
        /// <param name="ps">The property</param>
        /// <returns>True if successfully executed, false otherwise</returns>
        public bool Mortgage(PropertyState ps)
        {
            if (ps.Owner == null)
                return false;
            else if (ps.Buildings == 0)
            {
                if (Buildings > 0 || ps.Mortgaged)
                    return false;

                ps.Owner.Money += ps.Property.price / 2;
                ps.Mortgaged = true;
            }
            else
            {
                ps.Downgrade();

                if (EvenBuilding)
                    ps.Owner.Money += ps.Property.buildPrice / 2;
                else
                {
                    ps.Upgrade();
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}