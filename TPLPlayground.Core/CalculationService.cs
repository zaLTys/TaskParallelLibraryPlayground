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
    }
}
