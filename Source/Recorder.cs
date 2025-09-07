using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;

namespace GhostCar
{

    public class Recorder : MonoBehaviour
    {

        /*
         * 
         * Protected
         * 
         */

        protected int schema = 1;
        protected bool active = false;
        protected int stage = 1;

        /*
         * 
         * Private
         * 
         */

        private static Recorder instance;
        public static Recorder Instance => instance;

        private List<string> buffer     = new List<string>();
        private object lockBuffer       = new object();
        private Thread bufferThread;
        private float startTime         = 0f;
        private string timestamp        = null;
        private string filename         = null;
        private string filetime         = null;

        private GameObject Vehicle      = null;
        private Transform WheelFL       = null;
        private MonoBehaviour Wheel     = null;

        /*
         * 
         * Public
         * 
         */

        public bool isActive => active;

        public enum Type
        {
            Rally1,
            Rally2,
            Drag,
            Highway,
            Free
        }

        public Type type = Type.Free;

        protected string GetDisplayName()
        {
            switch (type)
            {
                case Type.Rally1: return "Stage1";
                case Type.Rally2: return "Stage2";
                case Type.Drag: return "Drag Race";
                case Type.Highway: return "Highway";
                default: return "Free";
            }
        }

        /*
         * 
         *
         * 
         */

        void Awake()
        {

            ModConsole.Print("Recorder Awake");

            if (instance != null && instance != this)
            {
                ModConsole.Error("Multiple Recorder instances detected! Destroying extra.");
                Destroy(this);
                return;
            }

            instance = this;

            ModConsole.Print($"Recorder attached to {this.name}");

            if (this == null || this.gameObject == null)
            {
                ModConsole.Log("Cannot access parent GameObject. Detaching.");
                Destroy( this );
            }

        }

        private void Start()
        {

            try
            {

                ModConsole.Print("Recorder: Start() called");

                startTime = Time.time;
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                filetime = DateTime.Now.ToString("yyyyMMddTHHmmss");
                filename = Path.Combine(GhostCar.assetsFolder, $"Replays\\ghostcar_{filetime}.log");

                ModConsole.Print(filename);

                Vehicle = Helper.GetVehicle();

                ModConsole.Print("Vehicle");
                ModConsole.Print(Vehicle.name);

                // To do: Need to track child of wheelFL. wheelFR has different localrotation.
                WheelFL = Helper.FindDeepChild(Vehicle.transform, "wheelFL");

                ModConsole.Print("WheelFL");
                ModConsole.Print(WheelFL.name);

                Wheel = WheelFL.GetComponents<MonoBehaviour>()
                    .FirstOrDefault(mono => mono.GetType().Name == "Wheel");

                ModConsole.Print("Wheel");
                //ModConsole.Print(Wheel.name);

                string header = $"# version={schema}; timestamp={timestamp}; duration=0.000; vehicle={Definitions.VehicleName[Vehicle.name]}; type={GetDisplayName()}";

                ModConsole.Print(header);

                try
                {
                    File.WriteAllText(filename, header + Environment.NewLine, Encoding.ASCII);
                }
                catch (Exception ex)
                {
                    ModConsole.Error("GhostRecorder: Failed to write header - " + ex.Message);
                }

                bufferThread = new Thread(() =>
                {
                    while (active)
                    {
                        Thread.Sleep(1000);
                        FlushBuffer();
                    }
                });

                bufferThread.IsBackground = true;
                bufferThread.Start();

                ModConsole.Print("GhostRecorder started logging to: " + filename);

                active = true;

            }
            catch (Exception e)
            {
                ModConsole.Print("ERROR");
                ModConsole.Print(e.Message);
            }

        }

        public void saveStage(string name = null)
        {
            if (name == null)
            {
                doSaveStage(this.stage++.ToString());
            }
            else
            {
                doSaveStage(name);
            }
        }

        private void doSaveStage(string name)
        {

            string data = $"# stage={name}; timestamp={Time.time - startTime};";

            ModConsole.Print($"Stage: {name}");

            try
            {
                lock (lockBuffer)
                {
                    buffer.Add(data);
                }
                //File.WriteAllText(filename, data + Environment.NewLine, Encoding.ASCII);
            }
            catch (Exception ex)
            {
                ModConsole.Error("GhostRecorder: Failed to write header - " + ex.Message);
            }

        }

        public void Stop()
        {

            ModConsole.Print("Recorder STOP Called");
            if (!active) return;

            active = false;

            FlushBuffer(true);

            float duration = Time.time - startTime;
            ModConsole.Print($"Final duration: {duration:F3}");

            try
            {
                // Read all lines
                string[] lines = File.ReadAllLines(filename);

                if (lines.Length > 0 && lines[0].StartsWith("#"))
                {
                    string updatedHeader = UpdateDurationInHeader(lines[0], duration);
                    lines[0] = updatedHeader;

                    File.WriteAllLines(filename, lines, Encoding.ASCII);

                    ModConsole.Print("Metadata header updated with final duration.");
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error("Failed to update metadata header: " + ex.Message);
            }

        }

        void OnEnable()
        {
            ModConsole.Print("Recorder: OnEnable()");
            Start();
        }

        void OnDisable()
        {
            Stop();
        }

        void OnDestroy()
        {
            Stop();
            instance = null;

            ModConsole.Print("Recorder: Destroyed.");
        }

        public void Update()
        {

            //ModConsole.Print($"Recorder: Update() Called. Active: {active}");

            if (!active || Vehicle == null || Wheel == null)
                return;

            float    pace = Time.time - startTime;

            Transform       t = Vehicle.transform;
            Vector3         p = t.position;
            Quaternion      r = t.rotation;
            //Quaternion      s = WheelFL.localRotation;
            Quaternion      s = new Quaternion();

            string line = string.Format("{0:F3},{1:F3},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F3}",
                pace, p.x, p.y, p.z, r.x, r.y, r.z, r.w, s.x, s.y, s.z, s.w);

            lock (lockBuffer)
            {
                buffer.Add(line);
            }

        }

        private void FlushBuffer(bool synchronous = false)
        {
            List<string> writeBuffer;

            lock (lockBuffer)
            {
                if (buffer.Count == 0) return;

                writeBuffer = new List<string>(buffer);
                buffer.Clear();
            }

            Action writeAction = () =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var line in writeBuffer)
                        sb.AppendLine(line);

                    File.AppendAllText(filename, sb.ToString(), Encoding.ASCII);
                }
                catch (Exception e)
                {
                    ModConsole.Error($"Recorder: {e}");
                }
            };

            if (synchronous)
                writeAction();
            else
                ThreadPool.QueueUserWorkItem(_ => writeAction());
        }

        private string UpdateDurationInHeader(string headerLine, float duration)
        {
            var parts = headerLine.Substring(1).Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].TrimStart().StartsWith("duration="))
                {
                    parts[i] = $" duration={duration:F3}";
                    break;
                }
            }

            return "#" + string.Join(";", parts);
        }


    }

}
