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
using System.Linq;

namespace MarcelJoachimKloubert.Extensions
{
    // ClearSubscriptions
    static partial class MJKMessageExtensionMethods
    {
        #region Methods (1)

        /// <summary>
        /// Removes all subscriptions.
        /// </summary>
        /// <typeparam name="TCtx">Type of the handler context.</typeparam>
        /// <param name="ctx">The handler context.</param>
        /// <returns>The instance from <paramref name="ctx" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ctx" /> is <see langword="null" />.
        /// </exception>
        public static TCtx ClearSubscriptions<TCtx>(this TCtx ctx)
            where TCtx : IMessageHandlerContext
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            using (var e = ctx.GetSubscriptions().Select(x => x.Key).GetEnumerator())
            {
                while (e.MoveNext())
                {
                    try
                    {
                        GetUnsubscribeAllMethod(ctx).MakeGenericMethod(e.Current)
                                                    .Invoke(obj: ctx,
                                                            parameters: null);
                    }
                    catch (Exception ex)
                    {
                        throw ex.GetBaseException();
                    }
                }
            }

            return ctx;
        }

        #endregion Methods (1)
    }
}