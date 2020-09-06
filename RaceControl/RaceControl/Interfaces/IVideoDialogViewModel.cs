using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel : IExtendedDialogAware
    {
        Guid UniqueIdentifier { get; }

        IPlayableContent PlayableContent { get; }

        VideoDialogInstance Instance { get; }
    }
}