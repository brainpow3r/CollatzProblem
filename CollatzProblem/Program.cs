using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CollatzProblem
{
    class Program
    {
        static long maxSteps = 0;
        static object _lock = new object();
        static readonly Stopwatch t = new Stopwatch();

        static void Main(string[] args)
        {
            int numberOfThreads = 16;
            long[] numbers = new long[100000];
            InitData(ref numbers);

            if (args.Length == 0)
                Console.WriteLine("Number of threads to use was not supplied. 4 Threads will be used");
            else
                numberOfThreads = int.Parse(args[0]);

            t.Start();
            /*foreach (long number in numbers)
                Collatz(number);*/

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = numberOfThreads;

            Parallel.ForEach(numbers, po, (currentNumber) =>
            {
                Collatz(currentNumber);
            });
            t.Stop();

            Console.WriteLine("Max steps: {0}", maxSteps);
            Console.WriteLine("Executed in {0}ms", t.ElapsedMilliseconds);

        }

        public static void Collatz(long number)
        {
            long steps = 0;
            long originalNumber = number;

            while (number != 1)
            {
                if (number % 2 != 0)
                {
                    number = 3 * number + 1;
                } else
                {
                    number = number / 2;
                }
                steps++;

            }
            //Console.WriteLine("[{0}]Collatz({1}) steps : {2}     Duration: {3}ms", Thread.CurrentThread.ManagedThreadId, originalNumber, steps, t.ElapsedMilliseconds);

            lock(_lock)
            {
                maxSteps = (maxSteps <= steps) ? steps : maxSteps;
            }
        }

        public static void InitData(ref long[] numbers)
        {
            Random r = new Random(int.MaxValue);
            for (int i = 0; i < 100000; i++)
                numbers[i] = r.Next();
        }
    }
}
