namespace Dagaren.Cached
{
    using System;
    using System.Threading.Tasks;

    public class BackgroundRefreshedCachedTask<T> : ICached<Task<T>>
    {
        private Task<T> value;

        private bool initialized = false;

        private readonly ICached<Task<T>> innerCache;

        private readonly object lockObject = new object();

        public BackgroundRefreshedCachedTask(ICached<Task<T>> innerCached)
        {
            this.innerCache = innerCache ?? throw new ArgumentNullException("innerCached");
        }

        public Task<T> Value
        {
            get
            {
                if(this.initialized == false)
                {
                    lock(this.lockObject)
                    {
                        if(this.initialized == false)
                        {
                            this.value = this.innerCache.Value;
                            this.initialized = true;
                        }
                    }
                }

                return this.value;
            }
        }

        public void Invalidate()
        {
            this.innerCache.Invalidate();

            if(this.initialized == true)
            {
                Task.Run(async () =>
                {
                    Task<T> temporalValue = this.innerCache.Value;
                    await temporalValue;
                    this.value = temporalValue;
                });
            }
        }
    }
}
