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

using MarcelJoachimKloubert.Messages.ChatExample.Contracts;
using System;
using System.Text;
using System.Windows.Forms;

namespace MarcelJoachimKloubert.Messages.ChatExample
{
    public partial class ChatForm : Form, IMessageHandler
    {
        #region Fields (1)

        private static int _currentId;

        #endregion Fields (1)

        #region Constructors (1)

        public ChatForm()
        {
            InitializeComponent();

            Id = ++_currentId;
            Text = ChatName;
        }

        #endregion Constructors (1)

        #region Events (1)

        private void Button_SendMessage_Click(object sender, EventArgs e)
        {
            var msg = TextBox_ChatMessage.Text;
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            var newChatMsg = MessageHandlerContext.CreateMessage<INewChatMessage>();
            newChatMsg.Message.From = ChatName;
            newChatMsg.Message.Message = msg;
            newChatMsg.Message.Time = DateTimeOffset.Now;

            newChatMsg.Send();
            AppendChatMessage(newChatMsg.Message);
        }

        #endregion Events (1)

        #region Properties (3)

        public string ChatName
        {
            get { return "Guy #" + Id; }
        }

        public IMessageHandlerContext MessageHandlerContext { get; private set; }

        public int Id { get; private set; }

        #endregion Properties (3)

        #region Methods (6)

        protected void AppendChatMessage(INewChatMessage msg)
        {
            var newLogLine = new StringBuilder();
            newLogLine.AppendFormat("[{0:yyyy-mm-dd HH:mm:ss zzz}] '{1}': {2}",
                                    msg.Time,
                                    msg.From,
                                    msg.Message).AppendLine();

            TextBox_ChatLog.AppendText(newLogLine.ToString());
        }

        [ReceiveMessage(MessageThreadOption.Background)]
        protected void ReceiveChatMessage(INewChatMessage msg)
        {
            InvokeThreadSafe((cf) => AppendChatMessage(msg));
        }

        protected void InvokeThreadSafe(Action<ChatForm> action)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Action<ChatForm>>(InvokeThreadSafe),
                       action);
                return;
            }

            action(this);
        }

        public void UpdateContext(IMessageHandlerContext ctx)
        {
            MessageHandlerContext = ctx;
        }

        #endregion Methods (6)
    }
}