/*
 * Whitecat Industries OrbitalDecay  for Kerbal Space Program. 
 * 
 * Written by Whitecat106 (Marcus Hehir).
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Whitecat Industries is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace WhitecatIndustries
{
    class ActiveVessel : MonoBehaviour
    {

        public static Vessel vessel;
        public static OrbitDriver driver;
        public static Orbit orbit;

        public static Vector3d decayVelVector;
        public static float EstimatedTimeUntilDeorbit;

        public static void DecayVector()
        {
            vessel = FlightGlobals.ActiveVessel;
            double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
            if (vessel.orbitDriver.orbit.PeA < MaxDecayInfluence)
            {
                double DecayValue = DecayController.DecayRate(vessel);
                double MaxDecayValue;
                orbit = vessel.orbitDriver.orbit;
                driver = vessel.orbitDriver;
                double BodyGravityConstant = vessel.orbitDriver.orbit.referenceBody.GeeASL;
                double AtmosphereMultiplier;
                double Lambda = 0.000000000133913 * UI.DifficultySetting;

                if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
                {
                    AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
                }
                else
                {
                    AtmosphereMultiplier = 0.5;
                }

                if (orbit.referenceBody.atmosphere)
                {
                    MaxDecayValue = ((vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.orbit.referenceBody.atmosphereDepth) * BodyGravityConstant * AtmosphereMultiplier * Lambda);
                    EstimatedTimeUntilDeorbit = ((float)(vessel.orbitDriver.orbit.semiMajorAxis - (float)vessel.orbitDriver.orbit.referenceBody.atmosphereDepth)) / (float)MaxDecayValue;
                }
                else
                {
                    MaxDecayValue = ((vessel.orbitDriver.orbit.referenceBody.Radius + 100) * BodyGravityConstant * AtmosphereMultiplier * Lambda);
                    EstimatedTimeUntilDeorbit = ((float)(vessel.orbitDriver.orbit.semiMajorAxis - (float)vessel.orbitDriver.orbit.referenceBody.atmosphereDepth)) / (float)MaxDecayValue;
                }
                if (VesselData.DecayTimes.ContainsKey(vessel.id))
                {
                    VesselData.DecayTimes.Remove(vessel.id);
                    VesselData.DecayTimes.Add(vessel.id, EstimatedTimeUntilDeorbit);
                }
                else
                {
                    VesselData.DecayTimes.Add(vessel.id, EstimatedTimeUntilDeorbit);
                }

                double DeltaS = DecayValue;
                decayVelVector = orbit.vel * ((DecayValue) / (447041.9058 / 10));
                vessel.ChangeWorldVelocity(-decayVelVector);
            }
        }
    }
}
