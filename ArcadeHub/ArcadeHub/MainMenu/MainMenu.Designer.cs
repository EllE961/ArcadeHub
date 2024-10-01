// MainMenuForm.Designer.cs
using System.Windows.Forms;

namespace ArcadeHub.MainMenu
{
    partial class MainMenuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Declare menu buttons and title label
        private Button btnSnake;
        private Button btnTetris;
        private Button btnPong;
        private Label lblTitle;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                btnSnake.Dispose();
                btnTetris.Dispose();
                btnPong.Dispose();
                lblTitle.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Method required for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSnake = new System.Windows.Forms.Button();
            this.btnTetris = new System.Windows.Forms.Button();
            this.btnPong = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSnake
            // 
            this.btnSnake.Location = new System.Drawing.Point(350, 250); // Adjust based on background image
            this.btnSnake.Name = "btnSnake";
            this.btnSnake.Size = new System.Drawing.Size(300, 50);
            this.btnSnake.TabIndex = 0;
            this.btnSnake.Text = "Snake";
            this.btnSnake.UseVisualStyleBackColor = true;
            this.btnSnake.Click += new System.EventHandler(this.btnSnake_Click);
            // 
            // btnTetris
            // 
            this.btnTetris.Location = new System.Drawing.Point(350, 320); // Adjust based on background image
            this.btnTetris.Name = "btnTetris";
            this.btnTetris.Size = new System.Drawing.Size(300, 50);
            this.btnTetris.TabIndex = 1;
            this.btnTetris.Text = "Tetris";
            this.btnTetris.UseVisualStyleBackColor = true;
            this.btnTetris.Click += new System.EventHandler(this.btnTetris_Click);
            // 
            // btnPong
            // 
            this.btnPong.Location = new System.Drawing.Point(350, 390); // Adjust based on background image
            this.btnPong.Name = "btnPong";
            this.btnPong.Size = new System.Drawing.Size(300, 50);
            this.btnPong.TabIndex = 2;
            this.btnPong.Text = "Pong";
            this.btnPong.UseVisualStyleBackColor = true;
            this.btnPong.Click += new System.EventHandler(this.btnPong_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White; // Ensure visibility against background
            this.lblTitle.Location = new System.Drawing.Point(400, 150); // Adjust based on background image
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 45);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "Arcade Hub";
            // 
            // MainMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
/*            this.BackgroundImage = Properties.Resources.arcade_background; // Set the same arcade background
*/            this.BackgroundImageLayout = ImageLayout.Stretch; // Adjust as needed
            this.ClientSize = new System.Drawing.Size(1000, 700); // Match SnakeGame's form size
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnPong);
            this.Controls.Add(this.btnTetris);
            this.Controls.Add(this.btnSnake);
            this.DoubleBuffered = true; // Reduce flickering
            this.Name = "MainMenuForm";
            this.Text = "Arcade Hub";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
