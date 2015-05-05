using System;
using System.Collections.Generic;
using System.Linq;
using System;
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
    class GivePedWepon : InfernoScript
    {
        private readonly string Keyword = "chaos";

        public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(Scheduler.Immediate);

        protected override int TickInterval
        {
            get { return 500; }
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
                .Subscribe(_ => GiveWeaponPeds());
        }

        /// <summary>
        /// 市民を取得して武器を持たせる
        /// </summary>
        private void GiveWeaponPeds()
        {
            try
            {
                //市民を取得
                var pedAvailableVeles = CachedPeds
                        .Where(x => x.IsSafeExist() && !x.IsSameEntity(this.GetPlayer()) && x.IsAlive);

                foreach (var ped in pedAvailableVeles)
                {
                    AttachPedWeapon(ped);
                }
            }
            catch
            {
                //nice catch!
            }
        }

        /// <summary>
        /// 武器を持たせる処理
        /// </summary>
        private unsafe void AttachPedWeapon(Ped ped)
        {
            try
            {
                ped.SetDropWeaponWhenDead(false);    //武器を落とさない
                var weapon = this.GetGTAObjectHashKey("WEAPON_RPG"); //武器名からハッシュ値取得
                ped.GiveWeapon(weapon, 1000);  //指定武器所持
                ped.EquipWeapon(weapon);  //武器装備
            }
            catch(Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message + "\r\n");
            }
        }
    }
}
