using System;

namespace JabbR.Infrastructure
{
    public interface IBackplaneChannel
    {
        void Subscribe<T>(T instance) where T : IDisposable;

        void Unsubscribe<T>(T instance) where T : IDisposable;

        void Invoke<T>(string methodName, object[] arguments);
    }
}
