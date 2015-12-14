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
using System.Runtime.Serialization;

namespace MarcelJoachimKloubert.Messages
{
    /// <summary>
    /// An exception that was throw by an <see cref="IMessageHandler" /> object.
    /// </summary>
    public class MessageHandlerException : Exception
    {
        #region Constructors (2)

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerException" /> class.
        /// </summary>
        /// <param name="handler">The value for the <see cref="MessageHandlerException.Handler" /> property.</param>
        /// <param name="innerException">The value for the <see cref="Exception.InnerException" /> property.</param>
        public MessageHandlerException(IMessageHandler handler, Exception innerException)
            : base(message: innerException != null ? innerException.Message : null,
                   innerException: innerException)
        {
            Handler = handler;
        }

        /// <inheriteddoc />
        protected MessageHandlerException(SerializationInfo info, StreamingContext context)
            : base(info: info,
                   context: context)
        {
        }

        #endregion Constructors (2)

        #region Properties (1)

        /// <summary>
        /// Gets the underlying handler.
        /// </summary>
        public IMessageHandler Handler { get; private set; }

        #endregion Properties (1)
    }
}