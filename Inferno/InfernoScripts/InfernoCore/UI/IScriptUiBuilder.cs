namespace Inferno.InfernoScripts.InfernoCore.UI
{
    public interface IScriptUiBuilder
    {
        /// <summary>
        /// アクティブフラグを切り替えることができるか
        /// </summary>
        bool CanChangeActive { get; }
        
        
    }
}