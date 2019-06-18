using System.Collections.Generic;
using Duckroll;
using VRage.Game;

namespace EscapeFromMars
{
	internal class Speaker
	{
        //https://www.youtube.com/watch?v=e4Be2itSVWI

        // Mabel (female): https://translate.google.com/  Echo delay 0.05 Decay 0.5
        // Convoy (female): Mabel?  or maybe https://www.naturalreaders.com/online/ EnglishUS/Heather  
        // Patrol Suppport (male): maybe http://ttsdemo.com/ Steven(US) or Tom(US)
        // Miki (Crazy male russian):
        // Mech: (Robot)
        // MrEd: (highly procesed computer voice) (male?)
        internal static readonly Speaker Mabel = new Speaker("Mabel", MyFontEnum.Green);
		internal static readonly Speaker GCorp = new Speaker("GCorp Transmission", MyFontEnum.Red);
		internal static readonly Speaker Mech = new Speaker("Experimental Mech", MyFontEnum.Red);
		internal static readonly Speaker MrEd = new Speaker("MR.ED", MyFontEnum.Blue);
		internal static readonly Speaker Miki = new Speaker("Miki", MyFontEnum.White);
		internal static readonly Speaker None = new Speaker("None", MyFontEnum.BuildInfo);

// radio sound  https://www.youtube.com/watch?v=YantpouC4Mk

		internal string Name { get; }
		internal MyFontEnum Font { get; }

		private Speaker(string name, MyFontEnum font)
		{
			Font = font;
			Name = name;
		}
	}

	public class AudioClip : IAudioClip
	{
		private static int _nextId = 1;
		private static readonly Dictionary<int, AudioClip> Index = new Dictionary<int, AudioClip>();

		internal static readonly AudioClip Cavern = Create("Cavern", EscapeFromMars.Speaker.None, "", 4000);

		internal static readonly AudioClip ConvoyArrivedSafely = Create("ConvoyArrivedSafely", EscapeFromMars.Speaker.GCorp,
			"*Muffled* Convoy arrived safely", 4000);

		internal static readonly AudioClip ConvoyDispatched1 = Create("ConvoyDispatched1", EscapeFromMars.Speaker.GCorp,
			"*Muffled* Convoy dispatched", 7000);

		internal static readonly AudioClip ConvoyDispatched2 = Create("ConvoyDispatched2", EscapeFromMars.Speaker.GCorp,
			"*Muffled* Convoy dispatched", 7000);

        internal static readonly AudioClip ConvoyDispatched3 = Create("ConvoyDispatched3", EscapeFromMars.Speaker.GCorp,
            "*Muffled* Convoy dispatched", 6000);



        internal static readonly AudioClip SteelPlateConvoyDispatched = Create("SteelPlateConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Steel Plate Convoy dispatched", 6000);
        internal static readonly AudioClip MetalGridConvoyDispatched = Create("MetalGridConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Metal Grid Convoy dispatched", 6000);

        internal static readonly AudioClip ConstructionConvoyDispatched = Create("ConstructionConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Construction Convoy dispatched", 6000);

        internal static readonly AudioClip InteriorPlateConvoyDispatched = Create("InteriorPlateConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Interior Plate Convoy dispatched", 6000);

        internal static readonly AudioClip GirderConvoyDispatched = Create("GirderConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Girder Convoy dispatched", 6000);

        internal static readonly AudioClip SmallTubeConvoyDispatched = Create("SmallTubeConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Small Steel Tube Convoy dispatched", 6000);
        internal static readonly AudioClip LargeTubeConvoyDispatched = Create("LargeTubeConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Large Steel Tube Convoy dispatched", 6000);
        internal static readonly AudioClip MotorConvoyDispatched = Create("MotorConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Motor Convoy dispatched", 6000);
        internal static readonly AudioClip DisplayConvoyDispatched = Create("DisplayConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Display Convoy dispatched", 6000);

        internal static readonly AudioClip BulletproofGlassConvoyDispatched = Create("BulletproofGlassConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Bulletproof Glass Convoy dispatched", 6000);

        internal static readonly AudioClip ComputerConvoyDispatched = Create("ComputerConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Computer Convoy dispatched", 6000);

        internal static readonly AudioClip ReactorConvoyDispatched = Create("ReactorConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Reactor Convoy dispatched", 6000);

        internal static readonly AudioClip MedicalConvoyDispatched = Create("MedicalConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Medical Convoy dispatched", 6000);

        internal static readonly AudioClip RadioCommunicationConvoyDispatched = Create("RadioCommunicationConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "RadioCommunication Convoy dispatched", 6000);

        internal static readonly AudioClip ExplosivesConvoyDispatched = Create("ExplosivesConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Explosives Convoy dispatched", 6000);

        internal static readonly AudioClip SolarCellConvoyDispatched = Create("SolarCellConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Solar Cell Convoy dispatched", 6000);
        internal static readonly AudioClip PowerCellConvoyDispatched = Create("PowerCellConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Power Cell Convoy dispatched", 6000);
        internal static readonly AudioClip NATO_5p56x45mmConvoyDispatched = Create("NATO56x45mmConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "NATO 56x45mm Convoy dispatched", 6000);
        internal static readonly AudioClip NATO25x184mmConvoyDispatched = Create("NATO25x184mmConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "NATO 25x184mm Convoy dispatched", 6000);
        internal static readonly AudioClip Missile200mmConvoyDispatched = Create("Missile200mmConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Missile 200mm Convoy dispatched", 6000);
        internal static readonly AudioClip UraniumConvoyDispatched = Create("UraniumConvoyDispatched", EscapeFromMars.Speaker.GCorp,
            "Uranium Convoy dispatched", 6000);


        internal static readonly AudioClip ConvoyUnderThreat = Create("ConvoyUnderThreat", EscapeFromMars.Speaker.GCorp,
			"Convoy under threat! Possible pirate activity", 4000);

		internal static readonly AudioClip DisengagingFromHostile = Create("DisengagingFromHostile",
		EscapeFromMars.Speaker.GCorp, "Disengaging from hostile");

		internal static readonly AudioClip DroneDisarmed = Create("DroneDisarmed", EscapeFromMars.Speaker.GCorp, // USED
			"Drone disarmed and returning for repairs");

		internal static readonly AudioClip EnemyDetectedMovingToIntercept = Create("EnemyDetectedMovingToIntercept", // USED
			EscapeFromMars.Speaker.GCorp, "Enemy detected, moving to intercept");

		internal static readonly AudioClip FacilityDetectedHostile = Create("FacilityDetectedHostile",
			EscapeFromMars.Speaker.GCorp, "Facility scanners have picked up a hostile. Dispatch nearby patrols to " +
			                              "location",
			 6000);

		internal static readonly AudioClip GCorpBlockingSignals = Create("GCorpBlockingSignals",									// USED
			EscapeFromMars.Speaker.Mabel,
			@"Our adversary has full control of the Martian surface. 
All of the initial colonists were removed following their acquistion of exclusive mining rights. 
G-Corp is blocking all signals from the planet except their own.",
			13000);

		internal static readonly AudioClip GCorpFacilitiesHeavilyArmed = Create("GCorpFacilitiesHeavilyArmed",					// USED
			EscapeFromMars.Speaker.Mabel,
			@"Scanning shows G-Corp facilities in the area are carefully guarded, 
approaching one is not recommended unless heavily armed yourself",
			8000);

		internal static readonly AudioClip GCorpFacilityThreatened = Create("GCorpFacilityThreatened",
		EscapeFromMars.Speaker.GCorp,
			// USED
			"G-Corp facility threatened, dispatching additional drones");

		internal static readonly AudioClip GCorpTowerScan = Create("GCorpTowerScan", EscapeFromMars.Speaker.Mabel,
		// USED
			@"I am detecting unusual readings inside the G-Corp headquarters tower.
It may be a way to leave the planet, however safer options may exist [This area is extremely dangerous]",
			10000);

		internal static readonly AudioClip HostileDisappeared = Create("HostileDisappeared", EscapeFromMars.Speaker.GCorp,
		// USED
			"Hostile has disappeared from scanners. Resume patrol",
			5000);

		internal static readonly AudioClip HostileStillPresent = Create("HostileStillPresent", EscapeFromMars.Speaker.GCorp,
			"Hostile still present. Requesting reinforcements immediately",
			5000);

		internal static readonly AudioClip IntruderRobot = Create("IntruderRobot", EscapeFromMars.Speaker.Mech,
			"Intruder!",
			2000);

		internal static readonly AudioClip IntrudersMustBeDestroyedRobot = Create("IntrudersMustBeDestroyedRobot",
		EscapeFromMars.Speaker.Mech,
			"Intruders must be destroyed",
			3000);

		internal static readonly AudioClip LocatedIceMine = Create("LocatedIceMine", EscapeFromMars.Speaker.Mabel,
		// USED
			@"Scanning for source of Oxygen. Located GCorp controlled ice mine.
Uploading coordinates to suit HUD.", 7000);

		internal static readonly AudioClip MarsGCorpOperationsExplained = Create("MarsGCorpOperationsExplained",
		EscapeFromMars.Speaker
		.Mabel,	// USED
			@"Mars is littered with G-Corp facilities and operations. They have exclusive access to the planet's surface since the
colonists were relocated. Magnesium, aluminium, titanium, iron, and chromium make up the majority of their exports.", 16000);

		internal static readonly AudioClip MilitaryPatrolInitiated = Create("MilitaryPatrolInitiated",
		EscapeFromMars.Speaker.GCorp,
		// USED
			"Military patrol initiated, searching for hostiles");

		internal static readonly AudioClip OldFreighterFound = Create("OldFreighterFound", EscapeFromMars.Speaker.Mabel,
		// USED
			@"Scans show an ancient Iguana class freighter. None of its systems are active.
Repairing it will take some time, but it may also be your only way to leave Mars safely", 11000);

		internal static readonly AudioClip OxygenGeneratorUnlocked = Create("OxygenGeneratorUnlocked",
		EscapeFromMars.Speaker.Mabel,
		// USED
			"Searching G-Corp data files. Oxygen generator technology unlocked!", 5000);

		internal static readonly AudioClip PowerUpClipped = Create("PowerUpClipped", EscapeFromMars.Speaker.None, "");

        internal static readonly AudioClip MabelPowerUpClipped = Create("PowerUpClipped", EscapeFromMars.Speaker.Mabel, "Powering Up Systems");
        internal static readonly AudioClip MabelUncoveringFiles = Create(null, EscapeFromMars.Speaker.Mabel, "Decrypting Computer files");

        internal static readonly AudioClip RingAroundMars = Create("RingAroundMars", EscapeFromMars.Speaker.Mabel,
		// USED
			@"By my calculations, in 27.6 million years Phobos will be torn apart by gravitational forces,
leading to the creation of a ring around Mars", 10000);

		internal static readonly AudioClip ShuttleDamageReport = Create("ShuttleDamage", EscapeFromMars.Speaker.Mabel,
		// USED
			@"Shuttle has taken critical damage. Communications offline. Engines not found. 
Multiple GCorp drones detected nearby",
			9000);

		internal static readonly AudioClip ShuttleDatabanks = Create("ShuttleDatabanks", EscapeFromMars.Speaker.Mabel,
			// USED
			@"Shuttle data-banks do not contain construction blueprints for atmospheric or hydrogen based propulsion.
Recommend investigating G-Corp computer storage",
			10000);

		internal static readonly AudioClip SuggestPiracy = Create("SuggestPiracy", EscapeFromMars.Speaker.Mabel,
			// USED
			@"I have calculated the optimal method to acquire resources is by intercepting G-Corp cargo transports.
However, be aware that they may increase security if too many of their shipments go missing",
			12000);

		internal static readonly AudioClip TargetFleeingPursuit = Create("TargetFleeingPursuit", EscapeFromMars.Speaker.GCorp,	// USED
			"Target fleeing pursuit, drones return to base");

		internal static readonly AudioClip TargetFoundDronesAttack = Create("TargetFoundDronesAttack",
		EscapeFromMars.Speaker.GCorp,
			"Target found, all drones attack!");

		internal static readonly AudioClip TargetIdentifiedUnitsConverge = Create("TargetIdentifiedUnitsConverge",
		EscapeFromMars.Speaker.GCorp, // USED
			"Target identified, all units converge!");

		internal static readonly AudioClip TargetLost = Create("TargetLost", EscapeFromMars.Speaker.GCorp, // USED
			"Target lost, return to positions");

		internal static readonly AudioClip TurretsAtTheMine = Create("TurretsAtTheMine", EscapeFromMars.Speaker.Mabel,
			"Detecting active computers around the mine. Be cautious of G-Corp  security turrets",
			6000);

		internal static readonly AudioClip UnknownHostileOnScanners = Create("UnknownHostileOnScanners",
		EscapeFromMars.Speaker.GCorp,
		// USED
			"Unknown hostile showing up on scanners. Engaging");

		internal static readonly AudioClip UnlockAtmospherics = Create("UnlockAtmospherics", EscapeFromMars.Speaker
		.Mabel, "Searching G-Corp data files. Atmospheric thruster technology unlocked!", 5000);

		internal static readonly AudioClip UnlockedMissiles = Create("UnlockedMissiles", EscapeFromMars.Speaker.Mabel,
			"Searching G-Corp data files. Missile technology unlocked!", 5000);

		internal static readonly AudioClip OxygenFarmUnlocked = Create("OxygenFarmUnlocked", EscapeFromMars.Speaker.Mabel,
		// USED
				"Searching data files. Oxygen Farm technology unlocked!", 5000);

        // V12
        internal static readonly AudioClip BasicWeaponsUnlocked = Create("BasicWeaponsUnlocked", EscapeFromMars.Speaker.Mabel,
        "Searching files. Basic Weapon technology unlocked!", 5000);

        internal static readonly AudioClip DistressBeaconSilly = Create("DistressBeaconSilly", EscapeFromMars.Speaker.Mabel,
			// USED
			@"I am picking up a corporation distress beacon nearby. Be careful. It may be a trap, or pirates.+
Either way, you would die horribly.", 9000);

		internal static readonly AudioClip EscapedMars = Create("EscapedMars", EscapeFromMars.Speaker.Mabel, // USED
		    "We have successfully escaped the Martian gravity well. No pursuit detected.", 5000);

		internal static readonly AudioClip FlightResearchCenter = Create("FlightResearchCenter", EscapeFromMars.Speaker
		.Mabel, // USED
			@"Corporation records indicate there is a flight research station on Mars with technology blueprints you may find useful. 
Uploading coordinates to suit hud.", 9000);

		internal static readonly AudioClip OlympusMons = Create("OlympusMons", EscapeFromMars.Speaker.Mabel,	// USED
			@"You are approaching Olympus Mons, the largest volcano in the solar system. 
Volcanoes initially helped to create an atmosphere on Mars, until the hot spots fuelling them cooled.", 11000);

		internal static readonly AudioClip WeaponsResearchFacility = Create("WeaponsResearchFacility",
		EscapeFromMars.Speaker.Mabel,
			@"Through data-mining corporation transport paths, I have determined there is an illegal weapons-research facility hidden at this location.
Uploading to suit hud.", 9000);

		internal static readonly AudioClip GasStorageUnlocked = Create("GasStorageUnlocked", EscapeFromMars.Speaker.Mabel,
		//USED
			"Searching G-Corp data files. Gas storage technology unlocked.", 4000);

		internal static readonly AudioClip InterceptingTransmissions = Create("InterceptingTransmissions",
		EscapeFromMars.Speaker.Mabel,
		//USED
			@"I have diverted 18 percent of my processing power to intercepting and decoding G Corp transmissions.
I will relay them to your suit if they seem relevant.", 9000);

		internal static readonly AudioClip ExplosivesNearby = Create("ExplosivesNearby", EscapeFromMars.Speaker.Mabel,
			"I am detecting explosive compounds nearby.", 3000);

		internal static readonly AudioClip PowerSignatureBehindWall = Create("PowerSignatureBehindWall",
		EscapeFromMars.Speaker.Mabel,
			"There is a power signature coming from behind one of the walls.", 3000);

		internal static readonly AudioClip AllTechUnlocked = Create("AllTechUnlocked", EscapeFromMars.Speaker.Mabel,
		//USED
	        "All technologies unlocked.", 2000);

	    internal static readonly AudioClip EndCredits = Create("EndCredits", EscapeFromMars.Speaker.Mabel,    //USED
	        @"... Transmitting data on all frequencies...
[Scenario is complete. It is recommended to remove the EscapeFromMars Mod if you want to carry on playing]", 120000);

	    internal static readonly AudioClip NotifyOfSatellite = Create("NotifyOfSatellite", EscapeFromMars.Speaker.Mabel,
	    //USED
	        @"I have computed the location of the nearest geosynchronous communications satellite.
If you manage to leave the planet, you should direct your ship there so I can transmit the evidence we gathered on the corporation to all public channels.", 14000);

		internal static readonly AudioClip FoundFilesOnNetwork = Create("FoundFilesOnNetwork", EscapeFromMars.Speaker.Mabel,
			@"While you were fumbling your way through the base, I found hidden files on the computer network.
Directing output to the nearest display.", 8000);

		internal static readonly AudioClip IceMineFoundByAccident = Create("IceMineFoundByAccident",
		EscapeFromMars.Speaker.Mabel,
			"You have located a disused corporation ice mining facility. This will be a valuable resource for oxygen generation.", 8000);

		internal static readonly AudioClip AccidentallyFoundFlightResearchCenter = Create
		("AccidentallyFoundFlightResearchCenter", EscapeFromMars.Speaker.Mabel,
			"This appears to be a lightly defended research center. It could contain valuable research for building your own flying craft.", 7000);

		internal static readonly AudioClip WeaponsFacilityFoundByAccident = Create("WeaponsFacilityFoundByAccident",
		EscapeFromMars.Speaker.Mabel,
			"This storage facility is not on any official maps. I recommend investigating it thoroughly.", 6000);

		internal static readonly AudioClip OhDear = Create("OhDear", EscapeFromMars.Speaker.MrEd,
			@"Oh dear, you are not supposed to be in here. This is Mars Research Expedition property.
Please display your identity badge or exit the building.", 9000);

		internal static readonly AudioClip WelcomeBack = Create("WelcomeBack", EscapeFromMars.Speaker.MrEd,
			@"Welcome back anonymous re-re-re-searcher! 
The elevator is ready to take you dow-dow-down to the experiment area.", 8000);

		internal static readonly AudioClip ExperimentProgress = Create("ExperimentProgress", EscapeFromMars.Speaker.MrEd,
			@"Experiment completion progress at... 342 percent. 
Atmosphere is 95% nitrogen, 3% oxygen, 1% argon, trace amounts of carbon dioxide.", 13000);

		internal static readonly AudioClip ElevatorHere = Create("ElevatorHere", EscapeFromMars.Speaker.Mabel,
			"Blueprints show there should be an elevator here. Strange. They must be outdated.", 6000);

		internal static readonly AudioClip ArmorVehicles = Create("ArmorVehicles", EscapeFromMars.Speaker.Mabel,
			@"You should try to build your vehicles prepared for combat. 
For especially hostile areas, you could use drones instead of risking your own life.", 9000);

		internal static readonly AudioClip FaintPowerSignature = Create("FaintPowerSignature", EscapeFromMars.Speaker
		.Mabel,
			"I am detecting a faint power signature...", 2000);

		internal static readonly AudioClip MreDefunded = Create("MreDefunded", EscapeFromMars.Speaker.Mabel,
			@"The Mars Research Expedition was defunded after their terraforming projects on mars failed.
However, they also ran several emergency medical facilities around the planet.
Some of them may still be in working order.", 13000);

		internal static readonly AudioClip PursuitEvaded = Create("PursuitEvaded", EscapeFromMars.Speaker.GCorp,
			"Pursuit evaded, resuming standard course and heading", 3000);

		internal static readonly AudioClip SensorsLostTrack = Create("SensorsLostTrack", EscapeFromMars.Speaker.GCorp,
			"Sensors lost track of hostile", 2000);

		internal static readonly AudioClip HackingSound = Create("HackingSound", EscapeFromMars.Speaker.None,	"", 14000);

		internal static readonly AudioClip ConnectionLostSound = Create("ConnectionLostSound", EscapeFromMars.Speaker.None, "", 2000);

		internal static readonly AudioClip HackFinished = Create("HackFinished", EscapeFromMars.Speaker.None, "", 2000);

		internal static readonly AudioClip BestCustomer = Create("BestCustomer", EscapeFromMars.Speaker.Miki,
			@"You are best customer all year! ... Only customer all year.", 6000);

		internal static readonly AudioClip DontBreatheIn = Create("DontBreatheIn", EscapeFromMars.Speaker.Miki,
			"Best not to ... breathe in fumes when we melt this.", 4000);

		internal static readonly AudioClip GreetingsMartianColonists = Create("GreetingsMartianColonists",
		EscapeFromMars.Speaker.Miki,
			@"Greetings Martian Colonist! Miki Scrap is now open for all recycling needs.
You have old junk, scrap metal? We give new, better things in return.
Just follow antenna signal!", 16000);

		internal static readonly AudioClip LavaLoop = Create("LavaLoop", EscapeFromMars.Speaker.None,
			"[Bubbling furnace sounds]", 75000);

		internal static readonly AudioClip NewMikiScrapsOpen = Create("NewMikiScrapsOpen", EscapeFromMars.Speaker.Miki,
			"New Miki Scraps open all the time! Look for us on other planets ... or we come look for you.", 8000);

		internal static readonly AudioClip PartOfBuilding = Create("PartOfBuilding", EscapeFromMars.Speaker.Miki,
			"What is that, part of building!? Get it out of here!", 4000);

		internal static readonly AudioClip TellAllFriends = Create("TellAllFriends", EscapeFromMars.Speaker.Miki,
			"Remember, tell all colonist friends about Miki Scrap!", 4000);

		internal static readonly AudioClip ThisIsGoodScrap = Create("ThisIsGoodScrap", EscapeFromMars.Speaker.Miki,
			"This is good scrap! We melt down for you.", 4000);

		internal static readonly AudioClip TiredOfGrindingCrap = Create("TiredOfGrindingCrap", EscapeFromMars.Speaker
		.Miki,
			"Tired of grinding crap? We can do that! Miki Scrap.", 4000);

		internal static readonly AudioClip WeCrushDown = Create("WeCrushDown", EscapeFromMars.Speaker.Miki,
			"We crush down to little cubes for you!", 3000);

		internal static readonly AudioClip WelcomeMikiScrap = Create("WelcomeMikiScrap", EscapeFromMars.Speaker.Miki,
			"Welcome to Miki Scrap!", 2000);

		internal static readonly AudioClip WhereDoYouGetScrapMetal = Create("WhereDoYouGetScrapMetal",
		EscapeFromMars.Speaker.Miki,
			"Where do you get all this scrap metal!? I've never seen so much!", 5000);

		internal static readonly AudioClip WhereIsThatFrom = Create("WhereIsThatFrom", EscapeFromMars.Speaker.Miki,
			"Where is that from? Nevermind! We make disappear for you.", 6000);

	    private static AudioClip Create(string subTypeName, Speaker speaker, string subtitle, int disappearTimeMs = 4200)
		{
			var id = _nextId++;
			var clip = new AudioClip(id, subTypeName, speaker, subtitle, disappearTimeMs);
			Index.Add(id, clip);
			return clip;
		}

		public static AudioClip GetClipFromId(int id)
		{
			return Index[id];
		}

		public string Filename { get; }
		public string Subtitle { get; }
		public string Speaker { get; }
		public MyFontEnum Font { get; }
		public int DisappearTimeMs { get; }
		public int Id { get; }

		private AudioClip(int id, string filename, Speaker speaker, string subtitle, int disappearTimeMs)
		{
			Id = id;
			DisappearTimeMs = disappearTimeMs;
			Speaker = speaker.Name;
			Font = speaker.Font;
			Filename = filename;
			Subtitle = subtitle;
		}
	}
}

/*
 * 06/01/2019
WicorelYesterday at 10:07 PM
Is there a notification when player takes damage?  And one for  player dyeing?  Respawning?

ThraxusYesterday at 10:16 PM
no direct ones that i see.  can likely just make a MyObjectBuilder_Player or MyObjectBuilder_Identity game logic comp and monitor state / health from there though.
or,IMyCharacter has a CharacterDied event


DigiToday at 5:23 AM
for damage there's the global damage system MyAPIGateway.Session.DamageSystem, register from there, then in the callback first thing you wanna do is check if it's IMyCharacter

    */