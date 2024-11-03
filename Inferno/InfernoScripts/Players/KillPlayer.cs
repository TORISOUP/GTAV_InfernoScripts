using System;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 自殺する
    /// </summary>
    internal class KillPlayer : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("KillPlayer", "killme")
                .Subscribe(_ =>
                {
                    if (IsActive)
                    {
                        Kill();
                    }
                });
        }

        private void Kill()
        {
            World.AddExplosion(PlayerPed.Position, GTA.ExplosionType.Grenade, 10.0f, 0.1f);
            PlayerPed.Kill();
            //自殺コマンドで死んだときはランダムな方向にふっとばす
            var x = Random.NextDouble() - 0.5;
            var y = Random.NextDouble() - 0.5;
            var z = Random.NextDouble() - 0.5;
            var randomVector = new Vector3((float)x, (float)y, (float)z);
            randomVector.Normalize();
            PlayerPed.ApplyForce(randomVector * 100);
        }


        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.KillMeTitle;

        public override string Description => PlayerLocalize.KillMeDescription;
        public override MenuIndex MenuIndex => MenuIndex.Misc;

        public override bool CanChangeActive => true;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            menu.AddButton(
                $"[Debug] " + PlayerLocalize.KillMeAction,
                "",
                _ => Kill()
            );
        }

        #endregion
    }
}