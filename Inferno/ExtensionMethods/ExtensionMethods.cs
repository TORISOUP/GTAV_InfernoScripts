using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;

namespace Inferno
{
    public static class ExtensionMethods
    {
        private static Random _random;

        private static Random Random => _random ?? (_random = new Random());

        public static Vehicle GetPlayerVehicle(this Script script)
        {
            var player = Game.Player.Character;
            return player.IsInVehicle() ? player.CurrentVehicle : null;
        }

        public static bool IsSafeExist(this Entity entity)
        {
            return entity != null && entity.Exists();
        }

        /// <summary>
        /// プレイヤが乗車している車両であるか
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static bool IsPlayerVehicle(this Vehicle vehicle)
        {
            if (!vehicle.IsSafeExist())
            {
                return false;
            }

            return vehicle == Game.Player.Character.CurrentVehicle;
        }

        /// <summary>
        /// 同じEntityであるかチェックする
        /// </summary>
        public static bool IsSameEntity(this Entity x, Entity y)
        {
            if (!x.IsSafeExist() || !y.IsSafeExist())
            {
                return false;
            }

            return x.Handle == y.Handle;
        }

        /// <summary>
        /// ターゲットへ向かう正規化したベクトルを返す
        /// </summary>
        public static Vector3 To(this Vector3 origin, Vector3 target)
        {
            return (target - origin).Normalized();
        }

        /// <summary>
        /// 正規化する
        /// </summary>
        public static Vector3 Normalized(this Vector3 origin)
        {
            var copy = origin;
            copy.Normalize();
            return copy;
        }

        /// <summary>
        /// 平面上でランダムな座標を加えた座標を返す
        /// </summary>
        /// <param name="vector">元座標</param>
        /// <param name="radius">ランダム範囲</param>
        /// <returns>ランダムな座標</returns>
        public static Vector3 AroundRandom2D(this Vector3 vector, float radius)
        {
            var randomBaseVector = new Vector3((float)Random.NextDouble() - 0.5f, (float)Random.NextDouble() - 0.5f, 0);
            randomBaseVector.Normalize();
            return vector + randomBaseVector * (float)Random.NextDouble() * radius;
        }

        /// <summary>
        /// カットシーン専用のキャラであるか
        /// </summary>
        public static bool IsCutsceneOnlyPed(this Ped ped)
        {
            return Enum.IsDefined(typeof(CutSceneOnlyPedHash), (CutSceneOnlyPedHash)ped.Model.Hash);
        }

        public static Vector3 GetBonePosition(this Ped ped, Bone boneIndex)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_BONE_POSTION, ped.Handle, (int)boneIndex);
        }

        public static void FreezePosition(this Entity entity, bool freeze)
        {
            Function.Call(Hash.FREEZE_ENTITY_POSITION, entity.Handle, freeze);
        }

        public static bool IsFloating(this Entity entity, float thresholdFromGround = 0.5f)
        {
            var groundZ = World.GetGroundHeight(entity.Position);
            var offset = entity is Ped ? 1.0f : 0f;
            return entity.Position.Z - offset - groundZ > thresholdFromGround;
        }

        public static bool IsInRangeOf(this Entity entity, Vector3 position, float distance)
        {
            return entity.Position.DistanceTo(position) < distance;
        }

        public static bool IsInRangeOfIgnoreZ(this Entity entity, Vector3 position, float distance)
        {
            var e = new Vector3(entity.Position.X, entity.Position.Y, 0);
            var p = new Vector3(position.X, position.Y, 0);

            return e.DistanceTo(p) < distance;
        }

        public static void SetForwardSpeed(this Vehicle v, float value)
        {
            if (v.Model.IsTrain)
            {
                Function.Call(Hash.SET_TRAIN_SPEED, v.Handle, value);
                Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, v.Handle, value);
            }
            else
            {
                Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, v.Handle, value);
            }
        }

        public static Vector3 ApplyVector(this Quaternion q, Vector3 v)
        {
            var w = -q.X * v.X - q.Y * v.Y - q.Z * v.Z;
            var x = q.Y * v.Z - q.Z * v.Y + q.W * v.X;
            var y = q.Z * v.X - q.X * v.Z + q.W * v.Y;
            var z = q.X * v.Y - q.Y * v.X + q.W * v.Z;
            return new Vector3(
                y * -q.Z + z * q.Y - w * q.X + x * q.W,
                z * -q.X + x * q.Z - w * q.Y + y * q.W,
                x * -q.Y + y * q.X - w * q.Z + z * q.W
            );
        }

        public static Quaternion ToQuaternion(this Vector3 forward)
        {
            return ToQuaternion(forward, Vector3.WorldUp);
        }

        public static Quaternion ToQuaternion(this Vector3 forward, Vector3 up)
        {
            forward.Normalize();

            var vector = Vector3.Normalize(forward);
            var vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
            var vector3 = Vector3.Cross(vector, vector2);
            var m00 = vector2.X;
            var m01 = vector2.Y;
            var m02 = vector2.Z;
            var m10 = vector3.X;
            var m11 = vector3.Y;
            var m12 = vector3.Z;
            var m20 = vector.X;
            var m21 = vector.Y;
            var m22 = vector.Z;

            var num8 = m00 + m11 + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }

            if (m00 >= m11 && m00 >= m22)
            {
                var num7 = (float)Math.Sqrt(1f + m00 - m11 - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }

            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(1f + m11 - m00 - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }

            var num5 = (float)Math.Sqrt(1f + m22 - m00 - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        public static PedType GetPedType(this Ped p)
        {
            return Function.Call<PedType>(Hash.GET_PED_TYPE, p.Handle);
        }

        // 追従・同行を示しやすいタスク一覧（必要に応じて拡張）
        private static readonly HashSet<Hash> FollowishTasks = new HashSet<Hash>
        {
            Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY,
            Hash.TASK_GO_TO_ENTITY,
            Hash.TASK_ENTER_VEHICLE,
            Hash.TASK_GOTO_ENTITY_OFFSET,
            Hash.TASK_VEHICLE_ESCORT, // 同行の車列
        };

        private static bool IsTaskActive(Ped ped, Hash taskHash)
        {
            var status = Function.Call<int>(Hash.GET_SCRIPT_TASK_STATUS, ped.Handle, taskHash);
            // 0: PENDING, 1: IN_PROGRESS, 7: FINISHED（ことが多い）
            return status == 0 || status == 1;
        }


        public static bool MayBeFriend(this Ped ped)
        {
            if (!ped.IsSafeExist()) return false;

            var player = Game.Player;

            var playerPed = Game.Player.Character;
            // 同乗（プレイヤー運転車に同乗)
            if (ped.IsInVehicle() && playerPed.IsInVehicle() && ped.CurrentVehicle == playerPed.CurrentVehicle)
            {
                return true;
            }

            var playerGroup = Function.Call<int>(Hash.GET_PLAYER_GROUP, player.Handle);
            if (Function.Call<bool>(Hash.IS_PED_GROUP_MEMBER, ped.Handle, playerGroup))
            {
                // 同じグループ
                return true;
            }
            
            // プレイヤーとの関係（Companion/Respect/Like）
            int rel = Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_PEDS, ped.Handle, player.Handle);
            // SHVDN の Relationship enum: Companion=0, Respect=1, Like=2, Neutral=3, Dislike=4, Hate=5
            if (rel <= 2) return true;

            // 5) ブリップが友好色
            var blip = ped.AttachedBlip;
            if (blip != null && blip.Exists())
            {
                if (blip.Color != BlipColor.Red && blip.Color != BlipColor.Red2 && blip.Color != BlipColor.Red3 &&
                    blip.Color != BlipColor.Red4)
                {
                    return true;
                }
            }

            // 追跡系のタスク実行中か
            foreach (var t in FollowishTasks)
            {
                if (IsTaskActive(ped, t)) return true;
            }

            return false;
        }

        /// <summary>
        /// 敵対すると手配度がつくPed一覧
        /// </summary>
        private static readonly PedHash[] CopPedHas =
        {
            PedHash.Cop01SFY, PedHash.Snowcop01SMM, PedHash.PrologueSec02, PedHash.Highsec02SMM, PedHash.ChemSec01SMM,
            PedHash.Sheriff01SFY, PedHash.JackHowitzerCutscene, PedHash.Prisguard01SMM, PedHash.Marine02SMY,
            PedHash.FibSec01, PedHash.Cop01SMY, PedHash.CiaSec01SMM, PedHash.Armymech01SMY, PedHash.Armoured02SMM,
            PedHash.Marine01SMY, PedHash.PrologueSec01, PedHash.Marine03SMY, PedHash.Hwaycop01SMY,
            PedHash.FibSec01SMM, PedHash.PrologueSec01Cutscene, PedHash.Swat01SMY, PedHash.Armoured01SMM,
            PedHash.Devinsec01SMY, PedHash.Sheriff01SMY, PedHash.Autopsy01SMY, PedHash.Uscg01SMY, PedHash.Security01SMM,
            PedHash.SecuroGuardMale01, PedHash.Ranger01SMY, PedHash.Marine02SMM, PedHash.Highsec01SMM,
            PedHash.Marine01SMM
        };

        /// <summary>
        /// 敵対すると手配度がつくPedであるかどうか
        /// </summary>
        /// <param name="ped"></param>
        public static bool IsCop(this Ped ped)
        {
            return ped.GetPedType() switch
            {
                PedType.PED_TYPE_COP => true,
                PedType.PED_TYPE_SWAT => true,
                PedType.PED_TYPE_ARMY => true,
                _ => CopPedHas.Contains((PedHash)ped.Model.Hash)
            };
        }

        #region Weapon

        /// <summary>
        /// 射撃系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static bool IsShootWeapon(this Weapon weapon)
        {
            return ChaosModeWeapons.ShootWeapons.Contains(weapon);
        }

        /// <summary>
        /// 近接系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static bool IsCloseWeapon(this Weapon weapon)
        {
            return ChaosModeWeapons.ClosedWeapons.Contains(weapon);
        }

        /// <summary>
        /// 投擲系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static bool IsProjectileWeapon(this Weapon weapon)
        {
            return ChaosModeWeapons.ProjectileWeapons.Contains(weapon);
        }

        #endregion
    }
}