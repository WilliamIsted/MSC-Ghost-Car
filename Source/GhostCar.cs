using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine;

using MSCLoader;
using HutongGames.PlayMaker.Actions;

namespace GhostCar
{
    public class GhostCar : Mod
    {

        public override string ID => "GhostCar";
        public override string Name => "Ghost Car";
        public override string Version => "0.2.0";
        public override string Author => "WilliamIsted";
        //public override byte[] Icon => null;
        public override string Description => "A ghost car mod for all your rally needs";

        //public string assetsFolder => ModLoader.GetModAssetsFolder(this);
        //public string replayFolder => Path.Combine(assetsFolder, "Replays");
        public static string assetsFolder { get; private set; }
        public static string replayFolder => Path.Combine(assetsFolder, "Replays");

        private bool isActive = false;

        private Recorder recorder;
        private AlphaManager alphaManager;
        private GameObject ghostInstance;

        private SettingsKeybind kbStart;
        private SettingsKeybind kbStop;
        private SettingsSliderInt sliderAlpha;
        private SettingsHeader SettingsNewReplay;
        private SettingsHeader SettingsCurrentReplay;

        public override void ModSetup()
        {
            assetsFolder = ModLoader.GetModAssetsFolder(this);

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

            if (ghostInstance != null)
            {
                UnityEngine.GameObject.Destroy(ghostInstance);
                ghostInstance = null;
            }

            GameObject menuCar = GameObject.Find("Scene/Car/MenuCar");
            if (menuCar != null)
            {
                GameObject clone = GameObject.Instantiate(menuCar);
                GameObject.DontDestroyOnLoad(clone);
                clone.name = "GhostMenuCar";

                clone.transform.Find("wheelFL").transform.localRotation = new Quaternion();
                clone.transform.Find("wheelFR").transform.localRotation = new Quaternion();

                clone.transform.Find("_Body/hood_pivot").transform.localRotation = new Quaternion();
                clone.transform.Find("_Body/door_right_pivot").transform.localRotation = new Quaternion();
                clone.transform.Find("_Body/door_left_pivot").transform.localRotation = new Quaternion();

                clone.transform.Find("_Body/bootlid_pivot").transform.localRotation = new Quaternion();

                clone.gameObject.transform.position = new Vector3(0, -10f, 0);

                alphaManager = clone.AddComponent<AlphaManager>();
                alphaManager.CollectMaterials(clone, sliderAlpha.GetValue() / 10f);
            }
        }

        private void DoPreLoad()
        {
            ModConsole.Print("DoPreLoad");
        }

        private void DoOnLoad()
        {
            ModConsole.Print("DoOnLoad");

            // Apply all patches using Harmony
            var harmony = Harmony.HarmonyInstance.Create("com.mscti.patches");
            harmony.PatchAll();

            if (Recorder.Instance != null && Recorder.Instance.isActive)
            {
                recorder = Recorder.Instance;
            }
        }

        private void DoPostLoad()
        {
            ModConsole.Print("DoPostLoad");

            try
            {

                ghostInstance = GameObject.Find("GhostMenuCar");

                List<string> SharedStates = new List<string>
                {
                    "Checkpoint 2",
                    "Checkpoint 3",
                    "Checkpoint 4"
                };

                // Rally - Day 1
                //
                GameObject Rally1 = Helper.FindGameObjectByPath("RALLY/Saturday/TimingSaturday");
                EventListener EventRally1 = Rally1.AddComponent<EventListener>();
                EventRally1.init(
                    "Timing",
                    "Checkpoint 1",
                    "Finish",
                    SharedStates
                );

                // Rally - Day 2
                //
                GameObject Rally2 = Helper.FindGameObjectByPath("RALLY/Sunday/TimingSunday");
                EventListener EventRally2 = Rally2.AddComponent<EventListener>();
                EventRally2.init(
                    "Timing",
                    "Checkpoint 1",
                    "Finish",
                    SharedStates
                );

                // Drag Race
                //
                GameObject Drag = Helper.FindGameObjectByPath("DRAGRACE/LOD/DRAGSTRIP/DragTiming");
                EventListener EventDrag = Drag.AddComponent<EventListener>();
                EventDrag.init(
                    "Timing",
                    "Checkpoint 1",
                    "Finish",
                    SharedStates
                );

            }
            catch (Exception e)
            {
                ModConsole.Print(e.Message);
                ModConsole.Print(e);
            }
        }

        private void DoOnSave()
        {
            ModConsole.Print("DoOnSave");
        }

        private void DoOnGUI()
        {
            //ModConsole.Print("DoOnGUI");

            // wheelFR -> angularVelocity (Spin tyres)
            // wheelFR -> steering 
        }

        private void DoUpdate()
        {

            if (Input.GetKeyUp(KeyCode.F5))
            {
                /* if (GhostRecorder.IsRunning)
                {
                    GhostRecorder.Stop();
                }
                else
                {
                    GhostRecorder.Start();
                } */

                ModConsole.Print("F5 Pressed");
                //Recorder.Instance.saveStage();
                //ModConsole.Print("Save Stage");

                Recorder.Instance.Stop();


                GameObject currentVehicle = Helper.GetVehicle();

                if (currentVehicle != null)
                {
                    var Recorder = currentVehicle.AddComponent<Recorder>();
                    Recorder.type = Recorder.Type.Rally1;
                }

            }

            if (Input.GetKeyUp(KeyCode.F6))
            {

                ModConsole.Print("F6 Pressed");

                //Recorder.Instance.Stop();

                if (ghostInstance == null)
                {
                    ModConsole.Error("No ghost instance to attach Replay to.");
                    return;
                }

                ghostInstance.GetComponents<Replay>()
                    .ToList()
                    .ForEach(r => UnityEngine.Object.Destroy(r));

                ghostInstance.transform.position = new Vector3(0, -10f, 0);
                ghostInstance.SetActive(true);

                string filename = Helper.GetLogFiles(null, 1).FirstOrDefault();
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

            if (kbStart.GetKeybindDown())
            {
                ModConsole.Warning("Pressing just single key is required for this to show");
            }
            if (kbStop.GetKeybindDown())
            {
                ModConsole.Warning("Pressing both keys is required for this to show");
            }

        }

        private void DoFixedUpdate()
        {

            /* if (GhostRecorder.IsRunning)
            {
                GhostRecorder.Update();
            } */

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

            try
            {

                SettingsManager.initSettings();

            }
            catch (Exception ex)
            {
                ModConsole.Print(ex.ToString());
                ModConsole.Print(ex.Message);
            }

            string[] ghostCarNames = { "Satsuma", "Fittan" };

            Settings.AddButton("Open Replay Folder", new Action(() => {
                if (Directory.Exists(replayFolder))
                {
                    // Opens the folder directly
                    Process.Start("explorer.exe", replayFolder);
                }
                else if (File.Exists(replayFolder))
                {
                    // Opens Explorer and highlights the file
                    Process.Start("explorer.exe", "/select,\"" + replayFolder + "\"");
                }
            }));
            Settings.AddDropDownList("ghostCarName", "Replay Vehicle", ghostCarNames, 0, new Action(() =>
            {

            }),false);
            Settings.AddText("", false);
            Settings.AddCheckBox("practiceReplays", "Automatically Record Rally", true);
            Settings.AddCheckBox("practiceReplays", "Automatically Record Drag Race", true);
            Settings.AddText("");
            Settings.AddCheckBox("practiceReplays", "Show Guest Practice Replays", true);

            Keybind.AddHeader("Ghost Car Recording");
            kbStart = Keybind.Add("gcStart", "Start", KeyCode.None);
            kbStop = Keybind.Add("gcStop", "Stop", KeyCode.None);

            SettingsNewReplay = Settings.AddHeader("New Replay", false, false);
            Settings.AddText("Your replay will be saved with the following name:");
            Settings.AddTextBox("newSaveName", "Replay Name", "", "");
            Settings.CreateGroup(true); // true = horizontal, false = vertical
            Settings.AddText("Minutes\r\n04");
            Settings.AddText("Seconds\r\n20");
            Settings.AddText("Microseconds\r\n418");
            Settings.EndGroup(); // End, closes the group and goes back to default layout

            Settings.AddText("");

            Settings.CreateGroup(true); // true = horizontal, false = vertical
            Settings.AddText("Event: Rally - Stage 1");
            Settings.AddText("Vehicle: Satsuma");
            Settings.EndGroup(); // End, closes the group and goes back to default layout

            SettingsCurrentReplay = Settings.AddHeader("Current Replay");
            Settings.AddText("Adjust the playback time of a replay to help practice");
            Settings.CreateGroup(true); // true = horizontal, false = vertical
            Settings.AddTextBox("ReplayMinutes", "Minutes", "00", "00", UnityEngine.UI.InputField.ContentType.DecimalNumber);
            Settings.AddTextBox("ReplaySeconds", "Seconds", "00", "00", UnityEngine.UI.InputField.ContentType.DecimalNumber);
            Settings.AddTextBox("ReplayMicroseconds", "Microseconds", "000", "000", UnityEngine.UI.InputField.ContentType.DecimalNumber);
            Settings.EndGroup(); // End, closes the group and goes back to default layout

            sliderAlpha = Settings.AddSlider("sliderAlpha", "Opacity", 0, 8, 4, onValueChanged: new Action(() =>
            {

                alphaManager.SetAlpha((float)sliderAlpha.GetValue() / 10f);

            }));

            Settings.AddHeader("Rally - Stage 1", true);
            Settings.CreateGroup(true);
            Settings.AddButton("04:32:420 - Ginauz", new Action(() => { }));
            Settings.EndGroup();
            Settings.AddButton("04:50:220 - CiggyFreud", new Action(() => { }));
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));

            Settings.AddHeader("Rally - Stage 2", true);
            Settings.AddButton("Stage 2: 00:00:000 - Blah blah", new Action(() => { }));
            Settings.AddButton("Stage 2: 00:00:000 - Blah blah", new Action(() => { }));

            Settings.AddHeader("Drag Race", true);
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));

            Settings.AddHeader("Highway", true);
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));
            Settings.AddButton("00:00:000 - Blah blah", new Action(() => { }));

            Settings.AddHeader("Other Replays", true);
            Settings.AddButton("05:12:320", new Action(() => { }));
            Settings.AddButton("05:04:112", new Action(() => { }));
            Settings.AddButton("04:40:030", new Action(() => { }));
            Settings.AddButton("DNF", new Action(() => { }));
            Settings.AddButton("Highway: 21:14:790", new Action(() => { }));
            
            

        }

        /*
         * 
         * 
         * 
         */

        public void CloneCar()
        {

            //GameObject ghost = GameObject.Find("TRAFFIC/VehiclesDirtRoad/Rally/FITTAN");
            GameObject ghost = GameObject.Find("GhostMenuCar");
            if (ghost == null)
            {
                ModConsole.Error("Vehicle not found.");
                return;
            }

            //ghostInstance = GameObject.Instantiate(fittan);
            ghostInstance = GameObject.Instantiate(ghost);
            ghostInstance.name = "GhostCar";

            /* foreach (var rb in ghostInstance.GetComponentsInChildren<Rigidbody>(true))
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

            alphaManager = ghostInstance.AddComponent<AlphaManager>();
            alphaManager.CollectMaterials(ghostInstance, sliderAlpha.GetValue() / 10f); // Or user-controlled alpha

            //ghostMaterials.Clear();


            /* foreach (var renderer in ghostInstance.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    SetMaterialTransparent(mat, 0.4f);

                    ghostMaterials.Add(mat);
                }
            } */

            // Make ghost visual
            /* ghostMaterials.Clear();

            foreach (var renderer in ghostInstance.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = Shader.Find("Transparent/Diffuse");
                    Color color = mat.color;
                    color.a = 0.4f;
                    mat.color = color;

                    ghostMaterials.Add(mat);
                }
            } */

            ghostInstance.SetActive(false);

        }

        public static void SetMaterialTransparent(Material mat, float alpha)
        {
            if (alpha >= 0.99f)
            {
                // Set to opaque mode
                mat.SetFloat("_Mode", 0); // Opaque
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1; // Use default opaque queue
            }
            else
            {
                // Set to transparent mode
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 1); // Still depth-write
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
        }


    }
}
