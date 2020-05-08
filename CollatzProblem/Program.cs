using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CollatzProblem
{
    public class Program
    {
        static void Main(string[] args)
        {
            long baseTime = 0;
            int threadsToUse = 16;
            long itemCount = 1000000;

            if (args.Length == 0)
                Console.WriteLine("Number of threads to use was not supplied. 1 Thread will be used");
            else
            {
                threadsToUse = int.Parse(args[0]);
                itemCount = long.Parse(args[1]);
            }

            long[] numbers = new long[itemCount];
            // Initialize array with random nubers ranging from 0 to MaxInt
            InitData(ref numbers);

            Console.WriteLine("[MaxIteration] [NumberOfThreads] [Workload] [TimeMs] [Speedup]");
            int[] t = new int[] { 1, 2, 4, 8, 12 };
            for (int i = 0; i < 5; i++)
            {
                long dtime = MakePerformanceTest(numbers, t[i]);
                // Performance test with 1 thread will be our base measurement
                baseTime = (t[i] == 1) ? dtime : baseTime;
                decimal speedup = (decimal)baseTime / (decimal)dtime;
                Console.WriteLine("{0} {1} {2} {3}", t[i], itemCount, dtime, speedup);
            }
        }

        public static long MakePerformanceTest(long[] numbers, int threadCount)
        {
            Thread[] workers = new Thread[threadCount];
            Stopwatch t = new Stopwatch();
            int elementsForThread = numbers.Length / threadCount;
            t.Reset();
            t.Start();

            long maxSteps = 0;
            object _lock = new object();
            for (int i = 0; i < threadCount; i++)
            {
                int rangeStart = i * elementsForThread;
                int rangeEnd = rangeStart + elementsForThread;
                workers[i] = new Thread(() => {
                    long maxRangeIteration = Collatz(numbers, rangeStart, rangeEnd);
                    Monitor.Enter(_lock);
                        maxSteps = (maxSteps <= maxRangeIteration) ? maxRangeIteration : maxSteps;
                    Monitor.Exit(_lock);
                    });

                workers[i].Start();
            }

            for (int i = 0; i < threadCount; i++)
            {
                workers[i].Join();
            }
            Console.Write($"{maxSteps} ");

            t.Stop();
            return t.ElapsedMilliseconds;
        }

        public static long Collatz(long[] numbers, int rangeStart, int rangeEnd)
        {
            long maxSteps = 0;
            for (int j = rangeStart; j < rangeEnd; j++)
            { 
                long collatzSteps = 0;
                long currentNumber = numbers[j];

                if (currentNumber == 0)
                    continue;

                while (currentNumber != 1)
                {
                    if (currentNumber % 2 != 0)
                    {
                        currentNumber = 3 * currentNumber + 1;
                    } else
                    {
                        currentNumber = currentNumber / 2;
                    }
                    collatzSteps++;
                }
                maxSteps = (collatzSteps >= maxSteps) ? collatzSteps : maxSteps;
            }
            return maxSteps;
        }

        public static void InitData(ref long[] numbers)
        {
            Random r = new Random(int.MaxValue);
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = r.Next();

        }
    }

}
