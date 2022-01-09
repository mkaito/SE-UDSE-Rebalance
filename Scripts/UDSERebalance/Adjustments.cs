using System;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.EntityComponents;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

// ReSharper disable IdentifierTypo
namespace UDSERebalance
// ReSharper restore IdentifierTypo
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class QoLAdjustments : MySessionComponentBase
    {
        private bool _initDone;
        public override void LoadData()
        {

            if (_initDone)
                return;
            _initDone = true;

            // Camera textures courtesy of Enenra of AQD fame
            const string camTexturePath = @"\Textures\GUI\Screens\camera_overlay.dds";
            const string turretTexturePath = @"\Textures\GUI\Screens\turret_overlay.dds";
            var camTextureFullPath = ModContext.ModPath + camTexturePath;
            var turretTextureFullPath = ModContext.ModPath + turretTexturePath;

            const float smallWheelMult = 8;
            const float largeWheelMult = 20;

            // Hydrogen fuel value
            MyGasProperties hydrogenDef;
            if (MyDefinitionManager.Static.TryGetDefinition(MyResourceDistributorComponent.HydrogenId, out hydrogenDef))
                hydrogenDef.EnergyDensity = 0.004668f;

            // Player character adjustments
            // Skip if save name contains "Creative"
            if (!Session.Name.Contains("Creative"))
            {
                foreach (var myCharacterDefinition in MyDefinitionManager.Static.Characters.Where(def => def.UsableByPlayer))
                {
                    // Nerf the jetpack in gravity
                    myCharacterDefinition.Jetpack.ThrustProperties.ForceMagnitude = 1800;
                    myCharacterDefinition.Jetpack.ThrustProperties.SlowdownFactor = 1;
                    myCharacterDefinition.Jetpack.ThrustProperties.MinPowerConsumption = 0.0000021666666666666666666666666667f;
                    myCharacterDefinition.Jetpack.ThrustProperties.MaxPowerConsumption = 0.00065f;
                    myCharacterDefinition.Jetpack.ThrustProperties.ConsumptionFactorPerG = 135;
                    myCharacterDefinition.Jetpack.ThrustProperties.EffectivenessAtMinInfluence = 1;
                    myCharacterDefinition.Jetpack.ThrustProperties.EffectivenessAtMaxInfluence = 0;

                    // Increase oxygen consumption significantly
                    myCharacterDefinition.OxygenConsumptionMultiplier = 8f;
                }
            }

            // Block adjustments
            foreach (var myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions()
                        .Select(myDefinitionBase => myDefinitionBase as MyCubeBlockDefinition)
                        .Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
            {
                bool largeVariant = myCubeBlockDefinition.CubeSize == MyCubeSize.Large;

                // Camera Overlay
                if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_CameraBlock))
                {
                    // This ensures that LyleCorp's / Novar's docking cameras with their specific overlay don't get replaced
                    if (myCubeBlockDefinition.Id.SubtypeId.String.Contains("DockingCamera"))
                        continue;

                    var def = myCubeBlockDefinition as MyCameraBlockDefinition;
                    if (def == null)
                        continue;

                    def.OverlayTexture = camTextureFullPath;
                }

                if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OxygenFarm))
                {
                    var def = myCubeBlockDefinition as MyOxygenFarmDefinition;
                    if (def == null)
                        continue;

                    def.MaxGasOutput *= 10;
                }

                // Turret Overlay
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorTurret)
                        || myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeGatlingTurret)
                        || myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeMissileTurret))
                {
                    var def = myCubeBlockDefinition as MyLargeTurretBaseDefinition;
                    if (def == null)
                        continue;

                    def.OverlayTexture = turretTextureFullPath;
                }

                // Wheel Suspension
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_MotorSuspension))
                {
                    var def = myCubeBlockDefinition as MyMotorSuspensionDefinition;
                    if (def == null)
                        continue;

                    var mult = largeVariant ? largeWheelMult : smallWheelMult;
                    def.AxleFriction *= mult;
                    def.PropulsionForce *= mult;
                    def.RequiredPowerInput *= (mult / 8);
                }

                // Ore Detector
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OreDetector))
                {
                    var def = myCubeBlockDefinition as MyOreDetectorDefinition;
                    if (def == null)
                        continue;

                    def.MaximumRange *= largeVariant ? 5.0f : 10.0f;
                }

                // Laser Antenna
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LaserAntenna))
                {
                    var def = myCubeBlockDefinition as MyLaserAntennaDefinition;
                    if (def == null)
                        continue;

                    def.PowerInputLasing /= 10;
                }

                // Ship Welder
                // Note: Tiered Tech Blocks already buffs the block welder
                //else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipWelder))
                //{
                //    var def = myCubeBlockDefinition as MyShipWelderDefinition;
                //    if (def == null)
                //        continue;

                //    def.SensorRadius *= largeVariant ? 2.0f : 1.1f;
                //}

                // Ship Grinder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipGrinder))
                {
                    var def = myCubeBlockDefinition as MyShipGrinderDefinition;
                    if (def == null)
                        continue;

                    def.SensorRadius *= largeVariant ? 2.0f : 1.25f;
                }

                // Ship Drill
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                {
                    var def = myCubeBlockDefinition as MyShipDrillDefinition;
                    if (def == null)
                        continue;

                    def.CutOutRadius *= largeVariant ? 2.0f : 1.3f;
                }

                // Thrusters
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                {
                    var def = myCubeBlockDefinition as MyThrustDefinition;
                    if (def == null)
                        continue;

                    // MES NPC-only thrusters
                    if (myCubeBlockDefinition.Id.SubtypeId.String.StartsWith("MES-NPC-"))
                        continue;

                    // Rider's Heli-carrier Thrusters
                    if (def.Id.SubtypeId.String.Contains("Heli"))
                        continue;

                    float mult;

                    // Hydrogen is more powerful, but uses more fuel.
                    // Atmospheric and Ion are both less effective, but use less power.
                    // Large variants are significantly more powerful and efficient
                    // than small variants.
                    // FIXME: Can't seem to get the balance right. Maybe it's time for spreadsheets.
                    /*
                    switch (def.ThrusterType.String)
                    {
                        case "Hydrogen":
                            mult = 1.2f;
                            def.ForceMagnitude *= largeVariant ? (float)Math.Pow(mult, 2) : mult;
                            def.FuelConverter.Efficiency *= 1f / (mult * (largeVariant ? 1.2f : 1.6f));
                            break;

                        case "Ion":
                            mult = 0.9f;
                            def.ForceMagnitude *= largeVariant ? mult : 0.8f * mult;
                            def.MaxPowerConsumption *= mult * (largeVariant ? 1.2f : 1.6f);
                            break;

                        case "Atmospheric":
                            mult = 0.8f;
                            def.ForceMagnitude *= largeVariant ? mult : 0.8f * mult;
                            def.MaxPowerConsumption *= mult * (largeVariant ? 1.2f : 1.6f);
                            break;
                    }
                */
                }

                // Spotlights
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ReflectorLight))
                {
                    var def = myCubeBlockDefinition as MyReflectorBlockDefinition;
                    if (def == null)
                        continue;

                    def.LightReflectorRadius = new MyBounds(10, 780, 320);
                }

                // Interior lights, corner lights, etc
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorLight))
                {
                    var def = myCubeBlockDefinition as MyLightingBlockDefinition;
                    if (def == null)
                        continue;

                    def.LightRadius = new MyBounds(1, 40, 3.6f);
                }
            }
        }
    }
}
