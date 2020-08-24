using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RaceControl.Core.Helpers
{
    public static class DownloadHelper
    {
        public static async Task<IList<T>> BufferedDownload<T>(Func<string, Task<T>> getter, IEnumerable<string> identifiers, int maxDegreeOfParallelism = 50)
        {
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            var downloader = new TransformBlock<string, T>(getter, options);
            var buffer = new BufferBlock<T>();
            downloader.LinkTo(buffer);

            foreach (var identifier in identifiers)
            {
                await downloader.SendAsync(identifier);
            }

            downloader.Complete();
            await downloader.Completion;

            return buffer.TryReceiveAll(out var list) ? list : new List<T>();
        }
    }
}