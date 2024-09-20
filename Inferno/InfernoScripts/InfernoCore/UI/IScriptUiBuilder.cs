using LemonUI.Menus;

namespace Inferno.InfernoScripts.InfernoCore.UI
{
    public interface IScriptUiBuilder
    {
        /// <summary>
        /// UIを使うか
        /// </summary>
        bool UseUI { get; }
        
        
        bool CanChangeActive { get; }
        
        string DisplayText { get; }
        

        /// <summary>
        /// SubMenuの構築
        /// </summary>
        void OnUiMenuConstruct(NativeMenu menu);
        
        bool IsActive { get; set; }
    }
}