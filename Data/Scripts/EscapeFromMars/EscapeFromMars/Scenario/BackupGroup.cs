using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using Duckroll;

namespace EscapeFromMars
{
	internal class BackupGroup : NpcGroup
	{
		private IMyCubeGrid leader;
		private readonly QueuedAudioSystem audioSystem;
		private const string InterceptingBeaconSuffix = " *PURSUING TARGET*";
		private const string ReturningToBase = " *RETURNING TO BASE*";
        HeatSystem heat;
		internal BackupGroup(NpcGroupState initialState, Vector3D destination, IMyCubeGrid leader,
			HeatSystem heatSystem, QueuedAudioSystem audioSystem, DateTime groupSpawnTime)
			: base(leader.EntityId, initialState, destination, NpcGroupType.Backup, groupSpawnTime, heatSystem)
		{
			this.leader = leader;
			this.audioSystem = audioSystem;
            this.heat = heatSystem;
		}

		internal override bool IsJoinable(UnitType unitType)
		{
			return false; // For now only 1 ship
		}

		internal override void JoinAsEscort(IMyCubeGrid escort, UnitType unitType, MyPlanet marsPlanet)
		{
			throw new ArgumentException("Not allowed!");
		}

		internal override void Update()
		{
			if (!leader.IsControlledByFaction("GCORP"))
			{
				leader = null;
				GroupState = NpcGroupState.Disbanded;
				return;
			}
//            ModLog.Info("Backup:" + leader.EntityId.ToString() + " " + GroupState.ToString());

			if ( ( GroupState == NpcGroupState.Travelling || GroupState==NpcGroupState.ReturningForRepairs )
                && Vector3D.DistanceSquared(Destination, leader.GetPosition()) < 40.0*40.0)
			{
                // it arrives at destination.  and nothing... get rid of it.
//                ModLog.Info(" Backup arrived at base/target and nothing found.  Disbanding");
				GroupState = NpcGroupState.Disbanding;
			}


			if (GroupState == NpcGroupState.Disbanding)
			{
				var isArmed = leader.HasUsableGun();
				if (AttemptDespawn(leader,200)) //V26
				{
					leader = null;
					GroupState = NpcGroupState.Disbanded;
					if (isArmed)
					{
						ArrivalObserver.GroupArrivedIntact();
					}
				}
				return;
			}

            if (DuckUtils.IsAnyPlayerNearPosition(leader.GetPosition(), 2000) && //V29 1000->2000  Backups were getting disbanded when player between 1000 and max spawn distance.
			    (GroupState == NpcGroupState.Travelling || GroupState == NpcGroupState.Disbanding))
			{
                GroupState = NpcGroupState.InCombat;
//                ModLog.Info("Backup:" + leader.EntityId.ToString() + " Found Target:" + GroupState.ToString());
                leader.SetLightingColors(Color.Red);
				leader.RemoveFromFirstBeaconName(ReturningToBase);
				leader.AppendToFirstBeaconName(InterceptingBeaconSuffix);
				audioSystem.PlayAudio(AudioClip.TargetFoundDronesAttack, AudioClip.TargetIdentifiedUnitsConverge);
			}
            // todo: if no player nearby go searching for player vehicles near base/convoy
            // todo: if can't target player after a delay (or player under cover?), search for player vehicles near current location

			if (GroupState == NpcGroupState.InCombat)
			{
				if (!leader.HasUsableGun())
				{
//                    GroupState = NpcGroupState.Disbanding;
                    GroupState = NpcGroupState.ReturningForRepairs; //V29
//                    ModLog.Info("Backup:" + leader.EntityId.ToString() + " No Gun." + GroupState.ToString());
                    leader.SetLightingColors(GcorpBlue);
                    leader.RemoveFromFirstBeaconName(" Investigating Backup Call"); // match text in NpcGroupManager
                    leader.RemoveFromFirstBeaconName(InterceptingBeaconSuffix);
                    leader.AppendToFirstBeaconName(ReturningToBase);
                    leader.SendToPosition(Destination);
					audioSystem.PlayAudio(AudioClip.DroneDisarmed);
                    // disbanding, but for backups we want to extra penalize for killing unit
                    heat.BackupDisabled();
				}
				else
				{
					var player = DuckUtils.GetNearestPlayerToPosition(leader.GetPosition(), 2000); //V29 1250->2000
					if (player == null)
					{
						GroupState = NpcGroupState.Disbanding; // Return to normal, cowardly players have run off or died
//                        ModLog.Info("Backup:" + leader.EntityId.ToString() + " No Players in range after Combat mode." + GroupState.ToString());
                        leader.SetLightingColors(GcorpBlue);
						leader.RemoveFromFirstBeaconName(InterceptingBeaconSuffix);
						leader.AppendToFirstBeaconName(ReturningToBase);
						leader.SendToPosition(Destination);
						audioSystem.PlayAudio(AudioClip.HostileDisappeared, AudioClip.TargetFleeingPursuit);
					}
					else
					{
                        float heightModifier = 15; // Change from 2 pre V26

                        // Added V26
                        if (DuckUtils.IsPlayerUnderCover(player))
                        {
                            heightModifier = 300;
                        }
						leader.SendToPosition(player.GetPosition(), heightModifier);
					}
				}
			}
		}

		internal override List<EscortSaveData> GetEscortSaveData()
		{
			return new List<EscortSaveData>();
		}

		internal override UnitType GetLeaderUnitType()
		{
			return UnitType.Air;
		}

		internal override Vector3D GetPosition()
		{
			return leader.GetPosition();
		}
	}
}