using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using MSCLoader;

namespace GhostCar
{
    
    public class Helper
    {

        public static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Finds a GameObject by path, including inactive ones.
        /// </summary>
        /// <param name="path">Hierarchy path using `/`, e.g., "RALLY/Sunday/TimingSunday"</param>
        /// <returns>The found GameObject, or null if not found</returns>
        public static GameObject FindGameObjectByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string[] parts = path.Split('/');
            if (parts.Length == 0) return null;

            GameObject current = GameObject.Find(parts[0]);
            if (current == null)
            {
                ModConsole.Error($"FindGameObjectByPath: Root object '{parts[0]}' not found.");
                return null;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                Transform child = current.transform.Find(parts[i]);
                if (child == null)
                {
                    ModConsole.Error($"FindGameObjectByPath: Could not find '{parts[i]}' under '{current.name}'.");
                    return null;
                }

                current = child.gameObject;
            }

            return current;
        }

        public static string GetLogFile(string filename)
        {
            string dir = Path.Combine( "Replays\\", GhostCar.assetsFolder );

            if (string.IsNullOrEmpty(filename) || !filename.StartsWith("ghostcar_") || !filename.EndsWith(".log"))
                return null;

            string fullPath = Path.Combine(dir, filename);

            return File.Exists(fullPath) ? fullPath : null;
        }

        public static List<string> GetLogFiles(string afterFile = null, int limit = 10)
        {
            ModConsole.Print("GetLogFiles()");

            string dir = Path.Combine(GhostCar.assetsFolder, "Replays");
            string[] files = Directory.GetFiles(dir, "ghostcar_*.log");

            ModConsole.Print("DONE");

            if (files.Length == 0)
                return new List<string>();

            var ordered = files.OrderByDescending(f => File.GetLastWriteTime(f)).ToList();

            if (!string.IsNullOrEmpty(afterFile))
            {
                int index = ordered.FindIndex(f => Path.GetFileName(f).Equals(afterFile, StringComparison.OrdinalIgnoreCase));
                if (index != -1 && index + 1 < ordered.Count)
                {
                    return ordered.Skip(index + 1).Take(limit).ToList();
                }
                else
                {
                    return new List<string>();
                }
            }

            return ordered.Take(limit).ToList();
        }

        /// <summary>
        /// Retrieves a vehicle GameObject by name. If no name is provided, uses the player's current vehicle.
        /// Caches found vehicles in <see cref="Definitions.Vehicles"/> for faster future access.
        /// </summary>
        /// <param name="name">The name of the vehicle GameObject. If null or empty, uses the player's current vehicle name.</param>
        /// <returns>The <see cref="GameObject"/> representing the vehicle, or null if not found.</returns>
        public static GameObject GetVehicle(string name = null)
        {
            // If no vehicle set, try to pick active vehicle
            if (string.IsNullOrEmpty(name))
            {
                name = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle").Value;
            }

            // Pick vehicle handle from Vehicles dictionary, on fail create new handle
            if (!Definitions.Vehicles.ContainsKey(name) || Definitions.Vehicles[name] == null)
            {
                GameObject found = GameObject.Find(name);
                if (found != null)
                    Definitions.Vehicles[name] = found;
            }

            return Definitions.Vehicles.ContainsKey(name) ? Definitions.Vehicles[name] : null;
        }

        public static List<Dictionary<string, string>> GetReplayMetadata(int limit = 10)
        {
            string dir = Path.Combine(GhostCar.assetsFolder, "Replays");

            if (!Directory.Exists(dir))
                return new List<Dictionary<string, string>>();

            return Directory.GetFiles(dir, "ghostcar_*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Take(limit)
                .Select(file =>
                {
                    try
                    {
                        using (var reader = new StreamReader(file))
                        {
                            string firstLine = reader.ReadLine();
                            return ParseMetadata(firstLine);
                        }
                    }
                    catch
                    {
                        return new Dictionary<string, string>(); // or log error
                    }
                })
                .Where(meta => meta.Count > 0)
                .ToList();
        }

        public static Dictionary<string, string> ParseMetadata(string line)
        {
            var data = new Dictionary<string, string>();

            if (!line.StartsWith("#"))
                return data;

            line = line.Substring(1).Trim(); // Remove leading '#'
            var fields = line.Split(';');

            foreach (var field in fields)
            {
                var pair = field.Trim().Split('=');
                if (pair.Length == 2)
                    data[pair[0].Trim()] = pair[1].Trim();
            }

            return data;

        }

    }

}
