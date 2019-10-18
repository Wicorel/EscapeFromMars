using System;
using System.Collections.Generic;
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
        // TODO: Change this to be lower/higher depending on difficulty & # of players
		private static //readonly 
            TimeSpan BackupTimeDelay = new TimeSpan(0, 5, 0);
        // set to true to turn off all backups
		public static readonly bool DebugStopBackupGroups = false;

		internal IMyRemoteControl RemoteControl { get; }
		private readonly long baseId;
		private readonly MyPlanet marsPlanet;
		private readonly HeatSystem heatSystem;
		private readonly QueuedAudioSystem audioSystem;
		private bool waitingOnBackup;
		private DateTime lastBackupRespondTime;

        private List<IMyLargeTurretBase> turrets;

        private bool bLostProcessed = false;


        internal GCorpBase(IMyRemoteControl remoteControl, DateTime lastBackupRespondTime, MyPlanet marsPlanet, HeatSystem heatSystem, QueuedAudioSystem audioSystem)
        {
            RemoteControl = remoteControl;
            this.lastBackupRespondTime = lastBackupRespondTime;
            this.marsPlanet = marsPlanet;
            this.heatSystem = heatSystem;
            this.audioSystem = audioSystem;
            baseId = remoteControl.EntityId;

            turrets = new List<IMyLargeTurretBase>();
            var slimBlocks2 = new List<IMySlimBlock>();
            remoteControl.CubeGrid.GetBlocks(slimBlocks2, b => b.FatBlock is IMyLargeTurretBase);
            foreach (var slim in slimBlocks2)
            {
                var turret = slim.FatBlock as IMyLargeTurretBase;
                //                turret.Enabled = false;
                turrets.Add(turret);
            }
        }

        internal void Update()
        {
            if (!RemoteControl.IsControlledByFaction("GCORP"))
            {
                // V26
                if(!bLostProcessed)
                {
                    // first time processing
                    bLostProcessed = true;

                    // TODO: maybe do something else.
                    // maybe turn OFF all turrets
                    // explode all warheads?

                    // if base is 'lost' increase heat level 
                    heatSystem.BaseCaptured();

                    // ?play audio (from mabel?) announcing base captured.
                }

                return; // Maybe remove from list of bases?
            }

            // TODO: get this value from base itself instead of hard-coding?
            var player = DuckUtils.GetNearestPlayerToPosition(RemoteControl.GetPosition(), 1300);

            // turn turrets off on bases if no player is nearby to save simspeed hits
            if (player != null)
            {
                foreach (var turret in turrets)
                {
                    turret.Enabled = true;
                }
            }
            else
            {
                foreach (var turret in turrets)
                    turret.Enabled = false;
            }

            int nPlayers = 0; // number of players in range
            var playerClose1 = DuckUtils.GetNearestPlayerToPosition(RemoteControl.GetPosition(), 1200, out nPlayers);
            if (!DebugStopBackupGroups && playerClose1 != null)
            {
                if (lastBackupRespondTime + BackupTimeDelay < MyAPIGateway.Session.GameDateTime)
                {
 //                   ModLog.Info("Heat Difficulty: " + heatSystem.HeatDifficulty);

                    // TODO: if player is INSIDE base, then do something else?
                    // V26: If player is underground, then do something else?
                    var players = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(players);
                    foreach (var aplayer in players)
                    {
                        // check each player

                        var controlled = aplayer.Controller.ControlledEntity;
                        if (controlled == null) continue;
                        var distSq = Vector3D.DistanceSquared(RemoteControl.GetPosition(), controlled.Entity.GetPosition());
                        if (distSq < 100*100) //V29
                        {
                            if (DuckUtils.IsPlayerUnderCover(aplayer))
                            { // player is under cover 'near' the base..

                            }
                        }
                        else if (distSq < 1000*1000)
                        { // betwee 100->1000
                            if (DuckUtils.IsPlayerUnderground(aplayer))
                            {
                                // player is underground
                                if (heatSystem.HeatDifficulty > 1)
                                { // only if difficulty is above default
//                                    ModLog.Info("Spawning Bomber");
                                    SpawnUndergroundDefense(aplayer); // on top of player
                                    // if we do this, then ground has lost integrity.. make it into an air base
                                    if(RemoteControl.CustomName.Contains("GROUND"))
                                    {
                                        RemoteControl.CustomName.Replace("GROUND", "AIR");
                                    }
                                }
                            }
                        }
                    }

//                    ModLog.Info("Backup Player Close 1");
                    SpawnHelperPatrol(playerClose1);

                    // if higher difficulty and player(s) get closer, then spawn more backup per backup event
                    if (heatSystem.HeatDifficulty > 1 || (heatSystem.MultiplayerScaling && nPlayers>1))
                    {
                        var playerClose2 = DuckUtils.GetNearestPlayerToPosition(RemoteControl.GetPosition(), 800);
                        if (playerClose2 != null)
                        {
                            SpawnHelperPatrol(playerClose2);
//                            ModLog.Info("Backup Player Close 2");
                        }
                    }
                    if (heatSystem.HeatDifficulty > 3 || (heatSystem.MultiplayerScaling && nPlayers > 3))
                    {
                        var playerClose3 = DuckUtils.GetNearestPlayerToPosition(RemoteControl.GetPosition(), 300);
                        if (playerClose3 != null)
                        {
                            SpawnHelperPatrol(playerClose3);
//                            ModLog.Info("Backup Player Close 4");
                        }
                    }
                    lastBackupRespondTime = MyAPIGateway.Session.GameDateTime;
                    audioSystem.PlayAudio(AudioClip.FacilityDetectedHostile, AudioClip.GCorpFacilityThreatened);
                    waitingOnBackup = true;
                }
            }
        }
        internal bool OfferBackup()
		{
			if (waitingOnBackup)
			{
//				waitingOnBackup = false;
				return true;
			}
			return false;
		}

        internal void ClearBackup()
        {
            waitingOnBackup = false;
        }

		internal Vector3D GetBackupPosition()
		{
            // maybe use center of grid and then top of grid (bounding box) and then xx meters 'above' it.
            // also. handle space-based bases (use .Up?)

            // V26
            var vng = RemoteControl.GetNaturalGravity();
            vng.Normalize();
            return RemoteControl.GetPosition() + vng * -20; // 20m above remote
            // RemoteControl.GetNaturalGravity() * -5f; //50m above us
		}

		private void SpawnHelperPatrol(IMyPlayer player)
		{
			var playerPos = player.GetPosition();
			var playerNaturalGravity = marsPlanet.GetGravityAtPoint(playerPos);
			var perpendicularDistance = MyUtils.GetRandomPerpendicularVector(ref playerNaturalGravity) * 400; //4km away NOTE: since mars NG is not 1.0, it's NOT 4km
			var locationToSpawnPatrol = playerPos + perpendicularDistance + playerNaturalGravity * -200f; // 2km up
			var naturalGravityAtSpawn = marsPlanet.GetGravityAtPoint(locationToSpawnPatrol);

			var spawnLocation = MyAPIGateway.Entities.FindFreePlace(locationToSpawnPatrol, 10, 20, 5, 10);

//            ModLog.Info("Spawning helper patrol. perpD=" + perpendicularDistance.ToString("N2"));

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

        private void SpawnUndergroundDefense(IMyPlayer player)
        {
            var playerPos = player.GetPosition();
            var playerNaturalGravity = marsPlanet.GetGravityAtPoint(playerPos);
            var vng = playerNaturalGravity;
            vng.Normalize();

            var surface = marsPlanet.GetClosestSurfacePointGlobal(player.GetPosition());
            var belowsurface = Vector3D.Distance(playerPos, surface);

            //            ModLog.Info("Below Surface=" + belowsurface.ToString("0.00"));

            var up = 50 + belowsurface; // 150m above the surface
//            var up = 150 + belowsurface; // 150m above the surface
            var locationToSpawn = playerPos + vng * -up;

            var naturalGravityAtSpawn = marsPlanet.GetGravityAtPoint(locationToSpawn);

//            DuckUtils.AddGpsToAllPlayers("Location to Spawn", "Above player", locationToSpawn);

            var spawnLocation = MyAPIGateway.Entities.FindFreePlace(locationToSpawn, 10, 20, 5, 10);

            if (spawnLocation.HasValue)
            {
                PrefabGrid bomb = PrefabGrid.GetBomb();
                DuckUtils.SpawnInGravity(spawnLocation.Value, naturalGravityAtSpawn, RemoteControl.OwnerId,
                    bomb.PrefabName, bomb.InitialBeaconName);

                //DEBUG
//                DuckUtils.AddGpsToAllPlayers("Bomb Spawn", "bomb for player", spawnLocation.Value);
//                ModLog.Info("Spawning Bomb!");
            }
            else
            {
                ModLog.DebugError("Couldn't spawn bomb!", locationToSpawn);
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

        public static void SetNormalBackupDelay()
        {
            BackupTimeDelay= new TimeSpan(0, 5, 0);
        }
        public static void SetFastBackupDelay()
        {
            BackupTimeDelay = new TimeSpan(0, 2, 0);
        }
    }

    public class GCorpBaseSaveData
	{
		public long BaseId { get; set; }
		public long LastBackupTimeBinary { get; set; }
	}
}