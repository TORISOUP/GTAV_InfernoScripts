using System;
using System.Reactive.Linq;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// プレイヤーの強制ラグドール状態化(脱力)
    /// </summary>
    public class PlayerRagdoll : InfernoScript
    {
        protected override void Setup()
        {
            IsActive = true;
            CreateTickAsObservable(TimeSpan.FromMilliseconds(50))
                .Where(_ => IsActive)
                .Where(_ => Game.IsControlPressed(Control.Duck) && Game.IsControlPressed(Control.Jump))
                .Subscribe(_ => SetPlayerRagdoll());
        }

        private void SetPlayerRagdoll()
        {
            Game.Player.Character.SetToRagdoll();
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayText => IsLangJpn ? "脱力" : "Player ragdoll";
        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            menu.AddButton(
                IsLangJpn ? "脱力する" : "Ragdoll",
                IsLangJpn ? "プレイヤーを脱力状態にします" : "Set player to ragdoll",
                () => Game.Player.Character.SetToRagdoll()
            );
        }

        #endregion
    }
}