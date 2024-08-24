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

        private bool _isBaseball;
        private CharacterChaosChecker chaosChecker;

        /// <summary>
        /// 設定
        /// </summary>
        private ChaosModeSetting chaosModeSetting;

        private int chaosRelationShipId;

        private MissionCharacterTreatmentType _currentTreatType =
            MissionCharacterTreatmentType.ExcludeUniqueCharacter;

        /// <summary>
        /// WeaponProvider
        /// </summary>
        private IWeaponProvider _defaultWeaponProvider;

        private MissionCharacterTreatmentType _nextTreatType;
        private SingleWeaponProvider _singleWeaponProvider;
        private IWeaponProvider CurrentWeaponProvider => _singleWeaponProvider ?? _defaultWeaponProvider;

        private CancellationTokenSource _localCts = new();
        private CancellationTokenSource _linkedCts;

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
            OnRecievedInfernoEvent
                .OfType<ChasoModeEvent>()
                .Subscribe(e =>
                {
                    if (e is ChangeToDefaultEvent)
                    {
                        _singleWeaponProvider = null;
                    }
                    else if (e is ChangeWeaponEvent)
                    {
                        var s = (ChangeWeaponEvent)e;
                        _singleWeaponProvider = new SingleWeaponProvider(s.Weapon);
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
            if (!PlayerPed.IsSafeExist()) return;

            //まだ処理をしていない市民を対象とする
            var nearPeds =
                CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius));

            _localCts ??= new CancellationTokenSource();
            _linkedCts ??=
                CancellationTokenSource.CreateLinkedTokenSource(_localCts.Token, GetActivationCancellationToken());

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
            foreach (var id in coroutineIds) StopCoroutine(id);
            coroutineIds.Clear();
        }


        /// <summary>
        /// 市民一人ひとりについて回るコルーチン
        /// </summary>
        private async ValueTask ChaosPedActionAsync(Ped ped, CancellationToken ct)
        {
            //魚なら除外する
            var m = (uint)ped.Model.Hash;
            if (fishHashes.Contains(m)) return;

            if (!ped.IsSafeExist()) return;
            var pedId = ped.Handle;

            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            var equipedWeapon = GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
                // 不適格なら3秒間は何もせずに放置し、リストから削除
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
                chaosedPedList.Remove(pedId);
                return;
            }

            SetPedStatus(ped);

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
                if (!ped.IsSafeExist() || ped.IsDead || !PlayerPed.IsSafeExist()) break;

                if (!ped.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius + 10)) break;

                if (!chaosChecker.IsPedChaosAvailable(ped)) break;

                //武器を変更する
                if (Random.Next(0, 100) < chaosModeSetting.WeaponChangeProbabillity)
                {
                    equipedWeapon = GiveWeaponTpPed(ped);
                }

                if (ped.IsInAir)
                {
                    // 空挺市民を考慮して空中にいるならちょっと待つ
                    await Task.Delay(TimeSpan.FromSeconds(3), ct);
                }
                else
                {
                    // ちょっと待つ
                    await DelayRandomFrameAsync(1, 10, ct);
                }

                if (!ped.IsSafeExist() || ped.IsDead) break;

                Entity target;
                if (counterattackTarget.IsSafeExist() && chaosChecker.IsAttackableEntity(counterattackTarget))
                {
                    // 反撃対象が設定されているならその人を対象とする
                    target = counterattackTarget;
                }
                else
                {
                    // 対象が居ないならランダムに決める
                    target = GetTargetPed(ped);
                }

                //攻撃する
                var canAttack = TryRiot(ped, target, equipedWeapon);

                // 攻撃失敗したらやりなおし
                if (!canAttack)
                {
                    counterattackTarget = null;
                    continue;
                }

                // 行動時間
                // 攻撃対象が乗り物なら短めの行動時間
                float waitTime = target is Ped ? Random.Next(5, 40) : Random.Next(5, 10);
                float checkWaitTime = 0;

                CancellationTokenSource studpidCts = null;


                while (!ct.IsCancellationRequested && waitTime > 0)
                {
                    waitTime -= NativeFunctions.GetFrameTime();
                    checkWaitTime += NativeFunctions.GetFrameTime();
                    if (!ped.IsSafeExist())
                    {
                        break;
                    }

                    if (!target.IsSafeExist() || !target.IsAlive)
                    {
                        break;
                    }


                    if (checkWaitTime > 1)
                    {
                        checkWaitTime = 0;
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
        private Entity GetTargetPed(Ped ped)
        {
            if (!ped.IsSafeExist() || !PlayerPed.IsSafeExist())
            {
                return null;
            }

            //プレイヤへの攻撃補正が設定されているならプレイヤを攻撃対象にする
            if (chaosModeSetting.IsAttackPlayerCorrectionEnabled &&
                Random.Next(0, 100) < chaosModeSetting.AttackPlayerCorrectionProbabillity)
            {
                return PlayerPed;
            }

            // 近くのEntityを取得
            var nearPeds = CachedEntities
                .Concat(new Entity[] { PlayerPed })
                .Where(x => x is Ped || x is Vehicle || x.IsSafeExist() && x.IsAlive && x != ped)
                .Where(x => chaosChecker.IsAttackableEntity(x))
                .OrderBy(x => (ped.Position - x.Position).Length())
                .Take(5)
                .ToArray();

            if (nearPeds.Length == 0)
            {
                return null;
            }

            var randomindex = Random.Next(nearPeds.Length);
            return nearPeds[randomindex];
        }

        // 自身に対して攻撃をしてきた相手を探す
        private Ped FindDamageToMePed(Ped me)
        {
            return World.GetAllPeds().FirstOrDefault(x => x.IsSafeExist() && x.IsAlive && me.HasBeenDamagedByPed(x));
        }

        private void SetPedStatus(Ped ped)
        {
            if (!ped.IsSafeExist()) return;

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
            ped.SetCombatAttributes(17,false);
            // 距離に応じて発射レートを変える
            ped.SetCombatAttributes(24, RandomBool());
            // ターゲットを切り替えるのを禁止するか
            ped.SetCombatAttributes(25, false);
            // 戦闘開始時のリアクションを無効化
            ped.SetCombatAttributes(26, false);
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
            ped.SetCombatRange(100);
            //攻撃を受けたら反撃する
            ped.RegisterHatedTargetsAroundPed(20);
        }

        /// <summary>
        /// 市民を暴徒化する
        /// </summary>
        private bool TryRiot(Ped ped, Entity target, Weapon equipWeapon)
        {
            try
            {
                if (!ped.IsSafeExist()) return false;
                if (!target.IsSafeExist()) return false;
                ped.TaskSetBlockingOfNonTemporaryEvents(true);
                ped.SetPedKeepTask(true);
                ped.AlwaysKeepTask = true;
                ped.IsVisible = true;

                bool canAttack = false;

                if (target is Ped targetPed)
                {
                    canAttack = true;
                    ped.Task.ClearAllImmediately();
                    ped.Task.FightAgainst(targetPed, 60000);
                }
                else if (target is Vehicle vehicleTarget)
                {
                    ped.Task.ClearAllImmediately();
                    if (ped.IsInVehicle())
                    {
                        if (equipWeapon.IsShootWeapon())
                        {
                            ped.Task.ShootAt(vehicleTarget.Position, 60000);
                            canAttack = true;
                        }
                    }
                    else
                    {
                        if (equipWeapon.IsProjectileWeapon())
                        {
                            ped.ThrowProjectile(target.Position);
                            canAttack = true;
                        }
                        else if (equipWeapon.IsShootWeapon())
                        {
                            ped.Task.ShootAt(vehicleTarget.Position, 10000);
                            canAttack = true;
                        }
                    }
                }

                ped.SetPedFiringPattern((int)FiringPattern.FullAuto);
                return canAttack;
            }
            catch (Exception e)
            {
                LogWrite(e.ToString());
                LogWrite(e.StackTrace);
                return false;
            }
        }


        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private Weapon GiveWeaponTpPed(Ped ped)
        {
            try
            {
                if (!ped.IsSafeExist()) return Weapon.UNARMED;
                //市民の武器を変更して良いか調べる
                if (!chaosChecker.IsPedChangebalWeapon(ped)) return Weapon.UNARMED;

                //車に乗っているなら車用の武器を渡す
                var weapon = Weapon.UNARMED;
                if (_isBaseball)
                {
                    weapon = CurrentWeaponProvider.GetRandomCloseWeapons();
                }
                else
                {
                    weapon = ped.IsInVehicle()
                        ? CurrentWeaponProvider.GetRandomDriveByWeapon()
                        : CurrentWeaponProvider.GetRandomAllWeapons();
                }

                ped.Weapons.Give((WeaponHash)weapon, 9999, true, true);
                ped.SetDropWeaponWhenDead(false); //武器を落とさない

                return weapon;
            }
            catch (Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message);
            }

            return Weapon.UNARMED;
        }
    }
}