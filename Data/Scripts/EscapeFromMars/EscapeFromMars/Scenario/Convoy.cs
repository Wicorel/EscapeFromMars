using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Duckroll;
using VRage.Utils;
using SpaceEngineers.Game.ModAPI;
using Sandbox.ModAPI.Interfaces;

namespace EscapeFromMars
{
	internal abstract class Convoy : NpcGroup
	{
		private string InterceptingBeaconSuffix = " *INTERCEPTING*"; // loaded from mytexts
		private string FleeingBeaconSuffix = " *FLEEING*"; // loaded from mytexts

        private static bool ConvoyUpdateDebug = true; // TODO: Change to false before release

        protected static readonly IList<EscortPosition> AllEscortPositions =
			new List<EscortPosition>(DuckUtils.GetEnumValues<EscortPosition>()).AsReadOnly();
        private readonly TimeSpan convoyInitiateTime = new TimeSpan(0, 0, 5);

		private readonly int ConvoyLeaderMaxSpeed = 10;
		private readonly double ConvoyLeaderArrivalDistance = 300.0;

        internal static readonly IList<EscortPosition> AirEscortPositions = new List<EscortPosition>
		{
			EscortPosition.AboveLeft,
			EscortPosition.AboveRight,
			EscortPosition.AboveLeftFar,
			EscortPosition.AboveRightFar,
			EscortPosition.AboveLeftSuperFar,
			EscortPosition.AboveRightSuperFar
		}.AsReadOnly();

		internal static readonly IList<EscortPosition> GroundEscortPositions = new List<EscortPosition>
		{
			EscortPosition.GroundLeft,
			EscortPosition.GroundRight
		}.AsReadOnly();

		private IMyCubeGrid leader;
		private readonly Dictionary<EscortPosition, IMyCubeGrid> escortDic = new Dictionary<EscortPosition, IMyCubeGrid>();
		private readonly QueuedAudioSystem audioSystem;

		private readonly IMyRemoteControl remoteControl; //V44

		internal Convoy(Vector3D destination, NpcGroupState initialState, NpcGroupArrivalObserver arrivalObserver,
			QueuedAudioSystem audioSystem, IMyCubeGrid leader, DateTime groupSpawnTime)
			: base(leader.EntityId, initialState, destination, NpcGroupType.Convoy, groupSpawnTime, arrivalObserver)
		{
			this.audioSystem = audioSystem;
			this.leader = leader;

            InterceptingBeaconSuffix = VRage.MyTexts.Get(MyStringId.TryGet("InterceptingBeaconSuffix")).ToString();
            FleeingBeaconSuffix = VRage.MyTexts.Get(MyStringId.TryGet("FleeingBeaconSuffix")).ToString();

            var slimBlocks = new List<IMySlimBlock>();
            leader.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
            foreach (var slim in slimBlocks)
            {
                remoteControl = slim.FatBlock as IMyRemoteControl;
            }

        }

        internal override void JoinAsEscort(IMyCubeGrid escortApplicant, UnitType unitType, MyPlanet marsPlanet)
		{
			var applicantPosition = escortApplicant.GetPosition();

			//V44 rewrite to allow null marsPlanet
			Vector3D gravity = remoteControl.WorldMatrix.Down;
            if (marsPlanet != null) 
			{
				gravity = marsPlanet.GetGravityAtPoint(applicantPosition);
            }
			var closestDistSq = double.MaxValue;
			EscortPosition? closestPosition = null;

			foreach (var escortPosition in GetPotentialEscortPositions(unitType))
			{
				if (!escortDic.ContainsKey(escortPosition))
				{
					var suggestedPosition = GetEscortPositionVector(leader, gravity, escortPosition, GetAdditionalHeightModifier());
					var distSq = Vector3D.DistanceSquared(applicantPosition, suggestedPosition);
					if (distSq < closestDistSq)
					{
						closestDistSq = distSq;
						closestPosition = escortPosition;
					}
				}
			}

			if (closestPosition.HasValue)
			{
				escortDic.Add(closestPosition.Value, escortApplicant);
				//TODO: Update the new recruit on our current state and have them act appropriately?
			}
		}

		internal void ReConnectEscort(IMyCubeGrid escort, NpcGroupSaveData npcGroupSaveData)
		{
			var escortSaveData = GetEscortSaveData(escort, npcGroupSaveData);
			escortDic.Add(escortSaveData.EscortPosition, escort);
		}

		private static EscortSaveData GetEscortSaveData(IMyEntity escort, NpcGroupSaveData npcGroupSaveData)
		{
			foreach (var escortSaveData in npcGroupSaveData.Escorts)
			{
				if (escortSaveData.EscortEntityId == escort.EntityId)
				{
					return escortSaveData;
				}
			}
			throw new ArgumentException("Can't find escort save data: " + escort.EntityId);
		}

		internal override void Update()
		{
			CheckEscortsAlive();
            if (!leader.IsControlledByFaction("GCORP"))
            {
                GroupState = NpcGroupState.Disbanding;
                InitiateDisbandProtocols();
            }
            else if ((GroupState == NpcGroupState.Travelling || GroupState == NpcGroupState.InCombat)
                )
            {
				// TODO: allow target base to define arrival radius
                if (Vector3D.DistanceSquared(Destination, leader.GetPosition()) < ConvoyLeaderArrivalDistance * ConvoyLeaderArrivalDistance) 
					// increase to 300 to allow for variations in height. V26
                    //                     && Vector3D.DistanceSquared(Destination, leader.GetPosition()) < 200.0*200) // increase to 200 to allow for variations in height.
                    //                     && Vector3D.Distance(Destination, leader.GetPosition()) < 100.0)
                {
                    string sBeacons = "";
                    var slimBlocks2 = new List<IMySlimBlock>();
                    leader.GetBlocks(slimBlocks2, b => b.FatBlock is IMyBeacon);
                    foreach (var slim2 in slimBlocks2)
                    {
                        var beacon = slim2.FatBlock as IMyBeacon;
                        sBeacons += beacon.CustomName;
                    }

                    ModLog.Info("Group Arrived at destination: " + leader.CustomName + " " + sBeacons);
                    ArrivalObserver.GroupArrivedIntact();
                    audioSystem.PlayAudioRandomChance(0.1, AudioClip.ConvoyArrivedSafely);
                    GroupState = NpcGroupState.Disbanding;
                    InitiateDisbandProtocols();
                    ResetBeaconNames();
                }
                else if (Vector3D.DistanceSquared(Destination, leader.GetPosition()) < 2000 * 2000) // max weapons range
                { // turn off target neutrals if within max weapon range of the HQ (destination)..added V41

//                    var slimBlocks2 = new List<IMySlimBlock>();
//                    leader.GetBlocks(slimBlocks2, b => b.FatBlock is IMyBeacon);
                    var slimBlocksG = new List<IMySlimBlock>();
                    leader.GetBlocks(slimBlocksG, b => b.FatBlock is IMyLargeGatlingTurret);
                    foreach (var slim in slimBlocksG)
                    {
                        var gatling = slim.FatBlock as IMyLargeGatlingTurret;
                        gatling.TargetNeutrals = false; //V41
                    }

                    slimBlocksG.Clear();
                    leader.GetBlocks(slimBlocksG, b => b.FatBlock is IMyLargeMissileTurret);
                    foreach (var slim in slimBlocksG)
                    {
                        var missile = slim.FatBlock as IMyLargeMissileTurret;
                        missile.TargetNeutrals = false;
                    }
                    slimBlocksG.Clear();
                    leader.GetBlocks(slimBlocksG, b => b.FatBlock is IMyLargeInteriorTurret);
                    foreach (var slim in slimBlocksG)
                    {
                        var interior = slim.FatBlock as IMyLargeInteriorTurret;
                        interior.TargetNeutrals = false;
                    }
                }
            }
            // ModLogs are for DEBUG nav script
            //            ModLog.Info("Convoy update:" + leader.CustomName+" ID:"+leader.EntityId.ToString() + " State:"+GroupState.ToString());
            if ((GroupState == NpcGroupState.Travelling))
            {
                var currentTime = MyAPIGateway.Session.GameDateTime;
                if (GroupSpawnTime + convoyInitiateTime < currentTime)
                {
                    leader.SetAllBeaconNames(null, 20000f);
                }
                bool bKeenAutopilotActive = false;
                var slimBlocks = new List<IMySlimBlock>();
                leader.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
                IMyRemoteControl remoteControl = null;
                foreach (var slim in slimBlocks)
                { // should only be one RC, but loop anyway, I guess
                    remoteControl = slim.FatBlock as IMyRemoteControl;
                    bKeenAutopilotActive = remoteControl.IsAutoPilotEnabled;
                    if (ConvoyUpdateDebug) ModLog.Info("Keen Autopilot:" + bKeenAutopilotActive.ToString());
                    break;
                }

                slimBlocks.Clear();
				bool bNavStarted = false;
                leader.GetBlocks(slimBlocks, b => b.FatBlock is IMyProgrammableBlock);
                foreach (var slim in slimBlocks)
                {
                    var block = slim.FatBlock as IMyProgrammableBlock;
                    if (block == null) continue;

					if (block.CustomName.Contains("NAV"))
					{
						if (!bKeenAutopilotActive
							&& GroupSpawnTime + convoyInitiateTime < currentTime // delay check for mode change.
							)
						{
							if (ConvoyUpdateDebug) ModLog.Info("NAV Bock:" + block.CustomName + "\nDetailedInfo:\n" + block.DetailedInfo + "\n---");
							if (//!bKeenAutopilotActive && 
								block.DetailedInfo.Contains("Assembly") || // "Assembly not found. Please compile script."
								block.DetailedInfo.Contains("mode=0") || block.DetailedInfo.Contains("mode=-1"))
							{
								if (remoteControl == null)
								{
									// No NAV PB and No PB...nothing left to do.  Remove it grid (and try again)
									GroupState = NpcGroupState.Inactive; // this will cause NpcGroupManager to spawn a new convoy to replace this one.
									return;
								}
								if (ConvoyUpdateDebug) ModLog.Info("Forcing Keen autopilot");
								// force it to use Keen Autopilot

								leader.SendToPosition(Destination, 0, ConvoyLeaderMaxSpeed);
								bNavStarted = true;
								/*
								remoteControl.ClearWaypoints();
								remoteControl.AddWaypoint(Destination, "Target");
								remoteControl.SpeedLimit = ConvoyLeaderMaxSpeed; // should come from NPCGroupManager
								remoteControl.SetAutoPilotEnabled(true);
								*/
								/*
                                 // debug output
                                var slimBlocks2 = new List<IMySlimBlock>();
                                leader.GetBlocks(slimBlocks2, b => b.FatBlock is IMyBeacon);
                                string sBeacons = "";
                                foreach(var slim2 in slimBlocks2)
                                {
                                    var beacon = slim2.FatBlock as IMyBeacon;
                                    sBeacons += beacon.CustomName;
                                }

                                // it didn't get the command!
                                //                                GroupState = NpcGroupState.Inactive; // this will cause NpcGroupManager to spawn a new convoy to replace this one.
                                ModLog.Info("Autopilot recovery because leader NAV not in correct mode: "+ sBeacons);
                                */
							}
							break;
						}
						//                        ModLog.Info("PB:"+block.CustomName+"\n"+"DetailedInfo=:\n" + block.DetailedInfo);
					}
                }

				if(!bNavStarted) //V44
                {
                    if (ConvoyUpdateDebug) ModLog.Info("Using Keen autopilot");
                    //
                    leader.SendToPosition(Destination, 0, ConvoyLeaderMaxSpeed);
                    /*
                    remoteControl.ClearWaypoints();
                    remoteControl.AddWaypoint(Destination, "Target");
                    remoteControl.SpeedLimit = ConvoyLeaderMaxSpeed; // should come from NPCGroupManager
                    remoteControl.SetAutoPilotEnabled(true);
                    */
                }

                // Following is just debug info
                /*
                leader.GetBlocks(slimBlocks, b => b.FatBlock is IMyGyro);
                foreach (var slim in slimBlocks)
                {
                    var block = slim.FatBlock as IMyGyro;
                    if (block!=null && block.CustomName.Contains("NAV"))
                    {
                        ModLog.Info("G:"+block.CustomName + "\n");
                    }
                }
                */
            }

            if (GroupState == NpcGroupState.Disbanding)
			{
				AttemptDespawning();
				return;
			}

			if (DuckUtils.IsAnyPlayerNearPosition(leader.GetPosition(),1250) && GroupState == NpcGroupState.Travelling)
			{
				GroupState = NpcGroupState.InCombat;
				InitiateAttackProtocols();
			}

			if (GroupState == NpcGroupState.InCombat)
			{
				// TODO: check if player grid is nearby?
				var player = DuckUtils.GetNearestPlayerToPosition(leader.GetPosition(), 4000);
				if (player == null)
				{
					GroupState = NpcGroupState.Travelling; // Return to normal, cowardly players have run off or died
					ResetBeaconNames();
					if (escortDic.Count > 0) //todo maybe check if the escorts are actually alive? Dunno if doing this already
					{
						audioSystem.PlayAudio(AudioClip.DisengagingFromHostile, AudioClip.TargetLost);
					}
					else
					{
						audioSystem.PlayAudio(AudioClip.PursuitEvaded, AudioClip.SensorsLostTrack);
					}
				}
				else
				{
					SendArmedEscortsNearPosition(player.GetPosition()); // Use same position as when escorting, to avoid collisions
				}
			}

			if (GroupState == NpcGroupState.Travelling)
			{
				foreach (var entry in escortDic)
				{
					SendEscortToGrid(entry.Key, entry.Value, leader);
				}
			}
		}

	    private void ResetBeaconNames()
	    {
	        foreach (var escort in escortDic.Values)
	        {
	            escort.SetLightingColors(GcorpBlue);
	            escort.RemoveFromFirstBeaconName(InterceptingBeaconSuffix);
	        }

	        if (leader != null && !leader.Closed)
	        {
	            leader.SetLightingColors(GcorpBlue);
	            leader.RemoveFromFirstBeaconName(FleeingBeaconSuffix);
	        }
	    }

	    private void CheckEscortsAlive()
		{
			List<EscortPosition> removedEscorts = null; // Don't initialise unless needed, this is called a lot
			foreach (var entry in escortDic)
			{
				if (!entry.Value.IsControlledByFaction("GCORP"))
				{
					if (removedEscorts == null)
					{
						removedEscorts = new List<EscortPosition>();
					}
					removedEscorts.Add(entry.Key);
				}
			}

			if (removedEscorts != null)
			{
				foreach (var escort in removedEscorts)
				{
					escortDic.Remove(escort);
				}
			}
		}

		private void AttemptDespawning()
		{
			if (leader != null)
			{
				if (AttemptDespawn(leader))
				{
					leader = null;
				}
			}

			var removedEscorts = new List<EscortPosition>();
			foreach (var entry in escortDic)
			{
				if (AttemptDespawn(entry.Value))
				{
					removedEscorts.Add(entry.Key);
				}
			}

			foreach (var escort in removedEscorts)
			{
				escortDic.Remove(escort);
			}

			if (leader == null && escortDic.Count == 0)
			{
				GroupState = NpcGroupState.Disbanded;
			}
		}

		private void InitiateDisbandProtocols()
		{
			foreach (var entry in escortDic)
			{
				SendEscortToPosition(entry.Key, entry.Value, Destination);
			}
		}

		//TODO adjust height depending on friendly or hostile?
		private void SendArmedEscortsNearPosition(Vector3D targetPosition)
		{
			foreach (var entry in escortDic)
			{
				var targetPos = entry.Value.HasUsableGun() ? targetPosition : Destination;
				SendEscortToPosition(entry.Key, entry.Value, targetPos);
			}
		}

		private void SendEscortToPosition(EscortPosition position, IMyCubeGrid escort, Vector3D targetPosition)
		{
            var escortPosition = GetEscortPositionVector(targetPosition, remoteControl.GetNaturalGravity(), position,
GetAdditionalHeightModifier());
            escort.SendToPosition(escortPosition);

			/*
            var slimBlocks = new List<IMySlimBlock>();
			escort.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
			foreach (var slim in slimBlocks)
			{
				var remoteControl = slim.FatBlock as IMyRemoteControl;
				remoteControl.ClearWaypoints();
				var escortPosition = GetEscortPositionVector(targetPosition, remoteControl.GetNaturalGravity(), position,
					GetAdditionalHeightModifier());
				remoteControl.AddWaypoint(escortPosition, "Target");
                // note: default speed limit
				remoteControl.SetAutoPilotEnabled(true);
			}
			*/
		}

		protected abstract int GetAdditionalHeightModifier();

		private void InitiateAttackProtocols()
		{
			var haveEscorts = false;
			foreach (var escort in escortDic.Values)
			{
                // V44 set each escort to attack mode
                var movementSlim = new List<IMySlimBlock>();
                var rcBlocks = new List<IMySlimBlock>();
                var offensiveSlim = new List<IMySlimBlock>();

                escort.GetBlocks(rcBlocks, b => b.FatBlock is IMyRemoteControl);
                escort.GetBlocks(movementSlim, b => b.FatBlock is IMyFlightMovementBlock);
                escort.GetBlocks(offensiveSlim, b => b.FatBlock is IMyOffensiveCombatBlock);

                foreach (var slim in rcBlocks)
                {
                    //                    var rcFunctional = slim.FatBlock as IMyFunctionalBlock;
                    //                    rcFunctional.Enabled = true;

                    var remoteControl = slim.FatBlock as IMyRemoteControl;
                    remoteControl.SetAutoPilotEnabled(false);
                    remoteControl.ClearWaypoints();
                }

                foreach (var slim in movementSlim)
                {
                    var block = slim.FatBlock as IMyFlightMovementBlock;
                    block.Enabled = true;
                    block.SetValueBool("ActivateBehavior", true);
                    
                }
                foreach (var slim in offensiveSlim)
                {
                    var block = slim.FatBlock as IMyOffensiveCombatBlock;
                    block.Enabled = true;
                    block.SetValueBool("ActivateBehavior", true);
                }



                escort.SetLightingColors(Color.Red);
				escort.AppendToFirstBeaconName(InterceptingBeaconSuffix);
				haveEscorts = true;
			}

			if (leader != null)
			{
				leader.SetLightingColors(Color.Orange, 0.8f);
				leader.AppendToFirstBeaconName(FleeingBeaconSuffix);
			}

			if (haveEscorts)
			{
				MyVisualScriptLogicProvider.MusicPlayMusicCategory("HeavyFight");
				audioSystem.PlayAudio(AudioClip.EnemyDetectedMovingToIntercept, AudioClip.UnknownHostileOnScanners);
			}
			else
			{
				audioSystem.PlayAudio(AudioClip.ConvoyUnderThreat);
			}
		}

		private void SendEscortToGrid(EscortPosition position, IMyCubeGrid escort, IMyEntity convoyLeaderGrid)
		{
            var escortPosition = GetEscortPositionVector(convoyLeaderGrid, remoteControl.GetNaturalGravity(), position,
GetAdditionalHeightModifier());
            escort.SendToPosition(escortPosition);

            /*
			var slimBlocks = new List<IMySlimBlock>();
			escort.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
			foreach (var slim in slimBlocks)
			{

                // This code is in lots of places :(
				var remoteControl = slim.FatBlock as IMyRemoteControl;
				remoteControl.ClearWaypoints();
				var escortPosition = GetEscortPositionVector(convoyLeaderGrid, remoteControl.GetNaturalGravity(), position,
					GetAdditionalHeightModifier());
				remoteControl.AddWaypoint(escortPosition, "Target");
                // note: default speed limit
				remoteControl.SetAutoPilotEnabled(true);
            }
				*/
        }

        internal static Vector3D GetEscortPositionVector(IMyEntity convoyLeaderGrid, Vector3D gravity,
			EscortPosition escortPosition, int additionalHeightModifier)
		{
			var deliveryShipPosition = convoyLeaderGrid.GetPosition();
			switch (escortPosition)
			{
				case EscortPosition.GroundLeft:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Left * 20;
				case EscortPosition.GroundRight:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Right * 20;
				case EscortPosition.AboveLeft:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Left * 10-
					       gravity * (2 + additionalHeightModifier);
				case EscortPosition.AboveRight:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Right * 10 -
					       gravity * (2 + additionalHeightModifier);
				case EscortPosition.AboveLeftFar:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Left * 20 -
					       gravity * (3 + additionalHeightModifier);
				case EscortPosition.AboveRightFar:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Right * 20 -
					       gravity * (3 + additionalHeightModifier);
				case EscortPosition.AboveLeftSuperFar:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Left * 30 -
					       gravity * (4 + additionalHeightModifier);
				case EscortPosition.AboveRightSuperFar:
					return deliveryShipPosition + convoyLeaderGrid.WorldMatrix.Right * 30 -
					       gravity * (4 + additionalHeightModifier);
				default:
					throw new ArgumentException("Uncoped for escort position: " + escortPosition);
			}
		}

		//TODO reconcile these two methods? Or stop using formations when attacking/moving to base without the escort.
		//TODO could follow a randomly decided leader, if more than one ship is performing the action.

		private const double Degrees90 = 0.5 * MathHelper.Pi;
		private const double Degrees270 = 1.5 * MathHelper.Pi;

		internal static Vector3D GetEscortPositionVector(Vector3D deliveryShipPosition, Vector3D gravity,
			EscortPosition escortPosition, int additionalHeightModifier)
		{
			switch (escortPosition)
			{
				case EscortPosition.GroundLeft:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees90) * 2;
				case EscortPosition.GroundRight:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees270) * 2;
				case EscortPosition.AboveLeft:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees90) -
					       gravity * (2 + additionalHeightModifier);
				case EscortPosition.AboveRight:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees270) -
					       gravity * (2 + additionalHeightModifier);
				case EscortPosition.AboveLeftFar:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees90) * 2 -
					       gravity * (3 + additionalHeightModifier);
				case EscortPosition.AboveRightFar:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees270) * 2 -
					       gravity * (3 + additionalHeightModifier);
				case EscortPosition.AboveLeftSuperFar:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees90) * 3 -
					       gravity * (4 + additionalHeightModifier);
				case EscortPosition.AboveRightSuperFar:
					return deliveryShipPosition + GetPerpendicularVector(ref gravity, Degrees270) * 3 -
					       gravity * (4 + additionalHeightModifier);
				default:
					throw new ArgumentException("Uncoped for escort position: " + escortPosition);
			}
		}

		internal List<IMyCubeGrid> GetEscorts()
		{
			var escorts = new List<IMyCubeGrid>();
			escorts.AddRange(escortDic.Values);
			return escorts;
		}

		internal override bool IsJoinable(UnitType unitType)
		{
			if (!leader.IsControlledByFaction("GCORP") || !IsUnitTypeAllowedToJoin(unitType))
			{
				return false;
			}

			foreach (var escortPosition in GetPotentialEscortPositions(unitType))
			{
				if (!escortDic.ContainsKey(escortPosition))
				{
					return true;
				}
			}
			return false;
		}

		private static IEnumerable<EscortPosition> GetPotentialEscortPositions(UnitType unitType)
		{
			IEnumerable<EscortPosition> potentialEscortPositions = unitType == UnitType.Air
				? AirEscortPositions
				: GroundEscortPositions;
			return potentialEscortPositions;
		}

		protected abstract bool IsUnitTypeAllowedToJoin(UnitType unitType);

		internal override Vector3D GetPosition()
		{
			return leader?.GetPosition() ?? Vector3D.Zero;
		}

		private static Vector3D GetPerpendicularVector(ref Vector3D axis, double angle)
		{
			var tangent = Vector3D.CalculatePerpendicularVector(axis);
			Vector3D bitangent;
			Vector3D.Cross(ref axis, ref tangent, out bitangent);
			return Math.Cos(angle) * tangent + Math.Sin(angle) * bitangent;
		}

		internal override List<EscortSaveData> GetEscortSaveData()
		{
			var escortSaveDatas = new List<EscortSaveData>();
			foreach (var entry in escortDic)
			{
				var escortSaveData = new EscortSaveData
				{
					EscortPosition = entry.Key,
					EscortEntityId = entry.Value.EntityId
				};
				escortSaveDatas.Add(escortSaveData);
			}
			return escortSaveDatas;
		}
	}

	public enum EscortPosition
	{
		GroundLeft,
		GroundRight,
		AboveLeft,
		AboveRight,
		AboveLeftFar,
		AboveRightFar,
		AboveLeftSuperFar,
		AboveRightSuperFar
	}

	public class EscortSaveData
	{
		public long EscortEntityId { get; set; }
		public EscortPosition EscortPosition { get; set; }
	}
}