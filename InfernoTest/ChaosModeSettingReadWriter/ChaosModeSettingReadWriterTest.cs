using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Inferno;
using Inferno.ChaosMode;
using Xunit;
using Xunit.Sdk;

namespace InfernoTest
{
    /// <summary>
    /// ChaosModeSettingReadWriterTest の概要の説明
    /// </summary>
    public class ChaosModeSettingReadWriterTest
    {
        [Theory]
        [FileData("ChaosModeSettingReadWriter/TestData/CorrectConf.json")]
        public void 全て正常値が設定されたJsonからChaosSettingが生成できる(string json)
        {
            var testLoader = new TestChaosModeSettingReadWriter(json);
            var result = testLoader.LoadSettingFile("");

            Assert.Equal(500, result.Radius);
            Assert.True(result.OverrideMissionCharacterWeapon);
            Assert.Equal(MissionCharacterBehaviour.ExcludeUniqueCharacter,
                result.MissionCharacterBehaviour);
            Assert.False(result.IsAttackPlayerCorrectionEnabled);
            Assert.Equal(100, result.AttackPlayerCorrectionProbability);
            Assert.Equal(new[] { Weapon.AssaultSMG }, result.WeaponList);
            Assert.Equal(new[] { Weapon.AutoShotgun, Weapon.MicroSMG }, result.WeaponListForDriveBy);
            Assert.Equal(50, result.StupidShootingRate);
            Assert.Equal(10, result.ShootAccuracy);
            Assert.Equal(100, result.WeaponChangeProbability);
            Assert.Equal(30, result.ForceExplosiveWeaponProbability);
            Assert.False(result.MeleeWeaponOnly);
            Assert.Equal(30, result.WeaponDropProbability);

        }

        [Theory]
        [FileData("ChaosModeSettingReadWriter/TestData/PartialConf.json")]
        public void 一部パラメータが抜けていてもデフォルト値で上書きされたChaosSettingが生成できる(string json)
        {
            var testLoader = new TestChaosModeSettingReadWriter(json);
            var result = testLoader.LoadSettingFile("");

            // 存在する
            Assert.Equal(500, result.Radius);
            Assert.False(result.OverrideMissionCharacterWeapon);
            Assert.True(result.IsAttackPlayerCorrectionEnabled);
            Assert.Equal(10, result.AttackPlayerCorrectionProbability);
            Assert.Equal(new[] { Weapon.AdvancedRifle }, result.WeaponList);
            Assert.Equal(new[] { Weapon.Pistol }, result.WeaponListForDriveBy);

            var settingDefault = new ChaosModeSettingDTO();

            // 存在しないものはデフォルト値
            Assert.Equal((MissionCharacterBehaviour)settingDefault.MissionCharacterBehaviour,
                result.MissionCharacterBehaviour);

            Assert.Equal(settingDefault.StupidShootingRate, result.StupidShootingRate);
            Assert.Equal(settingDefault.ShootAccuracy, result.ShootAccuracy);
            Assert.Equal(settingDefault.WeaponChangeProbability, result.WeaponChangeProbability);
            Assert.Equal(settingDefault.ForceExplosiveWeaponProbability, result.ForceExplosiveWeaponProbability);
        }


        [Fact]
        public void 不正なjsonが渡された場合はデフォルト値の設定ファイルになる()
        {
            var testLoader = new TestChaosModeSettingReadWriter("{AAA,BBB}"); //jsonの文法違反
            var result = testLoader.LoadSettingFile("");

            // デフォルト値
            var settingDefault = new ChaosModeSettingDTO();

            Assert.Equal(settingDefault.Radius, result.Radius);
            Assert.Equal(settingDefault.OverrideMissionCharacterWeapon, result.OverrideMissionCharacterWeapon);
            Assert.Equal(settingDefault.IsAttackPlayerCorrectionEnabled, result.IsAttackPlayerCorrectionEnabled);
            Assert.Equal(settingDefault.AttackPlayerCorrectionProbability, result.AttackPlayerCorrectionProbability);
            Assert.Equal((MissionCharacterBehaviour)settingDefault.MissionCharacterBehaviour,
                result.MissionCharacterBehaviour);
            Assert.Equal(settingDefault.StupidShootingRate, result.StupidShootingRate);
            Assert.Equal(settingDefault.ShootAccuracy, result.ShootAccuracy);
            Assert.Equal(settingDefault.WeaponChangeProbability, result.WeaponChangeProbability);
            Assert.Equal(settingDefault.ForceExplosiveWeaponProbability, result.ForceExplosiveWeaponProbability);

            // リストは少なくとも1つ以上の値がある
            Assert.True(result.WeaponList.Count > 0);
            Assert.True(result.WeaponListForDriveBy.Count > 0);
        }

        [Theory]
        [FileData("ChaosModeSettingReadWriter/TestData/WrongType.json")]
        public void jsonの型が一致しない場合はデフォルト値の設定ファイルになる(string json)
        {
            var testLoader = new TestChaosModeSettingReadWriter(json);
            var result = testLoader.LoadSettingFile("");
            
            // デフォルト値
            var settingDefault = new ChaosModeSettingDTO();

            Assert.Equal(settingDefault.Radius, result.Radius);
            Assert.Equal(settingDefault.OverrideMissionCharacterWeapon, result.OverrideMissionCharacterWeapon);
            Assert.Equal(settingDefault.IsAttackPlayerCorrectionEnabled, result.IsAttackPlayerCorrectionEnabled);
            Assert.Equal(settingDefault.AttackPlayerCorrectionProbability, result.AttackPlayerCorrectionProbability);
            Assert.Equal((MissionCharacterBehaviour)settingDefault.MissionCharacterBehaviour,
                result.MissionCharacterBehaviour);
            Assert.Equal(settingDefault.StupidShootingRate, result.StupidShootingRate);
            Assert.Equal(settingDefault.ShootAccuracy, result.ShootAccuracy);
            Assert.Equal(settingDefault.WeaponChangeProbability, result.WeaponChangeProbability);
            Assert.Equal(settingDefault.ForceExplosiveWeaponProbability, result.ForceExplosiveWeaponProbability);

            // リストは少なくとも1つ以上の値がある
            Assert.True(result.WeaponList.Count > 0);
            Assert.True(result.WeaponListForDriveBy.Count > 0);
        }

        [Fact]
        public void 空文字列だった場合はデフォルト値のChaosSettingが生成される()
        {
            var testLoader = new TestChaosModeSettingReadWriter("");
            var result = testLoader.LoadSettingFile("");
            
            // デフォルト値
            var settingDefault = new ChaosModeSettingDTO();

            Assert.Equal(settingDefault.Radius, result.Radius);
            Assert.Equal(settingDefault.OverrideMissionCharacterWeapon, result.OverrideMissionCharacterWeapon);
            Assert.Equal(settingDefault.IsAttackPlayerCorrectionEnabled, result.IsAttackPlayerCorrectionEnabled);
            Assert.Equal(settingDefault.AttackPlayerCorrectionProbability, result.AttackPlayerCorrectionProbability);
            Assert.Equal((MissionCharacterBehaviour)settingDefault.MissionCharacterBehaviour,
                result.MissionCharacterBehaviour);
            Assert.Equal(settingDefault.StupidShootingRate, result.StupidShootingRate);
            Assert.Equal(settingDefault.ShootAccuracy, result.ShootAccuracy);
            Assert.Equal(settingDefault.WeaponChangeProbability, result.WeaponChangeProbability);
            Assert.Equal(settingDefault.ForceExplosiveWeaponProbability, result.ForceExplosiveWeaponProbability);

            // リストは少なくとも1つ以上の値がある
            Assert.True(result.WeaponList.Count > 0);
            Assert.True(result.WeaponListForDriveBy.Count > 0);
        }

        /// <summary>
        /// テスト用ローダー
        /// </summary>
        private class TestChaosModeSettingReadWriter : ChaosModeSettingReadWriter
        {
            private readonly string _readJson;

            /// <summary>
            /// コンストラクタで渡された文字列をファイルから読み込んだjsonとして扱う
            /// </summary>
            /// <param name="readJson">json文字列</param>
            public TestChaosModeSettingReadWriter(string readJson)
            {
                _readJson = readJson;
            }

            /// <summary>
            /// テスト時はログを吐かないようにMockに挿し替える
            /// </summary>
            protected override IDebugLogger DebugLogger => EmptyDebugLogger.Instance;

            protected override string ReadFile(string filePath)
            {
                return _readJson;
            }
        }

        private class EmptyDebugLogger : IDebugLogger
        {
            public static EmptyDebugLogger Instance = new EmptyDebugLogger();

            public void Dispose()
            {
            }

            public void Log(string message)
            {
            }
        }

        public class FileDataAttribute : DataAttribute
        {
            private readonly string _path;

            public FileDataAttribute(string path)
            {
                _path = path;
            }

            public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            {
                if (File.Exists(_path))
                {
                    yield return new object[] { File.ReadAllText(_path) };
                }
            }
        }
    }
}