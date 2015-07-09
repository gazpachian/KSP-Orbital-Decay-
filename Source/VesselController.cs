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
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class VesselController : MonoBehaviour
    {
        private float UPTInterval = 1f;  
        private float lastUpdate = 0.0f;

        private Vessel vessel;
        private bool CanStationkeep;
        private bool StationKeeping;

        public void FixedUpdate()
        {
            if ((Time.time - lastUpdate) > UPTInterval && HighLogic.LoadedSceneIsGame)
            {
                lastUpdate = Time.time;

                for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                {
                    vessel = FlightGlobals.Vessels.ElementAt(i);
                    if (vessel.situation == Vessel.Situations.ORBITING)
                    {
                        VesselData.CanStationKeep.TryGetValue(vessel.id, out CanStationkeep);
                        VesselData.StationKeeping.TryGetValue(vessel.id, out StationKeeping);

                        if (CanStationkeep == true)
                        {

                            if (StationKeeping == true)
                            {
                                DecayController.StationKeeping(vessel.orbitDriver, vessel);
                            }

                            else
                            {
                                if (vessel.isActiveVessel)
                                {
                                    if (TimeWarp.CurrentRate == 1)
                                    {
                                        //print("Adding decay vector to: " + vessel.name);
                                        ActiveVessel.DecayVector();
                                    }

                                    else
                                    {
                                        DecayController.CalculateOrbit(vessel, vessel.orbitDriver.orbit, vessel.orbitDriver);
                                    }
                                }

                                else
                                {
                                    DecayController.CalculateOrbit(vessel, vessel.orbitDriver.orbit, vessel.orbitDriver);
                                }
                            }
                        }
                        if (CanStationkeep == false)
                        {
                            if (vessel.isActiveVessel)
                            {
                                if (TimeWarp.CurrentRate == 1)
                                {
                                    ActiveVessel.DecayVector();
                                }

                                else
                                {
                                    DecayController.CalculateOrbit(vessel, vessel.orbitDriver.orbit, vessel.orbitDriver);
                                }
                            }

                            else
                            {
                                DecayController.CalculateOrbit(vessel, vessel.orbitDriver.orbit, vessel.orbitDriver);
                            }
                        }
                    }
                }
            }
        }
    }
  }
