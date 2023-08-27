using Data;
using System.Diagnostics;
using static Data.DataStore;

namespace TPLPlayground.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {

            await RunWithDatastorePrices();

          //  RunWithCancelation2sAndParalelism2();
        }


        private static async Task RunWithDatastorePrices()
        {
            Console.WriteLine($"Running {nameof(RunWithDatastorePrices)}");
            var dataStore = new DataStore();
            var itemsSlow = dataStore.CreateItems(6);


            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);
            var cancelledItems = dataStore.CreateItemsInParallel(100, cts.Token);

            var items = dataStore.CreateItemsInParallel(100, CancellationToken.None);

            var service = new CalculationService();
            var progress = new Progress<Item>();
            progress.ProgressChanged += (_, item) =>
            {
                Console.WriteLine($"Item id {item.Id}, price {item.Price} progress update");
            };

            await service.ProcessItems(items, progress);

            Console.WriteLine($"Done {nameof(RunWithDatastorePrices)}");
        }



        private static void RunWithCancelation2sAndParalelism2()
        {
            Console.WriteLine($"Running {nameof(RunWithCancelation2sAndParalelism2)}");

            Stopwatch stopwatch = new();
            stopwatch.Start();

            var calculationService = new CalculationService();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(2000);

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationTokenSource.Token,
                MaxDegreeOfParallelism = 2
            };
            int total = 0;
            try
            {
                Parallel.For(0, 100, parallelOptions, (i) =>
                {
                    Interlocked.Add(ref total, (int)calculationService.Compute(i, 300));
                });
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Cancellation Requested!");
            }
            Console.WriteLine(total);
            Console.WriteLine($"It took: {stopwatch.ElapsedMilliseconds}ms to run");
            Console.ReadLine();
        }
    }
}
