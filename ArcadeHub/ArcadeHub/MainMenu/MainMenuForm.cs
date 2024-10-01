// MainMenuForm.cs
using System;
using System.Windows.Forms;
using ArcadeHub.Games.SnakeGame;
// Uncomment when Tetris and Pong are implemented
// using ArcadeHub.Games.TetrisGame;
// using ArcadeHub.Games.PongGame;

namespace ArcadeHub.MainMenu
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void btnSnake_Click(object sender, EventArgs e)
        {
            SnakeGame snake = new SnakeGame();
            snake.FormClosed += (s, args) => this.Show();
            snake.Show();
            this.Hide();
        }

        private void btnTetris_Click(object sender, EventArgs e)
        {
            // Uncomment when TetrisGame is implemented
            // TetrisGame tetris = new TetrisGame();
            // tetris.FormClosed += (s, args) => this.Show();
            // tetris.Show();
            MessageBox.Show("Tetris not implemented yet.");
        }

        private void btnPong_Click(object sender, EventArgs e)
        {
            // Uncomment when PongGame is implemented
            // PongGame pong = new PongGame();
            // pong.FormClosed += (s, args) => this.Show();
            // pong.Show();
            MessageBox.Show("Pong not implemented yet.");
        }
    }
}
