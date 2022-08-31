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
        private readonly List<IRemember> OriginalValues = new List<IRemember>();

        private void DoWork()
        {
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var blockDef = def as MyCubeBlockDefinition;
                if (blockDef != null)
                    AdjustArmorBlocks(blockDef);

                var myCockpitDef = def as MyCockpitDefinition;
                if (myCockpitDef != null)
                    OriginalValues.Add(Remember.Create(myCockpitDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.5f));

                var myThrusterDef = def as MyThrustDefinition;
                if (myThrusterDef != null)
                    OriginalValues.Add(Remember.Create(myThrusterDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.5f));

                var myWheelDef = def as MyMotorSuspensionDefinition;
                if (myWheelDef != null)
                    OriginalValues.Add(Remember.Create(myWheelDef, d => d.GeneralDamageMultiplier,
                        (d, v) => d.GeneralDamageMultiplier = v, 0.5f));
            }
        }

        private void AdjustArmorBlocks(MyCubeBlockDefinition blockDef)
        {
            const float LightArmorLargeDamageMod = 0.5f;
            const float LightArmorSmallDamageMod = 0.4f;

            const float HeavyArmorLargeDamageMod = 0.4f;
            const float HeavyArmorSmallDamageMod = 0.2f;

            if (blockDef.BlockTopology == MyBlockTopology.TriangleMesh &&
                !(blockDef.Id.SubtypeName.StartsWith("AQD_LG_LA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_SG_LA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_LG_HA_") ||
                  blockDef.Id.SubtypeName.StartsWith("AQD_SG_HA_"))) return;

            switch (blockDef.EdgeType)
            {
                case "Light":
                {
                    switch (blockDef.CubeSize)
                    {
                        case MyCubeSize.Large:
                            OriginalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                LightArmorLargeDamageMod));
                            break;
                        case MyCubeSize.Small:
                            OriginalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                LightArmorSmallDamageMod));
                            break;
                    }

                    break;
                }
                case "Heavy":
                {
                    switch (blockDef.CubeSize)
                    {
                        case MyCubeSize.Large:
                            OriginalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                HeavyArmorLargeDamageMod));
                            break;
                        case MyCubeSize.Small:
                            OriginalValues.Add(Remember.Create(blockDef, d => d.GeneralDamageMultiplier,
                                (d, v) => d.GeneralDamageMultiplier = v,
                                HeavyArmorSmallDamageMod));
                            break;
                    }

                    break;
                }
            }
        }

        public override void LoadData()
        {
            DoWork();
            base.LoadData();
        }

        protected override void UnloadData()
        {
            foreach (var r in OriginalValues)
            {
                r.Restore();
            }
            base.UnloadData();
        }
    }
}