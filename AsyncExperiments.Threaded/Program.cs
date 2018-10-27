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
            Console.ReadLine();
        }
    }
}
