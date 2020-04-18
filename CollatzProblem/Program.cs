using System;

namespace CollatzProblem
{
    class Program
    {
        static void Main(string[] args)
        {
            Collatz(new long[5] { 1477, 1311239111, 7, 3, 19 });
        }

        static void Collatz(long[] numbers)
        {
            long currentNumber = 0;
            
            long maxSteps = 0;
            
            foreach(long number in numbers)
            {
                Console.WriteLine("Collatz() with {0}", number);
                currentNumber = number;
                long steps = 0;
                while (currentNumber != 1)
                {
                    if (currentNumber % 2 != 0)
                    {
                        currentNumber = 3 * currentNumber + 1;
                    } else
                    {
                        currentNumber = currentNumber / 2;
                    }
                    Console.Write($"{currentNumber} ");
                    steps++;

                }
                maxSteps = (steps >= maxSteps) ? steps : maxSteps;
                Console.WriteLine();
            }

            Console.WriteLine("Max iterations: {0}", maxSteps);
        }
    }
}
