namespace RaceControl.Interfaces
{
    public interface IMediaRenderer
    {
        string Name { get; }

        object Renderer { get; }
    }
}