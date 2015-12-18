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

using MarcelJoachimKloubert.Extensions;
using System;

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// An <see cref="IMessageHandler" /> that simply uses delegates to work.
    /// </summary>
    public class DelegateMessageHandler : MessageHandlerBase
    {
        #region Methods (9)

        /// <summary>
        /// Removes all subscriptions.
        /// </summary>
        /// <returns>That instance.</returns>
        public DelegateMessageHandler ClearSubscriptions()
        {
            Context.ClearSubscriptions();

            return this;
        }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <typeparam name="TMsg">Type of the message.</typeparam>
        /// <returns>The created message (context).</returns>
        /// <exception cref="ObjectDisposedException">
        /// Handler has already been disposed.
        /// </exception>
        public INewMessageContext<TMsg> CreateMessage<TMsg>()
        {
            ThrowIfDisposed();

            return Context.CreateMessage<TMsg>();
        }

        /// <inheriteddoc />
        protected override void OnDispose(bool disposing, ref bool isDisposed)
        {
            ClearSubscriptions();
        }

        /// <summary>
        /// Subscribes an action for handling messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">Type of the messages.</typeparam>
        /// <param name="action">The action to subscribe.</param>
        /// <param name="threadOption">The way <paramref name="action" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Handler has already been disposed.
        /// </exception>
        public DelegateMessageHandler Subscribe<TMsg>(Action<IMessageContext<TMsg>> action,
                                                      MessageThreadOption threadOption = MessageThreadOption.Current,
                                                      bool isSynchronized = false)
        {
            ThrowIfDisposed();

            Context.Subscribe<TMsg>(handler: action,
                                    threadOption: threadOption, isSynchronized: isSynchronized);

            return this;
        }

        /// <summary>
        /// Subscribes an action for handling messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">Type of the messages.</typeparam>
        /// <param name="noContextAction">The action to subscribe.</param>
        /// <param name="threadOption">The way <paramref name="noContextAction" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>The action that is used for the subscription.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="noContextAction" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Handler has already been disposed.
        /// </exception>
        public Action<IMessageContext<TMsg>> Subscribe<TMsg>(Action<TMsg> noContextAction,
                                                             MessageThreadOption threadOption = MessageThreadOption.Current,
                                                             bool isSynchronized = false)
        {
            ThrowIfDisposed();

            return Context.Subscribe<TMsg>(noContextHandler: noContextAction,
                                           threadOption: threadOption, isSynchronized: isSynchronized);
        }

        /// <summary>
        /// Subscribes an action for handling messages of a specific type.
        /// </summary>
        /// <param name="msgType">The message type.</param>
        /// <param name="action">The action to subscribe.</param>
        /// <param name="threadOption">The way <paramref name="action" /> should be receive a message.</param>
        /// <param name="isSynchronized">Invoke action thread safe or not.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="msgType" /> and/or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Handler has already been disposed.
        /// </exception>
        public DelegateMessageHandler Subscribe(Type msgType, Action<IMessageContext<object>> action,
                                                MessageThreadOption threadOption = MessageThreadOption.Current,
                                                bool isSynchronized = false)
        {
            ThrowIfDisposed();

            Context.Subscribe(msgType: msgType, handler: action,
                              threadOption: threadOption, isSynchronized: isSynchronized);

            return this;
        }

        /// <summary>
        /// Unsubscribes an action from handling messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">Type of the messages.</typeparam>
        /// <param name="action">The action to unsubscribe.</param>
        /// <returns>That instance.</returns>
        public DelegateMessageHandler Unsubscribe<TMsg>(Action<IMessageContext<TMsg>> action)
        {
            Context.Unsubscribe<TMsg>(action);

            return this;
        }

        /// <summary>
        /// Unsubscribes all actions from handling messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">Type of the messages.</typeparam>
        /// <returns>That instance.</returns>
        public DelegateMessageHandler UnsubscribeAll<TMsg>()
        {
            Context.UnsubscribeAll<TMsg>();

            return this;
        }

        /// <summary>
        /// Unsubscribes all actions from handling messages of a specific type.
        /// </summary>
        /// <param name="msgType">Type of he messages.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="msgType" /> is <see langword="null" />.
        /// </exception>
        public DelegateMessageHandler UnsubscribeAll(Type msgType)
        {
            Context.UnsubscribeAll(msgType: msgType);

            return this;
        }

        #endregion Methods (9)
    }
}