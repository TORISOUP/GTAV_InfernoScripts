namespace Inferno.ChaosMode
{
    /// <summary>
    /// jsonの読み込み結果をマッピングするDTO
    /// </summary>
    public class ChaosModeSettingDTO
    {
        public int Radius { get; set; } = 300;
        public bool IsChangeMissionCharacterWeapon { get; set; } = true;
        public int DefaultMissionCharacterTreatment { get; set; } = 1;
        public bool IsAttackPlayerCorrectionEnabled { get; set; } = false;
        public int AttackPlayerCorrectionProbabillity { get; set; } = 100;
        public string[] WeaponList { get; set; } = { "" };
        public string[] WeaponListForDriveBy { get; set; } = { "" };
        public bool IsStupidShooting { get; set; } = true;
        public int ShootAccuracy { get; set; } = 15;
        public int WeaponChangeProbabillity { get; set; } = 20;
    }
}