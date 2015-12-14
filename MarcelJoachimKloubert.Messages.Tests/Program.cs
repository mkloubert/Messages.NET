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

using MarcelJoachimKloubert.Extensions;
using System;

namespace MarcelJoachimKloubert.Messages.Tests
{
    public class TestMessageClass : ITestMessage
    {
        public int A { get; set; }
    }

    [MessageInstance(typeof(TestMessageClass))]
    public interface ITestMessage
    {
        int A { get; set; }
    }

    internal class MyMessageHandler : MessageHandlerBase
    {
        private static int _instances = 0;

        [ReceiveMessage]
        private IMessageContext<ITestMessage> _lastTestMsg;

        public MyMessageHandler()
        {
            Id = ++_instances;
        }

        public int Id { get; private set; }

        [ReceiveMessage]
        public IMessageContext<ITestMessage> TestMessage
        {
            set
            {
                if (value != null)
                {
                }
            }
        }

        [ReceiveMessage(typeof(ITestMessage))]
        private void HandleTestMessage(IMessageContext<object> msg)
        {
            if (Id != null)
            {
            }
        }

        public void SendTestMessage()
        {
            var newMsg = Context.CreateMessage<ITestMessage>();

            newMsg.Send();
        }

        [ReceiveMessage]
        public event EventHandler<MessageReceivedEventArgs<ITestMessage>> TestMessageReceived;
    }

    internal static class Program
    {
        #region Methods (1)

        private static int Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                var handler1 = new MyMessageHandler();
                var handler2 = new MyMessageHandler();

                handler2.TestMessageReceived += (sender, e) =>
                    {
                        if (e != null)
                        {
                        }
                    };

                handler1.StartTimer((h, s) =>
                {
                }, (h) =>
                {
                    return new
                    {
                        Now = DateTimeOffset.Now,
                    };
                }, TimeSpan.FromSeconds(10));

                var distributor = new MessageDistributor();

                var cfg1 = distributor.RegisterHandler(handler1);
                cfg1.RegisterForSend<ITestMessage>();

                var cfg2 = distributor.RegisterHandler(handler2);
                cfg2.RegisterForReceive<ITestMessage>();

                handler1.SendTestMessage();
            }
            catch
            {
                exitCode = 1;
            }

            Console.WriteLine("===== ENTER =====");
            Console.ReadLine();

            return exitCode;
        }

        #endregion Methods (1)
    }
}