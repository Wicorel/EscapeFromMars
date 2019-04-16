using System.Collections.Generic;
using Duckroll;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace EscapeFromMars
{
    internal class ResearchControl
    {
        private readonly MyDefinitionId refinery = MyVisualScriptLogicProvider.GetDefinitionId("Refinery", "LargeRefinery");

        private readonly MyDefinitionId blastFurnace =
            MyVisualScriptLogicProvider.GetDefinitionId("Refinery", "Blast Furnace");

        private readonly MyDefinitionId jumpDrive =
            MyVisualScriptLogicProvider.GetDefinitionId("JumpDrive", "LargeJumpDrive");

        private readonly MyDefinitionId radioAntennaLarge = MyVisualScriptLogicProvider.GetDefinitionId("RadioAntenna",
            "LargeBlockRadioAntenna");

        private readonly MyDefinitionId radioAntennaSmall = MyVisualScriptLogicProvider.GetDefinitionId("RadioAntenna",
            "SmallBlockRadioAntenna");

        private readonly MyDefinitionId largeMissileTurret = MyVisualScriptLogicProvider.GetDefinitionId(
            "LargeMissileTurret", null);

        private readonly MyDefinitionId smallMissileTurret = MyVisualScriptLogicProvider.GetDefinitionId(
            "LargeMissileTurret", "SmallMissileTurret");

        private readonly MyDefinitionId rocketLauncher = MyVisualScriptLogicProvider.GetDefinitionId("SmallMissileLauncher",
            null);

        private readonly MyDefinitionId largeRocketLauncher =
            MyVisualScriptLogicProvider.GetDefinitionId("SmallMissileLauncher", "LargeMissileLauncher");

        private readonly MyDefinitionId smallReloadableRocketLauncher =
            MyVisualScriptLogicProvider.GetDefinitionId("SmallMissileLauncherReload", "SmallRocketLauncherReload");


        /*
        Gatling Turret
        MyLargeGatlingTurret {13CF6734E6091E8} Gatling Turret
        TyepID=MyObjectBuilder_LargeGatlingTurret
        SubtyepID=
        */
        private readonly MyDefinitionId LargeGatlingTurret =
            MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeGatlingTurret", null);

        /*
        Interior Turret
MyLargeInteriorTurret {1C7E4EACEB60541} Interior Turret
TyepID=MyObjectBuilder_InteriorTurret
SubtyepID=LargeInteriorTurret*/

        private readonly MyDefinitionId InteriorTurret =
            MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_InteriorTurret", "LargeInteriorTurret");


        /*
        Reloadable Rocket Launcher
MySmallMissileLauncherReload {1152A9D8B2F40B6} Reloadable Rocket Launcher
TyepID=MyObjectBuilder_SmallMissileLauncherReload
SubtyepID=SmallRocketLauncherReload*/

        private readonly MyDefinitionId SmallRocketLauncherReload =
            MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncherReload", "SmallRocketLauncherReload");


        /*
        Gatling Turret
MyLargeGatlingTurret {12D3E1E42993E0C} Gatling Turret
TyepID=MyObjectBuilder_LargeGatlingTurret
SubtyepID=SmallGatlingTurret*/
        private readonly MyDefinitionId SmallGatlingTurret =
            MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeGatlingTurret", "SmallGatlingTurret");


        private readonly MyDefinitionId ionThrusterSmallShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockSmallThrust");

                private readonly MyDefinitionId ionThrusterSmallShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockLargeThrust");

                private readonly MyDefinitionId ionThrusterLargeShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockSmallThrust");

                private readonly MyDefinitionId ionThrusterLargeShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockLargeThrust");

                private readonly MyDefinitionId hydroThrusterSmallShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockSmallHydrogenThrust");

                private readonly MyDefinitionId hydroThrusterSmallShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockLargeHydrogenThrust");

                private readonly MyDefinitionId hydroThrusterLargeShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockSmallHydrogenThrust");

                private readonly MyDefinitionId hydroThrusterLargeShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockLargeHydrogenThrust");

                private readonly MyDefinitionId atmoThrusterSmallShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockSmallAtmosphericThrust");

                private readonly MyDefinitionId atmoThrusterSmallShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "SmallBlockLargeAtmosphericThrust");

                private readonly MyDefinitionId atmoThrusterLargeShipSmall = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockSmallAtmosphericThrust");

                private readonly MyDefinitionId atmoThrusterLargeShipLarge = MyVisualScriptLogicProvider.GetDefinitionId("Thrust",
                    "LargeBlockLargeAtmosphericThrust");

                private readonly MyDefinitionId oxygenFarm = MyVisualScriptLogicProvider.GetDefinitionId("OxygenFarm",
                    "LargeBlockOxygenFarm");

                // MyObjectBuilder_OxygenGenerator
                //        private readonly MyDefinitionId oxygenGeneratorLarge = MyVisualScriptLogicProvider.GetDefinitionId("OxygenGenerator",            null);
                private readonly MyDefinitionId oxygenGeneratorLarge = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_OxygenGenerator",
                    null);

                //        private readonly MyDefinitionId oxygenGeneratorSmall = MyVisualScriptLogicProvider.GetDefinitionId("OxygenGenerator", "OxygenGeneratorSmall");
                private readonly MyDefinitionId oxygenGeneratorSmall = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_OxygenGenerator", "OxygenGeneratorSmall");

                private readonly MyDefinitionId oxygenTankLarge = MyVisualScriptLogicProvider.GetDefinitionId("OxygenTank",
                    null);

                private readonly MyDefinitionId oxygenTankSmall = MyVisualScriptLogicProvider.GetDefinitionId("OxygenTank",
                    "OxygenTankSmall");

                private readonly MyDefinitionId hydrogenTankLarge = MyVisualScriptLogicProvider.GetDefinitionId("OxygenTank",
                    "LargeHydrogenTank");

                private readonly MyDefinitionId hydrogenTankSmall = MyVisualScriptLogicProvider.GetDefinitionId("OxygenTank",
                    "SmallHydrogenTank");

                private readonly MyDefinitionId projectorLarge = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Projector",
                    "LargeProjector");

                private readonly MyDefinitionId projectorSmall = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Projector",
                    "SmallProjector");

                /*
                 * Hydrogen Engine
        MyHydrogenEngine {1CB79D5F7945883} Hydrogen Engine
        TyepID=MyObjectBuilder_HydrogenEngine
        SubtyepID=LargeHydrogenEngine
        */
        private readonly MyDefinitionId EngineLarge = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_HydrogenEngine",
            "LargeHydrogenEngine");

        /*
        Wind Turbine
MyWindTurbine {1A92546C67B4AD4} Wind Turbine
TyepID=MyObjectBuilder_WindTurbine
SubtyepID=LargeBlockWindTurbine
*/
        private readonly MyDefinitionId WindTurbineLarge = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_WindTurbine",
            "LargeBlockWindTurbine");

        /*
         Survival kit
MySurvivalKit {1FB6975B91C8F08} Survival kit
TyepID=MyObjectBuilder_SurvivalKit
SubtyepID=SurvivalKitLarge
*/
        private readonly MyDefinitionId SkLarge = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SurvivalKit",
            "SurvivalKitLarge");

        /*
        Basic Assembler
MyAssembler {1AA75AAA07BDEED} Basic Assembler
TyepID=MyObjectBuilder_Assembler
SubtyepID=BasicAssembler
*/
        private readonly MyDefinitionId BasicAssembler = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Assembler",
            "BasicAssembler");

        /*
        Small Battery
MyBatteryBlock {118326B35F9D289} Small Battery
TyepID=MyObjectBuilder_BatteryBlock
SubtyepID=SmallBlockSmallBatteryBlock
*/
        private readonly MyDefinitionId SmallBattery = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_BatteryBlock",
            "SmallBlockSmallBatteryBlock");

        /*
        O2/H2 Generator
MyGasGenerator {1FC3D5F6A00787C} O2/H2 Generator
TyepID=MyObjectBuilder_OxygenGenerator
SubtyepID=OxygenGeneratorSmall
*/

        /*
        Hydrogen Engine
        MyHydrogenEngine {11FFB14106D55F8} Hydrogen Engine
        TyepID=MyObjectBuilder_HydrogenEngine
        SubtyepID=SmallHydrogenEngine
        */

        private readonly MyDefinitionId EngineSmall = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_HydrogenEngine",
            "SmallHydrogenEngine");

        /*
        Survival kit
MySurvivalKit {12577CF2964CA4D} Survival kit
TyepID=MyObjectBuilder_SurvivalKit
SubtyepID=SurvivalKit */
        private readonly MyDefinitionId SkSmall = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SurvivalKit",
            "SurvivalKit");

        private readonly Dictionary<TechGroup, HashSet<MyDefinitionId>> techsForGroup =
        new Dictionary<TechGroup, HashSet<MyDefinitionId>>();

        private readonly QueuedAudioSystem audioSystem;

        internal ResearchControl(QueuedAudioSystem audioSystem)
        {
            this.audioSystem = audioSystem;
        }

        internal HashSet<TechGroup> UnlockedTechs { get; set; } = new HashSet<TechGroup>();

        internal void InitResearchRestrictions()
        {
            // SE 1.189 Turn ON any game-set blocks.  We want to control availability
            MyVisualScriptLogicProvider.ResearchListClear();

            // TODO: Figure out how to disable game-based progression tree...

            NeedsResearch(refinery, TechGroup.Permabanned);
            NeedsResearch(blastFurnace, TechGroup.Permabanned);
            NeedsResearch(blastFurnace, TechGroup.Permabanned);
            NeedsResearch(jumpDrive, TechGroup.Permabanned);
            NeedsResearch(projectorLarge, TechGroup.Permabanned);
            NeedsResearch(projectorSmall, TechGroup.Permabanned);
            NeedsResearch(largeMissileTurret, TechGroup.Rockets);
            NeedsResearch(smallMissileTurret, TechGroup.Rockets);
            NeedsResearch(rocketLauncher, TechGroup.Rockets);
            NeedsResearch(largeRocketLauncher, TechGroup.Rockets);
            NeedsResearch(smallReloadableRocketLauncher, TechGroup.Rockets);
            NeedsResearch(ionThrusterSmallShipSmall, TechGroup.Permabanned);
            NeedsResearch(ionThrusterSmallShipLarge, TechGroup.Permabanned);
            NeedsResearch(ionThrusterLargeShipSmall, TechGroup.Permabanned);
            NeedsResearch(ionThrusterLargeShipLarge, TechGroup.Permabanned);
            NeedsResearch(hydroThrusterSmallShipSmall, TechGroup.Permabanned);
            NeedsResearch(hydroThrusterSmallShipLarge, TechGroup.Permabanned);
            NeedsResearch(hydroThrusterLargeShipSmall, TechGroup.Permabanned);
            NeedsResearch(hydroThrusterLargeShipLarge, TechGroup.Permabanned);
            NeedsResearch(atmoThrusterSmallShipSmall, TechGroup.AtmosphericEngines);
            NeedsResearch(atmoThrusterSmallShipLarge, TechGroup.AtmosphericEngines);
            NeedsResearch(atmoThrusterLargeShipSmall, TechGroup.AtmosphericEngines);
            NeedsResearch(atmoThrusterLargeShipLarge, TechGroup.AtmosphericEngines);
            NeedsResearch(oxygenFarm, TechGroup.OxygenFarm);
            NeedsResearch(oxygenGeneratorLarge, TechGroup.OxygenGenerators);
            NeedsResearch(oxygenGeneratorSmall, TechGroup.OxygenGenerators);
            NeedsResearch(oxygenTankLarge, TechGroup.GasStorage);
            NeedsResearch(oxygenTankSmall, TechGroup.GasStorage);
            NeedsResearch(hydrogenTankLarge, TechGroup.GasStorage);
            NeedsResearch(hydrogenTankSmall, TechGroup.GasStorage);

            NeedsResearch(EngineLarge, TechGroup.GasStorage);
            NeedsResearch(EngineSmall, TechGroup.GasStorage);

            NeedsResearch(WindTurbineLarge, TechGroup.AtmosphericEngines);

            NeedsResearch(SmallGatlingTurret, TechGroup.BasicWeapons);
            NeedsResearch(SmallRocketLauncherReload, TechGroup.BasicWeapons);
            NeedsResearch(InteriorTurret, TechGroup.BasicWeapons);
            NeedsResearch(LargeGatlingTurret, TechGroup.BasicWeapons);
            NeedsResearch(largeMissileTurret, TechGroup.BasicWeapons);
            NeedsResearch(smallMissileTurret, TechGroup.BasicWeapons);

            NeedsResearch(SkLarge, TechGroup.Permabanned);
            NeedsResearch(SkSmall, TechGroup.Permabanned);
            NeedsResearch(BasicAssembler, TechGroup.Permabanned);

            //          NeedsResearch(SmallBattery, TechGroup.GasStorage);


        }
        public void AllowUnlockedTechs()
        {
            UnlockTechsSilently(0, UnlockedTechs);
        }

        private void NeedsResearch(MyDefinitionId techDef, TechGroup techgroup)
        {
            MyVisualScriptLogicProvider.ResearchListAddItem(techDef);

            HashSet<MyDefinitionId> techsInGroup;
            if (!techsForGroup.TryGetValue(techgroup, out techsInGroup))
            {
                techsInGroup = new HashSet<MyDefinitionId>();
                techsForGroup.Add(techgroup, techsInGroup);
            }
            techsInGroup.Add(techDef);
        }

        public void KeepTechsLocked()
        {
            // DOES NOT WORK (as of 1.190)

//            ModLog.Info("KeepTechsLocked()");

            foreach (var techGroup in techsForGroup)
            {
                var group = techGroup.Key;
//                ModLog.Info("KTL: Group=" + group.ToString());
                if (UnlockedTechs.Contains(group))
                {
//                    ModLog.Info(" UNLOCKED");
                    // OK to unlock
                    var technologies = techsForGroup[group];
                    foreach (var technology in technologies)
                    {
                        MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); 
                    }
                }
                else
                {
//                    ModLog.Info(" LOCKED");
                    // block should be locked
                    var technologies = techsForGroup[group];
                    if (technologies == null)
                    {
                        ModLog.Error("No technologies for group: " + techGroup);
                        continue;
                    }
//                    ModLog.Info(" # blocks=" + technologies.Count.ToString());
                    foreach (var technology in technologies)
                    {
                        MyVisualScriptLogicProvider.ResearchListAddItem(technology);
                    }
                }
            }

        }


        internal void UnlockTechGroupForAllPlayers(TechGroup techGroup)
        {
            if (UnlockedTechs.Contains(techGroup))
            {
                return; // Already unlocked
            }

            HashSet<MyDefinitionId> technologies;
            if (!techsForGroup.TryGetValue(techGroup, out technologies))
            {
                ModLog.Error("No technologies for group: " + techGroup);
                return;
            }
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var player in players)
            {
                foreach (var technology in technologies)
                {
                    MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
                    //MyVisualScriptLogicProvider.PlayerResearchUnlock(player.IdentityId, technology);
                }
            }
            UnlockedTechs.Add(techGroup);
            audioSystem.PlayAudio(GetAudioClipForTechGroup(techGroup));
        }

        private static AudioClip GetAudioClipForTechGroup(TechGroup techGroup)
        {
            switch (techGroup)
            {
                case TechGroup.Permabanned:
                    return AudioClip.AllTechUnlocked;
                case TechGroup.AtmosphericEngines:
                    return AudioClip.UnlockAtmospherics;
                case TechGroup.Rockets:
                    return AudioClip.UnlockedMissiles;
                case TechGroup.OxygenGenerators:
                    return AudioClip.OxygenGeneratorUnlocked;
                case TechGroup.OxygenFarm:
                    return AudioClip.OxygenFarmUnlocked;
                case TechGroup.GasStorage:
                    return AudioClip.GasStorageUnlocked;
                case TechGroup.BasicWeapons:
                    return AudioClip.BasicWeaponsUnlocked;
                default:
                    return AudioClip.PowerUpClipped;
            }
        }

        public void UnlockTechsSilently(long playerId, HashSet<TechGroup> techGroups)
        {
            foreach (var techGroup in techGroups)
            {
                var technologies = techsForGroup[techGroup];
                if (technologies == null)
                {
                    ModLog.Error("No technologies for group: " + techGroup);
                    return;
                }

                foreach (var technology in technologies)
                {

                    // unknown: does this work for ALL players?
                    MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
                    //MyVisualScriptLogicProvider.PlayerResearchUnlock(playerId, technology);
                }
            }
        }


        public void UnlockTechForJoiningPlayer(long playerId)
        {
            foreach (var techGroup in UnlockedTechs)
            {
                var technologies = techsForGroup[techGroup];
                if (technologies == null)
                {
                    ModLog.Error("No technologies for group: " + techGroup);
                    return;
                }

                foreach (var technology in technologies)
                {
                    MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
 //                   MyVisualScriptLogicProvider.PlayerResearchUnlock(playerId, technology);
                }
            }
        }
    }
}





