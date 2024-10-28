namespace UDSERebalance.ConfigData
{
    // ReSharper disable ConvertToConstant.Global
    public class SaveData
    {
        public float BoostOxygenConsumption { get; set; } = 12f;
        public float BoostOxygenCapacity { get; set; } = 8f;
        public bool NerfJetpack { get; set; } = false;
        public bool LaserAntennaRequireLos { get; set; } = true;
        public bool ThrusterRebalance { get; set; } = false;
        public bool ExpanseStyleThrusterRebalance { get; set; } = false;
        public bool EndgameJumpDrive { get; set; } = false;
        public float BoostHydrogenTankCapacity { get; set; } = 4f;
    }
}