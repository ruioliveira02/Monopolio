using System;
using System.Collections.Generic;
using System.Text;

namespace Monopolio
{
    public struct Square
    {
        public enum Type
        {
            Start,
            Property,
            CommunityChest,
            Tax,
            Chance,
            Jail,
            FreeParking,
            GoToJail,
        }

        public readonly Type type;
        public readonly Property property;  //se type = property
        public readonly int tax;            //se type = tax

        public Property.Color Color { get => property.color; }

        public Square(Type type, Property property = null, int tax = 0)
        {
            this.type = type;
            this.property = property;
            this.tax = tax;
        }
    }

    public class Board //não tem setters/acesso público às variáveis
    {
        Square[] squares;
        //cartas

        public Board(string file) //load board from file (json?)
        {
            //TODO
        }

        public Square GetSquare(int position) => squares[position];

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

        //gives the player his salary when passing through the start square
        public Square Walk(Player player, int spaces)
        {
            for (int i = 0; i < spaces; i++)
            {
                player.Position = (player.Position + 1) % squares.Length;

                if (squares[player.Position].type == Square.Type.Start)
                    player.Money += State.salary;
            }   

            return squares[player.Position];
        }

        public Square SendToJail(Player player)
        {
            if (squares[player.Position].type != Square.Type.Jail)
            {
                for (int i = (player.Position + 1) % squares.Length; i != player.Position; i = (i + 1) % squares.Length)
                {
                    if (squares[i].type == Square.Type.Jail)
                    {
                        player.Position = i;
                        break;
                    }
                }
            }

            if (!player.GetOutOfJailFreeCard)
                player.InJail = 1;

            return squares[player.Position];
        }
    }
}