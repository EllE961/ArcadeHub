//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System.Windows.Forms;
namespace ArcadeHub.Utilities
{
    /// <summary>
    /// A custom Panel with double buffering enabled to reduce flickering.
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleBufferedPanel"/> class.
        /// </summary>
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
    }
}
