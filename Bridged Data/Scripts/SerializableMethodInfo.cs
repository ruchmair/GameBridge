//This script was created by Bunny83 
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class SerializableMethodInfo : ISerializationCallbackReceiver
{
    public SerializableMethodInfo(MethodInfo aMethodInfo)
    {
        methodInfo = aMethodInfo;
    }
    public MethodInfo methodInfo;
    public SerializableType type;
    public string methodName;
    public List<SerializableType> parameters = null;
    public int flags = 0;

    public void OnBeforeSerialize()
    {
        if (methodInfo == null)
            return;
        type = new SerializableType(methodInfo.DeclaringType);
        methodName = methodInfo.Name;
        if (methodInfo.IsPrivate)
            flags |= (int)BindingFlags.NonPublic;
        else
            flags |= (int)BindingFlags.Public;
        if (methodInfo.IsStatic)
            flags |= (int)BindingFlags.Static;
        else
            flags |= (int)BindingFlags.Instance;
        var p = methodInfo.GetParameters();
        if (p != null && p.Length > 0)
        {
            parameters = new List<SerializableType>(p.Length);
            for (int i = 0; i < p.Length; i++)
            {
                parameters.Add(new SerializableType(p[i].ParameterType));
            }
        }
        else
            parameters = null;
    }

    public void OnAfterDeserialize()
    {
        if (type == null || string.IsNullOrEmpty(methodName))
            return;
        var t = type.type;
        System.Type[] param = null;
        if (parameters != null && parameters.Count > 0)
        {
            param = new System.Type[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                param[i] = parameters[i].type;
            }
        }
        if (param == null)
            methodInfo = t.GetMethod(methodName, (BindingFlags)flags);
        else
            methodInfo = t.GetMethod(methodName, (BindingFlags)flags, null, param, null);
    }
}