using System;
using System.Collections.Generic;
using System.IO;

namespace osum.Helpers
{
    /// <summary>
    /// Simple Configuration Manager
    /// </summary>
    public class pConfigManager : IDisposable
    {
        private readonly Dictionary<string, string> entriesRaw = new Dictionary<string, string>();
        private readonly Dictionary<string, object> entriesParsed = new Dictionary<string, object>();
        private string configFilename;
        private bool dirty;
        public bool WriteOnChange { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="pConfigManager"/> class with a custom filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public pConfigManager(string filename)
        {
            LoadConfig(filename);
        }

        ~pConfigManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (dirty) SaveConfig();
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            object obj;

            if (entriesParsed.TryGetValue(key, out obj))
                return (T)obj;

            string raw;
            if (!entriesRaw.TryGetValue(key, out raw))
            {
                //If we don't have a value, we should set the default to be written back to the config file.
                SetValue(key, defaultValue);
                return defaultValue;
            }

            string ty = typeof(T).Name;

            switch (ty)
            {
                case "Boolean":
                    obj = raw[0] == '1';
                    break;
                case "Int32":
                    obj = int.Parse(raw);
                    break;
                case "Int64":
                    obj = long.Parse(raw);
                    break;
                case "String":
                    obj = raw;
                    break;
            }

            entriesParsed[key] = obj;
            entriesRaw[key] = raw;

            return (T)obj;
        }

        public void SetValue<T>(string key, T value)
        {
            switch (typeof(T).Name)
            {
                default:
                    if (value == null)
                        entriesRaw[key] = null;
                    else
                        entriesRaw[key] = value.ToString();
                    break;
                case "Boolean":
                    entriesRaw[key] = value.ToString() == "True" ? "1" : "0";
                    break;
            }

            entriesParsed[key] = value;

            dirty = true;

            if (WriteOnChange) SaveConfig();
        }

        public void LoadConfig(string configName)
        {
            entriesRaw.Clear();
            entriesParsed.Clear();

            configFilename = configName;
            ReadConfigFile(configName);
        }

        private void ReadConfigFile(string filename)
        {
            if (!File.Exists(filename)) return;

            //a failure shouldn't mean a crash.
            try
            {
                using (StreamReader r = File.OpenText(filename))
                    while (!r.EndOfStream)
                    {
                        string line = r.ReadLine();
                        if (line.Length < 2)
                            continue;
                        int equals = line.IndexOf('=');
                        string key = line.Remove(equals).Trim();
                        string value = line.Substring(equals + 1).Trim();
                        entriesRaw[key] = value;
                    }
            }
            catch
            {
            }
        }

        public void SaveConfig()
        {
            if (configFilename == null)
                throw new Exception("Not initialized.");
            WriteConfigFile(configFilename);

            dirty = false;
        }

        private bool WriteConfigFile(string filename)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(filename, false))
                    foreach (KeyValuePair<string, string> p in entriesRaw)
                        if (p.Value != null)
                            w.WriteLine("{0} = {1}", p.Key, p.Value);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}