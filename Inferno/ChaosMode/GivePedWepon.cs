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
                .Subscribe(_ => GetPeds());
        }

        /// <summary>
        /// 市民を取得
        /// </summary>
        private void GetPeds()
        {
            try
            {
                //市民を取得
                var pedAvailableVeles = CachedPeds
                        .Where(x => x.IsSafeExist() && !x.IsSameEntity(Game.Player.Character) && x.IsAlive);

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
                ped.Accuracy = 100; //命中率
                Function.Call(Hash.SET_PED_DROPS_WEAPONS_WHEN_DEAD, ped, 0);    //武器を落とさない
                var weapon = Function.Call<int>(Hash.GET_HASH_KEY, "WEAPON_RPG");   //武器名からハッシュ値取得
                Function.Call(Hash.GIVE_DELAYED_WEAPON_TO_PED, ped, weapon, 1000, 0);  //指定武器装備
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, weapon, true);  //武器装備
            }
            catch(Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message + "\r\n");
            }
        }
    }
}
