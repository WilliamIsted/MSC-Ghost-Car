using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GhostCar
{
    public class Replay : MonoBehaviour
    {
        private class Entry
        {
            public float time;
            public Vector3 position;
            public Quaternion rotation;
        }

        public string filename;
        private List<Entry> entries = new List<Entry>();
        private float startTime = 0f;
        private int currentIndex = 0;
        private bool ready = false;

        void Start()
        {
            if (string.IsNullOrEmpty(filename))
            {
                ModConsole.Error("Replay: No filename set.");
                return;
            }

            if (!File.Exists(filename))
            {
                ModConsole.Error("Replay: File not found: " + filename);
                return;
            }

            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                // Output metadata
                if (line.StartsWith("#"))
                {
                    var meta = Helper.ParseMetadata(line);

                    ModConsole.Print("Replay metadata:");
                    foreach (var kvp in meta)
                        ModConsole.Print($"  {kvp.Key} = {kvp.Value}");

                    continue;
                }

                string[] parts = line.Split(',');
                if (parts.Length != 8) continue;

                entries.Add(new Entry
                {
                    time = float.Parse(parts[0]),
                    position = new Vector3(
                        float.Parse(parts[1]),
                        float.Parse(parts[2]),
                        float.Parse(parts[3])),
                    rotation = new Quaternion(
                        float.Parse(parts[4]),
                        float.Parse(parts[5]),
                        float.Parse(parts[6]),
                        float.Parse(parts[7]))
                });
            }

            if (entries.Count < 2)
            {
                ModConsole.Error("Replay: Not enough data to playback.");
                return;
            }

            startTime = Time.time;
            ready = true;
        }

        void Update()
        {
            if (!ready || currentIndex >= entries.Count - 1) return;

            float elapsed = Time.time - startTime;

            while (currentIndex < entries.Count - 2 && elapsed > entries[currentIndex + 1].time)
                currentIndex++;

            var a = entries[currentIndex];
            var b = entries[currentIndex + 1];
            float segmentTime = b.time - a.time;
            float t = segmentTime > 0 ? (elapsed - a.time) / segmentTime : 0f;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(a.position, b.position, t);
            transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

            // Prevent physics interference
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

    }
}
