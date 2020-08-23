using RaceControl.Services.Interfaces.F1TV;
using System;

namespace RaceControl.Interfaces
{
    public interface IVideoDialogViewModel
    {
        Guid UniqueIdentifier { get; }

        ContentType ContentType { get; }
    }
}