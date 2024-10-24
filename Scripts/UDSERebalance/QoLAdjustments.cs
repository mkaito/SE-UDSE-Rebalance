using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using SpaceEngineers.Game.Definitions.SafeZone;
using UDSERebalance.ConfigData;
using UDSERebalance.Utilities;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

namespace UDSERebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    // ReSharper disable once UnusedType.Global
    public class QoLAdjustments : MySessionComponentBase
    {
        private readonly List<IRemember> _originalValues = new List<IRemember>();
        private SaveData _modSaveData;

        private void DoWork()
        {
            // Player character adjustments
            foreach (var myCharacterDefinition in
                MyDefinitionManager.Static.Characters.Where(def => def.UsableByPlayer))
            {
                if (_modSaveData.NerfJetpack)
                {
                    var jp = myCharacterDefinition.Jetpack.ThrustProperties;

                    // Nerf the jetpack
                    _originalValues.Add(
                        Remember.Create(jp, d => d.ForceMagnitude, (d, v) => d.ForceMagnitude = v, 1600));
                    _originalValues.Add(Remember.Create(jp, d => jp.SlowdownFactor, (d, v) => d.SlowdownFactor = v, 1));
                    _originalValues.Add(Remember.Create(jp, d => d.MinPowerConsumption,
                        (d, v) => d.MinPowerConsumption = v, 0.0000021666666666666666666666666667f));
                    _originalValues.Add(Remember.Create(jp, d => d.MaxPowerConsumption,
                        (d, v) => d.MaxPowerConsumption = v, 0.00065f));
                    _originalValues.Add(Remember.Create(jp, d => d.ConsumptionFactorPerG,
                        (d, v) => d.ConsumptionFactorPerG = v, 135));
                    _originalValues.Add(Remember.Create(jp, d => d.EffectivenessAtMinInfluence,
                        (d, v) => d.EffectivenessAtMinInfluence = v, 1));
                    _originalValues.Add(Remember.Create(jp, d => d.EffectivenessAtMaxInfluence,
                        (d, v) => d.EffectivenessAtMaxInfluence = v, 0));
                }

                // Increase oxygen consumption significantly
                if (_modSaveData.BoostOxygenConsumption)
                {
                    _originalValues.Add(Remember.Create(myCharacterDefinition, d => d.OxygenConsumptionMultiplier,
                        (d, v) => d.OxygenConsumptionMultiplier = v, 12));
                }

                // Increase suit oxygen capacity
                if (_modSaveData.BoostOxygenCapacity)
                {
                    var oxygenTank = myCharacterDefinition.SuitResourceStorage.Find(x => x.Id.SubtypeName == "Oxygen");
                    _originalValues.Add(Remember.Create(oxygenTank, d => d.MaxCapacity,
                        (d, v) => d.MaxCapacity = v, 8));
                }
            }

            // Increase oxygen bottle capacity
            if (_modSaveData.BoostOxygenCapacity)
            {
                var oxygenBottle = MyDefinitionManager.Static.GetAllDefinitions()
                        .Select(myDefinitionBase => myDefinitionBase as MyPhysicalItemDefinition)
                        .Where(myPhysicalItemDefinition => myPhysicalItemDefinition?.Id.SubtypeId.String == "OxygenBottle")
                        .First() as MyOxygenContainerDefinition;

                if (oxygenBottle != null)
                {
                    _originalValues.Add(Remember.Create(oxygenBottle, d => d.Capacity,
                        (d, v) => d.Capacity = v, 8));
                }


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
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.MaxGasOutput, (d, v) => d.MaxGasOutput = v,
                        def.MaxGasOutput * 30));
                }

                // Wheel Suspension
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_MotorSuspension))
                {
                    var def = myCubeBlockDefinition as MyMotorSuspensionDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    var mult = largeGrid ? 20 : 8;

                    _originalValues.Add(Remember.Create(def, d => d.AxleFriction, (d, v) => d.AxleFriction = v,
                        def.AxleFriction * mult));
                    _originalValues.Add(Remember.Create(def, d => d.PropulsionForce, (d, v) => d.PropulsionForce = v,
                        def.PropulsionForce * mult));
                    // ReSharper disable once PossibleLossOfFraction
                    _originalValues.Add(Remember.Create(def, d => d.RequiredPowerInput,
                        (d, v) => d.RequiredPowerInput = v, def.RequiredPowerInput * (mult / 8)));
                }

                // Ore Detector
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OreDetector))
                {
                    var def = myCubeBlockDefinition as MyOreDetectorDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    // Vanilla:
                    // Small 50m
                    // Large 150m
                    _originalValues.Add(Remember.Create(def, d => d.MaximumRange, (d, v) => d.MaximumRange = v,
                        def.MaximumRange * 8));
                }

                // Laser Antenna
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LaserAntenna))
                {
                    var def = myCubeBlockDefinition as MyLaserAntennaDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.PowerInputLasing, (d, v) => d.PowerInputLasing = v,
                        def.PowerInputLasing / (largeGrid ? 10 : 20)));

                    if (!_modSaveData.LaserAntennaRequireLos)
                    {
                        _originalValues.Add(Remember.Create(def, d => d.RequireLineOfSight,
                            (d, v) => d.RequireLineOfSight = v, false));
                    }
                }

                // Ship Welder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipWelder))
                {
                    var def = myCubeBlockDefinition as MyShipWelderDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    if (def.Id.SubtypeId.String.Contains("LargeNaniteControlFacility"))
                    {
                        continue;
                    }

                    var newRadius = def.SensorRadius * (largeGrid ? 1.25f : 1.20f);
                    _originalValues.Add(Remember.Create(def, d => d.SensorRadius, (d, v) => d.SensorRadius = v,
                        newRadius));
                    _originalValues.Add(Remember.Create(def, d => d.SensorOffset, (d, v) => d.SensorOffset = v,
                        newRadius - 1.50f));
                }

                // Ship Grinder
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ShipGrinder))
                {
                    var def = myCubeBlockDefinition as MyShipGrinderDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    var newRadius = def.SensorRadius * (largeGrid ? 1.25f : 1.20f);
                    _originalValues.Add(Remember.Create(def, d => d.SensorRadius, (d, v) => d.SensorRadius = v,
                        newRadius));
                    _originalValues.Add(Remember.Create(def, d => d.SensorOffset, (d, v) => d.SensorOffset = v,
                        newRadius - 1.50f));
                }

                // Ship Drill
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                {
                    var def = myCubeBlockDefinition as MyShipDrillDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    var newRadius = def.CutOutRadius * (largeGrid ? 1.50f : 1.20f);
                    _originalValues.Add(Remember.Create(def, d => d.CutOutRadius, (d, v) => d.CutOutRadius = v,
                        newRadius));
                }

                // Jump Drive
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_JumpDrive))
                {
                    if (!_modSaveData.EndgameJumpDrive)
                    {
                        continue;
                    }    

                    var def = myCubeBlockDefinition as MyJumpDriveDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.RequiredPowerInput,
                        (d, v) => d.RequiredPowerInput = v,
                        def.RequiredPowerInput * (largeGrid ? 2000 : 500)));

                    _originalValues.Add(Remember.Create(def, d => d.PowerNeededForJump,
                        (d, v) => d.PowerNeededForJump = v,
                        def.PowerNeededForJump * (largeGrid ? 20000 : 5000)));

                    _originalValues.Add(Remember.Create(def, d => d.MaxJumpDistance,
                        (d, v) => d.MaxJumpDistance = v,
                        def.MaxJumpDistance * (largeGrid ? 10000 : 500)));

                    _originalValues.Add(Remember.Create(def, d => d.MaxJumpMass,
                        (d, v) => d.MaxJumpMass = v,
                        def.MaxJumpMass * (largeGrid ? 500000 : 2000)));
                }

                // Thrusters
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                {
                    if (!_modSaveData.ThrusterRebalance)
                        continue;

                    var def = myCubeBlockDefinition as MyThrustDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    // MES NPC-only thrusters
                    if (subtype.StartsWith("MES-NPC-"))
                    {
                        continue;
                    }

                    // Rider's Heli-carrier Thrusters
                    if (subtype.Contains("Heli"))
                    {
                        continue;
                    }

                    // Life Tech Fusion Engines
                    if (subtype.Contains("FusionEngine"))
                    {
                        continue;
                    }

                    // BY's Gravity Engines
                    if (subtype.StartsWith("BY_GravityEngine"))
                    {
                        continue;
                    }

                    // Aryx Lynxon Drives
                    if (subtype.StartsWith("AryxRCS") || subtype.StartsWith("LynxRcs") ||
                        subtype.StartsWith("ARYLNX") || subtype.StartsWith("ARYXLNX"))
                    {
                        continue;
                    }


                    // Expanse Style Thruster Rebalance:
                    // General idea: Ion and Atmospheric are power hungry but fairly efficient.
                    // Hydrogen consumes a LOT of fuel, but is very powerful. Mostly useful as a booster.
                    // In general, flight should be expensive. Microgravity allows for coasting, but flying in gravity should not be a routine thing.

                    // General Thruster Rebalance:
                    // All thrusters are a little stronger, favouring large grid.
                    // H2 thrusters use a little less fuel. Electric thrusters use a little more power.

                    switch (def.ThrusterType.String)
                    {
                        case "Hydrogen":
                            if(_modSaveData.ExpanseStyleThrusterRebalance)
                            {
                                // Strong but thirsty
                                // Reference target: LG Large: 12MN, 10.59 kL/s
                                // Vanilla reference: LG Large: 7.2MN, 4.82 kL/s
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 1.75f : 1.25f)));

                                _originalValues.Add(Remember.Create(def.FuelConverter, d => d.Efficiency,
                                    (d, v) => d.Efficiency = v, 0.18f));

                                // Reduce effectiveness in vacuum to 50%
                                _originalValues.Add(Remember.Create(def, d => d.MinPlanetaryInfluence,
                                    (d, v) => d.MinPlanetaryInfluence = v, 0f));
                                _originalValues.Add(Remember.Create(def, d => d.MaxPlanetaryInfluence,
                                    (d, v) => d.MaxPlanetaryInfluence = v, 1.0f));
                                _originalValues.Add(Remember.Create(def,
                                    d => d.EffectivenessAtMinInfluence,
                                    (d, v) => d.EffectivenessAtMinInfluence = v, 0.5f));
                                _originalValues.Add(Remember.Create(def,
                                    d => d.EffectivenessAtMaxInfluence,
                                    (d, v) => d.EffectivenessAtMaxInfluence = v, 1.0f));
                                _originalValues.Add(Remember.Create(def,
                                    d => d.NeedsAtmosphereForInfluence,
                                    (d, v) => d.NeedsAtmosphereForInfluence = v, true));
                            } else
                            {
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 2.85f : 2.15f)));

                                _originalValues.Add(Remember.Create(def.FuelConverter, d => d.Efficiency,
                                    (d, v) => d.Efficiency = v, 0.75f));
                            }
                            break;

                        case "Ion":
                            if(_modSaveData.ExpanseStyleThrusterRebalance)
                            {
                                // Very, very low power. Prefer using Epstein and REX in space, but Ion might have niche
                                // use cases.
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 0.10f : 0.15f)));
                                _originalValues.Add(Remember.Create(def, d => d.MaxPowerConsumption,
                                    (d, v) => d.MaxPowerConsumption = v, def.MaxPowerConsumption * 0.05f));
                            } else
                            {
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 2.20f : 1.85f)));
                                _originalValues.Add(Remember.Create(def, d => d.MaxPowerConsumption,
                                    (d, v) => d.MaxPowerConsumption = v, def.MaxPowerConsumption * 1.45f));
                            }
                            break;

                        case "Atmospheric":
                            if (_modSaveData.ExpanseStyleThrusterRebalance)
                            {
                                // Quite strong. Mind the 30% atmosphere gap though. To actually make it to space, you'll
                                // need Epstein or Hydrogen boosters.
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 3.00f : 2.00f)));
                                _originalValues.Add(Remember.Create(def, d => d.MaxPowerConsumption,
                                    (d, v) => d.MaxPowerConsumption = v, def.MaxPowerConsumption * 2.80f));
                            } else
                            {
                                _originalValues.Add(Remember.Create(def, d => def.ForceMagnitude,
                                    (d, v) => def.ForceMagnitude = v, def.ForceMagnitude * (largeGrid ? 2.35f : 1.95f)));
                                _originalValues.Add(Remember.Create(def, d => d.MaxPowerConsumption,
                                    (d, v) => d.MaxPowerConsumption = v, def.MaxPowerConsumption * 1.65f));
                            }
                            break;
                    }
                }

                // Spotlights
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_ReflectorLight))
                {
                    var def = myCubeBlockDefinition as MyReflectorBlockDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.LightReflectorRadius,
                        (d, v) => d.LightReflectorRadius = v, new MyBounds(10, 1250, 780)));
                }

                // Interior lights, corner lights, etc
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorLight) ||
                         myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LightingBlock))
                {
                    var def = myCubeBlockDefinition as MyLightingBlockDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.LightRadius, (d, v) => d.LightRadius = v,
                        largeGrid ? new MyBounds(1, 200, 20) : new MyBounds(1, 100, 20)));
                }

                // Make H2 production a highly inefficient and power hungry process
                // Rationale: Instead of requiring some rare stuff to get off a planet, make a scaling challenge.
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OxygenGenerator))
                {
                    var def = myCubeBlockDefinition as MyOxygenGeneratorDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    // Consume more power while operating
                    _originalValues.Add(Remember.Create(def, d => def.OperationalPowerConsumption,
                        (d, v) => def.OperationalPowerConsumption = v, def.OperationalPowerConsumption * 4.0f));
                }

                // Increase power generation of wind turbines and solar panels.
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_WindTurbine))
                {
                    var def = myCubeBlockDefinition as MyWindTurbineDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => d.MaxPowerOutput, (d, v) => d.MaxPowerOutput = v,
                        def.MaxPowerOutput * 2.2f));
                }

                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_SolarPanel))
                {
                    var def = myCubeBlockDefinition as MySolarPanelDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    _originalValues.Add(Remember.Create(def, d => def.MaxPowerOutput, (d, v) => def.MaxPowerOutput = v,
                        def.MaxPowerOutput * 2.8f));
                }

                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_HydrogenEngine))
                {
                    var def = myCubeBlockDefinition as MyHydrogenEngineDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    // ReSharper disable once CommentTypo
                    // Life Tech Energy Fusion Reactors
                    if (subtype.Contains("FusionReactor") || subtype.Contains("Fusion_Reactor") ||
                        subtype.Contains("FusionReaktor") || subtype.Contains("Fuelcell"))
                    {
                        continue;
                    }

                    // Engines are very fuel inefficient, producing about half as much power as was necessary to produce the H2 they burn.
                    _originalValues.Add(
                        Remember.Create(def, d => d.FuelProductionToCapacityMultiplier,
                            (d, v) => d.FuelProductionToCapacityMultiplier = v,
                            def.FuelProductionToCapacityMultiplier * 0.20f)
                    );

                    // _originalValues.Add(Remember.Create(def, d => d.MaxPowerOutput,
                    //     (d, v) => d.MaxPowerOutput = v,
                    //     def.MaxPowerOutput * 0.15f));
                }

                // Increase battery max output, input, and capacity.
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_BatteryBlock))
                {
                    var def = myCubeBlockDefinition as MyBatteryBlockDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    const int multiplier = 3;

                    _originalValues.Add(
                        Remember.Create(def, d => d.MaxPowerOutput,
                            (d, v) => d.MaxPowerOutput = v,
                            def.MaxPowerOutput * multiplier));

                    _originalValues.Add(
                        Remember.Create(def, d => d.MaxStoredPower,
                            (d, v) => d.MaxStoredPower = v,
                            def.MaxStoredPower * multiplier));

                    _originalValues.Add(
                        Remember.Create(def, d => d.RequiredPowerInput,
                            (d, v) => d.RequiredPowerInput = v,
                            def.RequiredPowerInput * multiplier * 1.25f));
                }

                // Safe Zones
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_SafeZone))
                {
                    var def = myCubeBlockDefinition as MySafeZoneBlockDefinition;
                    if (def == null)
                    {
                        continue;
                    }

                    // Min usage 1MW
                    _originalValues.Add(Remember.Create(def, d => d.MaxSafeZonePowerDrainkW,
                        (d, v) => d.MaxSafeZonePowerDrainkW = v, 10));
                    // Max usage 100MW
                    _originalValues.Add(Remember.Create(def, d => d.MinSafeZonePowerDrainkW,
                        (d, v) => d.MinSafeZonePowerDrainkW = v, 1000));
                    // 24h per chip
                    _originalValues.Add(Remember.Create(def, d => d.SafeZoneUpkeepTimeM,
                        (d, v) => d.SafeZoneUpkeepTimeM = v, 24u * 60));
                }
            }
        }

        public override void LoadData()
        {
            _modSaveData = Config.ReadFileFromWorldStorage<SaveData>("rebalance.xml", typeof(SaveData));
            if (_modSaveData == null)
            {
                _modSaveData = new SaveData
                {
                    BoostOxygenConsumption = true,
                    BoostOxygenCapacity = false,
                    NerfJetpack = true,
                    LaserAntennaRequireLos = true,
                    ThrusterRebalance = true,
                    ExpanseStyleThrusterRebalance = false,
                    EndgameJumpDrive = false,
                };

                Config.WriteFileToWorldStorage("rebalance.xml", typeof(SaveData), _modSaveData);
            }

            DoWork();
        }

        protected override void UnloadData()
        {
            foreach (var r in _originalValues)
            {
                r.Restore();
            }

            Config.WriteFileToWorldStorage("rebalance.xml", typeof(SaveData), _modSaveData);
        }
    }
}
