using RaceControl.Common.Interfaces;
using RaceControl.Common.Settings;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel
    {
        Guid UniqueIdentifier { get; }

        IPlayable Playable { get; }

        VideoDialogInstance GetVideoDialogInstance();
    }
}