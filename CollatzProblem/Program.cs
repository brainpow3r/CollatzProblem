using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CollatzProblem
{
    public class Program
    {
        static int numberOfThreads = 1;
        static long itemCount = 50000;
        
        static void Main(string[] args)
        {
            long baseTime = 0;
            SafeNumberArray numbers = new SafeNumberArray(itemCount) { NextFreeIndex = 0 };

            if (args.Length == 0)
                Console.WriteLine("Number of threads to use was not supplied. 1 Thread will be used");
            else
            {
                numberOfThreads = int.Parse(args[0]);
                itemCount = long.Parse(args[1]);
            }

            InitData(ref numbers);

            Console.WriteLine("[NumberOfThreads] [Workload] [TimeS] [Speedup]");
            for (numberOfThreads = 1; numberOfThreads <= 32; numberOfThreads *= 2)
            {
                long dtime = MakePerformanceTest(numbers);
                baseTime = (numberOfThreads == 1) ? dtime : baseTime;
                decimal speedup = (decimal)baseTime / (decimal)dtime;
                ReloadData(ref numbers);
                Console.WriteLine("{0} {1} {2} {3}", numberOfThreads, itemCount, dtime, speedup);
            }
        }

        public static long MakePerformanceTest(SafeNumberArray numbers)
        {
            Thread[] workers = new Thread[numberOfThreads];
            Stopwatch t = new Stopwatch();
            t.Reset();
            t.Start();
            for (int i = 0; i < numberOfThreads; i++)
            {
                workers[i] = new Thread(() => Collatz(ref numbers));
                workers[i].Start();
            }

            for (int i = 0; i < numberOfThreads; i++)
            {
                workers[i].Join();
            }
            t.Stop();
            return t.ElapsedMilliseconds;
        }

        public static void Collatz(ref SafeNumberArray numbers)
        {
            while(true)
            {
                SafeNumber freeNumber = numbers.GetNextFree();
                
                if (freeNumber == null)
                    break;

                long number = freeNumber.Number;
                while (number != 1)
                {
                    if (number % 2 != 0)
                    {
                        number = 3 * number + 1;
                    } else
                    {
                        number = number / 2;
                    }

                }
            }
            

        }

        public static void InitData(ref SafeNumberArray numbers)
        {
            Random r = new Random(int.MaxValue);
            for (int i = 0; i < itemCount; i++)
                numbers.Numbers[i] = new SafeNumber() { Index = i, Number = r.Next(), Taken = false };

        }

        public static void ReloadData(ref SafeNumberArray numbers)
        {
            for (int i = 0; i < itemCount; i++)
                numbers.Numbers[i].Taken = false;

            numbers.NextFreeIndex = 0;
        }
    }

    public class SafeNumberArray
    {
        public SafeNumberArray(long count) => Numbers = new SafeNumber[count];
        private object _lock = new object();
        private object _indexLock = new object();
        private long _nextIndex { get; set; } = 0;
        public SafeNumber[] Numbers { get; set; }
        public long NextFreeIndex {
            get
            {
                Monitor.Enter(_lock);
                try
                {
                    long returnIndex = _nextIndex;
                    _nextIndex = GetNextFreeNumberIndex();
                    return returnIndex;
                } 
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            set
            {
                this._nextIndex = value;
            }
        }

        public SafeNumber GetNextFree()
        {
            if (_nextIndex >= 0 && _nextIndex <= Numbers.Length-1)
            {
                bool lockTaken = false;
                TimeSpan ts = TimeSpan.FromMilliseconds(-1);

                try
                {
                    Monitor.TryEnter(_lock, ts, ref lockTaken);

                    if (lockTaken)
                    {
                        SafeNumber number = Numbers[NextFreeIndex];
                        if (number.Index == Numbers.Length - 1 && number.Taken)
                            return null;
                        else
                        {
                            number.Taken = true;
                            Numbers[number.Index].Taken = true;
                            return number;
                        }
                    }

                    return null;
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(_lock);
                }
            }
            else
                return null;
        }

        private long GetNextFreeNumberIndex()
        {
            bool lockTaken = false;
            TimeSpan ts = TimeSpan.FromMilliseconds(-1);
            try
            {
                Monitor.TryEnter(_indexLock, ts, ref lockTaken);
                if (lockTaken)
                    while (true) 
                    {
                        if (_nextIndex < (Numbers.Length - 1) && Numbers[_nextIndex].Taken)
                            ++_nextIndex;
                        else
                            break;
                    }
                return _nextIndex;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_indexLock);

            }
            
        }
    }

    public class SafeNumber
    {
        public long Index { get; set; }
        public long Number { get; set; }
        public bool Taken { get; set; }
    }


}
