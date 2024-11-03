using System;
using Inferno.Properties;
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
        
        bool IsAllOnEnable { get; }
        
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

    public static class MenuIndexExt
    {
        public static string ToLocalizedString(this MenuIndex index)
        {
            return index switch
            {
                MenuIndex.Root => "",
                MenuIndex.World => MenuLocalize.IndexWorld,
                MenuIndex.Player => MenuLocalize.IndexPlayer,
                MenuIndex.Entities => MenuLocalize.IndexEntities,
                MenuIndex.Misc => MenuLocalize.IndexMisc,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        }
        
    }
}