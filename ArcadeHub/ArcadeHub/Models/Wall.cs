//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System.Collections.Generic;
using System.Drawing;

namespace ArcadeHub.Models
{
    /// <summary>
    /// Represents a wall composed of multiple connected blocks in the Snake game.
    /// </summary>
    public class Wall
    {
        /// <summary>
        /// The list of grid positions that make up the wall.
        /// </summary>
        public List<Point> Blocks { get; set; }

        /// <summary>
        /// The current state of the wall (flashing or solid).
        /// </summary>
        public WallState State { get; set; }

        /// <summary>
        /// Counter to manage the flashing duration.
        /// </summary>
        public int FlashCounter { get; set; }

        /// <summary>
        /// The color of the wall based on its state.
        /// </summary>
        public Color WallColor
        {
            get
            {
                return State == WallState.Flashing ? Color.Yellow : Color.Gray;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wall"/> class.
        /// </summary>
        /// <param name="blocks">The list of grid positions that make up the wall.</param>
        /// <param name="initialFlashDuration">Initial flashing duration in game ticks.</param>
        public Wall(List<Point> blocks, int initialFlashDuration)
        {
            Blocks = blocks;
            State = WallState.Flashing;
            FlashCounter = initialFlashDuration;
        }
    }

    /// <summary>
    /// Enumerates the possible states of a wall.
    /// </summary>
    public enum WallState
    {
        Flashing,
        Solid
    }
}
