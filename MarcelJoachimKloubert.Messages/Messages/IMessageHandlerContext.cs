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

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// Describes a context for a message handler.
    /// </summary>
    public interface IMessageHandlerContext
    {
        #region Properties (1)

        /// <summary>
        /// Gets the underlying handler.
        /// </summary>
        IMessageHandler Handler { get; }

        #endregion Properties (1)

        #region Methods (6)

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        /// <returns>The created message (context)</returns>
        INewMessageContext<TMsg> CreateMessage<TMsg>();

        /// <summary>
        /// Returns all message types with their subscriptions.
        /// </summary>
        /// <returns>The list of subscriptions.</returns>
        IDictionary<Type, IEnumerable<Delegate>> GetSubscriptions();

        /// <summary>
        /// Subscribes for receiving a message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        /// <param name="handler">The action that handles a received message.</param>
        /// <param name="threadOption">The way <paramref name="handler" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> is <see langword="null" />.
        /// </exception>
        IMessageHandlerContext Subscribe<TMsg>(Action<IMessageContext<TMsg>> handler,
                                               MessageThreadOption threadOption = MessageThreadOption.Current,
                                               bool isSynchronized = false);

        /// <summary>
        /// Unsubscribes for receiving a message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        /// <param name="handler">The action to unsubscribe.</param>
        /// <returns>That instance.</returns>
        IMessageHandlerContext Unsubscribe<TMsg>(Action<IMessageContext<TMsg>> handler);

        /// <summary>
        /// Unsubscribes all handlers for receiving a message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        IMessageHandlerContext UnsubscribeAll<TMsg>();

        #endregion Methods (6)
    }
}