using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncExperiments.TapAndEapSamples
{
    /// <summary>
    /// DISCLAIMER: Some of this code is taken from Pluralsight course, TPL Async Module 1.
    /// TODO: Refactor and separate all scenarios so it doesn't need commenting or uncommenting code
    /// </summary>
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            ShowWindow(ThisConsole, MAXIMIZE);

            // works with conditions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Menu();

            autoResetEvent.WaitOne();
        }

        private static void Menu()
        {
            Console.WriteLine("SELECT AN OPERATION:");
            Console.WriteLine("");
            Console.WriteLine("1. UsingTaskFactory - Uses Task.Factory.StartNew and continuation with success");
            Console.WriteLine("2. UsingTaskDirectly - Same operation but this time it uses DownloadStringTaskAsync and continuation with success");
            Console.WriteLine("3. UsingEventBasedAsynchronousPattern - Same operation with Event-based Asynchronous Pattern");
            Console.WriteLine("4. UsingTaskDirectlySimulatingFaults - Some error handling experiments");
            Console.WriteLine("5. UsingTaskDirectlySimulatingFaults_NotCrashingProcessVariant - Simulating faults without crashing the process");
            Console.WriteLine("6. UsingTaskDirectlySimulatingFaults_CrashingProcessVariant - Simulating faults with crashing the process");
            Console.WriteLine("7. UsingTaskDirectlySimulatingFaults_AbandoningTaskException - In this scenario, task exception is abandoned. If you follow given instructions in the code this unobserved exception will be handled by UnobservedTaskException event");
            Console.WriteLine("8. UsingContinuations_OnlyOnSuccess - Continuation scenario, works only antecedent task is successful");
            Console.WriteLine("9. UsingContinuations_OnlyOnFault - Continuation scenario, works only antecedent task is faulted");
            Console.WriteLine("10. UsingComposition_WhenAll_AllSuccess - Composition scenario, all sub tasks are successful");
            Console.WriteLine("11. UsingComposition_WhenAll_OneFail - Composition scenario, one of sub tasks will fail");
            Console.WriteLine("12. UsingComposition_WhenAll_AllFail - Composition scenario, both sub tasks will fail");
            Console.WriteLine("13. UsingComposition_ContinueWhenAll_OneFail - Composition scenario with ContinueWhenAll, one sub tasks will fail but we can still track the other");
            Console.WriteLine("14. ConsumeCustomTask - Create a custom task with TaskCompletionSource<T> and experiment with success and error scenarios");
            Console.WriteLine("Hit e/E for exit");
            Console.WriteLine("");

            var result = Console.ReadLine();

            switch (result)
            {
                case "1":
                    UsingTaskFactory();
                    break;
                case "2":
                    UsingTaskDirectly();
                    break;
                case "3":
                    UsingEventBasedAsynchronousPattern();
                    break;
                case "4":
                    UsingTaskDirectlySimulatingFaults();
                    break;
                case "5":
                    UsingTaskDirectlySimulatingFaults_NotCrashingProcessVariant();
                    break;
                case "6":
                    UsingTaskDirectlySimulatingFaults_CrashingProcessVariant();
                    break;
                case "7":
                    UsingTaskDirectlySimulatingFaults_AbandoningTaskException();
                    break;
                case "8":
                    UsingContinuations_OnlyOnSuccess();
                    break;
                case "9":
                    UsingContinuations_OnlyOnFault();
                    break;
                case "10":
                    UsingComposition_WhenAll_AllSuccess();
                    break;
                case "11":
                    UsingComposition_WhenAll_OneFail();
                    break;
                case "12":
                    UsingComposition_WhenAll_AllFail();
                    break;
                case "13":
                    UsingComposition_ContinueWhenAll_OneFail();
                    break;
                case "14":
                    ConsumeCustomTask(false);
                    break;
                case "e":
                case "E":
                    autoResetEvent.Set();
                    break;
                default:
                    UsingTaskFactory();
                    break;
            }
        }

        private static void ReturnToMenu()
        {
            Console.WriteLine("Do you want to return to Menu ? (Y/N)");
            var menuAnswer = Console.ReadLine();
            switch (menuAnswer)
            {
                case "y":
                case "Y":
                    Console.Clear();
                    Menu();
                    break;
                case "n":
                case "N":
                    Console.WriteLine("Process will exit after Enter");
                    autoResetEvent.Set();
                    break;
                default:
                    Console.WriteLine("Process will exit after Enter");
                    autoResetEvent.Set();
                    break;
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("UnobservedTaskException, here's the Exception");
            Console.WriteLine(e.Exception);
            e.SetObserved(); // stop the escalation
        }

        private static void UsingTaskFactory()
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

                ReturnToMenu();
            });

            Console.WriteLine("Continuing on main thread");
        }

        private static void UsingTaskDirectly()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getTask = webClient.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine(t.Result);

                ReturnToMenu();
            });

            Console.WriteLine("Continuing on main thread");
        }

        /// <summary>
        /// Previous asynchronous model before TPL, before that there was APM but it's ancient coming from .NET 1.0
        /// EAP is .NET >= 2.0 and TPL is .NET >= 4.0
        /// </summary>
        private static void UsingEventBasedAsynchronousPattern()
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

            ReturnToMenu();
        }

        private static void UsingTaskDirectlySimulatingFaults()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getTask = webClient.DownloadStringTaskAsync("https://localhost:20202");

            Console.WriteLine("Setting up continuation");
            // When a task faults, it's considered to be complete (a completed task doesn't mean it is successful)
            // meaning that anything that's been waiting for the task to finish will proceed
            // So associated continuations will run
            getTask.ContinueWith(t =>
            {
                // if you attempt to read the Result property of a task that has failed, it'll throw an exception to you
                // there're two properties to check if a task is successful or not, those properties are IsFaulted and Status
                Console.WriteLine($"Is task successful: {!t.IsFaulted}"); // false
                Console.WriteLine($"Task's status is {t.Status}"); // Faulted

                if (t.IsFaulted)
                {
                    Console.WriteLine("Task is faulted, that means task's Exception property is not null");
                    // This is an AggregateException, not the original Exception
                    Console.WriteLine(t.Exception);
                }

                // Original context is preserved, a courtesy of .NET 4.5
                // Inner Exception 1: WebException: Unable to connect remote server
                // Inner Exception 2: SocketException: No connection could be made because the target machine actively refused it 127.0.0.1:20202

                // A task's Result property can be read many times so we'll use this
                try
                {
                    Console.WriteLine(t.Result);
                }
                catch (AggregateException)
                {
                    Console.WriteLine("I don't care about Exception's content here");
                }

                // Exception is not handled, will throw an AggregateException
                // This Exception will escalate, escalation means crashing the process in .NET 4.0
                // However in .NET 4.5, this behaviour is changed. As of .NET 4.5, unobserved exceptions caused by abandoned tasks will not crash the process by default
                Console.WriteLine(t.Result);

                // Wait, WaitForAll, WaitForAny methods also throw an exception if task is faulted

                ReturnToMenu();
            });

            Console.WriteLine("Continuing on main thread");
        }

        private static void UsingTaskDirectlySimulatingFaults_NotCrashingProcessVariant()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // this doesn't crash the process
            webClient.OpenWriteTaskAsync(new Uri("https://localhost:20202")).ContinueWith(t =>
            {
                Console.WriteLine(t.Result);

                ReturnToMenu();
            });

            Console.WriteLine("If we use Wait, process will crash");
            Console.ReadLine();
        }

        private static void UsingTaskDirectlySimulatingFaults_CrashingProcessVariant()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // but this does
            webClient.OpenWriteTaskAsync(new Uri("https://localhost:20202")).Wait();

            Console.WriteLine("If we use Wait, process will crash");
            Console.ReadLine();
        }

        // For UnobservedTaskException to work:
        // 1. TPL depends on the Garbage Collector's finalization mechanism to detect unobserved exceptions. This code doesn't do anything so unless we force one
        // we'll never going to see garbage collection (GC.Collect)
        // 2. Program needs to be built in Release mode
        // 3. Start without debugging (Ctrl+F5)
        private static void UsingTaskDirectlySimulatingFaults_AbandoningTaskException()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            webClient.OpenWriteTaskAsync(new Uri("https://localhost:20202")).ContinueWith(t => {
                Console.WriteLine("This task will throw an exception but since there's no call to Result or Wait, exception is unobserved");
                Console.WriteLine("If you follow given instructions, this unobserved exception will be handled by TaskScheduler's UnobservedTaskException event");
                Console.WriteLine("If you don't, nothing will happen");

                ReturnToMenu();
            });

            Console.WriteLine("Continuing on main thread");
            Console.ReadLine();

            GC.Collect();
        }

        private static void UsingContinuations_OnlyOnSuccess()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getTask = webClient.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                // only run if there's no fault
                Console.WriteLine(t.Result);

                ReturnToMenu();
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            Console.WriteLine("Continuing on main thread");
        }

        private static void UsingContinuations_OnlyOnFault()
        {
            var webClient = new WebClient();
            Console.WriteLine("Starting work");

            Task<string> getTask = webClient.DownloadStringTaskAsync("https://localhost:20202");

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                // only run if task is faulted
                Console.WriteLine(t.Exception);

                ReturnToMenu();
            }, TaskContinuationOptions.OnlyOnFaulted);

            Console.WriteLine("Continuing on main thread");
        }

        // Task.WhenAll is used for task composition. Composed task is considered as successful only if all individual tasks are successful
        // In this scenario, all tasks are successful so composed task is also successful therefore continuation will hit Result
        private static void UsingComposition_WhenAll_AllSuccess()
        {
            var webClient1 = new WebClient();
            var webClient2 = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getSubTask1 = webClient1.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");
            Task<string> getSubTask2 = webClient2.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/wwii/field-marshal-erwin-rommel-the-battle-of-kasserine-pass/");

            Task<string[]> getTask = Task.WhenAll(getSubTask1, getSubTask2);

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine("Completed");
                if (t.IsFaulted)
                {
                    // 
                    Console.WriteLine(t.Exception);
                }
                else {
                    // t.Result will be a string[] so ToString wont help
                    Console.WriteLine(string.Join(Environment.NewLine, t.Result));

                    ReturnToMenu();
                }
            });

            Console.WriteLine("Continuing on main thread");
        }

        // This time getSubTask2 will fail, this failure causes composed task to fail
        // There's no partial success or partial failure with WhenAll, if one individual task fails, composed task always fails
        private static void UsingComposition_WhenAll_OneFail()
        {
            var webClient1 = new WebClient();
            var webClient2 = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getSubTask1 = webClient1.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");
            Task<string> getSubTask2 = webClient2.DownloadStringTaskAsync("https://localhost:20202");

            Task<string[]> getTask = Task.WhenAll(getSubTask1, getSubTask2);

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine("Completed");
                if (t.IsFaulted)
                {
                    Console.WriteLine($"Inner exceptions count: {t.Exception.InnerExceptions.Count}");
                    Console.WriteLine(t.Exception);

                    ReturnToMenu();
                }
                else
                {
                    Console.WriteLine(t.Result);
                }
            });

            Console.WriteLine("Continuing on main thread");
        }

        // This time both sub tasks will fail, composed task will also fail
        // Only difference with UsingComposition_WhenAll_OneFail is composed task's Exception property
        // With both sub tasks fail, composed task's Exception property will include both of sub tasks Exceptions inside its InnerExceptions property
        private static void UsingComposition_WhenAll_AllFail()
        {
            var webClient1 = new WebClient();
            var webClient2 = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getSubTask1 = webClient1.DownloadStringTaskAsync("https://localhost:20203");
            Task<string> getSubTask2 = webClient2.DownloadStringTaskAsync("https://localhost:20202");

            Task<string[]> getTask = Task.WhenAll(getSubTask1, getSubTask2);

            Console.WriteLine("Setting up continuation");
            getTask.ContinueWith(t =>
            {
                Console.WriteLine("Completed");
                if (t.IsFaulted)
                {
                    Console.WriteLine($"Inner exceptions count: {t.Exception.InnerExceptions.Count}");
                    Console.WriteLine(t.Exception);

                    ReturnToMenu();
                }
                else
                {
                    Console.WriteLine(t.Result);
                }
            });

            Console.WriteLine("Continuing on main thread");
        }

        // If we need to track each one of sub tasks, WhenAll is meaningless because it's a one-for-all mechanism
        private static void UsingComposition_ContinueWhenAll_OneFail()
        {
            var webClient1 = new WebClient();
            var webClient2 = new WebClient();
            Console.WriteLine("Starting work");

            // This time we're using DownloadStringTaskAsync method and this time it's async
            Task<string> getSubTask1 = webClient1.DownloadStringTaskAsync("https://warfarehistorynetwork.com/daily/military-history/trafalgar-in-reverse-the-battle-of-jutland/");
            Task<string> getSubTask2 = webClient2.DownloadStringTaskAsync("https://localhost:20202");

            Console.WriteLine($"Setting up continuation for tasks {getSubTask1.Id} and {getSubTask2.Id}");

            Task.Factory.ContinueWhenAll(new[] { getSubTask1, getSubTask2 }, 
                (Task<string>[] tasks) => {
                    foreach (var t in tasks)
                    {
                        Console.WriteLine($"Completed task: {t.Id}");
                        if (t.IsFaulted)
                        {
                            Console.WriteLine($"Inner exceptions count for task {t.Id}: {t.Exception.InnerExceptions.Count}");
                            Console.WriteLine(t.Exception);
                        }
                        else
                        {
                            Console.WriteLine($"Task {t.Id} is successful");
                            Console.WriteLine(t.Result);
                        }
                    }

                    ReturnToMenu();
                });

            Console.WriteLine("Continuing on main thread");
        }

        private static Task<int> CustomTask(bool createException)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

            if (!createException)
            {
                taskCompletionSource.SetResult(5000);
            }
            else {
                taskCompletionSource.SetException(new Exception("Simulate an exception"));
            }

            return taskCompletionSource.Task;
        }

        private static void ConsumeCustomTask(bool createException)
        {
            CustomTask(createException).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("An error occured ...");
                    Console.WriteLine(t.Exception);
                }
                else {
                    Console.WriteLine("Our custom task is successful");
                    Console.WriteLine(t.Result);
                }

                ReturnToMenu();
            });
        }
    }
}
