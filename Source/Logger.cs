using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public static class GhostLogger
{
    private static List<string> buffer = new List<string>();
    private static object lockObj = new object();
    private static Thread flushThread;
    private static bool running = false;
    private static float startTime = 0f;
    private static string logFilePath;

    private static GameObject satsuma;

    public static void Start()
    {
        satsuma = GameObject.Find("SATSUMA(557kg, 248)");

        if (satsuma == null)
        {
            ModConsole.Error("GhostLogger: Could not find Satsuma");
            return;
        }

        startTime = Time.time;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logFilePath = Path.Combine(Application.persistentDataPath, $"ghostlog_{timestamp}.log");

        running = true;

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
    }

    public static void Stop()
    {
        running = false;
        FlushToFile();
    }

    public static void Update()
    {
        if (!running || satsuma == null)
            return;

        float time = Time.time - startTime;
        Transform t = satsuma.transform;
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
            if (buffer.Count == 0)
                return;

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
            ModConsole.Error("GhostLogger: Failed to write log - " + ex.Message);
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
