/**********************************************************************************************************************
 * Messages.NET (https://github.com/mkloubert/Messages.NET)                                                           *
 *                                                                                                                    *
 * Copyright (c) 2015, Marcel Joachim Kloubert <marcel.kloubert@gmx.net>                                              *
 * All rights reserved.                                                                                               *
 *                                                                                                                    *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the   *
 * following conditions are met:                                                                                      *
 *                                                                                                                    *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the          *
 *    following disclaimer.                                                                                           *
 *                                                                                                                    *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the       *
 *    following disclaimer in the documentation and/or other materials provided with the distribution.                *
 *                                                                                                                    *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote    *
 *    products derived from this software without specific prior written permission.                                  *
 *                                                                                                                    *
 *                                                                                                                    *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, *
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  *
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, *
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR    *
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  *
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE   *
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                                           *
 *                                                                                                                    *
 **********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// A simple and configurable message distributor.
    /// </summary>
    public partial class MessageDistributor
    {
        #region Fields (3)

        private readonly ICollection<MessageHandlerContext> _HANDLERS;
        private readonly ModuleBuilder _MODULE_BUILDER;
        private readonly IDictionary<Type, Type> _OBJECT_TYPES;

        #endregion Fields (3)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor" /> class.
        /// </summary>
        /// <param name="syncRoot">The custom object for <see cref="MessageDistributor.SyncRoot" /> property.</param>
        public MessageDistributor(object syncRoot = null)
        {
            SyncRoot = syncRoot ?? new object();

            _HANDLERS = new List<MessageHandlerContext>();
            _OBJECT_TYPES = new Dictionary<Type, Type>();

            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm.GetName();

            var asmBuilder = AppDomain.CurrentDomain
                                      .DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            _MODULE_BUILDER = asmBuilder.DefineDynamicModule(string.Format("MJKDynamicObjectFactoryModule_{0:N}_{1}",
                                                                           Guid.NewGuid(),
                                                                           GetHashCode()));
        }

        #endregion Constructors (1)

        #region Properties (2)

        /// <summary>
        /// Gets if the distributor has been initialized or not.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public object SyncRoot { get; private set; }

        #endregion Properties (2)

        #region Methods (6)

        private static MethodInfo GetSubscribeMethod(IMessageHandlerContext ctx)
        {
            var ctxType = ctx.GetType();

            return ctxType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          .First(x =>
                          {
                              if (x.Name != "Subscribe")
                              {
                                  return false;
                              }

                              if (!x.IsGenericMethod)
                              {
                                  return false;
                              }

                              var genericParams = x.GetGenericArguments();
                              if (genericParams.Length != 1)
                              {
                                  return false;
                              }

                              var @params = x.GetParameters();
                              if (@params.Length != 1)
                              {
                                  return false;
                              }

                              var genericMessageCtxType = typeof(IMessageContext<>);
                              var messageCtxType = genericMessageCtxType.MakeGenericType(genericParams[0]);

                              var genericMethodActionType = typeof(Action<>);
                              var methodActionType = genericMethodActionType.MakeGenericType(messageCtxType);

                              return methodActionType.Equals(@params[0].ParameterType);
                          });
        }

        /// <summary>
        /// Initialized the object.
        /// </summary>
        /// <exception cref="InvalidOperationException">Object has already been initialized.</exception>
        public void Initialize()
        {
            lock (SyncRoot)
            {
                if (IsInitialized)
                {
                    throw new InvalidOperationException();
                }

                using (var e = _HANDLERS.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var ctx = e.Current;

                        var handlerType = ctx.Handler.GetType();

                        var flags = BindingFlags.Instance | BindingFlags.Static |
                                    BindingFlags.Public | BindingFlags.NonPublic;

                        var members = Enumerable.Empty<MemberInfo>()
                                                .Concat(handlerType.GetFields(flags))
                                                .Concat(handlerType.GetProperties(flags))
                                                .Concat(handlerType.GetMethods(flags));

                        using (var eMembers = members.GetEnumerator())
                        {
                            while (eMembers.MoveNext())
                            {
                                var member = eMembers.Current;

                                // automatically subscribe for receiving messages
                                // of a specific type
                                var receivceMsgAttribs = member.GetCustomAttributes(typeof(ReceiveMessageAttribute), true)
                                                               .Cast<ReceiveMessageAttribute>();
                                {
                                    using (var eAttribs = receivceMsgAttribs.GetEnumerator())
                                    {
                                        while (eAttribs.MoveNext())
                                        {
                                            var attrib = eAttribs.Current;
                                            var msgType = attrib.MessageType;

                                            if (member is MethodInfo)
                                            {
                                                SubscribeMethod(ctx, (MethodInfo)member, msgType);
                                            }
                                            else if (member is PropertyInfo)
                                            {
                                                SubscribeProperty(ctx, (PropertyInfo)member, msgType);
                                            }
                                            else if (member is FieldInfo)
                                            {
                                                SubscribeField(ctx, (FieldInfo)member, msgType);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        ctx.Handler
                           .UpdateContext(ctx);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a handler.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        /// <returns>The configuration object.</returns>
        public IMessageHandlerConfiguration RegisterHandler(IMessageHandler handler)
        {
            lock (SyncRoot)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException("handler");
                }

                var result = new MessageHandlerConfiguration()
                {
                    Distributor = this,
                    Handler = handler,
                };

                var ctx = new MessageHandlerContext
                {
                    Config = result,
                    ModuleBuilder = _MODULE_BUILDER,
                };

                _HANDLERS.Add(ctx);
                return result;
            }
        }

        private static void SubscribeField(IMessageHandlerContext ctx, FieldInfo field, Type msgType)
        {
            if (msgType == null)
            {
                var fieldType = field.FieldType;

                msgType = fieldType.GetGenericArguments()[0];
            }

            var genericSubscribeMethod = GetSubscribeMethod(ctx);

            var sm = genericSubscribeMethod.MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    field.SetValue(obj: ctx.Handler,
                                   value: m);
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { action });
        }

        private static void SubscribeMethod(IMessageHandlerContext ctx, MethodInfo method, Type msgType)
        {
            if (msgType == null)
            {
                var param = method.GetParameters()[0];
                var paramType = param.ParameterType;

                msgType = paramType.GetGenericArguments()[0];
            }

            var genericSubscribeMethod = GetSubscribeMethod(ctx);

            var sm = genericSubscribeMethod.MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    method.Invoke(obj: ctx.Handler,
                                  parameters: new object[] { m });
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { action });
        }

        private static void SubscribeProperty(IMessageHandlerContext ctx, PropertyInfo property, Type msgType)
        {
            if (msgType == null)
            {
                var propertyType = property.PropertyType;

                msgType = propertyType.GetGenericArguments()[0];
            }

            var genericSubscribeMethod = GetSubscribeMethod(ctx);

            var sm = genericSubscribeMethod.MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    property.SetValue(ctx.Handler, m,
                                      index: null);
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { action });
        }

        #endregion Methods (6)
    }
}