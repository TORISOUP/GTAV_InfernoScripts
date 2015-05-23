using System;
using System.Linq;
using Inferno;
using Inferno.ChaosMode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfernoTest
{
    /// <summary>
    /// ChaosModeSettingのテスト
    /// </summary>
    [TestClass]
    public class ChaosModeSettingTest
    {
        [TestMethod]
        public void DefaultMissionCharacterTreatmentを正しく設定できる()
        {
            var dto = new ChaosModeSettingDTO();

            //0はAffectAllCharacter
            dto.DefaultMissionCharacterTreatment = 0;
            Assert.AreEqual(MissionCharacterTreatmentType.AffectAllCharacter
                , (new ChaosModeSetting(dto).DefaultMissionCharacterTreatment));

            //1はExcludeUniqueCharacter
            dto.DefaultMissionCharacterTreatment = 1;
            Assert.AreEqual(MissionCharacterTreatmentType.ExcludeUniqueCharacter
                , (new ChaosModeSetting(dto).DefaultMissionCharacterTreatment));

            //2はExcludeAllMissionCharacter
            dto.DefaultMissionCharacterTreatment = 2;
            Assert.AreEqual(MissionCharacterTreatmentType.ExcludeAllMissionCharacter
                , (new ChaosModeSetting(dto).DefaultMissionCharacterTreatment));
        }

        [TestMethod]
        public void 不正なDefaultMissionCharacterTreatmentはExcludeUniqueCharacterになる()
        {
            var dto = new ChaosModeSettingDTO();
            dto.DefaultMissionCharacterTreatment = -1;
            Assert.AreEqual(MissionCharacterTreatmentType.ExcludeUniqueCharacter
                , (new ChaosModeSetting(dto).DefaultMissionCharacterTreatment));
        }


        #region EnableWeaponListFilterTest

        [TestMethod]
        public void 正常な武器の文字列のみの配列からWeaponの配列が生成できる()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new String[] {Weapon.UNARMED.ToString(),Weapon.PISTOL.ToString(),Weapon.ASSAULTSMG.ToString(),Weapon.STUNGUN.ToString()};
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数が同じ
            Assert.AreEqual(testData.Length, resutl.Length);
            //要素も同じ
            Assert.IsTrue(resutl.All(x=>testData.Contains(x.ToString())));
        }

        [TestMethod]
        public void 不正な武器の文字列を含む配列からWeaponの配列が生成できる()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new String[] { Weapon.UNARMED.ToString(), Weapon.PISTOL.ToString(), "STANGUN" };
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数は2
            Assert.AreEqual(2, resutl.Length);
            //UNARMEDとPISTOLのみのはず
            Assert.IsTrue(resutl.Contains(Weapon.UNARMED));
            Assert.IsTrue(resutl.Contains(Weapon.PISTOL));
        }

        [TestMethod]
        public void 空配列を渡すと空のWeaponの配列が生成される()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new String[0];
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            Assert.AreEqual(0, resutl.Length);

        }

        [TestMethod]
        public void nullを渡すと空のWeaponの配列が生成される()
        {
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var resutl = testSetting.TestEnableWeaponListFilter(null);

            Assert.AreEqual(0, resutl.Length);
        }

        [TestMethod]
        public void 不正な文字列のみを渡すと全てのWeaponの配列が生成できる()
        {
            var allWeapons = ((Weapon[])Enum.GetValues(typeof(Weapon))).OrderBy(x => x.ToString()).ToArray();
            var testSetting = new TestSetting(new ChaosModeSettingDTO());
            var testData = new String[] {"HOGE", "FUGA", "PIYO"};
            var resutl = testSetting.TestEnableWeaponListFilter(testData);

            //数は全ての武器数と同じ
            Assert.AreEqual(allWeapons.Length, resutl.Length);
            //同じ配列になるはず
            Assert.IsTrue(allWeapons.SequenceEqual(resutl.OrderBy(x => x.ToString())));
        }

        #endregion

        class TestSetting:ChaosModeSetting
        {
            public Weapon[] TestEnableWeaponListFilter(string[] WeaponList)
            {
                return this.EnableWeaponListFilter(WeaponList);
            }

            public TestSetting(ChaosModeSettingDTO dto) : base(dto)
            {
            }
        }
    }
}
