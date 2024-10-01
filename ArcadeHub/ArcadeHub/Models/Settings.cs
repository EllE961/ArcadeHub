//  ████████      ██          ██          ████████ 
//  ██            ██          ██          ██       
//  ██  ████      ██          ██          ██  ████
//  ██            ██          ██          ██       
//  ████████      ████████    ████████    ████████ 

using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace ArcadeHub.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class Settings
    {
        public int Speed { get; set; } = 10; // Default speed
        public int Size { get; set; } = 20; // Size of each segment
        public Direction Direction { get; set; } = Direction.Right;
        public int ScoreIncrement { get; set; } = 10;

        private static readonly string settingsPath = Path.Combine(Application.StartupPath, "settings.xml");

        public static Settings Load()
        {
            if (File.Exists(settingsPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(settingsPath, FileMode.Open))
                {
                    return (Settings)serializer.Deserialize(fs);
                }
            }
            else
            {
                Settings defaultSettings = new Settings();
                defaultSettings.Save();
                return defaultSettings;
            }
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream fs = new FileStream(settingsPath, FileMode.Create))
            {
                serializer.Serialize(fs, this);
            }
        }
    }
}
