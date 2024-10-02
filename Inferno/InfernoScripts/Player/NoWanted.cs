using System;
using System.Reactive.Linq;
using GTA;
using GTA.Native;


namespace Inferno.InfernoScripts.Player
{
    public sealed class NoWanted : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("Wanted_Suppress","swanted")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;

                    DrawText("Suppress Wanted:" + IsActive);
                });
            
            CreateInputKeywordAsObservable("Wanted_Zero","nowanted")
                .Subscribe(_ =>
                {
                    Game.Player.WantedLevel = 0;
                    DrawText("No Wanted!");
                });


            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Subscribe(_ =>
                {
                    // 手配度1を無効化する
                    if (Game.Player.WantedLevel == 1)
                    {
                        Game.Player.WantedLevel = 0;
                    }

                    Function.Call(Hash.SET_WANTED_LEVEL_MULTIPLIER, 0f);
                });
        }
    }
}