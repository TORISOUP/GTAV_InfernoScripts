using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("ひとはなび")]
    internal class Hitohanabi : ParupunteScript
    {
        public Hitohanabi(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "ひとはなび";
        public override string EndMessage => "きたねぇ花火だ";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(5000);
            AddProgressBar(ReduceCounter);
            //コルーチン起動
            StartCoroutine(HitohanabiCoroutine());
        }

        private IEnumerable<object> HitohanabiCoroutine()
        {
            //プレイや周辺の15m上空を設定
            var targetPosition = core.PlayerPed.Position.Around(20) + new Vector3(0, 0, 15);
            var pedList = new HashSet<Ped>();

            //タイマが終わるまでカウントし続ける
            while (!ReduceCounter.IsCompleted)
            {
                foreach (
                    var targetPed in
                        core.CachedPeds.Where(
                            x => x.IsSafeExist()
                            && x.IsAlive
                            && x.IsHuman
                            && !x.IsCutsceneOnlyPed()
                            && x.IsInRangeOf(core.PlayerPed.Position, 50))
                    )
                {
                    //まだの人をリストにくわえる
                    if (pedList.Count < 30 && !pedList.Contains(targetPed))
                    {
                        pedList.Add(targetPed);
                        if (targetPed.IsInVehicle()) targetPed.Task.ClearAllImmediately();
                        targetPed.CanRagdoll = true;
                        targetPed.SetToRagdoll();
                    }
                }

                foreach (var targetPed in pedList.Where(x => x.IsSafeExist()))
                {
                    //すいこむ
                    var direction = targetPosition - targetPed.Position;
                    targetPed.FreezePosition = false;
                    targetPed.SetToRagdoll();
                    var lenght = direction.Length();
                    if (lenght > 5)
                    {
                        direction.Normalize();
                        targetPed.ApplyForce(direction * lenght.Clamp(0, 5) * 4);
                    }
                }
                yield return null;
            }

            //バクハツシサン
            foreach (var targetPed in pedList.Where(x => x.IsSafeExist()))
            {
                targetPed.Kill();
                targetPed.ApplyForce(InfernoUtilities.CreateRandomVector() * 10);
            }
            GTA.World.AddExplosion(targetPosition, GTA.ExplosionType.Rocket, 2.0f, 1.0f);

            //終了
            ParupunteEnd();
        }
    }
}
