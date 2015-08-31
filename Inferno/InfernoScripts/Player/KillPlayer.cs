using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno
{
    /// <summary>
    /// 自殺する
    /// </summary>
    class KillPlayer:InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("killme")
                .Subscribe(_ =>
                {
                    World.AddExplosion(playerPed.Position, GTA.ExplosionType.SmallExplosion1, 10.0f, 0.1f);
                    playerPed.Kill();
                    //自殺コマンドで死んだときはランダムな方向にふっとばす
                    var x = Random.NextDouble() - 0.5;
                    var y = Random.NextDouble() - 0.5;
                    var z = Random.NextDouble() - 0.5;
                    var randomVector = new Vector3((float)x, (float)y, (float)z);
                    randomVector.Normalize();
                    playerPed.ApplyForce(randomVector*100);
                });
        }
    }
}
