using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using GTA;

namespace Inferno
{
    internal class SpecialAbilityBgm : InfernoScript
    {
        private string[] filePaths;
        private SoundPlayer soundPlayer;

        protected override void Setup()
        {
            filePaths = LoadWavFilePaths(@"scripts/SpecialAbilityBgm");

            if (filePaths.Length <= 0)
            {
                return;
            }

            soundPlayer = new SoundPlayer { SoundLocation = filePaths[Random.Next(filePaths.Length)] };

            CreateInputKeywordAsObservable("sbmg")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SpecialAbilityBGM:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            OnThinnedTickAsObservable
                .Where(_ => IsActive && (uint)PlayerPed.Model.Hash == (uint)PedHash.Trevor)
                .Select(_ => Game.Player.IsSpecialAbilityActive() && PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x)
                    {
                        soundPlayer.Play();
                    }
                    else
                    {
                        soundPlayer.Stop();
                        //次の音をセット
                        soundPlayer.SoundLocation = filePaths[Random.Next(filePaths.Length)];
                        soundPlayer.LoadAsync();
                    }
                });
        }

        /// <summary>
        /// Wavファイルのファイルパス一覧を取得する
        /// </summary>
        private string[] LoadWavFilePaths(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }
    }
}