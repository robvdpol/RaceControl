using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.Lark
{
    public interface ILarkCollection<TResponse>
    {
        List<TResponse> Objects { get; set; }
    }
}