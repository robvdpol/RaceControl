namespace RaceControl.Common.Interfaces
{
    public interface IMediaRenderer
    {
        string Name { get; }

        object Renderer { get; }
    }
}