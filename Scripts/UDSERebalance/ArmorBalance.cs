using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

// This Code based on Enenra's AQD Quality of Life Core code, which in turn is
// based on Gauge's Balanced Deformation code.
namespace UDSERebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ArmorBalance : MySessionComponentBase
    {
        private readonly List<IRemember> OriginalValues = new List<IRemember>();

        private const float LightArmorLargeDamageMod = 0.5f;
        private const float LightArmorLargeDeformationMod = 0.2f;
        private const float LightArmorSmallDamageMod = 0.5f;
        private const float LightArmorSmallDeformationMod = 0.2f;

        private const float HeavyArmorLargeDamageMod = 0.5f;
        private const float HeavyArmorLargeDeformationMod = 0.15f;
        private const float HeavyArmorSmallDamageMod = 0.5f;
        private const float HeavyArmorSmallDeformationMod = 0.15f;

        private void DoWork()
        {
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null) continue;

                if (blockDef.BlockTopology == MyBlockTopology.TriangleMesh &&
                    !(blockDef.Id.SubtypeName.StartsWith("AQD_LG_LA_") ||
                      blockDef.Id.SubtypeName.StartsWith("AQD_SG_LA_") ||
                      blockDef.Id.SubtypeName.StartsWith("AQD_LG_HA_") ||
                      blockDef.Id.SubtypeName.StartsWith("AQD_SG_HA_"))) continue;

                switch (blockDef.EdgeType)
                {
                    case "Light":
                    {
                        switch (blockDef.CubeSize)
                        {
                            case MyCubeSize.Large:
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.GeneralDamageMultiplier,
                                    (d, v) => blockDef.GeneralDamageMultiplier = v,
                                    LightArmorLargeDamageMod));
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.DeformationRatio,
                                    (d, v) => blockDef.DeformationRatio = v,
                                    LightArmorLargeDeformationMod));
                                break;
                            case MyCubeSize.Small:
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.GeneralDamageMultiplier,
                                    (d, v) => blockDef.GeneralDamageMultiplier = v,
                                    LightArmorSmallDamageMod));
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.DeformationRatio,
                                    (d, v) => blockDef.DeformationRatio = v,
                                    LightArmorSmallDeformationMod));
                                break;
                        }

                        break;
                    }
                    case "Heavy":
                    {
                        switch (blockDef.CubeSize)
                        {
                            case MyCubeSize.Large:
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.GeneralDamageMultiplier,
                                    (d, v) => blockDef.GeneralDamageMultiplier = v,
                                    HeavyArmorLargeDamageMod));
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.DeformationRatio,
                                    (d, v) => blockDef.DeformationRatio = v,
                                    HeavyArmorLargeDeformationMod));
                                break;
                            case MyCubeSize.Small:
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.GeneralDamageMultiplier,
                                    (d, v) => blockDef.GeneralDamageMultiplier = v,
                                    HeavyArmorSmallDamageMod));
                                OriginalValues.Add(Remember.Create(blockDef, (d) => blockDef.DeformationRatio,
                                    (d, v) => blockDef.DeformationRatio = v,
                                    HeavyArmorSmallDeformationMod));
                                break;
                        }

                        break;
                    }
                }
            }
        }

        public override void LoadData()
        {
            DoWork();
        }
        
        protected override void UnloadData()
        {
            foreach (var r in OriginalValues)
            {
                r.Restore();
            }
        }
    }
}