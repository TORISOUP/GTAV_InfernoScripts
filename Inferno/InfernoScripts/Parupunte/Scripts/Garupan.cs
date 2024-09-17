using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("がるぱん")]
    internal class Garupan : ParupunteScript
    {
        private readonly List<Entity> resources = new();
        private string _name;
        private PlanType planType;

        public Garupan(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            planType = (PlanType)Random.Next(0, Enum.GetValues(typeof(PlanType)).Length);

            switch (planType)
            {
                case PlanType.Kosokoso:
                    _name = "こそこそ作戦、開始です！";
                    break;
                case PlanType.Kottsun:
                    _name = "こっつん作戦、開始です！";
                    break;
            }
        }

        public override void OnSetNames()
        {
            Name = _name;
            SubName = "ガ◯パンは、いいぞ";
            EndMessage = () =>
            {
                if (core.PlayerPed.IsAlive)
                {
                    return "おしまい";
                }

                EndMessageDisplayTime = 4.0f;
                return "フラッグ車走行不能！";
            };
        }

        public override void OnStart()
        {
            switch (planType)
            {
                case PlanType.Kosokoso:
                    StartKosokoso();
                    break;
                case PlanType.Kottsun:
                    KottsunStart();
                    break;
            }

            //プレイヤが死んだら終了
            OnUpdateAsObservable
                .Where(_ => !core.PlayerPed.IsAlive)
                .Take(1)
                .Subscribe(_ => ParupunteEnd());

            OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    foreach (var t in resources)
                    {
                        if (t.IsSafeExist())
                        {
                            t.MarkAsNoLongerNeeded();
                            if (t is Ped p)
                            {
                                p.Kill();
                            }
                            else if (t is Vehicle v)
                            {
                                v.Explode();
                                var b = v.AttachedBlip;
                                if (b != null)
                                {
                                    b.Delete();
                                }
                            }
                        }
                    }
                });
        }

        #region こそこそ作戦

        private void StartKosokoso()
        {
            ReduceCounter = new ReduceCounter(25 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            //戦車を遠目にたくさん配置して、遭遇したら攻撃してくる
            foreach (var _ in Enumerable.Range(0, 12))
            {
                //遠目につくる
                var (tank, ped) = SpawnTank(Random.Next(300, 500));
                if (!tank.IsSafeExist() || !ped.IsSafeExist())
                {
                    continue;
                }

                ped.Task.FightAgainst(core.PlayerPed);
                // 車で攻撃するか
                ped.SetCombatAttributes(52, true);
                // 車両の武器を使用するか
                ped.SetCombatAttributes(53, true);
                resources.Add(tank);
                resources.Add(ped);
                tank.EnginePowerMultiplier = 20.0f;
                tank.EngineTorqueMultiplier = 20.0f;
                tank.MaxSpeed = 300;
            }
        }

        #endregion

        private (Vehicle, Ped) SpawnTank(float range)
        {
            var pos = GTA.World.GetNextPositionOnStreet(core.PlayerPed.Position.Around(range));
            return SpawnTank(pos);
        }

        private (Vehicle, Ped) SpawnTank(Vector3 position)
        {
            var model = new Model(VehicleHash.Rhino);
            //戦車生成
            var tank = GTA.World.CreateVehicle(model, position);
            if (!tank.IsSafeExist())
            {
                return (null, null);
            }

            var b = tank.AddBlip();
            b.Color = BlipColor.Pink;

            //乗員召喚
            var ped = tank.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.Tonya));
            if (!ped.IsSafeExist())
            {
                return (null, null);
            }

            ped.SetNotChaosPed(true);

            //自動開放
            AutoReleaseOnGameEnd(tank);
            AutoReleaseOnGameEnd(ped);

            return new(tank, ped);
        }

        private enum PlanType
        {
            Kosokoso,
            Kottsun
        }

        #region こっつん作戦

        private void KottsunStart()
        {
            //ニトロで挟んで圧死させる
            ReduceCounter = new ReduceCounter(6 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            KottsunTankAsync(true, ActiveCancellationToken).Forget();
            KottsunTankAsync(false, ActiveCancellationToken).Forget();
        }

        //戦車を生成して数秒後に突進させる
        private async ValueTask KottsunTankAsync(bool isForward, CancellationToken ct)
        {
            //遠目につくる
            var ppos = core.PlayerPed.Position;
            var (tank, ped) = SpawnTank(ppos + new Vector3(0, isForward ? -30 : 30, 0));
            if (ped == null || tank == null)
            {
                ParupunteEnd();
            }

            tank.EnginePowerMultiplier = 20.0f;
            tank.EngineTorqueMultiplier = 20.0f;
            tank.MaxSpeed = 300;
            resources.Add(tank);
            resources.Add(ped);

            await DelaySecondsAsync(3, ct);
            if (!tank.IsSafeExist())
            {
                return;
            }

            //演出用
            Function.Call(Hash.ADD_EXPLOSION, tank.Position.X, tank.Position.Y, tank.Position.Z, -1, 0.0f, true, false,
                0.1f);
            tank.SetForwardSpeed(isForward ? 100 : -100);

            await DelaySecondsAsync(2, ct);
            if (ped.IsSafeExist())
            {
                // 車で攻撃するか
                ped.SetCombatAttributes(52, true);
                // 車両の武器を使用するか
                ped.SetCombatAttributes(53, true);
                ped.Task.FightAgainst(core.PlayerPed);
            }
        }

        #endregion
    }
}