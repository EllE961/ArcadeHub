//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ArcadeHub.Games.SnakeGame
{
    public class HighScores
    {
        private readonly string filePath = Path.Combine(Application.StartupPath, "highscores.txt");
        private List<int> scores;

        public HighScores()
        {
            LoadScores();
        }

        private void LoadScores()
        {
            if (File.Exists(filePath))
            {
                scores = File.ReadAllLines(filePath)
                             .Select(line => int.TryParse(line, out int score) ? score : 0)
                             .OrderByDescending(s => s)
                             .Take(10)
                             .ToList();
            }
            else
            {
                scores = new List<int>();
            }
        }

        public void AddScore(int score)
        {
            scores.Add(score);
            scores = scores.OrderByDescending(s => s).Take(10).ToList();
            SaveScores();
        }

        private void SaveScores()
        {
            File.WriteAllLines(filePath, scores.Select(s => s.ToString()));
        }

        public void ShowHighScores()
        {
            string message = "High Scores:\n";
            for (int i = 0; i < scores.Count; i++)
            {
                message += $"{i + 1}. {scores[i]}\n";
            }

            MessageBox.Show(message, "High Scores", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
