using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class BlackOut : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;
        private SoundPlayer soundPlayerEnd;

        public BlackOut(ParupunteCore core) : base(core)
        {
            SetUpSound();
        }

        public override string Name { get; } = "ctOS 停電";
        public override string EndMessage { get; } = "ctOS 復旧";

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                StartCoroutine(BlackOutEnd());
            });

            StartCoroutine(BlackOutStart());
            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    GTA.World.SetBlackout(false);
                    soundPlayerStart = null;
                    soundPlayerEnd = null;
                });
        }
        

        private IEnumerable<object> BlackOutStart()
        {
            //効果音に合わせてチカチカさせる
            soundPlayerStart?.Play();
            yield return WaitForSeconds(1.5f);
            var current = false;
            for (var i = 0; i < 10; i++)
            {
                GTA.World.SetBlackout(current);
                if (Random.Next(0, 2)%2 == 0)
                {
                    current = !current;
                }
                yield return null;
            }
            GTA.World.SetBlackout(true);
        }

        private IEnumerable<object> BlackOutEnd()
        {
            soundPlayerEnd?.Play();
            yield return WaitForSeconds(1);
            ParupunteEnd();
        } 

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("blackout_start.wav"));
            if (setupWav != null)
            {
                soundPlayerStart = new SoundPlayer(setupWav);
            }

            setupWav = filePaths.FirstOrDefault(x => x.Contains("blackout_end.wav"));
            if (setupWav != null)
            {
                soundPlayerEnd = new SoundPlayer(setupWav);
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
    }
}
