using GTA;
using GTA.Math;
using System;

namespace Inferno
{
    public static class ExtensionMethods
    {
        private static Random _random;

        private static Random Random
        {
            get { return _random ?? (_random = new Random()); }
        }

        public static Vehicle GetPlayerVehicle(this Script script)
        {
            var player = Game.Player.Character;
            return player.IsInVehicle() ? player.CurrentVehicle : null;
        }

        public static bool IsSafeExist(this Entity entity)
        {
            return entity != null && Entity.Exists(entity);
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
            if (!x.IsSafeExist() || !y.IsSafeExist()) return false;
            return x.Handle == y.Handle;
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

            Vector3 vector = Vector3.Normalize(forward);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            var m00 = vector2.X;
            var m01 = vector2.Y;
            var m02 = vector2.Z;
            var m10 = vector3.X;
            var m11 = vector3.Y;
            var m12 = vector3.Z;
            var m20 = vector.X;
            var m21 = vector.Y;
            var m22 = vector.Z;

            float num8 = (m00 + m11) + m22;
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
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }
    }
}
