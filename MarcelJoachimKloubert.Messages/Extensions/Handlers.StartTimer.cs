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
using System.Threading;

namespace MarcelJoachimKloubert.Extensions
{
    // StartTimer()
    static partial class MJKMessageExtensionMethods
    {
        #region Methods (6)

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="period">
        /// The period the action should be invoked (in milliseconds).
        /// If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTime">
        /// The time that should be waited until the first invocation of <paramref name="action" />
        /// (in milliseconds). If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTime" /> or
        /// <paramref name="period" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler>(this THandler handler,
                                                 Action<THandler> action,
                                                 int? period, int? dueTime = null)
            where THandler : IMessageHandler
        {
            return StartTimer<THandler>(handler: handler,
                                        action: action,
                                        periodTime: period.HasValue ? TimeSpan.FromMilliseconds(period.Value) : (TimeSpan?)null,
                                        dueTimeSpan: dueTime.HasValue ? TimeSpan.FromMilliseconds(dueTime.Value) : (TimeSpan?)null);
        }

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="periodTime">
        /// The period the action should be invoked. If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTimeSpan">
        /// The time that should be waited until the first invocation of <paramref name="action" />.
        /// If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTimeSpan" /> or
        /// <paramref name="periodTime" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler>(this THandler handler,
                                                 Action<THandler> action,
                                                 TimeSpan? periodTime, TimeSpan? dueTimeSpan = null)
            where THandler : IMessageHandler
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return StartTimer(handler: handler,
                              action: (h, a) => a(h), actionState: action,
                              periodTime: periodTime, dueTimeSpan: dueTimeSpan);
        }

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <typeparam name="TState">Type of the object of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The 2nd argument for <paramref name="action" />.</param>
        /// <param name="period">
        /// The period the action should be invoked (in milliseconds).
        /// If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTime">
        /// The time that should be waited until the first invocation of <paramref name="action" />
        /// (in milliseconds). If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTime" /> or
        /// <paramref name="period" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler, TState>(this THandler handler,
                                                         Action<THandler, TState> action, TState actionState,
                                                         int? period, int? dueTime = null)
            where THandler : IMessageHandler
        {
            return StartTimer<THandler, TState>(handler: handler,
                                                action: action, actionState: actionState,
                                                periodTime: period.HasValue ? TimeSpan.FromMilliseconds(period.Value) : (TimeSpan?)null,
                                                dueTimeSpan: dueTime.HasValue ? TimeSpan.FromMilliseconds(dueTime.Value) : (TimeSpan?)null);
        }

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <typeparam name="TState">Type of the object of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The 2nd argument for <paramref name="action" />.</param>
        /// <param name="periodTime">
        /// The period the action should be invoked. If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTimeSpan">
        /// The time that should be waited until the first invocation of <paramref name="action" />.
        /// If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" /> and/or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTimeSpan" /> or
        /// <paramref name="periodTime" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler, TState>(this THandler handler,
                                                         Action<THandler, TState> action, TState actionState,
                                                         TimeSpan? periodTime, TimeSpan? dueTimeSpan = null)
            where THandler : IMessageHandler
        {
            return StartTimer<THandler, TState>(handler: handler,
                                                action: action, actionStateProvider: (h) => actionState,
                                                periodTime: periodTime, dueTimeSpan: dueTimeSpan);
        }

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <typeparam name="TState">Type of the object of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateProvider">The function that provides the 2nd argument for <paramref name="action" />.</param>
        /// <param name="period">
        /// The period the action should be invoked (in milliseconds).
        /// If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTime">
        /// The time that should be waited until the first invocation of <paramref name="action" />
        /// (in milliseconds). If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" />, <paramref name="action" /> and/or <paramref name="actionStateProvider" />
        /// is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTime" /> or
        /// <paramref name="period" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler, TState>(this THandler handler,
                                                         Action<THandler, TState> action, Func<THandler, TState> actionStateProvider,
                                                         int? period, int? dueTime = null)
            where THandler : IMessageHandler
        {
            return StartTimer<THandler, TState>(handler: handler,
                                                action: action, actionStateProvider: actionStateProvider,
                                                periodTime: period.HasValue ? TimeSpan.FromMilliseconds(period.Value) : (TimeSpan?)null,
                                                dueTimeSpan: dueTime.HasValue ? TimeSpan.FromMilliseconds(dueTime.Value) : (TimeSpan?)null);
        }

        /// <summary>
        /// Creates a timer for an <see cref="IMessageHandler" />.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler.</typeparam>
        /// <typeparam name="TState">Type of the object of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateProvider">The function that provides the 2nd argument for <paramref name="action" />.</param>
        /// <param name="periodTime">
        /// The period the action should be invoked. If <see langword="null" /> no period time is set.
        /// </param>
        /// <param name="dueTimeSpan">
        /// The time that should be waited until the first invocation of <paramref name="action" />.
        /// If <see langword="null" /> the action is invoked immediately.
        /// </param>
        /// <returns>The created timer.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler" />, <paramref name="action" /> and/or <paramref name="actionStateProvider" />
        /// is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The number of milliseconds in the value of <paramref name="dueTimeSpan" /> or
        /// <paramref name="periodTime" /> is negative and not equal to <see cref="Timeout.Infinite" />,
        /// or is greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static Timer StartTimer<THandler, TState>(this THandler handler,
                                                         Action<THandler, TState> action, Func<THandler, TState> actionStateProvider,
                                                         TimeSpan? periodTime, TimeSpan? dueTimeSpan = null)
            where THandler : IMessageHandler
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (actionStateProvider == null)
            {
                throw new ArgumentNullException(nameof(actionStateProvider));
            }

            if (!periodTime.HasValue)
            {
                periodTime = TimeSpan.FromMilliseconds(Timeout.Infinite);
            }

            if (!dueTimeSpan.HasValue)
            {
                dueTimeSpan = TimeSpan.Zero;
            }

            return new Timer(callback: (state) =>
                             {
                                 var callbackArgs = (object[])state;

                                 var h = (THandler)callbackArgs[0];
                                 var a = (Action<THandler, TState>)callbackArgs[1];
                                 var asp = (Func<THandler, TState>)callbackArgs[2];

                                 a(h, asp(h));
                             },
                             state: new object[] { handler, action, actionStateProvider },
                             dueTime: dueTimeSpan.Value,
                             period: periodTime.Value);
        }

        #endregion Methods (6)
    }
}