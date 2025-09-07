using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MSCLoader;

namespace GhostCar
{
    public class EventListener : MonoBehaviour
    {

        /*
        * 
        * Protected
        * 
        */



        /*
        * 
        * Private
        * 
        */

        private PlayMakerFSM listenFSM = null;
        private string startState;
        private string stopState;
        private string lastState;
        private List<string> stageStates= new List<string>();

        private Recorder Recorder = null;

        /*
        * 
        * Public
        * 
        */

        

        /*
        * 
        * Methods
        * 
        */

        void Awake()
        {

            if (!Validate())
            {
                Destroy(this);
                throw new System.Exception("EventListener missing start or stop state.");
            }

        }

        void OnEnable()
        {
            Start();
        }

        void OnDisable()
        {
            Stop();
        }

        void OnDestroy()
        {

        }

        public void Start()
        {

            if (!Validate())
            {
                Destroy(this);
            }

        }

        public void Stop()
        {



        }

        public void Update()
        {

            string currentState = listenFSM.ActiveStateName;

            // Avoid repeating the same state
            if (currentState == lastState) return;

            lastState = currentState;

            //ModConsole.Print($"FSM changed to state: {currentState}");

            if (currentState == startState)
            {
                ModConsole.Print(">>> EVENT: START");
                ModConsole.Print($"FSM changed to state: {currentState}");

                ModConsole.Print($"Rally Type: {this.name}");

                //Recorder = this.gameObject.AddComponent<Recorder>();
                GameObject currentVehicle = Helper.GetVehicle();

                if (currentVehicle != null)
                {
                    Recorder = currentVehicle.AddComponent<Recorder>();
                    var Type = Recorder.Type.Free;

                    switch(this.name)
                    {
                        case "TimingSaturday":
                            Type = Recorder.Type.Rally1;
                            break;
                        case "TimingSunday":
                            Type = Recorder.Type.Rally2;
                            break;
                        case "DragTiming":
                            Type = Recorder.Type.Drag;
                            break;
                    }

                    Recorder.type = Type;
                }
            }
            else if (currentState == stopState)
            {
                ModConsole.Print(">>> EVENT: STOP");
                ModConsole.Print($"FSM changed to state: {currentState}");
                Recorder.Stop();
            }
            else if (stageStates.Contains(currentState))
            {
                ModConsole.Print($">>> EVENT: CHECKPOINT: {currentState}");
                ModConsole.Print($"FSM changed to state: {currentState}");
                Recorder.saveStage();
            }

        }

        public void listenTo(string name)
        {
            this.listenFSM = this.GetComponents<PlayMakerFSM>()
                .FirstOrDefault(fsm => fsm.FsmName == name);
        }

        public void setStart(string start)
        {
            this.startState = start;
        }

        public void setStop(string stop)
        {
            this.stopState = stop;
        }

        public void setCheckpoints(List<string> list)
        {
            stageStates.Clear();
            stageStates.AddRange(list);
        }

        public void init(string listen, string start, string stop, List<string> checkpoints = null)
        {

            this.listenTo(listen);
            this.setStart(start);
            this.setStop(stop);

            if (checkpoints != null && checkpoints.Count() > 0)
            {
                this.setCheckpoints(checkpoints);
            }

        }

        private bool Validate()
        {
            if (listenFSM == null)
            {
                ModConsole.Error("You must provide a component to listen for using init()");
                return false;
            }

            if (startState == null)
            {
                ModConsole.Error("You must set a start state using init()");
                return false;
            }

            if (stopState == null)
            {
                ModConsole.Error("You must set a stop state using init()");
                return false;
            }

            return true;
        }


    }
}
