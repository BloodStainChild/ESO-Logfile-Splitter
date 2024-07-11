using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Custom_Message;

namespace Editor
{
    public partial class Form1 : Form
    {
        private BackgroundWorker logFilReadereWorker = new BackgroundWorker();
        private BackgroundWorker logFileWriterWorker = new BackgroundWorker();
        public Form1()
        {
            InitializeComponent();
            // DB

            logFilReadereWorker.DoWork += new DoWorkEventHandler(logFilReadereWorker_DoWork);
            logFilReadereWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(logFilReadereWorker_RunWorkerCompleted);
            logFilReadereWorker.ProgressChanged += new ProgressChangedEventHandler(logFilReadereWorker_ProgressChanged);            
            logFilReadereWorker.WorkerReportsProgress = true;
            logFilReadereWorker.WorkerSupportsCancellation = false;

            logFileWriterWorker.DoWork += new DoWorkEventHandler(logFileWriterWorker_DoWork);
            logFileWriterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(logFileWriterWorker_RunWorkerCompleted);
            logFileWriterWorker.ProgressChanged += new ProgressChangedEventHandler(logFileWriterWorker_ProgressChanged);
            logFileWriterWorker.WorkerReportsProgress = true;
            logFileWriterWorker.WorkerSupportsCancellation = false;
        }

        public DateTime UnixSecondsToDateTime(Int64 timestamp, bool local = true)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            return local ? offset.LocalDateTime : offset.UtcDateTime;
        }

        private const string FormatLogTime = "dd_MM_yyyy_HH_mm";
        private const string LogFolder = "SPLIT_LOGS";
        public static List<LogInfo> LogInfos = new List<LogInfo>();
        public static string OpenedLogFile = "";
        public static int Selected_Log = -1;
        public static int Selected_Zone = -1;
        public static bool bWriteOnlySelectedLog = false;
        public static bool bWriteOnlySelectedZone = false;
        public static bool bIsReadingFile = false;
        public static bool bIsWritingFile = false;

        public void ReadLogFile(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (string.IsNullOrEmpty(OpenedLogFile))
                return;

            char[] charSeparators = new char[] { ',' };

            bIsReadingFile = true;
            LogInfos.Clear();

            List<AbilityInfo> effectInfo = new List<AbilityInfo>();
            List<AbilityInfo> abilityInfo = new List<AbilityInfo>();
            List<LogZoneInfo> zoneInfo = new List<LogZoneInfo>();

            Int64 lineCount = 0;
            Int64 LogStartIndex = 0;
            Int64 UnixStartTime = 0;
            string Server = "";
            string Language = "en";
            string patch = "";
            string BEGIN_LOG_LINE = "";

            long totalLines = File.ReadLines(OpenedLogFile).Count();  // Get total lines for progress reporting
            const int progressUpdatePercent = 1;  // Update progress every 1%
            int progressUpdateInterval = (int)(totalLines * progressUpdatePercent / 100);

            using (StreamReader sr = new StreamReader(OpenedLogFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("BEGIN_LOG"))
                    {
                        string[] splt = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        UnixStartTime = Convert.ToInt64(splt[2]);
                        LogStartIndex = lineCount;
                        Server = splt[4];
                        Language = splt[5];
                        patch = splt[6];
                        BEGIN_LOG_LINE = line;
                    }
                    else if (line.Contains("END_LOG"))
                    {
                        LogInfos.Add(new LogInfo
                        {
                            LogCount = LogInfos.Count + 1,
                            BEGIN_LOG_INDEX = LogStartIndex,
                            END_LOG_INDEX = lineCount,
                            LogStartUnixTime = UnixStartTime,
                            Name = "Encounter_" + (LogInfos.Count + 1),
                            Server = Server,
                            Language = Language,
                            Patch = patch,
                            BEGIN_LOG_LINE = BEGIN_LOG_LINE,
                            END_LOG_LINE = line,
                            EFFECT_INFO = new List<AbilityInfo>(effectInfo),
                            ABILITY_INFO = new List<AbilityInfo>(abilityInfo),
                            ZONE_CHANGED_INFO = new List<LogZoneInfo>(zoneInfo)
                        });

                        abilityInfo.Clear();
                        zoneInfo.Clear();
                        effectInfo.Clear();
                    }
                    else if(line.Contains("ABILITY_INFO"))
                    {
                        abilityInfo.Add(new AbilityInfo
                        {
                            LOG_INDEX = lineCount,
                            INFO = line
                        });
                    }
                    else if(line.Contains("EFFECT_INFO"))
                    {
                        effectInfo.Add(new AbilityInfo
                        {
                            LOG_INDEX = lineCount,
                            INFO = line
                        });
                    }
                    else if(line.Contains("ZONE_CHANGED"))
                    {
                        var splt = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        zoneInfo.Add(new LogZoneInfo
                        {
                            LOG_INDEX = lineCount,
                            ZoneName = splt[3],
                            ZoneStartTime = UnixStartTime + Convert.ToInt64(splt[0]),
                            line = line,
                            ZoneCount = zoneInfo.Count + 1
                        });
                    }

                    lineCount++;
                    // Report progress every progressUpdateInterval lines
                    if (lineCount % progressUpdateInterval == 0 || line == null)
                    {
                        int progressPercentage = (int)((float)lineCount / totalLines * 100);
                        worker.ReportProgress(progressPercentage);
                    }
                }
            }
        }
        public void StartLogFileReading()
        {
            if (!logFilReadereWorker.IsBusy)
            {
                progressBar1.Value = 0;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".log";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                OpenedLogFile = openFileDialog.FileName;
                Text = $"ESO LOG Splitter [{openFileDialog.FileName}]";

                logFilReadereWorker.RunWorkerAsync();
            }
        }
        private void logFilReadereWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadLogFile((BackgroundWorker)sender, e);
        }
        private void logFilReadereWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void logFilReadereWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bIsReadingFile = false;
            if (e.Error != null)
            {
                MessageBox.Show($"An error occurred: {e.Error.Message}");
            }
            else
            {
                progressBar1.Value = 100;
                new CustomMessage("Log file reading completed!", 30).Show();
                FillListBoxes();
            }
        }
        //

        public void FillListBoxes()
        {
            lB_FoundLogs.Items.Clear();
            lB_Zones.Items.Clear();
            for (int i = 0; i < LogInfos.Count; i++)
            {
                string item = LogInfos[i].LogCount.ToString() + " " + LogInfos[i].Name;
                lB_FoundLogs.Items.Add(item);
            }
        }

        private void lB_FoundLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Selected_Log = -1;
            if (lB_FoundLogs.SelectedItems.Count <= 0)
                return;

            int LI = LogInfos.FindIndex(x => x.LogCount == (lB_FoundLogs.SelectedIndex +1));
            if (LI == -1)
                return;

            Selected_Log = lB_FoundLogs.SelectedIndex + 1;

            tb_server.Text = LogInfos[LI].Server;
            tb_lang.Text = LogInfos[LI].Language;
            tb_patch.Text = LogInfos[LI].Patch;
            tb_startTime.Text = UnixSecondsToDateTime(LogInfos[LI].LogStartUnixTime).ToString();

            lB_Zones.Items.Clear();
            for (int i = 0; i < LogInfos[LI].ZONE_CHANGED_INFO.Count; i++)
            {
                string item = LogInfos[LI].ZONE_CHANGED_INFO[i].ZoneCount.ToString() + " " + LogInfos[LI].ZONE_CHANGED_INFO[i].ZoneName + " " + UnixSecondsToDateTime(LogInfos[LI].ZONE_CHANGED_INFO[i].ZoneStartTime).ToString();
                lB_Zones.Items.Add(item);
            }
        }
        private void lB_Zones_SelectedIndexChanged(object sender, EventArgs e)
        {
            Selected_Zone = -1;
            Selected_Log = -1;
            if (lB_FoundLogs.SelectedItems.Count <= 0)
                return;

            int LI = LogInfos.FindIndex(x => x.LogCount == (lB_FoundLogs.SelectedIndex + 1));
            if (LI == -1)
                return;

            Selected_Log = lB_FoundLogs.SelectedIndex + 1;

            if (lB_Zones.SelectedItems.Count <= 0)
                return;

            int ZCI = LogInfos[LI].ZONE_CHANGED_INFO.FindIndex(p => p.ZoneCount == (lB_FoundLogs.SelectedIndex + 1));
            if (ZCI == -1) 
                return;

            Selected_Zone = lB_Zones.SelectedIndex + 1;
        }

        public void ReportProgress(int processedLogs, int totalLogs, BackgroundWorker worker)
        {
            int progressPercentage = (int)((float)processedLogs / totalLogs * 100);
            worker.ReportProgress(progressPercentage);
        }

        public void WriteNewLogFile(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (string.IsNullOrEmpty(OpenedLogFile))
                return;

            FileInfo fileInfo = new FileInfo(OpenedLogFile);
            string directoryPath = fileInfo.DirectoryName;

            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                return;

            string logDirectory = Path.Combine(directoryPath, LogFolder);
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            int totalLogs = LogInfos.Sum(logInfo => logInfo.ZONE_CHANGED_INFO.Count);
            int processedLogs = 0;
            bIsWritingFile = true;

            foreach (var logInfo in LogInfos)
            {
                if (bWriteOnlySelectedLog && Selected_Log != -1 && Selected_Log != logInfo.LogCount)
                {
                    processedLogs++;
                    ReportProgress(processedLogs, totalLogs, worker);
                    continue;
                }

                foreach (var zoneChangedInfo in logInfo.ZONE_CHANGED_INFO)
                {
                    if (bWriteOnlySelectedZone && Selected_Zone != -1 && Selected_Zone != zoneChangedInfo.ZoneCount)
                    {
                        processedLogs++;
                        ReportProgress(processedLogs, totalLogs, worker);
                        continue;
                    }

                    string datetime = UnixSecondsToDateTime(zoneChangedInfo.ZoneStartTime).ToString(FormatLogTime);
                    string zoneName = zoneChangedInfo.ZoneName.Replace("\\\"", "\"").Trim('\"');
                    string fileName = Path.Combine(logDirectory, $"Encounter_{LogInfos.IndexOf(logInfo)+1}_{zoneName}_{datetime}.log");

                    try
                    {
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            // Write Start Log
                            sw.WriteLine(logInfo.BEGIN_LOG_LINE);
                            // Write Skill Info
                            WriteLogSection(sw, logInfo.ABILITY_INFO.Select(ai => ai.INFO));
                            // Write Effect Info
                            WriteLogSection(sw, logInfo.EFFECT_INFO.Select(ei => ei.INFO));
                            // Write Zone Change
                            sw.WriteLine(zoneChangedInfo.line);
                            WriteLogLinesFromFile(sw, OpenedLogFile, zoneChangedInfo.LOG_INDEX);
                            // Write END log
                            sw.WriteLine(logInfo.END_LOG_LINE);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error writing to file {fileName}: {ex.Message}");
                    }
                    processedLogs++;
                    ReportProgress(processedLogs, totalLogs, worker);
                }
            }
        }

        private void WriteLogSection(StreamWriter sw, IEnumerable<string> logSection)
        {
            int lineCount = 0;
            foreach (var line in logSection)
            {
                sw.WriteLine(line);
                lineCount++;

                if (lineCount % 1000 == 0)
                    sw.Flush();
            }
        }

        private void WriteLogLinesFromFile(StreamWriter sw, string filePath, long startIndex)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                long lineCount = 0;
                long swLineCount = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    if (lineCount <= startIndex)
                    {
                        lineCount++;
                        continue;
                    }

                    if (line.Contains("BEGIN_LOG") || line.Contains("ABILITY_INFO") || line.Contains("EFFECT_INFO"))
                    {
                        lineCount++;
                        continue;
                    }

                    if (line.Contains("END_LOG") || line.Contains("ZONE_CHANGED"))
                        break;

                    sw.WriteLine(line);
                    swLineCount++;

                    if (swLineCount % 1000 == 0)
                        sw.Flush();

                    lineCount++;
                }
            }
        }
        public void StartLogFileWriting()
        {
            if (!logFileWriterWorker.IsBusy)
            {
                progressBar1.Value = 0;
                logFileWriterWorker.RunWorkerAsync();
            }
        }
        private void logFileWriterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WriteNewLogFile((BackgroundWorker)sender, e);
        }        
        private void logFileWriterWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void logFileWriterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bIsWritingFile = false;
            if (e.Error != null)
            {
                MessageBox.Show($"An error occurred: {e.Error.Message}");
            }
            else
            {
                progressBar1.Value = 100;
                new CustomMessage("Log file writing completed successfully.", 30).Show();
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartLogFileReading();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(OpenedLogFile) || bIsReadingFile)
                return;

            bWriteOnlySelectedLog = false;
            bWriteOnlySelectedZone = false;
            StartLogFileWriting();
        }
        private void saveSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(OpenedLogFile) || bIsReadingFile)
                return;

            bWriteOnlySelectedLog = true;

            if (Selected_Zone != -1)
                bWriteOnlySelectedZone = true;

            StartLogFileWriting();
        }

    }
}
