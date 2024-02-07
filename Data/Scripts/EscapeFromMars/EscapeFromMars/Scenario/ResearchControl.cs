using System.Collections.Generic;
using Duckroll;
using Sandbox.Game;
using Sandbox.Game.SessionComponents;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace EscapeFromMars
{
    internal class ResearchControl
    {
        // use the new Keen research system
        readonly bool bNewResearch = true;

        // V44. Remove reactors from mar planet
        private readonly MyDefinitionId SmallBlockSmallGenerator = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "SmallBlockSmallGenerator");
        private readonly MyDefinitionId SmallBlockLargeGenerator = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "SmallBlockLargeGenerator");
        private readonly MyDefinitionId LargeBlockSmallGenerator = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "LargeBlockSmallGenerator");
        private readonly MyDefinitionId LargeBlockLargeGenerator = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "LargeBlockLargeGenerator");
        private readonly MyDefinitionId LargeBlockSmallGeneratorWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "LargeBlockSmallGeneratorWarfare2");
        private readonly MyDefinitionId LargeBlockLargeGeneratorWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "LargeBlockLargeGeneratorWarfare2");
        private readonly MyDefinitionId SmallBlockSmallGeneratorWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "SmallBlockSmallGeneratorWarfare2");
        private readonly MyDefinitionId SmallBlockLargeGeneratorWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Reactor", "SmallBlockLargeGeneratorWarfare2");


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


        // Economy.  V1.192
        /*Safe Zone
        MySafeZoneBlock {120C2D4D0123A2C} Safe Zone
        81279012572379692
        TyepID=MyObjectBuilder_SafeZoneBlock
        SubtyepID=SafeZoneBlock
        */
        private readonly MyDefinitionId SafeZoneBlock = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SafeZoneBlock",
            "SafeZoneBlock");

        /*Store
        MyStoreBlock {1C6D182A68027E6} Store
        128019998496008166
        TyepID=MyObjectBuilder_StoreBlock
        SubtyepID=StoreBlock
*/
        private readonly MyDefinitionId StoreBlock = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_StoreBlock",
            "StoreBlock");
        /*SContracts
        MyContractBlock {1A49A2924080102} Contracts
        118388991707316482
        TyepID=MyObjectBuilder_ContractBlock
        SubtyepID=ContractBlock
        */
        private readonly MyDefinitionId ContractBlock = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_ContractBlock",
            "ContractBlock");


        /*TyepID=MyObjectBuilder_VendingMachine
        SubtyepID=VendingMachine
        */
        private readonly MyDefinitionId VendingMachine = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_VendingMachine", "VendingMachine");

        /*TyepID=MyObjectBuilder_StoreBlock
        SubtyepID=AtmBlock
        */
        private readonly MyDefinitionId AtmBlock = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_StoreBlock", "AtmBlock");

        //V1.193

        /*TyepID=MyObjectBuilder_VendingMachine
        SubtyepID=FoodDispenser
        */
        private readonly MyDefinitionId FoodDispenser = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_VendingMachine", "FoodDispenser");

        /*TyepID=MyObjectBuilder_Cockpit    
SubtyepID=OpenCockpitSmall
*/
        private readonly MyDefinitionId OpenCockpitSmall = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Cockpit", "OpenCockpitSmall");

        /*TyepID=MyObjectBuilder_Cockpit
SubtyepID=OpenCockpitLarge
*/
        private readonly MyDefinitionId OpenCockpitLarge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Cockpit", "OpenCockpitLarge");

        /*TyepID=MyObjectBuilder_TextPanel
SubtyepID=TransparentLCDSmall*/
        private readonly MyDefinitionId TransparentLCDSmall = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_TextPanel", "TransparentLCDSmall");

        /*TyepID=MyObjectBuilder_ReflectorLight
SubtyepID=RotatingLightSmall*/
        private readonly MyDefinitionId RotatingLightSmall = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_ReflectorLight", "RotatingLightSmall");


        /*TyepID=MyObjectBuilder_LCDPanelsBlock
SubtyepID=LabEquipment*/
        private readonly MyDefinitionId LabEquipment = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_LCDPanelsBlock", "LabEquipment");

        /*TyepID=MyObjectBuilder_TextPanel
SubtyepID=TransparentLCDLarge*/
        private readonly MyDefinitionId TransparentLCDLarge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_TextPanel", "TransparentLCDLarge");

        /*TyepID=MyObjectBuilder_ReflectorLight
SubtyepID=RotatingLightLarge*/
        private readonly MyDefinitionId RotatingLightLarge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_ReflectorLight", "RotatingLightLarge");

        /*TyepID=MyObjectBuilder_LCDPanelsBlock
SubtyepID=MedicalStation*/
        private readonly MyDefinitionId MedicalStation = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_LCDPanelsBlock", "MedicalStation");

        /*TyepID=MyObjectBuilder_Jukebox
SubtyepID=Jukebox*/
        private readonly MyDefinitionId Jukebox = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Jukebox", "Jukebox");


        // V1.194
        /* TyepID=MyObjectBuilder_OxygenTank
SubtyepID=LargeHydrogenTankSmall */
        private readonly MyDefinitionId LargeHydrogenTankSmall = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_OxygenTank", "LargeHydrogenTankSmall");

        /* TyepID=MyObjectBuilder_OxygenTank
SubtyepID=SmallHydrogenTankSmall
*/
        private readonly MyDefinitionId SmallHydrogenTankSmall = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_OxygenTank", "SmallHydrogenTankSmall");


        // V1.195
        /* SG 
         * Sc-fi sliding door
         * TyepID=MyObjectBuilder_Door
        SubtyepID=SmallSideDoor
        */
        private readonly MyDefinitionId SmallSideDoor = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Door", "SmallSideDoor");
        /* SG medium hinge
         * TyepID=MyObjectBuilder_MotorAdvancedStator
        SubtyepID = MediumHinge
        */
        private readonly MyDefinitionId MediumHinge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_MotorAdvancedStator", "MediumHinge");


        /* SG Small hinge
         * TyepID=MyObjectBuilder_MotorAdvancedStator
        SubtyepID=SmallHinge
        */
        private readonly MyDefinitionId SmallHinge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_MotorAdvancedStator", "SmallHinge");

        /* Small hinge head
         * TyepID=MyObjectBuilder_MotorStator
        SubtyepID=SmallStator
        */
        private readonly MyDefinitionId SmallStator = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_MotorStator", "SmallStator");

        // Sparks of the Future DLC

        /* SG
         * Sci-Fi Ion Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=SmallBlockSmallThrustSciFi
        */
        private readonly MyDefinitionId SmallBlockSmallThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "SmallBlockSmallThrustSciFi");


        /* SG
         * Sci-Fi Large Ion Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=SmallBlockLargeThrustSciFi
        */
        private readonly MyDefinitionId SmallBlockLargeThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "SmallBlockLargeThrustSciFi");

        /* SG
         * Sci-Fi Atmospheric Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=SmallBlockSmallAtmosphericThrustSciFi
        */
        private readonly MyDefinitionId SmallBlockSmallAtmosphericThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "SmallBlockSmallAtmosphericThrustSciFi");

        /*
         * SG
         * Sci-Fi Large Atmospheric Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=SmallBlockLargeAtmosphericThrustSciFi
        */
        private readonly MyDefinitionId SmallBlockLargeAtmosphericThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "SmallBlockLargeAtmosphericThrustSciFi");


        /* LG
         * Hinge
        TyepID=MyObjectBuilder_MotorAdvancedStator
        SubtyepID=LargeHinge
        */
        private readonly MyDefinitionId LargeHinge = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_MotorAdvancedStator", "LargeHinge");


        /* LG
         * Sci-Fi Terminal Panel
        TyepID=MyObjectBuilder_TerminalBlock
        SubtyepID=LargeBlockSciFiTerminal
        */
        private readonly MyDefinitionId LargeBlockSciFiTerminal = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_TerminalBlock", "LargeBlockSciFiTerminal");

        /* LG
         * Sci-Fi One-Button Terminal
        TyepID=MyObjectBuilder_ButtonPanel
        SubtyepID=LargeSciFiButtonTerminal
        */
        private readonly MyDefinitionId LargeSciFiButtonTerminal = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_ButtonPanel", "LargeSciFiButtonTerminal");

        /* LG
         * Sci-Fi Four-Button Panel
        TyepID=MyObjectBuilder_ButtonPanel
        SubtyepID=LargeSciFiButtonPanel
        (4 surfaces)
        */
        private readonly MyDefinitionId LargeSciFiButtonPanel = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_ButtonPanel", "LargeSciFiButtonPanel");

        /* LG
         * Sci-Fi Large Ion Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=LargeBlockLargeThrustSciFi
        */
        private readonly MyDefinitionId LargeBlockLargeThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "LargeBlockLargeThrustSciFi");


        /* LG 
         * Sci-Fi Ion Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=LargeBlockSmallThrustSciFi
        */
        private readonly MyDefinitionId LargeBlockSmallThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "LargeBlockSmallThrustSciFi");


        /* LG
         * Sci-Fi Large Atmospheric Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=LargeBlockLargeAtmosphericThrustSciFi
        */
        private readonly MyDefinitionId LargeBlockLargeAtmosphericThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "LargeBlockLargeAtmosphericThrustSciFi");

        /* LG
         * Sci-Fi Atmospheric Thruster
        TyepID=MyObjectBuilder_Thrust
        SubtyepID=LargeBlockSmallAtmosphericThrustSciFi
        */
        private readonly MyDefinitionId LargeBlockSmallAtmosphericThrustSciFi = MyVisualScriptLogicProvider.GetDefinitionId(
            "MyObjectBuilder_Thrust", "LargeBlockSmallAtmosphericThrustSciFi");


        /* LG
         * Sci-Fi LCD Panel 3x3
        TyepID=MyObjectBuilder_TextPanel
        SubtyepID=LargeLCDPanel3x3
        */

        /* LG
         * Sci-Fi LCD Panel 5x3
        TyepID=MyObjectBuilder_TextPanel
        SubtyepID=LargeLCDPanel5x3
        */

        /* LG
         * Sci-Fi LCD Panel 5x5
        TyepID=MyObjectBuilder_TextPanel
        SubtyepID=LargeLCDPanel5x5
        */

        /* LG
         * 
         * Scrap
         * 
         * TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSuspension3x3mirrored

            TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSuspension1x1mirrored

            TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSuspension5x5mirrored


            TyepID=MyObjectBuilder_ExhaustBlock
        SubtyepID=LargeExhaustPipe
         */

        /* SG
         * Scap
         * 
         TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSmallSuspension3x3

        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSmallSuspension1x1

        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSmallSuspension5x5

        TyepID=MyObjectBuilder_InteriorLight
        SubtyepID=OffsetLight

        TyepID=MyObjectBuilder_ReflectorLight
        SubtyepID=OffsetSpotlight

        TyepID=MyObjectBuilder_ExhaustBlock
        SubtyepID=SmallExhaustPipe

        TyepID=MyObjectBuilder_Cockpit
        SubtyepID=BuggyCockpit
         */

        /* SE 1.198 Warefare 1
         * 
         * LG
        TyepID=MyObjectBuilder_CargoContainer
        SubtyepID=LargeBlockWeaponRack
        
        TyepID=MyObjectBuilder_TargetDummyBlock
        SubtyepID=TargetDummy

        TyepID=MyObjectBuilder_InteriorLight
        SubtyepID=PassageSciFiLight

            SG

        TyepID=MyObjectBuilder_CargoContainer
        SubtyepID=SmallBlockWeaponRack

            */

        /* SE 1.199
         * LG
            TyepID=MyObjectBuilder_CargoContainer
            SubtyepID=LargeBlockLargeIndustrialContainer

            TyepID=MyObjectBuilder_LandingGear
            SubtyepID=LargeBlockMagneticPlate

            TyepID=MyObjectBuilder_LandingGear
            SubtyepID=LargeBlockSmallMagneticPlate

            TyepID=MyObjectBuilder_Refinery
            SubtyepID=LargeRefineryIndustrial
            a
                <TypeId>ButtonPanel</TypeId>
                 <SubtypeId>VerticalButtonPanelLarge</SubtypeId>

                <TypeId>OxygenTank</TypeId>
                <SubtypeId>LargeHydrogenTankIndustrial</SubtypeId>

                <TypeId>Assembler</TypeId>
                <SubtypeId>LargeAssemblerIndustrial</SubtypeId>

                <TypeId>ConveyorSorter</TypeId>
                <SubtypeId>LargeBlockConveyorSorterIndustrial</SubtypeId>

        <TypeId>Thrust</TypeId>
        <SubtypeId>LargeBlockLargeHydrogenThrustIndustrial</SubtypeId>

        <TypeId>Thrust</TypeId>
        <SubtypeId>LargeBlockSmallHydrogenThrustIndustrial</SubtypeId>


          * SG
            TyepID=MyObjectBuilder_LandingGear
            SubtyepID=SmallBlockMagneticPlate

            TyepID=MyObjectBuilder_LandingGear
            SubtyepID=SmallBlockSmallMagneticPlate

            TyepID=MyObjectBuilder_MergeBlock
            SubtyepID=SmallShipSmallMergeBlock

        <TypeId>Thrust</TypeId>
        <SubtypeId>SmallBlockLargeHydrogenThrustIndustrial</SubtypeId>

        <TypeId>Thrust</TypeId>
        <SubtypeId>SmallBlockSmallHydrogenThrustIndustrial</SubtypeId>

        */


        private readonly MyDefinitionId IndustrialRefinery = MyVisualScriptLogicProvider.GetDefinitionId("Refinery", "LargeRefineryIndustrial");
        private readonly MyDefinitionId IndustrialAssembler = MyVisualScriptLogicProvider.GetDefinitionId("Assembler", "LargeAssemblerIndustrial");

        private readonly MyDefinitionId IndustrialLGLargeHTruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockLargeHydrogenThrustIndustrial");
        private readonly MyDefinitionId IndustrialLGSmallHTruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockSmallHydrogenThrustIndustrial");
        private readonly MyDefinitionId IndustrialSGLargeHTruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockLargeHydrogenThrustIndustrial");
        private readonly MyDefinitionId IndustrialSGSmallHTruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockSmallHydrogenThrustIndustrial");

        private readonly MyDefinitionId LargeHydrogenTankIndustrial = MyVisualScriptLogicProvider.GetDefinitionId("OxygenTank", "LargeHydrogenTankIndustrial");

        /*
         * V1.200 Warfare 2
         * SG
         * 
         * TyepID=MyObjectBuilder_InteriorLight
            SubtyepID=SmallLightPanel

            TyepID=MyObjectBuilder_SmallMissileLauncher
            SubtyepID=SmallMissileLauncherWarfare2

            TyepID=MyObjectBuilder_SmallMissileLauncher
            SubtyepID=SmallBlockAutocannon

            TyepID=MyObjectBuilder_SmallGatlingGun
            SubtyepID=SmallGatlingGunWarfare2
            
            TyepID=MyObjectBuilder_BatteryBlock
            SubtyepID=SmallBlockBatteryBlockWarfare2

            TyepID=MyObjectBuilder_Reactor
            SubtyepID=SmallBlockLargeGeneratorWarfare2
            
            TyepID=MyObjectBuilder_Reactor
            SubtyepID=SmallBlockSmallGeneratorWarfare2

                            <TypeId>Cockpit</TypeId>
                <SubtypeId>PassengerBench</SubtypeId>

                <TypeId>TurretControlBlock</TypeId>
                <SubtypeId>LargeTurretControlBlock</SubtypeId>

                <TypeId>Searchlight</TypeId>
                <SubtypeId>SmallSearchlight</SubtypeId>
                <TypeId>HeatVentBlock</TypeId>
                <SubtypeId>SmallHeatVentBlock</SubtypeId>

                <TypeId>Cockpit</TypeId>
                <SubtypeId>PassengerBench</SubtypeId>
                <TypeId>BatteryBlock</TypeId>
                <SubtypeId>SmallBlockBatteryBlockWarfare2</SubtypeId>

                <TypeId>Thrust</TypeId>
                <SubtypeId>SmallBlockSmallModularThruster</SubtypeId>
                <TypeId>Thrust</TypeId>
                <SubtypeId>SmallBlockLargeModularThruster</SubtypeId>


TyepID=MyObjectBuilder_LargeGatlingTurret
SubtyepID=AutoCannonTurret
TyepID=MyObjectBuilder_LargeMissileTurret
SubtyepID=SmallBlockMediumCalibreTurret

TyepID=MyObjectBuilder_SmallMissileLauncher
SubtyepID=SmallMissileLauncherWarfare2

TyepID=MyObjectBuilder_SmallMissileLauncher
SubtyepID=SmallBlockMediumCalibreGun

TyepID=MyObjectBuilder_SmallGatlingGun
SubtyepID=SmallBlockAutocannon

            TyepID=MyObjectBuilder_SmallGatlingGun
SubtyepID=SmallGatlingGunWarfare2

TyepID=MyObjectBuilder_SmallMissileLauncherReload
SubtyepID=SmallRailgun

            TyepID=MyObjectBuilder_TurretControlBlock
SubtyepID=SmallTurretControlBlock

            LG
                <TypeId>Thrust</TypeId>
                <SubtypeId>LargeBlockSmallModularThruster</SubtypeId>
                <TypeId>Thrust</TypeId>
                <SubtypeId>LargeBlockLargeModularThruster</SubtypeId>

            TyepID=MyObjectBuilder_Reactor
            SubtyepID=LargeBlockSmallGeneratorWarfare2

            TyepID=MyObjectBuilder_Reactor
            SubtyepID=LargeBlockLargeGeneratorWarfare2

            TyepID=MyObjectBuilder_InteriorLight
            SubtyepID=LargeLightPanel

            TyepID=MyObjectBuilder_AirtightHangarDoor
            SubtyepID=AirtightHangarDoorWarfare2C

            TyepID=MyObjectBuilder_AirtightHangarDoor
            SubtyepID=AirtightHangarDoorWarfare2B

            TyepID=MyObjectBuilder_AirtightHangarDoor
            SubtyepID=AirtightHangarDoorWarfare2A

                <TypeId>TurretControlBlock</TypeId>
                <SubtypeId>LargeTurretControlBlock</SubtypeId>

                <TypeId>Door</TypeId>
                <SubtypeId>SlidingHatchDoor</SubtypeId>
                <TypeId>Door</TypeId>
                <SubtypeId>SlidingHatchDoorHalf</SubtypeId>

                            <TypeId>Searchlight</TypeId>
                <SubtypeId>LargeSearchlight</SubtypeId>
                <TypeId>HeatVentBlock</TypeId>
                <SubtypeId>LargeHeatVentBlock</SubtypeId>
                <TypeId>BatteryBlock</TypeId>
                <SubtypeId>LargeBlockBatteryBlockWarfare2</SubtypeId>

TyepID=MyObjectBuilder_SmallMissileLauncherReload
SubtyepID=LargeRailgun

            TyepID=MyObjectBuilder_TurretControlBlock
SubtyepID=LargeTurretControlBlock

TyepID=MyObjectBuilder_LargeMissileTurret
SubtyepID=LargeBlockMediumCalibreTurret

TyepID=MyObjectBuilder_SmallMissileLauncher
SubtyepID=LargeBlockLargeCalibreGun


         */
        private readonly MyDefinitionId SmallMissileLauncherWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncher", "SmallMissileLauncherWarfare2");
        private readonly MyDefinitionId SmallBlockAutocannon = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncher", "SmallBlockAutocannon");
        private readonly MyDefinitionId SmallGatlingGunWarfare2 = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallGatlingGun", "SmallGatlingGunWarfare2");

        // artillery
        private readonly MyDefinitionId LargeBlockLargeCalibreGun = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncher", "LargeBlockLargeCalibreGun");
        private readonly MyDefinitionId LargeCalibreTurret = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeMissileTurret", "LargeCalibreTurret");

        // assault cannon
        private readonly MyDefinitionId LargeBlockMediumCalibreTurret = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeMissileTurret", "LargeBlockMediumCalibreTurret");
        private readonly MyDefinitionId AutoCannonTurret = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeGatlingTurret", "AutoCannonTurret");
        private readonly MyDefinitionId SmallBlockMediumCalibreTurret = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_LargeMissileTurret", "SmallBlockMediumCalibreTurret");

        private readonly MyDefinitionId LargeTurretControlBlock = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_TurretControlBlock", "LargeTurretControlBlock");
        private readonly MyDefinitionId SmallTurretControlBlock = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_TurretControlBlock", "SmallTurretControlBlock");

        private readonly MyDefinitionId LargeRailgun = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncherReload", "LargeRailgun");
        private readonly MyDefinitionId SmallRailgun = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_SmallMissileLauncherReload", "SmallRailgun");

        private readonly MyDefinitionId SmallBlockSmallModularThruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockSmallModularThruster");
        private readonly MyDefinitionId SmallBlockLargeModularThruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockLargeModularThruster");
        private readonly MyDefinitionId LargeBlockSmallModularThruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockSmallModularThruster");
        private readonly MyDefinitionId LargeBlockLargeModularThruster = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockLargeModularThruster");


        /* 1.201
                 LG

                 Wheel Suspension 2x2 Left
        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=Suspension2x2Mirrored
        (removed)

        Offroad Wheel Suspension 2x2 Left
        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSuspension2x2Mirrored
        (removed)


        SG

        Offroad Wheel Suspension 2x2 Left
        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=OffroadSmallSuspension2x2Mirrored
        (removed)

        MyMotorAdvancedStator {135FE96106CDDE2} Advanced Rotor
        TyepID=MyObjectBuilder_MotorAdvancedStator
        SubtyepID=SmallAdvancedStatorSmall

        MyMotorSuspension {1CAEAE003F8F59B} Wheel Suspension 2x2 Right
        TyepID=MyObjectBuilder_MotorSuspension
        SubtyepID=SmallSuspension2x2
        (removed)
*/

        /* 1.202
         * LG
        TyepID=MyObjectBuilder_EventControllerBlock
        SubtyepID=EventControllerLarge

        TyepID=MyObjectBuilder_DefensiveCombatBlock
        SubtyepID=LargeDefensiveCombat

            TyepID=MyObjectBuilder_FlightMovementBlock
SubtyepID=LargeFlightMovement

TyepID=MyObjectBuilder_CameraBlock
SubtyepID=LargeCameraTopMounted

TyepID=MyObjectBuilder_AirVent
SubtyepID=AirVentFanFull

TyepID=MyObjectBuilder_AirVent
SubtyepID=AirVentFull

            TyepID=MyObjectBuilder_OffensiveCombatBlock
SubtyepID=LargeOffensiveCombat

            TyepID=MyObjectBuilder_BasicMissionBlock
SubtyepID=LargeBasicMission

TyepID=MyObjectBuilder_PathRecorderBlock
SubtyepID=LargePathRecorderBlock

            TyepID=MyObjectBuilder_TerminalBlock
SubtyepID=MaintenancePanelLarge

            TyepID=MyObjectBuilder_SensorBlock
SubtyepID=LargeBlockSensorReskin

TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=OffroadSuspension2x2Mirrored

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=OffroadSuspension2x2

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=Suspension2x2Mirrored

TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=Suspension2x2

TyepID=MyObjectBuilder_TerminalBlock
SubtyepID=LargeBlockAccessPanel1

TyepID=MyObjectBuilder_ButtonPanel
SubtyepID=LargeBlockAccessPanel3

            TyepID=MyObjectBuilder_MyProgrammableBlock
SubtyepID=LargeProgrammableBlockReskin

            SG

            TyepID=MyObjectBuilder_MergeBlock
SubtyepID=SmallShipSmallMergeBlock

            TyepID=MyObjectBuilder_MotorAdvancedStator
SubtyepID=SmallAdvancedStatorSmall

            TyepID=MyObjectBuilder_DefensiveCombatBlock
SubtyepID=SmallDefensiveCombat

            TyepID=MyObjectBuilder_OffensiveCombatBlock
SubtyepID=SmallOffensiveCombat

            TyepID=MyObjectBuilder_CameraBlock
SubtyepID=SmallCameraTopMounted

            TyepID=MyObjectBuilder_FlightMovementBlock
SubtyepID=SmallFlightMovement

            TyepID=MyObjectBuilder_SensorBlock
SubtyepID=SmallBlockSensorReskin

            TyepID=MyObjectBuilder_BasicMissionBlock
SubtyepID=SmallBasicMission

            TyepID=MyObjectBuilder_PathRecorderBlock
SubtyepID=SmallPathRecorderBlock

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=OffroadSmallSuspension2x2

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=OffroadSmallSuspension2x2Mirrored

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=SmallSuspension2x2

            TyepID=MyObjectBuilder_MotorSuspension
SubtyepID=SmallSuspension2x2Mirrored

            TyepID=MyObjectBuilder_TerminalBlock
SubtyepID=SmallBlockAccessPanel1

TyepID=MyObjectBuilder_ButtonPanel
SubtyepID=SmallBlockAccessPanel3

            TyepID=MyObjectBuilder_MyProgrammableBlock
SubtyepID=SmallProgrammableBlockReskin

            TyepID=MyObjectBuilder_WorkAreaMissionBlock
SubtyepID=SmallWorkAreaMission

            */

        /* 1.203
         * 
         Button Pedastal
             TyepID=MyObjectBuilder_ButtonPanel
             SubtyepID=LargeButtonPanelPedestal

            TyepID=MyObjectBuilder_ButtonPanel
            SubtyepID=SmallButtonPanelPedestal
        
        Short wheel suspensions
        Inset blocks

        Holo LCD
            TyepID=MyObjectBuilder_TextPanel
            SubtyepID=HoloLCDLarge

        Wind Turbine reskin
            TyepID=MyObjectBuilder_WindTurbine
            SubtyepID=LargeBlockWindTurbineReskin

        Round Beacon
            TyepID=MyObjectBuilder_Beacon
            SubtyepID=LargeBlockBeaconReskin

            TyepID=MyObjectBuilder_Beacon
            SubtyepID=SmallBlockBeaconReskin

        Corner medical Room
            TyepID=MyObjectBuilder_MedicalRoom
            SubtyepID=LargeMedicalRoomReskin

        Truss Block Decoy
            TyepID=MyObjectBuilder_Decoy
            SubtyepID=TrussPillarDecoy

        Solar slope left
            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=LargeBlockColorableSolarPanelCorner

            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=SmallBlockColorableSolarPanelCorner

        Solar
            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=LargeBlockColorableSolarPanel

            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=SmallBlockColorableSolarPanel

        Solar slope right
            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=LargeBlockColorableSolarPanelCornerInverted

            TyepID=MyObjectBuilder_SolarPanel
            SubtyepID=SmallBlockColorableSolarPanelCornerInverted

        Control Pedastal
            TyepID=MyObjectBuilder_TerminalBlock
            SubtyepID=LargeControlPanelPedestal

        Inset Cryo Room
            TyepID=MyObjectBuilder_CryoChamber
            SubtyepID=LargeBlockCryoRoom

        Half Bed
            TyepID=MyObjectBuilder_CryoChamber
            SubtyepID=LargeBlockHalfBed

        Half Bed Open
            TyepID=MyObjectBuilder_CryoChamber
            SubtyepID=LargeBlockHalfBedOffset

        Inset LCD
            TyepID=MyObjectBuilder_TextPanel
            SubtyepID=LargeFullBlockLCDPanel
        Sloped/diagonal/Curved

        Truss Block Light
            TyepID=MyObjectBuilder_InteriorLight
            SubtyepID=TrussPillarBeacon

        Truss Block Decoy
            TyepID=MyObjectBuilder_Decoy
            SubtyepID=TrussPillarDecoy

        Inset Bed
            TyepID=MyObjectBuilder_CryoChamber
            SubtyepID=LargeBlockInsetBed

        Inset Cryo Room
            TyepID=MyObjectBuilder_CryoChamber
            SubtyepID=LargeBlockCryoRoom

        Flat thruster D shape
            TyepID=MyObjectBuilder_Thrust
            SubtyepID=LargeBlockSmallFlatAtmosphericThrustDShape  
        
            TyepID=MyObjectBuilder_Thrust
            SubtyepID=SmallBlockSmallFlatAtmosphericThrustDShape

        Flat thruster
            TyepID=MyObjectBuilder_Thrust
            SubtyepID=LargeBlockSmallFlatAtmosphericThrust

            TyepID=MyObjectBuilder_Thrust
            SubtyepID=SmallBlockSmallFlatAtmosphericThrust

        Large Flat thruster D shape
            TyepID=MyObjectBuilder_Thrust
            SubtyepID=LargeBlockLargeFlatAtmosphericThrustDShape

            TyepID=MyObjectBuilder_Thrust
            SubtyepID=SmallBlockLargeFlatAtmosphericThrustDShape

        Large Flat thruster
            TyepID=MyObjectBuilder_Thrust
            SubtyepID=LargeBlockLargeFlatAtmosphericThrust

            TyepID=MyObjectBuilder_Thrust
            SubtyepID=SmallBlockLargeFlatAtmosphericThrust

        Explosive Barrel
            TyepID=MyObjectBuilder_Warhead
            SubtyepID=SmallExplosiveBarrel

        Cap Cockpit
            TyepID=MyObjectBuilder_Cockpit
            SubtyepID=SmallBlockCapCockpit

         */

        private readonly MyDefinitionId LargeBlockSmallFlatAtmosphericThrustDShape = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockSmallFlatAtmosphericThrustDShape");
        private readonly MyDefinitionId SmallBlockSmallFlatAtmosphericThrustDShape = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockSmallFlatAtmosphericThrustDShape");
        private readonly MyDefinitionId LargeBlockSmallFlatAtmosphericThrust = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockSmallFlatAtmosphericThrust");
        private readonly MyDefinitionId SmallBlockSmallFlatAtmosphericThrust = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockSmallFlatAtmosphericThrust");
        private readonly MyDefinitionId LargeBlockLargeFlatAtmosphericThrustDShape = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockLargeFlatAtmosphericThrustDShape");
        private readonly MyDefinitionId SmallBlockLargeFlatAtmosphericThrustDShape = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockLargeFlatAtmosphericThrustDShape");
        private readonly MyDefinitionId LargeBlockLargeFlatAtmosphericThrust = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "LargeBlockLargeFlatAtmosphericThrust");
        private readonly MyDefinitionId SmallBlockLargeFlatAtmosphericThrust = MyVisualScriptLogicProvider.GetDefinitionId("Thrust", "SmallBlockLargeFlatAtmosphericThrust");

        private readonly MyDefinitionId LargeBlockWindTurbineReskin = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_WindTurbine", "LargeBlockWindTurbineReskin");


        // Ship Tools V44
        /* 
         * TyepID=MyObjectBuilder_ShipGrinder
SubtyepID=LargeShipGrinder
        TyepID=MyObjectBuilder_ShipWelder
SubtyepID=LargeShipWelder
        TyepID=MyObjectBuilder_Drill
SubtyepID=LargeBlockDrill


        */
        private readonly MyDefinitionId LargeBlockWelder = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_ShipWelder", "LargeShipWelder");
        private readonly MyDefinitionId LargeBlockDrill = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Drill", "LargeBlockDrill");
        private readonly MyDefinitionId LargeBlockGrinder = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_ShipGrinder", "LargeShipGrinder");

        /*
         * 
        TyepID=MyObjectBuilder_Drill
SubtyepID=SmallBlockDrill
        TyepID=MyObjectBuilder_ShipGrinder
SubtyepID=SmallShipGrinder
        TyepID=MyObjectBuilder_ShipWelder
SubtyepID=SmallShipWelder
        */
        private readonly MyDefinitionId SmallBlockWelder = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_ShipWelder", "SmallShipWelder");
        private readonly MyDefinitionId SmallBlockDrill = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_Drill", "SmallBlockDrill");
        private readonly MyDefinitionId SmallBlockGrinder = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_ShipGrinder", "SmallShipGrinder");

        //
        private readonly Dictionary<TechGroup, HashSet<MyDefinitionId>> techsForGroup = new Dictionary<TechGroup, HashSet<MyDefinitionId>>();

        private readonly QueuedAudioSystem audioSystem;

        internal ResearchControl(QueuedAudioSystem audioSystem)
        {
            this.audioSystem = audioSystem;
        }

        internal HashSet<TechGroup> UnlockedTechs { get; set; } = new HashSet<TechGroup>();

        // UNTESTED:
        void FunctionalityChanged(long entityId, long gridid, string entityName, string gridName, string typeid, string subtypeid, bool becameFunctional)
        {
            if (subtypeid.Contains("Hydrogen"))
            { // it's likely a hydrogen tank
                ModLog.Info(" It looks like a hydrogen tank just got built.");
                KeepTechsLocked();
            }

        }

        internal void InitResearchRestrictions(bool ExtendedScenario)
        {
            // 1.194            MySectorWeatherComponent wc;
            //            WeatherType wt;

            if (bNewResearch)
            {
                MyVisualScriptLogicProvider.ResearchListClear();
                MyVisualScriptLogicProvider.ResearchListWhitelist(false); // set it to be 'black list'
            }

            // TODO: Figure out how to disable game-based progression tree...
            // A: you can't.  combo of editting researchgroups.sbc and MOD API

            //            MyVisualScriptLogicProvider.BlockFunctionalityChanged += FunctionalityChanged;

            NeedsResearch(refinery, TechGroup.OreProcessing);
            NeedsResearch(blastFurnace, TechGroup.OreProcessing);
 //           NeedsResearch(blastFurnace, TechGroup.Permabanned);
            NeedsResearch(jumpDrive, TechGroup.JumpDrive);
            NeedsResearch(projectorLarge, TechGroup.Permabanned);
            NeedsResearch(projectorSmall, TechGroup.Permabanned);

            NeedsResearch(largeMissileTurret, TechGroup.AdvancedWeapons);
            NeedsResearch(smallMissileTurret, TechGroup.AdvancedWeapons);
            NeedsResearch(rocketLauncher, TechGroup.AdvancedWeapons);
            NeedsResearch(largeRocketLauncher, TechGroup.AdvancedWeapons);
            NeedsResearch(smallReloadableRocketLauncher, TechGroup.AdvancedWeapons);

            NeedsResearch(ionThrusterSmallShipSmall, TechGroup.IonThrusters);
            NeedsResearch(ionThrusterSmallShipLarge, TechGroup.IonThrusters);
            NeedsResearch(ionThrusterLargeShipSmall, TechGroup.IonThrusters);
            NeedsResearch(ionThrusterLargeShipLarge, TechGroup.IonThrusters);

            NeedsResearch(hydroThrusterSmallShipSmall, TechGroup.HyrdrogenThrusters);
            NeedsResearch(hydroThrusterSmallShipLarge, TechGroup.HyrdrogenThrusters);
            NeedsResearch(hydroThrusterLargeShipSmall, TechGroup.HyrdrogenThrusters);
            NeedsResearch(hydroThrusterLargeShipLarge, TechGroup.HyrdrogenThrusters);

            NeedsResearch(SmallBlockSmallGenerator, TechGroup.UraniumReactors);
            NeedsResearch(SmallBlockLargeGenerator, TechGroup.UraniumReactors);
            NeedsResearch(LargeBlockSmallGenerator, TechGroup.UraniumReactors);
            NeedsResearch(LargeBlockLargeGenerator, TechGroup.UraniumReactors);
            NeedsResearch(LargeBlockSmallGeneratorWarfare2, TechGroup.UraniumReactors);
            NeedsResearch(LargeBlockLargeGeneratorWarfare2, TechGroup.UraniumReactors);
            NeedsResearch(SmallBlockSmallGeneratorWarfare2, TechGroup.UraniumReactors);
            NeedsResearch(SmallBlockLargeGeneratorWarfare2, TechGroup.UraniumReactors);


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

            //SE 1.194 V33
            NeedsResearch(LargeHydrogenTankSmall, TechGroup.GasStorage);
            NeedsResearch(SmallHydrogenTankSmall, TechGroup.GasStorage);

            NeedsResearch(EngineLarge, TechGroup.GasStorage);
            NeedsResearch(EngineSmall, TechGroup.GasStorage);

            NeedsResearch(WindTurbineLarge, TechGroup.AtmosphericEngines);

            NeedsResearch(SmallGatlingTurret, TechGroup.BasicWeapons);
            NeedsResearch(SmallRocketLauncherReload, TechGroup.AdvancedWeapons);
            NeedsResearch(InteriorTurret, TechGroup.BasicWeapons);
            NeedsResearch(LargeGatlingTurret, TechGroup.BasicWeapons);

            NeedsResearch(SkLarge, TechGroup.OreProcessing);
            NeedsResearch(SkSmall, TechGroup.OreProcessing);
            NeedsResearch(BasicAssembler, TechGroup.OreProcessing);


            var gameVersion = MyAPIGateway.Session.Version;

            if ((gameVersion.Major == 1 && gameVersion.Minor >= 195) || gameVersion.Major > 1)
            {
                // V1.195 DLC
                NeedsResearch(SmallBlockSmallAtmosphericThrustSciFi, TechGroup.AtmosphericEngines);
                NeedsResearch(SmallBlockLargeAtmosphericThrustSciFi, TechGroup.AtmosphericEngines);
                NeedsResearch(LargeBlockSmallAtmosphericThrustSciFi, TechGroup.AtmosphericEngines);
                NeedsResearch(LargeBlockLargeAtmosphericThrustSciFi, TechGroup.AtmosphericEngines);

                NeedsResearch(SmallBlockSmallThrustSciFi, TechGroup.IonThrusters);
                NeedsResearch(SmallBlockLargeThrustSciFi, TechGroup.IonThrusters);
                NeedsResearch(LargeBlockSmallThrustSciFi, TechGroup.IonThrusters);
                NeedsResearch(LargeBlockLargeThrustSciFi, TechGroup.IonThrusters);
            }

            // V26.  For SE 1.192
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 192) || gameVersion.Major > 1)
            {
                NeedsResearch(SafeZoneBlock, TechGroup.Permabanned);
                NeedsResearch(StoreBlock, TechGroup.Permabanned);
                NeedsResearch(ContractBlock, TechGroup.Permabanned);

                // V27 SE 1.192 Economy DLC
                NeedsResearch(VendingMachine, TechGroup.Permabanned);
                NeedsResearch(AtmBlock, TechGroup.Permabanned);
            }
            //   For SE 1.193
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 193) || gameVersion.Major > 1)
            {
                NeedsResearch(FoodDispenser, TechGroup.Permabanned);
            }
            //   For SE 1.199
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 199) || gameVersion.Major > 1)
            {
                NeedsResearch(IndustrialRefinery, TechGroup.OreProcessing);
                //                NeedsResearch(IndustrialAssembler, TechGroup.Permabanned);

                NeedsResearch(IndustrialLGLargeHTruster, TechGroup.HyrdrogenThrusters);
                NeedsResearch(IndustrialLGSmallHTruster, TechGroup.HyrdrogenThrusters);
                NeedsResearch(IndustrialSGLargeHTruster, TechGroup.HyrdrogenThrusters);
                NeedsResearch(IndustrialSGSmallHTruster, TechGroup.HyrdrogenThrusters);

                NeedsResearch(LargeHydrogenTankIndustrial, TechGroup.GasStorage);
            }
            if ((gameVersion.Major == 1 && gameVersion.Minor >= 200) || gameVersion.Major > 1)
            {
                // these are guns, not turrets
                NeedsResearch(LargeBlockLargeCalibreGun, TechGroup.AdvancedWeapons); // artillery
                NeedsResearch(SmallBlockAutocannon, TechGroup.BasicWeapons);
                NeedsResearch(SmallGatlingGunWarfare2, TechGroup.BasicWeapons);

                // encourage engineering
                NeedsResearch(LargeTurretControlBlock, TechGroup.BasicWeapons);
                NeedsResearch(SmallTurretControlBlock, TechGroup.BasicWeapons);

                NeedsResearch(SmallMissileLauncherWarfare2, TechGroup.AdvancedWeapons);

                NeedsResearch(LargeRailgun, TechGroup.SpaceWeapons);
                NeedsResearch(SmallRailgun, TechGroup.SpaceWeapons);

                // artillery
                NeedsResearch(LargeCalibreTurret, TechGroup.AdvancedWeapons);
                
                NeedsResearch(LargeBlockMediumCalibreTurret, TechGroup.MarsGroundWeapons);
                NeedsResearch(AutoCannonTurret, TechGroup.MarsGroundWeapons);
                NeedsResearch(SmallBlockMediumCalibreTurret, TechGroup.MarsGroundWeapons);

                NeedsResearch(SmallBlockSmallModularThruster, TechGroup.IonThrusters);
                NeedsResearch(SmallBlockLargeModularThruster, TechGroup.IonThrusters);
                NeedsResearch(LargeBlockSmallModularThruster, TechGroup.IonThrusters);
                NeedsResearch(LargeBlockLargeModularThruster, TechGroup.IonThrusters);

            }
            //V44 SE 1.203
            NeedsResearch(LargeBlockSmallFlatAtmosphericThrustDShape, TechGroup.AtmosphericEngines);
            NeedsResearch(SmallBlockSmallFlatAtmosphericThrustDShape, TechGroup.AtmosphericEngines);
            NeedsResearch(LargeBlockSmallFlatAtmosphericThrust, TechGroup.AtmosphericEngines);
            NeedsResearch(SmallBlockSmallFlatAtmosphericThrust, TechGroup.AtmosphericEngines);
            NeedsResearch(LargeBlockLargeFlatAtmosphericThrustDShape, TechGroup.AtmosphericEngines);
            NeedsResearch(SmallBlockLargeFlatAtmosphericThrustDShape, TechGroup.AtmosphericEngines);
            NeedsResearch(LargeBlockLargeFlatAtmosphericThrust, TechGroup.AtmosphericEngines);
            NeedsResearch(SmallBlockLargeFlatAtmosphericThrust, TechGroup.AtmosphericEngines);

            NeedsResearch(LargeBlockWindTurbineReskin, TechGroup.AtmosphericEngines);

            // welders seem to bypass the research contraints
            NeedsResearch(SmallBlockWelder, TechGroup.Permabanned);
            NeedsResearch(LargeBlockWelder, TechGroup.Permabanned);

            NeedsResearch(SmallBlockDrill, TechGroup.OreProcessing);
            NeedsResearch(LargeBlockDrill, TechGroup.OreProcessing);

        }

        public void AllowUnlockedTechs()
        {
            //            ModLog.Info("AllowUnlockTechs():" + UnlockedTechs.Count.ToString() + " unlocked groups");
            UnlockTechsSilently(0, UnlockedTechs);
        }

        public bool IsTechUnlocked(TechGroup techgroup)
        {
            if (UnlockedTechs.Contains(techgroup))
            {
                return true;
            }
            return false;
        }

        private void NeedsResearch(MyDefinitionId techDef, TechGroup techgroup)
        {
            if (techDef == null)
            {
                ModLog.Info("Request for NULL techDef");
                return;
            }
            if(techDef.TypeId == MyObjectBuilderType.Invalid)
            {
                ModLog.Info("Request for invalid techDef");
                return;

            }

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
            /* joining players may not have gotten the message...
            if (UnlockedTechs.Contains(techGroup))
            {
                //                ModLog.Info("UTGFAP():" + UnlockedTechs.Count.ToString() + " unlocked groups. Already contains TechGroup:"+techGroup.ToString());
                return; // Already unlocked
            }
            */

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
                    if (bNewResearch)
                    {
                        MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
                    }
                    else
                    {
                        //                        ModLog.Info("Old research Method: Unlock for player:" + player.IdentityId.ToString() + " tech=" + technology.ToString());
                        MyVisualScriptLogicProvider.PlayerResearchUnlock(player.IdentityId, technology);
                    }
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
                case TechGroup.MarsGroundWeapons:
                    return AudioClip.UnlockedMissiles; // TODO: need to re-record
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
                    if (bNewResearch)
                        // unknown: does this work for ALL players?
                        MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
                    else
                    {
                        MyVisualScriptLogicProvider.PlayerResearchUnlock(playerId, technology);
                    }
                }
            }
        }

        public void UnlockTechForJoiningPlayer(long playerId)
        {
            ModLog.Info("Joining Player" + playerId.ToString());

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
                    if (bNewResearch)
                        MyVisualScriptLogicProvider.ResearchListRemoveItem(technology); // SE 1.189
                    else
                        MyVisualScriptLogicProvider.PlayerResearchUnlock(playerId, technology);
                }
            }
        }
    }
}

