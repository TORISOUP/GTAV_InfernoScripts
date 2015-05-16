
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

        public static Ped GetPlayer(this Script script)
        {
            return Game.Player.Character;
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
            return vehicle == Game.Player.Character.CurrentVehicle;
        }

        /// <summary>
        /// 同じEntityであるかチェックする
        /// </summary>
        public static bool IsSameEntity(this Entity x, Entity y)
        {
            if (x == null || y == null) return false;
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
            return vector + randomBaseVector*(float) Random.NextDouble()*radius;
        }
    }
}
