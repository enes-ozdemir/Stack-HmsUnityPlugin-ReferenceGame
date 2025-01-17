﻿using HuaweiMobileServices.Crash;
using HuaweiMobileServices.Utils;
using UnityEngine;
using UnityEngine.Diagnostics;

public class HMSCrashManager : HMSManagerSingleton<HMSCrashManager>
{
    IAGConnectCrash agConnectCrash;

    public HMSCrashManager()
    {
        Debug.Log($"[HMS] : HMSCrashManager Constructor");
        if (!HMSDispatcher.InstanceExists)
            HMSDispatcher.CreateDispatcher();
        HMSDispatcher.InvokeAsync(OnAwake);
    }

    private void OnAwake()
    {
        Debug.Log("[HMS]: Crash OnAwake - Initialized");
        agConnectCrash = AGConnectCrash.GetInstance();
    }

    //Crash Collection enable/disable method used on AnalyticsDemo scene with enable/disable radio button configuration 
    public void EnableCrashCollection(bool value)
    {
        agConnectCrash.EnableCrashCollection(value);
        Debug.Log($"[HMS]: Crash enableCrashCollection {value}");
    }

    public void TestCrash()
    {
        Debug.Log("[HMS]: Crash testIt");
        Utils.ForceCrash(0);
    }

    enum Log
    {
        DEBUG = 3,
        INFO = 4,
        WARN = 5,
        ERROR = 6,
    }

    public void customReport()
    {
        agConnectCrash.SetUserId("testuser");
        agConnectCrash.Log((int)Log.DEBUG, "set debug log.");
        agConnectCrash.Log((int)Log.INFO, "set info log.");
        agConnectCrash.Log((int)Log.WARN, "set warning log.");
        agConnectCrash.Log((int)Log.ERROR, "set error log.");
        agConnectCrash.SetCustomKey("stringKey", "Hello world");
        agConnectCrash.SetCustomKey("booleanKey", false);
        agConnectCrash.SetCustomKey("doubleKey", 1.1);
        agConnectCrash.SetCustomKey("floatKey", 1.1f);
        agConnectCrash.SetCustomKey("intKey", 0);
        agConnectCrash.SetCustomKey("longKey", 11L);
        Debug.Log("[HMS]: Crash customReport");
    }
}
