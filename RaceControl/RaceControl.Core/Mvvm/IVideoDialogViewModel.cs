using RaceControl.Common.Interfaces;
using RaceControl.Core.Settings;
using System;

namespace RaceControl.Core.Mvvm
{
    public interface IVideoDialogViewModel : IExtendedDialogAware
    {
        Guid UniqueIdentifier { get; }

        IPlayableContent PlayableContent { get; }

        VideoDialogSettings GetDialogSettings();
    }
}