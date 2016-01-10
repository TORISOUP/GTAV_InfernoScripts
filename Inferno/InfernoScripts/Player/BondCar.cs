using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using UniRx;

namespace Inferno.InfernoScripts.Player
{
    class BondCar : InfernoScript
    {
        protected override int TickInterval { get; } = 50;

        protected override void Setup()
        {
            

            this.OnTickAsObservable
                .Where(_ => this.IsGamePadPressed(GameKey.VehicleHorn))
                .Where(_ => PlayerPed.CurrentVehicle.IsSafeExist())
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    
                    Shoot(PlayerPed.CurrentVehicle);
                });

        }

        private void Shoot(Vehicle v)
        {
            StartCoroutine(PedShoot(
                v,
                v.Position + Vector3.WorldUp*3,
                v.Position + v.ForwardVector * 30));
        }

        private IEnumerable<object> PedShoot(Vehicle veh,Vector3 pos, Vector3 targetPosition)
        {
            var ped = GTA.World.CreateRandomPed(pos);
            if (!ped.IsSafeExist()) yield break;
            DrawText("できた");
            var weapon = (int)Weapon.RPG;
            ped.AttachTo(veh, 0, Vector3.WorldUp * 2, Vector3.Zero);
            ped.Task.ClearAllImmediately();
            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.SetNotChaosPed(true);
            ped.GiveWeapon(weapon, 1); //指定武器所持
            ped.EquipWeapon(weapon); //武器装備
          //  ped.IsVisible = false;
            ped.FreezePosition = true;
            
            ped.SetPedFiringPattern((int)FiringPattern.SingleShot);
            ped.TaskShootAtCoord(targetPosition,-1);
            yield return null;
            if (ped.IsSafeExist())
            {
                ped.Detach();
                ped.MarkAsNoLongerNeeded();
              //  ped.Position = Vector3.Zero;
            }
        }
    }
}
