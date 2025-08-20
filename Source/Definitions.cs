using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }
}
