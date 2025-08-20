using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GhostCar
{
    public static class GhostRecorder
    {
        private static readonly List<string> buffer = new List<string>();
        private static readonly object lockObj = new object();
        private static Thread flushThread;
        private static bool running = false;
        private static float startTime = 0f;
        private static string logFilePath;

        private static GameObject currentVehicle;

        public static bool IsRunning => running;

        public static void Start()
        {
            currentVehicle = Helper.GetVehicle();
            if (currentVehicle == null)
            {
                ModConsole.Error("GhostRecorder: No current vehicle found.");
                return;
            }

            startTime = Time.time;
            string filetimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            logFilePath = Path.Combine(new GhostCar().assetsFolder, $"ghostlog_{filetimestamp}.log");

            running = true;

            startTime = Time.time;
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string vehicleName = currentVehicle.name;

            string header = $"# version=1; timestamp={timestamp}; duration=0.000; vehicle={vehicleName}; notes=GhostCarMod";

            try
            {
                File.WriteAllText(logFilePath, header + Environment.NewLine, Encoding.ASCII);
            }
            catch (Exception ex)
            {
                ModConsole.Error("GhostRecorder: Failed to write header - " + ex.Message);
            }

            flushThread = new Thread(() =>
            {
                while (running)
                {
                    Thread.Sleep(1000);
                    FlushToFile();
                }
            });

            flushThread.IsBackground = true;
            flushThread.Start();

            ModConsole.Print("GhostRecorder started logging to: " + logFilePath);
        }

        public static void Stop()
        {
            running = false;
            FlushToFile();

            try
            {
                string[] lines = File.ReadAllLines(logFilePath);
                if (lines.Length > 0 && lines[0].StartsWith("#"))
                {
                    float duration = Time.time - startTime;
                    lines[0] = lines[0].Replace("duration=0.000", $"duration={duration:F3}");
                    File.WriteAllLines(logFilePath, lines, Encoding.ASCII);
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error("GhostRecorder: Failed to update duration - " + ex.Message);
            }


            ModConsole.Print("GhostRecorder stopped.");
        }

        public static void Update()
        {
            if (!running || currentVehicle == null)
                return;

            float time = Time.time - startTime;
            Transform t = currentVehicle.transform;
            Vector3 pos = t.position;
            Quaternion rot = t.rotation;

            string line = string.Format("{0:F3},{1:F3},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F3}",
                time, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);

            lock (lockObj)
            {
                buffer.Add(line);
            }
        }

        private static void FlushToFile()
        {
            List<string> toWrite;

            lock (lockObj)
            {
                if (buffer.Count == 0) return;

                toWrite = new List<string>(buffer);
                buffer.Clear();
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var line in toWrite)
                    sb.AppendLine(line);

                File.AppendAllText(logFilePath, sb.ToString(), Encoding.ASCII);
            }
            catch (Exception ex)
            {
                ModConsole.Error("GhostRecorder: Failed to write log - " + ex.Message);
            }
        }

        public static string GetLatestLogFile()
        {
            string dir = Application.persistentDataPath;
            string[] files = Directory.GetFiles(dir, "ghostlog_*.log");

            if (files.Length == 0) return null;

            return files.OrderByDescending(f => File.GetLastWriteTime(f)).First();
        }

    }
}
