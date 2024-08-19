using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;


namespace Inferno
{
    /// <summary>
    /// 死亡時に音を鳴らす
    /// </summary>
    internal class DeathSoundEffect : InfernoScript
    {
        private string[] filePath;
        private SoundPlayer soundPlayer;

        protected override void Setup()
        {
            filePath = LoadWavFiles(@"scripts/Pichun");

            soundPlayer = new SoundPlayer();
            //音声ファイルロード完了時に再生する
            soundPlayer.LoadCompleted += (sender, args) => { soundPlayer.Play(); };

            //ファイルが存在した時のみ
            if (filePath.Length > 0)
            {
                //プレイヤが死亡したら再生
                OnThinnedTickAsObservable
                    .Select(_ => PlayerPed)
                    .Where(p => p.IsSafeExist())
                    .Select(p => p.IsAlive)
                    .DistinctUntilChanged()
                    .Where(isAlive => !isAlive)
                    .Subscribe(_ => PlayAction());
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

        private void PlayAction()
        {
            var path = filePath[Random.Next(filePath.Length)];
            soundPlayer.SoundLocation = path;
            soundPlayer.LoadAsync();
        }
    }
}