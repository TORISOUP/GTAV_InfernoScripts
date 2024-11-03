using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("大量車両ハッキング")]
    [ParupunteIsono("はっきんぐ")]
    internal class MassiveVehicleHack : ParupunteScript
    {
        //演出用の線を引くリスト
        private readonly List<(Entity, Entity)> drawLineList = new();

        //ハック済み車両
        private List<Vehicle> _hacksList = new();

        public MassiveVehicleHack(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(3000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                _hacksList = null;
                ParupunteEnd();
            });

            DrawLineLoopAsync(ActiveCancellationToken).Forget();

            if (core.PlayerPed.IsInVehicle())
            {
                _hacksList.Add(core.PlayerPed.CurrentVehicle);
            }

            RootAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask RootAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                HackAsync(core.PlayerPed, ct).Forget();
                await DelaySecondsAsync(1, ct);
            }
        }

        private async ValueTask DrawLineLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                foreach (var tuple in drawLineList)
                {
                    var t1 = tuple.Item1;
                    var t2 = tuple.Item2;
                    if (!t1.IsSafeExist() || !t2.IsSafeExist())
                    {
                        continue;
                    }

                    DrawLine(t1.Position, t2.Position, Color.White);
                }

                await YieldAsync(ct);
            }
        }

        private async ValueTask HackAsync(Entity root, CancellationToken ct)
        {
            await DelayRandomFrameAsync(1, 5, ct);

            if (!root.IsSafeExist())
            {
                return;
            }

            //プレイヤから離れすぎてたら対象外
            if (!root.IsInRangeOf(core.PlayerPed.Position, 100))
            {
                return;
            }

            //ターゲットを探す
            var target = core.CachedVehicles
                .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(root.Position, 25))
                .Except(_hacksList)
                .OrderBy(x => x.Position.DistanceTo(root.Position))
                .FirstOrDefault();

            if (target == null)
            {
                return;
            }

            _hacksList.Add(target);

            //追加
            drawLineList.Add((root, target));
            ControlAsync(target, ct).Forget();

            //伝播させる
            for (var i = 0; i < 10; i++)
            {
                HackAsync(target, ct).Forget();
                await DelaySecondsAsync(0.5f, ct);
            }
        }

        //車両を暴走させるコルーチン
        private async ValueTask ControlAsync(Vehicle target, CancellationToken ct)
        {
            var isBack = Random.Next(0, 100) < 30;
            
            target.ForwardSpeed = isBack ? -20 : 20;

            
            while (target.IsSafeExist() && target.IsAlive)
            {
                target.CanWheelsBreak = false;
                target.IsHandbrakeForcedOn = false;
                target.IsEngineRunning = true;
                if (target.IsOnAllWheels)
                {
                    target.ThrottlePower = 100.0f;
                    target.BrakePower = 0;
                }

                await YieldAsync(ct);
            }
        }

        private void DrawLine(Vector3 from, Vector3 to, Color col)
        {
            Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, col.R, col.G, col.B, col.A);
        }
    }
}