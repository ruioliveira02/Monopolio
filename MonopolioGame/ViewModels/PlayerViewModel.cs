using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.ViewModels
{
    /// <summary>
    /// View Model for each player in the navbar with all the players' info
    /// </summary>
    public class PlayerViewModel : ViewModelBase
    {
        /// <summary>
        /// If the player being displayed is ours, then we display more information,
        /// like the player's money
        /// </summary>
        public bool IsPlayer { get; set; }

        /// <summary>
        /// Whether or not it is the player's turn
        /// </summary>
        public bool IsCurrentTurn { get; set; }

        /// <summary>
        /// Whether the player is disconnected or not
        /// </summary>
        public bool IsDisconnected { get; set; }

        /// <summary>
        /// The name of the player
        /// </summary>
        public string Name { get; set; }

        //TODO:: Change to image
        /// <summary>
        /// The profile picture of the player
        /// </summary>
        public IBrush PlayerImage { get; set; }

        /// <summary>
        /// The player's money (will only be displayed on client's player
        /// </summary>
        public string Money { get; set; }

        public PlayerViewModel(bool isPlayer, bool isCurrentTurn, bool isDisconnected, string name, IBrush image, int money)
        {
            IsPlayer = isPlayer;
            IsCurrentTurn = isCurrentTurn;
            IsDisconnected = isDisconnected;
            Name = name;
            PlayerImage = image;
            Money = string.Format("{0} €", money.ToString());
        }
    }
}
