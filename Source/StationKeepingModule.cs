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
   [KSPModule("StationKeepingModule")]
   public class StationKeepingModule : PartModule
    {
        bool StationKeeping = false;

        public override void OnFixedUpdate()
        {

            if (VesselData.StationKeeping.ContainsKey(this.vessel.id))
            {
                if (CheckCanStationKeep() == true)
                {
                    VesselData.StationKeeping.Remove(this.vessel.id);
                    VesselData.StationKeeping.Add(this.vessel.id, StationKeeping);
                }
                else
                {
                    VesselData.StationKeeping.Remove(this.vessel.id);
                    VesselData.StationKeeping.Add(this.vessel.id, false);
                }
            }

            if (!VesselData.StationKeeping.ContainsKey(this.vessel.id))
            {
                if (CheckCanStationKeep() == true)
                {
                    VesselData.StationKeeping.Add(this.vessel.id, StationKeeping);
                }

                else
                {
                    VesselData.StationKeeping.Add(this.vessel.id, false);
                }    
            }

            base.OnFixedUpdate();
        }

        public override string GetInfo()
        {
            return "\nContains a Whitecat Industries Orbital Stationkeeping Module.\n";
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Station-Keeping", active = true)]
        public void ToggleStationKeeping()
        {
            if (CheckCanStationKeep() == true)
            {
                StationKeeping = !StationKeeping;
                if (StationKeeping == true)
                {
                    ScreenMessages.PostScreenMessage("Station-Keeping Enabled", 1.0f, ScreenMessageStyle.UPPER_LEFT);
                    VesselData.StationKeeping.Remove(vessel.id);
                    VesselData.StationKeeping.Add(vessel.id, true);
                }
                else
                {
                    ScreenMessages.PostScreenMessage("Station-Keeping Disabled", 1.0f, ScreenMessageStyle.UPPER_LEFT);
                    VesselData.StationKeeping.Remove(vessel.id);
                    VesselData.StationKeeping.Add(vessel.id, false);
                }
            }
            else
            {
                StationKeeping = false;
                ScreenMessages.PostScreenMessage("No RCS fuel remaining: Cannot enable Station-Keeping", 1.0f, ScreenMessageStyle.UPPER_LEFT);
                VesselData.StationKeeping.Remove(vessel.id);
                VesselData.StationKeeping.Add(vessel.id, false);
            }
        }

        public bool CheckCanStationKeep()
        {
            double RequiredResource = DecayController.DecayRate(this.vessel);
            // Eventual resource handling. 

            return true;
        }

    }
}
