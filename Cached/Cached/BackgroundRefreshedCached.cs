namespace Dagaren.Cached
{
    using System;
    using System.Threading.Tasks;

    public class BackgroundRefreshedCached<T> : ICached<T>
    {
        private T value;

        private bool intialized = false;

        private readonly ICached<T> innerCache;

        private readonly object lockObject = new object();

        public BackgroundRefreshedCached(ICached<T> innerCache)
        {
            this.innerCache = innerCache ?? throw new ArgumentNullException("innerCache");
        }

        public T Value
        {
            get
            {
                if (this.intialized == false)
                {
                    lock (this.lockObject)
                    {
                        if (this.intialized == false)
                        {
                            RefreshValue();
                            this.intialized = true;
                        }
                    }
                }

                return this.value;
            }
        }

        public void Invalidate()
        {
            this.innerCache.Invalidate();

            if(this.intialized == true)
            {
                Task.Run(() => { RefreshValue(); });
            }
        }

        private void RefreshValue()
        {
            this.value = this.innerCache.Value;
        }
    }
}
