//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System.Drawing;

namespace ArcadeHub.Utilities
{
    public static class GraphicsHelper
    {
        public static void DrawCircle(Graphics canvas, Brush brush, Rectangle rect)
        {
            canvas.FillEllipse(brush, rect);
        }

        // Additional helper methods can be added here
    }
}
