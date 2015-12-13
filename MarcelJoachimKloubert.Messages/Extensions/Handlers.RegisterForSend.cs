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

using MarcelJoachimKloubert.Messages;
using System;
using System.Collections.Generic;

namespace MarcelJoachimKloubert.Extensions
{
    // RegisterForSend()
    static partial class MJKMessageExtensionMethods
    {
        #region Methods (2)

        /// <summary>
        /// Registers the underlying handler for sending messages of specific types.
        /// </summary>
        /// <param name="cfg">The handler configuration.</param>
        /// <param name="msgTypes">The list of message types.</param>
        /// <returns>The instance of <paramref name="cfg" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cfg" /> is <see langword="null" /> and/or at least
        /// one item in <paramref name="msgTypes" />.
        /// </exception>
        public static TCfg RegisterForSend<TCfg>(this TCfg cfg, params Type[] msgTypes)
            where TCfg : IMessageHandlerConfiguration
        {
            return RegisterForSend<TCfg>(cfg: cfg,
                                         msgTypeList: msgTypes ?? new Type[] { null });
        }

        /// <summary>
        /// Registers the underlying handler for sending messages of specific types.
        /// </summary>
        /// <param name="cfg">The handler configuration.</param>
        /// <param name="msgTypeList">The list of message types.</param>
        /// <returns>The instance of <paramref name="cfg" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cfg" /> is <see langword="null" /> and/or at least
        /// one item in <paramref name="msgTypeList" />.
        /// </exception>
        public static TCfg RegisterForSend<TCfg>(this TCfg cfg, IEnumerable<Type> msgTypeList)
            where TCfg : IMessageHandlerConfiguration
        {
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }

            if (msgTypeList != null)
            {
                using (var e = msgTypeList.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        cfg.RegisterForSend(msgType: e.Current);
                    }
                }
            }

            return cfg;
        }

        #endregion Methods (2)
    }
}