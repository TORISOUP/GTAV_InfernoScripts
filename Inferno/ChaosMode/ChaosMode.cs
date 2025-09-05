using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;
using Inferno.InfernoScripts.Event.ChasoMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        /// <summary>
        /// カオス化済み市民一覧
        /// </summary>
        private readonly HashSet<Ped> chaosedPedList = new();

        private readonly uint[] fishHashes =
        {
            (uint)PedHash.Fish,
            (uint)PedHash.HammerShark,
            (uint)PedHash.Dolphin,
            (uint)PedHash.Humpback,
            (uint)PedHash.KillerWhale,
            (uint)PedHash.Stingray,
            (uint)PedHash.TigerShark
        };

        private readonly string Keyword = "chaos";

        private MissionCharacterBehaviour _currentTreatType =
            MissionCharacterBehaviour.ExcludeUniqueCharacter;

        /// <summary>
        /// WeaponProvider
        /// </summary>
        private IWeaponProvider _defaultWeaponProvider;

        private CancellationTokenSource _linkedCts;

        private CancellationTokenSource _localCts = new();

        private MissionCharacterBehaviour _nextTreatType;
        private SingleWeaponProvider _singleWeaponProvider;
        private CharacterChaosChecker _chaosChecker;

        /// <summary>
        /// 設定
        /// </summary>
        private ChaosModeSetting _chaosModeSetting;

        private ChaosModeUIBuilder _uiBuilder;

        private IWeaponProvider CurrentWeaponProvider => _singleWeaponProvider ?? _defaultWeaponProvider;
        private IWeaponProvider DefaultWeaponProvider => _defaultWeaponProvider;

        private ChaosModeSettingReadWriter _chaosSettingLoader = new();

        protected override void Setup()
        {
            _chaosSettingLoader = new ChaosModeSettingReadWriter();
            _chaosModeSetting = _chaosSettingLoader.LoadSettingFile(@"ChaosMode.conf");
            
            _chaosChecker = new CharacterChaosChecker(_chaosModeSetting);

            var customWeaponProvider =
                new CustomWeaponProvider(_chaosModeSetting.WeaponList, _chaosModeSetting.WeaponListForDriveBy);
            _defaultWeaponProvider = customWeaponProvider;

            _uiBuilder = new ChaosModeUIBuilder(_chaosModeSetting);
            _uiBuilder.AddTo(CompositeDisposable);
            _uiBuilder.OnChangeWeaponSetting = () =>
            {
                customWeaponProvider.SetUp(_chaosModeSetting.WeaponList, _chaosModeSetting.WeaponListForDriveBy);
            };
            
            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable("ChaosMode_Activate", Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;

                    if (IsActive)
                    {
                        DrawText("ChaosMode:On/" + _currentTreatType);
                    }
                    else
                    {
                        DrawText("ChaosMode:Off");
                    }
                })
                .AddTo(CompositeDisposable);


            IsActiveRP.Subscribe(_ =>
            {
                chaosedPedList.Clear();
                _localCts?.Cancel();
                _localCts?.Dispose();
                _localCts = null;
                _linkedCts = null;
                _chaosChecker.AvoidAttackEntities = Array.Empty<Entity>();
            });


            _nextTreatType = _currentTreatType;

            //キャラカオスの切り替え
            CreateInputKeywordAsObservable("ChaosMode_MissionCharacterBehaviour", "F7")
                .Where(_ => IsActive)
                .Do(_ =>
                {
                    _nextTreatType = (MissionCharacterBehaviour)(((int)_nextTreatType + 1) % 3);
                    DrawText("CharacterChaos:" + _nextTreatType, 1.1f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    _currentTreatType = _nextTreatType;
                    _chaosModeSetting.MissionCharacterBehaviour= _nextTreatType;
                    DrawText("CharacterChaos:" + _currentTreatType + "[OK]");
                    chaosedPedList.Clear();
                    _localCts?.Cancel();
                    _localCts?.Dispose();
                    _localCts = null;
                    _linkedCts = null;
                })
                .AddTo(CompositeDisposable);


            CreateInputKeywordAsObservable("ChaosMode_MeleeOnly", "yakyu")
                .Subscribe(_ =>
                {
                    _chaosModeSetting.MeleeWeaponOnly = !_chaosModeSetting.MeleeWeaponOnly;
                    DrawText(_chaosModeSetting.MeleeWeaponOnly ? "BaseBallMode:On" : "BaseBallMode:Off");
                    if (IsActive)
                    {
                        ChangeAllRiotCitizenWeapon();
                    }
                })
                .AddTo(CompositeDisposable);


            var oneSecondTich = CreateTickAsObservable(TimeSpan.FromSeconds(1));

            //市民をカオス化する
            oneSecondTich
                .Where(_ => IsActive && PlayerPed.IsSafeExist() && PlayerPed.IsAlive)
                .Subscribe(_ => CitizenChaos())
                .AddTo(CompositeDisposable);


            //プレイヤが死んだらリセット
            oneSecondTich
                .Where(_ => PlayerPed.IsSafeExist())
                .Select(_ => PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    chaosedPedList.Clear();
                    _localCts?.Cancel();
                    _localCts?.Dispose();
                    _localCts = null;
                    _linkedCts = null;
                })
                .AddTo(CompositeDisposable);


            oneSecondTich
                .Where(_ => IsActive)
                .Subscribe(_ => NativeFunctions.SetAllRandomPedsFlee(Game.Player, false))
                .AddTo(CompositeDisposable);

            //イベントが来たら武器を変更する
            OnReceivedInfernoEvent
                .ObserveOn(InfernoScheduler)
                .OfType<ChasoModeEvent>()
                .Subscribe(e =>
                {
                    if (e is ChangeToDefaultEvent)
                    {
                        _singleWeaponProvider = null;
                    }
                    else if (e is ChangeWeaponEvent changeWeaponEvent)
                    {
                        _singleWeaponProvider = new SingleWeaponProvider(changeWeaponEvent.Weapon);
                    }

                    if (IsActive)
                    {
                        ChangeAllRiotCitizenWeapon();
                    }
                })
                .AddTo(CompositeDisposable);

            CachedMissionEntities
                .Where(_ => IsActive)
                .Subscribe(all =>
                {
                    _chaosChecker.AvoidAttackEntities = all
                        .Where(x => !_chaosChecker.IsAttackableEntity(x))
                        .ToArray();
                })
                .AddTo(CompositeDisposable);
        }

        private void CitizenChaos()
        {
            if (!PlayerPed.IsSafeExist())
            {
                return;
            }

            //まだ処理をしていない市民を対象とする
            var nearPeds =
                CachedPeds.Where(x => x.IsSafeExist() && x.IsHuman && x.IsInRangeOf(PlayerPed.Position, _chaosModeSetting.Radius));

            _localCts ??= new CancellationTokenSource();
            _linkedCts ??=
                CancellationTokenSource.CreateLinkedTokenSource(_localCts.Token, ActivationCancellationToken);

            var ct = _linkedCts.Token;

            foreach (var ped in nearPeds.Where(x => x.IsSafeExist() && !chaosedPedList.Contains(x)))
            {
                chaosedPedList.Add(ped);
                ChaosPedActionAsync(ped, ct).Forget();
            }
        }

        // すべての市民の武器を交換する
        private void ChangeAllRiotCitizenWeapon()
        {
            foreach (var ped in chaosedPedList)
            {
                if (ped.IsSafeExist())
                {
                    GiveWeaponTpPed(ped, true);
                }
            }
        }


        /// <summary>
        /// 市民一人ひとりについて回るコルーチン
        /// </summary>
        private async ValueTask ChaosPedActionAsync(Ped ped, CancellationToken ct)
        {
            //魚なら除外する
            var m = (uint)ped.Model.Hash;
            if (fishHashes.Contains(m))
            {
                return;
            }

            if (!ped.IsSafeExist())
            {
                return;
            }

            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!_chaosChecker.IsPedChaosAvailable(ped))
            {
                // 不適格なら3秒間は何もせずに放置し、リストから削除
                await DelayAsync(TimeSpan.FromSeconds(3), ct);
                chaosedPedList.Remove(ped);
                return;
            }


            if (ped.IsInVehicle())
            {
                var vehicle = ped.CurrentVehicle;
                var seat = ped.SeatIndex;
                ped.Task.ClearAllImmediately();
                ped.SetIntoVehicle(vehicle, seat);
            }
            else
            {
                ped.Task.ClearAllImmediately();
            }


            if (ped.IsRequiredForMission())
            {
                var playerGroup = Game.Player.GetPlayerGroup();
                if (!ped.IsPedGroupMember(playerGroup))
                {
                    ped.NeverLeavesGroup = false;
                    ped.RemovePedFromGroup();
                }
            }

            // 反撃対象
            Ped counterattackTarget = null;

            //以下ループ
            do
            {
                if (!ped.IsSafeExist() || ped.IsDead || !PlayerPed.IsSafeExist())
                {
                    break;
                }

                if (!ped.IsInRangeOf(PlayerPed.Position, _chaosModeSetting.Radius + 10))
                {
                    break;
                }

                if (!_chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                }

                SetPedStatus(ped);

                //武器を変更する
                if (Random.Next(0, 100) < _chaosModeSetting.WeaponChangeProbability)
                {
                    GiveWeaponTpPed(ped);
                }

                if (ped.IsInAir)
                {
                    // 空挺市民を考慮して空中にいるならちょっと待つ
                    await DelayAsync(TimeSpan.FromSeconds(3), ct);
                }
                else
                {
                    // ちょっと待つ
                    await DelayRandomFrameAsync(1, 10, ct);
                }

                if (!ped.IsSafeExist() || ped.IsDead)
                {
                    break;
                }


                if (counterattackTarget.IsSafeExist() && _chaosChecker.IsAttackableEntity(counterattackTarget))
                {
                    // 反撃対象が設定されているならその人を対象に追加する
                    Function.Call(Hash.REGISTER_TARGET, ped, counterattackTarget);
                }

                var targets = GetTargetPeds(ped);
                //攻撃する
                TryRiot(ped, targets);


                // 行動時間
                float waitTime = Random.Next(3, 20);
                float checkWaitTime = 100;
                float stupidShootingTime = 100;
                var isStupidShooting = Random.Next(0, 100) < _chaosModeSetting.StupidShootingRate &&
                                       !ped.IsPedEquippedWithMeleeWeapon() && !_chaosModeSetting.MeleeWeaponOnly;

                while (!ct.IsCancellationRequested && waitTime > 0)
                {
                    var f = NativeFunctions.GetFrameTime();
                    waitTime -= f;
                    checkWaitTime += f;
                    stupidShootingTime += f;

                    // 自分が死んでるなら中止
                    if (!ped.IsSafeExist() || !ped.IsAlive)
                    {
                        break;
                    }


                    if (ped.IsNotChaosPed())
                    {
                        // 除外キャラに指定されたら停止
                        chaosedPedList.Remove(ped);
                        return;
                    }

                    if (isStupidShooting && stupidShootingTime > 5f)
                    {
                        stupidShootingTime = 0;

                        // バカ射撃なら今のターゲットを執拗にうち続ける
                        var t = Function.Call<Entity>(Hash.GET_PED_TARGET_FROM_COMBAT_PED, ped, 1);
                        if (t.IsSafeExist())
                        {
                            if (t.Model.IsPed)
                            {
                                ped.Task.ShootAt((Ped)t, 4000, GTA.FiringPattern.FullAuto);
                            }
                            else
                            {
                                ped.Task.ShootAt(t.Position, 4000, GTA.FiringPattern.FullAuto);
                            }
                        }
                    }

                    // 定期的にチェックする部分
                    if (checkWaitTime > 1)
                    {
                        checkWaitTime = 0;

                        if (ped.Position.DistanceTo(PlayerPed.Position) > _chaosModeSetting.Radius + 30)
                        {
                            // プレイヤから遠くにいるなら終了
                            chaosedPedList.Remove(ped);
                            return;
                        }

                        if (ped.IsFleeing)
                        {
                            ped.Task.ClearAll();
                            break;
                        }

                        // 攻撃されたら次の反撃対象にしておく
                        if (ped.HasEntityBeenDamagedByAnyPed())
                        {
                            counterattackTarget = FindDamageToMePed(ped);
                            ped.ClearEntityLastDamageEntity();
                        }

                        // 攻撃しちゃいけない相手が近くにいるなら停止
                        // ただしミッションキャラクターの場合は無視
                        if (_chaosChecker.IsPedNearAvoidAttackEntities(ped) && !ped.IsRequiredForMission())
                        {
                            ped.Task.ClearAll();
                            chaosedPedList.Remove(ped);
                            return;
                        }

                        if (targets.All(x => !x.IsSafeExist() || !x.IsAlive))
                        {
                            //攻撃対象がいなくなったらやりなおし
                            break;
                        }

                        foreach (var t in targets.Where(x => x.IsSafeExist() && x.IsAlive))
                        {
                            ped.Task.FightAgainst(t);
                        }
                    }

                    await YieldAsync(ct);
                }
            } while (ped.IsSafeExist() && ped.IsAlive);

            chaosedPedList.Remove(ped);
        }


        /// <summary>
        /// カオス化時の攻撃対象を取得する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private Ped[] GetTargetPeds(Ped ped)
        {
            if (!ped.IsSafeExist() || !PlayerPed.IsSafeExist())
            {
                return Array.Empty<Ped>();
            }

            var isCorrectionEnabled = _chaosModeSetting.IsAttackPlayerCorrectionEnabled;

            // 近くのEntityを取得
            var nearPeds = CachedPeds
                .Concat(isCorrectionEnabled ? Array.Empty<Ped>() : new[] { PlayerPed })
                .Where(x => x.IsSafeExist() && x.IsAlive && x != ped)
                .Where(x => _chaosChecker.IsAttackableEntity(x))
                .OrderBy(x => (ped.Position - x.Position).Length())
                .Take(5)
                .ToArray();

            //プレイヤへの攻撃補正が設定されているならプレイヤをリストに追加する
            if (isCorrectionEnabled &&
                Random.Next(0, 100) < _chaosModeSetting.AttackPlayerCorrectionProbability)
            {
                return nearPeds.Concat(new[] { PlayerPed }).ToArray();
            }

            return nearPeds;
        }

        // 自身に対して攻撃をしてきた相手を探す
        private Ped FindDamageToMePed(Ped me)
        {
            return World.GetAllPeds().FirstOrDefault(x => x.IsSafeExist() && x.IsAlive && me.HasBeenDamagedByPed(x));
        }

        private void SetPedStatus(Ped ped)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            bool RandomBool()
            {
                return Random.Next(0, 100) > 50;
            }

            // カバーするか
            ped.SetCombatAttributes(0, RandomBool());
            // 車を使用するか
            ped.SetCombatAttributes(1, RandomBool());
            // ドライブバイを許可するか
            ped.SetCombatAttributes(2, true);
            // 車から降りることができるか
            ped.SetCombatAttributes(3, true);
            //非武装（近接武器）で武装した市民を攻撃できるか
            ped.SetCombatAttributes(5, true);
            // 逃げるのを優先するか
            ped.SetCombatAttributes(6, false);
            // 死体に反応するか
            ped.SetCombatAttributes(9, false);
            // ブラインドファイアをするか
            ped.SetCombatAttributes(12, RandomBool());
            // 逃げる
            ped.SetCombatAttributes(17, false);
            // 距離に応じて発射レートを変える
            ped.SetCombatAttributes(24, RandomBool());
            // ターゲットを切り替えるのを禁止するか
            ped.SetCombatAttributes(25, RandomBool());
            // 戦闘開始時のリアクションを無効化
            ped.SetCombatAttributes(26, true);
            // 射線が通って無くても攻撃するか
            ped.SetCombatAttributes(30, true);
            // 防御態勢をとるか
            ped.SetCombatAttributes(37, false);
            // 弾丸に対してのリアクションを無効化するか
            ped.SetCombatAttributes(38, true);
            // 自分が武器を持って無くても他人を襲うか
            ped.SetCombatAttributes(46, true);
            // 突撃を許可するか
            ped.SetCombatAttributes(50, RandomBool());
            // 車で攻撃するか(?)
            ped.SetCombatAttributes(52, RandomBool());
            // 車両の武器を使用するか
            ped.SetCombatAttributes(53, true);
            // 最適な武器を選択するか
            ped.SetCombatAttributes(54, RandomBool());
            // 視線が通らない場合の追跡を無効にする
            ped.SetCombatAttributes(57, false);
            // 戦闘から逃走することを許さない
            ped.SetCombatAttributes(58, true);
            // スモークグレネードを投げる
            ped.SetCombatAttributes(60, true);
            // 歩道に乗り上げて運転する
            ped.SetCombatAttributes(70, true);
            // 車両に対してRPGを優先的に使う
            ped.SetCombatAttributes(72, RandomBool());

            ped.SetFleeAttributes(0, 0);

            // ミッションキャラクタでないならば体力を上書きする
            if (!ped.IsRequiredForMission())
            {
                ped.MaxHealth = 5000;
                ped.Health = 5000;
            }

            ped.SetPedShootRate(100);
            ped.Accuracy = _chaosModeSetting.ShootAccuracy;
            //戦闘能力？
            ped.SetCombatAbility(100);
            //戦闘範囲
            ped.SetCombatRange(3);
        }

        /// <summary>
        /// 市民を暴徒化する
        /// </summary>
        private void TryRiot(Ped ped, Ped[] targets)
        {
            try
            {
                if (!ped.IsSafeExist())
                {
                    return;
                }

                ped.TaskSetBlockingOfNonTemporaryEvents(false);
                ped.SetPedKeepTask(true);
                ped.AlwaysKeepTask = true;
                ped.IsVisible = true;

                foreach (var target in targets.Where(x => x.IsSafeExist() && x.IsAlive))
                {
                    if (target == PlayerPed)
                    {
                        var isCop = ped.IsCop();
                        var isWanted = Game.Player.WantedLevel > 0;
                        if (isCop && !isWanted)
                        {
                            // 手配度が付いていない かつ 警察の場合は
                            // Playerと敵対させない（手配度がつくため）
                            continue;
                        }
                    }

                    ped.Task.FightAgainst(target, 60000);
                    Function.Call(Hash.REGISTER_TARGET, ped, target);
                }


                ped.SetPedFiringPattern((int)FiringPattern.FullAuto);
            }
            catch (Exception e)
            {
                LogWrite(e.ToString());
                LogWrite(e.StackTrace);
            }
        }


        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private void GiveWeaponTpPed(Ped ped, bool removeWeapon = false)
        {
            try
            {
                if (!ped.IsSafeExist())
                {
                    return;
                }

                //市民の武器を変更して良いか調べる
                if (!_chaosChecker.IsPedChangebalWeapon(ped))
                {
                    return;
                }

                //車に乗っているなら車用の武器を渡す
                Weapon weapon;
                if (_chaosModeSetting.MeleeWeaponOnly)
                {
                    // 野球大会中でもパルプンテ側の効果が優先される
                    weapon = CurrentWeaponProvider.GetRandomMeleeWeapons();
                }
                else
                {
                    if (Random.Next(0, 99) < _chaosModeSetting.ForceExplosiveWeaponProbability)
                    {
                        // 爆発物補正
                        weapon = CurrentWeaponProvider.GetExplosiveWeapon();
                    }
                    else
                    {
                        weapon = ped.IsInVehicle()
                            ? CurrentWeaponProvider.GetRandomDriveByWeapon()
                            : CurrentWeaponProvider.GetRandomAllWeapons();
                    }
                }

                if (removeWeapon)
                {
                    ped.Weapons.RemoveAll();
                }

                ped.Weapons.Give((WeaponHash)weapon, 9999, true, true);
                ped.SetDropWeaponWhenDead(Random.Next(0, 99) < _chaosModeSetting.WeaponDropProbability); //武器を落とすかどうか
            }
            catch (Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message);
            }
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => _uiBuilder.DisplayName;

        public override string Description => _uiBuilder.Description;

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Root;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            _uiBuilder.OnUiMenuConstruct(pool, subMenu);

            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                _chaosModeSetting.OverrideDto(_chaosSettingLoader.CreateDefaultChaosModeSetting);
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                _chaosSettingLoader.SaveSettingFile(@"ChaosMode.conf", _chaosModeSetting.ToDto());
                DrawText($"Saved to ChaosMode.conf");
            });
        }

        #endregion
    }
}