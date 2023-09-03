using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Firebase.Database;
using Firebase;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

public class FriendsManager : MonoBehaviour
{
    //홈
    public Image home_icon_notification;
    public Sprite[] selector_icon_notification;

    //알림 페이지 
    public GameObject notificationPage;
    public GameObject content_notificationList;
    public GameObject prefabs_alarm_requestFriend;
    GameObject newAlarmRequestFriend;

    //입사동기 페이지
    public GameObject friendsPage;
    public GameObject content_friendsList;
    public GameObject prefabs_singleFriendProfile;
    GameObject newFriendProfile;

    //입사동기 추가하기(검색)
    public TMP_InputField searchText;
    public GameObject searchedFriendProfile;

    //프로필이미지
    public Sprite[] array_profileImg;

    //유저 이름 리스트
    private List<string> userNameList = new List<string>();

    //현재 유저 이름
    string thisUserName;
    UserDB thisUserDB;

    //데이터베이스 레퍼런스
    DatabaseReference userListDB;
    DatabaseReference thisUserReference;

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
        //현재 유저의 정보 받아오기
        thisUserName = UserManager.Instance.newUserInformation.userName;
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        getThisUserDB();

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
                    userNameList.Add(userName.Key);
                }
            }
        });
    }
    //현재 유저의 정보 받아오기
    private async void getThisUserDB()
    {
        try
        {
            thisUserDB = await GetUserDB(thisUserName);
            //친구 신청 있으면 알람 띄워주기
            SelectorNotificationIcon();
        }
        catch(Exception e) { Debug.LogError("Error: " + e.Message); }
    }
    //알림 아이콘
    private void SelectorNotificationIcon()
    {
        if (thisUserDB.friendsRequestList != null && thisUserDB.friendsRequestList.Count > 0){
            home_icon_notification.sprite = selector_icon_notification[1]; }
        else { home_icon_notification.sprite = selector_icon_notification[0]; }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }

    //입사동기 페이지 오픈
    public void OpenFriendsPage()
    {
        friendsPage.SetActive(true);
        GetFriendsList();
    }

    //친구 리스트 불러오기
    private async void GetFriendsList()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserName);
        if (thisUserDB.friendsList!=null&& thisUserDB.friendsList.Count > 0)
        {
            //기존 리스트 초기화
            for(int i=0; i< content_friendsList.transform.childCount; i++)
            { Destroy(content_friendsList.transform.GetChild(i).gameObject); }

            friendsPage.transform.GetChild(1).gameObject.SetActive(false);
            foreach(UserDefaultInformation newUserInfo in thisUserDB.friendsList)
            {
                newFriendProfile = Instantiate(prefabs_singleFriendProfile, content_friendsList.transform);
                newFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[newUserInfo.userProfileImg];
                newFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = newUserInfo.userTitle + " · " + newUserInfo.userJob;
                newFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = newUserInfo.userName;
            }
        }
        else { friendsPage.transform.GetChild(1).gameObject.SetActive(true); }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }

    #region 친구 이름 검색하기
    //친구 이름으로 검색하기
    UserDefaultInformation searchedUser;
    string searchedName;
    public async void SearchUserName()
    {
        searchedName = searchText.text;
        if (userNameList.Contains(searchedName) && searchedName!=UserManager.Instance.newUserInformation.userName)
        {
            DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
            try
            {
                searchedUser = await GetUserInformationAsync(searchedName); //데이터 받아올 때까지 기다렸다가
                SetSearchedUserUI(searchedUser); //받아오면 UI 출력하기
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); } //예외처리
        }
        else { print("해당하는 유저 이름이 없습니다."); }
    }
    
    //UI처리
    private void SetSearchedUserUI(UserDefaultInformation searchedUserInfo)
    {
        searchedFriendProfile.SetActive(true);
        searchedFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[searchedUserInfo.userProfileImg];
        searchedFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = searchedUserInfo.userTitle + " · " + searchedUserInfo.userJob;
        searchedFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = searchedUserInfo.userName;
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    #endregion

    #region 친구 요청 보내기
    //친구 요청 보내기
    DatabaseReference searchedUserDBReference;
    public async void sendRequestFriend()
    {
        UserDB userDB = await GetSearchedUserDB();
        UpdateSearchedUserDB(userDB);
    }
    //검색된 유저의 DB 받아와서 현재 유저 정보 넣기
    private async Task<UserDB> GetSearchedUserDB()
    {
        searchedUserDBReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(searchedName);
        var taskResult = await searchedUserDBReference.GetValueAsync();
        UserDB userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
        RequestFriendInfo newRequestFriendInfo = new RequestFriendInfo();
        newRequestFriendInfo.requestDate = DateTime.Now.ToString("yyyy.MM.dd");
        newRequestFriendInfo.userInformation = thisUserDB.userInformation;
        userDB.friendsRequestList.Add(newRequestFriendInfo);
        return userDB;
    }
    //다시 검색된 유저 DB 업데이트 하기
    private void UpdateSearchedUserDB(UserDB userDB)
    {
        string newFriendRequestListJson = JsonConvert.SerializeObject(userDB);
        searchedUserDBReference.SetRawJsonValueAsync(newFriendRequestListJson).ContinueWith(task =>
        {
            if (task.IsCompleted) { Debug.Log("UpdateSearchedUserDB : IsCompleted"); }
        });
    }
    #endregion

    #region 친구 요청 알림 관련
    //알림 페이지 오픈
    public void OpenNotificationPage()
    {
        notificationPage.SetActive(true);
        CheckRequestFriend();
    }
    //요청 온 친구 리스트 띄우기
    private void CheckRequestFriend()
    {
        if (thisUserDB.friendsRequestList.Count > 0)
        {
            //기존 리스트 초기화
            for (int i = 0; i < content_notificationList.transform.childCount; i++)
            { Destroy(content_notificationList.transform.GetChild(i).gameObject); }

            foreach (RequestFriendInfo requestFriend in thisUserDB.friendsRequestList)
            {
                newAlarmRequestFriend = Instantiate(prefabs_alarm_requestFriend, content_notificationList.transform);
                newAlarmRequestFriend.transform.GetChild(1).GetComponent<TMP_Text>().text = requestFriend.requestDate;
                newAlarmRequestFriend.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = array_profileImg[requestFriend.userInformation.userProfileImg];
                newAlarmRequestFriend.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = requestFriend.userInformation.userJob;
                newAlarmRequestFriend.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = requestFriend.userInformation.userName;
                newAlarmRequestFriend.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(UpdateFriendsList);
            }
        }
    }
    //수락 시 친구 리스트 업데이트
    private async void UpdateFriendsList()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;

        //현재 유저의 DB에 친구 리스트 추가
        int thisObjIndex = thisObj.transform.GetSiblingIndex();
        UserDefaultInformation newFriendInfo = thisUserDB.friendsRequestList[thisObjIndex].userInformation;
        thisUserDB.friendsList.Add(newFriendInfo);
        //기존 request list에서 해당 유저 정보 삭제
        thisUserDB.friendsRequestList.RemoveAt(thisObjIndex);
        //현재 유저 DB 업데이트
        UpdateUserDB(thisUserName, thisUserDB);

        //요청한 유저의 DB에 현재 유저 정보 추가
        UserDB friendDB = await GetUserDB(newFriendInfo.userName);
        UpdateUserDB(newFriendInfo.userName, friendDB);

        


        Destroy(thisObj); //알람 삭제
    }
    #endregion

    //DB에서 유저 data 가져오기
    private async Task<UserDB> GetUserDB(String userName)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userName);
        var taskResult = await dataReference.GetValueAsync();
        UserDB userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
        return userDB;
    }

    //DB에 유저 data 저장하기
    private async void UpdateUserDB(String userName, UserDB userDB)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userName);
        string userDBstr = JsonConvert.SerializeObject(userDB);
        await dataReference.SetRawJsonValueAsync(userDBstr);
    }

    //DB에서 유저 기본 정보만 가져오기
    private async Task<UserDefaultInformation> GetUserInformationAsync(String userName)
    {
        var taskResult = await userListDB.Child(userName).Child("userInformation").GetValueAsync();
        searchedUser = JsonConvert.DeserializeObject<UserDefaultInformation>(taskResult.GetRawJsonValue());
        return searchedUser;
    }
}
