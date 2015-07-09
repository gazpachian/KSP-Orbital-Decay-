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
    public class DecayController : MonoBehaviour
    {
        public static double DecayValue;
        public static double MaxDecayValue;
        public static bool VesselDied = false;
        public static float EstimatedTimeUntilDeorbit;

        public static void CalculateOrbit(Vessel vessel, Orbit oldOrbit, OrbitDriver driver)
        {
            VesselDied = false;

            if (!VesselData.Message.ContainsKey(vessel.id))
            {
                VesselData.Message.Add(vessel.id, false);
                VesselData.DisplayedMessage = false;
            }
            else
            {
                VesselData.Message.TryGetValue(vessel.id, out VesselData.DisplayedMessage);
            }

            vessel.GoOnRails();

            var oldBody = vessel.orbitDriver.orbit.referenceBody;
           
           // The ISS decays at about 2km/month = 2000/30*24*60*60 == 7.7x10^-4 m/s 

            double BodyGravityConstant = vessel.orbitDriver.orbit.referenceBody.GeeASL;
            double AtmosphereMultiplier;
            double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
            if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
            {
                AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
            }
            else
            {
                 AtmosphereMultiplier = 0.5;
            }

            if (vessel.orbitDriver.orbit.semiMajorAxis + 50 < MaxDecayInfluence )
            {
                double Lambda = 0.000000000133913 * UI.DifficultySetting;                           //0.000000000133913;
                double Sigma = MaxDecayInfluence - vessel.orbitDriver.orbit.altitude;
                DecayValue = (double)TimeWarp.CurrentRate * Sigma * BodyGravityConstant * AtmosphereMultiplier * Lambda;

               
                if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
                {
                    if (vessel.orbitDriver.orbit.PeA < vessel.orbitDriver.orbit.referenceBody.atmosphereDepth)
                    {
                        DecayValue = DecayValue * (Math.Pow(Math.E, vessel.orbitDriver.orbit.referenceBody.atmosphereDepth - vessel.orbitDriver.orbit.PeA)); // Have it increase alot more as we enter the hard atmosphere
                    }

                    //EstimatedTimeUntilDeorbit = (float)(vessel.orbitDriver.orbit.altitude - (float)vessel.orbitDriver.orbit.referenceBody.atmosphereDepth) / (float)DecayValue;
                     MaxDecayValue = ((vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.orbit.referenceBody.atmosphereDepth) * BodyGravityConstant * AtmosphereMultiplier * Lambda);
                     EstimatedTimeUntilDeorbit = ((float)(vessel.orbitDriver.orbit.semiMajorAxis - (float)vessel.orbitDriver.orbit.referenceBody.atmosphereDepth)) / (float)MaxDecayValue; 
                   // EstimatedTimeUntilDeorbit = (float)(vessel.orbitDriver.orbit.PeA - (((vessel.orbitDriver.referenceBody.atmosphereDepth)/((MaxDecayValue+DecayValue)/2) - vessel.orbitDriver.orbit.PeA)) *((MaxDecayValue + DecayValue) / 2));
                }
                else
                {
                   //EstimatedTimeUntilDeorbit = (float)(vessel.orbitDriver.orbit.altitude - 100) / (float)DecayValue;
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

               // print(vessel.name + " Semi major axis reduced by: " + (DecayValue));
            }
            else
            {
                DecayValue = 0;

                if (VesselData.DecayTimes.ContainsKey(vessel.id))
                {
                    VesselData.DecayTimes.Remove(vessel.id);
                    VesselData.DecayTimes.Add(vessel.id, 0.5f);
                }
                else
                {
                    VesselData.DecayTimes.Add(vessel.id, 0.5f);
                }
            }
    
            if (VesselDied == false)         // Just Incase the vessel is destroyed part way though the check.
            {
                if (vessel.orbitDriver.orbit.referenceBody.GetInstanceID() != 0 || vessel.orbitDriver.orbit.semiMajorAxis > vessel.orbitDriver.orbit.referenceBody.Radius + 5)
                {
                    SetNewOrbit(driver, oldOrbit);
                }
            }

            if (vessel.orbitDriver.orbit.referenceBody.atmosphere) // Big problem ( Jool, Eve, Duna, Kerbin, Laythe)
            {
                if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.referenceBody.atmosphereDepth + 500)
                {
                    FlightDriver.SetPause(true);
                    TimeWarp.SetRate(1, true);
                    FlightDriver.SetPause(false);
                    print("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s hard atmosphere");
                    ScreenMessages.PostScreenMessage("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s hard atmosphere");
                    VesselData.Message.Remove(vessel.id);
                    VesselData.Message.Add(vessel.id, true);
                }

                if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.referenceBody.atmosphereDepth + 5)
                {
                    VesselDied = true;       
                }
            }
            else // Moon Smaller Problem
            {
                if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + 5000)
                {
                    FlightDriver.SetPause(true);
                    TimeWarp.SetRate(1, true);
                    FlightDriver.SetPause(false);
                    print("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s surface");
                    ScreenMessages.PostScreenMessage("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s surface");
                    VesselData.Message.Remove(vessel.id);
                    VesselData.Message.Add(vessel.id, true);
                }

                if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + 100)
                {
                    VesselDied = true; 
                }

            }

            if (VesselDied == false)
            {
                vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
                vessel.orbitDriver.vel = vessel.orbit.vel;
                var newBody = vessel.orbitDriver.orbit.referenceBody;
                if (newBody != oldBody)
                {
                    var evnt = new GameEvents.HostedFromToAction<Vessel, CelestialBody>(vessel, oldBody, newBody);
                    GameEvents.onVesselSOIChanged.Fire(evnt);
                }
            }

            if (VesselDied == true)
            {
                vessel.Die();
                VesselData.CanStationKeep.Remove(vessel.id);
                VesselData.DecayTimes.Remove(vessel.id);
                VesselData.DisplayedDecayTimes.Remove(vessel);
                VesselData.StationKeeping.Remove(vessel.id);

            }
        }

        public static void SetNewOrbit(OrbitDriver driver, Orbit oldOrbit)
        {
                var orbit = driver.orbit;
                orbit.inclination = driver.orbit.inclination;
                orbit.eccentricity = driver.orbit.eccentricity;
                orbit.semiMajorAxis = (driver.orbit.semiMajorAxis - DecayValue);
                orbit.LAN = driver.orbit.LAN;
                orbit.argumentOfPeriapsis = driver.orbit.argumentOfPeriapsis;
                orbit.meanAnomalyAtEpoch = driver.orbit.meanAnomalyAtEpoch;
                orbit.epoch = driver.orbit.epoch;
                orbit.referenceBody = driver.orbit.referenceBody;
                orbit.Init();
                orbit.UpdateFromUT(Planetarium.GetUniversalTime());
                if (orbit.referenceBody != driver.orbit.referenceBody)
                {
                    if (driver.OnReferenceBodyChange != null)
                        driver.OnReferenceBodyChange(driver.orbit.referenceBody);
                }
            
        }


        public static double DecayRate(Vessel vessel)
        {
            double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
            double AtmosphereMultiplier;

            if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
            {
                AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
            }
            else
            {
                AtmosphereMultiplier = 0.5;
            }

            if (vessel.orbitDriver.orbit.semiMajorAxis + 50 < MaxDecayInfluence)
            {
                double Lambda = 0.000000000133913 * UI.DifficultySetting;
                double Sigma = MaxDecayInfluence - vessel.orbitDriver.orbit.altitude;
                DecayValue = (double)TimeWarp.CurrentRate * Sigma * vessel.orbitDriver.orbit.referenceBody.GeeASL * AtmosphereMultiplier * Lambda;
            }
            else
            {
                DecayValue = 0;
            }
            return DecayValue;
        }



        public static void StationKeeping(OrbitDriver driver, Vessel vessel)
        {


            // Background resource processing

            bool ResourceFound = true;

            double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
            double AtmosphereMultiplier;

            if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
            {
                AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
            }
            else
            {
                AtmosphereMultiplier = 0.5;
            }

            if (vessel.orbitDriver.orbit.semiMajorAxis + 50 < MaxDecayInfluence)
            {
                double Lambda = 0.000000000133913 * UI.DifficultySetting;
                double Sigma = MaxDecayInfluence - vessel.orbitDriver.orbit.altitude;
                DecayValue = (double)TimeWarp.CurrentRate * Sigma * vessel.orbitDriver.orbit.referenceBody.GeeASL * AtmosphereMultiplier * Lambda;
            }
            else
            {
                DecayValue = 0;
            }

            foreach (Part part in vessel.Parts)
            {
                if (part.Resources.Count != 0)
                {
                    foreach (PartResource resource in part.Resources)
                    {
                        if (resource.name == "ElectricCharge" && resource.amount > DecayValue)
                        {
                            ResourceFound = true;
                            part.RequestResource("ElectricCharge", DecayValue);
                            VesselData.CanStationKeep.Remove(vessel.id);
                            VesselData.CanStationKeep.Add(vessel.id, true);
                            break;
                            
                        }
                    }
                }
            }

                    if (ResourceFound == false)
                    {
                        ScreenMessages.PostScreenMessage("Warning: " + vessel.name + " has run out of Mono-Propellant, stationkeeping stopped");
                        VesselData.CanStationKeep.Remove(vessel.id);
                        VesselData.CanStationKeep.Add(vessel.id, false);
                    }
                }
            }
        } 

