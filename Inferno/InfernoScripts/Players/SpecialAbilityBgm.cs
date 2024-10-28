using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using LemonUI;
using LemonUI.Menus;

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

            CreateInputKeywordAsObservable("SpecialAbilityBgm", "sbgm")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SpecialAbilityBGM:" + IsActive);
                });

           

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
        
        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.BgmTitle;
        
        public override string Description => PlayerLocalize.BgmDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Misc;

    }
}