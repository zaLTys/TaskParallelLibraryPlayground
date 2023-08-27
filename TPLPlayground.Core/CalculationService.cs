using static Data.DataStore;

namespace TPLPlayground.Core
{
    internal class CalculationService
    {
        public decimal Compute(int value, int minDurationMs)
        {
            var end = DateTime.Now + TimeSpan.FromMilliseconds(minDurationMs);

            while (DateTime.Now < end) { }

            Console.WriteLine($"Running in thread - {Thread.CurrentThread.ManagedThreadId}");
            return value + 0.5m;
        }


        public async Task ProcessItems(IEnumerable<Item> items, IProgress<Item> progress)
        {
            

            Console.WriteLine("Taking care of items");

            var parent = Task.Run(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                int remaining = items.Count();

                foreach (var item in items)
                {
                    var itemTask = Task.Factory.StartNew(() =>
                    {
                        Console.WriteLine($"Taking Care Of {item.Id}");

                        Thread.Sleep(100);
                        Console.WriteLine($"Item {item.Id} task thread - {Thread.CurrentThread.ManagedThreadId}");
                    }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                    _ = itemTask.ContinueWith(t =>
                    {
                        Console.WriteLine($"Item {item.Id} task continuation thread - {Thread.CurrentThread.ManagedThreadId}");
                        if (t.Status == TaskStatus.RanToCompletion)
                        {
                            progress?.Report(item);
                            if (Interlocked.Decrement(ref remaining) == 0)
                                tcs.SetResult(null);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                await tcs.Task;
            });

            await parent;

            Console.WriteLine("Taking care of items done");
        }
    }
}
