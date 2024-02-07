﻿using System;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;
using Duckroll;

namespace EscapeFromMars
{
	public abstract class NpcGroup
	{
		protected static readonly Color GcorpBlue = new Color(55, 69, 255);

		internal long GroupLeaderId { get; }
		public readonly NpcGroupType groupType;
        public NpcGroupState GroupState { get; set; }
        public Vector3D Destination { get; }
		internal DateTime GroupSpawnTime { get; }
		protected readonly NpcGroupArrivalObserver ArrivalObserver;

		internal abstract bool IsJoinable(UnitType unitType);

		internal abstract void JoinAsEscort(IMyCubeGrid escort, UnitType unitType, MyPlanet marsPlanet);

		/// Checks if we are disbanded - ie all grids are closed or hijacked
		internal bool IsDisbanded()
		{
			return GroupState == NpcGroupState.Disbanded;
		}

        public string NpcgroupInfo(NpcGroupType groupType)
        {
            string str = "";
            if(groupType<0 || this.groupType==groupType)
                str += "\n Type=" + groupType.ToString() + " State="+GroupState.ToString();
            return str;
        }
		/// <summary>
		/// Tells the group to try and disband if they are not already doing so. If disbanded in this manner,
		/// they are treated as arriving safely so that players aren't blamed for groups that bugged out and
		/// trapped themselves somewhere (this would raise the heat level overall if we didn't call it)
		/// </summary>
		internal void Expire()
		{
			if (GroupState != NpcGroupState.Disbanded && GroupState != NpcGroupState.Disbanding)
			{
				GroupState = NpcGroupState.Disbanding;
				ArrivalObserver.GroupArrivedIntact();
			}
		}

		internal abstract void Update();

		internal abstract List<EscortSaveData> GetEscortSaveData();

		internal abstract UnitType GetLeaderUnitType();

		internal abstract Vector3D GetPosition();

		protected static bool AttemptDespawn(IMyCubeGrid grid, Int32 minplayerdistance=1750)
		{
			if (!grid.IsControlledByFaction("GCORP"))
			{
				return true; // If we are not GCorp don't try to despawn, we may be wreckage or player hijacked
			}

            // if player is 'near' base, then disabled drones near the base won't be despawned
            //			if (!DuckUtils.IsAnyPlayerNearPosition(grid.GetPosition(), 1750))

            // V26
            if (!DuckUtils.IsAnyPlayerNearPosition(grid.GetPosition(), minplayerdistance))
            {
                grid.CloseAll();
				return true;
			}

			return false;
		}

		internal NpcGroup(long groupLeaderId, NpcGroupState initialState, Vector3D destination, NpcGroupType groupType, DateTime groupSpawnTime,
			NpcGroupArrivalObserver arrivalObserver)
		{
			GroupLeaderId = groupLeaderId;
			this.groupType = groupType;
			GroupSpawnTime = groupSpawnTime;
			ArrivalObserver = arrivalObserver;
			GroupState = initialState;
			Destination = destination;
		}

		internal NpcGroupSaveData GetSaveData()
		{
			var saveData = new NpcGroupSaveData
			{
				LeaderUnitType = GetLeaderUnitType(),
				GroupId = GroupLeaderId,
				Escorts = GetEscortSaveData(),
				State = GroupState,
				GroupDestination = Destination,
				NpcGroupType = groupType,
				SpawnTime = GroupSpawnTime.ToBinary()
			};
			return saveData;
		}
	}

	public class NpcGroupSaveData
	{
		public NpcGroupState State { get; set; }
		public UnitType LeaderUnitType { get; set; }
		public List<EscortSaveData> Escorts { get; set; }
		public long GroupId { get; set; }
		public Vector3D GroupDestination { get; set; }
		public NpcGroupType NpcGroupType { get; set; }
		public long SpawnTime { get; set; }
	}

	public enum NpcGroupState
	{
		Travelling,
		InCombat,
		Disbanding,
		Disbanded,
        ReturningForRepairs //V29
        ,Inactive // V31
	}

	public enum NpcGroupType
	{
        All=-1,
		Convoy=0,
		Backup
	}
}