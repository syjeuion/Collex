using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

public class FriendsManager : MonoBehaviour
{
    //오브젝트
    public GameObject friendsPage;
    public GameObject scrollView_content_friendsList;
    public GameObject prefabs_singleFriendProfile;
    GameObject newFriendProfile;

    //프로필이미지
    public Sprite[] array_profileImg;

    //입사동기 추가하기(검색)
    public TMP_InputField searchText;
    public GameObject scrollView_content_searchedfriend;
    public GameObject searchedFriendProfile;

    //유저 이름 리스트
    private List<string> userNameList = new List<string>();

    //데이터베이스 레퍼런스
    DatabaseReference userListDB;

    //시작 시 firebase 초기화
    private void Start()
    {
        // Firebase 초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            //reference = FirebaseDatabase.DefaultInstance.RootReference;

            //// 데이터베이스 경로 지정
            //DatabaseReference playerReference = reference.Child("players").Child("player1");

            //// ValueEventListener를 사용하여 데이터 읽어오기
            //playerReference.ValueChanged += HandlePlayerDataChange;
        });

        //유저 이름 저장 리스트
        userListDB = FirebaseDatabase.DefaultInstance.GetReference("userList");
        userListDB.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted) { Debug.LogError("getUserNameList Error"); }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var userName in snapshot.Children)
                {
                    print(userName.Key);
                    userNameList.Add(userName.Key);
                }
            }
        });
    }

    //입사동기 페이지 오픈
    public void OpenFriendsPage()
    {
        friendsPage.SetActive(true);
        GetFriendsList();
    }

    //친구 리스트 불러오기
    private void GetFriendsList()
    {
        string userName = UserManager.Instance.newUserInformation.userName;
        //DatabaseReference usersFriendsListDB = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userName).Child("friendsList");
        DatabaseReference userDB = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userName);
        userDB.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("userDB.GetValueAsync().ContinueWith : faulted");
            }
            else if (task.IsCompleted)
            {
                //DataSnapshot snapshot = task.Result;
                UserDB newUserData = JsonConvert.DeserializeObject<UserDB>(task.Result.GetRawJsonValue());
                List<UserDefaultInformation> usersFriendsList = new List<UserDefaultInformation>();
                print(newUserData.friendsList.Count);
                if (newUserData.friendsList.Count > 0)
                {
                    usersFriendsList = newUserData.friendsList;
                    //친구 리스트 띄우기
                    foreach (UserDefaultInformation friend in usersFriendsList)
                    {
                        newFriendProfile = Instantiate(prefabs_singleFriendProfile, scrollView_content_friendsList.transform);
                        newFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[friend.userProfileImg];
                        newFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = friend.userTitle + " · " + friend.userJob;
                        newFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = friend.userName;
                        newFriendProfile.transform.GetChild(3).gameObject.SetActive(false);
                    }
                }
                
            }
        });
    }

    //친구 이름으로 검색하기
    UserDefaultInformation searchedUser;
    public async void SearchUserName()
    {
        string searchedName = searchText.text;
        if (userNameList.Contains(searchedName) && searchedName!=UserManager.Instance.newUserInformation.userName)
        {
            try
            {
                searchedUser = await GetUserInformationAsync(searchedName); //데이터 받아올 때까지 기다렸다가
                SetSearchedUserUI(searchedUser); //받아오면 UI 출력하기
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); } //예외처리
        }
        else { print("해당하는 유저 이름이 없습니다."); }
    }
    //유저 정보 데이터 받아오기
    private async Task<UserDefaultInformation> GetUserInformationAsync(string searchedName)
    {
        var taskResult = await userListDB.Child(searchedName).Child("userInformation").GetValueAsync();
        searchedUser = JsonConvert.DeserializeObject<UserDefaultInformation>(taskResult.GetRawJsonValue());
        return searchedUser;
    }
    //UI처리
    private void SetSearchedUserUI(UserDefaultInformation searchedUserInfo)
    {
        print(searchedUser.userName);
        //newFriendProfile = Instantiate(prefabs_singleFriendProfile, scrollView_content_searchedfriend.transform);
        //newFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[searchedUserInfo.userProfileImg];
        //newFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = searchedUserInfo.userTitle + " · " + searchedUserInfo.userJob;
        //newFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = searchedUserInfo.userName;
        searchedFriendProfile.SetActive(true);
        searchedFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[searchedUserInfo.userProfileImg];
        searchedFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = searchedUserInfo.userTitle + " · " + searchedUserInfo.userJob;
        searchedFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = searchedUserInfo.userName;
    }

    //푸시 보내기
}
