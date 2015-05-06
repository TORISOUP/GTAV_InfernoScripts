using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Reactive.Bindings;
using System.Reactive.Concurrency;

namespace Inferno.ChaosMode
{
    class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";

        public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(Scheduler.Immediate);

        protected override int TickInterval
        {
            get { return 1000; }
        }

        protected override void Setup()
        {
            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive.Value = !_isActive.Value;
                });

            //interval間隔で実行
            OnTickAsObservable
                .Where(_ => _isActive.Value)
                .Subscribe(_ => PedRiotActive());
        }

        /// <summary>
        /// 市民を取得して攻撃準備
        /// </summary>
        private void PedRiotActive()
        {
            try
            {
                //市民を取得
                var pedAvailableVeles = CachedPeds
                        .Where(x => x.IsSafeExist() && !x.IsSameEntity(this.GetPlayer()) && x.IsAlive && !x.IsWeaponReloading());

                foreach (var ped in pedAvailableVeles)
                {
                    SetRiot(ped);
                }
            }
            catch
            {
                //nice catch!
            }
        }

        /// <summary>
        /// 攻撃本体
        /// </summary>
        /// <param name="ped">市民</param>
        private void SetRiot(Ped ped)
        {
            try
            {
                var Target = (CachedPeds[Random.Next(0, CachedPeds.Count)]).IsSameEntity(ped)
                    ? null : CachedPeds[Random.Next(0, CachedPeds.Count)];
                if (Target == null) return;
                ped.TaskShootAtCoord(new Vector3(Target.Position.X,Target.Position.Y,Target.Position.Z),10000);
                ped.SetPedFiringPattern((int)FiringPattern.FullAuto);    //フルオート射撃
            }
            catch (Exception e)
            {
                LogWrite("SetRiotError!" + e.Message + "\r\n");
            }
        }
    }
}
