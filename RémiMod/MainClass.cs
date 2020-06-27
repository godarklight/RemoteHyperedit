using System;
using System.Collections.Generic;
using UnityEngine;

namespace RémiMod
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class MainClass : MonoBehaviour
    {
        TcpServer tcps = new TcpServer(9999);
        private Dictionary<Guid, Queue<VesselUpdate>> vesselUpdates = new Dictionary<Guid, Queue<VesselUpdate>>();

        public void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
            {
                tcps.Pump(vesselUpdates);
            }
        }
    }
}
