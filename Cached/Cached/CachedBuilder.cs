namespace Dagaren.Cached
{
    using System;
    using System.Threading.Tasks;

    public class CachedBuilder<T>
    {
        private Func<T> refreshFunction = null;

        private TimeSpan? invalidationFrequency = null;

        private bool withBackgroundRefresh = false;

        public CachedBuilder<T> WithTimeInvalidationFrequency(TimeSpan invalidationFrequency)
        {
            this.invalidationFrequency = invalidationFrequency;

            return this;
        }

        public CachedBuilder<T> WithRefreshFunction(Func<T> refreshFunction)
        {
            this.refreshFunction = refreshFunction;

            return this;
        }

        public CachedBuilder<T> WithBackgroundRefresh(bool withBackgroundRefresh = true)
        {
            this.withBackgroundRefresh = withBackgroundRefresh;

            return this;
        }

        public ICached<T> Build()
        {
            ICached<T> cached = BuildBaseCached();
            cached = BuildBackgroundRefreshedCached(cached);
            cached = BuildTimedInvalidationCached(cached);

            return cached;
        }

        private ICached<T> BuildBaseCached()
        {
            if (this.refreshFunction == null)
            {
                throw new InvalidOperationException("No refresh function has been provided");
            }

            return new Cached<T>(this.refreshFunction);
        }

        private ICached<T> BuildBackgroundRefreshedCached(ICached<T> innerCached)
        {
            if (this.withBackgroundRefresh == true)
            {
                if(ParameterIsTask())
                {
                    Type cachedType = typeof(BackgroundRefreshedCachedTask<>).MakeGenericType(GetParameterTaskType());
                    return (ICached<T>)Activator.CreateInstance(cachedType, innerCached);
                }
                else
                {
                    return new BackgroundRefreshedCached<T>(innerCached);
                }
            }

            return innerCached;
        }

        private ICached<T> BuildTimedInvalidationCached(ICached<T> innerCached)
        {
            if (this.invalidationFrequency.HasValue)
            {
                return new TimedInvalidationCached<T>(innerCached, this.invalidationFrequency.Value);
            }

            return innerCached;
        }

        private bool ParameterIsTask()
        {
            Type parameterType = typeof(T);
            
            if(parameterType.IsGenericType)
            {
                Type genericParameterType = parameterType.GetGenericTypeDefinition();

                if(genericParameterType == typeof(Task<>))
                {
                    return true;
                }
            }

            return false;
        }

        private Type GetParameterTaskType()
        {
            return typeof(T).GetGenericArguments()[0];
        }
    }
}
