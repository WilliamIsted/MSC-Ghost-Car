using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public string fileName = "ghostlog_20250818.log"; // Replace with your file
        private List<Entry> entries = new List<Entry>();
        private float startTime = 0f;
        private int currentIndex = 0;
        private bool ready = false;

        void Start()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(fullPath))
            {
                ModConsole.Error("GhostReplay: File not found: " + fullPath);
                return;
            }

            var lines = File.ReadAllLines(fullPath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
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
                ModConsole.Error("GhostReplay: Not enough data to play back.");
                return;
            }

            startTime = Time.time;
            ready = true;
        }

        void Update()
        {
            if (!ready || currentIndex >= entries.Count - 1)
                return;

            float elapsed = Time.time - startTime;

            while (currentIndex < entries.Count - 2 && elapsed > entries[currentIndex + 1].time)
                currentIndex++;

            var a = entries[currentIndex];
            var b = entries[currentIndex + 1];

            float segmentTime = b.time - a.time;
            float t = segmentTime > 0 ? (elapsed - a.time) / segmentTime : 0f;
            t = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(a.position, b.position, t);
            Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);

            transform.position = pos;
            transform.rotation = rot;

            // Optional: cancel rigidbody influence
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        public static GameObject CreateGhostSatsuma(string fileName)
        {
            GameObject vehicle = GameObject.Find("TRAFFIC/VehiclesDirtRoad/Rally/FITTAN");
            if (vehicle == null)
            {
                ModConsole.Error("Satsuma not found.");
                return null;
            }

            // Clone the whole GameObject
            GameObject ghost = GameObject.Instantiate(vehicle);
            ghost.name = "GhostCar";

            RemovePlayerInteraction(ghost);

            // Disable unnecessary components
            foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (var col in ghost.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            foreach (var fsm in ghost.GetComponentsInChildren<PlayMakerFSM>())
            {
                fsm.enabled = false;
            }

            foreach (var carCtrl in ghost.GetComponentsInChildren<MonoBehaviour>())
            {
                if (carCtrl.GetType().Name.Contains("Colliders") || carCtrl.GetType().Name.Contains("Drivetrain") || carCtrl.GetType().Name.Contains("Drivetrain"))
                {
                    carCtrl.enabled = false;
                }
            }

            // Optional: make ghost semi-transparent
            MakeGhostVisual(ghost);

            ghost.AddComponent<GhostSanitizer>().Begin();

            // Add replay component
            Replay replay = ghost.AddComponent<Replay>();
            replay.fileName = fileName;

            return ghost;
        }

        private static void RemovePlayerInteraction(GameObject ghost)
        {
            // 1. Destroy the entire seat trigger tree
            var seat = ghost.transform.Find("SEAT/DriverSeat");
            if (seat != null)
                GameObject.Destroy(seat.gameObject);

            // 2. Destroy any child named SEAT (defensive)
            var seatRoot = ghost.transform.Find("SEAT");
            if (seatRoot != null)
                GameObject.Destroy(seatRoot.gameObject);

            // 3. Destroy any FSMs called SEATINTERACTION or PLAYERINTERACTION
            foreach (var fsm in ghost.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (fsm.FsmName == "SEATINTERACTION" || fsm.FsmName == "PLAYERINTERACTION")
                    GameObject.Destroy(fsm);
            }

            // 4. Disable all trigger colliders
            foreach (var col in ghost.GetComponentsInChildren<Collider>(true))
            {
                if (col.isTrigger)
                    col.enabled = false;
            }

            // 5. Disable all components referencing input or player
            foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string type = mb.GetType().Name.ToLowerInvariant();
                if (type.Contains("driver") || type.Contains("seat") || type.Contains("playerinput"))
                {
                    mb.enabled = false;
                }
            }
        }


        public static void MakeGhostVisual(GameObject ghost)
        {
            foreach (var renderer in ghost.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = Shader.Find("Transparent/Diffuse");
                    Color color = mat.color;
                    color.a = 0.4f;
                    mat.color = color;
                }
            }
        }

    }

}