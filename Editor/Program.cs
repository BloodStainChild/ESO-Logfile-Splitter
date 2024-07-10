using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Editor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class LogInfo
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string Language { get; set; }
        public string Patch { get; set; }
        public Int64 LogStartUnixTime { get; set; }
        public Int64 LogCount { get; set; }
        public Int64 BEGIN_LOG_INDEX { get; set; }
        public string BEGIN_LOG_LINE { get; set; }
        public Int64 END_LOG_INDEX { get; set; }
        public string END_LOG_LINE { get; set; }
        public List<AbilityInfo> ABILITY_INFO { get; set; }
        public List<AbilityInfo> EFFECT_INFO { get; set; }
        public List<LogZoneInfo> ZONE_CHANGED_INFO { get; set; }
    }

    public class AbilityInfo
    {   
        public Int64 LOG_INDEX { get; set; }
        public string INFO { get; set; }
    }

    public class LogZoneInfo
    {
        public Int64 ZoneCount { get; set; }
        public string ZoneName { get; set; }
        public Int64 ZoneStartTime { get; set; }
        public string line { get; set; }
        public Int64 LOG_INDEX { get; set; }

    }
}
