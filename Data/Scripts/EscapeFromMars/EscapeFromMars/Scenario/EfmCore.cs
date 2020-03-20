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
using System;
using VRage.ObjectBuilders;

namespace EscapeFromMars
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class EfmCore : AbstractCore<SaveData>
	{
        // Current mod version, increased each time before workshop publish
        private const int CurrentModVersion = 33;

        //V 31.  Drone script update for 1.193.100.  All previous drones have scripts that will not compile.
        // V33 SE 1.194

		private readonly QueuedAudioSystem audioSystem = new QueuedAudioSystem();
		private readonly HeatSystem heatSystem = new HeatSystem(-7,1);
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
        // V26
        private int modBuildWhenLastSaved;

		private NpcGroupManager npcGroupManager;
		private BaseManager baseManager;

        MyDefinitionId oxygenDefId = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Oxygen");
        MyDefinitionId hydrogenDefId = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Hydrogen");

        private Version gameVersion;

        protected override void InitCommon(IModSystemRegistry modSystemRegistry)
		{
            string sInit = "Initialising Escape From Mars build " + CurrentModVersion;

            MyAPIGateway.Utilities.ShowNotification(sInit, 5000, MyFontEnum.DarkBlue);
            ModLog.Info(sInit);

            if (MyAPIGateway.Session.IsServer)
                MyVisualScriptLogicProvider.SendChatMessage(sInit, "Wicorel", 0, MyFontEnum.DarkBlue);

            gameVersion = MyAPIGateway.Session.Version;
            ModLog.Info("SE Version=" + gameVersion.ToString());
            /*
            ModLog.Info(" Major=" + gameVersion.Major.ToString());
            ModLog.Info(" MajorRevision=" + gameVersion.MajorRevision.ToString());
            ModLog.Info(" Minor=" + gameVersion.Minor.ToString());
            ModLog.Info(" MinorRevision=" + gameVersion.MinorRevision.ToString());
            ModLog.Info(" Build=" + gameVersion.Build.ToString());
            */
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

            Session.SessionSettings.EnableBountyContracts = false; // SE V1.192

            if ((gameVersion.Major == 1 && gameVersion.Minor >= 192 ) || gameVersion.Major>1)
            {
                ModLog.Info("Economy items enabled");
                CargoType.AllowEconomyItems();
            }

            if (!bResearch)
            {
//                MyAPIGateway.Utilities.ShowNotification("Save, then Exit. Edit world /Advanced settings and Enable progression", 50000, MyFontEnum.Red);
                ModLog.Info("Research was not turned on");
            }
            TextAPI = new HudAPIv2();
            if (modBuildWhenGameStarted > 4)
			{
				DuckUtils.PutPlayerIntoFaction("CRASH");
            }
            researchControl = new ResearchControl(audioSystem);
			researchControl.InitResearchRestrictions();
            researchHacking = new ResearchHacking(researchControl, TextAPI, networkComms);
            networkComms.Init(audioSystem, researchControl, researchHacking);
			modSystemRegistry.AddCloseableModSystem(networkComms);
			modSystemRegistry.AddUpatableModSystem(audioSystem);

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;

        }

        // called when grid is created in-game (NOT on load)
        public override void GridInit(IMyCubeGrid grid)
        {
           
            base.GridInit(grid);
//            ModLog.Info("EFMCore: GridInit:" + grid.CustomName);

            var slimBlocks = new List<IMySlimBlock>();


        }

        public override void Close()
        {
            ModLog.Info("Close Called");
            base.Close();
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
           TextAPI.Close();
        }


        private void MessageEntered(string msg, ref bool visible)
        {
            if (msg.Equals("/efm", StringComparison.InvariantCultureIgnoreCase))
            {
                MyAPIGateway.Utilities.ShowMessage("EFM", "Valid Commands \n/efm heat\n/efm difficulty #");
                visible = false;
                return;
            }
            if (!msg.StartsWith("/efm", StringComparison.InvariantCultureIgnoreCase))
                return;
            visible = false;
            string[] args = msg.Split(' ');
            if (args.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowMessage("EFM", "Valid Commands \n/efm heat\n/efm difficulty #\n/efm scale [true|false]");
                return;
            }
            if (args[1].ToLower() == "heat")
            {
                if (args.Length > 2)
                {
                    int iParam = 0;
                    bool bOk = int.TryParse(args[2], out iParam);

                    if (bOk && iParam >= 0)
                    {
                        heatSystem.HeatLevel+= iParam;
                    }
                }
                string sHeat = "EFM\n Heat=" + heatSystem.HeatLevel.ToString() 
                    + "\n Difficulty=" + heatSystem.HeatDifficulty.ToString()
                    + "\n MultiplayerScaling=" + heatSystem.MultiplayerScaling.ToString()
                ;

                MyVisualScriptLogicProvider.SendChatMessage(sHeat, "Wicorel", 0, MyFontEnum.DarkBlue);

            }
            if (args[1].ToLower() == "difficulty")
            {
                if (args.Length < 3)
                {
                    MyVisualScriptLogicProvider.SendChatMessage("syntax: /efm difficulty #", "Wicorel", 0, MyFontEnum.DarkBlue);
                    visible = true;
                    return;
                }
                int iParam = 0;
                bool bOk=int.TryParse(args[2], out iParam);

                if(bOk && iParam>=0)
                {
                    heatSystem.HeatDifficulty = iParam;
                    MyVisualScriptLogicProvider.SendChatMessage("Difficulty set to "+heatSystem.HeatDifficulty.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                    if(heatSystem.HeatDifficulty>3)
                    {
                        GCorpBase.SetFastBackupDelay();
                    }
                    else
                    {
                        GCorpBase.SetNormalBackupDelay();
                    }
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessage("syntax: /efm difficulty #", "Wicorel", 0, MyFontEnum.DarkBlue);
                }

            }
            if (args[1].ToLower() == "scale")
            {
                if (args.Length < 3)
                {
                    MyVisualScriptLogicProvider.SendChatMessage("syntax: /efm scale [true|false]", "Wicorel", 0, MyFontEnum.DarkBlue);
                    visible = true;
                    return;
                }
                bool bParam = false;
                bool bOk = bool.TryParse(args[2], out bParam);

                if (bOk)
                {
                    heatSystem.MultiplayerScaling= bParam;
                    MyVisualScriptLogicProvider.SendChatMessage("MultiplayerScaling set to " + heatSystem.MultiplayerScaling.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessage("syntax: /efm scale [true|false]", "Wicorel", 0, MyFontEnum.DarkBlue);
                }

            }
            if (args[1].ToLower()== "convoy")
            {
                string sMsg = npcGroupManager.NpcGroupInfo(NpcGroupType.Convoy);
                MyVisualScriptLogicProvider.SendChatMessage(sMsg, "Wicorel", 0, MyFontEnum.DarkBlue);
            }
            if (args[1].ToLower()=="backup")
            {
                string sMsg = npcGroupManager.NpcGroupInfo(NpcGroupType.Backup);
                MyVisualScriptLogicProvider.SendChatMessage(sMsg, "Wicorel", 0, MyFontEnum.DarkBlue);
            }
            if (args[1].ToLower() == "base")
            {
                string sMsg=baseManager.BaseInfo();
                MyVisualScriptLogicProvider.SendChatMessage(sMsg, "Wicorel", 0, MyFontEnum.DarkBlue);
            }
            if (args[1].ToLower() == "players")
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);
                string sMsg = "#Players=" + players.Count;
                MyVisualScriptLogicProvider.SendChatMessage(sMsg, "Wicorel", 0, MyFontEnum.DarkBlue);
                foreach(var player in players)
                {
                    sMsg=" "+ player.DisplayName;
                    var ident = player.Identity;
                    //ident.IdentityId;
                    var chara= player.Character;
                    if (chara != null)
                    {
                        bool bUnderCover = DuckUtils.IsPlayerUnderCover(player);
                        if(bUnderCover) sMsg += " IS under cover\n";
                        bool bIsUnderground = DuckUtils.IsPlayerUnderground(player);
                        if (bIsUnderground) sMsg += " IS underground\n";
                        float health = chara.Integrity;
                        bool isDead = chara.IsDead;
                        float EnergyLevel = chara.SuitEnergyLevel;
                        float physicalMass = chara.CurrentMass;
                        bool thrustEnabled = chara.EnabledThrusts;
                        float hLevel = chara.GetSuitGasFillLevel(hydrogenDefId);
                        float o2Level = chara.GetSuitGasFillLevel(oxygenDefId);
                        sMsg += " H=" + health.ToString("0.00") + " E=" + EnergyLevel.ToString("0.00");
                        sMsg += "\n Mass=" + physicalMass.ToString("0.00") + " Thrust=" + thrustEnabled.ToString();
                        sMsg += "\n H=" + hLevel.ToString("0.00") + " O2=" + o2Level.ToString("0.00");
                    }
                    else { sMsg += " No character loaded yet"; }
                    MyVisualScriptLogicProvider.SendChatMessage(sMsg, "Wicorel", 0, MyFontEnum.DarkBlue);
                }

            }
        }
        protected override void InitHostPreLoading()
		{
			if (MyAPIGateway.Session == null)
				return;
			mikiScrapManager = new MikiScrapManager(audioSystem);
			baseManager = new BaseManager(heatSystem, audioSystem);
			convoySpawner = new ConvoySpawner(heatSystem, audioSystem);
            npcGroupManager = new NpcGroupManager(modBuildWhenLastSaved, heatSystem, audioSystem, baseManager, convoySpawner);
        }

        // after loading of saved data
        protected override void InitHostPostLoading(IModSystemRegistry modSystemRegistry)
		{
            ModLog.Info("Original world was loaded by Version:" + modBuildWhenGameStarted.ToString());
            ModLog.Info("Loaded world was saved by Version:" + modBuildWhenLastSaved.ToString());

            npcGroupManager.SetBuildWhenSaved(modBuildWhenLastSaved);

            researchHacking.InitHackingLocations(); // Uses research restrictions and save data
			DuckUtils.MakePeaceBetweenFactions("MIKI", "CRASH");
			DuckUtils.MakePeaceBetweenFactions("MIKI", "GCORP");

            //V27 for SE 1.192
            DuckUtils.RemoveFaction("SPRT");
            DuckUtils.SetAllPlayerReputation("MIKI", 0);

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

                DuckUtils.PutPlayerIntoFaction("CRASH"); // V28
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

                //V26
                , BuildWhenSaved = CurrentModVersion
                , HeatDifficultySetting = heatSystem.HeatDifficulty
                , MultiplayerScaling = heatSystem.MultiplayerScaling
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
			researchHacking.RestoreSaveData(saveData.HackingData);
			modBuildWhenGameStarted = saveData.BuildWhenGameStarted;
			baseManager.LoadSaveData(saveData.GCorpBaseSaveDatas);
			mikiScrapManager.LoadSaveData(saveData.MikiScrapSaveDatas);

            //V26
            modBuildWhenLastSaved = saveData.BuildWhenSaved;

            heatSystem.HeatDifficulty = saveData.HeatDifficultySetting;
            if (heatSystem.HeatDifficulty < 1) heatSystem.HeatDifficulty = 1;
            if (heatSystem.HeatDifficulty > 3)
            {
                GCorpBase.SetFastBackupDelay();
            }
            else
            {
                GCorpBase.SetNormalBackupDelay();
            }

            heatSystem.MultiplayerScaling = saveData.MultiplayerScaling;

            // Move to the end so other saved info is already loadedf
            missionSystem = new MissionSystem(modBuildWhenLastSaved, gameVersion, saveData.MissionStartTimeBinary, saveData.ExcludedMissionPrompts,
                audioSystem, researchControl);
        }

        public override void StartedNewGame()
		{
			missionSystem = new MissionSystem(modBuildWhenLastSaved,gameVersion, MyAPIGateway.Session.GameDateTime.ToBinary(), new HashSet<int>(),
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

        // V26
        public int BuildWhenSaved { get; set; }
        public int HeatDifficultySetting { get; set; }
        public bool MultiplayerScaling { get; set; }


    }
}