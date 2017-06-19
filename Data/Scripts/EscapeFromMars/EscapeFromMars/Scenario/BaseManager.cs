using System;
using System.Collections.Generic;
using Duckroll;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace EscapeFromMars
{
	public class BaseManager : ModSystemUpdatable
	{
		private readonly HeatSystem heatSystem;
		private readonly QueuedAudioSystem audioSystem;
		private readonly List<GCorpBase> bases = new List<GCorpBase>();

		private readonly Dictionary<long, GCorpBaseSaveData> restoredGcorpBaseSaveData =
			new Dictionary<long, GCorpBaseSaveData>();

		private static readonly DateTime ZeroDate = new DateTime(1970, 1, 1);

		internal BaseManager(HeatSystem heatSystem, QueuedAudioSystem audioSystem)
		{
			this.heatSystem = heatSystem;
			this.audioSystem = audioSystem;
		}

		public override void Update60()
		{
			foreach (var gCorpBase in bases)
			{
				gCorpBase.Update();
			}
		}

		internal void LoadSaveData(List<GCorpBaseSaveData> saveData)
		{
			foreach (var gCorpBaseSaveData in saveData)
			{
				restoredGcorpBaseSaveData.Add(gCorpBaseSaveData.BaseId, gCorpBaseSaveData);
			}
		}

		public override void GridInitialising(IMyCubeGrid grid)
		{
			if (grid.IsStatic && grid.IsControlledByFaction("GCORP"))
			{
				var slimBlocks = new List<IMySlimBlock>();
				grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
				foreach (var slim in slimBlocks)
				{
					var remoteControl = slim.FatBlock as IMyRemoteControl;
					if (remoteControl.IsControlledByFaction("GCORP"))
					{
						var planet = DuckUtils.FindPlanetInGravity(remoteControl.GetPosition());

						if (planet == null)
						{
							continue; // Space bases not yet supported.
						}

						bases.Add(new GCorpBase(remoteControl, ZeroDate, planet, heatSystem, audioSystem));
						return; // Accepted grid, no need to keep looping
					}
				}
			}
		}

		public override void AllGridsInitialised()
		{
			foreach (var npcBase in bases)
			{
				GCorpBaseSaveData saveData;
				if (restoredGcorpBaseSaveData.TryGetValue(npcBase.RemoteControl.EntityId, out saveData))
				{
					npcBase.RestoreSaveData(saveData);
				}
			}
			restoredGcorpBaseSaveData.Clear();
		}

		internal GCorpBase FindBaseWantingBackup()
		{
			foreach (var gCorpBase in bases)
			{
				if (gCorpBase.OfferBackup())
				{
					return gCorpBase;
				}
			}
			return null;
		}

		internal List<GCorpBaseSaveData> GetSaveData()
		{
			var gcorpBaseDatas = new List<GCorpBaseSaveData>();
			foreach (var gCorpBase in bases)
			{
				gcorpBaseDatas.Add(gCorpBase.GenerateSaveData());
			}
			return gcorpBaseDatas;
		}
	}
}