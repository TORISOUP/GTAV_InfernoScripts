using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("ていでん")]
    class BlackOut : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;
        private SoundPlayer soundPlayerEnd;

        private IDisposable drawingDisposable;

        public BlackOut(ParupunteCore core) : base(core)
        {
            SetUpSound();
        }

        public override string Name { get; } = "ctOS 停電";
        public override string EndMessage { get; } = "ctOS 復旧";

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                StartCoroutine(BlackOutEnd());
            });

            StartCoroutine(BlackOutStart());
            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    GTA.World.SetBlackout(false);
                    soundPlayerStart = null;
                    soundPlayerEnd = null;
                    drawingDisposable?.Dispose();
                });

            //周辺車両をエンストさせる
            this.OnUpdateAsObservable
                .Subscribe(_ =>
                {
                    var playerPos = core.PlayerPed.Position;
                    var playerVehicle = core.GetPlayerVehicle();
                    foreach (var v in core.CachedVehicles.Where(
                        x => x.IsSafeExist()
                        && x.IsInRangeOf(playerPos, 1000)
                        && x.IsAlive
                        && x != playerVehicle))
                    {
                        v.EngineRunning = false;
                        v.EnginePowerMultiplier = 0.0f;
                        v.EngineTorqueMultiplier = 0.0f;

                    }
                });
        }


        private IEnumerable<object> BlackOutStart()
        {
            StartCoroutine(DrawBlackOutLine());

            //効果音に合わせてチカチカさせる
            soundPlayerStart?.Play();
            yield return WaitForSeconds(1.5f);
            var current = false;
            for (var i = 0; i < 10; i++)
            {
                GTA.World.SetBlackout(current);
                if (Random.Next(0, 2) % 2 == 0)
                {
                    current = !current;
                }
                yield return null;
            }
            GTA.World.SetBlackout(true);
        }

        private IEnumerable<object> BlackOutEnd()
        {
            soundPlayerEnd?.Play();
            yield return WaitForSeconds(1);
            ParupunteEnd();
        }

        private IEnumerable<Vector3> GetAroundObjectPosition(Vector3 root, float distance, int n)
        {
            return core.CachedPeds
                .Concat<Entity>(core.CachedVehicles)
                .Concat<Entity>(GTA.World.GetAllProps())
                .Where(x => x.IsSafeExist() && x.IsInRangeOf(root, distance))
                .Select(x => x.Position)
                .OrderBy(_ => Guid.NewGuid())
                .Take(n).ToArray();

        }

        private IEnumerable<object> DrawBlackOutLine()
        {
            var targets = GetAroundObjectPosition(core.PlayerPed.Position, 50, 15);

            drawingDisposable = core.OnDrawingTickAsObservable
                .TakeUntil(this.OnFinishedAsObservable)
                .Subscribe(_ =>
                {
                    var p = core.PlayerPed.Position;
                    foreach (var t in targets)
                    {
                        DrawLine(p, t, Color.White);
                    }
                });

            for (int i = 0; i < 10; i++)
            {
                yield return WaitForSeconds(0.35f);
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
    }
}
