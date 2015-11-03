using GTA.Native;

namespace Inferno
{
    public enum GameKey
    {
        EnterCar = 23,
        Sprint = 21,
        Jump = 22,
        Reload = 45,
        SeekCover = 44,
        Attack = 24,
        ChangeWeapon = 37,
        Aim = 25,
        LookBehind = 26,
        Stealth = 36,
        Cover = 44,
        VehicleAim = 68,
        VehicleAttack = 69,
        VehicleAccelerate = 71,
        VehicleBrake = 72,
        VehicleDuck = 73,
        VehicleExit = 75,
        VehicleHandbrake = 76,
        VehicleHorn = 86,
        VehicleLookBehind = 79,
        LX = 30,
        LY = 31,
        LStickUp = 32,
        LStickDown = 33,
        LStickLeft = 34,
        LStickRight = 35,
        RX = 1,
        RY = 2,
        RStickUp = 3,
        RStickDown = 4,
        RStickLeft = 5,
        RStickRight = 6
        
    };

    public enum FiringPattern
    {
        FullAuto = -957453492,
        BurstFire = 1073727030,
        BurstInCover = 40051185,
        BurstFireDriveby = -753768974,
        FromGround = 577037782,
        DelayFireByOneSec = 2055493265,
        SingleShot = 1566631136,
        BurstFirePistol = -1608983670,
        BurstFireSMG = 1863348768,
        BurstFireRifle = -1670073338,
        BurstFireMG = -1250703948,
        BurstFirePumpShotGun = 12239771,
        BurstFireHeli = -1857128337,
        BurstFireMicro = 1122960381,
        BurstFireBursts = 445831135,
        BurstFireTank = -490063247
    }

    public enum Weapon
    {
        UNARMED = -1569615261,
        ANIMAL = -100946242,
        COUGAR = 148160082,
        KNIFE = -1716189206,
        NIGHTSTICK = 1737195953,
        HAMMER = 1317494643,
        BAT = -1786099057,
        GOLFCLUB = 1141786504,
        CROWBAR = -2067956739,
        PISTOL = 453432689,
        COMBATPISTOL = 1593441988,
        APPISTOL = 584646201,
        PISTOL50 = -1716589765,
        MICROSMG = 324215364,
        SMG = 736523883,
        ASSAULTSMG = -270015777,
        ASSAULTRIFLE = -1074790547,
        CARBINERIFLE = -2084633992,
        ADVANCEDRIFLE = -1357824103,
        MG = -1660422300,
        COMBATMG = 2144741730,
        PUMPSHOTGUN = 487013001,
        SAWNOFFSHOTGUN = 2017895192,
        ASSAULTSHOTGUN = -494615257,
        BULLPUPSHOTGUN = -1654528753,
        BULLPURIFLE = 2132975508,
        STUNGUN = 911657153,
        SNIPERRIFLE = 100416529,
        HEAVYSNIPER = 205991906,
        HEAVYPISTOL = -771403250,
        HEAVYSHOTGUN= 984333226,
        REMOTESNIPER = 856002082,
        GRENADELAUNCHER = -1568386805,
        GRENADELAUNCHER_SMOKE = 1305664598,
        RPG = -1312131151,
        PASSENGER_ROCKET = 375527679,
        AIRSTRIKE_ROCKET = 324506233,
        STINGER = 1752584910,
        MINIGUN = 1119849093,
        GRENADE = -1813897027,
        STICKYBOMB = 741814745,
        SMOKEGRENADE = -37975472,
        BZGAS = -1600701090,
        MOLOTOV = 615608432,
        FIREEXTINGUISHER = 101631238,
        FIREWORK = 2138347493,
        PROXIMITYMINE = -1420407917,
        PETROLCAN = 883325847,
        DIGISCANNER = -38085395,
        BRIEFCASE = -2000187721,
        BRIEFCASE_02 = 28811031,
        BALL = 600439132,
        FLARE = 1233104067,
        VEHICLE_ROCKET = -1090665087,
        BARBED_WIRE = 1223143800,
        DROWNING = -10959621,
        DROWNING_IN_VEHICLE = 1936677264,
        BLEEDING = -1955384325,
        ELECTRIC_FENCE = -1833087301,
        EXPLOSION = 539292904,
        FALL = -842959696,
        EXHAUSTION = 910830060,
        HIT_BY_WATER_CANNON = -868994466,
        RAMMED_BY_CAR = 133987706,
        RUN_OVER_BY_CAR = -1553120962,
        MOW_OVER_BY_AIRCRAFT = -1323279794,
        HELI_CRASH = 341774354,
        FIRE = -544306709,
        MUSCKET= -1466123874,
        MARKSMANRIFLE = -952879014,
        RAILGUN = 1834241177
    }

    public enum VehicleWeapon
    {
        ROTORS = -1323279794,
        TANK = 1945616459,
        SPACE_ROCKET = -123497569,
        PLANE_ROCKET = -821520672,
        PLAYER_LASER = -268631733,
        PLAYER_BULLET = 1259576109,
        PLAYER_BUZZARD = 1186503822,
        PLAYER_HUNTER = -1625648674,
        ENEMY_LASER = 1566990507,
        SEARCHLIGHT = -844344963,
        RADAR = -764006018,

    }

    public enum ExplosionType
    {
        GRENADE = -1236251694,
        GRENADELAUNCHER = 286056380,
        STICKYBOMB = -937058049,
        MOLOTOV = -61198893,
        ROCKET = 798856618,
        TANKSHELL = 1693512364,
        HI_OCTANE = 1436779599,
        CAR = 1768518260,
        PLANE = 428159217,
        PETROL_PUMP = 1114654932,
        BIKE = -411549476,
        DIR_STEAM = -1628533868,
        DIR_FLAME = 527211813,
        DIR_WATER_HYDRANT = 352593635,
        DIR_GAS_CANISTER = 845770333,
        BOAT = 1762719600,
        SHIP_DESTROY = 1907543711,
        TRUCK = 1115750597,
        BULLET = -1696146015,
        SMOKEGRENADELAUNCHER = -1014218325,
        SMOKEGRENADE = -1832600771,
        BZGAS = -515713583,
        FLARE = -1330848211,
    }
 

    /// <summary>
    /// ミッションキャラの扱い
    /// </summary>
    public enum MissionCharacterTreatmentType
    {
        AffectAllCharacter = 0,
        ExcludeUniqueCharacter = 1,
        ExcludeAllMissionCharacter = 2
    }

    public enum PedTaskAction
    {
        FALL_WITH_PARACHUTE = 334,
    }

}
