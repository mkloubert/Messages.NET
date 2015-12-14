﻿/**********************************************************************************************************************
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
        internal class MessageHandlerContext : IMessageHandlerContext
        {
            #region Fields (3)

            internal readonly IDictionary<Type, ICollection<IList<Delegate>>> SUBSCRIPTIONS = new Dictionary<Type, ICollection<IList<Delegate>>>();

            internal MessageHandlerConfiguration Config;

            internal ModuleBuilder ModuleBuilder;

            #endregion Fields (3)

            #region Contructors (1)

            internal MessageHandlerContext()
            {
                SyncRoot = new object();
            }

            #endregion Contructors (1)

            #region Properties (3)

            public MessageDistributor Distributor => Config.Distributor;

            public IMessageHandler Handler { get; internal set; }

            internal object SyncRoot { get; }

            #endregion Properties (3)

            #region Methods (10)

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
                var msgInterfaceAttribs = interfaceType.GetTypeInfo()
                                                       .GetCustomAttributes<MessageInstanceAttribute>(inherit: false)
                                                       .ToArray();

                if (msgInterfaceAttribs.Length > 0)
                {
                    return msgInterfaceAttribs.Last()
                                              .InstanceType;
                }

                var baseType = typeof(MessageBase);

                var typeBuilder = ModuleBuilder.DefineType($"MJKImplOf_{interfaceType.Name}_{Guid.NewGuid():N}_{ModuleBuilder.GetHashCode()}_Proxy",
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
                        var propertyTypeInfo = propertyType.GetTypeInfo();

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

                        var fieldBuilder = typeBuilder.DefineField($"_{fieldName}",
                                                                   propertyType,
                                                                   FieldAttributes.Family);

                        // getter
                        {
                            var methodBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
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
                            var methodBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
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

                        if (propertyTypeInfo.IsSerializable)
                        {
                            // DataMemberAttribute
                            {
                                var dataMemberAttrib = typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes);
                                var attribBuilder = new CustomAttributeBuilder(dataMemberAttrib, new object[0]);

                                propertyBuilder.SetCustomAttribute(attribBuilder);
                            }
                        }
                        /*
                        else
                        {
                            // NonSerializedAttribute
                            {
                                var nonSerializedAttrib = typeof(NonSerializedAttribute).GetConstructor(Type.EmptyTypes);
                                var attribBuilder = new CustomAttributeBuilder(nonSerializedAttrib, new object[0]);

                                fieldBuilder.SetCustomAttribute(attribBuilder);
                            }
                        }*/
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
                /*
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
                }*/

                // SerializableAttribute
                /* {
                    var serializableAttrib = typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes);
                    var attribBuilder = new CustomAttributeBuilder(serializableAttrib, new object[0]);

                    typeBuilder.SetCustomAttribute(attribBuilder);
                } */

                // DataContractAttribute
                {
                    var dataContractAttrib = typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes);
                    var attribBuilder = new CustomAttributeBuilder(dataContractAttrib, new object[0]);

                    typeBuilder.SetCustomAttribute(attribBuilder);
                }

                return typeBuilder.CreateTypeInfo()
                                  .AsType();
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

                    var typeInfo = msgType.GetTypeInfo();

                    instanceType = typeInfo.IsInterface ? CreateProxyForInterface(msgType)
                                                        : msgType;

                    if (typeInfo.IsAbstract)
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
                lock (SyncRoot)
                {
                    result = SUBSCRIPTIONS.ToDictionary(keySelector: (x) => x.Key,
                                                        elementSelector: (x) => (IEnumerable<Delegate>)x.Value.ToList());
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

                ICollection<IList<Delegate>> subscriberActions;
                lock (SyncRoot)
                {
                    var msgType = typeof(TMsg);

                    SUBSCRIPTIONS.TryGetValue(msgType, out subscriberActions);
                }

                if (subscriberActions == null)
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

                using (var e = subscriberActions.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        try
                        {
                            var h = e.Current[1];

                            h.GetMethodInfo()
                             .Invoke(obj: h.Target,
                                     parameters: new object[] { msg });
                        }
                        catch (Exception ex)
                        {
                            occuredExceptions.Add(ex.GetBaseException());
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
                lock (SyncRoot)
                {
                    if (handler == null)
                    {
                        throw new ArgumentNullException(nameof(handler));
                    }

                    var msgType = typeof(TMsg);

                    ICollection<IList<Delegate>> handlers;
                    if (!SUBSCRIPTIONS.TryGetValue(msgType, out handlers))
                    {
                        handlers = new List<IList<Delegate>>();
                        SUBSCRIPTIONS.Add(msgType, handlers);
                    }

                    handlers.Add(new List<Delegate>
                        {
                            // [0] the handler to use for subscription
                            handler,

                            // [1] the "real" / wrapped handler that is invoked
                            WrapSubscribeHandler<TMsg>(handler: handler,
                                                                threadOption: threadOption,
                                                                isSynchronized: isSynchronized),
                        });
                }

                return this;
            }

            public IMessageHandlerContext Unsubscribe<TMsg>(Action<IMessageContext<TMsg>> handler)
            {
                lock (SyncRoot)
                {
                    if (handler != null)
                    {
                        var msgType = typeof(TMsg);

                        ICollection<IList<Delegate>> handlerEntries;
                        if (SUBSCRIPTIONS.TryGetValue(msgType, out handlerEntries))
                        {
                            IList<Delegate> entry;
                            while ((entry = handlerEntries.FirstOrDefault(x => x[0].Equals(handler))) != null)
                            {
                                handlerEntries.Remove(entry);
                            }
                        }
                    }
                }

                return this;
            }

            public IMessageHandlerContext UnsubscribeAll<TMsg>()
            {
                lock (SyncRoot)
                {
                    SUBSCRIPTIONS.Remove(key: typeof(TMsg));
                }

                return this;
            }

            private Action<IMessageContext<TMsg>> WrapSubscribeHandler<TMsg>(Action<IMessageContext<TMsg>> handler,
                                                                             MessageThreadOption threadOption,
                                                                             bool isSynchronized)
            {
                var actionToInvoke = handler;

                if (threadOption == MessageThreadOption.Background)
                {
                    Action<object> taskAction = (s) =>
                        {
                            var taskArgs = (object[])s;

                            var h = (Action<IMessageContext<TMsg>>)taskArgs[0];
                            var mc = (IMessageContext<TMsg>)taskArgs[1];

                            h(mc);
                        };

                    var realTaskAction = taskAction;
                    if (isSynchronized)
                    {
                        var syncRoot = new object();

                        realTaskAction = (s) =>
                            {
                                lock (syncRoot)
                                {
                                    taskAction(s);
                                }
                            };
                    }

                    actionToInvoke = (msgCtx) =>
                        {
                            var newTask = new Task(action: realTaskAction,
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

                            newTask.Start();
                        };
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

            #endregion Methods (10)
        }
    }
}