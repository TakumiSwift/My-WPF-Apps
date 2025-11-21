using System.Diagnostics;
using System.Security.Cryptography;

namespace ThreadsCA
{
    internal class Program
    {

        static ulong sum = 0;
        static ulong multi = 0;

        static void SumOfNumbers(ulong firstNumber, ulong lastNumber)
        {
            ulong i = firstNumber;
            while(true)
            {
                sum += firstNumber + (firstNumber + 1);
                ++i;
                if (i == lastNumber) break;
            }
        }

        static void Multiplicity(ulong firstNumber, ulong lastNumber)
        {
            ulong i = firstNumber;
            while(true)
            {
                multi += firstNumber * (firstNumber + 1);
                ++i;
                if (i == lastNumber) break;
            }
        }

        static void Main(string[] args)
        {
            int i = DateTime.Now.Millisecond;
            var task = Task.Run(() => SumOfNumbers(1000000, 2000000));
            var tasK = Task.Run(() => Multiplicity(1000000, 2000000));
            Task.WaitAll(task, tasK);
            task.Dispose();
            tasK.Dispose();
            Console.WriteLine(sum + " " +  " " + multi + " " );
            Console.WriteLine(DateTime.Now.Millisecond - i);
        }
    }
}
