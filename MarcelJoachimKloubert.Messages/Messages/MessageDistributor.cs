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
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// A simple and configurable message distributor.
    /// </summary>
    public partial class MessageDistributor : IDisposable
    {
        #region Fields (3)

        private readonly ICollection<MessageHandlerContext> _HANDLERS;
        private readonly ModuleBuilder _MODULE_BUILDER;
        private readonly IDictionary<Type, Type> _OBJECT_TYPES;

        #endregion Fields (3)

        #region Constructors (2)

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

        /// <summary>
        /// Frees resource of that object.
        /// </summary>
        ~MessageDistributor()
        {
            Dispose(false);
        }

        #endregion Constructors (2)

        #region Events (2)

        /// <summary>
        /// Is invoked when a message log entry has been received.
        /// </summary>
        public event EventHandler<MessageLogEventArgs> MessageLogReceived;

        /// <summary>
        /// Is invoked when a log entry for a new message has been received.
        /// </summary>
        public event EventHandler<NewMessageLogEventArgs> NewMessageLogReceived;

        #endregion Events (2)

        #region Properties (5)

        /// <summary>
        /// Gets if the handler has been disposed or not.
        /// </summary>
        public virtual bool IsDisposed { get; protected set; }

        /// <summary>
        /// Returns the current time.
        /// </summary>
        public DateTimeOffset Now
        {
            get { return (TimeProvider ?? GetNow)(); }
        }

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public virtual object SyncRoot { get; protected set; }

        /// <summary>
        /// Gets or sets an object that should be linked with that instance.
        /// </summary>
        public virtual object Tag { get; set; }

        /// <summary>
        /// Gets or sets the function that provides the current time.
        /// </summary>
        public Func<DateTimeOffset> TimeProvider { get; set; }

        #endregion Properties (5)

        #region Methods (15)

        /// <inheriteddoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (SyncRoot)
            {
                if (disposing && IsDisposed)
                {
                    return;
                }

                var occuredException = new List<Exception>();

                if (disposing)
                {
                    using (var e = _HANDLERS.GetEnumerator())
                    {
                        while (e.MoveNext())
                        {
                            try
                            {
                                var ctx = e.Current;

                                if (ctx.Config.OwnsHandler)
                                {
                                    if (!ctx.Handler.IsDisposed)
                                    {
                                        ctx.Handler.Dispose();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                occuredException.Add(ex);
                            }
                        }
                    }
                }

                if (disposing)
                {
                    if (occuredException.Count > 0)
                    {
                        throw new AggregateException(occuredException);
                    }

                    IsDisposed = true;
                }
            }
        }

        /// <summary>
        /// The default logic for providing the current time.
        /// </summary>
        /// <returns>The current time.</returns>
        protected virtual DateTimeOffset GetNow()
        {
            return DateTimeOffset.Now;
        }

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

                              var messageCtxType = typeof(IMessageContext<>).MakeGenericType(genericParams[0]);
                              var methodActionType = typeof(Action<>).MakeGenericType(messageCtxType);

                              return methodActionType == @params[0].ParameterType;
                          });
        }

        /// <summary>
        /// Raises the <see cref="MessageDistributor.MessageLogReceived" />.
        /// </summary>
        /// <param name="handler">The message handler.</param>
        /// <param name="log">The log entry.</param>
        /// <returns>Event has been raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="log" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseMessageLogReceived(IMessageHandler handler, IMessageLogEntry log)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            var eventHandler = MessageLogReceived;
            if (eventHandler != null)
            {
                eventHandler(this, new MessageLogEventArgs(handler: handler,
                                                           log: log));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="MessageDistributor.NewMessageLogReceived" />.
        /// </summary>
        /// <param name="handler">The message handler.</param>
        /// <param name="log">The log entry.</param>
        /// <returns>Event has been raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="log" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseNewMessageLogReceived(IMessageHandler handler, INewMessageLogEntry log)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            var eventHandler = NewMessageLogReceived;
            if (eventHandler != null)
            {
                eventHandler(this, new NewMessageLogEventArgs(handler: handler,
                                                              log: log));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers a handler.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        /// <param name="ownsHandler">Own <paramref name="handler" /> or not.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ObjectDisposedException">Object has already been disposed.</exception>
        public IMessageHandlerConfiguration RegisterHandler(IMessageHandler handler, bool ownsHandler = false)
        {
            lock (SyncRoot)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException("handler");
                }

                ThrowIfDisposed();

                var result = new MessageHandlerConfiguration()
                {
                    Distributor = this,
                    Handler = handler,
                    OwnsHandler = ownsHandler,
                };

                var ctx = new MessageHandlerContext
                {
                    Config = result,
                    Handler = handler,
                    ModuleBuilder = _MODULE_BUILDER,
                };

                var handlerType = handler.GetType();

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

                                    if (member is MethodInfo)
                                    {
                                        SubscribeMethod(ctx, (MethodInfo)member, attrib);
                                    }
                                    else if (member is PropertyInfo)
                                    {
                                        SubscribeProperty(ctx, (PropertyInfo)member, attrib);
                                    }
                                    else if (member is FieldInfo)
                                    {
                                        SubscribeField(ctx, (FieldInfo)member, attrib);
                                    }
                                }
                            }
                        }
                    }
                }

                handler.UpdateContext(ctx);

                _HANDLERS.Add(ctx);
                return result;
            }
        }

        /// <summary>
        /// Registers a list of handlers that should use the same configuration.
        /// </summary>
        /// <param name="handlers">The handlers to register.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ObjectDisposedException">Object has already been disposed.</exception>
        public IMessageHandlerConfiguration RegisterListOfHandlers(params IMessageHandler[] handlers)
        {
            return RegisterListOfHandlers(ownsHandlers: false,
                                    handlers: handlers);
        }

        /// <summary>
        /// Registers a list of handlers that should use the same configuration.
        /// </summary>
        /// <param name="ownsHandlers">Own handlers or not.</param>
        /// <param name="handlers">The handlers to register.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ObjectDisposedException">Object has already been disposed.</exception>
        public IMessageHandlerConfiguration RegisterListOfHandlers(bool ownsHandlers, params IMessageHandler[] handlers)
        {
            return RegisterListOfHandlers(handlerList: handlers ?? new IMessageHandler[] { null },
                                    ownsHandlers: ownsHandlers);
        }

        /// <summary>
        /// Registers a list of handlers that should use the same configuration.
        /// </summary>
        /// <param name="handlerList">The handlers to register.</param>
        /// <param name="ownsHandlers">Own handlers or not.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ObjectDisposedException">Object has already been disposed.</exception>
        public IMessageHandlerConfiguration RegisterListOfHandlers(IEnumerable<IMessageHandler> handlerList, bool ownsHandlers = false)
        {
            ThrowIfDisposed();

            var result = new AggregateMessageHandlerConfiguration();

            var cfgList = new List<IMessageHandlerConfiguration>();

            if (handlerList != null)
            {
                using (var e = handlerList.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var handler = e.Current;
                        if (handler == null)
                        {
                            continue;
                        }

                        var cfg = RegisterHandler(handler: handler);
                        cfgList.Add(cfg);
                    }
                }
            }

            result.Configurations = cfgList;
            result.OwnsHandler = ownsHandlers;

            return result;
        }

        private static void SubscribeField(IMessageHandlerContext ctx, FieldInfo field, ReceiveMessageAttribute attrib)
        {
            var msgType = attrib.MessageType;

            if (msgType == null)
            {
                var fieldType = field.FieldType;

                msgType = fieldType.GetGenericArguments()[0];
            }

            var sm = GetSubscribeMethod(ctx).MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    field.SetValue(obj: ctx.Handler,
                                   value: m);
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { WrapSubscribeAction(action, attrib.ThreadOption) });
        }

        private static void SubscribeMethod(IMessageHandlerContext ctx, MethodInfo method, ReceiveMessageAttribute attrib)
        {
            var msgType = attrib.MessageType;

            if (msgType == null)
            {
                var param = method.GetParameters()[0];
                var paramType = param.ParameterType;

                msgType = paramType.GetGenericArguments()[0];
            }

            var sm = GetSubscribeMethod(ctx).MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    method.Invoke(obj: ctx.Handler,
                                  parameters: new object[] { m });
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { WrapSubscribeAction(action, attrib.ThreadOption) });
        }

        private static void SubscribeProperty(IMessageHandlerContext ctx, PropertyInfo property, ReceiveMessageAttribute attrib)
        {
            var msgType = attrib.MessageType;

            if (msgType == null)
            {
                var propertyType = property.PropertyType;

                msgType = propertyType.GetGenericArguments()[0];
            }

            var sm = GetSubscribeMethod(ctx).MakeGenericMethod(msgType);

            var action = new Action<object>((m) =>
                {
                    property.SetValue(ctx.Handler, m,
                                      index: null);
                });

            sm.Invoke(obj: ctx,
                      parameters: new object[] { WrapSubscribeAction(action, attrib.ThreadOption) });
        }

        /// <summary>
        /// Throws an exception if <see cref="MessageDistributor.IsDisposed" /> is <see langword="true" />.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="MessageDistributor.IsDisposed" /> is <see langword="true" />.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(objectName: GetType().FullName);
            }
        }

        private static Action<object> WrapSubscribeAction(Action<object> action, MessageThreadOption threadOption)
        {
            if (threadOption == MessageThreadOption.Background)
            {
                return (m) =>
                {
                    Task.Factory
                        .StartNew(action, state: m);
                };
            }

            return action;
        }

        #endregion Methods (15)
    }
}