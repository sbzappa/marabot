
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaraBot.Core
{
    /// <summary>
    /// Registry of mutexes used in commands.
    /// </summary>
    public class MutexRegistry : IDisposable
    {
        public SemaphoreSlim RandomizerExecutableMutex { get; private set; }
        public SemaphoreSlim WeeklyWriteAccessMutex { get; private set; }

        public MutexRegistry()
        {
            RandomizerExecutableMutex = new SemaphoreSlim(1);
            WeeklyWriteAccessMutex = new SemaphoreSlim(1);
        }

        public void Dispose()
        {
            RandomizerExecutableMutex.Dispose();
            WeeklyWriteAccessMutex.Dispose();
        }
    }

    /// <summary>
    /// Disposable mutex lock.
    /// </summary>
    public class MutexLock : IDisposable
    {
        SemaphoreSlim m_Mutex;

        MutexLock(SemaphoreSlim mutex)
        {
            m_Mutex = mutex;
        }

        public static async Task<MutexLock> WaitAsync(SemaphoreSlim mutex)
        {
            var mutexLock = new MutexLock(mutex);
            await mutex.WaitAsync();
            return mutexLock;
        }

        public void Dispose()
        {
            m_Mutex.Release();
        }
    }
}
