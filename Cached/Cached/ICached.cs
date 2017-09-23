namespace Dagaren.Cached
{
    public interface ICached<T>
    {
        T Value { get; }

        void Invalidate();
    }
}