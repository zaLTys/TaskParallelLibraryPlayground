using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public class DataStore
    {
        private static Random random = new();

        public record Item(string Id, decimal Price);

        public IEnumerable<Item> CreateItems(int amountOfItems)
        {
            var sw = new Stopwatch();
            sw.Start();

            var items = new List<Item>();

            for (int i = 0; i < amountOfItems; i++)
            {
                items.Add(new Item(Guid.NewGuid().ToString(), CreatePrice()));
                Thread.Sleep(100);
            }
            Console.WriteLine($"{nameof(DataStore)} Generated {amountOfItems} items in {sw.ElapsedMilliseconds}ms");
            return items;
        }

        public IEnumerable<Item> CreateItemsInParallel(int amountOfItems, CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            sw.Start();

            var bag = new ConcurrentBag<Item>();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                //MaxDegreeOfParallelism = 1
            };

            var loopResult = Parallel.For(0, amountOfItems, (x, loopState) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    loopState.Break();
                    return;
                }

                bag.Add(new Item(Guid.NewGuid().ToString(), CreatePrice()));
                Thread.Sleep(100);
            });

            sw.Stop();
            if (!loopResult.IsCompleted)
            {
                Console.WriteLine($"{nameof(DataStore)} Generated {bag.Count()} items in parallel in {sw.ElapsedMilliseconds}ms before it was cancelled");
            }
            else
            {
                Console.WriteLine($"{nameof(DataStore)} Generated {bag.Count()} items in parallel in {sw.ElapsedMilliseconds}ms");
            }
            return bag;
        }

        private decimal CreatePrice()
        {
            return random.Next(1, 100) + 0.1m;
        }

    }

}