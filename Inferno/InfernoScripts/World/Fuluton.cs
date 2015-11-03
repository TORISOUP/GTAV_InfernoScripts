using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;


namespace Inferno
{
    internal class Fuluton : InfernoScript
    {
        protected override int TickInterval { get; } = 100;

        /// <summary>
        /// フルトン回収のコルーチン対象になっているEntity
        /// </summary>
        private HashSet<int> fulutonedEntityList = new HashSet<int>();

        private Queue<PedHash> motherBasePeds = new Queue<PedHash>(30);
        private Queue<GTA.Native.VehicleHash> motherbaseVeh = new Queue<GTA.Native.VehicleHash>(30);
        private Random random = new Random();
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("fuluton")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Fuluton:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);


            OnKeyDownAsObservable
                .Where(x =>IsActive && x.KeyCode == Keys.F9 && motherbaseVeh.Count > 0)
                .Subscribe(_ => SpawnVehicle());

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F10 && motherBasePeds.Count > 0)
                .Subscribe(_ => SpawnCitizen());

            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ => FulutonUpdate());

            //プレイヤが死んだらリストクリア
            OnTickAsObservable
                .Select(_ => playerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => fulutonedEntityList.Clear());

            OnTickAsObservable.Subscribe(_ => {
                                                  Game.Player.WantedLevel = 0;
                                                 
                                                  playerPed.IsInvincible = true;

            });


        }

        #region 回収

        private void FulutonUpdate()
        {
            foreach (var entity in CachedPeds.Concat(CachedVehicles.Cast<Entity>()).Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(playerPed.Position, 5.0f)
                     && !fulutonedEntityList.Contains(x.Handle)
                ))
            {
                if (entity.HasBeenDamagedByPed(playerPed) &&(
                    entity.HasBeenDamagedBy(Weapon.UNARMED)
                    ))
                {
                    fulutonedEntityList.Add(entity.Handle);
                    StartCoroutine(FulutonCoroutine(entity));
                }
            }
        }

        private void LeaveAllPedsFromVehicle(Vehicle vec)
        {
            if (!vec.IsSafeExist()) return;

            foreach (
                var seat in
                    new[] {VehicleSeat.Driver, VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear})
            {
                var ped = vec.GetPedOnSeat(seat);
                if (ped.IsSafeExist())
                {
                    ped.Task.ClearAll();
                    ped.Task.ClearSecondary();
                    ped.Task.LeaveVehicle();
                }
            }
        }

        private IEnumerable<object> FulutonCoroutine(Entity entity)
        {
            //Entityが消え去った後に処理したいので先に情報を保存しておく
            int hash = -1;
            bool isPed = false;

            var upForce = new Vector3(0, 0, 1);
            if (entity is Ped)
            {
                var p = entity as Ped;
                p.CanRagdoll = true;
                p.SetToRagdoll(10*1000);

                isPed = true;
            }
            else
            {
                var v = entity as Vehicle;
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, v, 1);
                LeaveAllPedsFromVehicle(v);
            }
            hash = entity.Model.Hash;

            entity.ApplyForce(upForce * 2.0f);

            foreach (var s in WaitForSeconds(3))
            {
                if (!entity.IsSafeExist() || entity.IsDead) yield break;

                entity.ApplyForce(upForce*1.07f);

                yield return s;
            }

            if (!entity.IsSafeExist() || entity.IsRequiredForMission())
            {
                fulutonedEntityList.Remove(entity.Handle);
                yield break;
            }

            if (playerPed.CurrentVehicle.IsSafeExist() && playerPed.CurrentVehicle.Handle == entity.Handle)
            {
                yield break;
            }

            //弾みをつける
            yield return WaitForSeconds(0.25f);

            foreach (var s in WaitForSeconds(7))
            {
                if (!entity.IsSafeExist())
                {
                    if (playerPed.CurrentVehicle.IsSafeExist() && playerPed.CurrentVehicle.Handle == entity.Handle)
                    {
                        yield break;
                    }

                    if (isPed)
                    {
                        motherBasePeds.Enqueue((PedHash) hash);
                        Game.Player.Money -= 100;
                    }
                    else
                    {
                        motherbaseVeh.Enqueue((GTA.Native.VehicleHash) hash);
                        Game.Player.Money -= 1000;
                    }
                    DrawText("回収完了", 3.0f);
                    yield break;
                }

                if (entity.IsDead) yield break;

                entity.ApplyForce(upForce*1.0f/Game.FPS*500.0f);

                yield return s;
            }
        }

        #endregion

        #region 生成

        private void SpawnCitizen()
        {
            var hash = motherBasePeds.Dequeue();

            var p = World.CreatePed(new Model(hash), playerPed.Position.AroundRandom2D(3.0f) + new Vector3(0, 0, 0.5f));
            if (!p.IsSafeExist()) return;
            var weapon = Enum.GetValues(typeof (WeaponHash))
                .Cast<WeaponHash>()
                .OrderBy(c => random.Next())
                .FirstOrDefault();

            var weaponhash = (int) weapon;
            p.MarkAsNoLongerNeeded();

            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, p, Game.Player.GetPlayerGroup());
            p.SetDropWeaponWhenDead(false); //武器を落とさない
            p.GiveWeapon(weaponhash, 1000); //指定武器所持
            p.EquipWeapon(weaponhash); //武器装備
            p.Health = 1;
            p.Task.FightAgainstHatedTargets(50, 0);
            var blip = p.AddBlip();
            blip.Color = BlipColor.White;
            
        }

        private void SpawnVehicle()
        {
            var hash = motherbaseVeh.Dequeue();
            DrawText(hash.ToString(), 3.0f);
            StartCoroutine(SpawnVehicleCoroutine(new Model(hash), playerPed.Position.AroundRandom2D(20)));
        }

        private IEnumerable<object> SpawnVehicleCoroutine(Model model, Vector3 targetPosition)
        {
            var car = World.CreateVehicle(model, targetPosition + new Vector3(0, 0, 20));
            if (!car.IsSafeExist()) yield break;
            var upVector = new Vector3(0, 0, 1.0f);
            car.FreezePosition = false;
            car.Velocity = new Vector3();
            World.AddExplosion(targetPosition, GTA.ExplosionType.Flare, 1.0f, 0.0f);

            foreach (var s in WaitForSeconds(10))
            {
                if (!car.IsSafeExist()) yield break;
                car.ApplyForce(upVector);
                if (!car.IsInAir) break;
                yield return null;
            }

            if (!car.IsSafeExist()) yield break;
            car.MarkAsNoLongerNeeded();
        }

        #endregion
    }
}
