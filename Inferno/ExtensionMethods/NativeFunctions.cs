using System;
using System.Runtime.InteropServices;
using System.Security.Policy;
using GTA;
using GTA.Math;
using GTA.Native;
using Hash = GTA.Native.Hash;

namespace Inferno
{

    public static class NativeFunctions
    {
        public static bool IsGamePadPressed(this Script script, GameKey gameKey)
        {
            return Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, (int) gameKey);
        }

        /// <summary>
        /// 死亡時に武器を落とすかどうか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="isDrop">false:落とさない true:落とす</param>
        public static void SetDropWeaponWhenDead(this Ped ped, bool isDrop)
        {
            Function.Call(Hash.SET_PED_DROPS_WEAPONS_WHEN_DEAD, ped, isDrop ? 1 : 0);
        }

        /// <summary>
        /// 文字列からハッシュ値を取得
        /// </summary>
        /// <param name="script"></param>
        /// <param name="str">文字列（武器名等）</param>
        /// <returns>ハッシュ値</returns>
        public static int GetGTAObjectHashKey(this Script script, string str)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, str);
        }

        /// <summary>
        /// 指定した武器を所持させる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="weapon">武器（ハッシュ値）</param>
        /// <param name="ammunition">弾薬</param>
        public static void GiveWeapon(this Ped ped, int weapon, int ammunition)
        {
            Function.Call(Hash.GIVE_DELAYED_WEAPON_TO_PED, ped, weapon, ammunition, 0);
        }

        /// <summary>
        /// 武器を装備する（先に所持していることが必須?）
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="weapon">武器（ハッシュ値）</param>
        public static void EquipWeapon(this Ped ped, int weapon)
        {
            Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, weapon, true);
        }

        /// <summary>
        /// 該当タスクを実行中か調べる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="task">タスク</param>
        /// <returns>trueで実行中</returns>
        public static bool IsTaskActive(this Ped ped, PedTaskAction task)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, (int) task);
        }

        /// <summary>
        /// 市民のタスクを固定する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="toggle">固定するか</param>
        public static void SetPedKeepTask(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_KEEP_TASK, toggle);
        }


        /// <summary>
        /// 指定座標に攻撃する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="position">座標</param>
        /// <param name="duration">攻撃時間[ms]</param>
        public static void TaskShootAtCoord(this Ped ped, Vector3 position, int duration)
        {
            Function.Call(Hash.TASK_SHOOT_AT_COORD, ped, position.X, position.Y, position.Z, duration);
        }

        /// <summary>
        /// リロード中か
        /// </summary>
        /// <param name="ped">市民</param>
        public static bool IsWeaponReloading(this Ped ped)
        {
            return Function.Call<bool>(Hash.IS_PED_RELOADING, ped);
        }

        /// <summary>
        /// 市民の攻撃パターン
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="pattern">パターン（ハッシュ値）</param>
        public static void SetPedFiringPattern(this Ped ped, int pattern)
        {
            Function.Call(Hash.SET_PED_FIRING_PATTERN, ped, pattern);
        }

        /// <summary>
        /// 市民の所持金をセットする
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="money">金額</param>
        public static void SetPedMoney(this Ped ped, int money)
        {
            Function.Call(Hash.SET_PED_MONEY, ped, money);
        }

        /// <summary>
        /// 市民の所持金を取得する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>金額</returns>
        public static int GetPedMoney(this Ped ped)
        {
            return Function.Call<int>(Hash.GET_PED_MONEY, ped);
        }

        public static void SetPedShootRate(this Ped ped, int shootRate)
        {
            Function.Call(Hash.SET_PED_SHOOT_RATE, ped, shootRate);
        }

        /// <summary>
        /// 指定座標にパラシュート降下する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="target">目標地点</param>
        public static void ParachuteTo(this Ped ped, Vector3 target)
        {
            Function.Call(Hash.TASK_PARACHUTE_TO_TARGET, ped, target.X, target.Y, target.Z);
        }

        /// <summary>
        /// ?
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="target"></param>
        /// <param name="vehicle"></param>
        public static void TaskDriveBy(this Ped ped, Ped target, FiringPattern firingPattern)
        {
            var p = target.Position;
            Function.Call(Hash.TASK_DRIVE_BY, ped, 0, 0, p.X, p.Y, p.Z, 10000.0, 0, 0, (int)firingPattern);
        }

        /// <summary>
        /// 車両に乗り込む
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="vehicle">車両</param>
        /// <param name="timeout">タイムアウト[ms] この秒数以上かかった場合は車内にワープする</param>
        /// <param name="vehicleSeat">座席</param>
        public static void TaskEnterVehicle(this Ped ped, Vehicle vehicle, int timeout, GTA.VehicleSeat vehicleSeat)
        {
            Function.Call(Hash.TASK_ENTER_VEHICLE, ped, vehicle, timeout, (int) vehicleSeat, 1, 1, 0);
        }

        /// <summary>
        /// 市民をドライバとして召喚する
        /// </summary>
        /// <param name="vehicle">乗り物</param>
        /// <returns>生成市民</returns>
        public static Ped CreateRandomPedAsDriver(this Vehicle vehicle)
        {
            return Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, vehicle, true);
        }


        /// <summary>
        /// 警戒心？
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="alertness"></param>
        public static void SetAlertness(this Ped ped, int alertness)
        {
            Function.Call(Hash.SET_PED_ALERTNESS, ped, alertness);
        }

        /// <summary>
        /// 戦闘スキル？
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="ability"></param>
        public static void SetCombatAbility(this Ped ped, int ability)
        {
            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped, ability);
        }

        /// <summary>
        /// 戦闘範囲？
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="range"></param>
        public static void SetCombatRange(this Ped ped, int range)
        {
            Function.Call(Hash.SET_PED_COMBAT_RANGE, ped, range);
        }

        /// <summary>
        /// 攻撃してきた対象を攻撃する？
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="range"></param>
        public static void RegisterHatedTargetsAroundPed(this Ped ped, int range)
        {
            Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, ped, range);
        }

        /// <summary>
        ///　警官とし設定する
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="isPed"></param>
        public static void SetAsCop(this Ped ped, bool isPed)
        {
            Function.Call(Hash.SET_PED_AS_COP, ped, isPed);
        }

        /// <summary>
        /// 戦闘状態にする
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="target"></param>
        /// <param name="unk1"></param>
        /// <param name="unk2"></param>
        public static void TaskCombat(this Ped ped, Ped target, bool unk1, bool unk2)
        {
            Function.Call(Hash.TASK_COMBAT_PED, ped, target, unk1, unk2);
        }


        /// <summary>
        /// 市民をランダムに生成する
        /// </summary>
        /// <param name="position">座標</param>
        /// <returns>生成市民</returns>
        public static Ped CreateRandomPed(Vector3 position)
        {
            return Function.Call<Ped>(Hash.CREATE_RANDOM_PED, position.X, position.Y, position.Z);
        }

        /// <summary>
        /// ライトを生成する（呼び出した瞬間のみ描画される
        /// </summary>
        /// <param name="pos">座標</param>
        /// <param name="red">0-255</param>
        /// <param name="green">0-255</param>
        /// <param name="blue">0-255</param>
        /// <param name="radius">半径？</param>
        /// <param name="intensity">強さ</param>
        public static void CreateLight(Vector3 pos, int red, int green, int blue, float radius, float intensity)
        {
            Function.Call(Hash.DRAW_LIGHT_WITH_RANGE,
                pos.X,
                pos.Y,
                pos.Z,
                red,
                green,
                blue,
                5.0f,
                100.0f);
        }

        public static void ThrowProjectile(this Ped ped, Vector3 vector3)
        {
            Function.Call(Hash.TASK_THROW_PROJECTILE, ped, vector3.X, vector3.Y, vector3.Z);
        }


        /// <summary>
        /// 対象がミッション用のエンティティか
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsRequiredForMission(this Entity entity)
        {
            return Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, entity);
        }

        public static bool HasBeenDamagedBy(this Ped ped, Weapon weapon)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, ped, (int) weapon, false);
        }

        public static bool HasBeenDamagedByPed(this Ped ped, Ped target)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, ped, target, true);
        }


        /// <summary>
        /// テキストのフォント指定
        /// </summary>
        /// <param name="script"></param>
        /// <param name="font">フォント指定</param>
        public static void SetTextFont(this Script script, int font)
        {
            Function.Call(Hash.SET_TEXT_FONT, new InputArgument[] {font});
        }

        /// <summary>
        /// テキストのスケール指定
        /// </summary>
        /// <param name="script"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetTextScale(this Script script, float x, float y)
        {
            Function.Call(Hash.SET_TEXT_SCALE, new InputArgument[]
            {
                x,
                y
            });
        }

        /// <summary>
        /// テキストの文字色
        /// </summary>
        /// <param name="script"></param>
        /// <param name="Red"></param>
        /// <param name="Green"></param>
        /// <param name="Blue"></param>
        /// <param name="Alpha"></param>
        public static void SetTextColour(this Script script, int Red, int Green, int Blue, int Alpha)
        {
            Function.Call(Hash.SET_TEXT_COLOUR, new InputArgument[]
            {
                Red, //R?
                Green, //G?
                Blue, //B?
                Alpha //A?
            });
        }

        /// <summary>
        /// 文字を中央寄せにする
        /// </summary>
        /// <param name="script"></param>
        /// <param name="isCenter">false:左寄せ？true:中央寄せ</param>
        public static void SetTextCentre(this Script script, bool isCenter)
        {
            Function.Call(Hash.SET_TEXT_CENTRE, new InputArgument[] {isCenter ? 1 : 0});
        }

        /// <summary>
        /// テキストの影の色？引数が何を表してるのか要検証
        /// 現状はこのままで
        /// </summary>
        /// <param name="script"></param>
        public static void SetTextDropShadow(this Script script)
        {
            Function.Call(Hash.SET_TEXT_DROPSHADOW, new InputArgument[]
            {
                0,
                0,
                0,
                0,
                0
            });
        }

        /// <summary>
        /// 文字のエッジ 引数要検証
        /// </summary>
        /// <param name="sctipt"></param>
        public static void SetTextEdge(this Script sctipt)
        {
            Function.Call(Hash.SET_TEXT_EDGE, new InputArgument[]
            {
                1,
                0,
                0,
                0,
                205
            });
        }

        /// <summary>
        /// テキストとして表示したい文字列を追加する
        /// </summary>
        /// <param name="script"></param>
        /// <param name="str">表示したい文字列</param>
        public static void AddTextString(this Script script, string str)
        {
            //文字の種類?
            Function.Call(Hash._SET_TEXT_ENTRY, new InputArgument[] {"STRING"});
            //テキストとして表示する文字列
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, new InputArgument[] {str});
        }

        /// <summary>
        /// 指定した位置でテキスト描画
        /// </summary>
        /// <param name="script"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void DrawTextInSetPosition(this Script script, float x, float y)
        {
            Function.Call(Hash._DRAW_TEXT, new InputArgument[]
            {
                x,
                y
            });
        }

        public static unsafe Vector2 GetScreenResolution()
        {
            return new Vector2(UI.WIDTH, UI.HEIGHT);
        }

        /// <summary>
        /// 対象のタスクを全てキャンセルする
        /// </summary>
        /// <param name="ped"></param>
        public static void ClearTasksImmediately(this Ped ped)
        {
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, new InputArgument[] {ped});
        }

        public static void SetToRagdDoll(this Ped ped, float forceX = 0, float forceY = 0, float forceZ = 0)
        {
            Function.Call(Hash.SET_PED_TO_RAGDOLL, new InputArgument[]
            {
                ped, forceX, forceY, forceZ, true, true, true
            });
        }

        public static int GetClockHours()
        {
            return Function.Call<int>(Hash.GET_CLOCK_HOURS);
        }
    }
}
