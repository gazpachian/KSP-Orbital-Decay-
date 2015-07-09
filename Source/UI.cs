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
    public class UI : MonoBehaviour
    {
        public static float DifficultySetting = 1.0f;

        private static int currentTab = 0;
        private static string[] tabs = { "Station Keeping", "Decay Estimates", "Settings" };
        private static Rect windowPosition = new Rect(0, 0, 480, 360);
        private static Color tabUnselectedColor = new Color(0.0f, 0.0f, 1.0f);
        private static Color tabSelectedColor = new Color(0.0f, 1.0f, 0.0f);
        private static Color tabUnselectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabSelectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private static GUIStyle windowStyle = null;
        private static GUIStyle windowStyleCenter = null;
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        private Texture2D buttonTexture = new Texture2D(1, 1);
        private GUIStyle ButtonStyle = null;
        private float UPTInterval = 1.0f;
        private float lastUpdate = 0.0f;
        private ApplicationLauncherButton ToolbarButton = null;
        private bool visible = false;
        private bool Filter1 = false;
        private bool Filter2 = true;

        public static UI Instance
        {
            get;
            private set;
        }

        UI()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private GUIStyle buttonStyle
        {
            get
            {
                if (ButtonStyle == null)
                {
                    ButtonStyle = new GUIStyle(skins.button);
                    ButtonStyle.onNormal = ButtonStyle.hover;
                }
                return ButtonStyle;
            }
        }

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIDestroyed);
            GameEvents.onGameSceneLoadRequested.Add(OnRequest);
        }

        void OnRequest(GameScenes SceneToLoad)
        {

        }

        void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER
                || HighLogic.LoadedScene == GameScenes.TRACKSTATION 
                || HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                windowStyle = new GUIStyle(HighLogic.Skin.window);
                windowStyleCenter = new GUIStyle(HighLogic.Skin.window);
                windowStyleCenter.alignment = TextAnchor.MiddleCenter;
                RenderingManager.AddToPostDrawQueue(0, OnDraw);
                OnGUIReady();
           }
        }

        void FixedUpdate()
        {
            if ((Time.time - lastUpdate) > UPTInterval)
            {
                lastUpdate = Time.time;
                foreach (Vessel vessel in FlightGlobals.Vessels)
                {
                    if (!vessel.name.Contains("Ast."))
                       // VesselData.CheckStationKeepAbility(vessel);
                        // VesselData.CheckStationKeeping(vessel);
                        VesselData.FormatDecayTimes(vessel);
                }
            }
        }

        void OnGUIReady()
        {
            if (ApplicationLauncher.Ready && this.ToolbarButton == null)
            {
                this.ToolbarButton = ApplicationLauncher.Instance.AddModApplication(onToggleOn, onToggleOff, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_off.png", true));
            }
        }

        void OnGUIDestroyed()
        {
            if (this.ToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.ToolbarButton);
                this.ToolbarButton = null;
            }
        }

        void onToggleOn()
        {
            this.ToolbarButton.SetTexture(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_on", true));
            this.visible = true;
        }

        void onToggleOff()
        {
            this.ToolbarButton.SetTexture(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_off", true));
            this.visible = false;
        }

        private void OnDraw()
        {
            if (this.visible)
            {
                windowPosition = GUILayout.Window(id, windowPosition, OnWindow, "Decay Manager", windowStyle);
            }
        }
        private void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIReady);
            GameEvents.onGameSceneLoadRequested.Add(OnRequest);
            if (this.ToolbarButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
        }

        private void OnWindow(int windowID)
        {
            if (GUI.Button(new Rect(windowPosition.width - 22, 3, 19, 19), "x"))
            {
                visible = false;
                if (ToolbarButton != null)
                    ToolbarButton.SetFalse();
            }
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < tabs.Length; ++i)
            {
                GUI.backgroundColor = currentTab == i ? tabSelectedColor : tabUnselectedColor;
                GUI.contentColor = currentTab == i ? new Color(1f, 1f, 1f) : new Color(1f, 1f, 1f); new Color(1f, 1f, 1f);
                if (GUILayout.Button(tabs[i]))
                {
                    currentTab = i;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            switch (currentTab)
            {
                case 0:
                    StationKeepingTab();
                    break;
                case 1:
                    DecayEstimatesTab();
                    break;
                case 2:
                    SettingsTab();
                    break;
                default:
                    break;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
            windowPosition.x = Mathf.Clamp(windowPosition.x, 0f, Screen.width - windowPosition.width);
            windowPosition.y = Mathf.Clamp(windowPosition.y, 0f, Screen.height - windowPosition.height);

        }

        private void StationKeepingTab()
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label("Station Keeping Menu", GUILayout.Width(240));
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vessel:");
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUILayout.Label("Toggle Station Keeping:");
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(5);
            Vessel vessel;

            for(int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                vessel = FlightGlobals.Vessels.ElementAt(i);
                bool CanStationKeep;
                bool LastStationKeepingValue;
                bool newValue = false;
                if (!VesselData.StationKeeping.ContainsKey(vessel.id))
                {
                    VesselData.StationKeeping.Add(vessel.id, false);
                }
                if (!vessel.name.Contains("Ast.") & VesselData.StationKeeping.ContainsKey(vessel.id))
                {
                    VesselData.CanStationKeep.TryGetValue(vessel.id, out CanStationKeep);

                    if (CanStationKeep == true)
                    {
                        if (HighLogic.LoadedSceneIsFlight)
                        {
                            if (vessel.id != FlightGlobals.ActiveVessel.id)
                            {

                                GUILayout.BeginVertical();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                GUI.skin.button.alignment = TextAnchor.MiddleRight;
                                VesselData.StationKeeping.TryGetValue(vessel.id, out LastStationKeepingValue);
                                if (GUILayout.Button(LastStationKeepingValue.ToString(), GUILayout.Width(50)))
                                {
                                    newValue = !LastStationKeepingValue;
                                    VesselData.StationKeeping.Remove(vessel.id);
                                    VesselData.StationKeeping.Add(vessel.id, newValue);
                                }
                                GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                                GUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                GUILayout.EndVertical();
                            }

                            else if (vessel.id == FlightGlobals.ActiveVessel.id)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                                GUI.skin.label.alignment = TextAnchor.MiddleRight;
                                GUILayout.Label("Vessel is active");
                                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                                GUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                GUILayout.EndVertical();
                            }
                        }
                        else
                        {
                            GUILayout.BeginVertical();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.skin.button.alignment = TextAnchor.MiddleRight;
                            VesselData.StationKeeping.TryGetValue(vessel.id, out LastStationKeepingValue);
                            if (GUILayout.Button(LastStationKeepingValue.ToString(),GUILayout.Width(50)))
                            {
                                newValue = !LastStationKeepingValue;
                                VesselData.StationKeeping.Remove(vessel.id);
                                VesselData.StationKeeping.Add(vessel.id, newValue);
                            }
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            GUILayout.EndVertical();
                        }
                    }

                    else if (CanStationKeep == false)
                    {
                        if (Filter1 == false)
                        {
                            
                            GUILayout.BeginVertical();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                            GUI.skin.label.alignment = TextAnchor.MiddleRight;
                            GUILayout.Label("Vessel cannot Station-Keep");
                            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            GUILayout.EndVertical();
                        }
                    }
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                     }
                }
            }

        private void DecayEstimatesTab()
        {
           // Vector2 scrollPosition = Vector2.zero;
            GUILayout.BeginHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label("Estimated Decay Times", GUILayout.Width(240));
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vessel:");
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUILayout.Label("Estimated Time Until Decay:");
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(5);
            //scrollPosition = GUI.BeginScrollView(new Rect(0,20,420,360), scrollPosition, new Rect(0, 20, 420, 360));
            string DecayEstimate = "";
            Vessel vessel;

            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                vessel = FlightGlobals.Vessels.ElementAt(i);
                if (!vessel.name.Contains("Ast.") & VesselData.DisplayedDecayTimes.ContainsKey(vessel) & (!vessel.name.Contains("Debris")))
                {

                    VesselData.DisplayedDecayTimes.TryGetValue(vessel, out DecayEstimate);
                      if (DecayEstimate != "Vessel is in a stable orbit")
                        {
                           GUILayout.BeginVertical();
                           VesselData.DisplayedDecayTimes.TryGetValue(vessel, out DecayEstimate);
                           GUILayout.BeginHorizontal();
                           GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                           GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                           GUI.skin.label.alignment = TextAnchor.MiddleRight;
                           GUILayout.Label(DecayEstimate);
                           GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                           GUILayout.EndHorizontal();
                           GUILayout.Space(5);
                           GUILayout.EndVertical();
                            }
                        }
                        else
                        {
                            if (Filter2 == false && DecayEstimate != "")
                            {

                                GUILayout.BeginVertical();
                                VesselData.DisplayedDecayTimes.TryGetValue(vessel, out DecayEstimate);
                                GUILayout.BeginHorizontal();
                                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                                GUILayout.Label(vessel.name.TrimEnd("(unloaded)".ToCharArray()));
                                GUI.skin.label.alignment = TextAnchor.MiddleRight;
                                GUILayout.Label(DecayEstimate);
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                GUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                GUILayout.EndVertical();
                            }
                       }
               
            }
           // GUI.EndScrollView();
        }


        private void SettingsTab()
        {
            GUILayout.BeginVertical();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.toggle.alignment = TextAnchor.MiddleLeft;
            Filter1 = GUILayout.Toggle(Filter1, "Station Keeping filter: Hide Vessels unable to Station-Keep");
            GUILayout.Space(5);
            Filter2 = GUILayout.Toggle(Filter2, "Decay Times filter: Hide Vessels in stable orbits");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Decay Rate Multiplier (0.1 - 100.0, Default = 1.0):");
            DifficultySetting = GUILayout.HorizontalSlider(DifficultySetting, 0.1f, 100.0f);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Decay Rate Mutlitplier: " + DifficultySetting.ToString("F0"));
            GUILayout.EndVertical();
        }
    }
}


