using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;

namespace Inferno
{
    /// <summary>
    /// 市民を生成してパラシュート降下させる
    /// </summary>
    internal class SpawnParachuteCitizenArmy : InfernoScript
    {

        private bool _isActive = false;

        protected override int TickInterval
        {
            get { return 3000; }
        }

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("carmy")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("SpawnParachuteCitizenArmy:" + _isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);

            OnTickAsObservable
                .Where(_ => _isActive)
                .Subscribe(_ => CreateParachutePed());
        }

        private void CreateParachutePed()
        {
            var playerPosition = this.GetPlayer().Position;

            //プレイヤの50m上空100m以内の範囲に生成する
            var ped = NativeFunctions.CreateRandomPed(playerPosition + new Vector3(0, 0, 50).AroundRandom2D(50));
            
            if(!ped.IsSafeExist()) return;

            ped.MarkAsNoLongerNeeded();
            ped.Task.ClearAllImmediately();
            
            //プレイヤ周囲15mを目標に降下
            var targetPosition = playerPosition.AroundRandom2D(15);
            ped.ParachuteTo(targetPosition);

            //着地までカオス化させない
            StartCoroutine(PedOnGroundedCheck(ped));
        }

        /// <summary>
        /// 市民が着地するまで監視する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        IEnumerator PedOnGroundedCheck(Ped ped)
        {
            //カオスモードMODからカオス化させない
            ped.SetNotChaosPed(true);
            //市民無敵化
            ped.IsInvincible = true;

            //30秒チェックする
            for (var i = 0; i < 30; i++)
            {
                foreach (var t in WaitForSecond(1.0f))
                {
                    yield return t;
                }

                //市民が消えていたり死んでたら監視終了
                if(!ped.IsSafeExist()) yield break;
                if (ped.IsDead) yield break;

                //パラシュート降下タスクが終了していたら監視終了
                if (!ped.IsTaskActive(PedTaskAction.FALL_WITH_PARACHUTE))
                {
                    break;
                }
            }

            //監視終了後１秒待ってからカオス化許可する(アニメーションがおかしくなるのを避けるため)
            foreach (var s in WaitForSecond(1.0f))
            {
                yield return s;
            }
            
            if (ped.IsSafeExist())
            {
                ped.SetNotChaosPed(false);
            }

            //さらに5秒待ってから無敵化解除
            foreach (var s in WaitForSecond(1.0f))
            {
                yield return s;
            }
            if (ped.IsSafeExist())
            {
                ped.IsInvincible = false;
            }
        }

    }
}
