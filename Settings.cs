using System;
using System.IO;
using System.Text.Json;

namespace WorstPC
{
    // Persisted to %APPDATA%\WORST\settings.json
    internal sealed class Settings
    {
        public string Wsid { get; set; } = "";       // worst.homes session (from Discord login)
        public string VipKey { get; set; } = "";      // TOOL/VIP key, used as the vault token
        public string DeviceId { get; set; } = "";    // stable id for this PC
        public string PlaceId { get; set; } = "";     // last game id
        public bool MultiRoblox { get; set; } = true; // hold the Roblox single-instance lock
        public int JoinDelayMs { get; set; } = 2500;  // delay between launches

        private static string Dir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WORST");
        }
        private static string FilePath() { return Path.Combine(Dir(), "settings.json"); }

        public static Settings Load()
        {
            try
            {
                string p = FilePath();
                if (File.Exists(p))
                {
                    Settings s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(p));
                    if (s != null)
                    {
                        if (string.IsNullOrEmpty(s.DeviceId)) { s.DeviceId = NewDevice(); s.Save(); }
                        return s;
                    }
                }
            }
            catch { }
            Settings fresh = new Settings();
            fresh.DeviceId = NewDevice();
            fresh.Save();
            return fresh;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir());
                File.WriteAllText(FilePath(),
                    JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private static string NewDevice()
        {
            return "pc-" + Guid.NewGuid().ToString("N").Substring(0, 16);
        }
    }
}
