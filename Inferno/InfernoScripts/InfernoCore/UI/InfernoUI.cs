using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using Inferno.Properties;
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

        readonly NativeMenu _rootMenu = new("Inferno MOD", "MOD Menu")
        {
            Visible = false
        };

        private readonly List<IScriptUiBuilder> _builders = new();

        private readonly Dictionary<MenuIndex, NativeMenu> _manuIndex = new();

        private bool _isTickStarted;

        public InfernoUi()
        {
            try
            {
                CultureInfo.DefaultThreadCurrentUICulture =
                    Game.Language switch
                    {
                        Language.Japanese => new CultureInfo("ja-JP"),
                        _ => new CultureInfo("en-US")
                    };


                Instance = this;

                #region Inialize

                Interval = 0;
                _objectPool.Add(_rootMenu);

                var keyword = InfernoCommandProvider.Instance.GetCommand("OpenMainMenu", "F5");
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                        h => KeyDown -= h)
                    .Select(e => e.EventArgs)
                    .Select(e => e.KeyCode.ToString())
                    .SelectMany(e => e)
                    .Buffer(keyword.Length, 1)
                    .Select(x => string.Join("", x))
                    .Where(x => x == keyword.ToUpper())
                    .Select(_ => Unit.Default)
                    .Take(1)
                    .Repeat()
                    .Subscribe(_ =>
                    {
                        var menus = _objectPool.OfType<NativeMenu>().ToArray();
                        if (menus.Any(x => x.Visible))
                        {
                            foreach (var m in menus)
                            {
                                m.Visible = false;
                            }
                        }
                        else
                        {
                            _rootMenu.Visible = true;
                        }

                        if (!_isTickStarted)
                        {
                            _isTickStarted = true;
                            Tick += (_, _) => OnTick();
                        }
                    })
                    .AddTo(_compositeDisposable);

                Aborted += (_, _) => { _compositeDisposable.Dispose(); };

                _manuIndex.Add(MenuIndex.Root, _rootMenu);

                #endregion


                // 全部一斉ON
                var allOnItem = new NativeItem("[All ON]", MenuLocalize.AllOn);
                allOnItem.Activated += (_, __) =>
                {
                    foreach (var builder in _builders)
                    {
                        if (builder.CanChangeActive && builder.IsAllOnEnable)
                        {
                            builder.IsActive = true;
                        }
                    }
                };
                _rootMenu.Add(allOnItem);


                var allManu = Enum.GetValues(typeof(MenuIndex)).Cast<MenuIndex>();
                foreach (var m in allManu.Where(x => x != MenuIndex.Root))
                {
                    var menu = new NativeMenu(m.ToLocalizedString(), m.ToLocalizedString())
                    {
                        Visible = false,
                    };
                    _rootMenu.AddSubMenu(menu);
                    _objectPool.Add(menu);
                    _manuIndex.Add(m, menu);
                }
            }
            catch (Exception e)
            {
                DebugLogger.Instance.Log(e.ToString());
            }
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

            var subMenu = new NativeMenu(builder.DisplayName, builder.DisplayName, builder.Description)
            {
                Visible = false
            };

            if (builder.CanChangeActive)
            {
                var item = new NativeCheckboxItem("Active", builder.IsActive);
                item.Description = builder.Description;
                item.CheckboxChanged += (_, e) => builder.IsActive = item.Checked;
                subMenu.Add(item);
                builder.IsActiveRP.Subscribe(x => { item.Checked = x; })
                    .AddTo(_compositeDisposable);
            }

            // SubMenuの構築は各スクリプトが頑張る
            builder.OnUiMenuConstruct(_objectPool, subMenu);
            _objectPool.Add(subMenu);

            _manuIndex[builder.MenuIndex].AddSubMenu(subMenu);

            subMenu.BannerText.Scale = 0.5f;
            subMenu.BannerText.Recalculate();
        }
    }
}