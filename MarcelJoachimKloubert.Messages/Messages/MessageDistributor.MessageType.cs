using System;
using System.Collections.Generic;

namespace MarcelJoachimKloubert.Messages
{
    partial class MessageDistributor
    {
        internal class MessageType : IEquatable<Type>, IEquatable<MessageType>
        {
            #region Fields (2)

            internal readonly Type KEY;
            internal readonly ICollection<MessageTypeSubscription> SUBSCRIPTIONS = new HashSet<MessageTypeSubscription>();

            #endregion Fields (2)

            #region Constructors (1)

            internal MessageType(Type key)
            {
                KEY = key;
            }

            #endregion Constructors (1)

            #region Methods (5)

            public bool Equals(MessageType other)
            {
                return other != null &&
                       Equals(other.KEY);
            }

            public bool Equals(Type other)
            {
                return other == KEY;
            }

            public override bool Equals(object obj)
            {
                var type = obj as Type;
                if (type != null)
                {
                    return Equals(other: type);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return KEY.GetHashCode();
            }

            public override string ToString()
            {
                return KEY.ToString();
            }

            #endregion Methods (5)
        }
    }
}