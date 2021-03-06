using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using UDSERebalance.ConfigData;
using UDSERebalance.Utilities;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

namespace UDSERebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class QoLAdjustments : MySessionComponentBase
    {
        private readonly List<IRemember> OriginalValues = new List<IRemember>();
        private SaveData ModSaveData;

        private void DoWork()
        {
            const float smallWheelMult = 8;
            const float largeWheelMult = 20;

            // Player character adjustments
            foreach (var myCharacterDefinition in MyDefinitionManager.Static.Characters.Where(def =>
                         def.UsableByPlayer))
            {
                if (ModSaveData.NerfJetpack)
                {
                    var jp = myCharacterDefinition.Jetpack.ThrustProperties;

                    // Nerf the jetpack in gravity
                    OriginalValues.Add(Remember.Create(jp, d => d.ForceMagnitude, (d, v) => d.ForceMagnitude = v,
                        1800));
                    OriginalValues.Add(
                        Remember.Create(jp, d => jp.SlowdownFactor, (d, v) => d.SlowdownFactor = v, 1));
                    OriginalValues.Add(Remember.Create(jp, d => d.MinPowerConsumption,
                        (d, v) => d.MinPowerConsumption = v, 0.0000021666666666666666666666666667f));
                    OriginalValues.Add(Remember.Create(jp, d => d.MaxPowerConsumption,
                        (d, v) => d.MaxPowerConsumption = v, 0.00065f));
                    OriginalValues.Add(Remember.Create(jp, d => d.ConsumptionFactorPerG,
                        (d, v) => d.ConsumptionFactorPerG = v, 135));
                    OriginalValues.Add(Remember.Create(jp, d => d.EffectivenessAtMinInfluence,
                        (d, v) => d.EffectivenessAtMinInfluence = v, 1));
                    OriginalValues.Add(Remember.Create(jp, d => d.EffectivenessAtMaxInfluence,
                        (d, v) => d.EffectivenessAtMaxInfluence = v, 0));
                }

                if (ModSaveData.BoostOxygenConsumption)
                    // Increase oxygen consumption significantly
                    OriginalValues.Add(Remember.Create(myCharacterDefinition,
                        d => d.OxygenConsumptionMultiplier,
                        (d, v) => d.OxygenConsumptionMultiplier = v, 8));
            }

            // Block adjustments
            foreach (var myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions()
                         .Select(myDefinitionBase => myDefinitionBase as MyCubeBlockDefinition)
                         .Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
            {
                var largeGrid = myCubeBlockDefinition.CubeSize == MyCubeSize.Large;
                var subtype = myCubeBlockDefinition.Id.SubtypeId.String;

                // Oxygen Farm
                if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OxygenFarm))
                {
                    var def = myCubeBlockDefinition as MyOxygenFarmDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.MaxGasOutput, (d, v) => d.MaxGasOutput = v,
                        def.MaxGasOutput * 10));
                }

                // Wheel Suspension
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_MotorSuspension))
                {
                    var def = myCubeBlockDefinition as MyMotorSuspensionDefinition;
                    if (def == null)
                        continue;

                    var mult = largeGrid ? largeWheelMult : smallWheelMult;

                    OriginalValues.Add(Remember.Create(def, d => d.AxleFriction, (d, v) => d.AxleFriction = v,
                        def.AxleFriction * mult));
                    OriginalValues.Add(Remember.Create(def, d => d.PropulsionForce,
                        (d, v) => d.PropulsionForce = v,
                        def.PropulsionForce * mult));
                    OriginalValues.Add(Remember.Create(def, d => d.RequiredPowerInput,
                        (d, v) => d.RequiredPowerInput = v,
                        def.RequiredPowerInput * (mult / 8)));
                }

                // Ore Detector
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OreDetector))
                {
                    var def = myCubeBlockDefinition as MyOreDetectorDefinition;
                    if (def == null)
                        continue;

                    // Vanilla:
                    // Small 50m
                    // Large 150m
                    OriginalValues.Add(Remember.Create(def, d => d.MaximumRange,
                        (d, v) => d.MaximumRange = v,
                            def.MaximumRange * 20));
                }

                // Laser Antenna
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LaserAntenna))
                {
                    var def = myCubeBlockDefinition as MyLaserAntennaDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.PowerInputLasing,
                        (d, v) => d.PowerInputLasing = v,
                        def.PowerInputLasing / (largeGrid ? 10 : 20)));

                    if (!ModSaveData.LaserAntennaRequireLOS)
                        OriginalValues.Add(Remember.Create(def, d => d.RequireLineOfSight,
                            (d, v) => d.RequireLineOfSight = v, false ));
                }

                // Ship Welder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipWelder))
                {
                    var def = myCubeBlockDefinition as MyShipWelderDefinition;
                    if (def == null)
                        continue;

                    // This ensures that LyleCorp's / Novar's docking cameras with their specific overlay don't get replaced
                    if (def.Id.SubtypeId.String.Contains("LargeNaniteControlFacility"))
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.SensorRadius,
                        (d, v) => d.SensorRadius = v,
                        def.SensorRadius * (largeGrid ? 6 : 2)));
                }

                // Ship Grinder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipGrinder))
                {
                    var def = myCubeBlockDefinition as MyShipGrinderDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.SensorRadius,
                        (d, v) => d.SensorRadius = v,
                        def.SensorRadius * (largeGrid ? 6 : 1.25f)));
                }

                // Ship Drill
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                {
                    var def = myCubeBlockDefinition as MyShipDrillDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.CutOutRadius,
                        (d, v) => d.CutOutRadius = v,
                        def.CutOutRadius * (largeGrid ? 6 : 1.4f)));
                }

                // Thrusters
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                {
                    var def = myCubeBlockDefinition as MyThrustDefinition;
                    if (def == null)
                        continue;

                    // MES NPC-only thrusters
                    if (subtype.StartsWith("MES-NPC-"))
                        continue;

                    // Rider's Heli-carrier Thrusters
                    if (subtype.Contains("Heli"))
                        continue;

                    // Thruster balance based on where a thruster type is
                    // usable, rather than whether they need a conveyor.
                    //   * Atmo and Ion can be used only in atmo or space; are
                    //     stronger, consume more power.
                    //   * H2 can be used anywhere; are weaker, use less fuel.
                    switch (def.ThrusterType.String)
                    {
                        case "Hydrogen":
                            OriginalValues.Add(Remember.Create(def, (d) => def.ForceMagnitude,
                                (d, v) => def.ForceMagnitude = v,
                                (def.ForceMagnitude * (largeGrid ? 0.8f : 0.6f))));

                            OriginalValues.Add(Remember.Create(def.FuelConverter, (d) => d.Efficiency,
                                (d, v) => d.Efficiency = v, 1));

                            OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerConsumption,
                                (d, v) => d.MaxPowerConsumption = v,
                                (def.MaxPowerConsumption * 0.4)));
                            break;

                        case "Ion":
                            OriginalValues.Add(Remember.Create(def, (d) => def.ForceMagnitude,
                                (d, v) => def.ForceMagnitude = v,
                                (def.ForceMagnitude * (largeGrid ? 1.8f : 1.6f))));

                            OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerConsumption,
                                (d, v) => d.MaxPowerConsumption = v,
                                (def.MaxPowerConsumption * 1.5)));
                            break;

                        case "Atmospheric":
                            OriginalValues.Add(Remember.Create(def, (d) => def.ForceMagnitude,
                                (d, v) => def.ForceMagnitude = v,
                                (def.ForceMagnitude * (largeGrid ? 1.8f : 1.6f))));

                            OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerConsumption,
                                (d, v) => d.MaxPowerConsumption = v,
                                (def.MaxPowerConsumption * 1.5)));
                            break;
                    }
                }

                // Spotlights
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ReflectorLight))
                {
                    var def = myCubeBlockDefinition as MyReflectorBlockDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, d => d.LightReflectorRadius,
                        (d, v) => d.LightReflectorRadius = v,
                        new MyBounds(10, 780, 320)));
                }

                // Interior lights, corner lights, etc
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorLight))
                {
                    var def = myCubeBlockDefinition as MyLightingBlockDefinition;
                    if (def == null)
                        continue;

                    def.LightRadius = new MyBounds(1, 40, 3.6f);
                    OriginalValues.Add(Remember.Create(def, d => d.LightRadius,
                        (d, v) => d.LightRadius = v,
                        largeGrid ? new MyBounds(1, 200, 20) : new MyBounds(1, 100, 20)));
                }

                // Double all power generation. Shields and lazors be cray cray. Except nuclear. Nuclear be OP.
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_WindTurbine))
                {
                    var def = myCubeBlockDefinition as MyWindTurbineDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerOutput,
                        (d, v) => d.MaxPowerOutput = v,
                        (def.MaxPowerOutput * 4)));
                }

                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_SolarPanel))
                {
                    var def = myCubeBlockDefinition as MySolarPanelDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, (d) => def.MaxPowerOutput,
                        (d, v) => def.MaxPowerOutput = v,
                        (def.MaxPowerOutput * 4)));
                }

                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Reactor))
                {
                    var def = myCubeBlockDefinition as MyReactorDefinition;
                    if (def == null)
                        continue;

                    OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerOutput,
                        (d, v) => d.MaxPowerOutput = v,
                        (def.MaxPowerOutput / 2)));
                }

                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_HydrogenEngine))
                {
                    var def = myCubeBlockDefinition as MyHydrogenEngineDefinition;
                    if (def == null)
                        continue;

                    // ReSharper disable once CommentTypo
                    // Life'Tech Energy Fusion Reactors
                    if (subtype.Contains("FusionReactor")
                        || subtype.Contains("Fusion_Reactor")
                        || subtype.Contains("FusionReaktor"))
                        continue;

                    OriginalValues.Add(Remember.Create(def, (d) => d.MaxPowerOutput,
                        (d, v) => d.MaxPowerOutput = v,
                        (def.MaxPowerOutput * 3)));
                    OriginalValues.Add(Remember.Create(def, (d) => d.FuelProductionToCapacityMultiplier,
                        (d, v) => d.FuelProductionToCapacityMultiplier = v,
                        (def.FuelProductionToCapacityMultiplier * 3)));
                }
            }
        }

        public override void LoadData()
        {
            ModSaveData = Config.ReadFileFromWorldStorage<SaveData>("rebalance.xml", typeof(SaveData));
            if (ModSaveData == null)
            {
                ModSaveData = new SaveData
                {
                    BoostOxygenConsumption = true,
                    NerfJetpack = true,
                    LaserAntennaRequireLOS = false,
                };

                Config.WriteFileToWorldStorage("rebalance.xml", typeof(SaveData), ModSaveData);
            }

            DoWork();
        }

        protected override void UnloadData()
        {
            foreach (var r in OriginalValues) r.Restore();

            Config.WriteFileToWorldStorage("rebalance.xml", typeof(SaveData), ModSaveData);
        }
    }
}
