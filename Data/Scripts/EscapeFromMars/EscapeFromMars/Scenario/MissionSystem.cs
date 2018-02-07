using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using System.Linq;
using Duckroll;

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
		private readonly Vector3D computerSystems = new Vector3D(1843299.62, -1996457, 1324488.5);
		private readonly Vector3D mreBaseEntrance = new Vector3D(1858392, -1989135.38, 1312694.25);
		private readonly Vector3D mreElevator = new Vector3D(1858371.88, -1989136.88, 1312675.25);
		private readonly Vector3D oldFreighter = new Vector3D(1858216.25, -1988904.25, 1312611.88);
		private readonly Vector3D hiddenMreBase = new Vector3D(1858396.5, -1989138.38, 1312713.25);
		private readonly Vector3D mreResearchCenter = new Vector3D(1851938.38, -2001116.38, 1324439.62);
		private readonly Vector3D insideIceMine = new Vector3D(1869177.88, -2004855, 1316428.12);
		private readonly Vector3D mreMedFacility1 = new Vector3D(1857942.38, -2006158.62, 1324054);
		private readonly Vector3D droneWreck = new Vector3D(1854936.75, -2006193.25, 1325297.5);

		private readonly QueuedAudioSystem audioSystem;
		private readonly ResearchControl researchControl;
		private readonly DateTime missionStartTime;
		private readonly HashSet<int> excludedIDs;
		private readonly List<TimeBasedMissionPrompt> timeBasedPrompts = new List<TimeBasedMissionPrompt>();

		private readonly List<LocationBasedMissionPrompt> locationbasedMissionPrompts =
			new List<LocationBasedMissionPrompt>();

		internal MissionSystem(long missionStartTimeBinary, HashSet<int> alreadyExecutedPrompts,
			QueuedAudioSystem audioSystem, ResearchControl researchControl)
		{
			this.audioSystem = audioSystem;
			this.researchControl = researchControl;
			missionStartTime = DateTime.FromBinary(missionStartTimeBinary);
			excludedIDs = alreadyExecutedPrompts;
			GeneratePrompts();
			timeBasedPrompts.Sort((x, y) => -x.TriggerTime.CompareTo(y.TriggerTime));
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
			AddTimePrompt(10, new TimeSpan(0, 0, 10),
				PlayAudioClip(AudioClip.ShuttleDamageReport));

			AddTimePrompt(20, new TimeSpan(0, 3, 0),
				PlayAudioClip(AudioClip.ShuttleDatabanks));

			AddTimePrompt(30, new TimeSpan(0, 5, 0),
				PlayAudioClip(AudioClip.GCorpBlockingSignals));

			AddTimePrompt(40, new TimeSpan(0, 10, 0),
				PlayAudioClip(AudioClip.LocatedIceMine),
				AddGps("GCorp Ice Mine", "Mars ice mining facility", iceMineCoords));

			AddProximityPrompt(40, iceMineCoords, 300,
				PlayAudioClip(AudioClip.IceMineFoundByAccident),
				AddGps("GCorp Ice Mine", "Mars ice mining facility", iceMineCoords));

			AddTimePrompt(45, new TimeSpan(0, 15, 0),
				PlayAudioClip(AudioClip.ArmorVehicles));

			AddTimePrompt(50, new TimeSpan(0, 40, 0),
				PlayAudioClip(AudioClip.InterceptingTransmissions));

			AddTimePrompt(60, new TimeSpan(1, 0, 0),
				PlayAudioClip(AudioClip.SuggestPiracy));

			AddTimePrompt(70, new TimeSpan(2, 0, 0),
				PlayAudioClip(AudioClip.FlightResearchCenter),
				AddGps("Flight Research Station", "GCorp Flight Research Station", flightResearchStationCoords));

			AddProximityPrompt(70, flightResearchStationCoords, 1000,
				PlayAudioClip(AudioClip.AccidentallyFoundFlightResearchCenter),
				AddGps("Flight Research Station", "GCorp Flight Research Station", flightResearchStationCoords));

			AddTimePrompt(80, new TimeSpan(3, 0, 0),
				PlayAudioClip(AudioClip.MarsGCorpOperationsExplained));

			AddTimePrompt(90, new TimeSpan(5, 0, 0),
				PlayAudioClip(AudioClip.WeaponsResearchFacility),
				AddGps("Secret Weapon Research Facility", "Illegal GCorp Research Facility", weaponsResearchStationCoords));

			AddProximityPrompt(90, weaponsResearchStationCoords, 200,
				PlayAudioClip(AudioClip.WeaponsFacilityFoundByAccident),
				AddGps("Suspicious Storage Facility", "This facility was not on any official maps", weaponsResearchStationCoords));

			AddTimePrompt(100, new TimeSpan(6, 0, 0),
				PlayAudioClip(AudioClip.NotifyOfSatellite),
				AddGps("Communications Satellite", "Mars Communications Satellite", commsSatellite));

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
				PlayAudioClip(AudioClip.GCorpTowerScan));

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
				AddGps("GCorp hidden file contents", "Hidden files found by Mabel", computerSystems),
				AddGps("MRE Experiment Site", "Hidden site of Mars Research Expeditions terraforming poject", hiddenMreBase));

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
			return () => { audioSystem.PlayAudio(clip); };
		}

		internal static Action AddGps(string name, string description, Vector3D coords)
		{
			return () => { DuckUtils.AddGpsToAllPlayers(name, description, coords); };
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

		private void AddTimePrompt(int id, TimeSpan timeIntoGameTriggered, params Action[] actions)
		{
			if (excludedIDs.Contains(id))
			{
				return;
			}
			var prompt = new TimeBasedMissionPrompt(id, new List<Action>(actions), missionStartTime + timeIntoGameTriggered,
				excludedIDs);
			timeBasedPrompts.Add(prompt);
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
				prompt.Run();
				timeBasedPrompts.RemoveAt(timeBasedPrompts.Count - 1);
				excludedIDs.Add(prompt.Id); // Never trigger this again
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