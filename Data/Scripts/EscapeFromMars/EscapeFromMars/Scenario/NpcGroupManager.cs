using System;
using System.Collections.Generic;
using System.Linq;
using Duckroll;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using VRage.Utils;
using VRage.Game;

namespace EscapeFromMars
{
	public class NpcGroupManager : ModSystemUpdatable
	{
		private Vector3D airConvoyDestinationPosition;
		private Vector3D groundConvoyDestinationPosition;

		private readonly HeatSystem heatSystem;
		private readonly QueuedAudioSystem audioSystem;
		private readonly BaseManager baseManager;
		private readonly ConvoySpawner convoySpawner;
		private readonly Random random = new Random();
		private readonly List<NpcGroup> npcGroups = new List<NpcGroup>();
		private readonly TimeSpan convoyExpiryTime = new TimeSpan(1, 10, 0);
		private readonly Queue<IMyCubeGrid> unitialisedNewGrids = new Queue<IMyCubeGrid>();

		private readonly Dictionary<long, NpcGroupSaveData> restoredNpcGroupData = new Dictionary<long, NpcGroupSaveData>();
		private readonly Dictionary<long, long> restoredEscortAssignments = new Dictionary<long, long>();
		readonly Dictionary<long, Convoy> restoredConvoys = new Dictionary<long, Convoy>();
		readonly List<IMyCubeGrid> possibleEscorts = new List<IMyCubeGrid>();

        //v31 
        private int modBuildWhenLastSaved;
        private string EscortName = "Convoy Escort"; // loaded from mytexts
        private bool bOldRemovals = false;
        private static float ConvoySpeed = 10f; // needs to be 10f for release

        private string InvestigatingBackupCall = " Investigating Backup Call"; // loaded from mytexts
        private string ConvoyRecalled = "Convoy Recalled"; // loaded my mytexts

        internal NpcGroupManager(int modBuildWhenSaved, HeatSystem heatSystem, QueuedAudioSystem audioSystem, BaseManager baseManager,
			ConvoySpawner convoySpawner)
		{
			this.heatSystem = heatSystem;
			this.audioSystem = audioSystem;
			this.baseManager = baseManager;
			this.convoySpawner = convoySpawner;
            modBuildWhenLastSaved = modBuildWhenSaved;

			MyAPIGateway.Entities.OnEntityAdd += NewEntityEvent;

            EscortName = VRage.MyTexts.Get(MyStringId.TryGet("EscortName")).ToString();

            var InvestigatingBackupCallID = MyStringId.TryGet("InvestigatingBackupCall");
            InvestigatingBackupCall = VRage.MyTexts.Get(InvestigatingBackupCallID).ToString();

            ConvoyRecalled = VRage.MyTexts.Get(MyStringId.TryGet("ConvoyRecalled")).ToString();

        }

        public void SetBuildWhenSaved(int setto)
        {
            modBuildWhenLastSaved = setto;
        }

		// We have to listen for new entities because they are not returned by the spawning code.
		// The faction of the blocks also isn't set by this point, neither is the beacon name, so there's nothing much 
		// we can do other than queue it up to check next update
		internal void NewEntityEvent(IMyEntity ent)
		{
			if (!(ent is IMyCubeGrid)) return;
			var grid = (IMyCubeGrid) ent;
			unitialisedNewGrids.Enqueue(grid);
		}

        public bool bInitialInit=true;

		public override void GridInitialising(IMyCubeGrid grid)
		{
			if (!grid.IsControlledByFaction("GCORP"))
			{
				return;
			}

//            if (!bInitialInit) return;

			if (grid.IsStatic)
			{
				OfferPotentialDestination(grid);
			}
			else
			{
				OfferPotentialNpcShip(grid);
			}
		}

		public override void AllGridsInitialised()
		{
			// Loop through again to find escorts of the npcGroups we made above
			// We can't do this until the end when all group leaders have been found!
			foreach (var grid in possibleEscorts)
			{
				long deliveryShipId;
				if (restoredEscortAssignments.TryGetValue(grid.EntityId, out deliveryShipId))
				{
					Convoy convoy;
					NpcGroupSaveData npcGroupSaveData;
					if (restoredConvoys.TryGetValue(deliveryShipId, out convoy) &&
					    restoredNpcGroupData.TryGetValue(deliveryShipId, out npcGroupSaveData))
					{
						convoy.ReConnectEscort(grid, npcGroupSaveData);
					}
				}
			}

			// We don't need these afer finishing restores
			restoredNpcGroupData.Clear();
			restoredEscortAssignments.Clear();
            if(bInitialInit && bOldRemovals)
            {
                MyVisualScriptLogicProvider.SendChatMessage("NOTE: old convoys removed", "Wicorel", 0, MyFontEnum.DarkBlue);
            }
            bInitialInit = false; // we've already done it.
		}

		private void OfferPotentialDestination(IMyCubeGrid grid)
		{
//            ModLog.Info("Potential Destination:" + grid.CustomName);

			foreach (var remoteControl in grid.GetTerminalBlocksOfType<IMyRemoteControl>())
			{
//                ModLog.Info(" RC:" + remoteControl.CustomName);
                if (!remoteControl.IsControlledByFaction("GCORP"))
				{
					continue;
				}

				if (remoteControl.CustomName.Contains("AIR_DELIVERY_DESTINATION"))
				{
					// If the destination has not yet been setup (ie restored from a save), do so now
					if (Vector3D.IsZero(airConvoyDestinationPosition))
					{
//                        ModLog.Info(" Setting airConvoyDestinationPosition:" + remoteControl.GetPosition().ToString());
						airConvoyDestinationPosition = remoteControl.GetPosition() + remoteControl.GetNaturalGravity() * -23f;
//                        ModLog.Info(" Gravity:" + remoteControl.GetNaturalGravity());
//                        ModLog.Info(" Set airConvoyDestinationPosition:" + airConvoyDestinationPosition.ToString());
                    }
                    else
                    {
//                        ModLog.Info(" Using Saved airConvoyDestinationPosition:" + airConvoyDestinationPosition.ToString());
                    }
                }
                else if (remoteControl.CustomName.Contains("GROUND_DELIVERY_DESTINATION"))
				{
					//basesToInit.Add(remoteControl); // Is in the same place as the air destination, so disabled this
					// If the destination has not yet been setup (ie restored from a save), do so now
					if (Vector3D.IsZero(groundConvoyDestinationPosition))
					{
//                        ModLog.Info(" Setting groundConvoyDestinationPosition:" + remoteControl.GetPosition().ToString());
                        groundConvoyDestinationPosition = remoteControl.GetPosition() + remoteControl.GetNaturalGravity() * -0.3f;
                        //                      // 3M above for Ground Destination
//                        ModLog.Info(" Gravity:" + remoteControl.GetNaturalGravity());
//                        ModLog.Info(" Set groundConvoyDestinationPosition:" + groundConvoyDestinationPosition.ToString());
                    }
                    else
                    {
//                        ModLog.Info(" using Saved groundConvoyDestinationPosition:" + groundConvoyDestinationPosition.ToString());
                    }
                }
			}
//            ModLog.Info("EO:Potential Destination:" + grid.CustomName);
        }

        private void OfferPotentialNpcShip(IMyCubeGrid grid)
        {
            //            ModLog.Info("Potentential NPC:" + grid.CustomName);
            NpcGroupSaveData npcGroupSaveData;
            if (restoredNpcGroupData.TryGetValue(grid.EntityId, out npcGroupSaveData))
            {
                //                ModLog.Info(" Potentential NPC: Found in restored NPC Group data:" + npcGroupSaveData.NpcGroupType.ToString() + " Type="+ npcGroupSaveData.LeaderUnitType.ToString());
                if (npcGroupSaveData.NpcGroupType == NpcGroupType.Backup)
                {
                    npcGroups.Add(new BackupGroup(npcGroupSaveData.State, npcGroupSaveData.GroupDestination,
                        grid, heatSystem, audioSystem, DateTime.FromBinary(npcGroupSaveData.SpawnTime)));
                }
                else // Must be convoy
                {
                    if (modBuildWhenLastSaved > 30 || !bInitialInit)
                    {
                        restoredConvoys.Add(grid.EntityId,
                            RegisterConvoy(grid, npcGroupSaveData.State, npcGroupSaveData.LeaderUnitType,
                                npcGroupSaveData.GroupDestination, DateTime.FromBinary(npcGroupSaveData.SpawnTime)));
                    }
                    else
                    {
                        // else old drones with scripts that don't work on 1.193.100

                        if (grid.IsControlledByFaction("GCORP")) // see npcgroup.AttemptDespawn()
                        {
                            ModLog.Info("save build#:" + modBuildWhenLastSaved.ToString() + " Initial=" + bInitialInit.ToString());
                            ModLog.Info("Removing dead drone Grid:" + grid.CustomName);
                            bOldRemovals = true;
                            grid.CloseAll();
                        }
                    }

                }
            }
            else
            {
                //                ModLog.Info(" Potentential NPC: NOT in restored NPC Group data:" + grid.CustomName);
                if (modBuildWhenLastSaved > 30 || !bInitialInit)
                {
                    possibleEscorts.Add(grid);
                }
                else
                {
                    // Need better discernment to not delete placed G-Corp grids like G-Corp AP Turret and GCorp Experimental Mech
                    if (grid.IsControlledByFaction("GCORP")) // see npcgroup.AttemptDespawn()
                    {
                        var slimBlocks = new List<IMySlimBlock>();
                        grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyBeacon);
                        foreach (var slim in slimBlocks)
                        {
                            var fb = slim.FatBlock as IMyBeacon;
                            if(fb.CustomName.Contains(EscortName))
                            {
                                ModLog.Info("Removing dead escort drone Grid:" + grid.CustomName);
                                bOldRemovals = true;
                                grid.CloseAll();
                                break;
                            }
                        }

                    }
                }
            }
        }

		internal NpcGroup FindNearestJoinableNpcGroup(Vector3D position, UnitType unitType)
		{
			NpcGroup nearestGroup = null;
			var closestDistance = double.MaxValue;
			foreach (var group in npcGroups)
			{
				if (group.IsJoinable(unitType))
				{
					var distSquared = Vector3D.DistanceSquared(group.GetPosition(), position);
					if (distSquared < closestDistance)
					{
						closestDistance = distSquared;
						nearestGroup = group;
					}
				}
			}
			return nearestGroup;
		}

		internal List<NpcGroupSaveData> GetSaveData()
		{
			var npcGroupSaveDatas = new List<NpcGroupSaveData>();
			foreach (var group in npcGroups)
			{
				npcGroupSaveDatas.Add(group.GetSaveData());
			}
			return npcGroupSaveDatas;
		}

		internal void LoadSaveData(List<NpcGroupSaveData> saveDatas)
		{
			foreach (var convoySaveData in saveDatas) // Easier for later if these are in a map!
			{
				restoredNpcGroupData.Add(convoySaveData.GroupId, convoySaveData);
				foreach (var escortSaveData in convoySaveData.Escorts)
				{
					restoredEscortAssignments.Add(escortSaveData.EscortEntityId, convoySaveData.GroupId);
				}
			}
		}

		public override void Update60()
		{
			UpdateGroups(); // Disband or update existing, active groups
			GiveOrdersToUnassignedShips(); // Find ships that have been spawned but not set in a direction / mission
            RemoveInactiveGroups();
		}

        private void RemoveInactiveGroups()
        {
            foreach (var group in npcGroups)
            {
                if(group.GroupState==NpcGroupState.Inactive)
                {
                    // should be hidden in Convoy class, but need access to convoySpawner class to respawn.
                    if (group.groupType == NpcGroupType.Convoy)
                    {
                        MyVisualScriptLogicProvider.SendChatMessage(ConvoyRecalled, EscapeFromMars.Speaker.GCorp.Name, 0, EscapeFromMars.Speaker.GCorp.Font);

                        convoySpawner.SpawnConvoy(); // spawn a replacement one.
                        group.GroupState = NpcGroupState.Disbanding;
                    }
                }
            }
        }

        private void UpdateGroups()
		{
			foreach (var group in npcGroups.Reverse<NpcGroup>()) // Go backwards so we can remove part way through if needed
			{
				if (group.IsDisbanded())
				{
					npcGroups.Remove(group);
				}
				else
				{
					@group.Update();
				}
			}
		}

		private void GiveOrdersToUnassignedShips()
		{
            bool bFoundBackup = false;
			while (unitialisedNewGrids.Count > 0)
			{
				var grid = unitialisedNewGrids.Dequeue();
				if (!grid.IsControlledByFaction("GCORP"))
				{
					continue;
				}

				var roleAndUnitType = PrefabGrid.GetRoleAndUnitType(grid);
				if (roleAndUnitType == null)
				{
                    // V26 debug
                    ModLog.Info("Discarding grid because no role found");
					continue;
				}

				var unitType = roleAndUnitType.Value.UnitType;

				switch (roleAndUnitType.Value.UnitRole)
				{
					case UnitRole.Delivery:
						var cargoType = CargoType.GenerateRandomCargo(random);
						LoadCargo(grid, cargoType);

                        if (cargoType.subtypeName == "SteelPlate")
                            audioSystem.PlayAudioRandomChance(0.1,AudioClip.SteelPlateConvoyDispatched, AudioClip.ConvoyDispatched1);
                        else if (cargoType.subtypeName == "MetalGrid")
                            audioSystem.PlayAudioRandomChance(1,AudioClip.MetalGridConvoyDispatched);
                        else if (cargoType.subtypeName == "Construction")
                            audioSystem.PlayAudioRandomChance(0.1,AudioClip.ConstructionConvoyDispatched);
                        else if (cargoType.subtypeName == "InteriorPlate")
                            audioSystem.PlayAudioRandomChance(0.1,AudioClip.InteriorPlateConvoyDispatched);
                        else if (cargoType.subtypeName == "Girder")
                            audioSystem.PlayAudioRandomChance(0.1,AudioClip.GirderConvoyDispatched);
                        else if (cargoType.subtypeName == "SmallTube")
                            audioSystem.PlayAudioRandomChance(0.1,AudioClip.SmallTubeConvoyDispatched);
                        else if (cargoType.subtypeName == "LargeTube")
                            audioSystem.PlayAudioRandomChance(0.2,AudioClip.LargeTubeConvoyDispatched);
                        else if (cargoType.subtypeName == "Motor")
                            audioSystem.PlayAudioRandomChance(0.75,AudioClip.MotorConvoyDispatched);
                        else if (cargoType.subtypeName == "Display")
                            audioSystem.PlayAudioRandomChance(0.2,AudioClip.DisplayConvoyDispatched);
                        else if (cargoType.subtypeName == "BulletproofGlass")
                            audioSystem.PlayAudioRandomChance(0.3,AudioClip.BulletproofGlassConvoyDispatched);
                        else if (cargoType.subtypeName == "Computer")
                            audioSystem.PlayAudioRandomChance(0.2,AudioClip.ComputerConvoyDispatched);
                        else if (cargoType.subtypeName == "Reactor")
                            audioSystem.PlayAudioRandomChance(0.75,AudioClip.ReactorConvoyDispatched);
                        else if (cargoType.subtypeName == "Medical")
                            audioSystem.PlayAudioRandomChance(0.7,AudioClip.MedicalConvoyDispatched);
                        else if (cargoType.subtypeName == "RadioCommunication")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.RadioCommunicationConvoyDispatched);
                        else if (cargoType.subtypeName == "Explosives")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.ExplosivesConvoyDispatched);
                        else if (cargoType.subtypeName == "SolarCell")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.SolarCellConvoyDispatched);
                        else if (cargoType.subtypeName == "PowerCell")
                            audioSystem.PlayAudioRandomChance(0.75,AudioClip.PowerCellConvoyDispatched);
                        else if (cargoType.subtypeName == "NATO_5p56x45mm")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.NATO_5p56x45mmConvoyDispatched);
                        else if (cargoType.subtypeName == "NATO_25x184mm")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.NATO25x184mmConvoyDispatched);
                        else if (cargoType.subtypeName == "Missile200mm")
                            audioSystem.PlayAudioRandomChance(0.5,AudioClip.Missile200mmConvoyDispatched);
                        else if (cargoType.subtypeName == "Uranium")
                            audioSystem.PlayAudioRandomChance(1,AudioClip.UraniumConvoyDispatched);
                        else  // we don't know what it is..
                             audioSystem.PlayAudioRandomChance(0.1, AudioClip.ConvoyDispatched1, AudioClip.ConvoyDispatched2, AudioClip.ConvoyDispatched3);

                        string sPrefix = "T";
                        if (unitType == UnitType.Air) sPrefix += "A";
                        else sPrefix += "G";

                        string prepend = "";
                        string append = " Shipment";
                        MyStringId stringID;
                        if (MyStringId.TryGet("shipment_prepend", out stringID))
                        {
                            prepend=VRage.MyTexts.Get(stringID).ToString()+" ";
                        }
                        if (MyStringId.TryGet("shipment_append", out stringID))
                        {
                            append = " "+ VRage.MyTexts.Get(stringID).ToString();
                        }

                        grid.SetAllBeaconNames(sPrefix + random.Next(10000, 99999) + " - " + prepend + cargoType.GetDisplayName() + append,
							200f); // V31 set short until initialize check timeout
						var destination = unitType == UnitType.Air ? airConvoyDestinationPosition : groundConvoyDestinationPosition;

//                        ModLog.Info("Air Destination=" + airConvoyDestinationPosition.ToString());
//                        ModLog.Info("GND Destination=" + groundConvoyDestinationPosition.ToString());
//                        ModLog.Info("Chosen Dest=" + destination.ToString());
                        SetDestination(grid, destination);
						RegisterConvoy(grid, NpcGroupState.Travelling, unitType, destination, MyAPIGateway.Session.GameDateTime);

						var planet = DuckUtils.FindPlanetInGravity(grid.GetPosition());
						if (planet != null)
						{
							convoySpawner.SpawnConvoyEscorts(grid, unitType, planet);
						}
						break;
					case UnitRole.Escort:
						var group = FindNearestJoinableNpcGroup(grid.GetPosition(), unitType);
						if (group == null)
						{
							ModLog.Error("Escort ship spawned but can't find a group to join!");
							grid.CloseAll();
						}
						else
						{
							grid.SetAllBeaconNames("E" + random.Next(10000, 99999) + " - " + EscortName, 2500f); // V31 shorten escort beacon range to decrease hud spam
							var nearestPlanet = DuckUtils.FindPlanetInGravity(grid.GetPosition());
							if (nearestPlanet != null)
							{
								group.JoinAsEscort(grid, unitType, nearestPlanet);
							}
						}
						break;
					case UnitRole.Backup:
						var gCorpBase = baseManager.FindBaseWantingBackup();
                        // V26
                        bFoundBackup = true;

                        if (gCorpBase == null)
                            gCorpBase = baseManager.FindBaseNear(grid.GetPosition());

						if (gCorpBase == null)
						{
							ModLog.Error("Backup ship spawned but can't find the base that asked for it!");
							grid.CloseAll();
							break;
						}
						var backupPosition = gCorpBase.GetBackupPosition();
						grid.SendToPosition(backupPosition);
                        // backup debug
                        string sBeacon = "M" + random.Next(10000, 99999) + InvestigatingBackupCall;
//                        ModLog.Info("Backup Found:" + sBeacon);
//                        ModLog.Info(" Destination=" + backupPosition.ToString());
						grid.SetAllBeaconNames(sBeacon, 20000f);
						var backupGroup = new BackupGroup(NpcGroupState.Travelling, backupPosition, grid,
							heatSystem, audioSystem, MyAPIGateway.Session.GameDateTime);
						//damageSensor.RegisterDamageObserver(grid.EntityId, backupGroup);
						npcGroups.Add(backupGroup);
						break;
                    case UnitRole.Bomb:
                        bool hasSensors = false;
                        var slimBlocks = new List<IMySlimBlock>();
                        grid.GetBlocks(slimBlocks, b => b.FatBlock is IMySensorBlock);
                        if (slimBlocks.Count > 0) hasSensors = true;


                        grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyWarhead);
                        foreach (var slim in slimBlocks)
                        {
                            var wh = slim.FatBlock as IMyWarhead;
                            wh.IsArmed = true;
                            if(!hasSensors) // if no sensors, start the countdown
                            {
                                ModLog.Info("BOMB: no sensors: Starting timer");
                                wh.StartCountdown();
                            }
                        }
                        break;
					default:
						continue;
				}
			}
            if(bFoundBackup)  baseManager.ClearBaseBackupRequests();
		}

        // really should have a ConvoyManager to encapsulate these details...
		private Convoy RegisterConvoy(IMyCubeGrid leaderGrid, NpcGroupState npcGroupState, UnitType unitType,
			Vector3D destination, DateTime spawnTime)
		{
			leaderGrid.StartTimerBlocks();

			Convoy convoy;
			if (unitType == UnitType.Air)
			{
				convoy = new ConvoyAir(destination, npcGroupState, heatSystem, audioSystem, leaderGrid, spawnTime);
			}
			else
			{
				convoy = new ConvoyGround(destination, npcGroupState, heatSystem, audioSystem, leaderGrid, spawnTime);
			}
			npcGroups.Add(convoy);
			return convoy;
		}

		private static void LoadCargo(IMyCubeGrid deliveryShip, CargoType cargo)
		{
			var slimBlocks = new List<IMySlimBlock>();
			deliveryShip.GetBlocks(slimBlocks, b => b.FatBlock is IMyCargoContainer);
//            MyLog.Default.WriteLine("LoadCargo: " + deliveryShip.CustomName);
//            MyLog.Default.Flush();
            foreach (var slim in slimBlocks)
			{
                var cargoContainer = (IMyCargoContainer) slim.FatBlock;
//                MyLog.Default.WriteLine("LoadCargo: " + cargoContainer.CustomName);
//                MyLog.Default.Flush();
                var entity = cargoContainer as MyEntity;
				if (entity.HasInventory)
				{
					var inventory = entity.GetInventoryBase() as MyInventory;
                    if (inventory == null) continue;

                    // for V15: Added check  since SE V1.189 removed cargo container multiplier
                    int amount = cargo.AmountPerCargoContainer;
                    var cargoitem = cargo.GetObjectBuilder();
                    bool bPlaced = false;
                    do
                    {
                        bPlaced = inventory.AddItems(amount, cargoitem);
                        if (!bPlaced)
                        {
                            //                            MyLog.Default.WriteLine("LoadCargo: Does not fit-" + amount.ToString() + " "+cargo.GetDisplayName());
                            amount /= 2; // reduce size until it fits
                        }
                        if (amount < 3) bPlaced=true; // force to true if it gets too small
                    } while (!bPlaced);
                }
            }
		}


		private static void SetDestination(IMyCubeGrid grid, Vector3D destination)
		{
            bool bOldNav = false; // true=old way of NAV, false= new way
            // old= set gyro customname to NAV:
            // new = run PB directly with argument

            bool bKeenAutopilot = false; // force using keen autopilot

            if (ConvoySpeed != 10f)
                ModLog.Info("Using test convoy speed of " + ConvoySpeed.ToString());

            var slimBlocks = new List<IMySlimBlock>();

            grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyProgrammableBlock);
            IMyProgrammableBlock NavPB = null;

            foreach (var slim in slimBlocks)
            {
                //                var block = slim.FatBlock as IMyGyro;
                var block = slim.FatBlock as IMyProgrammableBlock;
                //                    ModLog.Info(" Found PB:" + block.CustomName);
                if (block.CustomName.Contains("NAV"))
                {
                    if (block.CustomData.Contains("Assembly not found."))
                    {
                        ModLog.Info("NAV computer not compiling:" + grid.CustomName);
                        continue;
                    }
                    NavPB = block as IMyProgrammableBlock;
                }
            }
            //            ModLog.Info("Set Destinatiion of:" + grid.CustomName);

            // C <comment>
            // S <max speed>
            // D <arrival distance>
            // W x:y:z
            // W <GPS> 
            // set destination
            //                block.CustomName = "NAV: C STARTED_DELIVERY; S 80; D 80 ; W " + destination.X + ":" + destination.Y + ":" + destination.Z;
            //                ModLog.Info("Set Waypoint to: " + block.CustomName);
            string sCommand = "S "+ConvoySpeed.ToString("0")+"; D 80; W " + destination.X + ":" + destination.Y + ":" + destination.Z;


            if (!bOldNav && !bKeenAutopilot && NavPB!=null)
            { // new way & not override keen autopilot and we found working nav pb
                // V31 Change to calling Nav module directly
                NavPB.Run(sCommand);
                bKeenAutopilot = false;
            }
            else if(!bKeenAutopilot && NavPB!=null)
            { // old way and we found working nav PB
                bool bFound = false;

                slimBlocks.Clear();
                grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyGyro);
                foreach (var slim in slimBlocks)
                {
                    var block = slim.FatBlock as IMyGyro;
                    // using STARTED_DELIVERY as a way to find our grids! For autopilot it's a harmless comment.
                    // NOTE: comment is not correct: it's not used as a way to find our grids

                    // C <comment>
                    // S <max speed>
                    // D <arrival distance>
                    // W x:y:z
                    // W <GPS> 
                    // set destination

                    block.CustomName = "NAV: " + sCommand;// C STARTED_DELIVERY; S 80; D 80 ; W " + destination.X + ":" + destination.Y + ":" + destination.Z;
                    ModLog.Info("oldnav. Setting Destination for:" + grid.CustomName + " to:" + block.CustomName);
                    //                ModLog.Info("Set Waypoint to: " + block.CustomName);
                    bFound = true;
                    break;
                }
                if (!bFound)
                {
                    ModLog.Info("No Gyro Found! Defaulting to Keen Autopilot. " + grid.CustomName);
                    bKeenAutopilot = true;
                }

            }
            if (bKeenAutopilot || NavPB==null)
            { // force keen autopilot or didn't find working nav pb
                grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
                foreach (var slim in slimBlocks)
                {
                    var remoteControl = slim.FatBlock as IMyRemoteControl;
                    remoteControl.ClearWaypoints();
                    remoteControl.AddWaypoint(destination, "Target");
                    remoteControl.SpeedLimit = ConvoySpeed;
                    remoteControl.SetAutoPilotEnabled(true);
                }
                // throw exception if no remote found? (change to Inactive state...)
            }
        }

        public override void Update1200()
		{
			var currentTime = MyAPIGateway.Session.GameDateTime;
			foreach (var npcGroup in npcGroups)
			{
				if (npcGroup.GroupSpawnTime + convoyExpiryTime < currentTime)
				{
					ModLog.DebugError("NPC group expired! Ordering to disband for passing time limit of "
					                  + convoyExpiryTime, npcGroup.GetPosition());
					npcGroup.Expire();
				}
                if(npcGroup.GroupState==NpcGroupState.Travelling)
                {

                }

			}
		}

		public override void Close()
		{
			MyAPIGateway.Entities.OnEntityAdd -= NewEntityEvent;
		}

        public string NpcGroupInfo(NpcGroupType groupType)
        {
            string str = "";
            str+="total # of NPCs="+npcGroups.Count;
            foreach (var npc in npcGroups)
            {
                str += npc.NpcgroupInfo(groupType);
            }
            return str;
        }
	}
}