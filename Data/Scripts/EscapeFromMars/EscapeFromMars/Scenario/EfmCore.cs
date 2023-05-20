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
using Sandbox.Game.Localization;
using VRage.Utils;
using SISK.LoadLocalization;

namespace EscapeFromMars
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class EfmCore : AbstractCore<SaveData>
	{
        // Current mod version, increased each time before workshop publish
        private const int CurrentModVersion = 43;

        // V31.  Drone script update for 1.193.100.  All previous drones have scripts that will not compile.
        // V33 SE 1.194
        // V34 SE 1.195
        // V35 MyTexts for Mod Text localization
        // V36 MyTexts for Audio and subtitle and LCD SCreen Text localizations
        // V37 04/21/2021 Prep for SE 1.198 release
        //    create ammos.sbc from keen base and update trajectory from 800 to 1200 (1.198 ammos.sbc doesn't work on 1.197)
        //    Force CRASH faction on server for player on client joint
        // V38 SE 1.199 (many blocks added)
        // V39 SE 1.200 prep for Warefare2 many weapon blocks and ammo types added
        //   NOTE: Removed EFM ammos.sbc
        // V40 SE 1.200 Fix convoys not piloting.
        // V41 SE 1.201 (or before) fix turrets default to targetting neutrals.  (ie, the HQ Rocket)
        // V42 SE 1.201 AutocannonClip cargo name fixup in CargoTypes
        //
        //  Reports of Miki not working
        //     and reports of research progress not correctly copied into joining players
        // V43 Trying to fix above
        

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

        private readonly Mod localization = new Mod();

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
            var stringIdInit= MyStringId.TryGet("Init");
            string sInit = VRage.MyTexts.Get(stringIdInit).ToString() + " "+ CurrentModVersion;

            var TranslationByID= MyStringId.TryGet("TranslationBy");
            string sTranslationBy = VRage.MyTexts.Get(TranslationByID).ToString();
            if (!string.IsNullOrWhiteSpace(sTranslationBy))
                sInit += "\n  " + sTranslationBy;

            var AudioByID = MyStringId.TryGet("AudioBy");
            string sAudioBy = VRage.MyTexts.Get(AudioByID).ToString();
            if (!string.IsNullOrWhiteSpace(sAudioBy))
                sInit += "\n  " + sAudioBy;
            //            string sInit = "Initialising Escape From Mars build " + CurrentModVersion;

            MyAPIGateway.Utilities.ShowNotification(sInit, 5000, MyFontEnum.DarkBlue);
            ModLog.Info(sInit);
            MyLog.Default.Info("EFM Init"+sInit);

//            var gamelanguage = MySpaceTexts.ToolTipOptionsGame_Language;
            var gamelanguage=MyAPIGateway.Session.Config.Language;
            ModLog.Info("Game Language="+ gamelanguage.ToString());

            /*
            var idString1=MyStringId.TryGet("String1");
            var String1=VRage.MyTexts.Get(idString1);
            ModLog.Info("idString1=" + idString1);
            ModLog.Info("String1=" + String1);
            */

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

            if ((gameVersion.Major == 1 && gameVersion.Minor >= 192) || gameVersion.Major > 1)
            {
                ModLog.Info("Economy items enabled");
                CargoType.AllowEconomyItems();
            }
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 198) || gameVersion.Major > 1)
            {
                ModLog.Info("Warefare1 items enabled");
                CargoType.AllowWarefare1Items();
            }
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 200) || gameVersion.Major > 1)
            {
                ModLog.Info("Warefare2 items enabled");
                CargoType.AllowWarefare2Items();
            }

            if (!bResearch)
            {
//                MyAPIGateway.Utilities.ShowNotification("Save, then Exit. Edit world /Advanced settings and Enable progression", 50000, MyFontEnum.Red);
                ModLog.Info("Research was not turned on");
            }
            TextAPI = new HudAPIv2();
//            if (modBuildWhenGameStarted > 4) V37
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
            if (args[1].ToLower() == "debug")
            {
                bool bDisplaySyntax = false;

                if (args.Length > 2)
                {
                    bool bOK;
                    bool bArgument = false;
                    if (args.Length > 3)
                    {
                        bOK = bool.TryParse(args[3], out bArgument);
                    }
                    if (args[2].ToLower() == "convoys")
                    {
                        if (args.Length > 3)
                        {
                            // set
                            ConvoySpawner.DebugConvoys = bArgument;
                            ModLog.Info("DebugConvoys Set to" + ConvoySpawner.DebugConvoys);
                        }
                        else
                        {
                            // display
                            MyVisualScriptLogicProvider.SendChatMessage("DebugConboys="+ConvoySpawner.DebugConvoys.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                        }

                    }
                    if (args[2].ToLower() == "air")
                    {
                        if (args.Length > 3)
                        {
                            // set
                            ConvoySpawner.ForceAirOnly = bArgument;
                            ModLog.Info("ForceAirOnly Set to" + ConvoySpawner.ForceAirOnly);
                        }
                        else
                        {
                            // display
                            MyVisualScriptLogicProvider.SendChatMessage("ForceAirOnly=" + ConvoySpawner.ForceAirOnly.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                        }

                    }
                    if (args[2].ToLower() == "ground")
                    {
                        if (args.Length > 3)
                        {
                            // set
                            ConvoySpawner.ForceGroundOnly = bArgument;
                            ModLog.Info("ForceGroundOnly Set to" + ConvoySpawner.ForceGroundOnly);
                        }
                        else
                        {
                            // display
                            MyVisualScriptLogicProvider.SendChatMessage("ForceGroundOnly=" + ConvoySpawner.ForceGroundOnly.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                        }

                    }
                }
                else
                {
                    bDisplaySyntax = true;
                }
                int iParam = 0;
                bool bOk = int.TryParse(args[2], out iParam);

                if (bOk && iParam >= 0)
                {
                    heatSystem.HeatDifficulty = iParam;
                    MyVisualScriptLogicProvider.SendChatMessage("Difficulty set to " + heatSystem.HeatDifficulty.ToString(), "Wicorel", 0, MyFontEnum.DarkBlue);
                    if (heatSystem.HeatDifficulty > 3)
                    {
                        GCorpBase.SetFastBackupDelay();
                    }
                    else
                    {
                        GCorpBase.SetNormalBackupDelay();
                    }
                }
                if (bDisplaySyntax)
                {
                    MyVisualScriptLogicProvider.SendChatMessage("syntax: /efm debug <convoy|air|ground> [true|false]", "Wicorel", 0, MyFontEnum.DarkBlue);
                    visible = true;
                    return;
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