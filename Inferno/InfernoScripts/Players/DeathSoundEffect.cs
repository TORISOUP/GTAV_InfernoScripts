using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;

namespace Inferno
{
    /// <summary>
    /// 死亡時に音を鳴らす
    /// </summary>
    internal class DeathSoundEffect : InfernoScript
    {
        private string[] filePath;
        private SoundPlayer soundPlayer;

        private IDisposable _disposable;

        protected override void Setup()
        {
            filePath = LoadWavFiles(@"scripts/Pichun");

            soundPlayer = new SoundPlayer();
            //音声ファイルロード完了時に再生する
            soundPlayer.LoadCompleted += (sender, args) => { soundPlayer.Play(); };

            IsActiveRP.Subscribe(x =>
            {
                _disposable?.Dispose();

                if (x)
                {
                    //ファイルが存在した時のみ
                    if (filePath.Length > 0)
                        //プレイヤが死亡したら再生
                    {
                        _disposable = OnThinnedTickAsObservable
                            .Select(_ => PlayerPed)
                            .Where(p => p.IsSafeExist())
                            .Select(p => p.IsAlive)
                            .DistinctUntilChanged()
                            .Where(isAlive => !isAlive)
                            .Subscribe(_ => PlayAction());
                    }
                }
            });
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

        private void PlayAction()
        {
            var path = filePath[Random.Next(filePath.Length)];
            soundPlayer.SoundLocation = path;
            soundPlayer.LoadAsync();
        }


        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.DeathSfxTitle;

        public override string Description => PlayerLocalize.DeathSfxDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Misc;

        #endregion
    }
}