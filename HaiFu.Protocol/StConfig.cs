using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace HaiFu.Protocol
{
    public static class StConfig
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        private static string ConfigFileName => "config.json";
        private static void Write(string key, string value)
        {
            JObject json = new JObject();
            string config = string.Empty;
            if (File.Exists(ConfigFileName))
            {
                config = File.ReadAllText(ConfigFileName);
                if (!string.IsNullOrEmpty(config))
                {
                    json = JObject.Parse(config);
                }
            }
            json[key] = value;
            config = json.ToString();
            File.WriteAllText(ConfigFileName, config);
        }
        private static T Read<T>(string key, T defaultvalue = default(T))
        {
            if (File.Exists(ConfigFileName))
            {
                string config = File.ReadAllText(ConfigFileName);
                if (!string.IsNullOrEmpty(config))
                {
                    JObject json = JObject.Parse(config);
                    if(json.Value<T>(key) != null)
                        return json.Value<T>(key);
                }
            }
            return defaultvalue;
        }
        public static string LastTime
        {
            get
            {
                return Read(nameof(LastTime), string.Empty);
            }
            set
            {
                Write(nameof(LastTime), value);
            }
        }
        public static int Count
        {
            get
            {
                return Read<int>(nameof(Count));
            }
            set
            {
                Write(nameof(Count), value.ToString());
            }
        }
        public static void AddCount()
        {
            int count = Count;
            count += 1;
            Count = count;
        }
        public static string SavePath
        {
            get
            {
                return Read(nameof(SavePath), Application.StartupPath);
            }
            set
            {
                Write(nameof(SavePath), value);
            }
        }
        public static string Name
        {
            get
            {
                return Read(nameof(Name), Guid.NewGuid().ToString());
            }
            set
            {
                Write(nameof(Name), value);
            }
        }
    }
}
