using LemonUI;
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

        string DisplayName { get; }

        MenuIndex MenuIndex { get; }

        /// <summary>
        /// SubMenuの構築
        /// </summary>
        void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu);

        bool IsActive { get; set; }
        IReadOnlyReactiveProperty<bool> IsActiveRP { get; }
        
        string Description { get; }
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