//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private Timer gameTimer; // Game loop timer
        private Timer wallSpawnTimer; // Wall spawning timer
        private Timer foodRespawnTimer; // Food respawn timer

        private Settings settings;
        private HighScores highScores;
        private bool gameStarted = false;
        private bool isPaused = false;
        private Random rand = new Random(); // Initialize once to prevent repetition
        private Point tailPrevPosition; // To store the tail's previous position

        // Direction input queue
        private Queue<Direction> directionQueue = new Queue<Direction>();

        // Walls
        private List<Wall> Walls = new List<Wall>();
        private const int WallSpawnInterval = 7000; // Spawn a wall every 7 seconds
        private const int WallFlashDuration = 30; // Wall flashes for 30 game ticks

        // Food Timer
        private const int FoodMaxTime = 100; // Total game ticks before food expires
        private int foodTimer = FoodMaxTime;
        private bool isFoodActive = true;

        public SnakeGame()
        {
            InitializeComponent(); // Initialize form components first
            InitializeSound();     // Initialize sound components
            InitializeGame();      // Initialize game-specific components
            InitializeWallSpawnTimer(); // Initialize wall spawn timer
            InitializeFoodRespawnTimer(); // Initialize food respawn timer
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

                // Initialize the Game Timer
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

        private void InitializeWallSpawnTimer()
        {
            try
            {
                wallSpawnTimer = new Timer();
                wallSpawnTimer.Interval = WallSpawnInterval; // Set interval for wall spawning
                wallSpawnTimer.Tick += WallSpawnTimer_Tick;
                wallSpawnTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while initializing wall spawn timer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeFoodRespawnTimer()
        {
            try
            {
                foodRespawnTimer = new Timer();
                foodRespawnTimer.Interval = 2000; // 2 seconds delay
                foodRespawnTimer.Tick += FoodRespawnTimer_Tick;
                foodRespawnTimer.Stop(); // Initially stopped
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while initializing food respawn timer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // Ensure the game timer is not running initially
                    gameTimer?.Stop();
                }

                gameStarted = false; // Reset the game started flag
                isPaused = false;    // Reset the pause flag

                // Clear any pending direction changes
                directionQueue.Clear();

                // Clear existing walls
                Walls.Clear();

                // Reset food timer
                foodTimer = FoodMaxTime;
                isFoodActive = true;

                // Stop any ongoing food respawn
                foodRespawnTimer?.Stop();
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

                    // Ensure food is not spawned on any existing walls
                    if (validPosition)
                    {
                        foreach (var wall in Walls)
                        {
                            foreach (var block in wall.Blocks)
                            {
                                if (block.X == food.X && block.Y == food.Y)
                                {
                                    validPosition = false;
                                    break;
                                }
                            }
                            if (!validPosition)
                                break;
                        }
                    }
                }

                // Reset food timer
                foodTimer = FoodMaxTime;
                isFoodActive = true;
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
                UpdateWalls();
                UpdateFoodTimer();
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
                if (Snake[0].X == food.X && Snake[0].Y == food.Y && isFoodActive)
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
                // Calculate remaining time percentage
                float remainingTimePercentage = (float)foodTimer / FoodMaxTime;
                remainingTimePercentage = Math.Max(0, Math.Min(1, remainingTimePercentage)); // Clamp between 0 and 1

                // Calculate score increment based on remaining time
                int scoreIncrement = (int)(remainingTimePercentage * 10);
                score += scoreIncrement;

                PlayEatSound(); // Play eating sound asynchronously

                // Add new segment at the tail's previous position
                Circle newCircle = new Circle
                {
                    X = tailPrevPosition.X,
                    Y = tailPrevPosition.Y
                };
                Snake.Add(newCircle);

                // Remove the current food and start respawn timer
                isFoodActive = false;
                foodRespawnTimer?.Start();
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

                // Check collision with walls
                foreach (var wall in Walls)
                {
                    if (wall.State == WallState.Solid)
                    {
                        foreach (var block in wall.Blocks)
                        {
                            if (Snake[0].X == block.X && Snake[0].Y == block.Y)
                            {
                                GameOver();
                                break;
                            }
                        }
                    }
                }
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
                wallSpawnTimer?.Stop();
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
                    wallSpawnTimer?.Stop();
                    backgroundPlayer?.Pause();
                    isPaused = true;
                }
                else
                {
                    gameTimer.Start();
                    wallSpawnTimer?.Start();
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

                // Draw walls
                foreach (var wall in Walls)
                {
                    using (Brush wallBrush = new SolidBrush(wall.WallColor))
                    {
                        foreach (var block in wall.Blocks)
                        {
                            Rectangle wallBlock = new Rectangle(
                                block.X * settings.Size,
                                block.Y * settings.Size,
                                settings.Size,
                                settings.Size);
                            canvas.FillRectangle(wallBrush, wallBlock);
                        }
                    }
                }

                // Draw snake
                for (int i = 0; i < Snake.Count; i++)
                {
                    if (i == 0)
                        canvas.FillEllipse(Brushes.DarkGreen, new Rectangle(Snake[i].X * settings.Size, Snake[i].Y * settings.Size, settings.Size, settings.Size)); // Head
                    else
                        canvas.FillEllipse(Brushes.Green, new Rectangle(Snake[i].X * settings.Size, Snake[i].Y * settings.Size, settings.Size, settings.Size)); // Body
                }

                // Draw food with timer bar
                if (isFoodActive)
                {
                    // Calculate remaining time percentage
                    float remainingTimePercentage = (float)foodTimer / FoodMaxTime;
                    remainingTimePercentage = Math.Max(0, Math.Min(1, remainingTimePercentage)); // Clamp between 0 and 1

                    // Calculate color based on remaining time (from green to transparent)
                    int alpha = (int)(remainingTimePercentage * 255);
                    Color foodColor = Color.FromArgb(alpha, Color.Red);

                    // Draw food
                    canvas.FillEllipse(new SolidBrush(foodColor), new Rectangle(food.X * settings.Size, food.Y * settings.Size, settings.Size, settings.Size));

                    // Draw timer bar below the food
                    int barWidth = settings.Size;
                    int barHeight = 5;
                    int barX = food.X * settings.Size;
                    int barY = food.Y * settings.Size + settings.Size + 2;

                    // Background of the timer bar
                    canvas.FillRectangle(Brushes.Gray, new Rectangle(barX, barY, barWidth, barHeight));

                    // Foreground representing remaining time
                    canvas.FillRectangle(Brushes.Lime, new Rectangle(barX, barY, (int)(barWidth * remainingTimePercentage), barHeight));
                }

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

        private void UpdateWalls()
        {
            try
            {
                foreach (var wall in Walls.ToList()) // ToList() to avoid modification during iteration
                {
                    if (wall.State == WallState.Flashing)
                    {
                        wall.FlashCounter--;

                        if (wall.FlashCounter <= 0)
                        {
                            wall.State = WallState.Solid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating walls: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WallSpawnTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                SpawnWall();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while spawning a wall: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SpawnWall()
        {
            try
            {
                int wallBlockCount = rand.Next(3, 9); // Random number of blocks between 3 and 8

                List<Point> wallBlocks = new List<Point>();

                // Random starting position
                int startX = rand.Next(0, gamePanel.Width / settings.Size);
                int startY = rand.Next(0, gamePanel.Height / settings.Size);

                wallBlocks.Add(new Point(startX, startY));

                // Generate connected blocks
                for (int i = 1; i < wallBlockCount; i++)
                {
                    Point lastBlock = wallBlocks[wallBlocks.Count - 1];
                    List<Point> possibleNextBlocks = new List<Point>
                    {
                        new Point(lastBlock.X + 1, lastBlock.Y), // Right
                        new Point(lastBlock.X - 1, lastBlock.Y), // Left
                        new Point(lastBlock.X, lastBlock.Y + 1), // Down
                        new Point(lastBlock.X, lastBlock.Y - 1)  // Up
                    };

                    // Remove blocks that are out of bounds
                    possibleNextBlocks = possibleNextBlocks.Where(p =>
                        p.X >= 0 && p.X < gamePanel.Width / settings.Size &&
                        p.Y >= 0 && p.Y < gamePanel.Height / settings.Size).ToList();

                    // Remove blocks that already exist in the wall
                    possibleNextBlocks = possibleNextBlocks.Where(p => !wallBlocks.Contains(p)).ToList();

                    if (possibleNextBlocks.Count == 0)
                        break; // Cannot extend further

                    // Choose a random next block
                    Point nextBlock = possibleNextBlocks[rand.Next(possibleNextBlocks.Count)];
                    wallBlocks.Add(nextBlock);
                }

                // Check for overlap with snake
                bool overlapsSnake = false;
                foreach (var block in wallBlocks)
                {
                    foreach (var segment in Snake)
                    {
                        if (segment.X == block.X && segment.Y == block.Y)
                        {
                            overlapsSnake = true;
                            break;
                        }
                    }
                    if (overlapsSnake)
                        break;
                }

                if (overlapsSnake)
                    return; // Abort spawning if overlaps with snake

                // Check for overlap with food
                foreach (var block in wallBlocks)
                {
                    if (block.X == food.X && block.Y == food.Y)
                    {
                        return; // Abort spawning if overlaps with food
                    }
                }

                // Check for overlap with existing walls
                foreach (var existingWall in Walls)
                {
                    foreach (var block in wallBlocks)
                    {
                        if (existingWall.Blocks.Contains(block))
                        {
                            return; // Abort spawning if overlaps with existing wall
                        }
                    }
                }

                // All checks passed, spawn the wall
                Wall newWall = new Wall(wallBlocks, WallFlashDuration);
                Walls.Add(newWall);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while spawning a wall: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateFoodTimer()
        {
            try
            {
                if (isFoodActive)
                {
                    foodTimer--;

                    if (foodTimer <= 0)
                    {
                        // Food expired
                        isFoodActive = false;
                        foodRespawnTimer?.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating food timer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FoodRespawnTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                foodRespawnTimer?.Stop();
                GenerateFood();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during food respawn: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                gameTimer?.Stop();
                wallSpawnTimer?.Stop();
                foodRespawnTimer?.Stop();
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
