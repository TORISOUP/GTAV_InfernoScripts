using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;

namespace Inferno
{
    internal class ArmorAndHealthSupplier : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("SupplyArmorAndHealth", "armor")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SupplyArmorAndHealth:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //ミッションが始まった時
            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => Game.IsMissionActive)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => SupplyArmorAndHealthAsync(DestroyCancellationToken).Forget());

            //プレイヤが復活した時
            OnThinnedTickAsObservable
                .Where(_ => IsActive && PlayerPed.IsSafeExist())
                .Select(_ => PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Skip(1) //ONにした直後の判定結果は無視
                .Where(x => x)
                .Subscribe(_ => SupplyArmorAndHealthAsync(DestroyCancellationToken).Forget());
        }

        /// <summary>
        /// 体力とアーマー回復
        /// </summary>
        private async ValueTask SupplyArmorAndHealthAsync(CancellationToken ct)
        {
            var player = PlayerPed;
            var maxHealth = player.MaxHealth;
            var maxArmor = Game.Player.GetPlayerMaxArmor();
            player.Health = maxHealth;
            player.Armor = maxArmor;

            while (!ct.IsCancellationRequested && !Game.Player.IsSpecialAbilityEnabled)
            {
                await YieldAsync(ct);
            }

            Game.Player.RefillSpecialAbility();
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.SupplierTitle;
        
        public override string Description => PlayerLocalize.SupplierDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Player;

        
        #endregion
    }
}