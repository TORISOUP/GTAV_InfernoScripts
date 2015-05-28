using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using GTA;
using GTA.Native;

namespace Inferno
{
    class ArmorAndHealthSupplier : InfernoScript
    {
        bool hasPlayerWasted = false; 
        bool isActive = false;

        /// <summary>
        /// 0.1秒間隔
        /// </summary>
        protected override int TickInterval
        {
            get { return 100; }
        }

        protected override void Setup()
        {

            CreateInputKeywordAsObservable("armor")
                .Subscribe(_ =>
                {
                    isActive = !isActive;
                    DrawText("SupplyArmorAndHealth:" + isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => isActive = true);

            OnTickAsObservable
                .Where(_ => isActive)
                .Select(_ => this.GetPlayer())
                .Where(p => p.IsSafeExist())
                .Select(p => p.IsDead)
                .DistinctUntilChanged()
                .Subscribe(flag =>
                {
                    if  (flag) { hasPlayerWasted = true; }
                });

            OnTickAsObservable
                .Where(_ => isActive)
                .Select(_ => this.GetPlayer())
                .Where(p => p.IsSafeExist())
                .Select(p => GetMissionFlag() || (p.IsAlive && hasPlayerWasted))
                .DistinctUntilChanged()
                .Where(flag => flag)
                .Subscribe(_ =>
                {
                    SupplyArmorAndHealth();
                    hasPlayerWasted = false;
                });
        }
        
        /// <summary>
        /// 体力とアーマー回復
        /// </summary>
        private void SupplyArmorAndHealth()
        {
            var player = this.GetPlayer();

            DrawText("The armor was supplied.", 3.0f);

            var maxHealth = player.MaxHealth;
            var maxArmor = Game.Player.GetPlayerMaxArmor();
            player.Health = maxHealth;
            player.Armor = maxArmor;
        }


        /// <summary>
        /// 現在ミッション中かどうか 
        /// </summary>
        /// (Player.IsOnMissionはミッションを始められる状態かどうかの判定)
        public bool GetMissionFlag()
        {
            var _missionFlag = Function.Call<int>(Hash.GET_MISSION_FLAG);
            if (_missionFlag != 0) { return true; }
            else { return false; }
        }
    }
}
