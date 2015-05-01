using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using GTA;

namespace Inferno
{
    public abstract class InfernoScript : Script
    {

        private Ped[] _cachedPeds = new Ped[0];
        private Vehicle[] _cachedVehicles = new Vehicle[0];

        protected Random random;

        /// <summary>
        /// プレイヤ周辺の市民
        /// </summary>
        public ReadOnlyCollection<Ped> CachedPeds { get { return Array.AsReadOnly(_cachedPeds); } }
       
        /// <summary>
        /// プレイヤ周辺の車両
        /// </summary>
        public ReadOnlyCollection<Vehicle> CachedVehicles { get { return Array.AsReadOnly(_cachedVehicles); } }


        protected InfernoScript()
        {
            InfernoCore.PedsNearPlayer.Subscribe(x => _cachedPeds = x);
            InfernoCore.VehicleNearPlayer.Subscribe(x => _cachedVehicles = x);
            Setup();
        }

        protected abstract void Setup();

        /// <summary>
        /// 入力文字列に応じて反応するIObservableを生成する
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        protected IObservable<Unit> CreateInputKeywordAsObservable(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new Exception("Keyword is empty.");
            }

            return InfernoCore.OnKeyDownAsObservable
                .Select(e => e.KeyCode.ToString())
                .Buffer(keyword.Length, 1)
                .Select(x => x.Aggregate((p, c) => p + c))
                .Where(x => x == keyword.ToUpper())
                .Select(_=>Unit.Default)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// 100ms単位でのTickイベントを生成する
        /// </summary>
        /// <param name="millsecond"></param>
        /// <returns></returns>
        protected IObservable<Unit> CreateTickAsObservable(int millsecond)
        {
            var skipCount = (millsecond/100) - 1;

            if (skipCount <= 0)
            {
                return InfernoCore.OnTickAsObservable;
            }

            return InfernoCore
                .OnTickAsObservable
                .Skip(skipCount)
                .Take(1)
                .Repeat()
                .Publish().RefCount();
        }


    }
}
