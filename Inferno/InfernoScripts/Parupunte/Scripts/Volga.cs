using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 上空からソロモンを落として大爆発させる
    /// </summary>
    [ParupunteConfigAttribute("ボルガ博士！お許し下さい!")]
    [ParupunteIsono("ぼるが")]
    internal class Volga : ParupunteScript
    {
        public Volga(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        private Blip _blip;

        public override void OnStart()
        {
            var playerPos = core.PlayerPed.Position;
            var around = playerPos.Around(70);
            var spawnPoint = around + Vector3.WorldUp * 200f;

            // 簡略化
            VolgaAsync(spawnPoint, Vector3.Zero, ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            if (_blip.Exists())
            {
                _blip.Delete();
            }
        }

        /*
        /// <summary>
        /// 飛行機を召喚する
        /// </summary>
        private Tuple<Vehicle, Ped> SpawnAirPlane()
        {
            var model = new Model(VehicleHash.Velum2);
            var plane = GTA.World.CreateVehicle(model, core.PlayerPed.Position + new Vector3(0, -400, 150));
            if (!plane.IsSafeExist())
            {
                return null;
            }

            plane.PetrolTankHealth = 100;
            plane.SetForwardSpeed(500);

            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist())
            {
                return null;
            }

            ped.SetNotChaosPed(true);

            return new Tuple<Vehicle, Ped>(plane, ped);
        }

        /// <summary>
        /// 飛行機をプレイヤに向けて飛ばす
        /// </summary>
        private IEnumerable<object> AirPlaneCoroutine()
        {
            var tuple = SpawnAirPlane();
            if (tuple == null)
            {
                ParupunteEnd();
                yield break;
            }

            var ped = tuple.Item2;
            var plane = tuple.Item1;

            if (!ped.IsSafeExist() || !plane.IsSafeExist())
            {
                ParupunteEnd();
                yield break;
            }

            //20秒間運転したら自動的に投棄
            foreach (var x in WaitForSeconds(20))
            {
                if (!ped.IsSafeExist() || !plane.IsSafeExist() || plane.IsDead)
                {
                    ParupunteEnd();
                    yield break;
                }

                SetPlaneTask(plane, ped, core.PlayerPed);
                var delta = plane.Position - core.PlayerPed.Position;
                delta.Z = 0;
                var length = delta.Length();

                //プレイヤとの平面上の距離が80m以内になったら投棄
                if (length < 80)
                {
                    break;
                }

                yield return null;
            }

            if (plane.IsSafeExist())
            {
                plane.MarkAsNoLongerNeeded();
                plane.Health = -1;
                plane.EngineHealth = 0;

                //ボルガ博士投棄
                StartCoroutine(VolgaCoroutine(plane.Position + new Vector3(0, 0, -2), plane.Velocity));
            }

            if (ped.IsSafeExist())
            {
                ped.MarkAsNoLongerNeeded();
            }
        }

        private void SetPlaneTask(Vehicle plane, Ped ped, Entity target)
        {
            var tarPos = target.Position + new Vector3(0, 0, 50);
            Function.Call(Hash.TASK_PLANE_MISSION, ped, plane, 0, 0, tarPos.X, tarPos.Y, tarPos.Z, 4, 100.0, -1.0, -1.0,
                100, 100);
        }

        */

        private async ValueTask VolgaAsync(Vector3 createPosition, Vector3 forward, CancellationToken ct)
        {
            var volga = GTA.World.CreatePed(new Model(PedHash.Solomon), createPosition);
            if (!volga.IsSafeExist())
            {
                ParupunteEnd();
                return;
            }


            volga.Task.ClearAllImmediately();
            volga.FreezePosition(false);
            volga.Health = 100;
            volga.IsCollisionProof = false;
            volga.IsInvincible = false;
            volga.Velocity = forward;
            volga.IsPersistent = true;
            _blip = volga.AddBlip();
            if (_blip.Exists())
            {
                _blip.Color = BlipColor.Yellow2;
            }

            AutoReleaseOnParupunteEnd(volga);
            await DelaySecondsAsync(1, ct);

            var targetTime = core.ElapsedTime + 10;
            while (core.ElapsedTime < targetTime && !ct.IsCancellationRequested)
            {
                if (!volga.IsSafeExist())
                {
                    ParupunteEnd();
                    return;
                }

                //着地するまで
                if (!volga.IsInAir)
                {
                    break;
                }

                await YieldAsync(ct);
            }

            if (!volga.IsSafeExist())
            {
                ParupunteEnd();
                return;
            }

            //着死したら大爆発する
            volga.MarkAsNoLongerNeeded();
            GTA.World.AddExplosion(volga.Position, GTA.ExplosionType.Rocket, 10.0f, 2.0f);
            GTA.World.AddExplosion(volga.Position, GTA.ExplosionType.RayGun, 10.0f, 2.0f);

            BlowOff(volga.Position);

            ParupunteEnd();
        }

        /// <summary>
        /// 指定座標を中心に周りのものをふっとばす
        /// </summary>
        private void BlowOff(Vector3 centerPos)
        {
            var peds = core.CachedPeds.Concat(new[] { core.PlayerPed });
            var vehicles = core.CachedVehicles;

            foreach (var p in peds)
            {
                if (!p.IsSafeExist())
                {
                    continue;
                }

                if (p.IsCutsceneOnlyPed())
                {
                    continue;
                }

                if (!p.IsInRangeOf(centerPos, 150))
                {
                    continue;
                }

                var dir = p.Position + Vector3.WorldUp * 10 - centerPos;
                var lenght = dir.Length();
                dir.Normalize();
                p.CanRagdoll = true;
                p.SetToRagdoll();

                float power = 50;
                if (lenght <= 30)
                {
                    power = 200;
                }

                if (30 < lenght && lenght <= 70)
                {
                    power = 100;
                }

                p.ApplyForce(dir * power);
            }

            foreach (var w in vehicles)
            {
                if (!w.IsSafeExist())
                {
                    continue;
                }

                if (!w.IsInRangeOf(centerPos, 150))
                {
                    continue;
                }


                var dir = w.Position + Vector3.WorldUp * 15 - centerPos;
                var lenght = dir.Length();
                dir.Normalize();

                float power = 30;
                if (lenght <= 30)
                {
                    power = 70;
                }

                if (30 < lenght && lenght <= 70)
                {
                    power = 50;
                }

                w.ApplyForce(dir * power, Vector3.RandomXYZ() * 10.0f);
            }
        }
    }
}