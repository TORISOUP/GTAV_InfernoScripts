using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class Hitohanabi : ParupunteScript
    {
        public Hitohanabi(ParupunteCore core) : base(core)
        {
            //コンストラクタは必須
        }

        public override void OnStart()
        {
            //コルーチン起動
            StartCoroutine(HitohanabiCoroutine());
        }

        private IEnumerable<object> HitohanabiCoroutine()
        {
            //プレイヤ周辺100mの市民取得
            var targetPeds =
                core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(core.PlayerPed.Position, 100)).ToArray();

            //市民を車から引きずり出す
            foreach (var targetPed in targetPeds.Where(x => x.IsSafeExist()))
            {
                targetPed.Task.ClearAllImmediately();
            }

            //プレイや周辺の15m上空を設定
                var targetPosition = core.PlayerPed.Position.AroundRandom2D(10) + new Vector3(0, 0, 15);

            //だいたい5秒間市民を一箇所に吸い込み続ける
            for (var i = 0; i < 50; i++)
            {
                foreach (var targetPed in targetPeds.Where(x=>x.IsSafeExist()))
                {
                    var direction = targetPosition - targetPed.Position;
                    var lenght = direction.Length();
                    direction.Normalize();
                    targetPed.ApplyForce(direction*lenght.Clamp(0, 2)*15);
                }
                yield return null;
            }

            //バクハツシサン
            foreach (var targetPed in targetPeds.Where(x => x.IsSafeExist()))
            {
                targetPed.Kill();
                targetPed.ApplyForce(InfernoUtilities.CreateRandomVector() * 40);
            }
            GTA.World.AddExplosion(targetPosition, GTA.ExplosionType.Rocket, 2.0f, 1.0f);

            //終了
            ParupunteEnd();
        } 
    }
}
