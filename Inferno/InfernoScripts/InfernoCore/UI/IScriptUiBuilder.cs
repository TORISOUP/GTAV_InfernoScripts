using LemonUI.Menus;
using Reactive.Bindings;

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
        
        MenuIndex MenuIndex { get; }

        /// <summary>
        /// SubMenuの構築
        /// </summary>
        void OnUiMenuConstruct(NativeMenu menu);
        
        bool IsActive { get; set; }
        IReadOnlyReactiveProperty<bool> IsActiveRP { get; }
    }
    
    public enum MenuIndex
    {
        Root,
        World,
        Player,
        Entities,
        Misc,
    }
}