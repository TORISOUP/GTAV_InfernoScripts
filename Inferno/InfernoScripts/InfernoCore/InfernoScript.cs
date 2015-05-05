using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GTA;

namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプトの基底
    /// </summary>
    public abstract class InfernoScript : Script
    {
        protected Random Random = new Random();

        private Ped[] _cachedPeds = new Ped[0];
        /// <summary>
        /// キャッシュされたプレイヤ周辺の市民
        /// </summary>
        public ReadOnlyCollection<Ped> CachedPeds { get { return Array.AsReadOnly(_cachedPeds??new Ped[0]); } }

        private Vehicle[] _cachedVehicles = new Vehicle[0];
        /// <summary>
        /// キャッシュされたプレイヤ周辺の車両
        /// </summary>
        public ReadOnlyCollection<Vehicle> CachedVehicles { get { return Array.AsReadOnly(_cachedVehicles ?? new Vehicle[0]); } }

        /// <summary>
        /// 一定間隔のTickイベント
        /// </summary>
        public IObservable<Unit> OnTickAsObservable { get; private set; } 

        /// <summary>
        /// スクリプトのTickイベントの実行頻度[ms]
        /// </summary>
        protected virtual int TickInterval { get { return 1000; } }

        /// <summary>
        /// テキスト表示
        /// </summary>
        /// <param name="str">表示したい文字列</param>
        public void DrawText(string str)
        {
            InfernoCore core = InfernoCore.Instance;
            core.SetDrawText(str);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected InfernoScript()
        {
            //キャッシュが変更されたら反映する
            InfernoCore.PedsNearPlayer.Subscribe(x => _cachedPeds = x);
            InfernoCore.VehicleNearPlayer.Subscribe(x => _cachedVehicles = x);

            //TickイベントをObservable化しておく
            Interval = TickInterval;
            OnTickAsObservable =
                Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h,
                    Scheduler.Immediate)
                    .Select(_ => Unit.Default).Publish().RefCount(); //Subscribeされたらイベントハンドラを登録する

            Setup();
        }

        /// <summary>
        /// 初期化処理はここに書く
        /// </summary>
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
                .Where(x => x == keyword.ToUpper()) //入力文字列を比較
                .Select(_=>Unit.Default)
                .Take(1).Repeat() //1回動作したらBufferをクリア
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// 100ms単位でのTickイベントをInfernoCore.OnTickAsObservableから生成する
        /// </summary>
        /// <param name="millsecond">ミリ秒(100ミリ秒単位で指定）</param>
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

        protected uint StartCoroutine(IEnumerator coroutine)
        {
          return InfernoCore.Instance.AddCrotoutine(coroutine);
        }

        /// <summary>
        /// 指定秒数待機するIEnumerable
        /// </summary>
        /// <param name="secound"></param>
        /// <returns></returns>
        protected IEnumerable WaitForSecond(float secound)
        {
            var waitLoopCount = (int)(secound * 10);
            for (var i = 0; i < waitLoopCount; i++)
            {
                yield return i;
            }
        }


        /// <summary>
        /// ログをTCPSocker経由で吐く
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogWrite(string message)
        {
#if DEBUG
            InfernoCore.Instance.LogWrite(message + "\r\n");
#endif
        }
    }
}
