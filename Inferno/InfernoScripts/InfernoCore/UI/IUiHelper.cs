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

        public static NativeItem AddButton(
            this NativeMenu parentManu,
            string title,
            string description = null,
            Action action = null)
        {
            var item = new NativeItem(title, description);
            item.Activated += (_, _) => { action?.Invoke(); };
            parentManu.Add(item);
            return item;
        }

        public static NativeCheckboxItem AddCheckbox(
            this NativeMenu parentManu,
            string title,
            string description = null,
            Action<NativeCheckboxItem> shown = null,
            Action<bool> action = null
        )
        {
            var checkbox =
                new NativeCheckboxItem(title, description);

            checkbox.CheckboxChanged += (_, e) => { action?.Invoke(checkbox.Checked); };
            parentManu.Shown += (_, _) => { shown?.Invoke(checkbox); };
            parentManu.Add(checkbox);
            return checkbox;
        }
    }
}
