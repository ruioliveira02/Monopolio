using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    public class Property
    {
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

        public int Rent(int buildings) => rent[buildings];
    }

    public class PropertyState
    {
        [JsonIgnore]
        public Property Property { get; private set; }
        public int Houses { get; set; }
        public int Hotels { get; set; }

        [JsonIgnore]
        public Player Owner { get; set; }
        public bool Mortgaged { get; set; }

        //used for json serializing (to avoid wrong references and stuff. You don't want to know)
        public string OwnerName { get => Owner?.name; }
        readonly string ownerName;

        public string PrpertyName { get => Property.name; }
        readonly string propertyName;


        [JsonIgnore]
        public string Name { get => Property.name; }

        [JsonIgnore]
        public Property.Color Color { get => Property.color; }

        [JsonIgnore]
        public int Buildings { get => Houses + 5 * Hotels; } //since 4 houses are removed when 
                                                             //upgrading to a hotel, 1 hotel = 5 houses


        public PropertyState(Property property) //new game
        {
            Property = property;
            Houses = 0;
            Hotels = 0;
        }

        [JsonConstructor]
        public PropertyState(string propertyName, string ownerName,
            bool mortgaged, int houses, int hotels) //saved game
        {
            this.propertyName = propertyName;
            this.ownerName = ownerName;
            Mortgaged = mortgaged;
            Houses = houses;
            Hotels = hotels;
        }

        public void ResolveOwner(Player[] players)
        {
            if (ownerName == null)
                return;

            foreach (Player p in players)
            {
                if (ownerName == p.name)
                {
                    Owner = p;
                    return;
                }
            }

            throw new Exception("ups");
        }

        public void ResolveProperty(Board board)
            => Property = board.GetProperty(propertyName);


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

    public struct PropertyGroup
    {
        public readonly PropertyState[] properties;

        public PropertyGroup(int numberOfProperties) //new game
        {
            properties = new PropertyState[numberOfProperties];
        }

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

        public PropertyState GetPropertyState(Property property)
        {
            foreach (var ps in properties)
                if (ps.Property == property)
                    return ps;

            return null;
        }

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

        //when checkOnly is true, no changes are made to the property
        //If the property is mortgaged, it is unmortgaged (provided the owner has enough money)
        //Returns false when:
        //-the owner can't pay
        //-the group isn't a monopoly
        //-the build doesn't respect the even building rule: in a group, the difference
        // between the maximum and minimum number of buildings must be 1 or less (1 hotel = 5 houses)
        //-the property can't be upgraded anymore (already has 1 hotel)
        //-the buildPrice of the property is 0 (signaling that building on the property is not allowed)
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
        public bool Mortgage(PropertyState ps)
        {
            if (ps.Owner == null)
                return false;
            else if (ps.Buildings == 0)
            {
                if (Buildings > 0)
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