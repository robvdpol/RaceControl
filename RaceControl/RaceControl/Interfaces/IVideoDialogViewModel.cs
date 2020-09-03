using RaceControl.Common.Settings;
using RaceControl.Core.Mvvm;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel : IExtendedDialogAware
    {
        Guid UniqueIdentifier { get; }

        PlayableContent PlayableContent { get; }

        VideoDialogInstance GetVideoDialogInstance();
    }
}