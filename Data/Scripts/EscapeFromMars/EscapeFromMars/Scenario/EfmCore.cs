using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using Duckroll;
using Sandbox.Game;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace EscapeFromMars
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class EfmCore : AbstractCore<SaveData>
	{
		// Current mod version, increased each time before workshop publish
		private const int CurrentModVersion = 8;

		private readonly QueuedAudioSystem audioSystem = new QueuedAudioSystem();
		private readonly HeatSystem heatSystem = new HeatSystem(-7);
		private readonly NetworkComms networkComms = new NetworkComms();
		private readonly TurretManager turretManager = new TurretManager();
		private MikiScrapManager mikiScrapManager;
		private ResearchControl researchControl;
		private MissionSystem missionSystem;
		private ConvoySpawner convoySpawner;
		private HUDTextAPI hudTextApi;
		private ResearchHacking researchHacking;
		private int modBuildWhenGameStarted;
		private NpcGroupManager npcGroupManager;
		private BaseManager baseManager;

		protected override void InitCommon(IModSystemRegistry modSystemRegistry)
		{
			MyAPIGateway.Utilities.ShowNotification("Initialising Escape From Mars build " + CurrentModVersion, 10000,
				MyFontEnum.DarkBlue);
			hudTextApi = new HUDTextAPI(11873852597);
			if (modBuildWhenGameStarted > 4)
			{
				DuckUtils.PutPlayerIntoFaction("CRASH");
			}
			researchControl = new ResearchControl(audioSystem);
			researchControl.InitResearchRestrictions();
			researchHacking = new ResearchHacking(researchControl, hudTextApi, networkComms);
			networkComms.Init(audioSystem, researchControl, researchHacking);
			modSystemRegistry.AddCloseableModSystem(hudTextApi);
			modSystemRegistry.AddCloseableModSystem(networkComms);
			modSystemRegistry.AddUpatableModSystem(audioSystem);
		}

		protected override void InitHostPreLoading()
		{
			if (MyAPIGateway.Session == null)
				return;
			mikiScrapManager = new MikiScrapManager(audioSystem);
			baseManager = new BaseManager(heatSystem, audioSystem);
			convoySpawner = new ConvoySpawner(heatSystem, audioSystem);
			npcGroupManager = new NpcGroupManager(heatSystem, audioSystem, baseManager, convoySpawner);
		}
		
		protected override void InitHostPostLoading(IModSystemRegistry modSystemRegistry)
		{
			researchHacking.InitHackingLocations(); // Uses research restrictions and save data
			DuckUtils.MakePeaceBetweenFactions("MIKI", "CRASH");
			DuckUtils.MakePeaceBetweenFactions("MIKI", "GCORP");
			audioSystem.AudioRelay = networkComms;
			networkComms.StartWipeHostToolbar();
			modSystemRegistry.AddRapidUpdatableModSystem(turretManager);
			modSystemRegistry.AddUpatableModSystem(researchHacking);
			modSystemRegistry.AddUpatableModSystem(missionSystem);
			modSystemRegistry.AddUpatableModSystem(mikiScrapManager);
			modSystemRegistry.AddUpatableModSystem(npcGroupManager);
			modSystemRegistry.AddUpatableModSystem(baseManager);
			modSystemRegistry.AddUpatableModSystem(convoySpawner);
		}

		

		protected override void InitClient(IModSystemRegistry modSystemRegistry)
		{
			var player = MyAPIGateway.Session.Player;
			if (player != null)
			{
				networkComms.NotifyServerClientJoined(player);
			}
		}

		private const string SaveFileName = "EFM-SaveData.xml";

		public override string GetSaveDataFileName()
		{
			return SaveFileName;
		}

		public override SaveData GetSaveData()
		{
			if (modBuildWhenGameStarted == 0)
			{
				modBuildWhenGameStarted = CurrentModVersion;
			}
			
			var saveData = new SaveData
			{
				HeatLevel = heatSystem.HeatLevel,
				UnlockedTechs = researchControl.UnlockedTechs,
				NpcGroupSaveDatas = npcGroupManager.GetSaveData(),
				NextSpawnTime = convoySpawner.GetNextSpawnTimeForSaving(),
				MissionStartTimeBinary = missionSystem.GetMissionStartTimeBinary(),
				ExcludedMissionPrompts = missionSystem.GetExcludedIDs(),
				RegisteredPlayers = networkComms.RegisteredPlayers,
				HackingData = researchHacking.GetSaveData(),
				BuildWhenGameStarted = modBuildWhenGameStarted,
				GCorpBaseSaveDatas = baseManager.GetSaveData(),
				MikiScrapSaveDatas = mikiScrapManager.GetSaveDatas()
			};
			return saveData;
		}

		public override void LoadPreviousGame(SaveData saveData)
		{
			networkComms.RegisteredPlayers = saveData.RegisteredPlayers;
			heatSystem.HeatLevel = saveData.HeatLevel;
			researchControl.UnlockedTechs = saveData.UnlockedTechs;
			npcGroupManager.LoadSaveData(saveData.NpcGroupSaveDatas);
			convoySpawner.RestoreSpawnTimeFromSave(saveData.NextSpawnTime);
			missionSystem = new MissionSystem(saveData.MissionStartTimeBinary, saveData.ExcludedMissionPrompts,
				audioSystem, researchControl);
			researchHacking.RestoreSaveData(saveData.HackingData);
			modBuildWhenGameStarted = saveData.BuildWhenGameStarted;
			baseManager.LoadSaveData(saveData.GCorpBaseSaveDatas);
			mikiScrapManager.LoadSaveData(saveData.MikiScrapSaveDatas);
		}

		public override void StartedNewGame()
		{
			missionSystem = new MissionSystem(MyAPIGateway.Session.GameDateTime.ToBinary(), new HashSet<int>(),
				audioSystem, researchControl);
			modBuildWhenGameStarted = CurrentModVersion;
		}
	}

	public class SaveData
	{
		public int HeatLevel { get; set; }
		public HashSet<TechGroup> UnlockedTechs { get; set; }
		public List<NpcGroupSaveData> NpcGroupSaveDatas { get; set; }
		public long NextSpawnTime { get; set; }
		public long MissionStartTimeBinary { get; set; }
		public HashSet<int> ExcludedMissionPrompts { get; set; }
		public HashSet<long> RegisteredPlayers { get; set; }
		public List<ResearchHacking.HackingSaveData> HackingData { get; set; }
		public int BuildWhenGameStarted { get; set; }
		public List<GCorpBaseSaveData> GCorpBaseSaveDatas { get; set; }
		public List<MikiScrapSaveData> MikiScrapSaveDatas { get; set; }
	}
}