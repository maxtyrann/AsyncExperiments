using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncExperiments.Threaded
{
    /// <summary>
    /// DISCLAIMER: This code is taken from Pluralsight course, TPL Async
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // MultiThreadedTest();
            // AsyncTest();

            EapTest();

            Console.ReadLine();
        }

        private static void MultiThreadedTest()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // Works on a ThreadPool thread
            // This is NOT asynchronous programming, this is multi threaded programming
            Task<string> getTask = Task.Factory.StartNew(() =>
            {
                // DownloadString method spends almost all of its time blocked waiting for a response from web server
                // while using a thread from the thread pool to do nothing very useful
                return webClient.DownloadString("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");
            });

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine(t.Result);
            });

            Console.WriteLine("Continuing on main thread");
        }

        private static void AsyncTest()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getTask = webClient.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine(t.Result);
            });

            Console.WriteLine("Continuing on main thread");
        }

        private static void EapTest()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/"));

            Console.WriteLine("Continuing on main thread");
        }

        private static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Console.WriteLine("WebClient_DownloadStringCompleted is called as soon as we get a result");
            Console.WriteLine(e.Result);
        }
    }
}
