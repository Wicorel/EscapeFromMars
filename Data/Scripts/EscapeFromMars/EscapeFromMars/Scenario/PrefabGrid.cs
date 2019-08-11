using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace EscapeFromMars
{
	internal class PrefabGrid
	{
		private const string Unitialised = "UNITIALISED";
		private const string EscortRole = Unitialised + "_ESCORT";
		private const string CargoRole = Unitialised + "_CARGO";
		private const string EscortAir = EscortRole + "_AIR";
		private const string EscortGround = EscortRole + "_GROUND";
		private const string CargoAir = CargoRole + "_AIR";
		private const string CargoGround = CargoRole + "_GROUND";
		private const string BackupAir = Unitialised + "_BACKUP_AIR";

        private const string Bomb = Unitialised + "_BOMB";

        private static readonly PrefabGrid LightAirTransport = new PrefabGrid("Envoy", UnitType.Air, UnitRole.Delivery);

		private static readonly PrefabGrid MediumAirTransport = new PrefabGrid("Medium GCorp Transport", UnitType.Air,
			UnitRole.Delivery);

		private static readonly PrefabGrid LargeAirTransport = new PrefabGrid("Large GCorp Transport", UnitType.Air,
			UnitRole.Delivery);

		private static readonly PrefabGrid LightAirEscort = new PrefabGrid("Combat Drone", UnitType.Air, UnitRole.Escort);

		private static readonly PrefabGrid MediumAirEscort = new PrefabGrid("Medium GCorp Combat Flyer", UnitType.Air, UnitRole.Escort);

		private static readonly PrefabGrid RocketAirEscort = new PrefabGrid("Medium GCorp Missile Flyer", UnitType.Air, UnitRole.Escort);

		private static readonly PrefabGrid LightGroundTransport = new PrefabGrid("Small Hover Transport", UnitType.Ground,
			UnitRole.Delivery);

		private static readonly PrefabGrid MediumGroundTransport = new PrefabGrid("Medium Hover Transport", UnitType.Ground,
			UnitRole.Delivery);

		private static readonly PrefabGrid LargeGroundTransport = new PrefabGrid("Large Hover Transport", UnitType.Ground,
				UnitRole.Delivery);

		private static readonly PrefabGrid LightGroundEscort = new PrefabGrid("Light Hover Drone", UnitType.Ground,
			UnitRole.Escort);

		private static readonly PrefabGrid MediumGroundEscort = new PrefabGrid("Medium Hover Drone", UnitType.Ground,
			UnitRole.Escort);

		private static readonly PrefabGrid RocketGroundEscort = new PrefabGrid("Medium Rocket Hover Drone", UnitType.Ground,
			UnitRole.Escort);

		private static readonly PrefabGrid LightAirBackup = new PrefabGrid("Combat Drone", UnitType.Air, UnitRole.Backup);

		private static readonly PrefabGrid MediumAirBackup = new PrefabGrid("Medium GCorp Combat Flyer", UnitType.Air, UnitRole.Backup);

		private static readonly PrefabGrid RocketAirBackup = new PrefabGrid("Medium GCorp Missile Flyer", UnitType.Air, UnitRole.Backup);

        //V26
        private static readonly PrefabGrid BombDefense = new PrefabGrid("GCorp Large Bomb", UnitType.Air, UnitRole.Bomb);


        // Define more prefabs here...


        internal string PrefabName { get; }
		internal UnitType UnitType { get; }
		internal UnitRole UnitRole { get; }
		internal string InitialBeaconName { get; }

		private PrefabGrid(string prefabName, UnitType unitType, UnitRole unitRole)
		{
			PrefabName = prefabName;
			UnitType = unitType;
			UnitRole = unitRole;
			InitialBeaconName = GetBeaconName(unitRole, unitType);
		}

		private static string GetBeaconName(UnitRole unitRole, UnitType unitType)
		{
			switch (unitRole)
			{
				case UnitRole.Delivery:
					return unitType == UnitType.Air ? CargoAir : CargoGround;
				case UnitRole.Escort:
					return unitType == UnitType.Air ? EscortAir : EscortGround;
				case UnitRole.Backup:
					return BackupAir; // Currently all backup is air, ground is tricky to place at random
                case UnitRole.Bomb: // V26
                    return Bomb;
				default:
					throw new ArgumentException(unitRole + " not recognised!");
			}
		}

		internal static PrefabGrid GetBackup(ShipSize size)
		{
			switch (size)
			{
				case ShipSize.Large:
					return RocketAirBackup;
				case ShipSize.Medium:
					return MediumAirBackup;
				case ShipSize.Small:
					return LightAirBackup;
				default:
					throw new ArgumentException(size + " air backup not found!");
			}
		}

        internal static PrefabGrid GetBomb()
        {
            return BombDefense;
        }

		internal static PrefabGrid GetAirTransport(ShipSize size)
		{
			switch (size)
			{
				case ShipSize.Large:
					return LargeAirTransport;
				case ShipSize.Medium:
					return MediumAirTransport;
				case ShipSize.Small:
					return LightAirTransport;
				default:
					throw new ArgumentException(size + " air transport not found!");
			}
		}

		internal static PrefabGrid GetGroundTransport(ShipSize size)
		{
			switch (size)
			{
				case ShipSize.Large:
					return LargeGroundTransport;
				case ShipSize.Medium:
					return MediumGroundTransport;
				case ShipSize.Small:
					return LightGroundTransport;
				default:
					throw new ArgumentException(size + " ground transport not found!");
			}
		}

		internal static PrefabGrid GetEscort(UnitType unitType, ShipSize shipSize)
		{
			return unitType == UnitType.Air ? GetAirEscort(shipSize) : GetGroundEscort(shipSize);
		}

		private static PrefabGrid GetAirEscort(ShipSize size)
		{
			switch (size)
			{
				case ShipSize.Large:
					return RocketAirEscort;
				case ShipSize.Medium:
					return MediumAirEscort;
				case ShipSize.Small:
					return LightAirEscort;
				default:
					throw new ArgumentException(size + " air escort not found!");
			}
		}

		private static PrefabGrid GetGroundEscort(ShipSize size)
		{
			switch (size)
			{
				case ShipSize.Large:
					return RocketGroundEscort;
				case ShipSize.Medium:
					return MediumGroundEscort;
				case ShipSize.Small:
					return LightGroundEscort;
				default:
					throw new ArgumentException(size + " ground escort not found!");
			}
		}

		internal static RoleAndUnitType? GetRoleAndUnitType(IMyCubeGrid grid)
		{
			var slimBlocks = new List<IMySlimBlock>();
			grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyBeacon);
			foreach (var slim in slimBlocks)
			{
				var beacon = slim.FatBlock as IMyBeacon;
				var beaconName = beacon.CustomName;
				if (beaconName.Contains(EscortAir))
				{
					return new RoleAndUnitType {UnitRole = UnitRole.Escort, UnitType = UnitType.Air};
				}
				if (beaconName.Contains(EscortGround))
				{
					return new RoleAndUnitType {UnitRole = UnitRole.Escort, UnitType = UnitType.Ground};
				}
				if (beaconName.Contains(CargoAir))
				{
					return new RoleAndUnitType {UnitRole = UnitRole.Delivery, UnitType = UnitType.Air};
				}
				if (beaconName.Contains(CargoGround))
				{
					return new RoleAndUnitType {UnitRole = UnitRole.Delivery, UnitType = UnitType.Ground};
				}
                if (beaconName.Contains(BackupAir))
                {
                    return new RoleAndUnitType { UnitRole = UnitRole.Backup, UnitType = UnitType.Air };
                }
                //V26
                if (beaconName.Contains(Bomb))
                {
                    return new RoleAndUnitType { UnitRole = UnitRole.Bomb, UnitType = UnitType.Air };
                }

            }
            grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyWarhead);
            if (slimBlocks.Count>0)
            {
                // if it has warheads (and no beacon defining else) assume it's a bomb
                return new RoleAndUnitType { UnitRole = UnitRole.Bomb, UnitType = UnitType.Air };
            }
            return null; // Not one of ours perhaps?
		}
	}
}