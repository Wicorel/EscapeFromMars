using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using System.Linq;
using Duckroll;
using VRage.Game;
using Sandbox.Game;
using SpaceEngineers.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.Entity;
using Sandbox.Common.ObjectBuilders.Definitions;
using VRage.Utils;

namespace EscapeFromMars
{
	public class MissionSystem : ModSystemUpdatable
	{
		private readonly Vector3D iceMineCoords = new Vector3D(1869197.75, -2004785.25, 1316381.88);
		private readonly Vector3D airBaseAlpha = new Vector3D(1874540.38, -2010993.5, 1317659);
		private readonly Vector3D airBaseBeta = new Vector3D(1854619, -2004395.25, 1324894.12);
		private readonly Vector3D groundBaseAlpha = new Vector3D(1861361.75, -2010331.12, 1323734);
		private readonly Vector3D groundBaseBeta = new Vector3D(1867627, -1998633.62, 1313936.38);
		private readonly Vector3D groundBaseGamma = new Vector3D(1842275.88, -1997805.25, 1325497.5);
		private readonly Vector3D gCorpHqTower = new Vector3D(1857362.12, -1999275.62, 1321122.5);
		private readonly Vector3D flightResearchStationCoords = new Vector3D(1854785.5, -2005881.88, 1325419.5);
		private readonly Vector3D olympusMons = new Vector3D(1879318.25, -2013002, 1318112.62);
		private readonly Vector3D wreckedGCorpVehicle = new Vector3D(1864453.62, -1997478.62, 1315474.12);
		private readonly Vector3D commsSatellite = new Vector3D(1891129.38, -1979974.88, 1354187);
		private readonly Vector3D explosivesOnWalls = new Vector3D(1843157.25, -1996289, 1324475.75);
		private readonly Vector3D weaponsResearchStationCoords = new Vector3D(1843160.75, -1996228.88, 1324462.88);

        // computer systems in weaponsResearchStation
        private readonly Vector3D computerSystems = new Vector3D(1843299.62, -1996457, 1324488.5);
        private readonly Vector3D GBAlphacomputerSystems = new Vector3D(1861360.78, -2010357.91, 1323716.37);
        private readonly Vector3D GBGammacomputerSystems = new Vector3D(1842216.2, -1997678.66, 1325445.14);

        //GPS:Computer Systems GB Alpha:1861360.78:-2010357.91:1323716.37:
        // GB Beta is close to crash site... too close=too early
        //GPS:Computer Systems GB Gamma:1842216.2:-1997678.66:1325445.14:

        private readonly Vector3D mreBaseEntrance = new Vector3D(1858392, -1989135.38, 1312694.25);
		private readonly Vector3D mreElevator = new Vector3D(1858371.88, -1989136.88, 1312675.25);
		private readonly Vector3D oldFreighter = new Vector3D(1858216.25, -1988904.25, 1312611.88);
		private readonly Vector3D hiddenMreBase = new Vector3D(1858396.5, -1989138.38, 1312713.25);
		private readonly Vector3D mreResearchCenter = new Vector3D(1851938.38, -2001116.38, 1324439.62);
		private readonly Vector3D insideIceMine = new Vector3D(1869177.88, -2004855, 1316428.12);
		private readonly Vector3D mreMedFacility1 = new Vector3D(1857942.38, -2006158.62, 1324054);
		private readonly Vector3D droneWreck = new Vector3D(1854936.75, -2006193.25, 1325297.5);

        // GPS:Opportunity:1859277.56:-2019476.58:1327135.68:

		private readonly QueuedAudioSystem audioSystem;
		private readonly ResearchControl researchControl;
		private readonly DateTime missionStartTime;
		private readonly HashSet<int> excludedIDs;
		private readonly List<TimeBasedMissionPrompt> timeBasedPrompts = new List<TimeBasedMissionPrompt>();

		private readonly List<LocationBasedMissionPrompt> locationbasedMissionPrompts =
			new List<LocationBasedMissionPrompt>();

        private readonly HashSet<long> desiredEntityIDs;

        private Version gameVersion;
        private int modBuildWhenLastSaved;

         List<IMyTerminalBlock> cachedTerminalBlocks = new List<IMyTerminalBlock>();

        private MyStringId GCorpIceMineID;
        private MyStringId MarsIceMininigFacilityID;

        private MyStringId FlightResearchStationID;
        private MyStringId GCorpFlightResearchStationID;

        private MyStringId SecretWeaponResearchFacilityID;
        private MyStringId IllegalGCorpResearchFacilityID;

        private MyStringId SuspiciousStorageFacilityID;
        private MyStringId FacilityNotOnAnyOfficalMapID;

        private MyStringId CommunicationsSatelliteID;
        private MyStringId MarsCommunicationsSatelliteID;

        private MyStringId GCorpHeadquartersID;
        private MyStringId AreaExtremelyDangerousID;

        private MyStringId GCorpHiddenFileContentsID;
        private MyStringId HiddenFilesFoundByMabelID;

        private MyStringId MREExperimentSiteID;
        private MyStringId HiddenSiteofMREID;

        internal MissionSystem(int modBuildWhenLastSaved, Version gameVersion, long missionStartTimeBinary, HashSet<int> alreadyExecutedPrompts,
			QueuedAudioSystem audioSystem, ResearchControl researchControl)
		{
            this.gameVersion = gameVersion;
			this.audioSystem = audioSystem;
			this.researchControl = researchControl;
            this.modBuildWhenLastSaved = modBuildWhenLastSaved; //V27

			missionStartTime = DateTime.FromBinary(missionStartTimeBinary);
			excludedIDs = alreadyExecutedPrompts;
            desiredEntityIDs = new HashSet<long>();
            GeneratePrompts();
			timeBasedPrompts.Sort((x, y) => -x.TriggerTime.CompareTo(y.TriggerTime));

            GCorpIceMineID = MyStringId.TryGet("GCorpIceMine");
            MarsIceMininigFacilityID = MyStringId.TryGet("MarsIceMininigFacility");

            FlightResearchStationID = MyStringId.TryGet("FlightResearchStation");
            GCorpFlightResearchStationID = MyStringId.TryGet("GCorpFlightResearchStation");

            SecretWeaponResearchFacilityID = MyStringId.TryGet("SecretWeaponResearchFacility");
            IllegalGCorpResearchFacilityID = MyStringId.TryGet("IllegalGCorpResearchFacility");

            SuspiciousStorageFacilityID = MyStringId.TryGet("SuspiciousStorageFacility");
            FacilityNotOnAnyOfficalMapID = MyStringId.TryGet("FacilityNotOnAnyOfficalMap");

            CommunicationsSatelliteID = MyStringId.TryGet("CommunicationsSatellite");
            MarsCommunicationsSatelliteID = MyStringId.TryGet("MarsCommunicationsSatellite");

            GCorpHeadquartersID = MyStringId.TryGet("GCorpHeadquarters");
            AreaExtremelyDangerousID = MyStringId.TryGet("AreaExtremelyDangerous");


            GCorpHiddenFileContentsID = MyStringId.TryGet("GCorpHiddenFileContents");
            HiddenFilesFoundByMabelID = MyStringId.TryGet("HiddenFilesFoundByMabel");

            MREExperimentSiteID = MyStringId.TryGet("MREExperimentSite");
            HiddenSiteofMREID = MyStringId.TryGet("HiddenSiteofMRE");

        //            ModLog.Info("Start Time = " + missionStartTime.ToString());
        ModLog.Info("Current Mission Length: " + (MyAPIGateway.Session.GameDateTime - missionStartTime).ToString(@"hh\:mm\:ss")); //V27
        }

        internal long GetMissionStartTimeBinary()
		{
			return missionStartTime.ToBinary();
		}

		internal HashSet<int> GetExcludedIDs()
		{
			return excludedIDs;
		}

		// Adds should be kept in order and never removed or reordered to after release, but can be added to!
		private void GeneratePrompts()
		{
            // Crash medbay: 79910699489349926
            // Crach Battery: 128751162949284485
            // Crash Oxygen Generator: 73692089107568958

            // Crash LCD Panel1 Outgoing Transmission: 121786820996539580
            // Crash LCD Panel Incoming Transmission: 128623467162369040
            // Crash LCD Panel Friendly Fire: 79781732474214152
            // Crash LCD Panel Autolog: 142828776791892368
            // Crash LCD Panel Tools Locker: 110371608100898677
            // Crash LCD Panel Build Guide: 80461927256500036
            // Crash LCD Panel Emergency Supplies: 87005598531295535
            // Crash LCD Panel Spare Parts: 143319951822334717
            // Crash Alarm Sound: 113240311055457124

            long medbayID = 79910699489349926;
            long batteryID = 128751162949284485;
            long gasGenID=73692089107568958;

            long outgoingID = 121786820996539580;
            long icomingID = 128623467162369040;
            long friendlyFireID = 79781732474214152;
            long autologID = 142828776791892368;
            long toolsLockerID = 110371608100898677;
            long buildGuideID = 80461927256500036;
            long emergencySuppliesID = 87005598531295535;
            long sparePartsID = 143319951822334717;
            long alarmSoundID = 113240311055457124;

            AddInterest(outgoingID);
            AddInterest(icomingID);
            AddInterest(friendlyFireID);
            AddInterest(autologID);
            AddInterest(toolsLockerID);
            AddInterest(buildGuideID);
            AddInterest(emergencySuppliesID);
            AddInterest(sparePartsID);
            AddInterest(alarmSoundID);
            AddInterest(medbayID);
            AddInterest(batteryID);
            AddInterest(gasGenID);
//            ModLog.Info("AddedInterest()");

            AddTimePrompt(1, new TimeSpan(0, 0, 1),
            TurnBlockOff(outgoingID),
            TurnBlockOff(icomingID),
            TurnBlockOff(friendlyFireID),
            TurnBlockOff(autologID),
//            TurnBlockOff(toolsLockerID),
            TurnBlockOff(buildGuideID),
//            TurnBlockOff(emergencySuppliesID),
//            TurnBlockOff(sparePartsID),
            TurnBlockOff(alarmSoundID),
 //           TurnBlockOff(medbayID), DO NOT TURN OFF.  NEEDED TO SPAWN PLAYERS!!!
            TurnBlockOn(medbayID),
            TurnBlockOff(batteryID),
            TurnBlockOff(gasGenID)
                );

            // battery

            AddTimePrompt(5, new TimeSpan(0, 0, 5),
				PlayAudioClip(AudioClip.ShuttleDamageReport));

            AddTimePrompt(7, new TimeSpan(0, 0, 11), TurnBlockOn(batteryID), PlayAudioClip(AudioClip.MabelPowerUpClipped));


            // Medbay and gas Gen
            AddTimePrompt(11, new TimeSpan(0, 0, 22), TurnBlockOn(medbayID), TurnBlockOn(gasGenID));

            AddTimePrompt(12, new TimeSpan(0, 0, 24), PlayAudioClip(AudioClip.MabelUncoveringFiles));
            // incoming transmission
            AddTimePrompt(18, new TimeSpan(0, 0, 26), TurnBlockOn(icomingID));

            // autolog
            AddTimePrompt(13, new TimeSpan(0, 0, 28), TurnBlockOn(autologID));

            // outgoing transmission
            AddTimePrompt(19, new TimeSpan(0, 0, 32), TurnBlockOn(outgoingID));

            // Friendly Fire
            AddTimePrompt(14, new TimeSpan(0, 0, 35), TurnBlockOn(friendlyFireID));
            // sound block
            AddTimePrompt(15, new TimeSpan(0, 0, 35), TurnBlockOn(alarmSoundID));

            // cargo LCD nameplates
            AddTimePrompt(16, new TimeSpan(0, 0, 40), TurnBlockOn(emergencySuppliesID), TurnBlockOn(sparePartsID), TurnBlockOn(toolsLockerID));

//            AddTimePrompt(17, new TimeSpan(0, 0, 45), PlayAudioClip(AudioClip.MabelUncoveringFiles));

            
            AddTimePrompt(20, new TimeSpan(0, 3, 0),
                PlayAudioClip(AudioClip.ShuttleDatabanks)
                );
            AddTimePrompt(21, new TimeSpan(0, 3, 5),
                TurnBlockOn(buildGuideID)
                );

            /*
            AddTimePrompt(21, new TimeSpan(0, 1, 1),
                AddObjectiveGps("Communications Satellite", "Mars Communications Satellite", commsSatellite, Color.LawnGreen),
                AddObjectiveGps("G-Corp Headquarters", "This area is extremely dangerous", gCorpHqTower, Color.LightBlue),
                AddObjectiveGps("MRE Experiment Site", "Hidden site of Mars Research Expeditions terraforming poject", hiddenMreBase, Color.LightBlue)
                );
                */

            AddTimePrompt(30, new TimeSpan(0, 5, 0),
				PlayAudioClip(AudioClip.GCorpBlockingSignals));

			AddTimePrompt(40, new TimeSpan(0, 10, 0),
				PlayAudioClip(AudioClip.LocatedIceMine),
                AddGps(VRage.MyTexts.Get(GCorpIceMineID).ToString(), VRage.MyTexts.Get(MarsIceMininigFacilityID).ToString(), iceMineCoords));
//            AddGps("GCorp Ice Mine", "Mars ice mining facility", iceMineCoords));

            AddProximityPrompt(40, iceMineCoords, 300,
				PlayAudioClip(AudioClip.IceMineFoundByAccident),
                AddGps(VRage.MyTexts.Get(GCorpIceMineID).ToString(), VRage.MyTexts.Get(MarsIceMininigFacilityID).ToString(), iceMineCoords));
            //			AddGps("GCorp Ice Mine", "Mars ice mining facility", iceMineCoords));

            AddTimePrompt(45, new TimeSpan(0, 15, 0),
				PlayAudioClip(AudioClip.ArmorVehicles),TurnBlockOn(80461927256500036));

			AddTimePrompt(50, new TimeSpan(0, 40, 0),
				PlayAudioClip(AudioClip.InterceptingTransmissions));

			AddTimePrompt(60, new TimeSpan(1, 0, 0),
				PlayAudioClip(AudioClip.SuggestPiracy));

			AddTimePrompt(70, new TimeSpan(2, 0, 0),
				PlayAudioClip(AudioClip.FlightResearchCenter),
                AddGps(VRage.MyTexts.Get(FlightResearchStationID).ToString(), VRage.MyTexts.Get(GCorpFlightResearchStationID).ToString(), flightResearchStationCoords));
//            AddGps("Flight Research Station", "GCorp Flight Research Station", flightResearchStationCoords));

			AddProximityPrompt(70, flightResearchStationCoords, 1000,
				PlayAudioClip(AudioClip.AccidentallyFoundFlightResearchCenter),
                AddGps(VRage.MyTexts.Get(FlightResearchStationID).ToString(), VRage.MyTexts.Get(GCorpFlightResearchStationID).ToString(), flightResearchStationCoords));
            //				AddGps("Flight Research Station", "GCorp Flight Research Station", flightResearchStationCoords));

            AddTimePrompt(80, new TimeSpan(3, 0, 0),
				PlayAudioClip(AudioClip.MarsGCorpOperationsExplained));

//            AddTimePrompt(90, new TimeSpan(5, 0, 0),
            AddTimePrompt(90, new TimeSpan(4, 0, 0), // Changed V12 Reduce time to reveal
                PlayAudioClip(AudioClip.WeaponsResearchFacility),
                AddGps(VRage.MyTexts.Get(SecretWeaponResearchFacilityID).ToString(), VRage.MyTexts.Get(IllegalGCorpResearchFacilityID).ToString(), weaponsResearchStationCoords));
            // 				AddGps("Secret Weapon Research Facility", "Illegal GCorp Research Facility", weaponsResearchStationCoords));

            AddProximityPrompt(90, weaponsResearchStationCoords, 200,
				PlayAudioClip(AudioClip.WeaponsFacilityFoundByAccident),
         AddGps(VRage.MyTexts.Get(SuspiciousStorageFacilityID).ToString(), VRage.MyTexts.Get(FacilityNotOnAnyOfficalMapID).ToString(), weaponsResearchStationCoords));
            // 				AddGps("Suspicious Storage Facility", "This facility was not on any official maps", weaponsResearchStationCoords));

            AddTimePrompt(100, new TimeSpan(6, 0, 0),
				PlayAudioClip(AudioClip.NotifyOfSatellite),
                AddObjectiveGps(VRage.MyTexts.Get(CommunicationsSatelliteID).ToString(), VRage.MyTexts.Get(MarsCommunicationsSatelliteID).ToString(), commsSatellite, Color.LawnGreen));
//            AddObjectiveGps("Communications Satellite", "Mars Communications Satellite", commsSatellite, Color.LawnGreen));

            AddTimePrompt(110, new TimeSpan(7, 0, 0),
				PlayAudioClip(AudioClip.RingAroundMars));

			AddProximityPrompt(130, airBaseAlpha, 2000,
				PlayAudioClip(AudioClip.GCorpFacilitiesHeavilyArmed));
			AddProximityPrompt(130, airBaseBeta, 2000,
				PlayAudioClip(AudioClip.GCorpFacilitiesHeavilyArmed));
			AddProximityPrompt(130, groundBaseAlpha, 2000,
				PlayAudioClip(AudioClip.GCorpFacilitiesHeavilyArmed));
			AddProximityPrompt(130, groundBaseBeta, 2000,
				PlayAudioClip(AudioClip.GCorpFacilitiesHeavilyArmed));
			AddProximityPrompt(130, groundBaseGamma, 2000,
				PlayAudioClip(AudioClip.GCorpFacilitiesHeavilyArmed));

			AddProximityPrompt(140, gCorpHqTower, 3000,
				PlayAudioClip(AudioClip.GCorpTowerScan),
                AddObjectiveGps(VRage.MyTexts.Get(GCorpHeadquartersID).ToString(), VRage.MyTexts.Get(AreaExtremelyDangerousID).ToString(), gCorpHqTower, Color.LightBlue)
//                AddObjectiveGps("G-Corp Headquarters", "This area is extremely dangerous",gCorpHqTower,Color.LightBlue)
                );


            AddProximityPrompt(150, iceMineCoords, 130,
				PlayAudioClip(AudioClip.TurretsAtTheMine));

			AddProximityPrompt(160, oldFreighter, 80,
				PlayAudioClip(AudioClip.OldFreighterFound));

			AddProximityPrompt(170, wreckedGCorpVehicle, 1500,
				PlayAudioClip(AudioClip.DistressBeaconSilly));

			AddProximityPrompt(180, explosivesOnWalls, 40,
				PlayAudioClip(AudioClip.ExplosivesNearby));

			AddProximityPrompt(190, olympusMons, 8500,
				PlayAudioClip(AudioClip.OlympusMons));

			AddProximityPrompt(200, computerSystems, 13,
				PlayAudioClip(AudioClip.FoundFilesOnNetwork),
                AddGps(VRage.MyTexts.Get(GCorpHiddenFileContentsID).ToString(), VRage.MyTexts.Get(HiddenFilesFoundByMabelID).ToString(), computerSystems),
                AddObjectiveGps(VRage.MyTexts.Get(MREExperimentSiteID).ToString(), VRage.MyTexts.Get(HiddenSiteofMREID).ToString(), hiddenMreBase, Color.LightBlue)
//                AddGps("GCorp hidden file contents", "Hidden files found by Mabel", computerSystems),
//                AddObjectiveGps("MRE Experiment Site", "Hidden site of Mars Research Expeditions terraforming poject", hiddenMreBase, Color.LightBlue)
                );

            // V 12 Add other ways to find out about MRE base.  The two 'far' ground bases have computer systems that reveal the location
            AddProximityPrompt(200, GBAlphacomputerSystems, 13,
                PlayAudioClip(AudioClip.FoundFilesOnNetwork),
                AddObjectiveGps(VRage.MyTexts.Get(MREExperimentSiteID).ToString(), VRage.MyTexts.Get(HiddenSiteofMREID).ToString(), hiddenMreBase, Color.LightBlue)
                //                AddObjectiveGps("MRE Experiment Site", "Hidden site of Mars Research Expeditions terraforming poject", hiddenMreBase, Color.LightBlue)
                );
            AddProximityPrompt(200, GBGammacomputerSystems, 13,
                PlayAudioClip(AudioClip.FoundFilesOnNetwork),
                AddObjectiveGps(VRage.MyTexts.Get(MREExperimentSiteID).ToString(), VRage.MyTexts.Get(HiddenSiteofMREID).ToString(), hiddenMreBase, Color.LightBlue)
                //                AddObjectiveGps("MRE Experiment Site", "Hidden site of Mars Research Expeditions terraforming poject", hiddenMreBase, Color.LightBlue)
                );



            AddProximityPrompt(210, commsSatellite, 9000,
				PlayAudioClip(AudioClip.EscapedMars));

			AddProximityPrompt(220, commsSatellite, 250, // reduced from 500 to allow satelite turret to kill incoming players in jetpack.
				PlayAudioClip(AudioClip.EndCredits),
				UnlockAllTech());

			AddProximityPrompt(230, mreBaseEntrance, 13.5,
				PlayAudioClip(AudioClip.WelcomeBack));

			AddProximityPrompt(240, mreElevator, 30,
				PlayAudioClip(AudioClip.ExperimentProgress));

			AddProximityPrompt(250, mreResearchCenter, 5,
				PlayAudioClip(AudioClip.OhDear));

			AddProximityPrompt(260, insideIceMine, 10,
				PlayAudioClip(AudioClip.ElevatorHere));

			AddProximityPrompt(270, droneWreck, 17,
				PlayAudioClip(AudioClip.FaintPowerSignature));

			AddProximityPrompt(280, mreMedFacility1, 30,
				PlayAudioClip(AudioClip.MreDefunded));
		}

		private Action PlayAudioClip(IAudioClip clip)
		{
            return () => {
                audioSystem.PlayAudio(clip);
//                ModLog.Info("PlayAudioClip(" + clip.Filename + ")");
            };
		}

		internal static Action AddGps(string name, string description, Vector3D coords)
		{
			return () => { DuckUtils.AddGpsToAllPlayers(name, description, coords); };
		}

        internal static Action AddObjectiveGps(string name, string description, Vector3D coords, Color color)
        {
            return () => { MyVisualScriptLogicProvider.AddGPSObjectiveForAll(name, description, coords, color); };
        }


        internal void AddInterest(long EntityId)
        {
            // register our interest in finding this entity ID later in TerminalBlocks
            desiredEntityIDs.Add(EntityId);
        }

        private Action SetTextPanel(long panelID, string text)
        {
//            ModLog.Info("SetTextPanel("+panelID+")");

            return () => {
                foreach (IMyTerminalBlock tb in cachedTerminalBlocks)
                {
                    if(tb.EntityId==panelID)
                    {
                        var panel = tb as IMyTextPanel;
                        if (panel!=null)
                        {
                            panel.WriteText(text);
                        }
                        break; // we found the only one
                    }
                }
            };
        }
        private Action TurnBlockOn(long blockID)
        {
            return () => {
//                ModLog.Info("TurnBlockOn(" + blockID + ")");
//                ModLog.Info(cachedTerminalBlocks.Count.ToString() + " Cached Blocks");
                foreach (IMyTerminalBlock tb in cachedTerminalBlocks)
                {
                    if (tb.EntityId == blockID)
                    {
                        IMyFunctionalBlock fb = tb as IMyFunctionalBlock;
                        if (fb != null) fb.Enabled = true;
                        if(fb is IMySoundBlock)
                        {
                            // if it's a sound block, start it playing
                            var sb = fb as IMySoundBlock;
                            sb.Play();
                        }
//                        ModLog.Info("Found");
                        break; // we found the only one
                    }
                }

            };
        }
        private Action TurnBlockOff(long blockID)
        {
            return () => {
//                ModLog.Info("TurnBlockOff(" + blockID + ")");
//                ModLog.Info(cachedTerminalBlocks.Count.ToString() + " Cached Blocks");
                foreach (IMyTerminalBlock tb in cachedTerminalBlocks)
                {
                    if (tb.EntityId == blockID)
                    {
                        if (tb is IMyFunctionalBlock)
                        {
                            var fb = tb as IMyFunctionalBlock;
                            fb.Enabled = false;
                        }
                        //                        ModLog.Info("Found");
                        break; // we found the only one
                    }
                }
//                ModLog.Info("Not Found");
            };
        }


        internal Action UnlockAllTech()
		{
			return () =>
			{
				foreach (TechGroup techGroup in Enum.GetValues(typeof(TechGroup)))
				{
					researchControl.UnlockTechGroupForAllPlayers(techGroup);
				}
			};
		}

		private bool AddTimePrompt(int id, TimeSpan timeIntoGameTriggered, params Action[] actions)
		{
			if (excludedIDs.Contains(id))
			{
				return false;
			}
			var prompt = new TimeBasedMissionPrompt(id, new List<Action>(actions), missionStartTime + timeIntoGameTriggered,
				excludedIDs);
			timeBasedPrompts.Add(prompt);
            return true;
		}

		private void AddProximityPrompt(int id, Vector3D locationVector, double distance, params Action[] actions)
		{
			if (excludedIDs.Contains(id))
			{
				return;
			}
			var prompt = new LocationBasedMissionPrompt(id, new List<Action>(actions), locationVector, distance, excludedIDs);
			locationbasedMissionPrompts.Add(prompt);
		}

		public override void Update60()
		{
			UpdateLocationBasedPrompts();
			UpdateTimeBasedPrompts();
		}

		private void UpdateLocationBasedPrompts()
		{
			if (locationbasedMissionPrompts.Count == 0)
			{
				return;
			}

			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);

			foreach (var player in players)
			{
				var controlled = player.Controller.ControlledEntity;
				if (controlled == null) continue;
				var position = controlled.Entity.GetPosition();

				foreach (var locationPrompt in locationbasedMissionPrompts.Reverse<LocationBasedMissionPrompt>())
				{
					var distSq = Vector3D.DistanceSquared(locationPrompt.Location, position);

					if (distSq <= locationPrompt.DistanceSquared)
					{
						locationPrompt.Run();
						locationbasedMissionPrompts.Remove(locationPrompt);
						excludedIDs.Add(locationPrompt.Id); // Never trigger this again
					}
				}
			}
		}

		private void UpdateTimeBasedPrompts()
		{
			if (timeBasedPrompts.Count == 0)
			{
				return;
			}

			var prompt = timeBasedPrompts[timeBasedPrompts.Count - 1];
			if (MyAPIGateway.Session.GameDateTime >= prompt.TriggerTime)
			{
//                ModLog.Info("Time trigger " + prompt.ToString());
				prompt.Run();
				timeBasedPrompts.RemoveAt(timeBasedPrompts.Count - 1);
				excludedIDs.Add(prompt.Id); // Never trigger this again
			}
		}

        /// <summary>
        /// On grid creation, walk the grid and find terminal blocks of interest
        /// </summary>
        /// <param name="grid"></param>
        public override void GridInitialising(IMyCubeGrid grid)
        {
//            ModLog.Info("GridInit:"+grid.EntityId.ToString());
//            ModLog.Info(desiredEntityIDs.Count + " Desired Blocks");
            var slimBlocks = new List<IMySlimBlock>();
            if (grid.EntityId == 92770753627258475 // crash ship
                && modBuildWhenLastSaved<27 // we haven't done this already
                )
            {
                grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyCargoContainer);

//                ModLog.Info("Found Crash Ship on initial load"+modBuildWhenLastSaved);

                /* SE 1.192 */
                var medkit = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>("Medkit");
                var powerkit = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>("Powerkit");
                /*
                var datapad = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Datapad>("Datapad");
                datapad.Data = "Wico Test\nGPS:Crash Site:1868092.62:-2003480.62:1316653.75:";
                datapad.Name = "Wico Name Test";
                */
                var spacecredit = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalObject>("SpaceCredit");

                foreach (var slim in slimBlocks)
                {
                    IMyTerminalBlock tb = slim.FatBlock as IMyTerminalBlock;
                    if (tb == null) continue;
                    if (tb.EntityId == 115310750474309501) // Emergency Supplies
                    {
                        var cargoContainer = (IMyCargoContainer)slim.FatBlock;
                        //                MyLog.Default.WriteLine("LoadCargo: " + cargoContainer.CustomName);
                        //                MyLog.Default.Flush();
                        var entity = cargoContainer as MyEntity;
                        if (entity.HasInventory)
                        {

                            MyInventory inventory = entity.GetInventoryBase() as MyInventory;
                            if (inventory == null) continue;

                            DuckUtils.PlaceItemIntoCargo(inventory, medkit, 15);
                            DuckUtils.PlaceItemIntoCargo(inventory, powerkit, 15);
                            //                            PlaceItemIntoCargo(inventory, datapad, 1);
                            DuckUtils.PlaceItemIntoCargo(inventory, spacecredit, 100);

                        }
                    }
                }
                /* */
            }

            grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyTerminalBlock);
            foreach (var slim in slimBlocks)
            {
                IMyTerminalBlock tb = slim.FatBlock as IMyTerminalBlock;

                if (tb == null)
                {
                    /*
                    if (grid.EntityId == 92770753627258475) // crash ship
                    {
                        ModLog.Info("Crash: Skipping" + slim.FatBlock.ToString());

                    }
                    */
                    //                   continue;
                }
                else
                {
                    /*
                    if (grid.EntityId == 92770753627258475) // crash ship
                    {
                        ModLog.Info("Checking Block:" + tb.EntityId.ToString());
                    }
                    */
                    if (desiredEntityIDs.Contains(tb.EntityId))
                    {
//                        ModLog.Info("Found desired Entity:" + tb.EntityId.ToString());

                        cachedTerminalBlocks.Add(tb);
                    }
                }
            }
        }
    }

    internal abstract class MissionPrompt
	{
		internal int Id { get; }
		private readonly List<Action> actions;
		private readonly HashSet<int> excludedIDs;

		internal MissionPrompt(List<Action> actions, int id, HashSet<int> excludedIDs)
		{
			this.actions = actions;
			this.excludedIDs = excludedIDs;
			Id = id;
		}

		internal void Run()
		{
			if (excludedIDs.Contains(Id))
			{
				return; // Do nothing, another prompt sharing our ID was triggered already
			}

			foreach (var action in actions)
			{
				action();
			}
		}
	}

	internal class TimeBasedMissionPrompt : MissionPrompt
	{
		internal DateTime TriggerTime { get; }

		internal TimeBasedMissionPrompt(int id, List<Action> actions, DateTime triggerTime, HashSet<int> excludedIDs)
			: base(actions, id, excludedIDs)
		{
			TriggerTime = triggerTime;
		}
	}

	internal class LocationBasedMissionPrompt : MissionPrompt
	{
		internal Vector3D Location { get; }
		internal double DistanceSquared { get; }

		internal LocationBasedMissionPrompt(int id, List<Action> actions, Vector3D locationVector, double distance,
			HashSet<int> excludedIDs) : base(actions, id, excludedIDs)
		{
			Location = locationVector;
			DistanceSquared = distance * distance;
		}
	}
}