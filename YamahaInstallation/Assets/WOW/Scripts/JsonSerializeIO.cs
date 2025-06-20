using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class Serialize<T>
{
    [SerializeField] List<T> list;

    public List<T> ToList() { return list; }

    public Serialize(List<T> list)
    {
        this.list = list;
    }
}

[Serializable]
public class Serialize<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] List<TKey> keys;
    [SerializeField] List<TValue> values;
    Dictionary<TKey, TValue> dictionary;

    public Dictionary<TKey, TValue> ToDictionary() { return dictionary; }

    public Serialize(Dictionary<TKey, TValue> dictionary)
    {
        this.dictionary = dictionary;
    }

    public void OnBeforeSerialize()
    {
        keys = new List<TKey>(dictionary.Keys);

        values = new List<TValue>(dictionary.Values);
    }

    public void OnAfterDeserialize()
    {
        var count = Math.Min(keys.Count, values.Count);

        dictionary = new Dictionary<TKey, TValue>(count);

        for (var i = 0; i < count; ++i) dictionary.Add(keys[i], values[i]);
    }
}

public class JsonSerializeIO
{
    public static void Save<T>(T serialize, String filePath)
    {
        string json = JsonUtility.ToJson(serialize);

        using (StreamWriter streamWriter = new StreamWriter(filePath))
        {
            streamWriter.Write(json);
            streamWriter.Flush();
        };
    }

    public static string GetJsonString<T>(T serialize)
    {
        return JsonUtility.ToJson(serialize);
    }

    public static T LoadFromJsonString<T>(String json)
    {
        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (FileNotFoundException e)
        {
            Debug.Log(e);

            return default(T);
        };
    }

    public static T Load<T>(String filePath)
    {
        string json;
        try
        {
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                json = streamReader.ReadToEnd();
            };

            return JsonUtility.FromJson<T>(json);
        }
        catch (FileNotFoundException e)
        {
            Debug.Log(e);

            return default(T);
        };
    }
}