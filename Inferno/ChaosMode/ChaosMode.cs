using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;
using Reactive.Bindings;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";
        public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(Scheduler.Immediate);
        private CharacterChaosChecker chaosChecker;

        /// <summary>
        /// カオス化済み市民一覧
        /// </summary>
        private HashSet<int> chaosedPedList = new HashSet<int>();
        private List<uint> coroutineIds = new List<uint>(); 

        /// <summary>
        /// 周辺市民
        /// </summary>
        private Ped[] cachedPedForChaos = new Ped[0];

        /// <summary>
        /// WeaponProvider
        /// </summary>
        private IWeaponProvider weaponProvider;

        /// <summary>
        /// 設定
        /// </summary>
        private ChaosModeSetting chaosModeSetting;

        private MissionCharacterTreatmentType currentTreatType =
            MissionCharacterTreatmentType.ExcludeUniqueCharacter;
        private MissionCharacterTreatmentType nextTreatType;


        protected override int TickInterval => 1000;

        protected override void Setup()
        {
            var chaosSettingLoader = new ChaosModeSettingLoader();
            var chaosModeSetting = chaosSettingLoader.LoadSettingFile(@"/scripts/chaosmode/default.conf");

            chaosChecker = new CharacterChaosChecker(chaosModeSetting.DefaultMissionCharacterTreatment,
                chaosModeSetting.IsChangeMissionCharacterWeapon);

            weaponProvider = new CustomWeaponProvider(chaosModeSetting.WeaponList, chaosModeSetting.WeaponListForDriveBy);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive.Value = !_isActive.Value;
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                    if (_isActive.Value)
                    {
                        DrawText("ChaosMode:On/" + currentTreatType.ToString(), 3.0f);
                    }
                    else
                    {
                        DrawText("ChaosMode:Off", 3.0f);
                    }

                });

            nextTreatType = currentTreatType;

            //F7でキャラカオスの切り替え（暫定
            OnKeyDownAsObservable
                .Where(x=> _isActive.Value && x.KeyCode == Keys.F7)
                .Do(_ =>
                {
                   nextTreatType = (MissionCharacterTreatmentType)(((int)nextTreatType + 1) % 3);
                    DrawText("CharacterChaos:" + nextTreatType.ToString(), 1.1f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    currentTreatType = nextTreatType;
                    chaosChecker.MissionCharacterTreatment = nextTreatType;
                    DrawText("CharacterChaos:" + currentTreatType.ToString() + "[OK]", 3.0f);
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                });

            //interval設定
            Interval = chaosModeSetting.Interval;

            //interval間隔で市民をカオス化する
            OnTickAsObservable
                .Where(_ => _isActive.Value)
                .Subscribe(_ => CitizenChaos());

            //プレイヤが死んだらリセット
            OnTickAsObservable
                .Select(_ => this.GetPlayer().IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                });
        }


        private void CitizenChaos()
        {
            //まだ処理をしていない市民に対してコルーチンを回す
            cachedPedForChaos = World.GetNearbyPeds(this.GetPlayer(), chaosModeSetting.Radius);
            foreach (var ped in cachedPedForChaos.Where(x =>x.IsSafeExist() && !chaosedPedList.Contains(x.Handle)))
            {
                chaosedPedList.Add(ped.Handle);
                var id = StartCoroutine(ChaosPedAction(ped));
                coroutineIds.Add(id);
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
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object>  ChaosPedAction(Ped ped)
        {
            var pedId = ped.Handle;
            
            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            Weapon equipedWeapon = GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
               yield break;
            }

            if (!ped.IsRequiredForMission())
            {
                ped.MaxHealth = 2000;
                ped.Health = 2000;
                ped.SetPedFiringPattern((int) FiringPattern.FullAuto);
                ped.SetPedShootRate(100);
                ped.Accuracy = chaosModeSetting.ShootAccuracy;
                ped.SetAlertness(0);
                ped.SetCombatAbility(100);
                ped.SetCombatRange(100);
                ped.RegisterHatedTargetsAroundPed(100);
                ped.SetFleeAttributes(0, 0);
                ped.SetCombatAttributes(17, 1);
            }

            //以下ループ
            do
            {
                if (!ped.IsSafeExist())
                {
                    yield break;
                }

                if (!chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                }

                //武器を変更する
                if (Random.Next(0, 100) < chaosModeSetting.WeaponChangeProbabillity)
                {
                    equipedWeapon = GiveWeaponTpPed(ped);
                }
                //攻撃する
                PedRiot(ped, equipedWeapon);

                //適当に待機
                yield return WaitForSeconds(1 + (float) Random.NextDouble()*5);

            } while (ped.IsSafeExist() && ped.IsAlive);

            chaosedPedList.Remove(pedId);
        }

        /// <summary>
        /// カオス化時の攻撃対象を取得する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private Ped GetTargetPed(Ped ped)
        {
            //プレイヤへの攻撃補正が設定されているならプレイヤを攻撃対象にする
            if (chaosModeSetting.IsAttackPlayerCorrectionEnabled &&
                Random.Next(0, 100) < chaosModeSetting.AttackPlayerCorrectionProbabillity)
            {
                return this.GetPlayer();
            }

            //周辺市民からランダムに選ぶ
            var nearPeds =
                cachedPedForChaos.Concat(new Ped[] { this.GetPlayer() }).Where(
                    x => x.IsSafeExist() && !x.IsSameEntity(ped) && x.IsAlive && (ped.Position - x.Position).Length() < 50)
                    .ToArray();

            if (nearPeds.Length == 0)
            {
                return null;
            }
            var randomindex = Random.Next(nearPeds.Length);
            return nearPeds[randomindex];
        }

        /// <summary>
        /// 市民を暴徒化する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="equipWeapon">装備中の武器（最終的には直接取得できるようにしたい)</param>
        private void PedRiot(Ped ped, Weapon equipWeapon)
        {
            try
            {
                var target = GetTargetPed(ped);
                if(!target.IsSafeExist()) return;

                ped.Task.ClearAll();

                if (ped.IsInVehicle())
                {
                    //TODO:車から投擲物を投げる方法を調べる
                   ped.TaskDriveBy(target,FiringPattern.BurstFireDriveby);
                }
                else
                {
                    if (weaponProvider.IsProjectileWeapon(equipWeapon))
                    {
                        ped.ThrowProjectile(target.Position);
                    }
                    else if (weaponProvider.IsShootWeapon(equipWeapon))
                    {
                        if (chaosModeSetting.IsStupidShooting)
                        {
                            //IsStupidShootingならその場で射撃する
                            ped.Task.ShootAt(target, 10000);
                        }
                        else
                        {
                            ped.Task.FightAgainst(target, 1000);
                        }
                    }
                    else
                    {
                        ped.Task.FightAgainst(target, 1000);
                    }
                }
                ped.SetPedKeepTask(true);
                ped.TaskSetBlockingOfNonTemporaryEvents(true);
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
        private Weapon GiveWeaponTpPed(Ped ped)
        {
            try
            {
                if (!ped.IsSafeExist()) return Weapon.UNARMED;
                //市民の武器を変更して良いか調べる
                if(!chaosChecker.IsPedChangebalWeapon(ped)) return Weapon.UNARMED;

                //車に乗っているなら車用の武器を渡す
                var weapon =  ped.IsInVehicle()
                    ? weaponProvider.GetRandomDriveByWeapon()
                    : weaponProvider.GetRandomWeaponExcludeClosedWeapon();

                var weaponhash = (int)weapon;

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(weaponhash, 1000); //指定武器所持
                ped.EquipWeapon(weaponhash); //武器装備
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
