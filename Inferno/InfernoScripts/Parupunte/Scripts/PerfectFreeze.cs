﻿using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using System.Reactive.Linq;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("パーフェクトフリーズ", "おわり")]
    [ParupunteIsono("ぱーふぇくとふりーず")]
    class PerfectFreeze : ParupunteScript
    {
        public PerfectFreeze(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        private HashSet<Entity> freezedEntities = new HashSet<Entity>();

        private readonly float FreezeRange = 60;

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                ParupunteEnd();
            });

            this.OnUpdateAsObservable
                .Subscribe(_ =>
                {
                    var playerPos = core.PlayerPed.Position;
                    var playerVehicle = core.GetPlayerVehicle();


                    #region Ped
                    foreach (var p in core.CachedPeds.Where(
                        x => x.IsSafeExist()
                        && !freezedEntities.Contains(x)
                        && x.IsInRangeOf(playerPos, FreezeRange)
                        && x.IsAlive
                        && x != playerVehicle))
                    {
                        p.FreezePosition = true;
                        freezedEntities.Add(p);
                    }
                    #endregion

                    #region Vehicle
                    foreach (var v in core.CachedVehicles.Where(
                        x => x.IsSafeExist()
                        && !freezedEntities.Contains(x)
                        && x.IsInRangeOf(playerPos, FreezeRange)
                        && x.IsAlive
                        && x != playerVehicle))
                    {
                        v.FreezePosition = true;
                        freezedEntities.Add(v);
                    }
                    #endregion

                });

            //プレイヤ車両は除外
            core.PlayerVehicle.Where(x => x.IsSafeExist())
                .Subscribe(x => x.FreezePosition = false);

            //終了時に全て解除
            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    foreach (var x in freezedEntities.Where(x => x.IsSafeExist()))
                    {
                        x.FreezePosition = false;
                    }
                });
        }
    }
}
