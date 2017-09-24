namespace Dagaren.Cached
{
    using System;

    public class Cached<T> : ICached<T>
    {
        private T value;

        private bool isValid = false;

        private Func<T> refreshFunction;

        private readonly object lockObject = new object();

        public Cached(Func<T> refreshFunction)
        {
            this.refreshFunction = refreshFunction ?? throw new ArgumentNullException("refreshFunction");
        }

        public T Value
        {
            get
            {
                if(this.isValid == false)
                {
                    lock(this.lockObject)
                    {
                        if(this.isValid == false)
                        {
                            this.value = this.refreshFunction.Invoke();
                            this.isValid = true;
                        }
                    }
                }

                return this.value;
            }
        }

        public void Invalidate()
        {
            this.isValid = false;
        }
    }
}
