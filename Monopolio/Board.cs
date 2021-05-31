using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Monopolio
{
    public class Board //não tem setters/acesso público às variáveis
    {
        public static Board LoadBoard(string file)
        {
            string json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<Board>(json);
        }


        readonly Square[] squares;
        public Card[] Chance { get; }
        public Card[] CommunityChest { get; }

        [JsonConstructor]
        public Board(Square[] squares, Card[] chance, Card[] communityChest)
        {
            this.squares = squares;
            Chance = chance;
            CommunityChest = communityChest;
        }

        public Square GetSquare(int position) => squares[position];

        public Property GetProperty(string propertyName)
        {
            foreach (Square s in squares)
                if (s.type == Square.Type.Property && s.property.name == propertyName)
                    return s.property;

            return null;
        }

        public PropertyGroup[] GetPropertyGroups()
        {
            PropertyGroup[] groups = new PropertyGroup[Enum.GetNames(typeof(Property.Color)).Length];
            int[] aux = new int[groups.Length]; //number of properties of each color

            //count how many properties are there of each color
            foreach (Square s in squares)
                if (s.type == Square.Type.Property)
                    aux[(int)s.Color]++;

            //assign the arrays and restart the counter
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new PropertyGroup(aux[i]);
                aux[i] = 0;
            }

            //set the properties
            foreach (Square s in squares)
                if (s.type == Square.Type.Property)
                    groups[(int)s.Color].properties[aux[(int)s.Color]++] = new PropertyState(s.property);

            return groups;
        }

        #region movement

        //gives the player his salary when passing through the start square
        //accepts negative values for spaces. the player walks backwards (still receivs
        //his salary when passing Start)
        public Square Walk(Player player, int spaces)
        {
            int dest = ((player.Position + spaces) % squares.Length + squares.Length) % squares.Length;

            while (player.Position != dest)
            {
                player.Position = spaces > 0 ? player.Position + 1 : player.Position - 1;

                if (squares[player.Position].type == Square.Type.Start)
                    player.Money += State.salary;
            }

            return squares[player.Position];
        }

        public Square SendToJail(Player player)
        {
            if (squares[player.Position].type == Square.Type.Jail)
            {
                if (player.GetOutOfJailFreeCards == 0)
                    player.InJail = 1;

                return squares[player.Position];
            }

            for (int i = (player.Position + 1) % squares.Length; i != player.Position; i = (i + 1) % squares.Length)
            {
                if (squares[i].type == Square.Type.Jail)
                {
                    player.Position = i;

                    if (player.GetOutOfJailFreeCards == 0)
                        player.InJail = 1;

                    return squares[player.Position];
                }
            }

            throw new Exception("Prision not found :/");
        }

        delegate bool Stop(Square s);

        //advances the player until a condition is satisfied. If no square
        //on the board satisfies the condition, an error is thrown.
        Square Advance(Player player, Stop condition)
        {
            if (condition(squares[player.Position]))
                return squares[player.Position];

            for (int i = (player.Position + 1) % squares.Length; i != player.Position; i = (i + 1) % squares.Length)
            {
                if (squares[i].type == Square.Type.Start)
                    player.Money += State.salary;
                else if (condition(squares[i]))
                {
                    player.Position = i;
                    return squares[i];
                }
            }

            throw new Exception("Property not found");
        }

        public Square AdvanceToProperty(Player player, string propertyName)
            => Advance(player, (Square s) => s.type == Square.Type.Property && s.property.name == propertyName);

        public Square AdvanceToNearest(Player player, Property.Color color)
            => Advance(player, (Square s) => s.type == Square.Type.Property && s.property.color == color);

        public Square AdvanceToStart(Player player)
            => Advance(player, (Square s) => s.type == Square.Type.Start);

        #endregion
    }

    public struct Square
    {
        public enum Type
        {
            Start,
            Property,
            Chance,
            CommunityChest,
            Tax,
            Jail,
            FreeParking,
            GoToJail,
        }

        public readonly Type type;
        public readonly Property property;  //se type = property
        public readonly int tax;            //se type = tax
        public readonly string name;        //se type = tax

        public Property.Color Color { get => property.color; }

        [JsonConstructor]
        public Square(Type type, Property property = null, int tax = 0, string name = null)
        {
            this.type = type;
            this.property = property;
            this.tax = tax;
            this.name = null;
        }
    }
}