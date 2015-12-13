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
    partial class MessageDistributor
    {
        internal class MessageContext<TMsg> : MarshalByRefObject, IMessageContext<TMsg>, ICloneable
        {
            #region Fields (1)

            internal MessageHandlerConfiguration Config;

            #endregion Fields (1)

            #region Properties (7)

            public DateTimeOffset CreationTime { get; set; }

            internal MessageDistributor Distributor => Config.Distributor;

            public Guid Id { get; set; }

            public TMsg Message { get; set; }

            public DateTimeOffset? SendTime { get; set; }

            DateTimeOffset IMessageContext<TMsg>.SendTime => SendTime.Value;

            public object Tag { get; set; }

            #endregion Properties (7)

            #region Methods (3)

            public object Clone()
            {
                return MemberwiseClone();
            }

            public virtual bool Log(object msg,
                                    MessageLogCategory category = MessageLogCategory.Info, MessageLogPriority prio = MessageLogPriority.None,
                                    string tag = null)
            {
                try
                {
                    var now = Distributor.Now;

                    var log = new MessageLogEntry<TMsg>()
                    {
                        Category = category,
                        Handler = Config.Handler,
                        Id = Guid.NewGuid(),
                        LogMessage = now,
                        Message = this,
                        Priority = prio,
                        Tag = ParseLogTag(tag),
                        Time = now,
                    };

                    Distributor.RaiseMessageLogReceived(Config.Handler, log);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            protected internal static string ParseLogTag(string tag)
            {
                return string.IsNullOrWhiteSpace(tag) ? null : tag.ToUpper().Trim();
            }

            #endregion Methods (3)
        }
    }
}