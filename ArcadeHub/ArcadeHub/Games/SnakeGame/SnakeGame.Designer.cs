// SnakeGame.Designer.cs
using System.Windows.Forms;

namespace ArcadeHub.Games.SnakeGame
{
    partial class SnakeGame
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Declare the game panel
        private Panel gamePanel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                gamePanel.Dispose(); // Dispose the game panel
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Method required for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            this.gamePanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // gamePanel
            // 
            this.gamePanel.Location = new System.Drawing.Point(100, 80); // Adjust based on background image
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(800, 600); // Set desired game screen size
            this.gamePanel.TabIndex = 0;
            this.gamePanel.BackColor = System.Drawing.Color.Black; // Optional: Background color for game screen
            this.gamePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GamePanel_Paint);
            // 
            // SnakeGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
/*            this.BackgroundImage = Properties.Resources.arcade_background; // Set arcade background
*/            this.BackgroundImageLayout = ImageLayout.Stretch; // Adjust as needed
            this.ClientSize = new System.Drawing.Size(1000, 700); // Adjust to fit background image
            this.Controls.Add(this.gamePanel);
            this.DoubleBuffered = true; // Reduce flickering
            this.Name = "SnakeGame";
            this.Text = "Snake Game";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
