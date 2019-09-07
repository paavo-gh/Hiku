
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Hiku.Core
{
    public interface DataReceiver
    {
        void SetRegistered(bool registered);
        void Dispose();
    }

    public interface ReceiverBuilder
    {
        void Receive(DataReceiver receiver);
    }

    public class Receivers : ReceiverBuilder
    {
        List<DataReceiver> receivers = new List<DataReceiver>();

        public void Receive(DataReceiver receiver) => receivers.Add(receiver);

        public void SetRegistered(bool registered)
        {
            for (int i = 0; i < receivers.Count; i++)
                receivers[i].SetRegistered(registered);
        }

        public void Dispose()
        {
            for (int i = 0; i < receivers.Count; i++)
                receivers[i].Dispose();
            receivers.Clear();
        }
    }

    /// <summary>
    /// Wraps a single parameter method that can work as a receiver.
    /// </summary>
    public class ReceiverMethod
    {
        public readonly Type Type;
        public readonly MethodInfo Method;
        public readonly Receive Attribute;
        public string Name => Method.Name;

        private ReceiverMethod(MethodInfo method, Type parameterType, Receive attribute)
        {
            this.Method = method;
            this.Type = parameterType;
            this.Attribute = attribute;
        }

        public static List<ReceiverMethod> GetAll(object source)
        {
            var list = new List<ReceiverMethod>();

            foreach (var method in source.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<Receive>();
                if (attribute != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1)
                        list.Add(new ReceiverMethod(method, parameters[0].ParameterType, attribute));
                    else
                        Debug.LogError($"[{source.GetType().Name}] Receiver method {method.Name} has invalid number of parameters ({parameters.Length})");
                }
            }

            list.Sort(CompareByOrder);
            
            return list;
        }

        private static int CompareByOrder(ReceiverMethod x, ReceiverMethod y)
            => x.Attribute.Order.CompareTo(y.Attribute.Order);
    }
}