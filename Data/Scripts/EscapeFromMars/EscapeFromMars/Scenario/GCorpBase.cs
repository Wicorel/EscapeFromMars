using System;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using Duckroll;

namespace EscapeFromMars
{
	internal class GCorpBase
	{
		private static readonly TimeSpan BackupTimeDelay = new TimeSpan(0, 5, 0);
		public static readonly bool DebugStopBackupGroups = false;

		internal IMyRemoteControl RemoteControl { get; }
		private readonly long baseId;
		private readonly MyPlanet marsPlanet;
		private readonly HeatSystem heatSystem;
		private readonly QueuedAudioSystem audioSystem;
		private bool waitingOnBackup;
		private DateTime lastBackupRespondTime;

		internal GCorpBase(IMyRemoteControl remoteControl, DateTime lastBackupRespondTime, MyPlanet marsPlanet, HeatSystem heatSystem, QueuedAudioSystem audioSystem)
		{
			RemoteControl = remoteControl;
			this.lastBackupRespondTime = lastBackupRespondTime;
			this.marsPlanet = marsPlanet;
			this.heatSystem = heatSystem;
			this.audioSystem = audioSystem;
			baseId = remoteControl.EntityId;
		}

		internal void Update()
		{
			if (!RemoteControl.IsControlledByFaction("GCORP"))
			{
				return; // Maybe remove from list of bases?
			}

			var player = DuckUtils.GetNearestPlayerToPosition(RemoteControl.GetPosition(), 1000);
			if (!DebugStopBackupGroups && player != null)
			{
				if (lastBackupRespondTime + BackupTimeDelay < MyAPIGateway.Session.GameDateTime)
				{
					SpawnHelperPatrol(player);
					waitingOnBackup = true;
					lastBackupRespondTime = MyAPIGateway.Session.GameDateTime;
					audioSystem.PlayAudio(AudioClip.FacilityDetectedHostile, AudioClip.GCorpFacilityThreatened);
				}
			}
		}

		internal bool OfferBackup()
		{
			if (waitingOnBackup)
			{
				waitingOnBackup = false;
				return true;
			}
			return false;
		}

		internal Vector3D GetBackupPosition()
		{
			return RemoteControl.GetPosition() + RemoteControl.GetNaturalGravity() * -5f; //50m above us
		}

		private void SpawnHelperPatrol(IMyPlayer player)
		{
			var playerPos = player.GetPosition();
			var playerNaturalGravity = marsPlanet.GetGravityAtPoint(playerPos);
			var perpendicularDistance = MyUtils.GetRandomPerpendicularVector(ref playerNaturalGravity) * 400; //4km away
			var locationToSpawnPatrol = playerPos + perpendicularDistance + playerNaturalGravity * -200f; // 2km up
			var naturalGravityAtSpawn = marsPlanet.GetGravityAtPoint(locationToSpawnPatrol);

			var spawnLocation = MyAPIGateway.Entities.FindFreePlace(locationToSpawnPatrol, 10, 20, 5, 10);

			if (spawnLocation.HasValue)
			{
				PrefabGrid backup = PrefabGrid.GetBackup(heatSystem.GenerateBackupShipSize());
				DuckUtils.SpawnInGravity(spawnLocation.Value, naturalGravityAtSpawn, RemoteControl.OwnerId,
					backup.PrefabName, backup.InitialBeaconName);
			}
			else
			{
				ModLog.DebugError("Couldn't spawn backup!", locationToSpawnPatrol);
			}

		}

		internal GCorpBaseSaveData GenerateSaveData()
		{
			return new GCorpBaseSaveData
			{
				BaseId = baseId,
				LastBackupTimeBinary = lastBackupRespondTime.ToBinary()
			};
		}

		public void RestoreSaveData(GCorpBaseSaveData saveData)
		{
			lastBackupRespondTime = DateTime.FromBinary(saveData.LastBackupTimeBinary);
		}
	}

	public class GCorpBaseSaveData
	{
		public long BaseId { get; set; }
		public long LastBackupTimeBinary { get; set; }
	}
}