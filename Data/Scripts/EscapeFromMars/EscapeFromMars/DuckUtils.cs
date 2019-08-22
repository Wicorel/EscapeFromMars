using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Duckroll
{
	internal class DuckUtils
	{
		internal static IEnumerable<T> GetEnumValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		/// <summary>
		/// Checks if any player is within distanceMeters of the given position
		/// </summary>
		/// <param name="myPosition"></param>
		/// <param name="distanceMeters"></param>
		/// <returns>true if player found close enough</returns>
		internal static bool IsAnyPlayerNearPosition(Vector3D myPosition, double distanceMeters)
		{
			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);
			var distanceMetersSq = distanceMeters * distanceMeters;
			foreach (var player in players)
			{
				var controlled = player.Controller.ControlledEntity;
				if (controlled == null) continue;
				var position = controlled.Entity.GetPosition();
				var distSq = Vector3D.DistanceSquared(myPosition, position);
				if (distSq < distanceMetersSq)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the nearest player to a position, given a maximum distance they can be away from it.
		/// Those further than the maximum are ignored.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="maxDistanceMeters"></param>
		/// <returns>IMyPlayer or null</returns>
		internal static IMyPlayer GetNearestPlayerToPosition(Vector3D position, double maxDistanceMeters)
		{
			var maxDistanceSq = maxDistanceMeters * maxDistanceMeters;
			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);
			var closestDistSq = double.MaxValue;
			IMyPlayer result = null;

			foreach (var player in players)
			{
				var controlled = player.Controller.ControlledEntity;
				if (controlled == null) continue;
				var distSq = Vector3D.DistanceSquared(position, controlled.Entity.GetPosition());
				if (distSq < closestDistSq && distSq < maxDistanceSq)
				{
					closestDistSq = distSq;
					result = player;
				}
			}
			return result;
		}

        /// <summary>
		/// Gets the nearest player to a position, given a maximum distance they can be away from it.
		/// Those further than the maximum are ignored. returns number of players within maximum
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxDistanceMeters"></param>
        /// <param name="nPlayers"></param>
        /// <returns></returns>
        internal static IMyPlayer GetNearestPlayerToPosition(Vector3D position, double maxDistanceMeters, out int nPlayers)
        {
            var maxDistanceSq = maxDistanceMeters * maxDistanceMeters;
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var closestDistSq = double.MaxValue;
            IMyPlayer result = null;

            nPlayers = 0;
            foreach (var player in players)
            {
                var controlled = player.Controller.ControlledEntity;
                if (controlled == null) continue;
                var distSq = Vector3D.DistanceSquared(position, controlled.Entity.GetPosition());
                if (distSq < maxDistanceSq)
                {
                    nPlayers++;
                    if (distSq < closestDistSq)// && distSq < maxDistanceSq)
                    {
                        closestDistSq = distSq;
                        result = player;
                    }
                }
            }
            return result;
        }

        //We could use MyGamePruningStructure.GetClosestPlanet but it is not recommended
        internal static MyPlanet FindPlanetInGravity(Vector3D vector3D)
		{
			var planets = new HashSet<IMyEntity>();
			MyAPIGateway.Entities.GetEntities(planets, x => x is MyPlanet);
			foreach (var planet in planets)
			{
				if (planet.Components.Get<MyGravityProviderComponent>().IsPositionInRange(vector3D))
				{
					return (MyPlanet) planet;
				}
			}
			return null;
		}

		internal static void SpawnInGravity(Vector3D position, Vector3D naturalGravity, long factionId, 
			string prefabName, string initialBeaconName)
		{
			SpawnInGravity(position, naturalGravity, factionId, prefabName, initialBeaconName,
				MyUtils.GetRandomPerpendicularVector(ref naturalGravity));
		}

		internal static void SpawnInGravity(Vector3D position, Vector3D naturalGravity, long factionId, 
			string prefabName, string initialBeaconName, Vector3D forwards)
		{
			var up = -Vector3D.Normalize(naturalGravity);
			MyAPIGateway.PrefabManager.SpawnPrefab(new List<IMyCubeGrid>(), prefabName, position, forwards, up,
                new Vector3(0f), spawningOptions: SpawningOptions.RotateFirstCockpitTowardsDirection, beaconName: initialBeaconName,
                // We need to set neutral owner due to a bug http://forum.keenswh.com/threads/01-157-stable-myprefabmanager-spawnprefab-doesnt-always-set-owner-id.7389238/
                //  R V14              new Vector3(0f), spawningOptions: SpawningOptions.SetNeutralOwner, beaconName: initialBeaconName,
                ownerId: factionId);
		}

		internal static void AddGpsToAllPlayers(string name, string description, Vector3D coords)
		{
			var gpsSystem = MyAPIGateway.Session.GPS;
			var gps = gpsSystem.Create(name, description, coords, true);
			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);
			foreach (var myPlayer in players)
			{
				gpsSystem.AddGps(myPlayer.IdentityId, gps);
			}
		}

		internal static bool IsPlayerId(long id)
		{
			var players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);
			foreach (var player in players)
			{
				if (id == player.IdentityId)
				{
					return true;
				}
			}
			return false;
		}

        internal static bool IsPlayerUnderground(IMyPlayer player)
        {
            var planet = FindPlanetInGravity(player.GetPosition());
            if (planet == null)
            {
                return false;
            }
            var surface = planet.GetClosestSurfacePointGlobal(player.GetPosition());
            var playerPos = player.GetPosition();
            var belowSurface = Vector3D.Distance(playerPos, surface);

            var playerCenterSq = Vector3D.DistanceSquared(playerPos, planet.PositionComp.WorldAABB.Center);
            var surfaceCenterSq=Vector3D.DistanceSquared(surface, planet.PositionComp.WorldAABB.Center);
            if (surfaceCenterSq < playerCenterSq || belowSurface<0.1) //(player above surface)
                return false;

            // else player is below the OLD surface...  but it may have been mined/destroyed.
//            ModLog.Info("BelowSurface=" + belowSurface);

            var playerNaturalGravity = planet.GetGravityAtPoint(player.GetPosition());
            var vng = playerNaturalGravity;
            vng.Normalize();

            var upAbovePlayer = player.GetPosition() + vng * -(belowSurface +.5);

//            	var vector4 = Color.Yellow.ToVector4();
//            	MySimpleObjectDraw.DrawLine(player.GetPosition(), upAbovePlayer, MyStringId.GetOrCompute("Square"), ref vector4, 0.04f);

            IHitInfo hitInfo;
            if (MyAPIGateway.Physics.CastRay(upAbovePlayer, player.GetPosition(), out hitInfo))
            {
                var entity = hitInfo.HitEntity;
                if (entity == null)
                {
                    return false;
                }
//                ModLog.Info("Entity=" + entity.ToString());
                return !(entity is IMyCharacter);
            }
            return false;
        }

        internal static bool IsPlayerUnderCover(IMyPlayer player)
		{
			var planet = FindPlanetInGravity(player.GetPosition());
			if (planet == null)
			{
				return false;
			}
//            var surface=planet.GetClosestSurfacePointGlobal(player.GetPosition());
			var playerNaturalGravity = planet.GetGravityAtPoint(player.GetPosition());
			var upAbovePlayer = player.GetPosition() + playerNaturalGravity * -5.0f;
			//	var vector4 = Color.Yellow.ToVector4();
			//	MySimpleObjectDraw.DrawLine(player.GetPosition(), upAbovePlayer, MyStringId.GetOrCompute("Square"), ref vector4, 0.04f);
			IHitInfo hitInfo;
			if (MyAPIGateway.Physics.CastRay(upAbovePlayer, player.GetPosition(), out hitInfo))
			{
				var entity = hitInfo.HitEntity;
				if (entity == null)
				{
					return false;
				}
				return !(entity is IMyCharacter);
			}
			return false;
		}

		internal static void MakePeaceBetweenFactions(string tag, string tag2)
		{
			var faction1 = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
			if (faction1 == null)
			{
				ModLog.Error("Can't find faction: " + tag);
				return;
			}

			var faction2 = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag2);
			if (faction2 == null)
			{
				ModLog.Error("Can't find faction: " + tag2);
				return;
			}
			MyAPIGateway.Session.Factions.SendPeaceRequest(faction1.FactionId, faction2.FactionId);
			MyAPIGateway.Session.Factions.AcceptPeace(faction2.FactionId, faction1.FactionId);

//            MyAPIGateway.Session.Factions.DeclareWar(faction2.FactionId, faction1.FactionId);

        }

        internal static void DeclareWarBetweenFactions(string tag, string tag2)
        {
            var faction1 = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
            if (faction1 == null)
            {
                ModLog.Error("Can't find faction: " + tag);
                return;
            }

            var faction2 = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag2);
            if (faction2 == null)
            {
                ModLog.Error("Can't find faction: " + tag2);
                return;
            }

            MyAPIGateway.Session.Factions.DeclareWar(faction2.FactionId, faction1.FactionId);
        }


        internal static void RemoveFaction(string tag)
        {
            var faction1 = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
            if (faction1 == null)
            {
//                ModLog.Error("Can't find faction: " + tag);
                return;
            }

            MyAPIGateway.Session.Factions.RemoveFaction(faction1.FactionId);
        }




        internal static void SetPlayerReputation(long playerID, string toFactionTag, int reputation)
        {
            var faction1 = MyAPIGateway.Session.Factions.TryGetFactionByTag(toFactionTag);
            if (faction1 == null)
            {
                ModLog.Error("Can't find faction: " + toFactionTag);
                return;
            }
            MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(playerID, faction1.FactionId, reputation);
        }

        internal static void SetAllPlayerReputation(string toFactionTag, int reputation)
        {
            var faction2 = MyAPIGateway.Session.Factions.TryGetFactionByTag(toFactionTag);
            if (faction2 == null)
            {
                ModLog.Error("Can't find faction: " + toFactionTag);
                return;
            }

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(player.IdentityId, faction2.FactionId, reputation);
            }
        }

        /// <summary>
        /// Puts LOCAL player into faction.  Safe to call even if no local player
        /// </summary>
        /// <param name="tag"></param>
        internal static void PutPlayerIntoFaction(string tag)
		{
			var faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
			if (faction == null)
			{
				ModLog.Error("Can't find faction: " + tag);
				return;
			}

			var player = MyAPIGateway.Session.Player;
			if (player == null)
			{
				return;
			}

			MyAPIGateway.Session.Factions.SendJoinRequest(faction.FactionId, player.IdentityId);
			MyAPIGateway.Session.Factions.AcceptJoin(faction.FactionId, player.IdentityId);
		}

        internal static void PlaceItemIntoCargo(MyInventory inventory, MyObjectBuilder_Base cargoitem, int amount)
        {
            if (inventory == null) return;
            bool bPlaced = false;
            do
            {
                bPlaced = inventory.AddItems(amount, cargoitem);
                if (!bPlaced)
                {
                    //                            MyLog.Default.WriteLine("LoadCargo: Does not fit-" + amount.ToString() + " "+cargo.GetDisplayName());
                    amount /= 2; // reduce size until it fits
                }
                if (amount < 3) bPlaced = true; // force to true if it gets too small
            } while (!bPlaced);
        }

    }

}