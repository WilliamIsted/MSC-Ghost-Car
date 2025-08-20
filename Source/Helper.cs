using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GhostCar
{
    
    public class Helper
    {

        public static string GetLatestLogFile()
        {
            string dir = new GhostCar().assetsFolder;
            string[] files = Directory.GetFiles(dir, "ghostlog_*.log");

            if (files.Length == 0)
                return null;

            return files.OrderByDescending(f => File.GetLastWriteTime(f)).First();
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
