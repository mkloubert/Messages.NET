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
    partial class MessageDistributor
    {
        internal class MessageHandlerConfiguration : IMessageHandlerConfiguration
        {
            #region Fields (3)

            public readonly ICollection<Type> RECEIVE_TYPES;
            public readonly ICollection<Type> SEND_TYPES;
            internal MessageDistributor Distributor;

            #endregion Fields (2)

            #region Constructors (1)

            internal MessageHandlerConfiguration()
            {
                SyncRoot = new object();

                RECEIVE_TYPES = new HashSet<Type>();
                SEND_TYPES = new HashSet<Type>();
            }

            #endregion Constructors (1)

            #region Properties (3)

            public IMessageHandler Handler { get; internal set; }

            internal object SyncRoot { get; private set; }

            #endregion Properties (3)

            #region Methods (4)

            public IMessageHandlerConfiguration RegisterForReceive<TMsg>()
            {
                return RegisterForReceive(typeof(TMsg));
            }

            public IMessageHandlerConfiguration RegisterForReceive(Type msgType)
            {
                lock (SyncRoot)
                {
                    if (msgType == null)
                    {
                        throw new ArgumentNullException("msgType");
                    }

                    RECEIVE_TYPES.Add(msgType);
                    return this;
                }
            }

            public IMessageHandlerConfiguration RegisterForSend<TMsg>()
            {
                return RegisterForSend(typeof(TMsg));
            }

            public IMessageHandlerConfiguration RegisterForSend(Type msgType)
            {
                lock (SyncRoot)
                {
                    if (msgType == null)
                    {
                        throw new ArgumentNullException("msgType");
                    }

                    SEND_TYPES.Add(msgType);
                    return this;
                }
            }

            #endregion Methods (4)
        }
    }
}