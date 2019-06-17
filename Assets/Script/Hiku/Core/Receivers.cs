
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Hiku.Core
{
    public delegate Provider ProviderFinder(Type type);

    public interface DataReceiver
    {
        void Dispose();
    }

    public class Receivers
    {
        List<DataReceiver> receivers = new List<DataReceiver>();

        public void Receive(DataReceiver receiver) => receivers.Add(receiver);

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
        public string Name => Method.Name;

        private ReceiverMethod(MethodInfo method, Type parameterType)
        {
            this.Method = method;
            this.Type = parameterType;
        }

        public static List<ReceiverMethod> GetAll(object source)
        {
            var list = new List<ReceiverMethod>();

            foreach (var method in source.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.GetCustomAttribute<Receive>() != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        Debug.LogError($"[{source.GetType().Name}] Receiver method {method.Name} has invalid number of parameters ({parameters.Length})");
                        return null;
                    }
                    list.Add(new ReceiverMethod(method, parameters[0].ParameterType));
                }
            }
            return list;
        }
    }
}