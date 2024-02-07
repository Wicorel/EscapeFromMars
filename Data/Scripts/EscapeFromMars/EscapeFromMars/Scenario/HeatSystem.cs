using System;
using System.Collections.Generic;
using Duckroll;
using Sandbox.ModAPI;
using VRage.Game;

namespace EscapeFromMars
{
	internal class HeatSystem : NpcGroupArrivalObserver
	{
		private static readonly IList<ShipSize> EmptyList = new List<ShipSize>().AsReadOnly();
		private readonly Random random = new Random();
		public int HeatLevel { get; set; }

        // V26
        public int HeatDifficulty { get; set; }
        public bool MultiplayerScaling { get; set; }

		internal HeatSystem(int initialHeat, int initialDifficulty)
		{
			HeatLevel = initialHeat;
            HeatDifficulty = initialDifficulty;
		}

		internal ShipSize GenerateCargoShipSize()
		{
			NewShipGroupLaunched();

            // TODO: remove hardcoding and make data driven.

			if (HeatLevel < 10) // Very early game, make them all small
			{
				return ShipSize.Small;
			}

			var d100 = random.Next(0, 100); // Random between 0 and 99

			if (HeatLevel < 50) // Mediums start to appear after 10
			{
				if (d100 > 97) // Tiny 2% chance of giant delivery!
				{
					return ShipSize.Large;
				}

				return d100 < HeatLevel ? ShipSize.Medium : ShipSize.Small;
			}

			if (HeatLevel < 100)  // Largs start to appear after 50
			{
				if (d100 < 10)
				{
					return ShipSize.Small;
				}
				return d100 < 70 ? ShipSize.Medium : ShipSize.Large;
			}

			// End game - player killed 100+ convoys!
//			if (d100 < 10)
//			{
//				return ShipSize.Small;
//			}
			return d100 < 40 ? ShipSize.Medium : ShipSize.Large;
		}

		// Contract: Should return a list of SMALLEST SIZE FIRST. The list should be mutable if it is not the empty list.
        // TODO: make this not hardcoded so it can be more finely controlled based on conditions such as difficulty settings, # players, faction info, etc.
		internal IList<ShipSize> GenerateEscortSpecs()
		{
			var d100 = random.Next(0, 100); // Random between 0 and 99
			if (HeatLevel < 10)
			{
				return d100 > 90 ? new List<ShipSize> {ShipSize.Small} : EmptyList;
			}

			if (HeatLevel < 20)
			{
				if (d100 > 90)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small};
				}
				return d100 > 70 ? new List<ShipSize> {ShipSize.Small} : EmptyList;
			}

			if (HeatLevel < 40)
			{
				if (d100 > 90)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small, ShipSize.Medium};
				}
				if (d100 > 80)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small};
				}
				return d100 > 30 ? new List<ShipSize> {ShipSize.Small} : EmptyList;
			}

			if (HeatLevel < 60)
			{
				if (d100 > 90)
				{
					return new List<ShipSize> {ShipSize.Large};
				}
				if (d100 > 80)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small, ShipSize.Medium};
				}
				if (d100 > 70)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small, ShipSize.Small, ShipSize.Small};
				}
				return d100 > 20 ? new List<ShipSize> {ShipSize.Small} : EmptyList;
			}

			if (HeatLevel < 80)
			{
				if (d100 > 90)
				{
					return new List<ShipSize> {ShipSize.Medium, ShipSize.Medium, ShipSize.Large};
				}
				if (d100 > 80)
				{
					return new List<ShipSize> {ShipSize.Small, ShipSize.Small, ShipSize.Small, ShipSize.Small, ShipSize.Medium};
				}
				return d100 > 60 ? new List<ShipSize> {ShipSize.Medium, ShipSize.Medium} : CreateSmallShips(d100 / 10);
			}

			// End game values
			if (d100 > 90)
			{
				return new List<ShipSize> {ShipSize.Large, ShipSize.Large, ShipSize.Large};
			}
			if (d100 > 80)
			{
				return new List<ShipSize> {ShipSize.Medium, ShipSize.Medium, ShipSize.Medium, ShipSize.Large};
			}
			return d100 > 60 ? new List<ShipSize> {ShipSize.Medium, ShipSize.Medium, ShipSize.Large} : CreateSmallShips(d100 / 10);
		}

		private static IList<ShipSize> CreateSmallShips(int smallShipsToSpawn)
		{
			var shipsToSpawn = new List<ShipSize>();
			for (var i = 0; i < smallShipsToSpawn; i++)
			{
				shipsToSpawn.Add(ShipSize.Small);
			}
			return shipsToSpawn;
		}

		internal ShipSize GenerateBackupShipSize()
		{
			NewShipGroupLaunched();
			if (HeatLevel < 20)
			{
				return ShipSize.Small;
			}
			return HeatLevel < 50 ? ShipSize.Medium : ShipSize.Large;
		}

		private void NewShipGroupLaunched()
		{
            HeatLevel = HeatLevel + 1 * HeatDifficulty;
            ModLog.DebugInfo("Ships Launched: Heat Level: " + HeatLevel);
		}

		public void GroupArrivedIntact()
		{
            HeatLevel = HeatLevel - 1 * HeatDifficulty;
            ModLog.DebugInfo("Ships Arrived: Heat Level: " + HeatLevel);
		}
        public void BackupDisabled()
        {
            HeatLevel = HeatLevel + 1 * HeatDifficulty;
            ModLog.DebugInfo("Backup Disabled: Heat Level: " + HeatLevel);
 //           ModLog.Info("Backup Disabled: Heat Level: " + HeatLevel);
        }

        public void BaseCaptured()
        { 
            // a base was captured by players..  increase heat
            HeatLevel = HeatLevel + 5 * HeatDifficulty;
        }
	}
}