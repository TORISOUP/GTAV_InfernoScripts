using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 死因表示
    /// </summary>
    internal class DisplayCauseOfDeath : InfernoScript
    {
        private readonly Vector2 _textPositionScale = new(0.5f, 0.75f);
        private ContainerElement _mContainer;
        private int _screenHeight;
        private int _screenWidth;
        private CompositeDisposable _activityDisposables;

        protected override void Setup()
        {
            var screenResolution = NativeFunctions.GetScreenResolution();
            _screenHeight = (int)screenResolution.Y;
            _screenWidth = (int)screenResolution.X;

            _mContainer = new ContainerElement(
                new Point(0, 0), new Size(_screenWidth, _screenHeight));

            IsActiveRP.Subscribe(x =>
            {
                _activityDisposables?.Dispose();

                if (!x)
                {
                    return;
                }

                _activityDisposables = new CompositeDisposable();

                OnDrawingTickAsObservable
                    .Where(_ => _mContainer.Items.Count > 0)
                    .Subscribe(_ => _mContainer.Draw())
                    .AddTo(_activityDisposables);

                CreateTickAsObservable(TimeSpan.FromSeconds(0.5f))
                    .Where(_ => PlayerPed.IsSafeExist())
                    .Select(x => PlayerPed.IsAlive)
                    .DistinctUntilChanged()
                    .Subscribe(isAlive =>
                    {
                        _mContainer.Items.Clear();
                        if (isAlive)
                        {
                            return;
                        }

                        //死んでいたら死因を出す
                        var damageWeapon = PlayerPed.GetCauseOfDeath();
                        if (damageWeapon == 0)
                        {
                            return;
                        }

                        string damageName = null;

                        // damageWeaponがWeapon内に定義されているか
                        if (Enum.IsDefined(typeof(Weapon), damageWeapon))
                        {
                            damageName = damageWeapon.ToString();
                        }
                        else
                        {
                            //定義されていない場合はWeaponHashとして表示
                            damageName = ((WeaponHash)damageWeapon).ToString();
                        }


                        if (PlayerPed.Killer == PlayerPed)
                        {
                            damageName += "(SUICIDE)";
                        }

                        var text = new TextElement(damageName,
                            new Point((int)(_screenWidth * _textPositionScale.X),
                                (int)(_screenHeight * _textPositionScale.Y)),
                            1.0f, Color.White, 0, Alignment.Center, false, true);

                        _mContainer.Items.Add(text);
                    })
                    .AddTo(_activityDisposables);
            });
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => MiscLocalization.DisplayCoDTitle;

        public override string Description => MiscLocalization.DisplayCoDDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Misc;

        #endregion
    }
}