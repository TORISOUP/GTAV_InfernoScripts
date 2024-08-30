using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ctOS 停電", "ctOS 復旧")]
    [ParupunteIsono("ていでん")]
    [ParupunteDebug(true)]
    internal class BlackOut : ParupunteScript
    {
        private IDisposable drawingDisposable;
        private SoundPlayer soundPlayerEnd;
        private SoundPlayer soundPlayerStart;

        public BlackOut(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync
                .Subscribe(_ => BlackOutEndAsync(ActiveCancellationToken).Forget());
            
            BlackOutStartAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnUpdate()
        {
            //周辺車両をエンストさせる
            if (!core.PlayerPed.IsSafeExist()) return;

            var playerPos = core.PlayerPed.Position;
            var playerVehicle = core.GetPlayerVehicle();
            foreach (var v in core.CachedVehicles.Where(
                         x => x.IsSafeExist()
                              && x.IsInRangeOf(playerPos, 1000)
                              && x.IsAlive
                              && x != playerVehicle))
            {
                if (!v.IsEngineRunning)
                {
                    continue;
                }

                v.IsEngineRunning = false;
                v.EnginePowerMultiplier = 0.0f;
                v.EngineTorqueMultiplier = 0.0f;
            }
        }

        protected override void OnFinished()
        {
            SetArtificialLights(false);
            soundPlayerStart = null;
            soundPlayerEnd = null;
            drawingDisposable?.Dispose();
        }

        private async ValueTask BlackOutStartAsync(CancellationToken ct)
        {
            DrawBlackOutLineAsync(ct).Forget();

            //効果音に合わせてチカチカさせる
            soundPlayerStart?.Play();
            await DelaySecondsAsync(1.5f, ct);
            var current = false;
            for (var i = 0; i < 10; i++)
            {
                SetArtificialLights(current);
                if (Random.Next(0, 2) % 2 == 0)
                {
                    current = !current;
                }

                await Delay100MsAsync(ct);
            }

            SetArtificialLights(true);
        }

        private async ValueTask BlackOutEndAsync(CancellationToken ct)
        {
            soundPlayerEnd?.Play();
            await DelaySecondsAsync(1, ct);
            ParupunteEnd();
        }

        private IEnumerable<Vector3> GetAroundObjectPosition(Vector3 root, float distance, int n)
        {
            return core.CachedPeds
                .Concat<Entity>(core.CachedVehicles)
                .Concat(GTA.World.GetAllProps())
                .Where(x => x.IsSafeExist() && x.IsInRangeOf(root, distance))
                .Select(x => x.Position)
                .OrderBy(_ => Guid.NewGuid())
                .Take(n)
                .ToArray();
        }

        private async ValueTask DrawBlackOutLineAsync(CancellationToken ct)
        {
            var targets = GetAroundObjectPosition(core.PlayerPed.Position, 50, 15);

            drawingDisposable = core.OnDrawingTickAsObservable
                .TakeUntil(OnFinishedAsObservable)
                .Subscribe(_ =>
                {
                    var p = core.PlayerPed.Position;
                    foreach (var t in targets)
                    {
                        DrawLine(p, t, Color.White);
                    }
                });

            for (var i = 0; i < 10; i++)
            {
                await DelaySecondsAsync(0.35f, ct);
                targets = GetAroundObjectPosition(core.PlayerPed.Position, 50, 15);
            }

            drawingDisposable?.Dispose();
        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("blackout_start.wav"));
            if (setupWav != null)
            {
                soundPlayerStart = new SoundPlayer(setupWav);
            }

            setupWav = filePaths.FirstOrDefault(x => x.Contains("blackout_end.wav"));
            if (setupWav != null)
            {
                soundPlayerEnd = new SoundPlayer(setupWav);
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

        private void DrawLine(Vector3 from, Vector3 to, Color col)
        {
            Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, col.R, col.G, col.B, col.A);
        }

        private static void SetArtificialLights(bool isOn)
        {
            Function.Call(Hash.SET_ARTIFICIAL_LIGHTS_STATE, isOn);
        }
    }
}