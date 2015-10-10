namespace Inferno
{
    /// <summary>
    /// ProgressBarの描画に用いることができるインターフェース
    /// </summary>
    public interface IProgressBar
    {
        /// <summary>
        /// 描画時のバーの度合い
        /// </summary>
        float Rate { get; }

        /// <summary>
        /// 完了したかフラグ
        /// </summary>
        bool IsCompleted { get; }
    }
}
