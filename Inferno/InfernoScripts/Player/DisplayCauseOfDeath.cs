using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno
{
    /// <summary>
    /// 死因表示
    /// </summary>
    class DisplayCauseOfDeath : InfernoScript
    {
        private UIContainer _mContainer;
        private int ScreenHeight;
        private int ScreenWidth;

        private Vector2 textPositionScale = new Vector2(0.5f,0.75f);


        protected override void Setup()
        {
            var screenResolution = NativeFunctions.GetScreenResolution();
            ScreenHeight = (int)screenResolution.Y;
            ScreenWidth = (int)screenResolution.X;

            _mContainer = new UIContainer(
                new Point(0, 0), new Size(ScreenWidth, ScreenHeight));

            this.OnDrawingTickAsObservable
                .Where(_ => _mContainer.Items.Count > 0)
                .Subscribe(_ => _mContainer.Draw());

            CreateTickAsObservable(500)
               .Where(_ => playerPed.IsSafeExist())
                .Select(x => playerPed.IsAlive)
                .DistinctUntilChanged()
                .Subscribe(isAlive =>
                {
                    _mContainer.Items.Clear();
                    if (isAlive) return;
                    
                    //死んでいたら死因を出す
                    var damageWeapon = playerPed.GetCauseOfDeath();
                    if(damageWeapon==0)return;
                        
                    var damageName = damageWeapon.ToString();
                    if (playerPed.GetKiller() == playerPed) damageName += "(SUICIDE)";
                    var text = new UIText(damageName,
                        new Point((int)(ScreenWidth * textPositionScale.X),(int)(ScreenHeight*textPositionScale.Y)),
                        1.0f, Color.White, 0, true);

                    _mContainer.Items.Add(text);
                });

        }
    }
}
