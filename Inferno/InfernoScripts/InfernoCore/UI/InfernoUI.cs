using System;
using System.Collections.Generic;
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
        #region Setup

        private readonly ObjectPool _objectPool = new();
        private readonly CompositeDisposable _compositeDisposable = new();
        readonly NativeMenu _rootMenu = new NativeMenu("Inferno MOD", "MOD Menu");
        private readonly List<IScriptUiBuilder> _builders = new();

        public InfernoUi()
        {
            Instance = this;

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


            // 全部一斉ON
            var allOnItem = new NativeItem("All ON", "All ON");
            allOnItem.Activated += (_, __) =>
            {
                foreach (var builder in _builders)
                {
                    if (builder.CanChangeActive)
                    {
                        builder.IsActive = true;
                    }
                }
            };
            _rootMenu.Add(allOnItem);
        }

        public static InfernoUi Instance { private set; get; }

        private void OnTick()
        {
            _objectPool.Process();
        }

        #endregion

        public void RegisterInfernoBuilder(IScriptUiBuilder builder)
        {
            _builders.Add(builder);
            
            if (!builder.UseUI)
            {
                return;
            }

            var subMenu = new NativeMenu(builder.DisplayText, builder.DisplayText);

            if (builder.CanChangeActive)
            {
                var item = new NativeCheckboxItem("Active", builder.IsActive);
                item.CheckboxChanged += (_, e) => builder.IsActive = item.Checked;
                subMenu.Add(item);
                
                subMenu.ItemActivated += (_, _) =>
                {
                    item.Checked = builder.IsActive;
                };
                
            }

            // SubMenuの構築は各スクリプトが頑張る
            builder.OnUiMenuConstruct(subMenu);
            _rootMenu.Add(subMenu);
            _objectPool.Add(subMenu);
        }
    }
}