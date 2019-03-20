using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using Sandbox.Game.Entities;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using Duckroll;

namespace EscapeFromMars
{
	internal class ConvoySpawner : ModSystemUpdatable
	{
		public static readonly bool DebugConvoys = false; // turn on to force lots of convoy spawns
        public static readonly bool ForceGroundOnly = false; // turn to force ground-only convoys
        public static readonly bool ForceAirOnly = false; // uhh.. yeah

		private readonly HeatSystem heatSystem;
		private readonly QueuedAudioSystem audioSystem;
		private readonly List<IMyRemoteControl> spawningBases = new List<IMyRemoteControl>();
		private readonly Random random = new Random();
		private DateTime nextSpawnTime;
            
		internal ConvoySpawner(HeatSystem heatSystem, QueuedAudioSystem audioSystem)
		{
			this.heatSystem = heatSystem;
			this.audioSystem = audioSystem;
		}

		internal long GetNextSpawnTimeForSaving()
		{
			return nextSpawnTime.ToBinary();
		}

		private const string DeliverySpawnerPrefix = "DELIVERY_SPAWNER";

		public override void GridInitialising(IMyCubeGrid grid)
		{
			if (!grid.IsStatic)
			{
				return;
			}
			
			foreach (var remoteControl in grid.GetTerminalBlocksOfType<IMyRemoteControl>())
			{
				if (remoteControl.IsControlledByFaction("GCORP") && 
				    remoteControl.CustomName.Contains(DeliverySpawnerPrefix)) // Finds both ground and air spawners
				{
					spawningBases.Add(remoteControl);
				}
			}
		}

		internal void RestoreSpawnTimeFromSave(long savedTime)
		{
			nextSpawnTime = DebugConvoys ?  MyAPIGateway.Session.GameDateTime 
				: DateTime.FromBinary(savedTime);
		}

		public override void Update300()
		{
			if (nextSpawnTime.Equals(DateTime.MinValue)) // New game before any save
			{
				var delayUntilFirstConvoy = DebugConvoys ? new TimeSpan(0, 0, 10) : new TimeSpan(0, 45, 0);
				nextSpawnTime = MyAPIGateway.Session.GameDateTime + delayUntilFirstConvoy;
			}
			else
			{
				if (MyAPIGateway.Session.GameDateTime >= nextSpawnTime)
				{
					SpawnConvoy();
					ResetTimeUntilNextConvoy();
				}
			}
		}

		private void ResetTimeUntilNextConvoy()
		{
			var delayUntilNextConvoy = DebugConvoys ? new TimeSpan(0, 0, 30) : new TimeSpan(0, random.Next(12, 18), 0);
			nextSpawnTime = MyAPIGateway.Session.GameDateTime + delayUntilNextConvoy;
		}

		internal void SpawnConvoy()
		{
			var baseToSpawnAt = ChooseSpawningBase();
			if (baseToSpawnAt != null)
			{
				SpawnConvoy(baseToSpawnAt);
				audioSystem.PlayAudioRandomChance(0.1, AudioClip.ConvoyDispatched1, AudioClip.ConvoyDispatched2,
					AudioClip.ConvoyDispatched3);
			}
		}

		private IMyRemoteControl ChooseSpawningBase()
		{
			if (spawningBases.Count == 0)
			{
				return null;
			}

			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);

			var playerPositions = new List<Vector3D>();
			foreach (var player in players)
			{
				var controlled = player.Controller.ControlledEntity;
				if (controlled == null) continue;
				playerPositions.Add(controlled.Entity.GetPosition());
			}

			if (playerPositions.Count == 0)
			{
				return null;
			}

			var baseDistances = new Dictionary<double, IMyRemoteControl>();
			var positionToSpawnNearTo = playerPositions.GetRandomItemFromList();
			foreach (var spawningBase in spawningBases)
			{
				if (!spawningBase.IsControlledByFaction("GCORP"))
				{
					continue;
				}

				var distSq = Vector3D.DistanceSquared(spawningBase.PositionComp.GetPosition(), positionToSpawnNearTo);
                if (DebugConvoys && ForceGroundOnly && !spawningBase.CustomName.Contains("GROUND")) continue;
                if (DebugConvoys && ForceAirOnly && spawningBase.CustomName.Contains("GROUND")) continue;

                baseDistances.Add(distSq, spawningBase);
			}

			var sortedDistances = baseDistances.Keys.ToList();
			sortedDistances.Sort();

			foreach (var distance in sortedDistances)
			{
				var d4 = random.Next(0, 4);
				// 50% of spawning at closest, then next closest and so on. But don't spawn within 1km of a player, as they will see it
				if (d4 > 1 && NoPlayerNearby(baseDistances[distance]))
				{
					return baseDistances[distance];
				}
			}
			return null; // Small chance of nothing spawning at all
		}

		private static bool NoPlayerNearby(VRage.Game.ModAPI.Ingame.IMyEntity baseRc)
		{
			return DebugConvoys || !DuckUtils.IsAnyPlayerNearPosition(baseRc.GetPosition(), 1000);
		}

        private void SpawnConvoy(IMyShipController baseToSpawnAt)
        {
            var factionId = baseToSpawnAt.OwnerId;
            var spawnerPosition = baseToSpawnAt.GetPosition();
            var gravity = baseToSpawnAt.GetNaturalGravity();
            var unitType = baseToSpawnAt.CustomName.Contains("GROUND") ? UnitType.Ground : UnitType.Air;
            var cargoSize = heatSystem.GenerateCargoShipSize();

            //TODO: Should let base define the convoy spawn points
            //TODO: gravity is not normalized and is being used to scale the spawn point...  It should be normalized and then meters used to modify.
            // NOTE: The .Forward IS normalized, so the scalar is in meters.
            if (unitType == UnitType.Air)
            {
                var positionToSpawn = spawnerPosition + gravity * -5f + baseToSpawnAt.WorldMatrix.Forward * 30;
                var transportPrefab = PrefabGrid.GetAirTransport(cargoSize);
                DuckUtils.SpawnInGravity(positionToSpawn, gravity, factionId, transportPrefab.PrefabName,
                    transportPrefab.InitialBeaconName,
                    Vector3D.Normalize(baseToSpawnAt.WorldMatrix.Forward));
            }
            else
            {
                var positionToSpawn = spawnerPosition + gravity * -1f + baseToSpawnAt.WorldMatrix.Forward * 35;
                var transportPrefab = PrefabGrid.GetGroundTransport(cargoSize);
                DuckUtils.SpawnInGravity(positionToSpawn, gravity, factionId, transportPrefab.PrefabName,
                    transportPrefab.InitialBeaconName,
                    Vector3D.Normalize(baseToSpawnAt.WorldMatrix.Forward));
            }
        }

        internal void SpawnConvoyEscorts(IMyCubeGrid convoyLeaderGrid, UnitType unitType, MyPlanet marsPlanet)
		{
			var gravity = marsPlanet.GetGravityAtPoint(convoyLeaderGrid.GetPosition());
			var escortsNeededToSpawn = heatSystem.GenerateEscortSpecs();
			if (unitType == UnitType.Air)
			{
				SpawnAirEscorts(escortsNeededToSpawn, gravity, convoyLeaderGrid);
			}
			else
			{
				SpawnLandEscorts(escortsNeededToSpawn, gravity, convoyLeaderGrid);
				SpawnAirEscorts(escortsNeededToSpawn, gravity, convoyLeaderGrid);
			}
		}

		private void SpawnLandEscorts(ICollection<ShipSize> escortsNeededToSpawn, Vector3D gravity,
			IMyCubeGrid convoyLeaderGrid)
		{
			SpawnEscorts(escortsNeededToSpawn, gravity, Convoy.GroundEscortPositions, UnitType.Ground, convoyLeaderGrid);
		}

		private void SpawnAirEscorts(ICollection<ShipSize> escortsNeededToSpawn, Vector3D gravity,
			IMyCubeGrid convoyLeaderGrid)
		{
			SpawnEscorts(escortsNeededToSpawn, gravity, Convoy.AirEscortPositions, UnitType.Air, convoyLeaderGrid);
		}

		private void SpawnEscorts(ICollection<ShipSize> escortsNeededToSpawn, Vector3D gravity,
			IList<EscortPosition> allowedPositions, UnitType unitType, IMyCubeGrid convoyLeaderGrid)
		{
			var positionIndex = 0;
			foreach (var escort in escortsNeededToSpawn.Reverse())
			{
				SpawnEscortGrid(PrefabGrid.GetEscort(unitType, escort), gravity,
					allowedPositions[positionIndex], unitType, convoyLeaderGrid);
				positionIndex++;
				escortsNeededToSpawn.Remove(escort);
				if (positionIndex == allowedPositions.Count)
				{
					break; // Can't spawn any more of this type, all positions full
				}
			}
		}

		private int GetAdditionalHeightModifier(UnitType convoyUnitType)
		{
			return convoyUnitType == UnitType.Air ? ConvoyAir.AdditionalHeightModifier : ConvoyGround.AdditionalHeightModifier;
		}

		private void SpawnEscortGrid(PrefabGrid prefabGrid, Vector3D naturalGravity, EscortPosition escortPosition,
			UnitType convoyUnitType, IMyCubeGrid convoyLeaderGrid)
		{
			var positionToSpawn = Convoy.GetEscortPositionVector(convoyLeaderGrid, naturalGravity, escortPosition,
				GetAdditionalHeightModifier(convoyUnitType));
			var factionId = convoyLeaderGrid.GetGridControllerFaction();
			var forwards = convoyLeaderGrid.WorldMatrix.Forward;
			DuckUtils.SpawnInGravity(positionToSpawn, naturalGravity, factionId, prefabGrid.PrefabName,
				prefabGrid.InitialBeaconName, forwards);
		}
	}
}