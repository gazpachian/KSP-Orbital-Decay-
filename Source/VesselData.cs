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
    class VesselData : MonoBehaviour
    {
        public static bool DisplayedMessage;
        public static Dictionary<Guid, bool> CanStationKeep = new Dictionary<Guid, bool>();
        public static Dictionary<Guid, bool> Message = new Dictionary<Guid, bool>();
        public static Dictionary<Guid, bool> StationKeeping = new Dictionary<Guid, bool>();
        public static Dictionary<Guid, float> DecayTimes = new Dictionary<Guid, float>();
        public static Dictionary<Vessel, string> DisplayedDecayTimes = new Dictionary<Vessel, string>();

        public string FilePath = KSPUtil.ApplicationRootPath + "GameData/WhitecatIndustries/OrbitalDecay/VesselData.cfg";

        ConfigNode data = new ConfigNode();

        private float UPTInterval = 1.0f;
        private float lastUpdate = 0.0f;

        public void FixedUpdate()
        {
            if ((Time.time - lastUpdate) > UPTInterval)
            {
                lastUpdate = Time.time;

                if (HighLogic.LoadedScene.Equals(GameScenes.MAINMENU) || (HighLogic.LoadedScene.Equals(GameScenes.LOADINGBUFFER)) || (HighLogic.LoadedScene.Equals(GameScenes.LOADING)))
                {
                }

                else
                {
                    for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                    {
                        Vessel vessel = FlightGlobals.Vessels.ElementAt(i);
                        CheckStationKeepingAbility(vessel);
                    }
                    UpdateLists();
                }
            }
        }


        public void Start()
        {
        }

        public void Save()
        {
             if (HighLogic.LoadedScene.Equals(GameScenes.MAINMENU) || (HighLogic.LoadedScene.Equals(GameScenes.LOADINGBUFFER)) || (HighLogic.LoadedScene.Equals(GameScenes.LOADING)))
                {
                }

                else
                {
                    SaveData();                        
                }
        }

        public void Load()
        {
              if (HighLogic.LoadedScene.Equals(GameScenes.MAINMENU) || (HighLogic.LoadedScene.Equals(GameScenes.LOADINGBUFFER)) || (HighLogic.LoadedScene.Equals(GameScenes.LOADING)))
                {
                }

                else
                {
                    LoadData();                       
                }
        }

        public void SaveData()
        {
            data = ConfigNode.Load(FilePath);
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++) 
            {
                Vessel ship = FlightGlobals.Vessels.ElementAt(i);
                bool StatKeep;
                float LifeTime;
                bool vesselfound = false;

                StationKeeping.TryGetValue(ship.id, out StatKeep);
                DecayTimes.TryGetValue(ship.id, out LifeTime);

                if (!ship.name.Contains("Ast."))
                {
                    foreach (ConfigNode vessel in data.nodes)
                    {
                        if (vessel.GetValue("id") == ship.id.ToString())
                        {
                            vessel.SetValue("stationkeeping", StatKeep.ToString());
                            vessel.SetValue("decaytime", LifeTime.ToString());
                            vesselfound = true;
                        }
                    }
                    if (vesselfound == false)
                    {
                        ConfigNode vessel = data.AddNode("VESSEL");
                        vessel.AddValue("id", ship.id.ToString());
                        vessel.AddValue("stationkeeping", StatKeep.ToString());
                        vessel.AddValue("decaytime", LifeTime.ToString());
                    }
                }
                data.Save(FilePath);
            }
        }

        public void LoadData()
        {
            data = ConfigNode.Load(FilePath);
            bool StationKeepingfound = false;
            bool DecayTimefound = false;

            foreach (ConfigNode vessel in data.nodes)
            {
                string id = vessel.GetValue("id");
                bool stationkeeping = bool.Parse(vessel.GetValue("stationkeeping"));
                float decaytime = float.Parse(vessel.GetValue("decaytime"));
                bool vesseldead = false;
                foreach (Vessel vess in FlightGlobals.Vessels)
                {
                    if (vess.id.ToString() == id)
                    {
                        if (vess.state == Vessel.State.DEAD)
                        {
                            data.RemoveNode(vessel);
                            StationKeeping.Remove(vess.id);
                            CanStationKeep.Remove(vess.id);
                            DecayTimes.Remove(vess.id);
                            DisplayedDecayTimes.Remove(vess);
                            vesseldead = true;
                            break;
                        }
                    }
                }
                if (vesseldead == true)
                {
                    break;
                }

                foreach (Guid ship in StationKeeping.Keys)
                {
                    if (ship.ToString() == id)
                    {
                        StationKeeping.Remove(ship);
                        StationKeeping.Add(ship, stationkeeping);
                        StationKeepingfound = true;
                        break;
                    }

                    if (StationKeepingfound == false)
                    {
                    }
                }
                foreach (Guid ship in DecayTimes.Keys)
                {
                    if (ship.ToString() == id)
                    {
                        DecayTimes.Remove(ship);
                        DecayTimes.Add(ship, decaytime);
                        DecayTimefound = true;
                        break;
                    }


                    if (DecayTimefound == false)
                    {
                    }
                }
            }
        }

        public void UpdateLists()
        {
            data = ConfigNode.Load(FilePath);
            if (data.nodes.Count > 0)
            {
                SaveData();
                LoadData();
            }

            else
            {
                NewSave();
            } 
        }

        public void NewSave()
        {
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel ship = FlightGlobals.Vessels.ElementAt(i);
                bool StatKeep;
                float LifeTime;

                StationKeeping.TryGetValue(ship.id, out StatKeep);
                DecayTimes.TryGetValue(ship.id, out LifeTime);

                if (!ship.name.Contains("Ast."))
                {
                    ConfigNode vessel = data.AddNode("VESSEL");
                    vessel.AddValue("id", ship.id);
                    vessel.AddValue("stationkeeping", StatKeep.ToString());
                    vessel.AddValue("decaytime", LifeTime.ToString());
                }
            }
            data.Save(FilePath);
        }

        public void CheckStationKeepingAbility(Vessel vessel)
        {
            CanStationKeep.Remove(vessel.id);
            CanStationKeep.Add(vessel.id, true);

           foreach (Part part in vessel.parts)
            {
                if (part.FindModuleImplementing<StationKeepingModule>())
                {
                    CanStationKeep.Remove(vessel.id);
                    CanStationKeep.Add(vessel.id, true);
                    break;
                }
            }

        }

        public static void FormatDecayTimes(Vessel vessel)
        {
            float time;
            bool stationkeeping = false;

            if (DisplayedDecayTimes.ContainsKey(vessel))
            {
                DisplayedDecayTimes.Remove(vessel);
            }

            if (StationKeeping.ContainsKey(vessel.id))
            {
                StationKeeping.TryGetValue(vessel.id, out stationkeeping);
            }

            DecayTimes.TryGetValue(vessel.id, out time);

            if (time < 1.0f)
            {
                DisplayedDecayTimes.Remove(vessel);
                DisplayedDecayTimes.Add(vessel, "Vessel is in a stable orbit");
            }
            if (stationkeeping == true)
            {
                DisplayedDecayTimes.Remove(vessel);
                DisplayedDecayTimes.Add(vessel, "Vessel is Station-Keeping in a stable orbit");
            }
            else if (time > 1 && stationkeeping == false)
            {
                // Seconds per kerbin year =  426.08 * 6 * 60 * 60 = 9,203,328, per day = 21600
                float _years;
                float _days;
                float _hours;
                string estTime;
                if (!vessel.isActiveVessel)
                {
                    time = time / 34 * (float)(vessel.orbitDriver.orbit.altitude / 550000); // Correction Factor
                }
                else
                {
                    time = time / 34 * (float)((vessel.orbitDriver.orbit.altitude / 2500) / 210.75); // Correction Factor
                }
                _years = time / 9203328;
                float totalDays = _years * 426.08f;
                float totalSeconds = totalDays * 21600f;
                _days = _years * totalDays;
                _hours = _days * 6;

                if (_years > 1)
                {
                    estTime = _years.ToString("F0") + " years.";
                }

                else
                {
                    if (_days > 1)
                    {
                        estTime = _days.ToString("F0") + " days.";
                    }

                    else
                    {
                        estTime = _hours.ToString("F0") + " hours.";
                    }
                }
                DisplayedDecayTimes.Add(vessel, estTime);

            }
        }
    }


}
