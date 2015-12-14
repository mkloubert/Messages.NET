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
        internal class MessageTypeSubscription : IEquatable<Delegate>, IEquatable<MessageTypeSubscription>
        {
            #region Fields (4)

            internal readonly Delegate ACTION;
            internal readonly Delegate KEY;
            internal readonly MessageType MESSAGE_TYPE;
            internal readonly object SYNC_ROOT;

            #endregion Fields (4)

            #region Constructors (1)

            internal MessageTypeSubscription(MessageType msgType, Delegate key, Delegate action)
            {
                SYNC_ROOT = new object();
                MESSAGE_TYPE = msgType;
                KEY = key;
                ACTION = action;
            }

            #endregion Constructors (1)

            #region Methods (6)

            public bool Equals(MessageTypeSubscription other)
            {
                return other != null &&
                       other.MESSAGE_TYPE.Equals(MESSAGE_TYPE) &&
                       other.Equals(KEY);
            }

            public bool Equals(Delegate other)
            {
                return other != null &&
                       other.Equals(KEY);
            }

            public override bool Equals(object obj)
            {
                var @delegate = obj as Delegate;
                if (@delegate != null)
                {
                    return Equals(other: @delegate);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return KEY.GetHashCode();
            }

            internal object Invoke(params object[] args)
            {
                try
                {
                    return ACTION.Method
                                 .Invoke(obj: ACTION.Target,
                                         parameters: args ?? new object[] { null });
                }
                catch (Exception ex)
                {
                    throw ex.GetBaseException();
                }
            }

            public override string ToString()
            {
                return KEY.ToString();
            }

            #endregion Methods (6)
        }
    }
}