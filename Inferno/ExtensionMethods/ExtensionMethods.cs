
using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

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
            var randomBaseVector = new Vector3((float) Random.NextDouble() - 0.5f, (float) Random.NextDouble() - 0.5f, 0);
            randomBaseVector.Normalize();
            return vector + randomBaseVector*(float) Random.NextDouble()*radius;
        }

        /// <summary>
        /// カットシーン専用のキャラであるか
        /// </summary>
        public static bool IsCutsceneOnlyPed(this Ped ped)
        {
            return Enum.IsDefined(typeof (CutSceneOnlyPedHash), (CutSceneOnlyPedHash) ped.Model.Hash);
        }

        public static Vector3 ApplyVector(this Quaternion q, Vector3 v)
        {

            var w = -q.X*v.X - q.Y*v.Y - q.Z*v.Z;
            var x = q.Y*v.Z - q.Z*v.Y + q.W*v.X;
            var y = q.Z*v.X - q.X*v.Z + q.W*v.Y;
            var z = q.X*v.Y - q.Y*v.X + q.W*v.Z;
            return new Vector3(
                y*-q.Z + z*q.Y - w*q.X + x*q.W,
                z*-q.X + x*q.Z - w*q.Y + y*q.W,
                x*-q.Y + y*q.X - w*q.Z + z*q.W
                );
        }
    }
}
