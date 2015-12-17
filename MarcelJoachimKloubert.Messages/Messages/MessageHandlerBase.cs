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

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// A basic message handler.
    /// </summary>
    public abstract class MessageHandlerBase : MarshalByRefObject,
                                               IMessageHandler
    {
        #region Constructors (2)

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerBase" /> class.
        /// </summary>
        /// <param name="syncRoot">The custom object for <see cref="MessageHandlerBase.SyncRoot" /> property.</param>
        protected MessageHandlerBase(object syncRoot = null)
        {
            SyncRoot = syncRoot ?? new object();
        }

        /// <summary>
        /// Frees resource of that object.
        /// </summary>
        ~MessageHandlerBase()
        {
            Dispose(false);
        }

        #endregion Constructors (2)

        #region Properties (4)

        /// <inheriteddoc />
        public virtual bool IsDisposed { get; protected set; }

        /// <summary>
        /// Gets the current context.
        /// </summary>
        public virtual IMessageHandlerContext Context { get; protected set; }

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public virtual object SyncRoot { get; }

        /// <summary>
        /// Gets or sets an object that should be linked with that instance.
        /// </summary>
        public virtual object Tag { get; set; }

        #endregion Properties (4)

        #region Methods (6)

        /// <inheriteddoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            var isDisposedValue = IsDisposed;
            if (disposing && isDisposedValue)
            {
                return;
            }

            if (disposing)
            {
                isDisposedValue = true;
            }

            OnDispose(disposing, ref isDisposedValue);

            IsDisposed = isDisposedValue;
        }

        /// <summary>
        /// Is invoked AFTER value of <see cref="MessageHandlerBase.Context" /> has been updated.
        /// </summary>
        /// <param name="oldCtx">The old context.</param>
        /// <param name="newCtx">The new context.</param>
        protected virtual void OnContextUpdated(IMessageHandlerContext oldCtx, IMessageHandlerContext newCtx)
        {
            // do nothing by default
        }

        /// <summary>
        /// The logic for the <see cref="MessageHandlerBase.Dispose()" /> method and the destructor.
        /// </summary>
        /// <param name="disposing">
        /// <see cref="MessageHandlerBase.Dispose()" /> method (<see langword="true" />)
        /// or the destructor (<see langword="false" />).
        /// </param>
        /// <param name="isDisposed">The new value for <see cref="MessageHandlerBase.IsDisposed" /> property.</param>
        protected virtual void OnDispose(bool disposing, ref bool isDisposed)
        {
            // do nothing by default
        }

        /// <summary>
        /// Throws an exception if <see cref="MessageHandlerBase.IsDisposed" /> is <see langword="true" />.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="MessageHandlerBase.IsDisposed" /> is <see langword="true" />.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(objectName: GetType().FullName);
            }
        }

        /// <inheriteddoc />
        public void UpdateContext(IMessageHandlerContext ctx)
        {
            var oldCtx = Context;
            Context = ctx;

            OnContextUpdated(oldCtx, ctx);
        }

        #endregion Methods (6)
    }
}