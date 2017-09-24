namespace Dagaren.Cached
{
    using System;
    using System.Threading.Tasks;

    public class BackgroundRefreshedCachedTask<T> : ICached<Task<T>>
    {
        private Task<T> value;

        private bool initialized = false;

        private readonly ICached<Task<T>> innerCached;

        private readonly object lockObject = new object();

        public BackgroundRefreshedCachedTask(ICached<Task<T>> innerCached)
        {
            this.innerCached = innerCached ?? throw new ArgumentNullException("innerCached");
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
                            this.value = this.innerCached.Value;
                            this.initialized = true;
                        }
                    }
                }

                return this.value;
            }
        }

        public void Invalidate()
        {
            this.innerCached.Invalidate();

            if(this.initialized == true)
            {
                Task.Run(async () =>
                {
                    Task<T> temporalValue = this.innerCached.Value;
                    await temporalValue;
                    this.value = temporalValue;
                });
            }
        }
    }
}
