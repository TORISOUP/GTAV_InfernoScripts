using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using GTA; using UniRx;
using GTA.Native;

namespace Inferno
{
    class ArmorAndHealthSupplier : InfernoScript
    {

        protected override void Setup()
        {

            CreateInputKeywordAsObservable("armor")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SupplyArmorAndHealth:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //ミッションが始まった時
            OnTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => Game.MissionFlag)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => SupplyArmorAndHealth());

            //プレイヤが復活した時
            OnTickAsObservable
                .Where(_ => IsActive && PlayerPed.IsSafeExist())
                .Select(_ => PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Skip(1) //ONにした直後の判定結果は無視
                .Where(x => x)
                .Subscribe(_ => SupplyArmorAndHealth());
        }
        
        /// <summary>
        /// 体力とアーマー回復
        /// </summary>
        private void SupplyArmorAndHealth()
        {
            var player = PlayerPed;
            var maxHealth = player.MaxHealth;
            var maxArmor = Game.Player.GetPlayerMaxArmor();
            player.Health = maxHealth;
            player.Armor = maxArmor;
        }
    }
}
