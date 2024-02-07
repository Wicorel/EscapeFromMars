﻿using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using Duckroll;

namespace EscapeFromMars
{
	public class TurretManager : ModSystemRapidUpdatable
	{
		private readonly List<ITurret> turrets = new List<ITurret>();

		public override void GridInitialising(IMyCubeGrid grid)
		{
			if (grid.IsStatic)
			{
                // change in SE 1.186.  Small grids in voxels are no longer 'static'
//				return;
			}
            //if (grid.IsAliveAndGCorpControlled()) ?? do we need this for turrets?

            var azimuthRotors = FindRotorsWithSubgrid(grid, "TURRET_ROTOR");
            if (azimuthRotors.Count > 0)
			{
//                ModLog.Info("Found Turret Rotor on: " + grid.CustomName);
                var azimuthRotor = azimuthRotors[0];
				var subGrid = azimuthRotor.TopGrid;
				var remoteControl = FindFirstRemoteControl(subGrid);
				if (remoteControl != null 
                    // TESTING: TURN THIS BACK ON!
                    && remoteControl.IsControlledByFaction("GCORP")
                    )
				{
					var elevationRotors = FindRotorsWithSubgrid(azimuthRotor.TopGrid, "TURRET_ROTOR_ELEVATION");
					var weapons = new List<IMyUserControllableGun>();
					foreach (var elevationRotor in elevationRotors)
					{
// Broken/removed SE 1.192.006                        weapons.AddList(elevationRotor.TopGrid.FindNonTurretWeapons());
                        var subGuns = elevationRotor.TopGrid.FindNonTurretWeapons();
                        foreach (var gun in subGuns)
                        {
                            weapons.Add(gun);
                        }
					}

					if (elevationRotors.Count == 0 || weapons.Count == 0)
					{
//                        ModLog.Info(" Found Horz Turret Rotor on: " + grid.CustomName);
                        turrets.Add(new TurretSingleAxis(remoteControl, subGrid, azimuthRotor));
					}
					else
					{
//                        ModLog.Info("Found Dual Turret Rotor on: " + grid.CustomName);
                        turrets.Add(new TurretDualAxis(remoteControl, subGrid, azimuthRotor, elevationRotors, weapons));
					}
				}
                else
                {
//                    ModLog.Info(" No remote Control found; ignoring. " + grid.CustomName);
                }
			}
		}

		internal static List<IMyMotorStator> FindRotorsWithSubgrid(IMyCubeGrid parentGrid, string nameMatch)
		{
			var rotors = new List<IMyMotorStator>();
			var slimBlocks = new List<IMySlimBlock>();
			parentGrid.GetBlocks(slimBlocks, b => b.FatBlock is IMyMotorStator);
			foreach (var slim in slimBlocks)
			{
				var rotorFound = slim.FatBlock as IMyMotorStator;
				var subGrid = rotorFound.TopGrid;
				if (subGrid != null && rotorFound.CustomName.Contains(nameMatch))
				{
					rotors.Add(rotorFound);
				}
			}

			return rotors;
		}

		private static IMyRemoteControl FindFirstRemoteControl(IMyCubeGrid grid)
		{
			var slimBlocks = new List<IMySlimBlock>();
			grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
			foreach (var slim in slimBlocks)
			{
				var remoteControl = slim.FatBlock as IMyRemoteControl;
				return remoteControl;
			}
			return null;
		}

		public override void Update1()
		{
			foreach (var turret in turrets)
			{
				turret.Update1();
			}
		}

        bool bShownTurretCount = false;
        int turretshowncount = 0;
		public override void Update60()
		{
/*
            if(!bShownTurretCount)// && turretshowncount < 5)
            {
                turretshowncount++;
                if(turretshowncount >= 4)
                {
                    bShownTurretCount = true;
                    MyAPIGateway.Utilities.ShowNotification("#rotor Turrest=: " + turrets.Count, 2000, MyFontEnum.DarkBlue);
                }
            }
            */

			// Lets turrets check around for enemies, but they mostly sleep until needed
			foreach (var turret in turrets)
			{
				turret.Update60();
			}
		}
	}
}