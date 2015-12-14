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

namespace MarcelJoachimKloubert.Messages
{
    partial class MessageDistributor
    {
        internal class NewMessageContext<TMsg> : MessageContext<TMsg>, INewMessageContext<TMsg>
        {
            #region Methods (3)

            internal MessageContext<TMsg> CloneForRecipient()
            {
                var msg = Message;
                if (msg is ICloneable)
                {
                    msg = (TMsg)((ICloneable)msg).Clone();
                }

                return new MessageContext<TMsg>()
                {
                    Config = Config,
                    CreationTime = CreationTime,
                    Id = Id,
                    Message = msg,
                };
            }

            public override bool Log(object msg,
                                     MessageLogCategory category = MessageLogCategory.Info, MessageLogPriority prio = MessageLogPriority.None,
                                     string tag = null)
            {
                try
                {
                    var now = Distributor.Now;

                    var log = new NewMessageLogEntry<TMsg>()
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

                    Distributor.RaiseNewMessageLogReceived(Config.Handler, log);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public void Send()
            {
                lock (SYNC_ROOT)
                {
                    var now = Distributor.Now;

                    if (SendTime.HasValue)
                    {
                        throw new InvalidOperationException("Cannot resend message!");
                    }

                    lock (Config.SyncRoot)
                    {
                        if (!Config.SEND_TYPES.Contains(typeof(TMsg)))
                        {
                            // not configured to send that message type
                            return;
                        }
                    }

                    var otherHandlers = new List<MessageHandlerContext>();
                    lock (Config.Distributor.SyncRoot)
                    {
                        otherHandlers.AddRange(Config.Distributor
                                                     ._HANDLERS
                                                     .Where(x => !x.Handler.Equals(Config.Handler)));
                    }

                    var occuredExceptions = new List<Exception>();

                    try
                    {
                        using (var e = otherHandlers.GetEnumerator())
                        {
                            while (e.MoveNext())
                            {
                                var ctx = e.Current;

                                try
                                {
                                    var msg = CloneForRecipient();
                                    msg.SendTime = now;

                                    ctx.Receive(msg);
                                }
                                catch (Exception ex)
                                {
                                    occuredExceptions.Add(new MessageHandlerException(ctx.Handler, ex));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        occuredExceptions.Add(ex);
                    }
                    finally
                    {
                        SendTime = now;
                    }

                    if (occuredExceptions.Count < 1)
                    {
                        return;
                    }

                    var exceptionToThrow = new AggregateException(occuredExceptions);
                    if (!Distributor.RaiseSendingMessageFailed(Config.Handler, (IMessageContext<object>)this, exceptionToThrow))
                    {
                        throw exceptionToThrow;
                    }
                }
            }

            #endregion Methods (3)
        }
    }
}