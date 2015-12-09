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
    /// Describes the configuration for an <see cref="IMessageHandler" /> object.
    /// </summary>
    public interface IMessageHandlerConfiguration
    {
        #region Properties (2)

        /// <summary>
        /// Gets the underlying handler.
        /// </summary>
        IMessageHandler Handler { get; }

        /// <summary>
        /// Gets or sets if the underlying distributor owns the handler or not.
        /// </summary>
        bool OwnsHandler { get; set; }

        #endregion Properties (2)

        #region Methods (4)

        /// <summary>
        /// Registers the underlying handler for receiving messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">The type of the message.</typeparam>
        /// <returns>That instance.</returns>
        IMessageHandlerConfiguration RegisterForReceive<TMsg>();

        /// <summary>
        /// Registers the underlying handler for receiving messages of a specific type.
        /// </summary>
        /// <param name="msgType">The type of the message.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="msgType" /> is <see langword="null" />.
        /// </exception>
        IMessageHandlerConfiguration RegisterForReceive(Type msgType);

        /// <summary>
        /// Registers the underlying handler for sending messages of a specific type.
        /// </summary>
        /// <typeparam name="TMsg">The type of the message.</typeparam>
        /// <returns>That instance.</returns>
        IMessageHandlerConfiguration RegisterForSend<TMsg>();

        /// <summary>
        /// Registers the underlying handler for sending messages of a specific type.
        /// </summary>
        /// <param name="msgType">The type of the message.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="msgType" /> is <see langword="null" />.
        /// </exception>
        IMessageHandlerConfiguration RegisterForSend(Type msgType);

        #endregion Methods (4)
    }
}