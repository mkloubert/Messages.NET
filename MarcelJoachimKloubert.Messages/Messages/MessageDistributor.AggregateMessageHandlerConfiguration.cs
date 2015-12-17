using System;
using System.Collections.Generic;
using System.Linq;

namespace MarcelJoachimKloubert.Messages
{
    partial class MessageDistributor
    {
        internal class AggregateMessageHandlerConfiguration : MarshalByRefObject,
                                                              IMessageHandlerConfiguration
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
                    return Configurations.Select(x => (bool?)x.OwnsHandler)
                                         .Distinct()
                                         .SingleOrDefault() ?? _ownsHandler;
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