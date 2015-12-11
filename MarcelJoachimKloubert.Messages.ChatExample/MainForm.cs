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
using MarcelJoachimKloubert.Messages.ChatExample.Contracts;
using System;
using System.Windows.Forms;

namespace MarcelJoachimKloubert.Messages.ChatExample
{
    public partial class MainForm : Form
    {
        #region Constructors (1)

        public MainForm()
        {
            InitializeComponent();

            MessageDistributor = new MessageDistributor();
        }

        #endregion Constructors (1)

        #region Events (3)

        private void Button_CreateWindow_Click(object sender, EventArgs e)
        {
            CreateChatWindow();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (MessageDistributor)
            {
                MessageDistributor = null;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CreateChatWindow();
            CreateChatWindow();
        }

        #endregion Events (3)

        #region Properties (1)

        public MessageDistributor MessageDistributor { get; private set; }

        #endregion Properties (1)

        #region Methods (1)

        protected void CreateChatWindow()
        {
            var newChatForm = new ChatForm();
            newChatForm.Show(this);

            newChatForm.RegisterTo(MessageDistributor)
                       .RegisterFor<INewChatMessage>();
        }

        #endregion Methods (1)
    }
}