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
    public GameObject friendSearchPage;
    public TMP_InputField searchText;
    public GameObject searchedFriendProfile;
    public GameObject snackBar;

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
        
    }
    //현재 유저의 정보 받아오기
    private async void getThisUserDB()
    {
        try
        {
            thisUserDB = await GetUserDB(thisUserName);
            //친구 신청 있으면 알람 띄워주기
            SelectorNotificationIcon();
            //전체 유저 이름 리스트 불러와서 저장
            GetUserNameList();
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
    //전체 유저 이름 리스트 불러와서 저장
    private void GetUserNameList()
    {
        userListDB = FirebaseDatabase.DefaultInstance.GetReference("userList");
        userListDB.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted) { Debug.LogError("getUserNameList Error"); }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var userName in snapshot.Children)
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
    private async void GetFriendsList()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserName);
        if (thisUserDB.friendsList!=null&& thisUserDB.friendsList.Count > 0)
        {

            //기존 리스트 초기화
            for(int i=0; i< content_friendsList.transform.childCount; i++)
            { Destroy(content_friendsList.transform.GetChild(i).gameObject); }

            friendsPage.transform.GetChild(2).gameObject.SetActive(false); //empty page 삭제
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
    //검색 페이지 오픈
    public void OpenSearchPage()
    {
        friendSearchPage.SetActive(true);
        //검색바 리셋
        friendSearchPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = "";
        //검색 결과 리셋
        searchedFriendProfile.SetActive(false);
    }
    //친구 이름으로 검색하기
    //UserDefaultInformation searchedUser;
    string searchedName;
    public async void SearchUserName()
    {
        searchedName = searchText.text;
        if (userNameList.Contains(searchedName) && searchedName!=UserManager.Instance.newUserInformation.userName)
        {
            print("searched");
            friendSearchPage.transform.GetChild(3).gameObject.SetActive(false);
            DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
            try
            {
                //searchedUser = await GetUserInformationAsync(searchedName); //데이터 받아올 때까지 기다렸다가
                UserDB userDB = await GetUserDB(searchedName);

                SetSearchedUserUI(userDB); //받아오면 UI 출력하기
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); } //예외처리
        }
        else {
            print("fail");
            searchedFriendProfile.SetActive(false);
            friendSearchPage.transform.GetChild(3).gameObject.SetActive(true);
        }
    }
    
    //UI처리
    private void SetSearchedUserUI(UserDB userDB)
    {
        searchedFriendProfile.SetActive(true);
        //이미 요청 보낸 유저면 취소 버튼 뜨게
        bool alreadyRequest = false;
        for(int i=0; i<userDB.friendsRequestList.Count; i++)
        {
            if (userDB.friendsRequestList[i].userInformation.userName == thisUserName)
            { alreadyRequest = true; }
        }
        if (alreadyRequest)
        {
            searchedFriendProfile.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "취소";
        }
        else { searchedFriendProfile.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "추가"; }

        UserDefaultInformation userInfo = userDB.userInformation;

        searchedFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[userInfo.userProfileImg];
        searchedFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = userInfo.userTitle + " · " + userInfo.userJob;
        searchedFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = userInfo.userName;
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    #endregion

    #region 친구 요청 보내기
    //친구 요청 보내기
    public void FriendAddOrCancel()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        TMP_Text btnText = button.transform.GetChild(0).GetComponent<TMP_Text>();
        print(btnText.text);
        if(btnText.text == "추가")
        {
            SendRequestFriend();
            btnText.text = "취소";
        }
        else
        {
            CancelRequestFriend();
            btnText.text = "추가";
        }
    }
    //추가 버튼
    private async void SendRequestFriend()
    {
        //snackBar 띄우기
        StartCoroutine(SetSnackBar());

        //검색된 유저의 DB 받아오기
        print("next");
        UserDB userDB = await GetUserDB(searchedName);

        //현재 유저 정보 넣기
        RequestFriendInfo newRequestFriendInfo = new RequestFriendInfo();
        newRequestFriendInfo.requestDate = DateTime.Now.ToString("yyyy.MM.dd");
        newRequestFriendInfo.userInformation = thisUserDB.userInformation;
        userDB.friendsRequestList.Add(newRequestFriendInfo);

        //다시 검색된 유저 DB 업데이트 하기
        UpdateUserDB(searchedName, userDB);
    }
    IEnumerator SetSnackBar()
    {
        snackBar.SetActive(true);
        snackBar.transform.GetChild(0).GetComponent<TMP_Text>().text =
            searchedName + "님에게 입사동기를 신청했어요!\n상대가 수락하면 랭킹에서 볼 수 있어요.";
        yield return new WaitForSeconds(3f);
        snackBar.SetActive(false);
    }
    //취소 버튼
    private async void CancelRequestFriend()
    {
        //검색된 유저의 DB 받아오기
        UserDB userDB = await GetUserDB(searchedName);

        //현재 유저 정보 빼기
        //userDB.friendsRequestList.RemoveAt(userDB.friendsRequestList.Count - 1); //마지막에 추가된 유저 빼기
        for(int i=0; i<userDB.friendsRequestList.Count; i++)
        {
            print(userDB.friendsRequestList[i].userInformation.userName);
            if(userDB.friendsRequestList[i].userInformation.userName == thisUserName)
            { userDB.friendsRequestList.RemoveAt(i); }
        }

        //다시 검색된 유저 DB 업데이트 하기
        UpdateUserDB(searchedName, userDB);
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
    private async void CheckRequestFriend()
    {
        thisUserDB = await GetUserDB(thisUserName);
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
                newAlarmRequestFriend.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(DeleteRequestFriend);
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
        friendDB.friendsList.Add(thisUserDB.userInformation);
        UpdateUserDB(newFriendInfo.userName, friendDB);

        Destroy(thisObj); //알람 삭제
    }
    //거절 시 친구 요청 리스트에서 삭제
    private void DeleteRequestFriend()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;

        //기존 request list에서 해당 유저 정보 삭제
        int thisObjIndex = thisObj.transform.GetSiblingIndex();
        thisUserDB.friendsRequestList.RemoveAt(thisObjIndex);

        //현재 유저 DB 업데이트
        UpdateUserDB(thisUserName, thisUserDB);

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
    //private async Task<UserDefaultInformation> GetUserInformationAsync(String userName)
    //{
    //    var taskResult = await userListDB.Child(userName).Child("userInformation").GetValueAsync();
    //    searchedUser = JsonConvert.DeserializeObject<UserDefaultInformation>(taskResult.GetRawJsonValue());
    //    return searchedUser;
    //}
}
