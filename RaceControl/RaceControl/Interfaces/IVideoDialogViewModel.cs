using RaceControl.Common.Enum;
using RaceControl.Common.Settings;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel
    {
        Guid UniqueIdentifier { get; }

        ContentType ContentType { get; }

        string ContentUrl { get; }

        VideoDialogInstance GetVideoDialogInstance();
    }
}