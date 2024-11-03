namespace Inferno.ChaosMode
{
    internal interface IWeaponProvider
    {
        Weapon GetRandomMeleeWeapons();

        Weapon GetRandomAllWeapons();

        /// <summary>
        /// 遠距離攻撃系の武器を取得する
        /// </summary>
        Weapon GetRandomWeaponExcludeClosedWeapon();

        /// <summary>
        /// 爆発物系の武器を取得する
        /// </summary>
        Weapon GetExplosiveWeapon();
        
        /// <summary>
        /// ドライブバイ用の武器を取得する
        /// </summary>
        Weapon GetRandomDriveByWeapon();
    }
}