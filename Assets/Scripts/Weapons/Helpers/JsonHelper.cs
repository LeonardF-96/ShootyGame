using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    //// Generic method to parse JSON arrays with a wrapper
    //public static List<T> FromJsonArray<T>(string json)
    //{
    //    ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
    //    return wrapper.array ?? new List<T>();
    //}

    //[System.Serializable]
    //private class ArrayWrapper<T>
    //{
    //    public List<T> array;
    //}

    public static List<T> FromJsonArray<T>(string json)
    {
        // Wrap the JSON array in a dummy object to make it a valid JSON object
        string wrappedJson = $"{{\"items\":{json}}}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}