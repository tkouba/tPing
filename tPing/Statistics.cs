using System;

namespace tPing
{
    class Statistics
    {
        public int Attempts { get; private set; } = 0;
        public int Good { get; private set; } = 0;
        public long MinTime { get; private set; } = 0;
        public long MaxTime { get; private set; } = 0;
        public decimal SumTime { get; private set; } = 0;
        public decimal AverageTime { get => Good != 0 ? SumTime / Good : 0; }

        public int Bad { get => Attempts - Good; }

        public void AddGood(long elapsed)
        {
            Attempts++;
            Good++;
            SumTime += elapsed;
            if (elapsed > MaxTime)
                MaxTime = elapsed;
            if (MinTime == 0 || elapsed < MinTime)
                MinTime = elapsed;
        }

        public void AddBad()
        {
            Attempts++;
        }

    }
}
