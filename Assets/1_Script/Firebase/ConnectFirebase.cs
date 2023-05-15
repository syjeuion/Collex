using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using Google;
using Firebase;

public class ConnectFirebase : MonoBehaviour
{
    public FirebaseAuth auth;

    //private SignInClient oneTapClient;
    //private BeginSignInRequest signInRequest;

    private void Awake()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;
    }

    public void OnGoogleSignIn()
    {
        /*Task<GoogleSignInUser> signInTask = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();

        signInTask.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                return;
            }
            if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                return;
            }

            // Sign in to Firebase with the Google credential
            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWith(signInTask =>
            {
                if (signInTask.IsCanceled)
                {
                    signInCompleted.SetCanceled();
                    return;
                }
                if (signInTask.IsFaulted)
                {
                    signInCompleted.SetException(signInTask.Exception);
                    return;
                }

                // User sign-in succeeded
                signInCompleted.SetResult(signInTask.Result);
            });
        });*/
    }

}
