using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    public class Property
    {
        public const string control_caracters = "\"\n";

        //IMPORTANTE!! os valores teem de ir de 0 até n-1
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
        public readonly Property property;
        public int Houses { get; set; }
        public int Hotels { get; set; }

        public string Name { get => property.name; }

        public Property.Color Color { get => property.color; }

        //new game
        public PropertyState(Property property)
        {
            this.property = property;
            Houses = 0;
            Hotels = 0;
        }

        //saved game
        public PropertyState(Property property, int houses, int hotels)
        {
            this.property = property;
            Houses = houses;
            Hotels = hotels;
        }

        public Player Owner { get; set; }

        public int Buildings { get => Houses + Hotels; }
    }

    public struct PropertyGroup
    {
        public readonly PropertyState[] properties;

        public PropertyGroup(int numberOfProperties)
        {
            properties = new PropertyState[numberOfProperties];
        }

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

        public int Buildings {
            get
            {
                int ans = 0;

                foreach (var ps in properties)
                    ans += ps.Buildings;

                return ans;
            }
        }

        public bool EvenBuilding {
            get
            {
                int[] b = new int[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                    b[i] = properties[i].Buildings;

                int min = 0, max = 0;

                for (int i = 0; i < properties.Length; i++)
                {
                    if (b[i] < b[min])
                        min = i;
                    else if (b[i] > b[max])
                        max = i;
                }

                return max - min <= 1;
            }
        }

        public PropertyState GetPropertyState(Property property)
        {
            foreach (var ps in properties)
                if (ps.property == property)
                    return ps;

            return null;
        }

        public int Rent(PropertyState ps)
        {
            if (ps.Color == Property.Color.Station)
            {
                int stations = 0;

                foreach (var ps2 in properties)
                    if (ps2.Owner == ps.Owner)
                        stations++;

                return 25 * (1 << (stations - 1)); // 25 / 50 / 100 / 200
            }
            else if (ps.Color == Property.Color.Utility)
            {
                int dice = State.randomizer.Next(1, 7);
                return Owner == null ? 4 * dice : 10 * dice;
            }
            else
                return ps.property.Rent(Buildings);
        }

        //Returns false when:
        //-the group isn't a monopoly
        //-the build doesn't respect the even building rule (in a group, the difference
        // between the maximum and minimum number of buildings must be 1 or less)
        //-the group already has the maximum number of buildings (4 houses + 1 hotel)
        //-the buildPrice of the property is 0 (signaling that building on the property is not allowed)
        public bool CanBuild(Property property)
        {
            if (Owner == null)
                return false;

            int[] b = new int[properties.Length];
            int total = 0;

            for (int i = 0; i < properties.Length; i++)
            {
                b[i] = properties[i].Buildings;
                total += b[i];

                if (properties[i].property == property)
                    b[i]++;
            }

            if (total >= 5 || property.buildPrice == 0)
                return false;

            int min = 0, max = 0;

            for (int i = 0; i < properties.Length; i++)
            {
                if (b[i] < b[min])
                    min = i;
                else if (b[i] > b[max])
                    max = i;
            }

            return max - min <= 1;
        }

        public bool Build(Property property)
        {
            if (CanBuild(property))
            {
                var ps = GetPropertyState(property);

                if (Buildings >= 4)
                    ps.Hotels++;
                else
                    ps.Houses++;

                return true;
            }
            else
                return false;
        }

        public bool Mortgage(PropertyState ps)
        {
            if (Owner == null || Buildings == 0)
            {
                ps.Owner.Money += ps.property.price / 2;
                ps.Owner = null;
            }
            else if (ps.Buildings == 0)
                return false;
            else if (ps.Hotels > 0)
            {
                ps.Hotels--;

                if (!EvenBuilding)
                {
                    ps.Hotels++;
                    return false;
                }
            }
            else
            {
                ps.Houses--;

                if (!EvenBuilding)
                {
                    ps.Houses++;
                    return false;
                }
            }

            ps.Owner.Money += ps.property.buildPrice / 2;
            return true;
        }
    }
}