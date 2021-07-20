using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace mkaito.QoL
{
    [MySessionComponentDescriptor((MyUpdateOrder.NoUpdate))]
    public class _QoLAdjustments : MySessionComponentBase
    {
        public override void BeforeStart()
        {
            base.BeforeStart();
            SetUpdateOrder(MyUpdateOrder.AfterSimulation);
        }

        public override void LoadData()
        {
            // Camera textures courtesy of enenra of AQD fame
            const string camTexturePath = @"\Textures\GUI\Screens\camera_overlay.dds";
            const string turretTexturePath = @"\Textures\GUI\Screens\turret_overlay.dds";
            var camTextureFullPath = ModContext.ModPath + camTexturePath;
            var turretTextureFullPath = ModContext.ModPath + turretTexturePath;
            
            const float smallWheelMult = 6;
            const float largeWheelMult = 18;

            foreach (var myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions()
                        .Select(myDefinitionBase => myDefinitionBase as MyCubeBlockDefinition)
                        .Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null)
            )
            {
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

                    def.MaxGasOutput *= 4f;
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

                    var mult = def.CubeSize == MyCubeSize.Large ? largeWheelMult : smallWheelMult;
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

                    def.MaximumRange *= 3;
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
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipWelder))
                {
                    var def = myCubeBlockDefinition as MyShipWelderDefinition;
                    if (def == null)
                        continue;

                    def.SensorRadius *= 1.2f;
                }
                
                // Ship Grinder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipGrinder))
                {
                    var def = myCubeBlockDefinition as MyShipGrinderDefinition;
                    if (def == null)
                        continue;

                    def.SensorRadius *= 1.25f;
                }
                
                // Ship Drill
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                {
                    var def = myCubeBlockDefinition as MyShipDrillDefinition;
                    if (def == null)
                        continue;

                    def.CutOutRadius *= 1.4f;
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
                
                    // Rider's Helicarrier Thrusters
                    if (def.Id.SubtypeId.String.Contains("Heli"))
                        continue;

                    float mult;

                    switch (def.ThrusterType.String)
                    {
                        case "Hydrogen":
                            mult = 2f;
                            def.ForceMagnitude *= mult;
                            def.FuelConverter.Efficiency *= 1f / (mult * 2);
                            break;
                        case "Ion":
                            mult = 1.2f;
                            def.ForceMagnitude *= mult;
                            def.MaxPowerConsumption *= mult * 2;
                            break;
                        case "Atmospheric":
                            mult = 1.2f;
                            def.ForceMagnitude *= mult;
                            def.MaxPowerConsumption *= mult * 2;
                            break;
                    }
                }
                
                // Hydrogen Power
                // else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_HydrogenEngine))
                // {
                //     var def = myCubeBlockDefinition as MyHydrogenEngineDefinition;
                //     if (def == null)
                //         continue;

                //     def.FuelCapacity *= 3f;
                //     def.MaxPowerOutput *= 3f;
                // }
            }
        }
    }
}