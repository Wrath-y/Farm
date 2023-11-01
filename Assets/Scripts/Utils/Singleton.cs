using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get => _instance;
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
            return;
        }

        _instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
