// SnakeGame.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using ArcadeHub.Models;
using ArcadeHub.Utilities;

namespace ArcadeHub.Games.SnakeGame
{
    public partial class SnakeGame : Form
    {
        private List<Circle> Snake = new List<Circle>();
        private Circle food = new Circle();
        private int score;

        // NAudio objects
        private WaveOutEvent backgroundPlayer;
        private AudioFileReader backgroundReader;
        private LoopStream loopBackgroundStream;

        private WaveOutEvent gameOverPlayer;
        private AudioFileReader gameOverReader;

        private Timer gameTimer; // Declared at class level
        private Settings settings;
        private HighScores highScores;
        private bool gameStarted = false;
        private bool isPaused = false;
        private Random rand = new Random(); // Initialize once to prevent repetition
        private Point tailPrevPosition; // To store the tail's previous position

        // Direction input queue
        private Queue<Direction> directionQueue = new Queue<Direction>();

        public SnakeGame()
        {
            InitializeComponent(); // Initialize form components first
            InitializeSound();     // Initialize sound components
            InitializeGame();      // Then initialize game-specific components
        }

        private void InitializeSound()
        {
            try
            {
                string soundDirectory = Path.Combine(Application.StartupPath, "Resources", "Sounds");
                string backgroundPath = Path.Combine(soundDirectory, "background.wav");
                string gameOverPath = Path.Combine(soundDirectory, "gameover.wav");

                // Initialize Background Music with LoopStream
                if (File.Exists(backgroundPath))
                {
                    backgroundReader = new AudioFileReader(backgroundPath);
                    loopBackgroundStream = new LoopStream(backgroundReader);
                    backgroundPlayer = new WaveOutEvent();
                    backgroundPlayer.Init(loopBackgroundStream);
                }
                else
                {
                    MessageBox.Show("Background sound file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Initialize Game Over Sound
                if (File.Exists(gameOverPath))
                {
                    gameOverReader = new AudioFileReader(gameOverPath);
                    gameOverPlayer = new WaveOutEvent();
                    gameOverPlayer.Init(gameOverReader);
                }
                else
                {
                    MessageBox.Show("Game Over sound file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during sound initialization: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeGame()
        {
            try
            {
                settings = Settings.Load();
                highScores = new HighScores();

                // Initialize the Timer
                gameTimer = new Timer();
                if (gameTimer == null)
                {
                    MessageBox.Show("Failed to initialize gameTimer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                gameTimer.Interval = 1000 / settings.Speed;
                gameTimer.Tick += GameLoop;

                // Initialize the game state
                RestartGame(initial: true);

                // Display initial message
                MessageBox.Show("Press any key to start the game!", "Snake Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during game initialization: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestartGame(bool initial = false)
        {
            try
            {
                Snake.Clear();
                Circle head = new Circle { X = 10, Y = 5 };
                Snake.Add(head);
                score = 0;
                GenerateFood();
                settings.Direction = Direction.Right;

                if (!initial)
                {
                    // Stop previous background music if any
                    backgroundPlayer?.Stop();
                    loopBackgroundStream?.Dispose();
                    backgroundReader?.Dispose();
                    backgroundPlayer?.Dispose();

                    // Re-initialize background music
                    InitializeSound();
                    backgroundPlayer?.Play();
                }
                else
                {
                    // Ensure the timer is not running initially
                    gameTimer?.Stop();
                }

                gameStarted = false; // Reset the game started flag
                isPaused = false;    // Reset the pause flag

                // Clear any pending direction changes
                directionQueue.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while restarting the game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateFood()
        {
            try
            {
                bool validPosition = false;

                while (!validPosition)
                {
                    food = new Circle
                    {
                        X = rand.Next(0, gamePanel.Width / settings.Size),
                        Y = rand.Next(0, gamePanel.Height / settings.Size)
                    };

                    // Ensure food is not spawned on the snake
                    validPosition = true;
                    foreach (var segment in Snake)
                    {
                        if (segment.X == food.X && segment.Y == food.Y)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    // Additional check to prevent overlapping after growth
                    if (validPosition && Snake.Count > 0)
                    {
                        // Prevent immediate collision after eating
                        if (Snake[0].X == food.X && Snake[0].Y == food.Y)
                            validPosition = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating food: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            try
            {
                // Process the next direction change if available
                if (directionQueue.Count > 0)
                {
                    Direction nextDir = directionQueue.Dequeue();

                    // Predict the new head position
                    Point currentHead = new Point(Snake[0].X, Snake[0].Y);
                    Point newHead = GetNextPosition(currentHead, nextDir);

                    // Check if the new position is where the first body segment is
                    if (Snake.Count > 1)
                    {
                        Point firstBody = new Point(Snake[1].X, Snake[1].Y);
                        if (newHead == firstBody)
                        {
                            // Invalid direction change, ignore
                            // Optionally, you can log this event or provide feedback
                        }
                        else
                        {
                            // Valid direction change, apply it
                            settings.Direction = nextDir;
                        }
                    }
                    else
                    {
                        // No body segments, safe to change direction
                        settings.Direction = nextDir;
                    }
                }

                MoveSnake();
                CheckCollision();
                gamePanel.Invalidate(); // Triggers the gamePanel's Paint event
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the game loop: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Point GetNextPosition(Point currentPos, Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return new Point(currentPos.X + 1, currentPos.Y);
                case Direction.Left:
                    return new Point(currentPos.X - 1, currentPos.Y);
                case Direction.Up:
                    return new Point(currentPos.X, currentPos.Y - 1);
                case Direction.Down:
                    return new Point(currentPos.X, currentPos.Y + 1);
                default:
                    return currentPos;
            }
        }

        private void MoveSnake()
        {
            try
            {
                // Store tail's previous position before moving
                tailPrevPosition = new Point(Snake[Snake.Count - 1].X, Snake[Snake.Count - 1].Y);

                for (int i = Snake.Count - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        switch (settings.Direction)
                        {
                            case Direction.Right:
                                Snake[i].X++;
                                break;
                            case Direction.Left:
                                Snake[i].X--;
                                break;
                            case Direction.Up:
                                Snake[i].Y--;
                                break;
                            case Direction.Down:
                                Snake[i].Y++;
                                break;
                        }

                        // Wrap around the screen
                        if (Snake[i].X < 0) Snake[i].X = gamePanel.Width / settings.Size - 1;
                        if (Snake[i].Y < 0) Snake[i].Y = gamePanel.Height / settings.Size - 1;
                        if (Snake[i].X >= gamePanel.Width / settings.Size) Snake[i].X = 0;
                        if (Snake[i].Y >= gamePanel.Height / settings.Size) Snake[i].Y = 0;
                    }
                    else
                    {
                        Snake[i].X = Snake[i - 1].X;
                        Snake[i].Y = Snake[i - 1].Y;
                    }
                }

                // Check if snake has eaten the food
                if (Snake[0].X == food.X && Snake[0].Y == food.Y)
                {
                    Eat();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while moving the snake: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Eat()
        {
            try
            {
                score += settings.ScoreIncrement;
                PlayEatSound(); // Play eating sound asynchronously

                // Add new segment at the tail's previous position
                Circle newCircle = new Circle
                {
                    X = tailPrevPosition.X,
                    Y = tailPrevPosition.Y
                };
                Snake.Add(newCircle);

                GenerateFood();
                gameTimer.Interval = 1000 / settings.Speed; // Adjust speed if necessary
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while eating food: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlayEatSound()
        {
            try
            {
                string soundDirectory = Path.Combine(Application.StartupPath, "Resources", "Sounds");
                string eatPath = Path.Combine(soundDirectory, "eat.wav");

                if (File.Exists(eatPath))
                {
                    // Create a new AudioFileReader and WaveOutEvent for each sound effect
                    AudioFileReader reader = new AudioFileReader(eatPath);
                    WaveOutEvent player = new WaveOutEvent();
                    player.Init(reader);
                    player.Play();

                    // Dispose resources after playback completes to free memory
                    player.PlaybackStopped += (s, e) =>
                    {
                        player.Dispose();
                        reader.Dispose();
                    };
                }
                else
                {
                    MessageBox.Show("Eat sound file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while playing eat sound: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckCollision()
        {
            try
            {
                // Check collision with self
                for (int i = 1; i < Snake.Count; i++)
                {
                    if (Snake[0].X == Snake[i].X && Snake[0].Y == Snake[i].Y)
                    {
                        GameOver();
                        break;
                    }
                }

                // Optional: Check collision with walls if not wrapping around
                /*
                if (Snake[0].X < 0 || Snake[0].Y < 0 ||
                    Snake[0].X >= gamePanel.Width / settings.Size ||
                    Snake[0].Y >= gamePanel.Height / settings.Size)
                {
                    GameOver();
                }
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while checking collisions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GameOver()
        {
            try
            {
                gameTimer?.Stop();
                backgroundPlayer?.Stop();
                gameOverPlayer?.Play();
                highScores.AddScore(score);
                MessageBox.Show($"Game Over! Your score: {score}", "Snake Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                highScores.ShowHighScores();

                DialogResult result = MessageBox.Show("Do you want to play again?", "Snake Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    RestartGame(initial: true); // Reset the game and wait for player input
                    MessageBox.Show("Press any key to start the game!", "Snake Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during game over: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!gameStarted)
                {
                    gameStarted = true;
                    gameTimer?.Start();
                    PlayBackgroundMusic();
                }

                Direction newDirection = settings.Direction;

                switch (e.KeyCode)
                {
                    case Keys.Right:
                    case Keys.D:
                        newDirection = Direction.Right;
                        break;
                    case Keys.Left:
                    case Keys.A:
                        newDirection = Direction.Left;
                        break;
                    case Keys.Up:
                    case Keys.W:
                        newDirection = Direction.Up;
                        break;
                    case Keys.Down:
                    case Keys.S:
                        newDirection = Direction.Down;
                        break;
                    case Keys.R:
                        // Allow restarting the game by pressing 'R'
                        RestartGame(initial: true); // Reset the game and wait for player input
                        MessageBox.Show("Press any key to start the game!", "Snake Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    case Keys.P:
                        // Toggle pause
                        TogglePause();
                        return;
                }

                // Check if the new direction is not opposite to the current direction
                if (!IsOppositeDirection(newDirection, settings.Direction))
                {
                    // Check if the direction is already in the queue
                    if (!directionQueue.Contains(newDirection))
                    {
                        directionQueue.Enqueue(newDirection);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during key press handling: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsOppositeDirection(Direction newDir, Direction currentDir)
        {
            return (newDir == Direction.Up && currentDir == Direction.Down) ||
                   (newDir == Direction.Down && currentDir == Direction.Up) ||
                   (newDir == Direction.Left && currentDir == Direction.Right) ||
                   (newDir == Direction.Right && currentDir == Direction.Left);
        }

        private void PlayBackgroundMusic()
        {
            try
            {
                if (backgroundPlayer != null)
                {
                    backgroundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while playing background music: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TogglePause()
        {
            try
            {
                if (gameTimer == null)
                {
                    MessageBox.Show("Game Timer is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (gameTimer.Enabled)
                {
                    gameTimer.Stop();
                    backgroundPlayer?.Pause();
                    isPaused = true;
                }
                else
                {
                    gameTimer.Start();
                    backgroundPlayer?.Play();
                    isPaused = false;
                }

                gamePanel.Invalidate(); // Redraw to show/hide paused message
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while toggling pause: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics canvas = e.Graphics;

                // Draw snake
                for (int i = 0; i < Snake.Count; i++)
                {
                    if (i == 0)
                        canvas.FillEllipse(Brushes.Black, new Rectangle(Snake[i].X * settings.Size, Snake[i].Y * settings.Size, settings.Size, settings.Size)); // Head
                    else
                        canvas.FillEllipse(Brushes.Green, new Rectangle(Snake[i].X * settings.Size, Snake[i].Y * settings.Size, settings.Size, settings.Size)); // Body
                }

                // Draw food
                canvas.FillEllipse(Brushes.Red, new Rectangle(food.X * settings.Size, food.Y * settings.Size, settings.Size, settings.Size));

                // Draw score
                using (Font font = new Font("Arial", 16))
                {
                    canvas.DrawString($"Score: {score}", font, Brushes.White, new PointF(5, 5));
                }

                // Draw paused message
                if (isPaused)
                {
                    using (Font font = new Font("Arial", 24, FontStyle.Bold))
                    {
                        SizeF textSize = canvas.MeasureString("Paused", font);
                        canvas.DrawString("Paused", font, Brushes.Yellow, (gamePanel.Width - textSize.Width) / 2, (gamePanel.Height - textSize.Height) / 2);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during painting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                gameTimer?.Stop();
                backgroundPlayer?.Stop();
                gameOverPlayer?.Stop();

                // Dispose all NAudio resources
                loopBackgroundStream?.Dispose();
                backgroundReader?.Dispose();
                backgroundPlayer?.Dispose();

                gameOverReader?.Dispose();
                gameOverPlayer?.Dispose();

                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during form closing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
