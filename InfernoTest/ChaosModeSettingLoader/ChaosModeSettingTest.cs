using System;
using System.Linq;
using Inferno;
using Inferno.ChaosMode;
using Xunit;

namespace InfernoTest
{
    /// <summary>
    /// ChaosModeSettingのテスト
    /// </summary>
    public class ChaosModeSettingTest
    {
        [Fact]
        public void DefaultMissionCharacterTreatmentを正しく設定できる()
        {
            var dto = new ChaosModeSettingDTO();

            //0はAffectAllCharacter
            dto.DefaultMissionCharacterTreatment = 0;
            Assert.Equal(MissionCharacterTreatmentType.AffectAllCharacter
                , new ChaosModeSetting(dto).DefaultMissionCharacterTreatment);

            //1はExcludeUniqueCharacter
            dto.DefaultMissionCharacterTreatment = 1;
            Assert.Equal(MissionCharacterTreatmentType.ExcludeUniqueCharacter
                , new ChaosModeSetting(dto).DefaultMissionCharacterTreatment);

            //2はExcludeAllMissionCharacter
            dto.DefaultMissionCharacterTreatment = 2;
            Assert.Equal(MissionCharacterTreatmentType.ExcludeAllMissionCharacter
                , new ChaosModeSetting(dto).DefaultMissionCharacterTreatment);
        }

        [Fact]
        public void 不正なDefaultMissionCharacterTreatmentはExcludeUniqueCharacterになる()
        {
            var dto = new ChaosModeSettingDTO();
            dto.DefaultMissionCharacterTreatment = -1;
            Assert.Equal(MissionCharacterTreatmentType.ExcludeUniqueCharacter
                , new ChaosModeSetting(dto).DefaultMissionCharacterTreatment);
        }

        private class TestSetting : ChaosModeSetting
        {
            public TestSetting(ChaosModeSettingDTO dto) : base(dto)
            {
            }

            public Weapon[] TestEnableWeaponListFilter(string[] WeaponList)
            {
                return EnableWeaponListFilter(WeaponList);
            }
        }

        #region EnableWeaponListFilterTest

        [Fact]
        public void 正常な武器の文字列のみの配列からWeaponの配列が生成できる()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new[]
            {
                Weapon.Unarmed.ToString(), Weapon.Pistol.ToString(), Weapon.AssaultSMG.ToString(),
                Weapon.StunGun.ToString()
            };
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数が同じ
            Assert.Equal(testData.Length, resutl.Length);
            //要素も同じ
            Assert.True(resutl.All(x => testData.Contains(x.ToString())));
        }

        [Fact]
        public void 不正な武器の文字列を含む配列からWeaponの配列が生成できる()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new[] { Weapon.Unarmed.ToString(), Weapon.Pistol.ToString(), "STANGUN" };
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数は2
            Assert.Equal(2, resutl.Length);
            //UNARMEDとPISTOLのみのはず
            Assert.Contains(Weapon.Unarmed, resutl);
            Assert.Contains(Weapon.Pistol, resutl);
        }

        [Fact]
        public void 空配列を渡すと空のWeaponの配列が生成される()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new string[0];
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            Assert.Empty(resutl);
        }

        [Fact]
        public void nullを渡すと空のWeaponの配列が生成される()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var resutl = testSetting.TestEnableWeaponListFilter(null);

            Assert.Empty(resutl);
        }

        [Fact]
        public void 不正な文字列のみを渡すと全てのWeaponの配列が生成できる()
        {
            var allWeapons = ((Weapon[])Enum.GetValues(typeof(Weapon))).OrderBy(x => x.ToString()).ToArray();
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new[] { "HOGE", "FUGA", "PIYO" };
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数は全ての武器数と同じ
            Assert.Equal(allWeapons.Length, resutl.Length);
            //同じ配列になるはず
            Assert.True(allWeapons.SequenceEqual(resutl.OrderBy(x => x.ToString())));
        }

        #endregion EnableWeaponListFilterTest
    }
}