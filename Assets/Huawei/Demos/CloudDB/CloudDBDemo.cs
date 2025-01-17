﻿using HmsPlugin;

using HuaweiMobileServices.AuthService;
using HuaweiMobileServices.Base;
using HuaweiMobileServices.CloudDB;
using HuaweiMobileServices.Common;
using HuaweiMobileServices.Id;
using HuaweiMobileServices.Utils;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Text = UnityEngine.UI.Text;

public class CloudDBDemo : MonoBehaviour
{
    private string TAG = "CloudDBDemo";
    private HMSAuthServiceManager authServiceManager = null;
    private AGConnectUser user = null;
    private Text loggedInUser;

    private const string NOT_LOGGED_IN = "No user logged in";
    private const string LOGGED_IN = "{0} is logged in";
    private const string LOGGED_IN_ANONYMOUSLY = "Anonymously Logged In";
    private const string LOGIN_ERROR = "Error or cancelled login";

    private HMSCloudDBManager cloudDBManager = null;
    private readonly string cloudDBZoneName = "QuickStartDemo";
    private readonly string BookInfoClass = "com.refapp.stack.huawei.BookInfo";
    private readonly string ObjectTypeInfoHelper = "com.refapp.stack.huawei.ObjectTypeInfoHelper";
    List<GameSessions> bookInfoList = null;

    public void Start()
    {
        loggedInUser = GameObject.Find("LoggedUserText").GetComponent<Text>();
        loggedInUser.text = NOT_LOGGED_IN;

        authServiceManager = HMSAuthServiceManager.Instance;
        authServiceManager.OnSignInSuccess = OnAuthSericeSignInSuccess;
        authServiceManager.OnSignInFailed = OnAuthSericeSignInFailed;

        if (authServiceManager.GetCurrentUser() != null)
        {
            user = authServiceManager.GetCurrentUser();
            loggedInUser.text = user.IsAnonymous() ? LOGGED_IN_ANONYMOUSLY : string.Format(LOGGED_IN, user.DisplayName);
        }
        else
        {
            SignInWithHuaweiAccount();
        }

        cloudDBManager = HMSCloudDBManager.Instance;
        cloudDBManager.Initialize();
        cloudDBManager.GetInstance(AGConnectInstance.GetInstance(), AGConnectAuth.GetInstance());
        cloudDBManager.OnExecuteQuerySuccess = OnExecuteQuerySuccess;
        cloudDBManager.OnExecuteQueryFailed = OnExecuteQueryFailed;
    }

    private void OnAccountKitLoginSuccess(AuthAccount authHuaweiId)
    {
        AGConnectAuthCredential credential = HwIdAuthProvider.CredentialWithToken(authHuaweiId.AccessToken);
        authServiceManager.SignIn(credential);
    }

    public void SignInWithHuaweiAccount()
    {
        HMSAccountKitManager.Instance.OnSignInSuccess = OnAccountKitLoginSuccess;
        HMSAccountKitManager.Instance.OnSignInFailed = OnAuthSericeSignInFailed;
        HMSAccountKitManager.Instance.SignIn();
    }

    private void OnAuthSericeSignInFailed(HMSException error)
    {
        loggedInUser.text = LOGIN_ERROR;
    }

    private void OnAuthSericeSignInSuccess(SignInResult signInResult)
    {
        user = signInResult.GetUser();
        loggedInUser.text = user.IsAnonymous() ? LOGGED_IN_ANONYMOUSLY : string.Format(LOGGED_IN, user.DisplayName);
    }

    public void CreateObjectType()
    {
        cloudDBManager.CreateObjectType(ObjectTypeInfoHelper);
    }

    public void GetCloudDBZoneConfigs()
    {
        IList<CloudDBZoneConfig> CloudDBZoneConfigs = cloudDBManager.GetCloudDBZoneConfigs();
        Debug.Log($"{TAG} " + CloudDBZoneConfigs.Count);
    }

    public void OpenCloudDBZone()
    {
        cloudDBManager.OpenCloudDBZone(cloudDBZoneName, CloudDBZoneConfig.CloudDBZoneSyncProperty.CLOUDDBZONE_CLOUD_CACHE, CloudDBZoneConfig.CloudDBZoneAccessProperty.CLOUDDBZONE_PUBLIC);
    }

    public void OpenCloudDBZone2()
    {
        cloudDBManager.OpenCloudDBZone2(cloudDBZoneName, CloudDBZoneConfig.CloudDBZoneSyncProperty.CLOUDDBZONE_CLOUD_CACHE, CloudDBZoneConfig.CloudDBZoneAccessProperty.CLOUDDBZONE_PUBLIC);
    }

    public void EnableNetwork() => cloudDBManager.EnableNetwork(cloudDBZoneName);
    public void DisableNetwork() => cloudDBManager.DisableNetwork(cloudDBZoneName);

    public void AddBookInfo()
    {
        BookInfo bookInfo = new BookInfo();
        bookInfo.Id = 1;
        bookInfo.BookName = "bookName";
        bookInfo.Author = "Author 1";
        cloudDBManager.ExecuteUpsert(bookInfo);
    }

    public void AddBookInfoList()
    {
        IList<AndroidJavaObject> bookInfoList = new List<AndroidJavaObject>();

        BookInfo bookInfo1 = new BookInfo();
        bookInfo1.Id = 2;
        bookInfo1.Author = "Author 2";
        bookInfoList.Add(bookInfo1.GetObj());

        BookInfo bookInfo2 = new BookInfo();
        bookInfo2.Id = 3;
        bookInfo2.Author = "Author 3";
        bookInfoList.Add(bookInfo2.GetObj());

        cloudDBManager.ExecuteUpsert(bookInfoList);
    }

    public void UpdateBookInfo()
    {
        BookInfo bookInfo = new BookInfo();
        bookInfo.Id = 1;
        bookInfo.BookName = "bookName";
        bookInfo.Author = "Author 1";
        bookInfo.Price = 300;
        cloudDBManager.ExecuteUpsert(bookInfo);
    }

    public void DeleteBookInfo()
    {
        BookInfo bookInfo = new BookInfo();
        bookInfo.Id = 1;
        cloudDBManager.ExecuteDelete(bookInfo);
    }

    public void DeleteBookInfoList()
    {
        IList<AndroidJavaObject> bookInfoList = new List<AndroidJavaObject>();

        BookInfo bookInfo1 = new BookInfo();
        bookInfo1.Id = 2;
        bookInfo1.Author = "Author 2";
        bookInfoList.Add(bookInfo1.GetObj());

        BookInfo bookInfo2 = new BookInfo();
        bookInfo2.Id = 3;
        bookInfo2.Author = "Author 3";
        bookInfoList.Add(bookInfo2.GetObj());

        cloudDBManager.ExecuteDelete(bookInfoList);
    }

    public void GetBookInfo()
    {
        CloudDBZoneQuery mCloudQuery = CloudDBZoneQuery.Where(new AndroidJavaClass(BookInfoClass));
        cloudDBManager.ExecuteQuery(mCloudQuery, CloudDBZoneQuery.CloudDBZoneQueryPolicy.CLOUDDBZONE_CLOUD_CACHE);
    }

    private void OnExecuteQuerySuccess(CloudDBZoneSnapshot<GameSessions> snapshot) => ProcessQueryResult(snapshot);

    private void OnExecuteQueryFailed(HMSException error) => Debug.Log($"{TAG} OnExecuteQueryFailed(HMSException error) => {error.WrappedExceptionMessage}");

    private void ProcessQueryResult(CloudDBZoneSnapshot<GameSessions> snapshot)
    {
        CloudDBZoneObjectList<GameSessions> bookInfoCursor = snapshot.GetSnapshotObjects();
        bookInfoList = new List<GameSessions>();
        try
        {
            while (bookInfoCursor.HasNext())
            {
                GameSessions bookInfo = bookInfoCursor.Next();
                bookInfoList.Add(bookInfo);
                //Debug.Log($"{TAG} bookInfoCursor.HasNext() {bookInfo.Id}  {bookInfo.Author}");
            }
        }
        catch (Exception e)
        {
            Debug.Log($"{TAG} processQueryResult:  Exception => " + e.Message);
        }
        finally
        {
            snapshot.Release();
        }
    }

    public void ExecuteSumQuery()
    {
        CloudDBZoneQuery mCloudQuery = CloudDBZoneQuery.Where(new AndroidJavaClass(BookInfoClass));
        cloudDBManager.ExecuteSumQuery(mCloudQuery, "price", CloudDBZoneQuery.CloudDBZoneQueryPolicy.CLOUDDBZONE_LOCAL_ONLY);
    }

    public void ExecuteCountQuery()
    {
        CloudDBZoneQuery mCloudQuery = CloudDBZoneQuery.Where(new AndroidJavaClass(BookInfoClass));
        cloudDBManager.ExecuteCountQuery(mCloudQuery, "price", CloudDBZoneQuery.CloudDBZoneQueryPolicy.CLOUDDBZONE_LOCAL_ONLY);
    }

}
