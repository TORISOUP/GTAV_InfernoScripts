using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("てきとうにしょうかん")]
    internal class SpawnCharacters : ParupunteScript
    {
        private string name;
        private Model pedModel;
        private Random random;

        public SpawnCharacters(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnSetUp()
        {
            random = new Random();

            switch (random.Next(0, 100) % 11)
            {
                case 0:
                    pedModel = new Model(PedHash.LamarDavis);
                    name = "ジャマーデイビス";
                    break;

                case 1:
                    pedModel = new Model(PedHash.LesterCrest);
                    name = "レレレのレ～";
                    break;

                case 2:
                    pedModel = new Model(PedHash.Fabien);
                    name = "誰かヨガって言いました？";
                    break;

                case 3:
                    pedModel = new Model(PedHash.Lazlow);
                    name = "フェイムオアシェイム";
                    break;

                case 4:
                    pedModel = new Model(PedHash.CrisFormage);
                    name = "キフロム！";
                    break;

                case 5:
                    pedModel = new Model(PedHash.Chimp);
                    name = "猿の惑星";
                    break;

                case 6:
                    pedModel = new Model(PedHash.Zombie01);
                    name = "ウォーキング・デッド";
                    break;

                case 7:
                    pedModel = new Model(PedHash.RsRanger01AMO);
                    name = "それは外宇宙からやってきた";
                    break;

                case 8:
                    pedModel = new Model(PedHash.Imporage);
                    name = "インポマン";
                    break;

                case 9:
                    pedModel = new Model(PedHash.MovAlien01);
                    name = "ロスサントス決戦";
                    break;

                case 10:
                    pedModel = new Model(PedHash.Clown01SMY);
                    name = "らんらんる～";
                    break;
            }
        }

        public override void OnStart()
        {
            SpawnCharacterAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask SpawnCharacterAsync(CancellationToken ct)
        {
            var player = core.PlayerPed;
            var isInVehicle = player.IsInVehicle();
            var pv = player.CurrentVehicle;
            var isSeatFull = false;

            // Vehicle seatをすべて列挙
            var seats = (VehicleSeat[])Enum.GetValues(typeof(VehicleSeat));

            for (int i = 0; i < 20; i++)
            {
                Ped ped = null;

                if (isInVehicle && pv.IsSafeExist() && !isSeatFull)
                {
                    for (int s = 0; s < seats.Length; i++)
                    {
                        var seat = seats[i];
                        if (pv.IsSeatFree(seat))
                        {
                            ped = pv.CreatePedOnSeat(seat, pedModel);
                            break;
                        }
                    }

                    isSeatFull = true;
                }
                else
                {
                    ped = GTA.World.CreatePed(pedModel, player.Position.AroundRandom2D(15));
                }
                

                if (ped.IsSafeExist())
                {
                    ped.MarkAsNoLongerNeeded();
                    GiveWeaponTpPed(ped);
                }

                await Delay100MsAsync(ct);
            }

            ParupunteEnd();
        }

        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private void GiveWeaponTpPed(Ped ped)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            //車に乗っているなら車用の武器を渡す
            var weapon = Enum.GetValues(typeof(WeaponHash))
                .Cast<WeaponHash>()
                .OrderBy(c => random.Next())
                .FirstOrDefault();

            var weaponhash = (int)weapon;

            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.GiveWeapon(weaponhash, 1000); //指定武器所持
            ped.EquipWeapon(weaponhash); //武器装備
            ped.Task.FightAgainst(core.PlayerPed);
        }
    }
}