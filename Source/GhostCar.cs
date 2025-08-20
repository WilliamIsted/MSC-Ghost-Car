using Harmony;
using MSCLoader;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Effects;

namespace GhostCar
{
    public class GhostCar : Mod
    {

        public override string ID => "GhostCar";
        public override string Name => "Ghost Car";
        public override string Version => "0.1.0";
        public override string Author => "WilliamIsted";
        //public override byte[] Icon => null;
        public override string Description => "A ghost car mod for all your rally needs";

        public string assetsFolder => ModLoader.GetModAssetsFolder(this);

        private bool isActive = false;

        private GameObject ghostInstance;

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
            ModConsole.Print("DoOnNewGame");
        }

        private void DoOnMenuLoad()
        {
            ModConsole.Print("DoOnMenuLoad");
        }

        private void DoPreLoad()
        {
            ModConsole.Print("DoPreLoad");

            CloneCar();
        }

        private void DoOnLoad()
        {
            ModConsole.Print("DoOnLoad");

            // Apply all patches using Harmony
            var harmony = Harmony.HarmonyInstance.Create("com.mscti.patches");
            harmony.PatchAll();
        }

        private void DoPostLoad()
        {
            ModConsole.Print("DoPostLoad");
        }

        private void DoOnSave()
        {
            ModConsole.Print("DoOnSave");
        }

        private void DoOnGUI()
        {
            //ModConsole.Print("DoOnGUI");
        }

        private void DoUpdate()
        {

            if (Input.GetKeyUp(KeyCode.F5))
            {
                if (GhostRecorder.IsRunning)
                {
                    GhostRecorder.Stop();
                }
                else
                {
                    GhostRecorder.Start();
                }
            }

            if (Input.GetKeyUp(KeyCode.F6))
            {

                if (ghostInstance == null)
                {
                    ModConsole.Error("No ghost instance to attach Replay to.");
                    return;
                }

                ghostInstance.SetActive(true); // If you had disabled it earlier

                string filename = Helper.GetLatestLogFile();
                if (filename != null)
                {
                    var replay = ghostInstance.AddComponent<Replay>();
                    replay.filename = filename;
                }
                else
                {
                    ModConsole.Print("No ghost log file found.");
                }

            }

        }

        private void DoFixedUpdate()
        {

            if (GhostRecorder.IsRunning)
            {
                GhostRecorder.Update();
            }

        }

        private void DoOnModEnabled()
        {
            ModConsole.Print("DoOnModEnabled");
        }

        private void DoOnModDisabled()
        {
            ModConsole.Print("DoOnModDisabled");
        }

        private void DoModSettingsLoaded()
        {
            ModConsole.Print("DoModSettingsLoaded");
        }

        private void DoModSettings()
        {

            

        }

        /*
         * 
         * 
         * 
         */

        public void CloneCar()
        {

            GameObject fittan = GameObject.Find("TRAFFIC/VehiclesDirtRoad/Rally/FITTAN");
            if (fittan == null)
            {
                ModConsole.Error("Fittan not found.");
                return;
            }

            ghostInstance = GameObject.Instantiate(fittan);
            ghostInstance.name = "GhostCar";

            foreach (var rb in ghostInstance.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            string[] childrenToRemove = {
                "Navigation", "CarColliderAI", "DeadBody", "CrashEvent",
                "Driver", "DriverHeadPivot", "PlayerTrigger", "Colliders"
            };

            foreach (string name in childrenToRemove)
            {
                var child = ghostInstance.transform.Find(name);
                if (child != null)
                    GameObject.Destroy(child.gameObject);
            }

            var LOD = ghostInstance.transform.Find("LOD");
            if (LOD != null)
            {
                string[] LODChildrenToRemove = {
                    "PlayerFunctions", "DriverHands", "EngineSound",
                    "HeadTarget", "Gearstick", "CarHorn"
                };

                foreach (string name in LODChildrenToRemove)
                {
                    var child = LOD.Find(name);
                    if (child != null)
                        GameObject.Destroy(child.gameObject);
                }
            }

            string[] componentsToDisable = {
                "CarDynamics", "DriveTrain", "MobileCarController", "EventSounds"
            };

            foreach (var mb in ghostInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (Array.Exists(componentsToDisable, c => c == mb.GetType().Name))
                    mb.enabled = false;
            }

            string[] fsmsToRemove = { "Throttle", "Shifting" };
            foreach (var fsm in ghostInstance.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (Array.Exists(fsmsToRemove, f => f == fsm.FsmName))
                    GameObject.Destroy(fsm);
            }

            // Make ghost visual
            foreach (var renderer in ghostInstance.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = Shader.Find("Transparent/Diffuse");
                    Color color = mat.color;
                    color.a = 0.4f;
                    mat.color = color;
                }
            }

            ghostInstance.SetActive(false);

        }

    }
}
