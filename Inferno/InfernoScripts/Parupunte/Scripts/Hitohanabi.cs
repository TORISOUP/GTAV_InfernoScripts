﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug]
    class Hitohanabi : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public Hitohanabi(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "ひとはなび";

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(5000);
            AddProgressBar(reduceCounter);
            //コルーチン起動
            StartCoroutine(HitohanabiCoroutine());
            
        }

        private IEnumerable<object> HitohanabiCoroutine()
        {

            //プレイや周辺の15m上空を設定
            var targetPosition = core.PlayerPed.Position.Around(20) + new Vector3(0, 0, 15);
            var pedList = new List<Ped>();

            //タイマが終わるまでカウントし続ける
            while(!reduceCounter.IsCompleted)
            {
                foreach (
                    var targetPed in
                        core.CachedPeds.Where(
                            x =>x.IsSafeExist() && x.IsAlive && x.IsHuman && x.IsInRangeOf(core.PlayerPed.Position, 50))
                    )
                {
                    //まだの人をリストにくわえる
                    if (pedList.Count < 30 && !pedList.Contains(targetPed))
                    {
                        pedList.Add(targetPed);
                        if (targetPed.IsInVehicle()) targetPed.Task.ClearAllImmediately();
                        targetPed.CanRagdoll = true;
                        targetPed.SetToRagdoll(5000);
                    }

                }

                foreach (var targetPed in pedList.Where(x=>x.IsSafeExist()))
                {
                    //すいこむ
                    var direction = targetPosition - targetPed.Position;
                    targetPed.FreezePosition = false;
                    var lenght = direction.Length();
                    if (lenght > 1)
                    {
                        direction.Normalize();
                        targetPed.ApplyForce(direction*lenght.Clamp(0, 1)*20);
                    }
                }
                yield return null;
            }

            //バクハツシサン
            foreach (var targetPed in pedList.Where(x => x.IsSafeExist()))
            {
                targetPed.Kill();
                targetPed.ApplyForce(InfernoUtilities.CreateRandomVector() * 10);
            }
            GTA.World.AddExplosion(targetPosition, GTA.ExplosionType.Rocket, 2.0f, 1.0f);

            //終了
            ParupunteEnd();
        } 
    }
}