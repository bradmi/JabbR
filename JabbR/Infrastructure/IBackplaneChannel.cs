namespace JabbR.Infrastructure
{
    public interface IBackplaneChannel
    {
        void Subscribe<T>(T instance);

        void Invoke<T>(string funcName, object[] arguments);
    }
}
