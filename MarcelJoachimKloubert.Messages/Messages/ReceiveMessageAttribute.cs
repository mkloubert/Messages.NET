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
    /// Marks a member for receiving a message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event,
                    Inherited = true, AllowMultiple = true)]
    public class ReceiveMessageAttribute : Attribute
    {
        #region Constructors (2)

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInstanceAttribute" /> class.
        /// </summary>
        /// <param name="threadOption">The value for the <see cref="ReceiveMessageAttribute.ThreadOption" /> property.</param>
        public ReceiveMessageAttribute(MessageThreadOption threadOption = MessageThreadOption.Current)
            : this(msgType: null, threadOption: threadOption)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveMessageAttribute" /> class.
        /// </summary>
        /// <param name="msgType">The value for the <see cref="ReceiveMessageAttribute.MessageType" /> property.</param>
        /// <param name="threadOption">The value for the <see cref="ReceiveMessageAttribute.ThreadOption" /> property.</param>
        public ReceiveMessageAttribute(Type msgType, MessageThreadOption threadOption = MessageThreadOption.Current)
        {
            MessageType = msgType;
            ThreadOption = threadOption;
        }

        #endregion Constructors (2)

        #region Properties (2)

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// Gets or sets how a member should be executed.
        /// </summary>
        public MessageThreadOption ThreadOption { get; set; }

        #endregion Properties (2)
    }
}