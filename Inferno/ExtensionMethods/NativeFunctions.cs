using System;
using GTA;
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
        /// 対象がミッション用のエンティティか
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsRequiredForMission(this Entity entity)
        {
            return Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, new InputArgument[] {entity});
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
