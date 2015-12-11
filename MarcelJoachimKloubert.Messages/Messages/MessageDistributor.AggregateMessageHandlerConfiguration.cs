using System;
using System.Collections.Generic;
using System.Linq;

namespace MarcelJoachimKloubert.Messages
{
    partial class MessageDistributor
    {
        internal class AggregateMessageHandlerConfiguration : IMessageHandlerConfiguration
        {
            #region Fields (2)

            internal ICollection<IMessageHandlerConfiguration> Configurations;
            private bool _ownsHandler;

            #endregion Fields (2)

            #region Properties (1)

            public bool OwnsHandler
            {
                get
                {
                    if (Configurations.Count < 1)
                    {
                        return _ownsHandler;
                    }

                    return Configurations.Select(x => x.OwnsHandler)
                                         .Distinct()
                                         .SingleOrDefault();
                }

                set
                {
                    _ownsHandler = value;
                    InvokeForMessageConfigurationList(x => x.OwnsHandler = value);
                }
            }

            #endregion Properties (1)

            #region Methods (5)

            private void InvokeForMessageConfigurationList(Action<IMessageHandlerConfiguration> action)
            {
                using (var e = Configurations.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        action(e.Current);
                    }
                }
            }

            public IMessageHandlerConfiguration RegisterForReceive<TMsg>()
            {
                InvokeForMessageConfigurationList(x => x.RegisterForReceive<TMsg>());
                return this;
            }

            public IMessageHandlerConfiguration RegisterForReceive(Type msgType)
            {
                InvokeForMessageConfigurationList(x => x.RegisterForReceive(msgType: msgType));
                return this;
            }

            public IMessageHandlerConfiguration RegisterForSend<TMsg>()
            {
                InvokeForMessageConfigurationList(x => x.RegisterForSend<TMsg>());
                return this;
            }

            public IMessageHandlerConfiguration RegisterForSend(Type msgType)
            {
                InvokeForMessageConfigurationList(x => x.RegisterForSend(msgType: msgType));
                return this;
            }

            #endregion Methods (5)
        }
    }
}