using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using Duckroll;
using Sandbox.Game;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;
using VRage.Game.ModAPI;
using Draygo.API;

namespace EscapeFromMars
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class EfmCore : AbstractCore<SaveData>
	{
        // Current mod version, increased each time before workshop publish
        private const int CurrentModVersion = 25;

		private readonly QueuedAudioSystem audioSystem = new QueuedAudioSystem();
		private readonly HeatSystem heatSystem = new HeatSystem(-7);
		private readonly NetworkComms networkComms = new NetworkComms();
		private readonly TurretManager turretManager = new TurretManager();
		private MikiScrapManager mikiScrapManager;
		private ResearchControl researchControl;
		private MissionSystem missionSystem;
		private ConvoySpawner convoySpawner;
        //		private HUDTextAPI hudTextApi;
        // For EFM 23: (finally) update to TextHudAPI V2.
        private HudAPIv2 TextAPI;

        private ResearchHacking researchHacking;
		private int modBuildWhenGameStarted;
		private NpcGroupManager npcGroupManager;
		private BaseManager baseManager;

		protected override void InitCommon(IModSystemRegistry modSystemRegistry)
		{
            string sInit = "Initialising Escape From Mars build " + CurrentModVersion;

            MyAPIGateway.Utilities.ShowNotification(sInit, 5000, MyFontEnum.DarkBlue);
            ModLog.Info(sInit);

            if (MyAPIGateway.Session.IsServer)
                MyVisualScriptLogicProvider.SendChatMessage(sInit, "Wicorel", 0, MyFontEnum.DarkBlue);

            /*
            // TESTING: (they seem to be the same)
            if (MyAPIGateway.Session.IsServer)
                ModLog.Info("MyAPIGateway.Session.IsServer.IsServer");
            else
                ModLog.Info("MyAPIGateway.Session.NOT Server");
            if (MyAPIGateway.Multiplayer.IsServer)
                ModLog.Info("MyAPIGateway.Multiplayer.IsServer");
            else
                ModLog.Info("MyAPIGateway.Multiplayer.NOT Server");
                */
            bool bResearch = Session.SessionSettings.EnableResearch;

            // This works to change the setting.
            Session.SessionSettings.EnableResearch = true;

            if (!bResearch)
            {
//                MyAPIGateway.Utilities.ShowNotification("Save, then Exit. Edit world /Advanced settings and Enable progression", 50000, MyFontEnum.Red);
                ModLog.Info("Research was not turned on");
            }
            //            hudTextApi = new HUDTextAPI(11873852597);
            TextAPI = new HudAPIv2();
            if (modBuildWhenGameStarted > 4)
			{
				DuckUtils.PutPlayerIntoFaction("CRASH");
            }
            researchControl = new ResearchControl(audioSystem);
			researchControl.InitResearchRestrictions();
//			researchHacking = new ResearchHacking(researchControl, hudTextApi, networkComms);
            researchHacking = new ResearchHacking(researchControl, TextAPI, networkComms);
            networkComms.Init(audioSystem, researchControl, researchHacking);
//V2 does not need this			modSystemRegistry.AddCloseableModSystem(hudTextApi);
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