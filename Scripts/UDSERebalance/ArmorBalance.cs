using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

// Code is based on Gauge's Balanced Deformation code, but heavily modified for more control. 
namespace UDSERebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ArmorBalance : MySessionComponentBase
    {
        private const float LightArmorLargeDamageMod = 0.5f;
        private const float LightArmorLargeDeformationMod = 0.2f;
        private const float LightArmorSmallDamageMod = 0.5f;
        private const float LightArmorSmallDeformationMod = 0.2f;

        private const float HeavyArmorLargeDamageMod = 0.5f;
        private const float HeavyArmorLargeDeformationMod = 0.15f;
        private const float HeavyArmorSmallDamageMod = 0.5f;
        private const float HeavyArmorSmallDeformationMod = 0.15f;

        private bool _initDone;

        private void DoWork()
        {
            if (_initDone)
                return;
            _initDone = true;

            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null) continue;

                if (blockDef.BlockTopology == MyBlockTopology.TriangleMesh && !(blockDef.Id.SubtypeName.StartsWith("AQD_LG_LA_") || blockDef.Id.SubtypeName.StartsWith("AQD_SG_LA_") || blockDef.Id.SubtypeName.StartsWith("AQD_LG_HA_") || blockDef.Id.SubtypeName.StartsWith("AQD_SG_HA_"))) continue;

                if (blockDef.EdgeType == "Light")
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = LightArmorLargeDamageMod;
                        blockDef.DeformationRatio = LightArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = LightArmorSmallDamageMod;
                        blockDef.DeformationRatio = LightArmorSmallDeformationMod;
                    }
                }

                if (blockDef.EdgeType == "Heavy")
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = HeavyArmorLargeDamageMod;
                        blockDef.DeformationRatio = HeavyArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = HeavyArmorSmallDamageMod;
                        blockDef.DeformationRatio = HeavyArmorSmallDeformationMod;
                    }
                }
            }
        }
        
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {

        }

        public override void LoadData()
        {
            DoWork();  
        }
    }
}
