// DoubleBufferedPanel.cs
using System.Windows.Forms;

namespace ArcadeHub.Games.SnakeGame
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }
    }
}
