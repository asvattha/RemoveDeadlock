using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RemoveDeadlock
{
    class Program
    {

        public const int NUM_PHILOSOPHERS = 5;
        public const int THINK_TIME = 10;
        public const int EAT_TIME = 10;
        public const int LOCK_TIMEOUT = 15;
        public const int RECOVERY_TIME = 15;
        public const int RUN_TIME = 10000;

        public static object[] chopstick = new object[NUM_PHILOSOPHERS];

        public static Thread[] philosopher = new Thread[NUM_PHILOSOPHERS];

        public static Stopwatch stopwatch = new Stopwatch();

        public static Dictionary<int, int> eatingTime = new Dictionary<int, int>();

        public static object eatingSync = new object();

        public static void Think()
        {
            Random random = new Random();
            Thread.Sleep(random.Next(THINK_TIME));
        }

        public static void Eat(int index)
        {
            Random random = new Random();
            int time_spent_eating = random.Next(EAT_TIME);
            Thread.Sleep(time_spent_eating);

            lock (eatingSync)
            {
                eatingTime[index] += time_spent_eating;
            }
        }

        public static void DoWork(int index, object chopstick1, object chopstick2)
        {
            while (stopwatch.ElapsedMilliseconds < RUN_TIME)
            {
                bool lockTaken1 = false;
                bool lockTaken2 = false;

               

                try
                {
                    Monitor.TryEnter(chopstick1, LOCK_TIMEOUT, ref lockTaken1);
                    if (lockTaken1)
                    {
                        try
                        {
                            Monitor.TryEnter(chopstick2, LOCK_TIMEOUT, ref lockTaken2);
                            if (lockTaken2)
                            {
                                Eat(index);
                            }
                            else
                            {
                                Random random = new Random();
                                int time_spent_eating = random.Next(RECOVERY_TIME);
                                Thread.Sleep(time_spent_eating);
                            }
                        }
                        finally
                        {
                            if (lockTaken2)
                                Monitor.Exit(chopstick2);
                        }
                    }
                    else
                    {
                        Random random = new Random();
                        int time_spent_eating = random.Next(RECOVERY_TIME);
                        Thread.Sleep(time_spent_eating);
                    }
                }
                finally
                {
                    if (lockTaken1)
                        Monitor.Exit(chopstick1);
                }

                if (lockTaken1 & lockTaken2)
                {
                    Think();
                }
            }
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                eatingTime.Add(i, 0);
            }

            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                chopstick[i] = new object();
            }

            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                int index = i;
                object chopstick1 = chopstick[i];
                object chopstick2 = chopstick[(i + 1) % NUM_PHILOSOPHERS];
                philosopher[i] = new Thread(() => DoWork(index, chopstick1, chopstick2));
            }

            stopwatch.Start();
            Console.WriteLine("Starting philosophers...");
            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                philosopher[i].Start();
            }

            for(int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                philosopher[i].Join();
            }
            Console.WriteLine("Alll philosophers are finished");

            int total_eating_time = 0;
            for(int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                Console.WriteLine("Philosopher {0} has eaten for {1} ms", i , eatingTime[i]);
                total_eating_time += eatingTime[i];
            }

            Console.WriteLine("Total time spend eating: {0} ms", total_eating_time);
        }
    }
}
