using System;
using UnityEngine;

namespace RémiMod
{
    public class VesselUpdate
    {
        public double universeTime;
        public Guid vesselID;
        public string planet;
        public double[] orbit;
        public double[] position;
        public double[] velocity;
        public float[] rotation;

        public void Apply()
        {
            //Find vessel and body
            Vessel v = FlightGlobals.Vessels.Find(findVessel => findVessel.id == vesselID);
            if (v == null)
            {
                Debug.Log("Vessel not found");
                return;
            }
            CelestialBody cb = FlightGlobals.Bodies.Find(findBody => findBody.name == planet);
            if (cb == null)
            {
                Debug.Log("Body not found");
                return;
            }

            //Set orbit & position
            if (orbit != null)
            {
                v.orbitDriver.orbit.SetOrbit(orbit[0], orbit[1], orbit[2], orbit[3], orbit[4], orbit[5], orbit[6], cb);
            }
            if (position != null && velocity != null)
            {
                Vector3d pos = new Vector3d(position[0], position[1], position[2]);
                Vector3d vel = new Vector3d(velocity[0], velocity[1], velocity[2]);
                v.orbitDriver.orbit.UpdateFromFixedVectors(pos, vel, cb, universeTime);
            }
            v.orbitDriver.orbit.UpdateFromOrbitAtUT(v.orbitDriver.orbit, Planetarium.GetUniversalTime(), cb);

            //Set rotation
            Quaternion surfaceRotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            v.SetRotation(v.mainBody.bodyTransform.rotation * surfaceRotation);
            v.srfRelRotation = surfaceRotation;

            //Update fields that KSP uses elsewhere
            v.precalc.CalculatePhysicsStats();
            v.latitude = cb.GetLatitude(cb.position + v.orbitDriver.pos);
            v.longitude = cb.GetLongitude(cb.position + v.orbitDriver.pos);
            v.altitude = v.orbitDriver.orbit.altitude;
        }
    }
}
