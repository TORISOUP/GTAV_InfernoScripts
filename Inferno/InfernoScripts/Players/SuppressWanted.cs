using System;
using System.Reactive.Linq;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using LemonUI;
using LemonUI.Menus;


namespace Inferno.InfernoScripts.Player
{
    public sealed class SuppressWanted : InfernoScript
    {
        private int _suppressWantedLevel = 1;

        protected override bool DefaultAllOnEnable => false;

        
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("Wanted_Suppress", "swanted")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Suppress Wanted:" + IsActive);
                });

            CreateInputKeywordAsObservable("Wanted_Zero", "nowanted")
                .Subscribe(_ =>
                {
                    Game.Player.WantedLevel = 0;
                    DrawText("No Wanted!");
                });


            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Subscribe(_ =>
                {
                    // 手配度を無効化する
                    if (Game.Player.WantedLevel <= _suppressWantedLevel)
                    {
                        Game.Player.WantedLevel = 0;
                    }
                });
        }

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.NoWantedTitle;

        public override string Description => PlayerLocalize.NoWantedDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Player;


        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            menu.AddSlider(
                $"Level: {_suppressWantedLevel}",
                PlayerLocalize.NoWantedLevel,
                _suppressWantedLevel,
                6,
                x =>
                {
                    x.Value = _suppressWantedLevel;
                    x.Multiplier = 1;
                }, item =>
                {
                    _suppressWantedLevel = item.Value;
                    item.Title = $"Level: {_suppressWantedLevel}";
                });

            menu.AddButton(
                PlayerLocalize.NoWantedAction,
                "",
                _ => Game.Player.WantedLevel = 0
            );
        }
    }
}