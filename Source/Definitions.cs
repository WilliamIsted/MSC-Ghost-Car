using System;
using System.Collections.Generic;
using UnityEngine;

namespace GhostCar
{
    public static class Definitions
    {

        public static Dictionary<string, GameObject> Vehicles = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase)
        {
            { "Boat", GameObject.Find("BOAT") },
            { "Combine", GameObject.Find("COMBINE(350-400psi)") },
            { "Ferndale", GameObject.Find("FERNDALE(1630kg)") },
            { "Gifu", GameObject.Find("GIFU(750/450psi)") },
            { "Hayosiko", GameObject.Find("HAYOSIKO(1500kg, 250)") },
            { "Jonnez", GameObject.Find("JONNEZ ES(Clone)") },
            { "Kekmet", GameObject.Find("KEKMET(350-400psi)") },
            { "Ruscko", GameObject.Find("RCO_RUSCKO12(270)") },
            { "Satsuma", GameObject.Find("SATSUMA(557kg, 248)") }
        };

        public static Dictionary<string, string> VehicleName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "BOAT", "Boat" },
            { "COMBINE(350-400psi)", "Combine" },
            { "FERNDALE(1630kg)", "Ferndale" },
            { "GIFU(750/450psi)", "Gifu" },
            { "HAYOSIKO(1500kg, 250)", "Hayosiko" },
            { "JONNEZ ES(Clone)", "Jonnez" },
            { "KEKMET(350-400psi)", "Kekmet" },
            { "RCO_RUSCKO12(270)", "Ruscko" },
            { "SATSUMA(557kg, 248)", "Satsuma" }
        };

    }
}
