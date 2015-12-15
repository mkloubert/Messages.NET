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
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.Messages
{
    partial class MessageDistributor
    {
        internal class MessageHandlerContext : MarshalByRefObject, IMessageHandlerContext
        {
            #region Fields (4)

            internal MessageHandlerConfiguration Config;

            internal readonly ICollection<MessageType> MESSAGE_TYPES = new HashSet<MessageType>();

            internal ModuleBuilder ModuleBuilder;

            internal object SYNC_ROOT = new object();

            #endregion Fields (4)

            #region Properties (3)

            public MessageDistributor Distributor
            {
                get { return Config.Distributor; }
            }

            public IMessageHandler Handler { get; internal set; }

            #endregion Properties (3)

            #region Methods (11)

            public INewMessageContext<TMsg> CreateMessage<TMsg>()
            {
                var now = Distributor.Now;

                var result = new NewMessageContext<TMsg>()
                {
                    Config = Config,
                    CreationTime = now,
                    Id = Guid.NewGuid(),
                    Message = (TMsg)Activator.CreateInstance(GetMessageType<TMsg>()),
                };

                return result;
            }

            protected internal Type CreateProxyForInterface(Type interfaceType)
            {
                var msgInterfaceAttribs = interfaceType.GetCustomAttributes(typeof(MessageInstanceAttribute), false)
                                                       .Cast<MessageInstanceAttribute>()
                                                       .ToArray();

                if (msgInterfaceAttribs.Length > 0)
                {
                    return msgInterfaceAttribs.Last()
                                              .InstanceType;
                }

                var baseType = typeof(MessageBase);

                var typeBuilder = ModuleBuilder.DefineType(string.Format("MJKImplOf_{0}_{1:N}_{2}_Proxy",
                                                                         interfaceType.Name,
                                                                         Guid.NewGuid(),
                                                                         ModuleBuilder.GetHashCode()),
                                                           TypeAttributes.Public | TypeAttributes.Class,
                                                           baseType);

                typeBuilder.AddInterfaceImplementation(interfaceType);

                // collect properties
                var properties = new HashSet<PropertyInfo>();
                using (var e = interfaceType.GetProperties().Cast<PropertyInfo>().GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        properties.Add(e.Current);
                    }
                }

                var fieldNamesInUse = new HashSet<string>();
                using (var e = properties.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var property = e.Current;

                        var propertyName = property.Name;
                        var propertyType = property.PropertyType;

                        var propertyBuilder = typeBuilder.DefineProperty(propertyName,
                                                                         PropertyAttributes.None,
                                                                         propertyType,
                                                                         Type.EmptyTypes);

                        var fieldBaseName = char.ToLower(propertyName[0]) +
                                                         new string(propertyName.Skip(1).ToArray());

                        var fieldName = fieldBaseName;
                        ulong? fieldNameIndex = null;
                        while (fieldNamesInUse.Contains(fieldName))
                        {
                            if (fieldNameIndex.HasValue)
                            {
                                ++fieldNameIndex;
                            }
                            else
                            {
                                fieldNameIndex = 0;
                            }

                            fieldName = fieldBaseName + fieldNameIndex;
                        }

                        fieldNamesInUse.Add(fieldName);

                        var fieldBuilder = typeBuilder.DefineField("_" + fieldName,
                                                                   propertyType,
                                                                   FieldAttributes.Family);

                        // getter
                        {
                            var methodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
                                                                         MethodAttributes.Public | MethodAttributes.Virtual,
                                                                         propertyType,
                                                                         Type.EmptyTypes);

                            var ilGen = methodBuilder.GetILGenerator();

                            ilGen.Emit(OpCodes.Ldarg_0);      // load "this"
                            ilGen.Emit(OpCodes.Ldfld, fieldBuilder); // load the property's underlying field onto the stack
                            ilGen.Emit(OpCodes.Ret);          // return the value on the stack

                            propertyBuilder.SetGetMethod(methodBuilder);
                        }

                        // setter
                        {
                            var methodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
                                                                         MethodAttributes.Public | MethodAttributes.Virtual,
                                                                         typeof(void),
                                                                         new[] { propertyType });

                            var ilGen = methodBuilder.GetILGenerator();

                            ilGen.Emit(OpCodes.Ldarg_0);      // load "this"
                            ilGen.Emit(OpCodes.Ldarg_1);      // load "value" onto the stack
                            ilGen.Emit(OpCodes.Stfld, fieldBuilder); // set the field equal to the "value" on the stack
                            ilGen.Emit(OpCodes.Ret);          // return nothing

                            propertyBuilder.SetSetMethod(methodBuilder);
                        }

                        if (propertyType.IsSerializable)
                        {
                            // DataMemberAttribute
                            {
                                var dataMemberAttrib = typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes);
                                var attribBuilder = new CustomAttributeBuilder(dataMemberAttrib, new object[0]);

                                propertyBuilder.SetCustomAttribute(attribBuilder);
                            }
                        }
                        else
                        {
                            // NonSerializedAttribute
                            {
                                var nonSerializedAttrib = typeof(NonSerializedAttribute).GetConstructor(Type.EmptyTypes);
                                var attribBuilder = new CustomAttributeBuilder(nonSerializedAttrib, new object[0]);

                                fieldBuilder.SetCustomAttribute(attribBuilder);
                            }
                        }
                    }
                }

                // .ctor()
                {
                    var constructorParams = Type.EmptyTypes;

                    var baseConstructor = baseType.GetConstructor(constructorParams);

                    var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                                                                           CallingConventions.Standard,
                                                                           constructorParams);

                    var ilGen = constructorBuilder.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);                  // load "this"
                    ilGen.Emit(OpCodes.Call, baseConstructor);    // call the base constructor

                    ilGen.Emit(OpCodes.Ret);    // return nothing
                }

                // .ctor(SerializationInfo, StreamingContext)
                {
                    var constructorParams = new[] { typeof(SerializationInfo), typeof(StreamingContext) };

                    var baseConstructor = baseType.GetConstructor(constructorParams);

                    var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                                                                           CallingConventions.Standard,
                                                                           constructorParams);

                    var ilGen = constructorBuilder.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);                  // load "this"
                    ilGen.Emit(OpCodes.Ldarg_1);                  // load "SerializationInfo"
                    ilGen.Emit(OpCodes.Ldarg_2);                  // load "StreamingContext"
                    ilGen.Emit(OpCodes.Call, baseConstructor);    // call the base constructor

                    ilGen.Emit(OpCodes.Ret);    // return nothing
                }

                // SerializableAttribute
                {
                    var serializableAttrib = typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes);
                    var attribBuilder = new CustomAttributeBuilder(serializableAttrib, new object[0]);

                    typeBuilder.SetCustomAttribute(attribBuilder);
                }

                // DataContractAttribute
                {
                    var dataContractAttrib = typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes);
                    var attribBuilder = new CustomAttributeBuilder(dataContractAttrib, new object[0]);

                    typeBuilder.SetCustomAttribute(attribBuilder);
                }

                return typeBuilder.CreateType();
            }

            protected internal Type GetMessageType<TMsg>()
            {
                lock (Config.Distributor.SyncRoot)
                {
                    var msgType = typeof(TMsg);

                    Type instanceType;
                    if (Config.Distributor._OBJECT_TYPES.TryGetValue(msgType, out instanceType))
                    {
                        return instanceType;
                    }

                    instanceType = msgType.IsInterface ? CreateProxyForInterface(msgType)
                                                       : msgType;

                    if (instanceType.IsAbstract)
                    {
                        throw new InvalidOperationException("Cannot use an abstract class as message type!");
                    }

                    Config.Distributor._OBJECT_TYPES.Add(msgType, instanceType);
                    return Config.Distributor._OBJECT_TYPES[msgType];
                }
            }

            public IDictionary<Type, IEnumerable<Delegate>> GetSubscriptions()
            {
                Dictionary<Type, IEnumerable<Delegate>> result;
                lock (SYNC_ROOT)
                {
                    result = MESSAGE_TYPES.ToDictionary(keySelector: x => x.KEY,
                                                        elementSelector: x => (IEnumerable<Delegate>)x.SUBSCRIPTIONS
                                                                                                      .Select(y => y.KEY)
                                                                                                      .ToList());
                }

                return result;
            }

            private void RaiseReceiveMessageError<TMsg>(IMessageContext<TMsg> msgCtx, Exception ex)
            {
                if (!Distributor.RaiseReceivingMessageFailed(Handler, (IMessageContext<object>)msgCtx, ex))
                {
                    throw ex;
                }
            }

            internal bool? Receive<TMsg>(MessageContext<TMsg> msg)
            {
                if (msg == null)
                {
                    return null;
                }

                MessageType msgType;
                lock (SYNC_ROOT)
                {
                    msgType = TryFindMessageType(typeof(TMsg));
                }

                if (msgType == null)
                {
                    return false;
                }

                lock (Config.SyncRoot)
                {
                    if (Config.Handler.IsDisposed)
                    {
                        return false;
                    }

                    if (!Config.RECEIVE_TYPES.Contains(typeof(TMsg)))
                    {
                        // not configured to eeceive the type of message
                        return false;
                    }
                }

                var occuredExceptions = new List<Exception>();

                using (var e = msgType.SUBSCRIPTIONS.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        try
                        {
                            e.Current
                             .Invoke(msg);
                        }
                        catch (Exception ex)
                        {
                            occuredExceptions.Add(ex);
                        }
                    }
                }

                if (occuredExceptions.Count > 0)
                {
                    var exceptionToThrow = new AggregateException(occuredExceptions);

                    RaiseReceiveMessageError(msg, exceptionToThrow);
                }

                return true;
            }

            public IMessageHandlerContext Subscribe<TMsg>(Action<IMessageContext<TMsg>> handler,
                                                          MessageThreadOption threadOption = MessageThreadOption.Current,
                                                          bool isSynchronized = false)
            {
                lock (SYNC_ROOT)
                {
                    if (handler == null)
                    {
                        throw new ArgumentNullException("handler");
                    }

                    var mt = typeof(TMsg);

                    var msgType = TryFindMessageType(mt);
                    if (msgType == null)
                    {
                        msgType = new MessageType(mt);
                        MESSAGE_TYPES.Add(msgType);
                    }

                    var newSubscription = new MessageTypeSubscription(
                        msgType: msgType,
                        key: handler,
                        action: WrapSubscribeHandler<TMsg>(handler: handler, msgType: msgType,
                                                           threadOption: threadOption,
                                                           isSynchronized: isSynchronized));

                    msgType.SUBSCRIPTIONS.Add(newSubscription);
                }

                return this;
            }

            private MessageType TryFindMessageType(Type msgType)
            {
                return MESSAGE_TYPES.FirstOrDefault(x => x.Equals(msgType));
            }

            public IMessageHandlerContext Unsubscribe<TMsg>(Action<IMessageContext<TMsg>> handler)
            {
                lock (SYNC_ROOT)
                {
                    if (handler != null)
                    {
                        var msgType = TryFindMessageType(typeof(TMsg));
                        if (msgType != null)
                        {
                            using (var e = msgType.SUBSCRIPTIONS.Where(x => x.Equals(handler)).GetEnumerator())
                            {
                                while (e.MoveNext())
                                {
                                    msgType.SUBSCRIPTIONS
                                           .Remove(e.Current);
                                }
                            }
                        }
                    }
                }

                return this;
            }

            public IMessageHandlerContext UnsubscribeAll<TMsg>()
            {
                lock (SYNC_ROOT)
                {
                    var msgType = TryFindMessageType(typeof(TMsg));
                    if (msgType != null)
                    {
                        MESSAGE_TYPES.Remove(msgType);
                    }
                }

                return this;
            }

            private Action<IMessageContext<TMsg>> WrapSubscribeHandler<TMsg>(Action<IMessageContext<TMsg>> handler,
                                                                             MessageType msgType,
                                                                             MessageThreadOption threadOption,
                                                                             bool isSynchronized)
            {
                var actionToInvoke = handler;

                switch (threadOption)
                {
                    case MessageThreadOption.Background:
                        {
                            Action<object> taskAction = (s) =>
                                {
                                    var taskArgs = (object[])s;

                                    var h = (Action<IMessageContext<TMsg>>)taskArgs[0];
                                    var mc = (IMessageContext<TMsg>)taskArgs[1];

                                    h(mc);
                                };

                            if (isSynchronized)
                            {
                                var unsynchronizedAction = taskAction;

                                taskAction = (s) =>
                                    {
                                        lock (msgType.SYNC_ROOT)
                                        {
                                            unsynchronizedAction(s);
                                        }
                                    };
                            }

                            actionToInvoke = (msgCtx) =>
                                {
                                    var newTask = new Task(action: taskAction,
                                                           state: new object[] { handler, msgCtx });
                                    newTask.ContinueWith((task) =>
                                        {
                                            if (!task.IsFaulted)
                                            {
                                                return;
                                            }

                                            var ex = task.Exception;
                                            if (ex == null ||
                                                ex.InnerExceptions.Count < 1)
                                            {
                                                return;
                                            }

                                            RaiseReceiveMessageError(msgCtx, ex);
                                        });

                                    var scheduler = Distributor.TaskScheduler;
                                    if (scheduler == null)
                                    {
                                        newTask.Start();
                                    }
                                    else
                                    {
                                        newTask.Start(scheduler);
                                    }
                                };
                        }
                        break;

                    default:
                        if (isSynchronized)
                        {
                            var unsynchronizedAction = actionToInvoke;

                            actionToInvoke = (msgCtx) =>
                                {
                                    lock (msgType.SYNC_ROOT)
                                    {
                                        unsynchronizedAction(msgCtx);
                                    }
                                };
                        }
                        break;
                }

                return (msgCtx) =>
                    {
                        try
                        {
                            actionToInvoke(msgCtx);
                        }
                        catch (Exception ex)
                        {
                            RaiseReceiveMessageError(msgCtx, ex);
                        }
                    };
            }

            #endregion Methods (11)
        }
    }
}