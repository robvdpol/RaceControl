using RaceControl.Common.Interfaces;
using RaceControl.Common.Settings;
using RaceControl.Core.Mvvm;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel : IExtendedDialogAware
    {
        Guid UniqueIdentifier { get; }

        IPlayableContent PlayableContent { get; }

        VideoDialogInstance GetVideoDialogInstance();
    }
}