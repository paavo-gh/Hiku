
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Hiku.Core
{
    /// <summary>
    /// Controls how methods tagged with [Receive] attribute would receive 
    /// their data. The default behaviour, when not defined in the linker, 
    /// is to receive it's data from any component from the object itself 
    /// or the nearest parent providing the type.
    /// </summary>
    [Serializable]
    public class ReceiverLinker
    {
        public const string IngoreReceiver = "null";

        /// <summary>
        /// Building the list of receivers from methods tagged with [Receive] attribute 
        /// is done only once for each component with equal linker setup.
        /// </summary>
        static Dictionary<ReceiversCreatorKey, ReceiversCreator> CreatorCache = new Dictionary<ReceiversCreatorKey, ReceiversCreator>();

        public List<ReceiverLinkerItem> Members;

        private ReceiverLinkerItem GetMember(string id)
        {
            for (int i = 0; i < Members.Count; i++)
                if (Members[i].Receiver == id)
                    return Members[i];
            return null;
        }

        private ReceiversCreator BuildCreator(MonoBehaviour target)
        {
            var creators = new ReceiversCreator();

            foreach (var d in ReceiverMethod.GetAll(target))
            {
                var member = GetMember(d.Name);
                if (member == null)
                {
                    // The default behaviour
                    creators.Add(new ReceiverCreator(d));
                }
                else
                {
                    // Set manually
                    if (member.ReceiverType == IngoreReceiver)
                        continue;
                    
                    // Subtype of the receiver method's parameter type
                    var type = d.Type;
                    if (!string.IsNullOrEmpty(member.ReceiverType))
                    {
                        type = Type.GetType(member.ReceiverType, false);
                        if (type == null)
                        {
                            Debug.LogError($"[{target.GetType().Name}] Receiver ({member.Receiver}) type {member.ReceiverType} not found", target);
                            continue;
                        }
                    }

                    if (string.IsNullOrEmpty(member.Path))
                    {
                        creators.Add(new ReceiverCreator(d, type));
                    }
                    else
                    {
                        // Getter calls
                        try
                        {
                            var path = member.Path.Split('/');
                            creators.Add(new ReceiverCreator(d, type, GetterChainCall.Construct(type, path)));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[{target.GetType().Name}] Receiver {member.Receiver} type {type.Name}:{member.Path} creation failed:\n{e.Message}\n{e.StackTrace}", target);
                            continue;
                        }
                    }
                }
            }
            return creators;
        }

        public Receivers Build(MonoBehaviour target)
        {
            // May happen if never inspected in editor
            if (Members == null)
                Members = new List<ReceiverLinkerItem>();

            var key = new ReceiversCreatorKey(target.GetType(), this);
            ReceiversCreator creator;
            if (!CreatorCache.TryGetValue(key, out creator))
                CreatorCache.Add(key, creator = BuildCreator(target));
            return creator.Create(target);
        }
    }

    [Serializable]
    public class ReceiverLinkerItem
    {
        /// <summary>
        /// Name of the receiving method.
        /// </summary>
        public string Receiver;

        /// <summary>
        /// If set, used instead of the receiver's type.
        /// </summary>
        public string ReceiverType;

        /// <summary>
        /// Optional chain of methods to retreive the data separated by '/'.
        /// </summary>
        public string Path;

        public override bool Equals(object obj)
        {
            var o = obj as ReceiverLinkerItem;
            if (o == null)
                return false;
            return o.Receiver == Receiver && o.ReceiverType == ReceiverType && o.Path == Path;
        }

        public override int GetHashCode()
        {
            return Receiver.GetHashCode() ^ ReceiverType.GetHashCode() ^ Path.GetHashCode();
        }
    }

    class ReceiversCreatorKey
    {
        Type type;
        ReceiverLinker linker;

        public ReceiversCreatorKey(Type type, ReceiverLinker linker)
        {
            this.type = type;
            this.linker = linker;
        }

        public override bool Equals(object obj)
        {
            var o = obj as ReceiversCreatorKey;
            if (o == null)
                return false;
            return o.type == type && o.linker.Members.SequenceEqual(o.linker.Members);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (var member in linker.Members)
                    hash = hash * 31 + member.GetHashCode();
                return type.GetHashCode() ^ hash;
            }
        }
    }
}