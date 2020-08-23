using RaceControl.Common.Settings;
using RaceControl.Services.Interfaces.F1TV;
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