using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Stealth = 36
    };

    public enum VehicleSeat
    {
        Driver = -1,
        Passenger = 0,
        LeftRear = 1,
        RightRear = 2
    }

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

}
