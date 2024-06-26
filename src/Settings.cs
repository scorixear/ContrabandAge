﻿using System.IO;
using System.Text.Json;
using Timer = System.Timers.Timer;

namespace ContrabandAge
{

    public class Settings
    {
        private readonly string Location = Environment.ExpandEnvironmentVariables(@"%appdata%\ContrabandAge\");
        private readonly string FileName = "settings.json";

        private SettingsJson settingsJson;
        private readonly JsonSerializerOptions serializerOptions = new()
        {
            AllowTrailingCommas = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        private Timer SyncTimer { get; set; } = new(1000);

        public DateOnly CurrentDay
        {
            get => DateOnly.ParseExact(settingsJson.CurrentDay, "dd.MM.yyyy");
            set
            {
                settingsJson.CurrentDay = value.ToString("dd.MM.yyyy");
                SaveJson(settingsJson);
            }
        }

        public int Age
        {
            get => settingsJson.Age;
            set
            {
                settingsJson.Age = value;
                SaveJson(settingsJson);
            }
        }

        public delegate void SyncChangeHandler();
        public event SyncChangeHandler? SyncChange;

        public Settings()
        {
            settingsJson = LoadJson();
            SyncTimer.AutoReset = true;
            SyncTimer.Elapsed += (sender, args) => Sync();
            SyncTimer.Start();
        }

        private void Sync()
        {
            lock (settingsJson)
            {
                SettingsJson newSettings = LoadJson();
                if (settingsJson.Compare(newSettings) == false)
                {
                    settingsJson = newSettings;
                    SyncChange?.Invoke();
                }
            }
        }


        private void SaveJson(SettingsJson settingsJson)
        {
            string json = JsonSerializer.Serialize(settingsJson, serializerOptions);
            File.WriteAllText(Location + FileName, json);
        }

        private SettingsJson LoadJson()
        {
            if (!Directory.Exists(Location))
            {
                Directory.CreateDirectory(Location);
            }

            SettingsJson settingsJson;
            if (!File.Exists(Location + FileName))
            {
                settingsJson = new SettingsJson();
                SaveJson(settingsJson);
            }
            else
            {
                SettingsJson? deseralized = JsonSerializer.Deserialize<SettingsJson>(File.ReadAllText(Location + FileName), serializerOptions);
                if (deseralized == null)
                {
                    settingsJson = new SettingsJson();
                    SaveJson(settingsJson);
                }
                else
                {
                    settingsJson = deseralized;
                }
            }
            return settingsJson;
        }

        [Serializable]
        private class SettingsJson
        {
            public string CurrentDay { get; set; } = "01.01.1980";
            public int Age { get; set; } = 50;

            public bool Compare(SettingsJson other)
            {
                return CurrentDay == other.CurrentDay && Age == other.Age;
            }
        }
    }
}
