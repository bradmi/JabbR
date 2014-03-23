namespace JabbR.Infrastructure
{
    public class NullBackplaneChannel : IBackplaneChannel
    {
        public void Subscribe<T>(T instance)
        {
        }

        public void Invoke<T>(string funcName, object[] arguments)
        {
        }
    }
}