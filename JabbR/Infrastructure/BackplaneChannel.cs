using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;

using Newtonsoft.Json;

namespace JabbR.Infrastructure
{
    public class BackplaneChannel : ISubscriber, IBackplaneChannel
    {
        private const string BackplaneSignal = "__BACKPLANE__";

        private readonly List<string> _signals = new List<string>{ BackplaneSignal };

        private readonly JsonSerializer _jsonSerializer;

        private readonly IServerIdManager _serverIdManager;

        private readonly IMessageBus _messageBus;

        private readonly Dictionary<string, BackplaneChanelRegistration> _subscriptions = new Dictionary<string, BackplaneChanelRegistration>();

        public BackplaneChannel(JsonSerializer jsonSerializer, IServerIdManager serverIdManager, IMessageBus messageBus)
        {
            _jsonSerializer = jsonSerializer;
            _serverIdManager = serverIdManager;
            _messageBus = messageBus;
        }

        public void Subscribe()
        {
            _messageBus.Subscribe(this, cursor: null, callback: HandleMessages, maxMessages: 10, state: null);
        }

        public void Subscribe<T>(T instance) where T : IDisposable
        {
            string typeName = typeof(T).FullName;
            foreach (var method in typeof(T).GetMethods())
            {
                _subscriptions.Add(typeName + '+' + method.Name, new BackplaneChanelRegistration
                    {
                        Instance = instance,
                        Method = method 
                    });
            }
        }

        public void Unsubscribe<T>(T instance) where T : IDisposable
        {
            foreach (var key in _subscriptions.Keys)
            {
                if (_subscriptions[key].Instance == (object)instance)
                {
                    _subscriptions.Remove(key);
                }
            }
        }

        public void Invoke<T>(string methodName, object[] arguments)
        {
            var message = new BackplaneChannelMessage
                {
                    Subscription = typeof(T).FullName + '+' + methodName, 
                    Arguments = arguments.Select(e => _jsonSerializer.Stringify(e)).ToArray()
                };
            SendMessage(message);
        }

        private Task<bool> HandleMessages(MessageResult result, object state)
        {
            result.Messages.Enumerate<object>(
                m => _signals.Any(s => s == m.Key),
                (s, m) =>
                {
                    // ignore the message if we sent it in the first place
                    if (m.Source == _serverIdManager.ServerId)
                    {
                        return;
                    }

                    // find a subscription
                    var message = _jsonSerializer.Parse<BackplaneChannelMessage>(m.Value, m.Encoding);

                    BackplaneChanelRegistration subscription;
                    if (_subscriptions.TryGetValue(message.Subscription, out subscription))
                    {
                        // deserialize parameters
                        var parameters = subscription.Method.GetParameters();
                        var args = new object[parameters.Length];

                        for (var idx = 0; idx < parameters.Length; idx++)
                        {
                            using (var paramReader = new StringReader(message.Arguments[idx]))
                            {
                                args[idx] = _jsonSerializer.Deserialize(paramReader, parameters[idx].ParameterType);
                            }
                        }

                        subscription.Method.Invoke(subscription.Instance, args);
                    }
                },
                state: null);

            return Task.FromResult(true);
        }

        private void SendMessage<T>(T message)
        {
            _messageBus.Publish(new Message(_serverIdManager.ServerId, BackplaneSignal, _jsonSerializer.Stringify(message)));
        }

        event Action<ISubscriber, string> ISubscriber.EventKeyAdded
        {
            add { }
            remove { }
        }

        event Action<ISubscriber, string> ISubscriber.EventKeyRemoved
        {
            add { }
            remove { }
        }

        IList<string> ISubscriber.EventKeys
        {
            get
            {
                return _signals;
            }
        }

        string ISubscriber.Identity
        {
            get
            {
                return _serverIdManager.ServerId;
            }
        }

        Subscription ISubscriber.Subscription { get; set; }

        Action<TextWriter> ISubscriber.WriteCursor { get; set; }

        private class BackplaneChannelMessage
        {
            public string Subscription { get; set; }
            public string[] Arguments { get; set; }
        }

        private class BackplaneChanelRegistration
        {
            public MethodInfo Method { get; set; }
            public object Instance {get; set; }
        }
    }
}