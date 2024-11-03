using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 市民を生成してパラシュート降下させる
    /// </summary>
    internal class SpawnParachuteCitizenArmy : InfernoScript
    {
        private SpawnParachuteCitizenArmyConfig _conf;

        protected override string ConfigFileName { get; } = "SpawnParachuteCitizenArmy.conf";
        private int SpawnDurationSeconds => _conf?.IntervalSeconds ?? 5;

        protected override void Setup()
        {
            _conf = LoadConfig<SpawnParachuteCitizenArmyConfig>();
            CreateInputKeywordAsObservable("SpawnParachuteCitizenArmy", "carmy")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SpawnParachuteCitizenArmy:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {
                if (x)
                {
                    LoopAsync(ActivationCancellationToken).Forget();
                }
            });
        }

        private async ValueTask LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                CreateParachutePedAsync(ct).Forget();
                await DelayAsync(TimeSpan.FromSeconds(SpawnDurationSeconds), ct);
            }
        }

        private async ValueTask CreateParachutePedAsync(CancellationToken ct)
        {
            if (!PlayerPed.IsSafeExist())
            {
                return;
            }

            var playerPosition = PlayerPed.Position;

            var velocity = PlayerPed.Velocity;
            //プレイヤが移動中ならその進行先に生成する
            var ped =
                NativeFunctions.CreateRandomPed(
                    playerPosition + 3 * velocity + new Vector3(0, 0, 70).AroundRandom2D(30));

            if (!ped.IsSafeExist())
            {
                return;
            }

            ped.IsInvincible = true;
            ped.SetNotChaosPed(true);
            ped.MarkAsNoLongerNeeded();
            ped.Task.ClearAllImmediately();
            ped.TaskSetBlockingOfNonTemporaryEvents(true);
            ped.SetPedKeepTask(true);
            ped.AlwaysKeepTask = true;
            
            // 逃げるのを優先するか
            ped.SetCombatAttributes(6, false);
            // 死体に反応するか
            ped.SetCombatAttributes(9, false);
            // 戦闘開始時のリアクションを無効化
            ped.SetCombatAttributes(26, true);
            // 弾丸に対してのリアクションを無効化するか
            ped.SetCombatAttributes(38, true);
            // 逃げる
            ped.SetCombatAttributes(17, false);
            // 防御態勢をとるか
            ped.SetCombatAttributes(37, false);
            // 戦闘から逃走することを許さない
            ped.SetCombatAttributes(58, true);
            ped.SetFleeAttributes(0, 0);


            ped.IsCollisionProof = true;
            //プレイヤ周囲15mを目標に降下
            var targetPosition = playerPosition.AroundRandom2D(3);
            ped.ParachuteTo(targetPosition);
            PlayerPed.PedGroup.Add(ped, false);
            ped.Velocity = new Vector3(0, 0, 4);
            await DelaySecondsAsync(3, ct);

            //着地までカオス化させない
            PedOnGroundedCheckAsync(ped, ActivationCancellationToken).Forget();
        }

        /// <summary>
        /// 市民が着地するまで監視する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private async ValueTask PedOnGroundedCheckAsync(Ped ped, CancellationToken ct)
        {
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    await DelaySecondsAsync(1, ct);


                    //市民が消えていたり死んでたら監視終了
                    if (!ped.IsSafeExist())
                    {
                        return;
                    }

                    if (ped.IsDead)
                    {
                        return;
                    }

                    //着地していたら監視終了
                    if (!ped.IsFloating() || ped.IsInGroup)
                    {
                        break;
                    }

                    if (!ped.IsInParachuteFreeFall && !ped.IsInGroup)
                    {
                        ped.Task.ClearAllImmediately();
                        var targetPosition = PlayerPed.Position.AroundRandom2D(3);
                        ped.ParachuteTo(targetPosition);
                        ped.Velocity = new Vector3(0, 0, 4);
                        await DelaySecondsAsync(3, ct: ct);
                    }
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.LeaveGroup();
                    ped.SetNotChaosPed(false);
                    ped.IsInvincible = false;
                    ped.IsCollisionProof = false;
                }
            }
        }

        [Serializable]
        private class SpawnParachuteCitizenArmyConfig : InfernoConfig
        {
            private int _intervalSeconds = 5;

            /// <summary>
            /// 生成間隔
            /// </summary>
            public int IntervalSeconds
            {
                get => _intervalSeconds;
                set => _intervalSeconds = value.Clamp(1, 60);
            }

            public override bool Validate()
            {
                return IntervalSeconds > 0;
            }
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => EntitiesLocalize.ParachuteTitle;

        public override string Description => EntitiesLocalize.ParachuteDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Interval: {_conf.IntervalSeconds}[s]",
                EntitiesLocalize.ParachuteIntterval,
                _conf.IntervalSeconds,
                60,
                x =>
                {
                    x.Value = _conf.IntervalSeconds;
                    x.Multiplier = 1;
                }, item =>
                {
                    _conf.IntervalSeconds = item.Value;
                    item.Title = $"Interval: {_conf.IntervalSeconds}[s]";
                });

            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                _conf = LoadDefaultConfig<SpawnParachuteCitizenArmyConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(_conf);
                DrawText($"Saved to {ConfigFileName}");
            });
        }

        #endregion
    }
}