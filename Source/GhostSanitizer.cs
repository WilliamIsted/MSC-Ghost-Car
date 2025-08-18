using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GhostCar
{
    using UnityEngine;
    using System.Collections;
    using HutongGames.PlayMaker;

    public class GhostSanitizer : MonoBehaviour
    {
        public void Begin()
        {
            StartCoroutine(DelaySanitize());
        }

        private IEnumerator DelaySanitize()
        {
            yield return null; // wait 1 frame

            // 1. Disable all seat FSMs
            foreach (var fsm in GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (fsm.FsmName == "SEATINTERACTION" || fsm.FsmName == "PLAYERINTERACTION")
                {
                    Destroy(fsm);
                }
            }

            // 2. Destroy known seat paths
            var seat = transform.Find("SEAT/DriverSeat");
            if (seat) Destroy(seat.gameObject);

            var seatRoot = transform.Find("SEAT");
            if (seatRoot) Destroy(seatRoot.gameObject);

            // 3. Kill triggers
            foreach (var col in GetComponentsInChildren<Collider>(true))
            {
                if (col.isTrigger) col.enabled = false;
            }

            // 4. Hard-kill Rigidbody and drivetrain
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
            {
                Destroy(rb);
            }

            foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
            {
                string name = mb.GetType().Name.ToLower();
                if (name.Contains("carcontroller") || name.Contains("drivetrain") || name.Contains("input") || name.Contains("seat"))
                    mb.enabled = false;
            }
        }
    }

}
