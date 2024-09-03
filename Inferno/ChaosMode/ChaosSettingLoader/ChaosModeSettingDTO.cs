namespace Inferno.ChaosMode
{
    /// <summary>
    /// jsonの読み込み結果をマッピングするDTO
    /// </summary>
    public class ChaosModeSettingDTO
    {
        public int Radius { get; set; } = 500;
        public bool IsChangeMissionCharacterWeapon { get; set; } = true;
        public int DefaultMissionCharacterTreatment { get; set; } = 1;
        public bool IsAttackPlayerCorrectionEnabled { get; set; } = false;
        public int AttackPlayerCorrectionProbability { get; set; } = 100;
        public string[] WeaponList { get; set; } = new[] { "" };
        public string[] WeaponListForDriveBy { get; set; } = new[] { "" };
        public int StupidPedRate { get; set; } = 80;
        public int ShootAccuracy { get; set; } = 10;
        public int WeaponChangeProbability { get; set; } = 100;
        public int ForceExplosiveWeaponProbability { get; set; } = 10;
    }
}