using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using MvvmCross.UI;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using System.Reactive.Subjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive;
using System.Reactive.Disposables;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectColorViewModel : MvxViewModel<ColorParameters, MvxColor>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ISchedulerProvider schedulerProvider;

        private MvxColor initialColor;

        private BehaviorSubject<SelectableColorViewModel> customColorSubject = new BehaviorSubject<SelectableColorViewModel>(
            new SelectableColorViewModel(MvxColors.Transparent, false)
        );

        private BehaviorSubject<float> hueSubject = new BehaviorSubject<float>(0);
        private BehaviorSubject<float> saturationSubject = new BehaviorSubject<float>(0);
        private BehaviorSubject<float> valueSubject = new BehaviorSubject<float>(0.375f);

        private BehaviorSubject<IEnumerable<SelectableColorViewModel>> selectableColorsSubject =
            new BehaviorSubject<IEnumerable<SelectableColorViewModel>>(Enumerable.Empty<SelectableColorViewModel>());

        public IObservable<float> Hue { get; private set; }
        public IObservable<float> Saturation { get; private set; }
        public IObservable<float> Value { get; private set; }

        public bool AllowCustomColors { get; private set; }

        public UIAction Save { get; set; }
        public UIAction Close { get; set; }

        public InputAction<float> SetHue { get; set; }
        public InputAction<float> SetSaturation { get; set; }
        public InputAction<SelectableColorViewModel> SetColor { get; set; }

        [Obsolete("This should be removed in favor of SetColor.")]
        public IMvxCommand<SelectableColorViewModel> SelectColorCommand { get; set; }

        public IObservable<IEnumerable<SelectableColorViewModel>> SelectableColors { get; private set; }

        public SelectColorViewModel(IMvxNavigationService navigationService, ISchedulerProvider schedulerProvider)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));

            this.navigationService = navigationService;
            this.schedulerProvider = schedulerProvider;

            Hue = hueSubject.DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            Saturation = saturationSubject.DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            Value = valueSubject.DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            customColorSubject.CombineLatest(Hue, Saturation, Value, fromCustomColorWithHSV)
                              .Subscribe(setColor);

            SelectableColors = selectableColorsSubject
                .AsDriver(schedulerProvider);

            Save = UIAction.FromAsync(save);
            Close = UIAction.FromAsync(close);

            SetHue = InputAction<float>.FromObservable(setHue);
            SetSaturation = InputAction<float>.FromObservable(setSaturation);
            SetColor = InputAction<SelectableColorViewModel>.FromAction(setColor);

            //SelectColorCommand = new MvxCommand<SelectableColorViewModel>(selectColor);
        }

        public override void Prepare(ColorParameters parameter)
        {
            initialColor = parameter.Color;
            AllowCustomColors = parameter.AllowCustomColors;

            var selectableColors = Color.DefaultProjectColors
                .Select(color => new SelectableColorViewModel(color, color == initialColor))
                .ToList();

            var noColorsSelected = selectableColors.None(color => color.Selected);
            if (AllowCustomColors)
            {
                selectableColors.Add(customColorSubject.Value);

                if (noColorsSelected)
                {
                    customColorSubject.OnNext(new SelectableColorViewModel(initialColor, true));
                    var (hue, saturation, value) = initialColor.GetHSV();

                    hueSubject.OnNext(hue);
                    saturationSubject.OnNext(saturation);
                    valueSubject.OnNext(value);
                }
                else
                {
                    var changedColor = customColorSubject.Value.WithColor(
                        Color.FromHSV(hueSubject.Value, saturationSubject.Value, valueSubject.Value));

                    customColorSubject.OnNext(changedColor);
                }
            }
            else if (noColorsSelected)
            {
                selectableColors = selectableColors
                    .Select((colorViewModel, index) => colorViewModel.Select(index == 0))
                    .ToList();
            }

            selectableColorsSubject.OnNext(selectableColors);
        }

        private SelectableColorViewModel fromCustomColorWithHSV(SelectableColorViewModel customColor, float hue, float saturation, float value)
            => customColor.WithColor(Color.FromHSV(hue, saturation, value));

        private void setColor(SelectableColorViewModel selectedColorViewModel)
        {
            var selectableColors = selectableColorsSubject.Value
                .Select(colorViewModel => colorViewModel.Select(colorViewModel.IsSameColorAs(selectedColorViewModel)));

            selectableColorsSubject.OnNext(selectableColors);

            if (AllowCustomColors)
                return;

            save();
        }

        private IObservable<Unit> setHue(float hue) =>
            Observable.Create<Unit>(observer =>
            {
                hueSubject.OnNext(hue);
                observer.CompleteWithUnit();
                return Disposable.Empty;
            });

        private IObservable<Unit> setSaturation(float hue) =>
            Observable.Create<Unit>(observer =>
            {
                saturationSubject.OnNext(hue);
                observer.CompleteWithUnit();
                return Disposable.Empty;
            });

        private Task close()
            => navigationService.Close(this, initialColor);

        private Task save()
            => navigationService.Close(this, selectableColorsSubject.Value.Single(x => x.Selected).Color);
    }
}
