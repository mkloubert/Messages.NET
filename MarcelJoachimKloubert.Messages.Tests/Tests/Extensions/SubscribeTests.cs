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
using NUnit.Framework;

namespace MarcelJoachimKloubert.Messages.Tests.Extensions
{
    public class SubscribeTests : TestFixtureBase
    {
        #region INTERFACE: INewContact

        public interface INewContact
        {
            string Firstname { get; set; }

            string Lastname { get; set; }
        }

        #endregion INTERFACE: INewContact

        #region CLASS: AddressBook

        private class AddressBook : MessageHandlerBase
        {
            public IMessageContext<INewContact> LastNewContact;

            public string Name;

            public bool CreateNewContact(string firstname, string lastname, out INewMessageContext<INewContact> newMsg)
            {
                ThrowIfDisposed();

                newMsg = Context.CreateMessage<INewContact>();
                newMsg.Tag = Name;

                var newContact = newMsg.Message;
                newContact.Firstname = firstname;
                newContact.Lastname = lastname;

                return newMsg.Send();
            }

            protected void GetNewContact(IMessageContext<object> ctx)
            {
                ThrowIfDisposed();

                LastNewContact = (IMessageContext<INewContact>)ctx;

                ctx.Tag = Name;
            }

            protected override void OnContextUpdated(IMessageHandlerContext oldCtx, IMessageHandlerContext newCtx)
            {
                newCtx.Subscribe(typeof(INewContact),
                                 GetNewContact);
            }

            public void Reset()
            {
                ThrowIfDisposed();

                LastNewContact = null;
            }
        }

        #endregion CLASS: AddressBook

        #region Tests (3)

        [Test]
        public void Test1()
        {
            var outlook = new AddressBook()
            {
                Name = "Outlook",
            };
            var thunderbird = new AddressBook()
            {
                Name = "Thunderbird",
            };

            var distributor = new MessageDistributor();
            using (distributor)
            {
                distributor.RegisterHandler(outlook, true)
                           .RegisterForSend<INewContact>()
                           .RegisterForReceive<INewContact>();

                distributor.RegisterHandler(thunderbird, true)
                           .RegisterForSend<INewContact>()
                           .RegisterForReceive<INewContact>();

                Assert.IsFalse(outlook.IsDisposed);
                Assert.IsFalse(thunderbird.IsDisposed);

                INewMessageContext<INewContact> newMsg;
                Assert.IsTrue(outlook.CreateNewContact("Marcel", "Kloubert", out newMsg));

                Assert.IsNotNull(newMsg);

                Assert.AreEqual(newMsg.Tag, outlook.Name);

                Assert.IsNull(outlook.LastNewContact);
                Assert.IsNotNull(thunderbird.LastNewContact);

                Assert.AreEqual(thunderbird.LastNewContact.CreationTime, newMsg.CreationTime);
                Assert.AreEqual(thunderbird.LastNewContact.Id, newMsg.Id);
                Assert.AreEqual(thunderbird.LastNewContact.MessageType, newMsg.MessageType);
                Assert.AreEqual(thunderbird.LastNewContact.SendTime, newMsg.SendTime);

                Assert.AreNotEqual(newMsg.Tag, thunderbird.LastNewContact.Tag);

                Assert.AreEqual(thunderbird.LastNewContact.Message.Firstname, newMsg.Message.Firstname);
                Assert.AreEqual(thunderbird.LastNewContact.Message.Lastname, newMsg.Message.Lastname);
            }

            Assert.IsTrue(distributor.IsDisposed);
            Assert.IsTrue(outlook.IsDisposed);
            Assert.IsTrue(thunderbird.IsDisposed);
        }

        [Test]
        public void Test2()
        {
            var outlook = new AddressBook()
            {
                Name = "Outlook",
            };
            var ab2 = new AddressBook()
            {
                Name = "Thunderbird",
            };

            var distributor = new MessageDistributor();
            using (distributor)
            {
                distributor.RegisterHandler(outlook, true)
                           .RegisterForSend<INewContact>()
                           .RegisterForReceive<INewContact>();

                distributor.RegisterHandler(ab2, true)
                           .RegisterForSend<INewContact>();

                outlook.Reset();
                ab2.Reset();

                // outlook => thunderbird
                {
                    Assert.IsFalse(outlook.IsDisposed);
                    Assert.IsFalse(ab2.IsDisposed);

                    INewMessageContext<INewContact> newMsg;
                    Assert.IsTrue(outlook.CreateNewContact("Marcel", "Kloubert", out newMsg));

                    Assert.IsNotNull(newMsg);

                    Assert.AreEqual(newMsg.Tag, outlook.Name);

                    Assert.IsNull(outlook.LastNewContact);
                    Assert.IsNull(ab2.LastNewContact);
                }

                outlook.Reset();
                ab2.Reset();

                // thunderbird => outlook
                {
                    Assert.IsFalse(outlook.IsDisposed);
                    Assert.IsFalse(ab2.IsDisposed);

                    INewMessageContext<INewContact> newMsg;
                    Assert.IsTrue(ab2.CreateNewContact("Marcel", "Kloubert", out newMsg));

                    Assert.IsNotNull(newMsg);

                    Assert.AreEqual(newMsg.Tag, ab2.Name);

                    Assert.IsNotNull(outlook.LastNewContact);
                    Assert.IsNull(ab2.LastNewContact);

                    Assert.AreEqual(outlook.LastNewContact.CreationTime, newMsg.CreationTime);
                    Assert.AreEqual(outlook.LastNewContact.Id, newMsg.Id);
                    Assert.AreEqual(outlook.LastNewContact.MessageType, newMsg.MessageType);
                    Assert.AreEqual(outlook.LastNewContact.SendTime, newMsg.SendTime);

                    Assert.AreNotEqual(newMsg.Tag, outlook.LastNewContact.Tag);

                    Assert.AreEqual(outlook.LastNewContact.Message.Firstname, newMsg.Message.Firstname);
                    Assert.AreEqual(outlook.LastNewContact.Message.Lastname, newMsg.Message.Lastname);
                }
            }

            Assert.IsTrue(distributor.IsDisposed);
            Assert.IsTrue(outlook.IsDisposed);
            Assert.IsTrue(ab2.IsDisposed);
        }

        [Test]
        public void Test3()
        {
            var outlook = new AddressBook()
            {
                Name = "Outlook",
            };
            var thunderbird = new AddressBook()
            {
                Name = "Thunderbird",
            };

            var distributor = new MessageDistributor();
            using (distributor)
            {
                distributor.RegisterHandler(outlook, true)
                           .RegisterForSend<INewContact>();

                distributor.RegisterHandler(thunderbird, true)
                           .RegisterForSend<INewContact>()
                           .RegisterForReceive<INewContact>();

                outlook.Reset();
                thunderbird.Reset();

                // outlook => thunderbird
                {
                    Assert.IsFalse(thunderbird.IsDisposed);
                    Assert.IsFalse(outlook.IsDisposed);

                    INewMessageContext<INewContact> newMsg;
                    Assert.IsTrue(outlook.CreateNewContact("Marcel", "Kloubert", out newMsg));

                    Assert.IsNotNull(newMsg);

                    Assert.AreEqual(newMsg.Tag, outlook.Name);

                    Assert.IsNotNull(thunderbird.LastNewContact);
                    Assert.IsNull(outlook.LastNewContact);

                    Assert.AreEqual(thunderbird.LastNewContact.CreationTime, newMsg.CreationTime);
                    Assert.AreEqual(thunderbird.LastNewContact.Id, newMsg.Id);
                    Assert.AreEqual(thunderbird.LastNewContact.MessageType, newMsg.MessageType);
                    Assert.AreEqual(thunderbird.LastNewContact.SendTime, newMsg.SendTime);

                    Assert.AreNotEqual(newMsg.Tag, thunderbird.LastNewContact.Tag);

                    Assert.AreEqual(thunderbird.LastNewContact.Message.Firstname, newMsg.Message.Firstname);
                    Assert.AreEqual(thunderbird.LastNewContact.Message.Lastname, newMsg.Message.Lastname);
                }

                outlook.Reset();
                thunderbird.Reset();

                // thunderbird => outlook
                {
                    Assert.IsFalse(thunderbird.IsDisposed);
                    Assert.IsFalse(outlook.IsDisposed);

                    INewMessageContext<INewContact> newMsg;
                    Assert.IsTrue(thunderbird.CreateNewContact("Marcel", "Kloubert", out newMsg));

                    Assert.IsNotNull(newMsg);

                    Assert.AreEqual(newMsg.Tag, thunderbird.Name);

                    Assert.IsNull(thunderbird.LastNewContact);
                    Assert.IsNull(outlook.LastNewContact);
                }
            }

            Assert.IsTrue(distributor.IsDisposed);
            Assert.IsTrue(outlook.IsDisposed);
            Assert.IsTrue(thunderbird.IsDisposed);
        }

        #endregion Tests (3)
    }
}