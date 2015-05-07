using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno
{

    public static class NativeFunctions
    {
        public static bool IsGamePadPressed(this Script script, GameKey gameKey)
        {
            return Function.Call<bool>(Hash.IS_CONTROL_PRESSED, new InputArgument[2] { 0, (int)gameKey });
        }

        /// <summary>
        /// プレイヤーIDを取得
        /// </summary>
        public static Player GetPlayerId()
        {
            return Function.Call<Player>(Hash.PLAYER_ID);
        }

        /// <summary>
        /// プレイヤーがラグドール状態になれるかどうか
        /// </summary>
        public static void CanPlayerControlRagdoll(this Player player, bool CanControlRagdoll)
        {
            Function.Call(Hash.GIVE_PLAYER_RAGDOLL_CONTROL, player, CanControlRagdoll);
        }

        /// <summary>
        /// ラグドール状態にする 
        /// </summary>
        /// <param name="Xforce">X軸方向の力</param>
        /// <param name="Yforce">Y軸方向の力</param>
        /// <param name="Zforce">Z軸方向の力</param>
        public static void SetPedToRagdoll(this Ped ped, int Xforce, int Yforce, int Zforce)
        {
            Function.Call(Hash.SET_PED_TO_RAGDOLL, new InputArgument[]
            {
                ped,
                Xforce,
                Yforce,
                Zforce,
                0, 0, 0,
            });
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
        /// 指定座標に攻撃する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="position">座標</param>
        /// <param name="duration">攻撃時間[ms]</param>
        public static void TaskShootAtCoord(this Ped ped,Vector3 position,int duration)
        {
            Function.Call(Hash.TASK_SHOOT_AT_COORD,new InputArgument[]
            {
                ped,
                position.X,
                position.Y,
                position.Z,
                duration
            });
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
        public static void SetPedFiringPattern(this Ped ped, int pattern){
            Function.Call(Hash.SET_PED_FIRING_PATTERN,new InputArgument[]{ ped, pattern});
        }

        public static void SetPedShootRate(this Ped ped, int shootRate)
        {
            Function.Call(Hash.SET_PED_SHOOT_RATE, new InputArgument[] {ped, shootRate});
        }

        public static void DestroyEntity(this Entity entity)
        {
          //  Function.Call(Hash.DELETE_ENTITY, new InputArgument[] {entity});
        }

        /// <summary>
        /// 車両に乗り込む
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="vehicle">車両</param>
        /// <param name="timeout">タイムアウト[ms] この秒数以上かかった場合は車内にワープする</param>
        /// <param name="vehicleSeat">座席</param>
        public static void TaskEnterVehicle(this Ped ped, Vehicle vehicle, int timeout, VehicleSeat vehicleSeat)
        {
            Function.Call(Hash.TASK_ENTER_VEHICLE, new InputArgument[]
            {
                ped,
                vehicle,
                timeout,
                (int) vehicleSeat,
                1, 1, 0
            });
        }

        /// <summary>
        /// 車両の座席にいる人を取得する
        /// </summary>
        /// <param name="vehicle">車両</param>
        /// <param name="vehicleSeat">座席</param>
        /// <returns></returns>
        public static Ped GetPedInVehicleSeat(this Vehicle vehicle, VehicleSeat vehicleSeat)
        {
            return Function.Call<Ped>(Hash.GET_PED_IN_VEHICLE_SEAT, new InputArgument[]
            {
                vehicle,
                (int) vehicleSeat,
            });
        }

        /// <summary>
        /// 最大運転速度を設定 
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="MaxDriveSpeed">最大運転速度</param>
        public static void SetMaxDriveSpeed(this Ped ped, float MaxDriveSpeed)
        {
            Function.Call(Hash.SET_DRIVE_TASK_MAX_CRUISE_SPEED, new InputArgument[]
            {
                 ped,
                 MaxDriveSpeed,
            });
        }

        /// <summary>
        /// 運転速度を設定 
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="DriveSpeed">運転速度</param>
        public static void SetDriveSpeed(this Ped ped, float DriveSpeed)
        {
            Function.Call(Hash.SET_DRIVE_TASK_CRUISE_SPEED, new InputArgument[]
            {
                 ped,
                 DriveSpeed,
            });
        }

        /// <summary>
        /// 対象がミッション用のエンティティか
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsRequiredForMission(this Entity entity)
        {
            return Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, new InputArgument[] {entity});
        }

        /// <summary>
        /// テキストのフォント指定
        /// </summary>
        /// <param name="script"></param>
        /// <param name="font">フォント指定</param>
        public static void SetTextFont(this Script script, int font)
        {
            Function.Call(Hash.SET_TEXT_FONT, new InputArgument[] { font });
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
                Red,        //R?
                Green,      //G?
                Blue,       //B?
                Alpha       //A?
            });
        }

        /// <summary>
        /// 文字を中央寄せにする
        /// </summary>
        /// <param name="script"></param>
        /// <param name="isCenter">false:左寄せ？true:中央寄せ</param>
        public static void SetTextCentre(this Script script, bool isCenter)
        {
            Function.Call(Hash.SET_TEXT_CENTRE, new InputArgument[] { isCenter ? 1 : 0 });
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
            Function.Call(Hash._SET_TEXT_ENTRY, new InputArgument[] { "STRING" });
            //テキストとして表示する文字列
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, new InputArgument[] { str });
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

        /// <summary>
        /// 対象のタスクを全てキャンセルする
        /// </summary>
        /// <param name="ped"></param>
        public static void ClearTasksImmediately(this Ped ped)
        {
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, new InputArgument[] {ped});
        }

        public static void SetToRagdDoll(this Ped ped,float forceX=0,float forceY=0,float forceZ=0)
        {
            Function.Call(Hash.SET_PED_TO_RAGDOLL,new InputArgument[]
            {
                ped,forceX,forceY,forceZ,true,true,true
            });
        }
        }
}
