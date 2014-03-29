using System;

namespace JabbR.Infrastructure
{
    public class NullBackplaneChannel : IBackplaneChannel
    {
        public void Subscribe<T>(T instance) where T : IDisposable
        {
        }

        public void Unsubscribe<T>(T instance) where T : IDisposable
        {
        }

        public void Invoke<T>(string funcName, object[] arguments)
        {
        }
    }
}