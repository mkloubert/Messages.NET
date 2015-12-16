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

using MarcelJoachimKloubert.Messages;
using System;
using System.Linq;
using System.Reflection;

namespace MarcelJoachimKloubert.Extensions
{
    // Subscribe()
    static partial class MJKMessageExtensionMethods
    {
        #region Methods (3)

        /// <summary>
        /// Subscribes for receiving a non-wrapped message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        /// <param name="ctx">The handler context.</param>
        /// <param name="noContextHandler">The action that handles a received message.</param>
        /// <param name="threadOption">The way <paramref name="noContextHandler" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>The action that is used in <paramref name="ctx" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ctx" /> and/or <paramref name="noContextHandler" /> is <see langword="null" />.
        /// </exception>
        public static Action<IMessageContext<TMsg>> Subscribe<TMsg>(this IMessageHandlerContext ctx, Action<TMsg> noContextHandler,
                                                                    MessageThreadOption threadOption = MessageThreadOption.Current,
                                                                    bool isSynchronized = false)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            if (noContextHandler == null)
            {
                throw new ArgumentNullException(nameof(noContextHandler));
            }

            Action<IMessageContext<TMsg>> result = (msgCtx) => noContextHandler(msgCtx.Message);

            ctx.Subscribe<TMsg>(handler: result,
                                threadOption: threadOption,
                                isSynchronized: isSynchronized);
            return result;
        }

        /// <summary>
        /// Subscribes for receiving a non-wrapped message.
        /// </summary>
        /// <param name="ctx">The handler context.</param>
        /// <param name="msgType">The message type.</param>
        /// <param name="noContextHandler">The action that handles a received message.</param>
        /// <param name="threadOption">The way <paramref name="noContextHandler" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>The action that is used in <paramref name="ctx" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ctx" />, <paramref name="msgType" /> and/or <paramref name="noContextHandler" /> is <see langword="null" />.
        /// </exception>
        public static Action<IMessageContext<object>> Subscribe(this IMessageHandlerContext ctx, Type msgType,
                                                                Action<object> noContextHandler,
                                                                MessageThreadOption threadOption = MessageThreadOption.Current,
                                                                bool isSynchronized = false)
        {
            if (noContextHandler == null)
            {
                throw new ArgumentNullException(nameof(noContextHandler));
            }

            Action<IMessageContext<object>> result = (msgCtx) => noContextHandler(msgCtx.Message);
            Subscribe(ctx: ctx,
                      msgType: msgType,
                      handler: result,
                      threadOption: threadOption, isSynchronized: isSynchronized);

            return result;
        }

        /// <summary>
        /// <see cref="IMessageHandlerContext.Subscribe{TMsg}(Action{IMessageContext{TMsg}}, MessageThreadOption, bool)" />
        /// </summary>
        public static TCtx Subscribe<TCtx>(this TCtx ctx,
                                           Type msgType,
                                           Action<IMessageContext<object>> handler,
                                           MessageThreadOption threadOption = MessageThreadOption.Current,
                                           bool isSynchronized = false)
            where TCtx : IMessageHandlerContext
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            if (msgType == null)
            {
                throw new ArgumentNullException(nameof(msgType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var sm = typeof(TCtx)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
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

                           return methodActionType == @params[0].ParameterType &&
                                  typeof(MessageThreadOption) == @params[1].ParameterType &&
                                  typeof(bool) == @params[2].ParameterType;
                       });

            sm.MakeGenericMethod(msgType)
              .Invoke(obj: ctx,
                      parameters: new object[] { handler, threadOption, isSynchronized });

            return ctx;
        }

        #endregion Methods (3)
    }
}