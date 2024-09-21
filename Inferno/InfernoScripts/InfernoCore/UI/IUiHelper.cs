using System;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.InfernoCore.UI
{
    public static class UiHelperExt
    {
        public static NativeSliderItem AddEnumSlider<TEnum>(
            this NativeMenu parentManu,
            string title,
            string description = null,
            TEnum initialValue = default,
            Action<NativeSliderItem> shown = null,
            Action<NativeSliderItem> changeSlider = null) where TEnum : Enum
        {
            var slider =
                new NativeSliderItem(title, description, Enum.GetValues(typeof(TEnum)).Length - 1,
                    Convert.ToInt32(initialValue));

            slider.ValueChanged += (_, _) => { changeSlider?.Invoke(slider); };
            parentManu.Shown += (_, _) => { shown?.Invoke(slider); };
            parentManu.Add(slider);
            return slider;
        }

        public static NativeSliderItem AddSlider(
            this NativeMenu parentManu,
            string title,
            string description = null,
            int initialValue = 0,
            int maxValue = 100,
            Action<NativeSliderItem> shown = null,
            Action<NativeSliderItem> changeSlider = null)
        {
            var slider =
                new NativeSliderItem(title, description, maxValue, initialValue);

            slider.ValueChanged += (_, _) => { changeSlider?.Invoke(slider); };
            parentManu.Shown += (_, _) => { shown?.Invoke(slider); };
            parentManu.Add(slider);
            return slider;
        }
    }
}