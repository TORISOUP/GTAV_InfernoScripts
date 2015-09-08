using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using Inferno.ChaosMode.WeaponProvider;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";
        private CharacterChaosChecker chaosChecker;

        /// <summary>
        /// カオス化済み市民一覧
        /// </summary>
        private HashSet<int> chaosedPedList = new HashSet<int>();
        private List<uint> coroutineIds = new List<uint>(); 

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


        protected override int TickInterval => 100;

        protected override void Setup()
        {
            var chaosSettingLoader = new ChaosModeSettingLoader();
            chaosModeSetting = chaosSettingLoader.LoadSettingFile(@"./scripts/chaosmode/default.conf");

            chaosChecker = new CharacterChaosChecker(chaosModeSetting.DefaultMissionCharacterTreatment,
                chaosModeSetting.IsChangeMissionCharacterWeapon);

            weaponProvider = new CustomWeaponProvider(chaosModeSetting.WeaponList, chaosModeSetting.WeaponListForDriveBy);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                    if (IsActive)
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
                .Where(x=> IsActive && x.KeyCode == Keys.F7)
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

            //市民をカオス化する
            CreateTickAsObservable(1000)
                .Where(_ => IsActive && playerPed.IsSafeExist() && playerPed.IsAlive)
                .Subscribe(_ => CitizenChaos());

            //プレイヤが死んだらリセット
            CreateTickAsObservable(1000)
                .Where(_ => playerPed.IsSafeExist())
                .Select(_ => playerPed.IsDead)
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
            if(!playerPed.IsSafeExist())return;

            //まだ処理をしていない市民に対してコルーチンを回す
            var nearPeds = World.GetNearbyPeds(playerPed, chaosModeSetting.Radius);
            foreach (var ped in nearPeds.Where(x =>x.IsSafeExist() && !chaosedPedList.Contains(x.Handle)))
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
            yield return RandomWait();

            if (!ped.IsSafeExist()) yield break;
            var pedId = ped.Handle;
            
            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            var equipedWeapon = GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
                chaosedPedList.Remove(pedId);
                yield break;
            }

            if (ped.IsSafeExist() && !ped.IsRequiredForMission())
            {
                SetPedStatus(ped);
            }
            //以下ループ
            do
            {
                if (!ped.IsSafeExist() || !playerPed.IsSafeExist())
                {
                    break;
                }

                if (!ped.IsInRangeOf(playerPed.Position, chaosModeSetting.Radius))
                {
                    break;
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

                yield return RandomWait();

                //攻撃する
                PedRiot(ped, equipedWeapon);

                //適当に待機
                yield return WaitForSeconds(3 + (float) Random.NextDouble()*5);

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
            if (!ped.IsSafeExist() || !playerPed.IsSafeExist()) return null;

            //プレイヤへの攻撃補正が設定されているならプレイヤを攻撃対象にする
            if (chaosModeSetting.IsAttackPlayerCorrectionEnabled &&
                Random.Next(0, 100) < chaosModeSetting.AttackPlayerCorrectionProbabillity)
            {
                return playerPed;
            }

            //100m以内の市民
            var aroundPeds =
                CachedPeds.Concat(new[] { playerPed }).Where(
                    x => x.IsSafeExist() && !x.IsSameEntity(ped) && x.IsAlive && ped.IsInRangeOf(x.Position, 100))
                    .ToArray();
                    
            //100m以内の市民のうち、より近い人を選出
            var nearPeds = aroundPeds.OrderBy(x => (ped.Position - x.Position).Length()).Take(5).ToArray();

            if (nearPeds.Length == 0) return null;
            var randomindex = Random.Next(nearPeds.Length);
            return nearPeds[randomindex];
        }

        private void SetPedStatus(Ped ped)
        {
            if(!ped.IsSafeExist()) return;
            //FIBミッションからのコピペ（詳細不明）
            ped.SetCombatAttributes(9,0);
            ped.SetCombatAttributes(1, 0);
            ped.SetCombatAttributes(3, 1);
            ped.SetCombatAttributes(29, 1);

            ped.MaxHealth = 2000;
            ped.Health = 2000;
            ped.SetPedShootRate(100);
            ped.Accuracy = chaosModeSetting.ShootAccuracy;
            //戦闘能力？
            ped.SetCombatAbility(1000);
            //戦闘範囲
            ped.SetCombatRange(1000);
            //攻撃を受けたら反撃する
            ped.RegisterHatedTargetsAroundPed(500);
            //タスクを中断しない
            ped.TaskSetBlockingOfNonTemporaryEvents(false);
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
                if(!ped.IsSafeExist()) return;
                var target = GetTargetPed(ped);
                if(!target.IsSafeExist()) return;
                ped.TaskSetBlockingOfNonTemporaryEvents(false);
                ped.Task.ClearAll();
                ped.SetPedKeepTask(true);
                ped.AlwaysKeepTask = true;
                if (ped.IsInVehicle())
                {
                    //TODO:車から投擲物を投げる方法を調べる
                   ped.TaskDriveBy(target,FiringPattern.BurstFireDriveby);
                }
                else
                {
                    if (chaosModeSetting.IsStupidShooting)
                    {
                        if (weaponProvider.IsProjectileWeapon(equipWeapon))
                        {
                            ped.ThrowProjectile(target.Position);
                        }
                        else if (weaponProvider.IsShootWeapon(equipWeapon))
                        {
                            ped.Task.ShootAt(target, 10000);
                        }
                        else
                        {
                            ped.Task.FightAgainst(target, 60000);
                        }
                    }
                    else
                    {
                        ped.Task.FightAgainst(target, 60000);
                    }
                }
                ped.SetPedFiringPattern((int)FiringPattern.FullAuto);
                ped.SetPedKeepTask(true);
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
