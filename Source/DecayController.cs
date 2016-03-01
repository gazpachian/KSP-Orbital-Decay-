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
		public const double lambda = 0.000000000133913;
		public static double decayValue;
		public static double maxDecayValue;
		public static bool vesselDied = false;
		public static float estTimeToDeorbit;

		public static void CalculateOrbit (Vessel vessel, Orbit oldOrbit, OrbitDriver driver)
		{
			vesselDied = false;

			if (!VesselData.Message.ContainsKey (vessel.id)) {
				VesselData.Message.Add (vessel.id, false);
				VesselData.DisplayedMessage = false;
			} else {
				VesselData.Message.TryGetValue (vessel.id, out VesselData.DisplayedMessage);
			}
			var oldBody = oldOrbit.referenceBody;
			var parentG_ASL = oldBody.GeeASL;
			var atmoMult = oldBody.atmosphere ? oldBody.atmospherePressureSeaLevel / 101.325 : 0.5;
			var atmoDepth = oldBody.atmosphere ? oldBody.atmosphereDepth : 100; 
			var maxDecayInfluence = oldBody.Radius * 10.0;

			vessel.GoOnRails ();

			var currentOrbit = vessel.orbitDriver.orbit;
			// The ISS decays at about 2km/month = 2000/30*24*60*60 == 7.7x10^-4 m/s
			// The default lambda is scaled to achieve numbers similar to this. Maybe.
			if (oldOrbit.semiMajorAxis + 50 < maxDecayInfluence) { // TODO : use the method decayRate instead, right? Why have to copies of basically the same code?
				double scaledLambda = lambda * UI.DifficultySetting;
				double sigma = maxDecayInfluence - currentOrbit.altitude;
				decayValue = (double)TimeWarp.CurrentRate * sigma * parentG_ASL * atmoMult * scaledLambda;

				if (oldOrbit.PeA < atmoDepth) { //TODO : Check the math here
					decayValue = decayValue * (Math.Pow (Math.E, atmoDepth - oldOrbit.PeA)); // Have it increase alot more as we enter the hard atmosphere
				}
				maxDecayValue = ((oldBody.Radius + atmoDepth) * parentG_ASL * atmoMult * scaledLambda);
				// TODO : This should be fixed to do some integral calculation, but maybe leave it until the exponential falloff of drag has been implemented
				estTimeToDeorbit = ((float)(oldOrbit.semiMajorAxis - (float)oldBody.atmosphereDepth)) / (float)maxDecayValue; 
				if (VesselData.DecayTimes.ContainsKey (vessel.id)) {
					VesselData.DecayTimes.Remove (vessel.id);
				}
				VesselData.DecayTimes.Add (vessel.id, estTimeToDeorbit);
			} else {
				decayValue = 0;

				if (VesselData.DecayTimes.ContainsKey (vessel.id)) {
					VesselData.DecayTimes.Remove (vessel.id);
				}
				VesselData.DecayTimes.Add (vessel.id, 0.5f);
			}
    
			if (vessel.orbitDriver.orbit.referenceBody.GetInstanceID () != 0 || oldOrbit.semiMajorAxis > oldBody.Radius + 5) {
				SetNewOrbit (driver, decayValue);
			}

			if (vessel.orbitDriver.orbit.referenceBody.atmosphere) { // Big problem ( Jool, Eve, Duna, Kerbin, Laythe)
				if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.referenceBody.atmosphereDepth + 500) {
					FlightDriver.SetPause (true);
					TimeWarp.SetRate (1, true);
					FlightDriver.SetPause (false);
					print ("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s hard atmosphere");
					ScreenMessages.PostScreenMessage ("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s hard atmosphere");
					VesselData.Message.Remove (vessel.id);
					VesselData.Message.Add (vessel.id, true);
				}

			} else { // Moon Smaller Problem
				if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + 5000) {
					FlightDriver.SetPause (true);
					TimeWarp.SetRate (1, true);
					FlightDriver.SetPause (false);
					print ("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s surface");
					ScreenMessages.PostScreenMessage ("Warning: " + vessel.name + " is approaching " + oldOrbit.referenceBody.name + "'s surface");
					VesselData.Message.Remove (vessel.id);
					VesselData.Message.Add (vessel.id, true);
				}
			}
			if (vessel.orbitDriver.orbit.semiMajorAxis < vessel.orbitDriver.orbit.referenceBody.Radius + 100) {
				vesselDied = true; 
			}

			if (vesselDied == false) {
				vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
				vessel.orbitDriver.vel = vessel.orbit.vel;
				var newBody = vessel.orbitDriver.orbit.referenceBody;
				if (newBody != oldBody) {
					var e = new GameEvents.HostedFromToAction<Vessel, CelestialBody> (vessel, oldBody, newBody);
					GameEvents.onVesselSOIChanged.Fire (e);
				}
			} else {
				vessel.Die ();
				print ("Notice: " + vessel.name + " was destroyed on re-entry above " + oldOrbit.referenceBody.name + ".");
				ScreenMessages.PostScreenMessage ("Notice: " + vessel.name + " was destroyed on re-entry above " + oldOrbit.referenceBody.name + ".");
				VesselData.CanStationKeep.Remove (vessel.id);
				VesselData.DecayTimes.Remove (vessel.id);
				VesselData.DisplayedDecayTimes.Remove (vessel);
				VesselData.StationKeeping.Remove (vessel.id);

			}
		}

		public static void SetNewOrbit (OrbitDriver driver, double decayValue)
		{
			var orbit = driver.orbit;
			orbit.semiMajorAxis -= decayValue;
			orbit.Init ();
			orbit.UpdateFromUT (Planetarium.GetUniversalTime ());
			if (orbit.referenceBody != driver.orbit.referenceBody) {
				if (driver.OnReferenceBodyChange != null)
					driver.OnReferenceBodyChange (driver.orbit.referenceBody);
			}
            
		}

		public static double decayRate (Orbit orbit)
		{
			var parent = orbit.referenceBody;
			double maxDecayInfluence = parent.Radius * 10;
			double atmoMult = parent.atmosphere ? parent.atmospherePressureSeaLevel / 101.325 : 0.5;
			double sigma = maxDecayInfluence - orbit.altitude;
			return orbit.semiMajorAxis + 50 < maxDecayInfluence ? (double)TimeWarp.CurrentRate * sigma * parent.GeeASL * atmoMult * lambda : 0.0;
		}

		public static void StationKeeping (OrbitDriver driver, Vessel vessel)
		{


			// Background resource processing

			bool ResourceFound = true;

			double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
			double AtmosphereMultiplier;

			if (vessel.orbitDriver.orbit.referenceBody.atmosphere) {
				AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
			} else {
				AtmosphereMultiplier = 0.5;
			}

			if (vessel.orbitDriver.orbit.semiMajorAxis + 50 < MaxDecayInfluence) {
				double Lambda = 0.000000000133913 * UI.DifficultySetting;
				double Sigma = MaxDecayInfluence - vessel.orbitDriver.orbit.altitude;
				decayValue = (double)TimeWarp.CurrentRate * Sigma * vessel.orbitDriver.orbit.referenceBody.GeeASL * AtmosphereMultiplier * Lambda;
			} else {
				decayValue = 0;
			}

			foreach (Part part in vessel.Parts) {
				if (part.Resources.Count != 0) {
					foreach (PartResource resource in part.Resources) {
						if (resource.name == "ElectricCharge" && resource.amount > decayValue) {
							ResourceFound = true;
							part.RequestResource ("ElectricCharge", decayValue);
							VesselData.CanStationKeep.Remove (vessel.id);
							VesselData.CanStationKeep.Add (vessel.id, true);
							break;
                            
						}
					}
				}
			}

			if (ResourceFound == false) {
				ScreenMessages.PostScreenMessage ("Warning: " + vessel.name + " has run out of Mono-Propellant, stationkeeping stopped");
				VesselData.CanStationKeep.Remove (vessel.id);
				VesselData.CanStationKeep.Add (vessel.id, false);
			}
		}
	}
} 

