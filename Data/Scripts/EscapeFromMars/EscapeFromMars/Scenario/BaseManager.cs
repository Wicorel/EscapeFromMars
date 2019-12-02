using System;
using System.Collections.Generic;
using Duckroll;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
//using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

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
            // TODO: what if ALL bases are taken out?
            // TODO: what if the air base (convoy target) is taken out?  what happens to existing convoys? What about new convoys?
			foreach (var gCorpBase in bases)
			{
                //TODO: spread out these updates across bases?
                //TODO: put this in Update10() instead and above.
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
            BlocksFixup(grid);

            if (grid.IsStatic && grid.IsControlledByFaction("GCORP"))
            {
                var slimBlocks = new List<IMySlimBlock>();
                grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyRemoteControl);
                foreach (var slim in slimBlocks)
                {
                    var remoteControl = slim.FatBlock as IMyRemoteControl;
                    if (remoteControl.IsControlledByFaction("GCORP") 
                        && remoteControl.CustomName.Contains("DELIVERY")// AIR_DELIVERY_SPAWNER // GROUND_DELIVERY_SPAWNER //Remote Control
                        )
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

        /// <summary>
        /// Fix up the blocks in ALL grids to have correct values
        /// This routine could also be used to set text panels for other languages.
        /// 
        /// </summary>
        /// <param name="grid"></param>
        void BlocksFixup(IMyCubeGrid grid)
        {

            // fix up the Gatling blocks to have max range //V28
            var slimBlocksG = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocksG, b => b.FatBlock is IMyLargeGatlingTurret);
            foreach (var slim in slimBlocksG)
            {
                var gatling = slim.FatBlock as IMyLargeGatlingTurret;
 //              gatling.Range get only :(
            }
            // fix up the LCD blocks with show on hud // V27
            var slimBlocksT = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocksT, b => b.FatBlock is IMyTextPanel);
            foreach (var slim in slimBlocksT)
            {
                var textPanel = slim.FatBlock as IMyTextPanel;
                textPanel.ShowOnHUD = false;
            }

            // fix up the beacon blocks  // V26
            var slimBlocksB = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocksB, b => b.FatBlock is IMyBeacon);
            foreach (var slim in slimBlocksB)
            {
                var beacon = slim.FatBlock as IMyBeacon;
                if(beacon.CustomName.Contains("CLEANCE"))
                { 
                    // beacon in HQ has spelling error
                    string sName = beacon.CustomName;
                    ModLog.Info("Fixing Beacon Text:" + sName);
                    beacon.CustomName = sName.Replace("CLEANCE", "CLEARANCE");
                }
            }

            // fix up the text panel blocks
            var slimBlocks = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocks, b => b.FatBlock is IMyTextPanel);
            foreach (var slim in slimBlocks)
            {
                var textPanel = slim.FatBlock as IMyTextPanel;
                //bool bShow = textPanel.GetValueBool("ShowTextOnScreen");
                // V 1.190
                bool bShow=textPanel.ContentType == VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                //                bool bShow = textPanel.ShowOnScreen != VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.NONE;
                if (bShow)
                {
                    // We've already set this up once before (or world was created post 1.189

                    /* Try to fix text not showing on text panels
                     * 
                     * (on further testing, panels are showing.. maybe Keen fixed this themselves?)
                     * 
                     * Saw on Rity's stream that they were NOT showing in all places.
                     * And on EpikTek's https://www.youtube.com/watch?v=CkpGGPZd78k
                     * */
//                    textPanel.SetValue("ShowTextOnScreen", false);
//                    textPanel.SetValue("ShowTextOnScreen", true);
                    //                      textPanel.SetShowOnScreen(VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.PUBLIC);


                    // Could set text of text panels here to be language specific
  //                  switch (textPanel.EntityId)
                    {
                        //Crashed Shuttle:92770753627258475
                        //{ X: 1868088.00058876 Y: -2003485.99356789 Z: 1316645.85240929}
                        /* Example of setting text:
                        case 121786820996539580: //TEXT!
                            textPanel.WritePublicText(
                                "OUTGOING TRANSMISSION\n"
                               + "ERROR: FAILED TO SEND\n"
                            + "\n"
                            + "M,\n"
                            + "I'm going to Mars to talk to\n"
                            + "he CEO about what that\n"
                            + "weasel Bhaskar has been up to.\n"
                            + "I think we can trust him.\n"
                            + "                                \n"
                            + "-T\n"
                            );
                            break;
                         */
                    }
                }
// V26                else
                {
                    var strings = new List<string>();
                    textPanel.GetSelectedImages(strings);
                    textPanel.TextPadding = 0; //V26
                    if (strings.Count < 1)
                    {
                        // note: better method would be use use .CustomData of the textpanels.
                        // Using this EntityID method to be backward compatible with player's existing worlds.
                        switch (textPanel.EntityId)
                        {
                            //            long buildGuideID = 80461927256500036;

                            case 80461927256500036: // Crash ship build info screen
                                /* "old" text
Mabel: Loading survival guide...

> Recommend searching cargo
   and disassembling shuttle for 
   rover parts
> Recommend configuration:
    - Six wheels
    - Friction 10%
    - Damping 30%
    - Strength ~5% 
               (Depends on load)
    - Speed Limit 100km/h
*/
                                textPanel.WriteText("Mabel: Loading survival guide..."+
"\n"+
"\n> Recommend searching cargo"+
"\n   and disassembling shuttle for"+
"\n   rover parts"+
"\n> Recommend configuration:"+
"\n    -Six wheels"+
"\n    - Friction 60%"+
"\n    - *Strength 10%" +
"\n    - *Power 50%" +
"\n    - Speed Limit 50km/h" +
"\n  * =Depends on load"
);
                                break;

                            /*
                            ((Ice Mine Entrance))
Static Grid 1300:141864706275857195
{ X: 1869175.27852051 Y: -2004745.97266307 Z: 1316375.91819424}
                            -----TEXTPANELS
74525033656413945:1:+LCD Panel 2
| Galactic Corporation Logo
*/
                            case 74525033656413945:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;
                            // MIKI FIXUP
                            case 81986956045310309:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 103164071082162108:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 120342266177165062:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 96762650175047086:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 78808963600022563:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 102592293973822089:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 93732106466977771:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 119196087123513740:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 115470908890383058:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 84905149027762603:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 73748511314883089:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 108707493869969783:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 107003280298094914:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 93717234609661072:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;
                            case 100685062896355827:
                                textPanel.AddImageToSelection("MikiScrap");
                                break;

                            //green house
                            case 110846399081478285:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;

                            //                                Signal Station:139030682896359375
                            //{ X: 1844500.54997961 Y: -1995403.72976435 Z: 1323630.52767273}
                            case 132717438306395557:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;

                            //                                ((MRE MedBay 2))
                            //MRE Emergency Medical Station:120085596920753880
                            //{ X: 1844533.71376673 Y: -1995424.91354928 Z: 1323629.33162825}
                            case 104050761648970549:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;

                            // {X:1856577.01869837 Y:-1999257.98373971 Z:1321414.17733138}
                            case 110250540272021112:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;

                            //  {X:1857176.1781222 Y:-2000013.37619576 Z:1321553.01815169}
                            case 89933889116515952:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;

                            //((and another HQ SAM))
                            //GCorp Mobile SAM: 89611551362269325
                            //{ X: 1857213.83363977 Y: -1998518.53635201 Z: 1320657.55013411}
                            case 95067624877541028:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;

                            //((another HQ SAM))
                            //GCorp Mobile SAM: 104043950566026386
                            //{ X: 1858538.51759973 Y: -1999989.95859322 Z: 1320777.02901595}
                            case 120326586562634149:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;

                            //((Bunker in weapons Research))
                            //GCorp Bunker:132649980418733079
                            //{ X: 1843291.04608973 Y: -1996439.44943181 Z: 1324491.09281518}
                            case 102565050409374890:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 76555655031247133:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 82207545563969038:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 140611613672853263:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;

                            //((Air Research))
                            //GCorp Flight Research Center:76779347935557670
                            //{ X: 1854754.1883761 Y: -2005852.06052194 Z: 1325419.84578841}
                            case 91899503586440685:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 89540866723022250:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 139513180460721275:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 123742114812299814:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 72707843196346809:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 84058947393377852:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 124309978476859628:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 85895320964444780:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 117967784354173602:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 128468555565951583:
                                textPanel.AddImageToSelection("Clouds");
                                break;
                            case 107231006349635239:
                                textPanel.AddImageToSelection("Clouds");
                                break;

                            //((MRE hidden Base))
                            //MRE Experiment Base: 104361129531664144
                            //{ X: 1858398.21613573 Y: -1989137.98910994 Z: 1312706.48643797}
                            case 98971232666660757:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;
                            case 97512105090353134:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;

                            case 81228556864103207:
                                textPanel.AddImageToSelection("MRE Logo");
                                break;

                            //((HQ))
                            //GCorp HQ Tower: 144104082158837389
                            //{ X: 1857310.65834681 Y: -1999316.80991158 Z: 1321066.96761458}
                            case 125129893494374181:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 134131747655270782:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 74727284886995000:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 98708608904781783:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 138656136990359379:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 80870473015345748:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;
                            case 98370398719554346:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 80724503483430325:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 87324639907322082:
                                textPanel.AddImageToSelection("White screen");
                                break;
                            case 110549946960318740:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 142860245424722839:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 117922822138601150:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;

                            // ((HQ Rocket))
                            //GCorp Space Transport: 97585502667028994
                            //{ X: 1857324.30678431 Y: -1999293.08872018 Z: 1321075.17171614}
                            case 140237983630899577:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                break;
                            case 105805752262318800:
                                textPanel.AddImageToSelection("Arrow");
                                break;
                            case 104593951810486921:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 141641456443241951:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 101582007425482292:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;
                            case 80366112655444611:
                                textPanel.AddImageToSelection("Galactic Corporation Logo");
                                textPanel.AddImageToSelection("Automation Simplified");
                                break;

                        }
                    }
                }
            }

            // fix up the sound blocks
            var slimBlocks2 = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocks2, b => b.FatBlock is IMySoundBlock);
            foreach (var slim in slimBlocks2)
            {
                var soundBlock = slim.FatBlock as IMySoundBlock; // Fixed V21
//                if (soundBlock == null) continue; // why do we need this?
                switch (soundBlock.EntityId)
                {
                    // air base alpha:
                    case 116378614635193269:
                        soundBlock.SelectedSound = "WelcomeToGcorp";
                        break;

                    // ice mine entrance
                    case 101979433782763108:
                        soundBlock.SelectedSound = "NotAuthorisedDeployDefences";
                        break;

                    // ice mine shaft botom
                    case 106640376870960334:
                        soundBlock.SelectedSound = "MineClosed";
                        break;

                    //((Upper Ice Mine Meeting Room))
                    //{ X: 1869176.67818993 Y: -2004930.50366838 Z: 1316377.58089576}
                    case 80185389104537910:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;

                    // MRE HIdden base
                    case 95206901925432014:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 100498114358496283:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 77589249328205128:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 104066113506074975:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 90866709199081947:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 141427853315146800:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 116242415031561830:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 110850485077900653:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;
                    case 88298229398547791:
                        soundBlock.SelectedSound = "BigSwitch";
                        break;


                    // wrecked drone
                    case 102293828213773618:
                        soundBlock.SelectedSound = "PowerUpClipped";
                        break;

                    //((old drone @ hangar 1))
                    case 108074122634121508:
                        soundBlock.SelectedSound = "PowerUpClipped";
                        break;

                    // air research
                    case 84998344586050021:
                        soundBlock.SelectedSound = "SoundBlockAlert2";
                        break;
                    case 91070301367474416:
                        soundBlock.SelectedSound = "AirplaneSound";
                        break;
                    case 90629854631902381:
                        soundBlock.SelectedSound = "FlightResearchExhibition";
                        break;

                    // air base beta
                    case 130193226083241264:
                        soundBlock.SelectedSound = "WelcomeToGcorp";
                        break;

                    // mech
                    case 82762879450865423:
                        //            bodyGrid.SetSoundBlocks("Mech Intruders Must Be Destroyed"); // fix missing sound on sound block on mech
                        soundBlock.SelectedSound = "IntruderRobot";
                        break;

                    // HQ
                    case 95872484442737911:
                        soundBlock.SelectedSound = "SoundBlockAlert1";
                        break;
                    case 82604291774017685:
                        soundBlock.SelectedSound = "WelcomeToGcorp";
                        break;

                    // MIKI
                    case 129379111872365143:
                        soundBlock.SelectedSound = "Carmen";
                        break;

                    case 140249950038454545:
                        soundBlock.SelectedSound = "LavaLoop";
                        break;


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
        internal GCorpBase FindBaseNear(Vector3D position)
        {
            GCorpBase nearestBase = null;
            var closestDistance = double.MaxValue;
            foreach (var gCorpBase in bases)
            {
                var distSquared = Vector3D.DistanceSquared(gCorpBase.RemoteControl.GetPosition(), position);
                if (distSquared < closestDistance)
                {
                    closestDistance = distSquared;
                    nearestBase = gCorpBase;
                }
            }
            return nearestBase;
        }

        internal void ClearBaseBackupRequests()
        {
            foreach (var gCorpBase in bases)
            {
                gCorpBase.ClearBackup();
            }
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

        public string BaseInfo()
        {
            string str = "";
            int count = bases.Count;
            str += "#Bases=" + count;
            foreach(var gcorpbase in bases)
            {
                str += "\n " + gcorpbase.RemoteControl.CustomName;
            }
            return str;
        }
    }
}