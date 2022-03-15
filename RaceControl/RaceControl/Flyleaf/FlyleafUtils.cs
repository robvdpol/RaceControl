namespace RaceControl.Flyleaf;

public static class FlyleafUtils
{
    public static VideoStream GetVideoStreamForQuality(this List<VideoStream> videoStreams, VideoQuality videoQuality)
    {
        if (!videoStreams.Any())
        {
            return null;
        }

        var maxHeight = videoStreams.Max(stream => stream.Height);
        var minHeight = maxHeight;

        switch (videoQuality)
        {
            case VideoQuality.Medium:
                minHeight = maxHeight / 3 * 2;
                break;

            case VideoQuality.Low:
                minHeight = maxHeight / 2;
                break;

            case VideoQuality.Lowest:
                minHeight = maxHeight / 3;
                break;
        }

        return videoStreams.OrderBy(stream => stream.Height).FirstOrDefault(stream => stream.Height >= minHeight);
    }
}