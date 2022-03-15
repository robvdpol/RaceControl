namespace RaceControl.Flyleaf;

public class FlyleafAspectRatio : IAspectRatio
{
    public FlyleafAspectRatio(AspectRatio aspectRatio)
    {
        Value = aspectRatio.ValueStr;
        Description = aspectRatio.ToString();
    }

    public string Value { get; }
    public string Description { get; }
}