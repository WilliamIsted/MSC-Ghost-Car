using Harmony;
using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using UnityEngine;

namespace GhostCar
{

    public class GhostCar : Mod
    {

        [HarmonyPatch(typeof(CarController), "FixedUpdate")]
        public static class GhostCarMovementTracker
        {
            static void Postfix(CarController __instance)
            {
                if (__instance.name != "SATSUMA(557kg, 248)") return;

                var rb = __instance.GetComponent<Rigidbody>();
                if (rb == null) return;

                // Optional: Access wheels if needed
                var wheelsField = AccessTools.Field(typeof(CarController), "allWheels");
                Wheel[] wheels = wheelsField?.GetValue(__instance) as Wheel[];

                // Log steering + movement data
                GhostCarDataStore.Update(
                    rb.position,
                    rb.rotation,
                    rb.velocity,
                    __instance.steerInput,
                    __instance.steering,
                    wheels
                );
            }
        }

        public override string ID => "com.williamisted.MSCGhostCarMod";
        public override string Name => "Ghost Car";
        public override string Version => "0.0.1";
        public override string Author => "WilliamIsted";
        //public override byte[] Icon => null;
        public override string Description => "A ghost car mod for all your rally needs";

        public string assetsFolder => ModLoader.GetModAssetsFolder(this);

        private bool isActive = false;

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

        private GameObject Player = null;
        private GameObject Rally = null;

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

            Player = GameObject.Find("PLAYER");
            Rally = GameObject.Find("RALLY/Saturday/TimingSaturday");
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
            string currentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle").Value;

            if (currentVehicle == "")
            {
                GameObject vehicle = GameObject.Find(currentVehicle);
            }

            if (!isActive)
            {
                PlayMakerFSM Timing = Rally.GetComponents<PlayMakerFSM>()
                    .FirstOrDefault(fsm => fsm.FsmName == "Timing");

                if (Timing != null)
                {
                    if (Timing.ActiveStateName == "Checkpoint 1")
                    {
                        ModConsole.Log("Rally Started");
                        isActive = true;
                        string fileName = Path.Combine(assetsFolder, "ghostcar_{timestamp}.log");
                        Replay.CreateGhostSatsuma(fileName);
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.F5))
            {
                if (isActive)
                {
                    isActive = !isActive;
                    GhostLogger.Stop();
                    return;
                }

                isActive = true;
                //GhostLerpManager.BeginLerpForward();
                GhostLogger.Start();

                /* float time = Time.time;
                GhostLogWriter.StartNewLog(time.ToString());
                GhostLogWriter.LogFrame(time); */

                /* isActive = !isActive;

                if (this.isActive)
                {
                    float time = Time.time;
                    GhostLogWriter.StartNewLog(time.ToString());
                    GhostLogWriter.LogFrame(time);
                }
                else
                {
                    GhostLogWriter.Close();
                } */
            }

            if (isActive)
            {
                //GhostLerpManager.FixedUpdate(Time.fixedDeltaTime);
            }

            if (Input.GetKeyUp(KeyCode.F6))
            {
                ModConsole.Log("Start GhostReplay");

                /* string latest = GhostLogger.GetLatestLogFile();
                if (latest != null)
                { */
                    // Use only the ghost
                    //string fileName = Path.GetFileName(latest); // just "ghostcar_20250818.log"
                    string fileName = Path.Combine(assetsFolder, "ghostcar_{timestamp}.log");
                    Replay.CreateGhostSatsuma(fileName);
                /* }
                else
                {
                    ModConsole.Log("No ghost log file found.");
                } */
            }

        }

        private void DoFixedUpdate()
        {

            if (this.isActive)
            {
                GhostLogger.Update();
                /* float time = Time.time;
                GhostLogWriter.LogFrame(time);
                this.isActive = false; */
            }

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

        /*
         * 
         * 
         * 
         */

        public static class GhostCarDataStore
        {
            private static readonly object _lock = new object();

            private static Vector3 _position;
            private static Quaternion _rotation;
            private static Vector3 _velocity;

            private static float _steerInput;
            private static float _steering;

            private static float[] _wheelAngles;

            public static void Update(Vector3 pos, Quaternion rot, Vector3 vel, float steerInput, float steering, Wheel[] wheels)
            {
                lock (_lock)
                {
                    _position = pos;
                    _rotation = rot;
                    _velocity = vel;
                    _steerInput = steerInput;
                    _steering = steering;

                    if (wheels != null)
                    {
                        _wheelAngles = new float[wheels.Length];
                        for (int i = 0; i < wheels.Length; i++)
                        {
                            _wheelAngles[i] = wheels[i]?.steering ?? 0f;
                        }
                    }
                }
            }

            public static void Get(out Vector3 pos, out Quaternion rot, out Vector3 vel, out float steerInput, out float steering, out float[] wheelAngles)
            {
                lock (_lock)
                {
                    pos = _position;
                    rot = _rotation;
                    vel = _velocity;
                    steerInput = _steerInput;
                    steering = _steering;
                    wheelAngles = (_wheelAngles != null) ? (float[])_wheelAngles.Clone() : new float[0];
                }
            }
        }

        public static class GhostLogWriter
        {
            private static readonly object fileLock = new object();
            private static string logPath;
            private static StreamWriter writer;

            public static void StartNewLog(string filename)
            {
                logPath = Path.Combine(new GhostCar().assetsFolder, $"GhostCarLogs\\{filename}.csv");

                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                writer = new StreamWriter(logPath, false);
                writer.WriteLine("time,posX,posY,posZ,rotX,rotY,rotZ,rotW,velX,velY,velZ,steerInput,steerOutput,wheelAngles...");
                writer.Flush();
            }

            public static void LogFrame(float time)
            {
                GhostCarDataStore.Get(out var pos, out var rot, out var vel, out var steerIn, out var steerOut, out var wheels);

                var line = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0:F3},{1:F3},{2:F3},{3:F3},{4:F5},{5:F5},{6:F5},{7:F5},{8:F3},{9:F3},{10:F3},{11:F3},{12:F3}",
                    time, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, vel.x, vel.y, vel.z, steerIn, steerOut
                );

                if (wheels != null && wheels.Length > 0)
                {
                    line += "," + string.Join(",", (string[])wheels.Select(w => w.ToString("F3")));
                }

                lock (fileLock)
                {
                    writer.WriteLine(line);
                    writer.Flush(); // or buffer if performance matters
                }
            }

            public static void Close()
            {
                lock (fileLock)
                {
                    writer?.Flush();
                    writer?.Close();
                    writer = null;
                }
            }
        }


    }
}
