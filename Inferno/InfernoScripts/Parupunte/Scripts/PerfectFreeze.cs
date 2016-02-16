using System.Collections.Generic;
using System.Linq;
using GTA;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(false, true)]
    class PerfectFreeze : ParupunteScript
    {
        public PerfectFreeze(ParupunteCore core) : base(core)
        {
        }

        private HashSet<Entity> freezedEntities = new HashSet<Entity>();

        public override string Name { get; } = "パーフェクトフリーズ";
        public override string EndMessage { get; } = "おわり";
        private readonly float FreezeRange = 30;

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
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

                    #region props
                    var props = GTA.World.GetAllProps();
                    foreach (var prop in props.Where(x =>
                        x.IsSafeExist()
                        && !freezedEntities.Contains(x)
                        && x.IsInRangeOf(playerPos, FreezeRange)
                    ))
                    {
                        prop.FreezePosition = true;
                        freezedEntities.Add(prop);
                    }
                    #endregion

                    //離れていたら解除
                    var deleteTargets = freezedEntities.FirstOrDefault(x =>
                        x.IsSafeExist() && !x.IsInRangeOf(playerPos, FreezeRange + 5));

                    if (deleteTargets != null)
                    {
                        deleteTargets.FreezePosition = false;
                        freezedEntities.Remove(deleteTargets);
                    }

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
