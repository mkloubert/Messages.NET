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
    partial class MessageDistributor
    {
        internal class MessageHandlerContext : IMessageHandlerContext
        {
            #region Fields (3)

            internal readonly IDictionary<Type, ICollection<Delegate>> SUBSCRIPTIONS = new Dictionary<Type, ICollection<Delegate>>();

            internal MessageHandlerConfiguration Config;

            internal ModuleBuilder ModuleBuilder;

            #endregion Fields (3)

            #region Contructors (1)

            internal MessageHandlerContext()
            {
                SyncRoot = new object();
            }

            #endregion Contructors (1)

            #region Properties (2)

            public IMessageHandler Handler
            {
                get { return Config.Handler; }
            }

            internal object SyncRoot { get; private set; }

            #endregion Properties (2)

            #region Methods (5)

            public INewMessageContext<TMsg> CreateMessage<TMsg>()
            {
                var result = new NewMessageContext<TMsg>()
                {
                    Config = Config,
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

                var baseType = typeof(object);

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

                        var field = typeBuilder.DefineField("_" + fieldName,
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
                            ilGen.Emit(OpCodes.Ldfld, field); // load the property's underlying field onto the stack
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
                            ilGen.Emit(OpCodes.Stfld, field); // set the field equal to the "value" on the stack
                            ilGen.Emit(OpCodes.Ret);          // return nothing

                            propertyBuilder.SetSetMethod(methodBuilder);
                        }
                    }
                }

                // constructor
                {
                    var baseConstructor = baseType.GetConstructor(new Type[0]);

                    var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                                                                           CallingConventions.Standard,
                                                                           Type.EmptyTypes);

                    var ilGen = constructorBuilder.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);                  // load "this"
                    ilGen.Emit(OpCodes.Call, baseConstructor);    // call the base constructor

                    ilGen.Emit(OpCodes.Ret);    // return nothing
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

            internal bool? Receive<TMsg>(MessageContext<TMsg> msg)
            {
                if (msg == null)
                {
                    return null;
                }

                lock (Config.SyncRoot)
                {
                    if (!Config.RECEIVE_TYPES.Contains(typeof(TMsg)))
                    {
                        // not configured to eeceive the type of message
                        return false;
                    }
                }

                ICollection<Delegate> handlers;
                lock (SyncRoot)
                {
                    var msgType = typeof(TMsg);

                    SUBSCRIPTIONS.TryGetValue(msgType, out handlers);
                }

                if (handlers == null)
                {
                    return false;
                }

                var occuredExceptions = new List<Exception>();

                using (var e = handlers.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        try
                        {
                            var h = e.Current;

                            h.Method
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
                    throw new AggregateException(occuredExceptions);
                }

                return true;
            }

            public void Subscribe<TMsg>(Action<IMessageContext<TMsg>> handler)
            {
                lock (SyncRoot)
                {
                    if (handler == null)
                    {
                        throw new ArgumentNullException("handler");
                    }

                    var msgType = typeof(TMsg);

                    ICollection<Delegate> handlers;
                    if (!SUBSCRIPTIONS.TryGetValue(msgType, out handlers))
                    {
                        handlers = new List<Delegate>();
                        SUBSCRIPTIONS.Add(msgType, handlers);
                    }

                    handlers.Add(handler);
                }
            }

            #endregion Methods (5)
        }
    }
}