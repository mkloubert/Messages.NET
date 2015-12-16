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
            _MODULE_BUILDER = asmBuilder.DefineDynamicModule($"MJKDynamicObjectFactoryModule_{Guid.NewGuid():N}_{GetHashCode()}");
        }

        /// <summary>
        /// Frees resource of that object.
        /// </summary>
        ~MessageDistributor()
        {
            Dispose(false);
        }

        #endregion Constructors (2)

        #region Events (4)

        /// <summary>
        /// Is invoked when a message log entry has been received.
        /// </summary>
        public event EventHandler<MessageLogEventArgs> MessageLogReceived;

        /// <summary>
        /// Is invoked when a log entry for a new message has been received.
        /// </summary>
        public event EventHandler<NewMessageLogEventArgs> NewMessageLogReceived;

        /// <summary>
        /// Is invoked when receiving a message failes.
        /// </summary>
        public event EventHandler<SendingMessageFailedEventArgs> ReceivingMessageFailed;

        /// <summary>
        /// Is invoked when sending a message failes.
        /// </summary>
        public event EventHandler<SendingMessageFailedEventArgs> SendingMessageFailed;

        #endregion Events (4)

        #region Properties (6)

        /// <summary>
        /// Gets if the handler has been disposed or not.
        /// </summary>
        public virtual bool IsDisposed { get; protected set; }

        /// <summary>
        /// Returns the current time.
        /// </summary>
        public DateTimeOffset Now => (TimeProvider ?? GetNow)();

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public virtual object SyncRoot { get; protected set; }

        /// <summary>
        /// Gets or sets an object that should be linked with that instance.
        /// </summary>
        public virtual object Tag { get; set; }

        /// <summary>
        /// Gets or sets the custom task scheduler for background actions.
        /// </summary>
        public TaskScheduler TaskScheduler { get; set; }

        /// <summary>
        /// Gets or sets the function that provides the current time.
        /// </summary>
        public Func<DateTimeOffset> TimeProvider { get; set; }

        #endregion Properties (6)

        #region Methods (19)

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

        private static object GetRealMemberArgument(IMessageContext<object> msgCtx, Type memberType)
        {
            object result = msgCtx;
            if (memberType.IsInstanceOfType(msgCtx.Message))
            {
                result = msgCtx.Message;
            }

            return result;
        }

        private static Type GetRealMessageType(Type msgType, Type memberType)
        {
            if (msgType != null)
            {
                return msgType;
            }

            var genericParams = memberType.GetGenericArguments();

            return genericParams.Length > 0 ? genericParams.Single()
                                            : memberType;
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
                              if (@params.Length != 3)
                              {
                                  return false;
                              }

                              var messageCtxType = typeof(IMessageContext<>).MakeGenericType(genericParams[0]);
                              var methodActionType = typeof(Action<>).MakeGenericType(messageCtxType);

                              return methodActionType == @params[0].ParameterType;
                          });
        }

        private static void InvokeSubscribeMethod(MessageHandlerContext ctx, MethodInfo method,
                                                  Action<object> action,
                                                  ReceiveMessageAttribute attrib)
        {
            try
            {
                method.Invoke(obj: ctx,
                              parameters: new object[] { action, attrib.ThreadOption, attrib.IsSynchronized });
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
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
                throw new ArgumentNullException(nameof(handler));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var eventHandler = MessageLogReceived;
            if (eventHandler == null)
            {
                return false;
            }

            eventHandler(this, new MessageLogEventArgs(handler: handler,
                                                       log: log));
            return true;
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
                throw new ArgumentNullException(nameof(handler));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var eventHandler = NewMessageLogReceived;
            if (eventHandler == null)
            {
                return false;
            }

            eventHandler(this, new NewMessageLogEventArgs(handler: handler,
                                                          log: log));
            return true;
        }

        /// <summary>
        /// Raises the <see cref="MessageDistributor.ReceivingMessageFailed" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The failed message.</param>
        /// <param name="ex">The thrown exception(s).</param>
        /// <returns>Event was raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sender" />, <paramref name="msg" /> and/or
        /// <paramref name="ex" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseReceivingMessageFailed(IMessageHandler sender, IMessageContext<object> msg, Exception ex)
        {
            return RaiseSendingMessageFailedEventHandler(ReceivingMessageFailed,
                                                         sender, msg, ex);
        }

        /// <summary>
        /// Raises the <see cref="MessageDistributor.SendingMessageFailed" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The failed message.</param>
        /// <param name="ex">The thrown exception(s).</param>
        /// <returns>Event was raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sender" />, <paramref name="msg" /> and/or
        /// <paramref name="ex" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseSendingMessageFailed(IMessageHandler sender, IMessageContext<object> msg, Exception ex)
        {
            return RaiseSendingMessageFailedEventHandler(SendingMessageFailed,
                                                         sender, msg, ex);
        }

        private bool RaiseSendingMessageFailedEventHandler(EventHandler<SendingMessageFailedEventArgs> eventHandler,
                                                           IMessageHandler sender, IMessageContext<object> msg, Exception ex)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            if (eventHandler == null)
            {
                return false;
            }

            eventHandler(this, new SendingMessageFailedEventArgs(sender, msg, ex));
            return true;
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
                    throw new ArgumentNullException(nameof(handler));
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

                const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static |
                                           BindingFlags.Public | BindingFlags.NonPublic;

                var members = Enumerable.Empty<MemberInfo>()
                                        .Concat(handlerType.GetFields(FLAGS))
                                        .Concat(handlerType.GetProperties(FLAGS))
                                        .Concat(handlerType.GetMethods(FLAGS))
                                        .Concat(handlerType.GetEvents(FLAGS));

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
                                    else if (member is EventInfo)
                                    {
                                        SubscribeEvent(ctx, (EventInfo)member, attrib);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
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

        private static void SubscribeEvent(MessageHandlerContext ctx, EventInfo @event, ReceiveMessageAttribute attrib)
        {
            var handler = ctx.Handler;

            var eventName = @event.Name;

            var msgType = attrib.MessageType;
            if (msgType == null)
            {
                msgType = @event.EventHandlerType  // EventHandler<>
                                .GetGenericArguments()[0]  // MessageReceivedEventArgs<>
                                .GetGenericArguments()[0];  // TMsg
            }

            var action = new Action<object>((m) =>
                {
                    var eventDelegate = handler.GetType()
                                               .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                                               ?.GetValue(handler) as Delegate;
                    if (eventDelegate == null)
                    {
                        return;
                    }

                    var msgCtx = (IMessageContext<object>)m;

                    var eventArgsType = typeof(MessageReceivedEventArgs<>).MakeGenericType(msgType);
                    var eventArgs = Activator.CreateInstance(type: eventArgsType,
                                                             args: new object[] { msgCtx });

                    IEnumerable<Delegate> eventHandlers = (eventDelegate as MulticastDelegate)?.GetInvocationList() ??
                                                          new[] { eventDelegate };

                    using (var e = eventHandlers.GetEnumerator())
                    {
                        while (e.MoveNext())
                        {
                            var eh = e.Current;

                            try
                            {
                                eh.Method
                                  .Invoke(obj: eh.Target,
                                          parameters: new object[] { handler, eventArgs });
                            }
                            catch (Exception ex)
                            {
                                throw ex.GetBaseException();
                            }
                        }
                    }
                });

            InvokeSubscribeMethod(ctx,
                                  GetSubscribeMethod(ctx).MakeGenericMethod(msgType), action,
                                  attrib);
        }

        private static void SubscribeField(MessageHandlerContext ctx, FieldInfo field, ReceiveMessageAttribute attrib)
        {
            var fieldType = field.FieldType;

            var msgType = GetRealMessageType(attrib.MessageType, fieldType);

            var action = new Action<object>((m) =>
                {
                    var msgCtx = (IMessageContext<object>)m;

                    try
                    {
                        field.SetValue(obj: ctx.Handler,
                                       value: GetRealMemberArgument(msgCtx, fieldType));
                    }
                    catch (Exception ex)
                    {
                        throw ex.GetBaseException();
                    }
                });

            InvokeSubscribeMethod(ctx,
                                  GetSubscribeMethod(ctx).MakeGenericMethod(msgType), action,
                                  attrib);
        }

        private static void SubscribeMethod(MessageHandlerContext ctx, MethodInfo method, ReceiveMessageAttribute attrib)
        {
            var param = method.GetParameters()[0];
            var paramType = param.ParameterType;

            var msgType = GetRealMessageType(attrib.MessageType, paramType);

            var action = new Action<object>((m) =>
                {
                    var msgCtx = (IMessageContext<object>)m;

                    try
                    {
                        method.Invoke(obj: ctx.Handler,
                                      parameters: new object[] { GetRealMemberArgument(msgCtx, paramType) });
                    }
                    catch (Exception ex)
                    {
                        throw ex.GetBaseException();
                    }
                });

            InvokeSubscribeMethod(ctx,
                                  GetSubscribeMethod(ctx).MakeGenericMethod(msgType), action,
                                  attrib);
        }

        private static void SubscribeProperty(MessageHandlerContext ctx, PropertyInfo property, ReceiveMessageAttribute attrib)
        {
            var propertyType = property.PropertyType;

            var msgType = GetRealMessageType(attrib.MessageType, propertyType);

            var action = new Action<object>((m) =>
                {
                    var msgCtx = (IMessageContext<object>)m;

                    try
                    {
                        property.SetValue(obj: ctx.Handler, value: GetRealMemberArgument(msgCtx, propertyType),
                                          index: null);
                    }
                    catch (Exception ex)
                    {
                        throw ex.GetBaseException();
                    }
                });

            InvokeSubscribeMethod(ctx,
                                  GetSubscribeMethod(ctx).MakeGenericMethod(msgType), action,
                                  attrib);
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

        #endregion Methods (19)
    }
}