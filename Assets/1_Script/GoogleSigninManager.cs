using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class GoogleSigninManager : MonoBehaviour
{
    //public Text infoText;
    public GameObject SigninPage;
    public string webClientId = "615960248310-icso3iasprglng283rh9kcl882bd2do9.apps.googleusercontent.com";

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    auth = FirebaseAuth.DefaultInstance;
                else
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
        }
        else
        {
            AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            AddToInformation("Email = " + task.Result.Email);
            AddToInformation("Google ID Token = " + task.Result.IdToken);
            //AddToInformation("Email = " + task.Result.Email); 
            //print("UserAuthCode: " + task.Result.AuthCode);
            //print("UserGetHashCode: " + task.Result.GetHashCode());
            print("UserId: " + task.Result.UserId);
            SignInWithGoogleOnFirebase(task.Result.IdToken, task.Result.UserId);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken, string userId)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
                //성공시 실행
                AddToInformation("Sign In Successful.");
                UserManager.Instance.newUserInformation.userId = userId;
                checkOnboarding(userId);
                SigninPage.SetActive(false);
            }
        });
    }
    //해당 유저가 이미 온보딩 했던 유저인지 확인
    private async void checkOnboarding(string userId)
    {
        //해당 유저 이메일이 userList에 있는지 확인
        bool isThereUserId = await GetUserEmailList(userId);
        if (isThereUserId)
        {
            UserDB userDB = await GetUserDB(userId);
            //이미 해당 유저 데이터가 존재하고 이름도 존재한다면
            if (!string.IsNullOrEmpty(userDB.userInformation.userName))
            {
                SceneManager.LoadScene("1_Home");
                UserManager.Instance.newUserInformation.titleCheck[0]++;

            }
            else { return; }
        }
        else { return; }
    }
    //유저 이메일 리스트 가져오기
    private async Task<bool> GetUserEmailList(string userId)
    {
        bool isThereUserEmail = false;

        DatabaseReference userList = FirebaseDatabase.DefaultInstance.GetReference("userList");
        print("getUserList");
        try
        {
            var taskResult = await userList.GetValueAsync();
            foreach(var user in taskResult.Children)
            {
                if (user.Key == userId) { isThereUserEmail = true; }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        }
        print("istThereUserEmail : "+isThereUserEmail);
        return isThereUserEmail;
    }


    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToInformation("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    //DB에서 유저 data 가져오기
    private async Task<UserDB> GetUserDB(string userId)
    {
        UserDB userDB = new UserDB();
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        try
        {
            var taskResult = await dataReference.GetValueAsync();
            userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            //DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        }
        return userDB;
    }

    private void AddToInformation(string str) { print(str); }
}
