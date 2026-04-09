using System;
#if !NET452
using System.Runtime.InteropServices;
#endif
using System.Threading;

namespace UnitTests
{
    /// <summary>
    /// Description of ThreadWrapper.
    /// </summary>
    public static class ThreadExecutor
    {
        public static void RunCodeAsSTA(AutoResetEvent are, ThreadStart originalDelegate)
        {
            Thread thread = new Thread(
                delegate()
                {
                    try
                    {
                        originalDelegate.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred {0}", ex.Message);
                    }

                    are.Set();
                });
#if !NET452
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                thread.SetApartmentState(ApartmentState.STA);
            }
#else
            // On .NET Framework, we're always on Windows
            thread.SetApartmentState(ApartmentState.STA);
#endif

            thread.Start();
        }
    }
}