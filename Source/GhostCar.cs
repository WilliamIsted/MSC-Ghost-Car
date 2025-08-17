using Harmony;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using UnityEngine;

namespace GhostCar
{
    public class GhostCar : Mod
    {

        public override string ID => "com.williamisted.MSCGhostCarMod";
        public override string Name => "Ghost Car";
        public override string Version => "0.0.1";
        public override string Author => "WilliamIsted";
        //public override byte[] Icon => null;
        public override string Description => "A ghost car mod for all your rally needs";

        public string assetsFolder => ModLoader.GetModAssetsFolder(this);

        public override void ModSetup()
        {
            SetupFunction(Setup.OnNewGame, DoOnNewGame);
            SetupFunction(Setup.OnMenuLoad, DoOnMenuLoad);
            SetupFunction(Setup.PreLoad, DoPreLoad);
            SetupFunction(Setup.OnLoad, DoOnLoad);
            SetupFunction(Setup.PostLoad, DoPostLoad);
            SetupFunction(Setup.OnSave, DoOnSave);
            SetupFunction(Setup.OnGUI, DoOnGUI);
            SetupFunction(Setup.Update, DoUpdate);
            SetupFunction(Setup.FixedUpdate, DoFixedUpdate);
            SetupFunction(Setup.OnModEnabled, DoOnModEnabled);
            SetupFunction(Setup.OnModDisabled, DoOnModDisabled);
            SetupFunction(Setup.ModSettingsLoaded, DoModSettingsLoaded);
            SetupFunction(Setup.ModSettings, DoModSettings);
        }

        /*
         * 
         * 
         * 
         */

        private void DoOnNewGame()
        {
            // TODO: Implement logic for OnNewGame setup
            ModConsole.Print("DoOnNewGame");
        }

        private void DoOnMenuLoad()
        {
            // User has opened game, or exited to the menu
        }

        private void DoPreLoad()
        {
            // TODO: Implement logic for PreLoad setup
            ModConsole.Print("DoPreLoad");
        }

        private void DoOnLoad()
        {
            // Apply all mod (effect) patches using Harmony
            var harmony = HarmonyInstance.Create("com.mscti.patches");
            harmony.PatchAll();
        }

        private void DoPostLoad()
        {
            ModConsole.Print("DoPostLoad");
        }

        private void DoOnSave()
        {
            // TODO: Implement logic for OnSave setup
        }

        private void DoOnGUI()
        {
            // TODO: Implement logic for OnGUI setup
        }

        private void DoUpdate()
        {

            if (Input.GetKeyUp(KeyCode.F5))
            {
                
            }

        }

        private void DoFixedUpdate()
        {

            

        }

        private void DoOnModEnabled()
        {
            // TODO: Implement logic for OnModEnabled setup
        }

        private void DoOnModDisabled()
        {
            // TODO: Implement logic for OnModDisabled setup
        }

        private void DoModSettingsLoaded()
        {
            // TODO: Implement logic for ModSettingsLoaded setup
        }

        private void DoModSettings()
        {

            Settings.AddButton("Open Ghost Car Settings", () =>
            {
                // Open the settings UI
                // This is a placeholder for the actual implementation
                ModLoader.print("Opening Ghost Car Settings...");
                ModLoader.print(Path.Combine(ModLoader.GetModAssetsFolder(this), "commands.txt"));
            });

        }

    }
}
