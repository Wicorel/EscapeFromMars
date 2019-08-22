using System;
using System.Collections.Generic;
using EscapeFromMars;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Duckroll
{
	public abstract class AbstractCore<T> : MySessionComponentBase
	{
		// Wait because isWorking() on blocks doesn't seem to function early on and is used to check on entities
		private const int UpdatesUntilInitAttempted = 10;

		private const int MaxUpdatesCount = 1200;
		public static readonly bool DebugSaveData = false;

		private bool init;
		private int updateCount;
		private MachineType machineType;
        private readonly UpdateableListImpl modProxy = new UpdateableListImpl();

        public override void UpdateBeforeSimulation()
		{
			try
			{
				updateCount = ++updateCount % MaxUpdatesCount;

				if (!init && updateCount > UpdatesUntilInitAttempted)
				{
					ModLog.Init();
//                    (MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Utilities.IsDedicated)

                    machineType = MyAPIGateway.Multiplayer.IsServer ? MachineType.Host : MachineType.Client;
                    MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                    InitCommon(modProxy);
					if (machineType == MachineType.Host)
					{
						InitHostPreLoading();
						LoadSaveData();
						InitHostPostLoading(modProxy);
					}
					else
					{
						InitClient(modProxy);
					}
					modProxy.InitModSystems();
					init = true;
				}

				modProxy.Update1();

                if (updateCount % UpdatesUntilInitAttempted == 0)
                {
                    dequeueAddedEntities();
                }
                if (updateCount % 30 == 0)
				{
					modProxy.Update30();
				}

				if (updateCount % 60 == 0)
				{
					modProxy.Update60(); // There are 60 updates per second with simspeed at normal
				}

				if (updateCount % 300 == 0)
				{
					modProxy.Update300();
				}

				if (updateCount % 1200 == 0)
				{
					modProxy.Update1200();
				}
			}
			catch (Exception e)
			{
				ModLog.Error(e);
			}
		}

		protected virtual void InitHostPreLoading()
		{
		}

		protected virtual void InitCommon(IModSystemRegistry proxy)
		{
		}

		protected virtual void InitHostPostLoading(IModSystemRegistry modSystemRegistry)
		{
		}

		protected virtual void InitClient(IModSystemRegistry clientUpdateables)
		{
		}

		protected override void UnloadData()
		{
			try
			{
                Close();
				init = false;
				modProxy.Shutdown();
			}
			catch (Exception ex)
			{
				ModLog.Error(ex);
			}
			ModLog.Close();
		}


		public override void SaveData()
		{
			try
			{
				T saveData = GetSaveData();
				MyAPIGateway.Utilities.SetVariable(GetSaveDataFileName(), MyAPIGateway.Utilities.SerializeToXML(saveData));
				if (DebugSaveData)
				{
					MyAPIGateway.Parallel.Start(() =>
					{
						using (var sw = MyAPIGateway.Utilities.WriteFileInWorldStorage("SaveDataSnapshot.xml",
							typeof(EfmCore))) sw.Write(MyAPIGateway.Utilities.SerializeToXML(saveData));
					});
				}
			}
			catch (Exception e)
			{
				ModLog.Error(e);
			}
		}

		public abstract string GetSaveDataFileName();

		public abstract T GetSaveData();

		private void LoadSaveData()
		{
			try
			{
				string xmlData;
				if (MyAPIGateway.Utilities.GetVariable(GetSaveDataFileName(), out xmlData)) // New saving system
				{
					LoadPreviousGame(MyAPIGateway.Utilities.SerializeFromXML<T>(xmlData));
				}
				else
				{
					StartedNewGame();
				}
			}
			catch (Exception e)
			{
				ModLog.Error(e);
			}
		}

		public abstract void LoadPreviousGame(T saveData);

        public abstract void StartedNewGame();
        public virtual void Close()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
        }

        public virtual void GridInit(IMyCubeGrid grid)
        {
            // Called when a grid is created when in the game
//            ModLog.Info("AC:Base: GridInit:" + grid.CustomName);
            modProxy.GridInit(grid);
        }
        List<IMyEntity> queueAddedEntities = new List<IMyEntity>();

        private void dequeueAddedEntities()
        {
            foreach (var entity in queueAddedEntities)
            {
                if (entity is IMyCubeGrid)
                {
                    var grid = entity as IMyCubeGrid;
                    //                ModLog.Info("Added Grid:" + grid.CustomName);
                    if (grid.Physics != null)
                    {
                        //                    ModLog.Info(" Has Physics");
                        GridInit(grid);
                    }
                    else
                    {
                        // it's a projection or a paste in progress
                        //                    ModLog.Info(" NO Physics!");
                    }
                }
                else if (entity is IMyPlayer)
                {
                    IMyPlayer player = (IMyPlayer)entity;
                    ModLog.Info("Added Player:" + player.DisplayName);

                }
            }
            queueAddedEntities.Clear();
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            // enqueue it..
            queueAddedEntities.Add(entity);
        }

        //        internal
        class UpdateableListImpl : IModSystemRegistry
		{
			private readonly List<ModSystemRapidUpdatable> rapidUpdatables = new List<ModSystemRapidUpdatable>();
			private readonly List<ModSystemUpdatable> updatables = new List<ModSystemUpdatable>();
			private readonly List<ModSystemCloseable> closeables = new List<ModSystemCloseable>();

			public void AddCloseableModSystem(ModSystemCloseable modSystemCloseable)
			{
				closeables.Add(modSystemCloseable);
			}

			public void AddUpatableModSystem(ModSystemUpdatable modSystemUpdatable)
			{
				closeables.Add(modSystemUpdatable);
				updatables.Add(modSystemUpdatable);
			}

			public void AddRapidUpdatableModSystem(ModSystemRapidUpdatable modSystemRapidUpdatable)
			{
				closeables.Add(modSystemRapidUpdatable);
				updatables.Add(modSystemRapidUpdatable);
				rapidUpdatables.Add(modSystemRapidUpdatable);
			}
            public virtual void GridInit(IMyCubeGrid grid)
            {
                // Called on ALL grid inits
 //               ModLog.Info("UIL: Base: GridInit:" + grid.CustomName);
                foreach (var modSystemUpdatable in updatables)
                {
                    modSystemUpdatable.GridInitialising(grid);
                }
            }

            public void InitModSystems()
			{
				var ents = new HashSet<IMyEntity>();
				MyAPIGateway.Entities.GetEntities(ents, e => e is IMyCubeGrid);
				foreach (var myEntity in ents)
				{
					var grid = (IMyCubeGrid) myEntity;
                    GridInit(grid);
                    /*
					foreach (var modSystemUpdatable in updatables)
					{
						modSystemUpdatable.GridInitialising(grid);
					}
                    */
				}

				foreach (var modSystemUpdatable in updatables)
				{
					modSystemUpdatable.AllGridsInitialised();
				}
			}

			public void Update1()
			{
				foreach (var updateable in rapidUpdatables)
				{
					updateable.Update1();
				}
			}

			public void Update30()
			{
				foreach (var updateable in updatables)
				{
					updateable.Update30();
				}
			}

			public void Update60()
			{
				foreach (var updateable in updatables)
				{
					updateable.Update60();
				}
			}

			public void Update300()
			{
				foreach (var updateable in updatables)
				{
					updateable.Update300();
				}
			}

			public void Update1200()
			{
				foreach (var updateable in updatables)
				{
					updateable.Update1200();
				}
			}

			public void Shutdown()
			{
				foreach (var closeable in closeables)
				{
					closeable.Close();
				}
			}
		}
	}
}