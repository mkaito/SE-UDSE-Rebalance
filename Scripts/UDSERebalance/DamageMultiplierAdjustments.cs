using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

// This Code based on Enenra's AQD Quality of Life Core code, which in turn is
// based on Gauge's Balanced Deformation code.
namespace UDSERebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class DamageMultiplierAdjustments : MySessionComponentBase
    {
        readonly private List<IRemember> _originalValues = new List<IRemember>();

        private void AdjustFunctionalBlocks()
        {
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var blockDef = def as MyCubeBlockDefinition;
                if (blockDef != null)
                {
                    AdjustArmorBlocks(blockDef);
                }

                var myCockpitDef = def as MyCockpitDefinition;
                if (myCockpitDef != null)
                {
                    _originalValues.Add(Remember.Create(myCockpitDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.50f));
                }

                var myThrusterDef = def as MyThrustDefinition;
                if (myThrusterDef != null)
                {
                    _originalValues.Add(Remember.Create(myThrusterDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.75f));
                }

                var myWheelDef = def as MyMotorSuspensionDefinition;
                if (myWheelDef != null)
                {
                    _originalValues.Add(Remember.Create(myWheelDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.25f));
                }
            }
        }

        private void AdjustArmorBlocks(MyCubeBlockDefinition blockDef)
        {
            const float lightArmorLargeDamageMod = 0.85f;
            const float lightArmorSmallDamageMod = 0.70f;

            const float heavyArmorLargeDamageMod = 0.20f;
            const float heavyArmorSmallDamageMod = 0.30f;

            if (blockDef.BlockTopology == MyBlockTopology.TriangleMesh &&
                !(blockDef.Id.SubtypeName.StartsWith("AQD_LG_LA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_SG_LA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_LG_HA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_SG_HA_")))
            {
                return;
            }

            switch (blockDef.EdgeType)
            {
                case "Light":
                {
                    switch (blockDef.CubeSize)
                    {
                        case MyCubeSize.Large:
                            _originalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                lightArmorLargeDamageMod));
                            break;
                        case MyCubeSize.Small:
                            _originalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                lightArmorSmallDamageMod));
                            break;
                        default:
                            return;
                    }

                    break;
                }
                case "Heavy":
                {
                    switch (blockDef.CubeSize)
                    {
                        case MyCubeSize.Large:
                            _originalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                heavyArmorLargeDamageMod));
                            break;
                        case MyCubeSize.Small:
                            _originalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                heavyArmorSmallDamageMod));
                            break;
                        default:
                            return;
                    }

                    break;
                }
            }
        }

        public override void LoadData()
        {
            AdjustFunctionalBlocks();
            base.LoadData();
        }

        protected override void UnloadData()
        {
            foreach (var r in _originalValues)
            {
                r.Restore();
            }
            base.UnloadData();
        }
    }
}