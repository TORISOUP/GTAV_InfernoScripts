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

        bool isSuppliedOnMission = false;  //ミッション開始してから全回復させたか
        bool supplyFlag = false;

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
                .Subscribe(_ =>
                    {
                        var player = this.GetPlayer();

                        if (player.IsSafeExist())
                        {
                            if (CheckSupplyFlag())
                            {
                                supplyFlag = true;
                                if (!isSuppliedOnMission) { isSuppliedOnMission = true; }
                            }
                            if ((supplyFlag && player.IsAlive)) { SupplyArmorAndHealth(); }
                        }
                    });


        }
        
        /// <summary>
        /// 体力とアーマーを回復させるタイミングかどうか
        /// </summary>
        private bool CheckSupplyFlag()
        {
            var player = this.GetPlayer();

            if (player.IsSafeExist() && !player.IsAlive)
            {
                return true;
            }

            if (GetMissionFlag() && !isSuppliedOnMission)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 体力とアーマー回復
        /// </summary>
        private void SupplyArmorAndHealth()
        {
            var player = this.GetPlayer();

            DrawText("The armor was supplied.", 3.0f);
            supplyFlag = false; 

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
