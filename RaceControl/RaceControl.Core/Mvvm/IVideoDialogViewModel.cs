using Prism.Services.Dialogs;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Settings;
using System;

namespace RaceControl.Core.Mvvm
{
    public interface IVideoDialogViewModel : IDialogAware
    {
        Guid UniqueIdentifier { get; }

        IPlayableContent PlayableContent { get; }

        VideoDialogSettings GetDialogSettings();
    }
}