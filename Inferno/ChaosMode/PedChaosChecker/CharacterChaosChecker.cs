using System;
using GTA;

namespace Inferno.ChaosMode
{
    public class CharacterChaosChecker
    {
        public CharacterChaosChecker(MissionCharacterTreatmentType missionCharacterTreatment,
            bool isChangeMissonCharacterWeapon)
        {
            MissionCharacterTreatment = missionCharacterTreatment;
            IsChangeMissonCharacterWeapon = isChangeMissonCharacterWeapon;
        }

        /// <summary>
        /// ミッションキャラの武器を変更するか
        /// </summary>
        public bool IsChangeMissonCharacterWeapon { get; set; }

        /// <summary>
        /// ミッションキャラのカオス化
        /// </summary>
        public MissionCharacterTreatmentType MissionCharacterTreatment { get; set; }

        /// <summary>
        /// 攻撃を避けるべき対象群
        /// </summary>
        public Entity[] AvoidAttackEntities { get; set; } = Array.Empty<Entity>();

        /// <summary>
        /// カオス化対象であるか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueでカオス化して良い</returns>
        public bool IsPedChaosAvailable(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer && !ped.IsNotChaosPed() &&
                   IsRiotableMissionCharacter(ped) && !IsPedNearAvoidAttackEntities(ped);
        }

        public bool IsPedNearAvoidAttackEntities(Ped ped)
        {
            foreach (var avoidAttackEntity in AvoidAttackEntities)
            {
                if (!avoidAttackEntity.IsSafeExist())
                {
                    continue;
                }

                // 対象者の近くにいる
                if (avoidAttackEntity.Position.DistanceTo(ped.Position) < 5.0f)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 武器を取り替えて良いか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueで武器を取り替えて良い</returns>
        public bool IsPedChangebalWeapon(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer
                   && (!ped.IsRequiredForMission() || IsChangeMissonCharacterWeapon);
        }

        /// <summary>
        /// 対象の市民がユニークキャラであるか
        /// </summary>
        /// <param name="pedHash">ハッシュ値</param>
        /// <returns>trueでユニークキャラ</returns>
        public bool IsUniqueCharacter(uint pedHash)
        {
            return Enum.IsDefined(typeof(UniqueCharacters), pedHash);
        }

        /// <summary>
        /// カオスモードの対象として選出して良いEntityであるかどうか
        /// </summary>
        /// <param name="entity">市民</param>
        /// <returns>trueでカオス化</returns>
        private bool IsRiotableMissionCharacter(Entity entity)
        {
            if (!entity.IsSafeExist())
            {
                return false;
            }

            //ミッションキャラじゃないならtrue
            if (!entity.IsRequiredForMission())
            {
                return true;
            }

            switch (MissionCharacterTreatment)
            {
                case MissionCharacterTreatmentType.AffectAllCharacter:
                    return true;

                case MissionCharacterTreatmentType.ExcludeUniqueCharacter:
                    return !IsUniqueCharacter((uint)entity.Model.Hash); //ユニークキャラじゃないならカオス化
                case MissionCharacterTreatmentType.ExcludeAllMissionCharacter:
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 攻撃を加えて良いEntityであるかどうか
        /// </summary>
        public bool IsAttackableEntity(Entity entity)
        {
            if (Game.Player.Character == entity)
            {
                return true;
            }

            // カオス化して良い判定と条件は今のところ同じ
            return IsRiotableMissionCharacter(entity);
        }

        private enum UniqueCharacters : uint
        {
            Michael = 0xD7114C9,
            Franklin = 0x9B22DBAF,
            Trevor = 0x9B810FA2,
            Abigail = 0x400AEC41,
            AmandaTownley = 0x6D1E15F7,
            Andreas = 0x47E4EEA0,
            Ashley = 0x7EF440DB,
            Ballasog = 0xA70B4A92,
            Bankman = 0x909D9E7F,
            Barry = 0x2F8845A3,
            Bestmen = 0x5746CD96,
            Beverly = 0xBDA21E5C,
            Brad = 0xBDBB4922,
            Bride = 0x6162EC47,
            Car3Guy1 = 0x84F9E937,
            Car3Guy2 = 0x75C34ACA,
            Casey = 0xE0FA2554,
            Chef = 0x49EADBF6,
            Clay = 0x6CCFE08A,
            Claypain = 0x9D0087A8,
            Cletus = 0xE6631195,
            CrisFormage = 0x286E54A7,
            Dale = 0x467415E9,
            DaveNorton = 0x15CD4C33,
            Denise = 0x820B33BD,
            Devin = 0x7461A0B0,
            Dom = 0x9C2DB088,
            Dreyfuss = 0xDA890932,
            DrFriedlander = 0xCBFC0DF5,
            Fabien = 0xD090C350,
            FbiSuit01 = 0x3AE4A33B,
            Floyd = 0xB1B196B2,
            Groom = 0xFECE8B85,
            Hao = 0x65978363,
            Hunter = 0xCE1324DE,
            Janet = 0xD6D9C49,
            JayNorris = 0x7A32EE74,
            Jewelass = 0xF5D26BB,
            JimmyBoston = 0xEDA0082D,
            JimmyDisanto = 0x570462B9,
            JoeMinuteman = 0xBE204C9B,
            JohnnyKlebitz = 0x87CA80AE,
            Josef = 0xE11A9FB4,
            Josh = 0x799E9EEE,
            KerryMcintosh = 0x5B3BD90D,
            LamarDavis = 0x65B93076,
            Lazlow = 0xDFE443E5,
            LesterCrest = 0x4DA6E849,
            Lifeinvad01 = 0x5389A93C,
            Lifeinvad02 = 0x27BD51D4,
            Magenta = 0xFCDC910A,
            Manuel = 0xFD418E10,
            Marnie = 0x188232D0,
            MaryAnn = 0xA36F9806,
            Maude = 0x3BE8287E,
            Michelle = 0xBF9672F4,
            Milton = 0xCB3059B2,
            Molly = 0xAF03DDE1,
            MrK = 0xEDDCAB6D,
            MrsPhillips = 0x3862EEA8,
            MrsThornhill = 0x1E04A96B,
            Natalia = 0xDE17DD3B,
            NervousRon = 0xBD006AF1,
            Nigel = 0xC8B7167D,
            OldMan1a = 0x719D27F4,
            OldMan2 = 0xEF154C47,
            Omega = 0x60E6A7D8,
            ONeil = 0x2DC6D3E7,
            Orleans = 0x61D4C771,
            Ortega = 0x26A562B7,
            Paper = 0x999B00C6,
            Patricia = 0xC56E118C,
            Priest = 0x6437E77D,
            PrologueDriver = 0x855E36A3,
            PrologueSec01 = 0x709220C7,
            PrologueSec02 = 0x27B3AD75,
            RampGang = 0xE52E126C,
            RampHic = 0x45753032,
            RampHipster = 0xDEEF9F6E,
            RampMex = 0xE6AC74A4,
            RoccoPelosi = 0xD5BA52FF,
            RussianDrunk = 0x3D0A5EB1,
            ScreenWriter = 0xFFE63677,
            SiemonYetarian = 0x4C7B2F05,
            Solomon = 0x86BDFE26,
            SteveHains = 0x382121C8,
            Stretch = 0x36984358,
            Talina = 0xE793C8E8,
            Tanisha = 0xD810489,
            TaoCheng = 0xDC5C5EA5,
            TaosTranslator = 0x7C851464,
            TennisCoach = 0xA23B5F57,
            Terry = 0x67000B94,
            TomEpsilon = 0xCD777AAA,
            Tonya = 0xCAC85344,
            TracyDisanto = 0xDE352A35,
            TrafficWarden = 0x5719786D,
            TylerDixon = 0x5265F707,
            Wade = 0x92991B72,
            WeiCheng = 0xAAE4EA7B,
            Zimbor = 0xB34D6F5,
            Abner = 0xF0AC2626,
            AlDiNapoli = 0xF0EC56E2,
            Antonb = 0xCF623A2C,
            Armoured01 = 0xCDEF5408,
            Babyd = 0xDA116E7E,
            Bankman01 = 0xC306D6F5,
            Baygor = 0x5244247D,
            BikeHire01 = 0x76474545,
            BikerChic = 0xFA389D4F,
            BurgerDrug = 0x8B7D3766,
            Chip = 0x24604B2B,
            Claude01 = 0xC0F371B7,
            ComJane = 0xB6AA85CE,
            Corpse01 = 0x2E140314,
            Corpse02 = 0xD9C72F8,
            Cyclist01 = 0x2D0EFCEB,
            DeadHooker = 0x73DEA88B,
            ExArmy01 = 0x45348DBB,
            Famdd01 = 0x33A464E5,
            FibArchitect = 0x342333D3,
            FibMugger01 = 0x85B9C668,
            FibSec01 = 0x5CDEF405,
            FilmDirector = 0x2B6E1BB6,
            Finguru01 = 0x46E39E63,
            FreemodeFemale01 = 0x9C9EFFD8,
            FreemodeMale01 = 0x705E61F2,
            Glenstank01 = 0x45BB1666,
            Griff01 = 0xC454BCBB,
            Guido01 = 0xC6B49A2F,
            GunVend01 = 0xB3229752,
            Hacker = 0x99BB00F8,
            Hippie01 = 0xF041880B,
            Hotposh01 = 0x969B6DFE,
            Imporage = 0x348065F5,
            Jesus01 = 0xCE2CB751,
            Jewelass01 = 0xF0D4BE2E,
            JewelSec01 = 0xACCCBDB6,
            JewelThief = 0xE6CC3CDC,
            Justin = 0x7DC3908F,
            Mani = 0xC8BB1E52,
            Markfost = 0x1C95CB0B,
            Marston01 = 0x38430167,
            MilitaryBum = 0x4705974A,
            Miranda = 0x414FA27B,
            Mistress = 0x5DCA2528,
            Misty01 = 0xD128FF9D,
            MovieStar = 0x35578634,
            MPros01 = 0x6C9DD7C9,
            Niko01 = 0xEEDACFC9,
            Paparazzi = 0x5048B328,
            Party01 = 0x36E70600,
            PartyTarget = 0x81F74DE7,
            PestContDriver = 0x3B474ADF,
            PestContGunman = 0xB881AEE,
            Pogo01 = 0xDC59940D,
            Poppymich = 0x23E9A09E,
            Princess = 0xD2E3A284,
            Prisoner01 = 0x7B9B4BC0,
            PrologueHostage01 = 0xC512DD23,
            PrologueMournFemale01 = 0xA20899E7,
            PrologueMournMale01 = 0xCE96030B,
            RivalPaparazzi = 0x60D5D6DA,
            ShopKeep01 = 0x18CE57D0,
            SpyActor = 0xAC0EA5D8,
            SpyActress = 0x5B81D86C,
            StripperLite = 0x2970A494,
            Taphillbilly = 0x9A1E5E52,
            Tramp01 = 0x6A8F1F9B,
            WillyFist = 0x90769A8F,
            Zombie01 = 0xAC4B4506
        }
    }
}