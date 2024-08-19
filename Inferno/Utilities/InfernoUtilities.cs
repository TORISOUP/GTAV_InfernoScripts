using System;
using System.Threading.Tasks;
using GTA.Math;

namespace Inferno.Utilities
{
    //便利関数群
    public static class InfernoUtilities
    {
        private static readonly Random random;

        static InfernoUtilities()
        {
            random = new Random();
        }

        /// <summary>
        /// ランダムな方向のベクトルを生成する
        /// </summary>
        public static Vector3 CreateRandomVector()
        {
            var x = random.NextDouble() - 0.5;
            var y = random.NextDouble() - 0.5;
            var z = random.NextDouble() - 0.5;
            var randomVector = new Vector3((float)x, (float)y, (float)z);
            randomVector.Normalize();
            return randomVector;
        }
    }

    public static class TaskExtension
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}