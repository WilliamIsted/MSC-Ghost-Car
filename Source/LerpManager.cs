using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GhostCar
{
    public static class GhostLerpManager
    {
        private static GameObject satsuma;
        private static Vector3 start;
        private static Vector3 end;
        private static float duration = 1f;
        private static float elapsed = 0f;
        private static bool isLerping = false;

        private static Vector3 previousPos;
        private static bool hasPrevious = false;

        public static void BeginLerpForward()
        {
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            if (satsuma == null)
            {
                ModConsole.Error("Satsuma not found!");
                return;
            }

            start = satsuma.transform.position;
            end = start + satsuma.transform.forward * 3f;
            elapsed = 0f;
            isLerping = true;
        }

        public static void FixedUpdate(float deltaTime)
        {
            if (!isLerping || satsuma == null)
                return;

            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            satsuma.transform.position = Vector3.Lerp(start, end, t);

            // Spin wheels
            if (!hasPrevious)
            {
                previousPos = satsuma.transform.position;
                hasPrevious = true;
            }
            else
            {
                float wheelRadius = 0.3f;
                float moved = Vector3.Distance(previousPos, satsuma.transform.position);
                float degrees = (moved / (2f * Mathf.PI * wheelRadius)) * 360f;
                RotateWheels(degrees);
                previousPos = satsuma.transform.position;
            }

            if (t >= 1f)
            {
                isLerping = false;
            }
        }

        private static void RotateWheels(float degrees)
        {
            /* var wheelObjects = satsuma.GetComponentsInChildren<Transform>()
                .Where(t => t.name.ToLower().Contains("wheel"))
                .ToArray();

            foreach (var wheel in wheelObjects)
            {
                // Rotate forward around local X
                wheel.Rotate(Vector3.right, degrees, Space.Self);
            } */
        }


    }
}
