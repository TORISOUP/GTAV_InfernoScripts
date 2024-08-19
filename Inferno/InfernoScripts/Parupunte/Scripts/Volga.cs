using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 上空からソロモンを落として大爆発させる
    /// </summary>
    [ParupunteConfigAttribute("頭の中に爆弾が!")]
    [ParupunteIsono("ぼるが")]
    internal class Volga : ParupunteScript
    {
        public Volga(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            StartCoroutine(AirPlaneCoroutine());
        }

        /// <summary>
        /// 飛行機を召喚する
        /// </summary>
        private Tuple<Vehicle, Ped> SpawnAirPlane()
        {
            var model = new Model(VehicleHash.Velum2);
            var plane = GTA.World.CreateVehicle(model, core.PlayerPed.Position + new Vector3(0, -400, 150));
            if (!plane.IsSafeExist()) return null;
            plane.PetrolTankHealth = 100;
            plane.Speed = 50;

            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist()) return null;
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
                if (length < 80) break;

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

            if (ped.IsSafeExist()) ped.MarkAsNoLongerNeeded();
        }

        private void SetPlaneTask(Vehicle plane, Ped ped, Entity target)
        {
            var tarPos = target.Position + new Vector3(0, 0, 50);
            Function.Call(Hash.TASK_PLANE_MISSION, ped, plane, 0, 0, tarPos.X, tarPos.Y, tarPos.Z, 4, 100.0, -1.0, -1.0, 100, 100);
        }

        private IEnumerable<object> VolgaCoroutine(Vector3 createPosition, Vector3 forward)
        {
            var volga = GTA.World.CreatePed(new Model(PedHash.Solomon), createPosition);
            if (!volga.IsSafeExist())
            {
                ParupunteEnd();
                yield break;
            }

            core.DrawParupunteText("ボルガ博士！お許し下さい！", 3.0f);

            volga.Task.ClearAllImmediately();
            volga.FreezePosition = false;
            volga.Health = 30;
            volga.IsCollisionProof = false;
            volga.IsInvincible = false;
            volga.Velocity = forward;
            yield return WaitForSeconds(1);

            foreach (var w in WaitForSeconds(10))
            {
                if (!volga.IsSafeExist())
                {
                    ParupunteEnd();
                    yield break;
                }
                //着地するまで
                if (!volga.IsInAir) break;
                yield return null;
            }
            if (!volga.IsSafeExist())
            {
                ParupunteEnd();
                yield break;
            }

            //着死したら大爆発する
            volga.MarkAsNoLongerNeeded();
            GTA.World.AddExplosion(volga.Position, GTA.ExplosionType.Rocket, 10.0f, 2.0f);
            BlowOff(volga.Position);
            ParupunteEnd();
        }

        /// <summary>
        /// 指定座標を中心に周りのものをふっとばす
        /// </summary>
        private void BlowOff(Vector3 centerPos)
        {
            var peds = GTA.World.GetNearbyPeds(centerPos, 150);
            var vehicles = GTA.World.GetNearbyVehicles(centerPos, 150);

            foreach (var p in peds)
            {
                if (!p.IsSafeExist()) continue;
                if (p.IsCutsceneOnlyPed()) continue;

                var dir = p.Position - centerPos;
                var lenght = dir.Length();
                dir.Normalize();
                p.CanRagdoll = true;
                p.SetToRagdoll(100);

                float power = 50;
                if (lenght <= 30) power = 200;
                if (30 < lenght && lenght <= 70) power = 100;
                p.ApplyForce(dir * power);
            }

            foreach (var w in vehicles)
            {
                if (!w.IsSafeExist()) continue;
                var dir = w.Position - centerPos;
                var lenght = dir.Length();
                dir.Normalize();

                float power = 30;
                if (lenght <= 30) power = 70;
                if (30 < lenght && lenght <= 70) power = 50;
                w.ApplyForce(dir * power, Vector3.RandomXYZ() * 10.0f);
            }
        }
    }
}
