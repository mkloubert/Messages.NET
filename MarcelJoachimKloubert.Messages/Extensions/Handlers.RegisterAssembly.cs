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
using System.Linq;
using System.Reflection;

namespace MarcelJoachimKloubert.Extensions
{
    // RegisterAssembly()
    static partial class MJKMessageExtensionMethods
    {
        #region Methods (2)

        /// <summary>
        /// Registers the types of the current assembly.
        /// </summary>
        /// <typeparam name="TCfg">Type of the handler configuration.</typeparam>
        /// <param name="cfg">The handler configuration.</param>
        /// <param name="directions">The directions.</param>
        /// <param name="allTypes">
        /// Register all types (<see langword="true" />) or the types that are marked with
        /// <see cref="MessageContractAttribute" /> only (<see langword="false" />).
        /// </param>
        /// <returns>The instance of <paramref name="cfg" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cfg" /> is <see langword="null" />.
        /// </exception>
        public static TCfg RegisterAssembly<TCfg>(this TCfg cfg,
                                                  MessageDirections directions = MessageDirections.Receive | MessageDirections.Send,
                                                  bool allTypes = false)
            where TCfg : IMessageHandlerConfiguration
        {
            return RegisterAssembly<TCfg>(cfg: cfg,
                                          asm: Assembly.GetCallingAssembly(),
                                          directions: directions, allTypes: allTypes);
        }

        /// <summary>
        /// Registers the types of an assembly.
        /// </summary>
        /// <typeparam name="TCfg">Type of the handler configuration.</typeparam>
        /// <param name="cfg">The handler configuration.</param>
        /// <param name="asm">The assembly.</param>
        /// <param name="directions">The directions.</param>
        /// <param name="allTypes">
        /// Register all types (<see langword="true" />) or the types that are marked with
        /// <see cref="MessageContractAttribute" /> only (<see langword="false" />).
        /// </param>
        /// <returns>The instance of <paramref name="cfg" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cfg" /> is <see langword="null" />.
        /// </exception>
        public static TCfg RegisterAssembly<TCfg>(this TCfg cfg, Assembly asm,
                                                  MessageDirections directions = MessageDirections.Receive | MessageDirections.Send,
                                                  bool allTypes = false)
            where TCfg : IMessageHandlerConfiguration
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            if (asm != null)
            {
                IEnumerable<Type> typesToRegister = asm.GetTypes();
                if (!allTypes)
                {
                    typesToRegister = typesToRegister.Where(x => x.GetCustomAttributes(typeof(MessageContractAttribute), false)
                                                                  .Any());
                }

                using (var e = typesToRegister.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var msgType = e.Current;

                        if (directions.HasFlag(MessageDirections.Receive))
                        {
                            cfg.RegisterForReceive(msgType: msgType);
                        }

                        if (directions.HasFlag(MessageDirections.Send))
                        {
                            cfg.RegisterForSend(msgType: msgType);
                        }
                    }
                }
            }

            return cfg;
        }

        #endregion Methods (2)
    }
}