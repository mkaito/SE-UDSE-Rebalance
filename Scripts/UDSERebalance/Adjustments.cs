using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
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

            // Player character adjustments
            // Skip if save name contains "Creative"
            if (!Session.Name.Contains("Creative") 
                && !Session.Name.Contains("DEV"))
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
                bool largeGrid = myCubeBlockDefinition.CubeSize == MyCubeSize.Large;

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

                    var mult = largeGrid ? largeWheelMult : smallWheelMult;
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

                    def.MaximumRange *= largeGrid ? 8.0f : 5.0f;
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

                    def.SensorRadius *= largeGrid ? 2.0f : 1.25f;
                }

                // Ship Drill
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                {
                    var def = myCubeBlockDefinition as MyShipDrillDefinition;
                    if (def == null)
                        continue;

                    def.CutOutRadius *= largeGrid ? 2.0f : 1.3f;
                }

                // Thrusters
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                {
                    var def = myCubeBlockDefinition as MyThrustDefinition;
                    if (def == null)
                        continue;

                    var subtype = def.Id.SubtypeId.String;

                    // MES NPC-only thrusters
                    if (subtype.StartsWith("MES-NPC-"))
                        continue;

                    // Rider's Heli-carrier Thrusters
                    if (subtype.Contains("Heli"))
                        continue;

                    //
                    // Hard mode. "Expanse mode". Requires redesign of all ships.
                    //   Theory: H2 thrusters act like "afterburners", or main engines, that provide huge thrust, but consume a lot of fuel.
                    //   Ion and Atmo act as maneuvering thrusters, providing little thrust, but consuming virtually no energy.
                    // TODO: Significantly improve force magnitude of large variants
                    // TODO: Alternatively, use the Aryx-Lynxon Drive mod for the true Expanse feel, and nerf all of these.
                    //
                    // float mult;
                    // switch (def.ThrusterType.String)
                    // {
                    //     case "Hydrogen":
                    //         mult = 1.2f;
                    //         def.ForceMagnitude *= largeGrid ? (float)Math.Pow(mult, 2) : mult;
                    //         def.FuelConverter.Efficiency *= 1f / (mult * (largeGrid ? 1.2f : 1.6f));
                    //         break;
                    //
                    //     case "Ion":
                    //         mult = 0.9f;
                    //         def.ForceMagnitude *= largeGrid ? mult : 0.8f * mult;
                    //         def.MaxPowerConsumption *= mult * (largeGrid ? 1.2f : 1.6f);
                    //         break;
                    //
                    //     case "Atmospheric":
                    //         mult = 0.8f;
                    //         def.ForceMagnitude *= largeGrid ? mult : 0.8f * mult;
                    //         def.MaxPowerConsumption *= mult * (largeGrid ? 1.2f : 1.6f);
                    //         break;
                    // }
                    
                    //
                    // Easy mode. Favour large grid.
                    //
                    switch (def.ThrusterType.String)
                    {
                        case "Hydrogen":
                            // Increase H2 thruster fuel efficiency
                            def.FuelConverter.Efficiency = 1;
                            def.MaxPowerConsumption /= largeGrid ? 3f : 2f;
                            break;

                        case "Ion":
                            // Increase Ion thruster force magnitude
                            def.ForceMagnitude *= largeGrid ? 1.6f : 1.4f;
                            break;

                        case "Atmospheric":
                            // Increase Atmospheric thruster force magnitude
                            def.ForceMagnitude *= largeGrid ? 1.4f : 1.2f;
                            break;
                    }
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

                // Double all power generation. Shields and lazors be cray cray.
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_WindTurbine))
                {
                    var def = myCubeBlockDefinition as MyWindTurbineDefinition;
                    if (def == null)
                        continue;

                    def.MaxPowerOutput *= 2;
                }
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_SolarPanel))
                {
                    var def = myCubeBlockDefinition as MySolarPanelDefinition;
                    if (def == null)
                        continue;

                    def.MaxPowerOutput *= 2;
                }
                
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Reactor))
                {
                    var def = myCubeBlockDefinition as MyReactorDefinition;
                    if (def == null)
                        continue;

                    def.MaxPowerOutput *= 2;
                }
                
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_HydrogenEngine))
                {
                    var def = myCubeBlockDefinition as MyHydrogenEngineDefinition;
                    if (def == null)
                        continue;

                    def.MaxPowerOutput *= 3;
                    def.FuelCapacity *= 3;
                }
            }
        }
    }
}
