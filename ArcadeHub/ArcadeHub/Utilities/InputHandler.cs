//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System.Windows.Forms;
using ArcadeHub.Models;

namespace ArcadeHub.Utilities
{
    public static class InputHandler
    {
        public static Direction GetDirection(Keys key, Direction currentDirection)
        {
            switch (key)
            {
                case Keys.Right:
                    return currentDirection != Direction.Left ? Direction.Right : currentDirection;
                case Keys.Left:
                    return currentDirection != Direction.Right ? Direction.Left : currentDirection;
                case Keys.Up:
                    return currentDirection != Direction.Down ? Direction.Up : currentDirection;
                case Keys.Down:
                    return currentDirection != Direction.Up ? Direction.Down : currentDirection;
                default:
                    return currentDirection;
            }
        }
    }
}
