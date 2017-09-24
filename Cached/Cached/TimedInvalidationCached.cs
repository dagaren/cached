namespace Dagaren.Cached
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class TimedInvalidationCached<T> : ICached<T>, IDisposable
    {
        private ICached<T> innerCached;

        private TimeSpan invalidationFrequency;

        private CancellationTokenSource cancellationTokenSource;

        private bool intialized = false;

        private object lockObject = new object();

        public TimedInvalidationCached(ICached<T> innerCached, TimeSpan invalidationFrequency)
        {
            this.innerCached = innerCached ?? throw new ArgumentNullException("innerCached");

            this.invalidationFrequency = invalidationFrequency;

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public T Value
        {
            get
            {
                if(intialized == false)
                {
                    lock(lockObject)
                    {
                        if(intialized == false)
                        {
                            this.Run();
                            intialized = true;
                        }
                    }
                }

                return innerCached.Value;
            }
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void Invalidate()
        {
            this.innerCached.Invalidate();
        }

        private void Run()
        {
            Task.Run(async () => {
                while (!this.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(this.invalidationFrequency, this.cancellationTokenSource.Token);
                        this.Invalidate();
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            });
        }
    }
}
