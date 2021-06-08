using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Monopolio
{
    /// <summary>
    /// Represents a board template, similar to a box with the board and the cards
    /// </summary>
    public class Board
    {
        /// <summary>
        /// Loads a board template in JSON format from the specified file
        /// </summary>
        /// <param name="file">The specified file</param>
        /// <returns>The stored board template</returns>
        public static Board LoadBoard(string file)
        {
            string json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<Board>(json);
        }


        public Square[] Squares { get; }
        public Card[] Chance { get; }
        public Card[] CommunityChest { get; }

        /// <summary>
        /// Loads a board template from a previously saved board
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="squares">The array of squares on the board</param>
        /// <param name="chance">The array of chance cards</param>
        /// <param name="communityChest">The array of community chest cards</param>
        [JsonConstructor]
        public Board(Square[] squares, Card[] chance, Card[] communityChest)
        {
            Squares = squares;
            Chance = chance;
            CommunityChest = communityChest;
        }

        /// <summary>
        /// Fetches the square at the specified position
        /// </summary>
        /// <param name="position">The specified posisiton</param>
        /// <returns>The board square</returns>
        public Square GetSquare(int position) => Squares[position];

        /// <summary>
        /// Retrieves the property with the specified name
        /// (if multiple properties with the specified name exist, the first is returned)
        /// </summary>
        /// <param name="propertyName">The specified name</param>
        /// <returns>The property</returns>
        public Property GetProperty(string propertyName)
        {
            foreach (Square s in Squares)
                if (s.type == Square.Type.Property && s.property.name == propertyName)
                    return s.property;

            return null;
        }

        /// <summary>
        /// Sorts all properties into groups, based on their color
        /// </summary>
        /// <returns>An array of groups, where the n-th group contains all
        /// properties with color n</returns>
        public PropertyGroup[] GetPropertyGroups()
        {
            PropertyGroup[] groups = new PropertyGroup[Enum.GetNames(typeof(Property.Color)).Length];
            int[] aux = new int[groups.Length]; //number of properties of each color

            //count how many properties are there of each color
            foreach (Square s in Squares)
                if (s.type == Square.Type.Property)
                    aux[(int)s.Color]++;

            //assign the arrays and restart the counter
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new PropertyGroup(aux[i]);
                aux[i] = 0;
            }

            //set the properties
            foreach (Square s in Squares)
                if (s.type == Square.Type.Property)
                    groups[(int)s.Color].properties[aux[(int)s.Color]++] = new PropertyState(s.property);

            return groups;
        }

        #region movement

        /// <summary>
        /// Advances the player a spacified number of spaces and gives him his salary if he
        /// moves into a Start square
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="spaces">The specified number of spaces. If negative, the player walks
        /// backwards (still receives his salary if he passes Start)</param>
        /// <returns>The board square the player landed on</returns>
        public Square Walk(Player player, int spaces)
        {
            int dest = ((player.Position + spaces) % Squares.Length + Squares.Length) % Squares.Length;

            while (player.Position != dest)
            {
                player.Position = ((spaces > 0 ? player.Position + 1 : player.Position - 1)
                    % Squares.Length + Squares.Length) % Squares.Length;

                if (Squares[player.Position].type == Square.Type.Start)
                    player.Money += State.salary;
            }

            return Squares[player.Position];
        }

        /// <summary>
        /// Sends the player directly to jail (doesn't receive his salary if he passes Start).
        /// If multiple Jail squares exist, the first one after the player is chosen.
        /// If no Jail square is found, an error is thrown
        /// </summary>
        /// <param name="player">The player</param>
        /// <returns>The Jail square the player landed on</returns>
        public Square SendToJail(Player player)
        {
            if (Squares[player.Position].type == Square.Type.Jail)
                return Squares[player.Position];

            for (int i = (player.Position + 1) % Squares.Length; i != player.Position; i = (i + 1) % Squares.Length)
            {
                if (Squares[i].type == Square.Type.Jail)
                {
                    player.Position = i;
                    return Squares[player.Position];
                }
            }

            throw new Exception("Jail not found :/");
        }

        delegate bool Stop(Square s);

        /// <summary>
        /// Advances a player until the given condition is met. If no square on the board
        /// satisfies the condition, an error is thrown.
        /// The player receives his salary if he moves into a Start square
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="condition">The given condition (dependant only on the square)</param>
        /// <returns>The square the player landed on</returns>
        Square Advance(Player player, Stop condition)
        {
            if (condition(Squares[player.Position]))
                return Squares[player.Position];

            for (int i = (player.Position + 1) % Squares.Length; i != player.Position; i = (i + 1) % Squares.Length)
            {
                if (Squares[i].type == Square.Type.Start)
                    player.Money += State.salary;
                else if (condition(Squares[i]))
                {
                    player.Position = i;
                    return Squares[i];
                }
            }

            throw new Exception("Square not found");
        }

        /// <summary>
        /// Advances the player to the next property with the given name
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="propertyName">The given name</param>
        /// <returns>The square the player landed on</returns>
        public Square AdvanceToProperty(Player player, string propertyName)
            => Advance(player, (Square s) => s.type == Square.Type.Property && s.property.name == propertyName);

        /// <summary>
        /// Advances the player to the next property with the given color
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="propertyName">The given color</param>
        /// <returns>The square the player landed on</returns>
        public Square AdvanceToNearest(Player player, Property.Color color)
            => Advance(player, (Square s) => s.type == Square.Type.Property && s.property.color == color);

        /// <summary>
        /// Advances the player to the next Start square
        /// </summary>
        /// <param name="player">The player</param>
        /// <returns>The square the player landed on</returns>
        public Square AdvanceToStart(Player player)
            => Advance(player, (Square s) => s.type == Square.Type.Start);

        #endregion
    }

    /// <summary>
    /// Represents one square of the board
    /// </summary>
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

        [JsonIgnore]
        public Property.Color Color { get => property.color; }

        [JsonIgnore]
        public string Name { get => name != null ? name : property?.name; }

        /// <summary>
        /// Creates a Square object from a previously saved square
        /// (used for JSON deserialization)
        /// </summary>
        /// <param name="type">The type of the square</param>
        /// <param name="property">If a proeprty, the corresponding Property object</param>
        /// <param name="name">The name to be displayed on the board (if null, Name
        /// defaults to the name of the property, or null if the property is null)</param>
        /// <param name="tax">If a tax square, the corresponding tax value</param>
        [JsonConstructor]
        public Square(Type type, Property property = null, string name = null, int tax = 0)
        {
            this.type = type;
            this.property = property;
            this.name = name;
            this.tax = tax;
        }
    }
}