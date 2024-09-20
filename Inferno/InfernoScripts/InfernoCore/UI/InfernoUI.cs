using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.InfernoCore.UI
{
    public sealed class InfernoUi : Script
    {
        private readonly ObjectPool _objectPool = new();
        private CompositeDisposable _compositeDisposable = new();
        readonly NativeMenu _rootMenu = new NativeMenu("Inferno MOD", "Inferno", "Inferno");

        public InfernoUi()
        {
            #region Inialize

            Interval = 0;
            _objectPool.Add(_rootMenu);
            var keyword = "inferno";
            Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                    h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .Select(e => e.KeyCode.ToString())
                .Buffer(keyword.Length, 1)
                .Select(x => x.Aggregate((p, c) => p + c))
                .Where(x => x == keyword.ToUpper())
                .Select(_ => Unit.Default)
                .Take(1)
                .Repeat()
                .Subscribe(_ => { _rootMenu.Visible = !_rootMenu.Visible; })
                .AddTo(_compositeDisposable);

            Tick += (_, _) => OnTick();
            Aborted += (_, _) => { _compositeDisposable.Dispose(); };

            #endregion
            
            
        }

        public static InfernoUi Instance { get; private set; }

        private void OnTick()
        {
            _objectPool.Process();
        }
    }
}