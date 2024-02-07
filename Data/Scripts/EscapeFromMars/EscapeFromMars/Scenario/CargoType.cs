using Sandbox.Common.ObjectBuilders.Definitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace EscapeFromMars
{
	internal class CargoType
	{
		public static bool Debug = true;

		private static readonly List<CargoType> AllTypes = new List<CargoType>();
		private static int _totalProbability;
		private static readonly IObjectBuilderFactory ComponentType = new ComponentObjectBuilder();
		private static readonly IObjectBuilderFactory AmmoType = new AmmoObjectBuilder();
		private static readonly IObjectBuilderFactory IngotType = new IngotObjectBuilder();

		

        /* SE 1.192 */
        private static readonly IObjectBuilderFactory DatapadType = new DatapadObjectBuilder();
        private static readonly IObjectBuilderFactory ConsumableType = new ConsumableObjectBuilder();
        private static readonly IObjectBuilderFactory CreditType = new CreditObjectBuilder();
        private static readonly IObjectBuilderFactory PackageType = new PackageObjectBuilder();

        //V44 Add Ice
        private static readonly IObjectBuilderFactory OreType = new OreObjectBuilder();

        /* */
        private static void AddComponent(string subtypeName, int amountPerCargoContainer, int probability)
		{
			Add(subtypeName, amountPerCargoContainer, probability, ComponentType);
		}

		private static void AddAmmo(string subtypeName, int amountPerCargoContainer, int probability)
		{
			Add(subtypeName, amountPerCargoContainer, probability, AmmoType);
		}

        private static void AddIngot(string subtypeName, int amountPerCargoContainer, int probability)
        {
            Add(subtypeName, amountPerCargoContainer, probability, IngotType);
        }
        /* SE 1.192 */
        private static void AddDatapad(string subtypeName, int amountPerCargoContainer, int probability)
        {
            Add(subtypeName, amountPerCargoContainer, probability, DatapadType);
        }
        private static void AddConsumable(string subtypeName, int amountPerCargoContainer, int probability)
        {
            Add(subtypeName, amountPerCargoContainer, probability, ConsumableType);
        }
        private static void AddPackage(string subtypeName, int amountPerCargoContainer, int probability)
        {
            Add(subtypeName, amountPerCargoContainer, probability, PackageType);
        }
        private static void AddOre(string subtypeName, int amountPerCargoContainer, int probability)
        {
            Add(subtypeName, amountPerCargoContainer, probability, PackageType);
        }
        /* */
        private static void Add(string subtypeName, int amountPerCargoContainer, int probability,
			IObjectBuilderFactory objectBuilderFactory)
		{
			var startRange = _totalProbability;
			_totalProbability += probability;
			AllTypes.Add(new CargoType(subtypeName, amountPerCargoContainer, startRange, _totalProbability, objectBuilderFactory));
		}

		static CargoType()
		{
            // these should be base/faction specific

            // Also would be nice to have changing list as game goes on.. and maybe based on difficulty and heatlevel
			AddComponent("SteelPlate", 250, 2);
			AddComponent("MetalGrid", 200, 4);
			AddComponent("Construction", 150, 5);
			AddComponent("InteriorPlate", 100, 3);
			AddComponent("Girder", 1500, 2);
			AddComponent("SmallTube", 150, 4);
			AddComponent("LargeTube", 125, 4);
			AddComponent("Motor", 125, 2);
			AddComponent("Display", 100, 2);
			AddComponent("BulletproofGlass", 125, 3);
			AddComponent("Computer", 150, 3);
			AddComponent("Medical", 50, 2);
			AddComponent("RadioCommunication", 50, 2);
			AddComponent("Explosives", 30, 2);
			AddComponent("SolarCell", 100, 4);
			AddComponent("PowerCell", 75, 4);
			AddAmmo("NATO_5p56x45mm", 100, 2);
			AddAmmo("NATO_25x184mm", 50, 5);

			// Remove Uranium V44
			if (EfmCore.OldWorld)
			{
                AddAmmo("Missile200mm", 30, 4);
                AddIngot("Uranium", 15, 3);
				AddComponent("Reactor", 100, 2);
			}
			AddOre("Ice", 500, 3);
        }

        internal static void AllowEconomyItems()
        {
            // SE 1.192
            AddConsumable("Medkit", 200, 2);
            AddConsumable("Powerkit", 200, 2);
        }
        internal static void AllowWarefare1Items()
        {
            // SE 1.198
            AddAmmo("AutomaticRifleGun_Mag_20rd", 100, 3);
            AddAmmo("RapidFireAutomaticRifleGun_Mag_50rd", 100, 3);
            AddAmmo("PreciseAutomaticRifleGun_Mag_5rd", 100, 3);
            AddAmmo("UltimateAutomaticRifleGun_Mag_30rd", 100, 3);
        }
        internal static void AllowWarefare2Items()
        {
            // SE 1.200

            // ammo mags
            AddAmmo("AutocannonClip", 100, 3);
            AddAmmo("NATO_25x184mm", 150, 3);
//            AddAmmo("LargeCalibreAmmo", 100, 3); // Contains U
//            AddAmmo("MediumCalibreAmmo", 150, 3);
//            AddAmmo("LargeRailgunAmmo", 25, 3);
//            AddAmmo("SmallRailgunAmmo", 50, 3);

        }
		private static bool bDumpedList=false;

        internal static CargoType GenerateRandomCargo(Random random)
		{
			if(Debug && !bDumpedList)
			{
                foreach (var cargo in AllTypes)
                {
					MyLog.Default.Info(cargo.GetDisplayName() + " " +cargo.subtypeName+" "+cargo.AmountPerCargoContainer);
                }

                bDumpedList = true;
			}
			var randomNumber = random.Next(_totalProbability);
			foreach (var cargo in AllTypes)
			{
				if (cargo.IsThisYourNumber(randomNumber))
				{
					return cargo;
				}
			}
			throw new InvalidOperationException("No random type for number: " + randomNumber); // This should never happen!
		}

		internal readonly int AmountPerCargoContainer;
//		private readonly string subtypeName;
        public readonly string subtypeName;
        private readonly int probabilityRangeStart, probabilityRangeEnd; // Start (inclusive) to end (exclusive)
		private readonly IObjectBuilderFactory objectBuilderFactory;

		CargoType(string subtypeName, int amountPerCargoContainer, int probabilityRangeStart, int probabilityRangeEnd,
			IObjectBuilderFactory objectBuilderFactory)
		{
			this.subtypeName = subtypeName;
			this.probabilityRangeStart = probabilityRangeStart;
			this.probabilityRangeEnd = probabilityRangeEnd;
			this.objectBuilderFactory = objectBuilderFactory;
			AmountPerCargoContainer = amountPerCargoContainer;
		}

		private bool IsThisYourNumber(int number)
		{
			if (number < probabilityRangeStart)
			{
				return false;
			}
			return number < probabilityRangeEnd;
		}

		internal string GetDisplayName()
		{
			// This is kind of shitty but can't find the real ones
			if (subtypeName.Equals("SmallTube"))
			{
				return MyTexts.GetString("DisplayName_Item_SmallSteelTube");
			}
			if (subtypeName.Equals("LargeTube"))
			{
				return MyTexts.GetString("DisplayName_Item_LargeSteelTube");
			}
			if (subtypeName.Equals("Construction"))
			{
				return MyTexts.GetString("DisplayName_Item_ConstructionComponent");
			}
			if (subtypeName.Equals("RadioCommunication"))
			{
				return MyTexts.GetString("DisplayName_Item_RadioCommunicationComponents");
			}
			if (subtypeName.Equals("Uranium"))
			{
				return MyTexts.GetString("DisplayName_Item_UraniumIngot");
			}
			if (subtypeName.Equals("Reactor"))
			{
				return MyTexts.GetString("DisplayName_Item_ReactorComponents");
			}
            if (subtypeName.Equals("Medical"))
            {
                return MyTexts.GetString("DisplayName_Item_MedicalComponents");
            }
            if (subtypeName.Equals("AutocannonClip"))
            {
                return MyTexts.GetString("DisplayName_Item_AutocannonClipComponents");
            }

            return MyTexts.GetString("DisplayName_Item_" + subtypeName);
		}

		internal MyObjectBuilder_Base GetObjectBuilder()
		{
			return objectBuilderFactory.GetObjectBuilder(subtypeName);
		}

		private interface IObjectBuilderFactory
		{
			MyObjectBuilder_PhysicalObject GetObjectBuilder(String subtypeName);
		}

		private class ComponentObjectBuilder : IObjectBuilderFactory
		{
			public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
			{
				return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(subtypeName);
			}
		}

		private class AmmoObjectBuilder : IObjectBuilderFactory
		{
			public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
			{
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_AmmoMagazine>(subtypeName);
            }
        }

		private class IngotObjectBuilder : IObjectBuilderFactory
		{
			public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
			{
				return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ingot>(subtypeName);
			}
		}
        /* SE V1.192 */
        private class DatapadObjectBuilder : IObjectBuilderFactory
        {
            // DataPad/Datapad
            public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
            {
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Datapad>(subtypeName);
            }
        }
        private class ConsumableObjectBuilder : IObjectBuilderFactory
        {
            // ConsumableItem/Medkit
            // ConsumableItem/Powerkit
            public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
            {
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>(subtypeName);
            }
        }
        private class CreditObjectBuilder : IObjectBuilderFactory
        {
            // PhysicalObject/SpaceCredit
            public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
            {
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalObject>(subtypeName);
            }
        }
        private class PackageObjectBuilder : IObjectBuilderFactory
        {
            // Package/Package
            public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
            {
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Package>(subtypeName);
            }
        }
        private class OreObjectBuilder : IObjectBuilderFactory
        {
            // Ore
            public MyObjectBuilder_PhysicalObject GetObjectBuilder(string subtypeName)
            {
                return MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(subtypeName);
            }
        }
        /**/
    }
}