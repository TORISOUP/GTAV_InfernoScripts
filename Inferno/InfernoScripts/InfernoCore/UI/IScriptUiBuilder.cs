using LemonUI.Menus;

namespace Inferno.InfernoScripts.InfernoCore.UI
{
    public interface IScriptUiBuilder
    {
        /// <summary>
        /// UIを使うか
        /// </summary>
        bool UseUI { get; }
        
        /// <summary>
        /// アクティブフラグを切り替えることができるか
        /// </summary>
        bool CanChangeActive { get; }

        /// <summary>
        /// SubMenuを必要とするか
        /// </summary>
        bool NeedSubMenu { get; }

        /// <summary>
        /// SubMenuの構築
        /// </summary>
        void OnUiMenuConstruct(NativeMenu menu);
    }
}