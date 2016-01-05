using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA; using UniRx;
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
                    World.AddExplosion(PlayerPed.Position, GTA.ExplosionType.Grenade, 10.0f, 0.1f);
                    PlayerPed.Kill();
                    //自殺コマンドで死んだときはランダムな方向にふっとばす
                    var x = Random.NextDouble() - 0.5;
                    var y = Random.NextDouble() - 0.5;
                    var z = Random.NextDouble() - 0.5;
                    var randomVector = new Vector3((float)x, (float)y, (float)z);
                    randomVector.Normalize();
                    PlayerPed.ApplyForce(randomVector*100);
                });
        }
    }
}
