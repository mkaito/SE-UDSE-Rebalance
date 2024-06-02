using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace UDSERebalance
{
    // Blame Keen
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    // ReSharper disable once UnusedType.Global
    public class GridThrustDirectionEnforcer : MySessionComponentBase
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<long, IMyCubeGrid> _grids = new Dictionary<long, IMyCubeGrid>();
        private readonly Dictionary<long, List<MyThrust>> _drives = new Dictionary<long, List<MyThrust>>();

        private static readonly string[] EpsteinSubtypes =
        {
            "ARYLNX_Epstein_Drive",
            "ARYLNX_MUNR_Epstein_Drive",
            "ARYLNX_PNDR_Epstein_Drive",
            "ARYLNX_QUADRA_Epstein_Drive",
            "ARYLNX_RAIDER_Epstein_Drive",
            "ARYLNX_ROCI_Epstein_Drive",
            "ARYLYNX_SILVERSMITH_DRIVE",
            "ARYLNX_DRUMMER_Epstein_Drive",
            "ARYLNX_SCIRCOCCO_Epstein_Drive",
            "ARYLNX_Mega_Epstein_Drive",
            "ARYLNX_RZB_Epstein_Drive",
            "ARYXLNX_YACHT_EPSTEIN_DRIVE",
            "ARYLNX_Epstein_Drive_S",
            "ARYLNX_PNDR_Epstein_Drive_S",
            "ARYLNX_QUADRA_Epstein_Drive_S",
            "ARYLNX_RAIDER_Epstein_Drive_S",
            "ARYLNX_ROCI_Epstein_Drive_S",
            "ARYLYNX_SILVERSMITH_DRIVE_S",
            "ARYLNX_DRUMMER_Epstein_Drive_S",
            "ARYLNX_SCIRCOCCO_Epstein_Drive_S",
            "ARYLNX_Mega_Epstein_Drive_S"
        };

        /// <summary>
        /// Checks if the subtype is a known drive type.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool IsEpsteinDrive(MyDefinitionId d) => EpsteinSubtypes.Contains(d.SubtypeId.ToString());

        /// <summary>
        /// Set up the root handler on session load.
        /// </summary>
        public override void LoadData()
        {
            try
            {
                if (MyAPIGateway.Utilities.IsDedicated)
                {
                    return;
                }

                MyAPIGateway.Entities.OnEntityAdd += EntitySpawned;
            }
            catch (Exception e)
            {
                UnloadData();

                SimpleLog.Error(this, e);
                throw;
            }
        }

        /// <summary>
        /// Pack up, we're going home.
        /// </summary>
        protected override void UnloadData()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
            {
                return;
            }

            _grids.Clear();
            _drives.Clear();
            
            MyAPIGateway.Entities.OnEntityAdd -= EntitySpawned;
        }

        /// <summary>
        /// Entity (grid) spawned. Happens on world load or any other time a grid is spawned by any means.
        /// We register the grid and its event handlers, and iterate it to make sure no unaligned drives exist.
        /// </summary>
        /// <param name="entity"></param>
        private void EntitySpawned(IMyEntity entity)
        {
            var grid = entity as MyCubeGrid;
            if (grid?.Physics == null || grid.IsPreview)
            {
                return;
            }

            _grids.Add(grid.EntityId, grid);
            _drives[grid.EntityId] = new List<MyThrust>();
            
            grid.OnMarkForClose += GridMarkedForClose;
            grid.OnBlockAdded += BlockAdded;
            grid.OnBlockRemoved += BlockRemoved;
            
            EnsureDrivesAligned(grid);
            
            // SimpleLog.Info(this, $"Entity spawned {grid.EntityId}", true);
        }

        /// <summary>
        /// Yeet unaligned Epstein drives. First Epstein drive found on grid iteration used as reference.
        /// </summary>
        /// <param name="grid"></param>
        private void EnsureDrivesAligned(MyCubeGrid grid)
        {
            var blocks = grid.CubeBlocks.Cast<IMySlimBlock>();
            foreach (var block in blocks)
            {
                var defId = block.BlockDefinition.Id;
                var eId = block.CubeGrid.EntityId;
                if (!IsEpsteinDrive(defId))
                {
                    continue;
                }
                
                var fatBlock = block.FatBlock as MyThrust;
                if (fatBlock == null)
                {
                    continue;
                }
                    
                if (_drives[eId].Count == 0 || _drives[eId][0].Orientation == fatBlock.Orientation)
                {
                    _drives[eId].Add(fatBlock);
                }
                else
                {
                    block.CubeGrid.RemoveBlock(block);
                }
            }
        }

        /// <summary>
        /// Called every time a block is removed for any reason. If it was a drive, remove it from the list.
        /// </summary>
        /// <param name="slimBlock"></param>
        private void BlockRemoved(IMySlimBlock slimBlock)
        {
            var fatBlock = slimBlock.FatBlock as MyThrust;
            if (fatBlock == null)
            {
                // SimpleLog.Error(this, $"SlimBlock has no FatBlock definition {slimBlock.BlockDefinition.Id}");
                return;
            }

            var eId = slimBlock.CubeGrid.EntityId;
            _drives[eId].Remove(fatBlock);
        }

        /// <summary>
        /// Called any time a block is placed. If it's a drive, ensure it is aligned and yeet it otherwise.
        /// </summary>
        /// <param name="slimBlock"></param>
        private void BlockAdded(IMySlimBlock slimBlock)
        {
            // SimpleLog.Info(this, "Checking...");
            var defId = slimBlock.BlockDefinition.Id;
            if (!IsEpsteinDrive(defId))
            {
                // SimpleLog.Info(this, "Not a drive");
                return;
            }

            var fatBlock = slimBlock.FatBlock as MyThrust;
            if (fatBlock == null)
            {
                // SimpleLog.Error(this, $"SlimBlock has no FatBlock definition {slimBlock.BlockDefinition.Id}");
                return;
            }
            
            var eId = slimBlock.CubeGrid.EntityId;
            if (_drives[eId].Count == 0)
            {
                // We gucci
                // SimpleLog.Info(this, "First drive placed");
                return;
            } 
            
            if (fatBlock.Orientation == _drives[eId][0].Orientation)
            {
                // We gucci
                _drives[slimBlock.CubeGrid.EntityId].Add(fatBlock);
                // SimpleLog.Info(this, "Drive is aligned");
                return;
            }
            
            // We ball
            // SimpleLog.Info(this, "Heresy detected!");
            slimBlock.CubeGrid.RemoveBlock(slimBlock);
        }

        /// <summary>
        /// Grid going bye bye. Remove any handlers and clean house.
        /// </summary>
        /// <param name="entity"></param>
        private void GridMarkedForClose(IMyEntity entity)
        {
            var grid = entity as IMyCubeGrid;
            if (grid == null)
            {
                return;
            }

            grid.OnMarkForClose -= GridMarkedForClose;
            grid.OnBlockAdded -= BlockAdded;
            grid.OnBlockRemoved -= BlockRemoved;

            _grids.Remove(grid.EntityId);
        }
    }
}