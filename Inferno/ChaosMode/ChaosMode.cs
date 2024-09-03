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
using Inferno.Utilities;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        /// <summary>
        /// カオス化済み市民一覧
        /// </summary>
        private readonly HashSet<int> chaosedPedList = new();

        private readonly List<uint> coroutineIds = new();

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

        private MissionCharacterTreatmentType _currentTreatType =
            MissionCharacterTreatmentType.ExcludeUniqueCharacter;

        /// <summary>
        /// WeaponProvider
        /// </summary>
        private IWeaponProvider _defaultWeaponProvider;

        private bool _isBaseball;
        private CancellationTokenSource _linkedCts;

        private CancellationTokenSource _localCts = new();

        private MissionCharacterTreatmentType _nextTreatType;
        private SingleWeaponProvider _singleWeaponProvider;
        private CharacterChaosChecker chaosChecker;

        /// <summary>
        /// 設定
        /// </summary>
        private ChaosModeSetting chaosModeSetting;

        private int chaosRelationShipId;
        private IWeaponProvider CurrentWeaponProvider => _singleWeaponProvider ?? _defaultWeaponProvider;

        protected override void Setup()
        {
            var chaosSettingLoader = new ChaosModeSettingLoader();
            chaosModeSetting = chaosSettingLoader.LoadSettingFile(@"ChaosMode_Default.conf");

            chaosChecker = new CharacterChaosChecker(chaosModeSetting.DefaultMissionCharacterTreatment,
                chaosModeSetting.IsChangeMissionCharacterWeapon);

            _defaultWeaponProvider =
                new CustomWeaponProvider(chaosModeSetting.WeaponList, chaosModeSetting.WeaponListForDriveBy);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();

                    _localCts?.Cancel();
                    _localCts?.Dispose();
                    _localCts = null;
                    _linkedCts = null;

                    if (IsActive)
                    {
                        DrawText("ChaosMode:On/" + _currentTreatType);
                    }
                    else
                    {
                        DrawText("ChaosMode:Off");
                        chaosChecker.AvoidAttackEntities = Array.Empty<Entity>();
                    }
                })
                .AddTo(CompositeDisposable);


            _nextTreatType = _currentTreatType;

            //F7でキャラカオスの切り替え（暫定
            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F7)
                .Do(_ =>
                {
                    _nextTreatType = (MissionCharacterTreatmentType)(((int)_nextTreatType + 1) % 3);
                    DrawText("CharacterChaos:" + _nextTreatType, 1.1f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    _currentTreatType = _nextTreatType;
                    chaosChecker.MissionCharacterTreatment = _nextTreatType;
                    DrawText("CharacterChaos:" + _currentTreatType + "[OK]");
                    chaosedPedList.Clear();
                    _localCts?.Cancel();
                    _localCts?.Dispose();
                    _localCts = null;
                    _linkedCts = null;
                    StopAllChaosCoroutine();
                })
                .AddTo(CompositeDisposable);


            CreateInputKeywordAsObservable("yakyu")
                .Subscribe(_ =>
                {
                    _isBaseball = !_isBaseball;
                    DrawText(_isBaseball ? "BaseBallMode:On" : "BaseBallMode:Off");
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
                    StopAllChaosCoroutine();
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
                })
                .AddTo(CompositeDisposable);

            CachedMissionEntities
                .Where(_ => IsActive)
                .Subscribe(all =>
                {
                    chaosChecker.AvoidAttackEntities = all
                        .Where(x => !chaosChecker.IsAttackableEntity(x))
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
                CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius));

            _localCts ??= new CancellationTokenSource();
            _linkedCts ??=
                CancellationTokenSource.CreateLinkedTokenSource(_localCts.Token, ActivationCancellationToken);

            var ct = _linkedCts.Token;

            foreach (var ped in nearPeds.Where(x => x.IsSafeExist() && !chaosedPedList.Contains(x.Handle)))
            {
                chaosedPedList.Add(ped.Handle);
                ChaosPedActionAsync(ped, ct).Forget();
            }
        }

        /// <summary>
        /// 全てのカオスモードの用のコルーチンを停止する
        /// </summary>
        private void StopAllChaosCoroutine()
        {
            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }

            coroutineIds.Clear();
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

            var pedId = ped.Handle;

            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
                // 不適格なら3秒間は何もせずに放置し、リストから削除
                await DelayAsync(TimeSpan.FromSeconds(3), ct);
                chaosedPedList.Remove(pedId);
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

                if (!ped.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius + 10))
                {
                    break;
                }

                if (!chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                }

                SetPedStatus(ped);

                //武器を変更する
                if (Random.Next(0, 100) < chaosModeSetting.WeaponChangeProbability)
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


                if (counterattackTarget.IsSafeExist() && chaosChecker.IsAttackableEntity(counterattackTarget))
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
                var isStupidShooting = Random.Next(0, 100) < chaosModeSetting.StupidPedRate;

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
                        chaosedPedList.Remove(pedId);
                        return;
                    }

                    if (isStupidShooting && stupidShootingTime > 2f)
                    {
                        stupidShootingTime = 0;
                        
                        // バカ射撃なら今のターゲットを執拗にうち続ける
                        var t = Function.Call<Entity>(Hash.GET_PED_TARGET_FROM_COMBAT_PED, ped, 1);
                        if (t.IsSafeExist())
                        {
                            if (t.Model.IsPed)
                            {
                                ped.Task.ShootAt((Ped)t, 5000, GTA.FiringPattern.FullAuto);
                            }
                            else
                            {
                                ped.Task.ShootAt(t.Position, 5000, GTA.FiringPattern.FullAuto);
                            }
                        }
                    }

                    // 定期的にチェックする部分
                    if (checkWaitTime > 1)
                    {
                        checkWaitTime = 0;
                        ped.SetPedFiringPattern((int)FiringPattern.FullAuto);

                        if (ped.Position.DistanceTo(PlayerPed.Position) > chaosModeSetting.Radius + 30)
                        {
                            // プレイヤから遠くにいるなら終了
                            chaosedPedList.Remove(pedId);
                            return;
                        }

                        if (ped.IsFleeing)
                        {
                            break;
                        }


                        // 攻撃されたら次の反撃対象にしておく
                        if (ped.HasEntityBeenDamagedByAnyPed())
                        {
                            counterattackTarget = FindDamageToMePed(ped);
                            ped.ClearEntityLastDamageEntity();
                        }

                        if (chaosChecker.IsPedNearAvoidAttackEntities(ped))
                        {
                            // 攻撃しちゃいけない相手が近くにいるなら停止
                            ped.Task.ClearAll();
                            chaosedPedList.Remove(pedId);
                            return;
                        }

                        if (targets.All(x => !x.IsSafeExist() || !x.IsAlive))
                        {
                            //攻撃対象がいなくなったらやりなおし
                            break;
                        }
                    }

                    await YieldAsync(ct);
                }
            } while (ped.IsSafeExist() && ped.IsAlive);

            chaosedPedList.Remove(pedId);
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

            // 近くのEntityを取得
            var nearPeds = CachedPeds
                .Concat(new[] { PlayerPed })
                .Where(x => x.IsSafeExist() && x.IsAlive && x != ped)
                .Where(x => chaosChecker.IsAttackableEntity(x))
                .OrderBy(x => (ped.Position - x.Position).Length())
                .Take(5)
                .ToArray();

            //プレイヤへの攻撃補正が設定されているならプレイヤをリストに追加する
            if (chaosModeSetting.IsAttackPlayerCorrectionEnabled &&
                Random.Next(0, 100) < chaosModeSetting.AttackPlayerCorrectionProbabillity)
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

            ped.MaxHealth = 5000;
            ped.Health = 5000;
            ped.SetPedShootRate(100);
            ped.Accuracy = chaosModeSetting.ShootAccuracy;
            //戦闘能力？
            ped.SetCombatAbility(100);
            //戦闘範囲
            ped.SetCombatRange(3);

            Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, ped, 3);

            //攻撃を受けたら反撃する
            ped.RegisterHatedTargetsAroundPed(20);
            ped.FiringPattern = GTA.FiringPattern.FullAuto;
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
        private void GiveWeaponTpPed(Ped ped)
        {
            try
            {
                if (!ped.IsSafeExist())
                {
                    return;
                }

                //市民の武器を変更して良いか調べる
                if (!chaosChecker.IsPedChangebalWeapon(ped))
                {
                    return;
                }

                //車に乗っているなら車用の武器を渡す
                Weapon weapon;
                if (_isBaseball)
                {
                    weapon = CurrentWeaponProvider.GetRandomCloseWeapons();
                }
                else
                {
                    if (Random.Next(0, 99) < chaosModeSetting.ForceExplosiveWeaponProbability)
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

                ped.Weapons.Give((WeaponHash)weapon, 9999, true, true);
                ped.SetDropWeaponWhenDead(false); //武器を落とさない
            }
            catch (Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message);
            }
        }
    }
}