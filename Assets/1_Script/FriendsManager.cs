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
    #region 오브젝트 모음
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
        //편집하기
    public GameObject menuArea;
    public GameObject dialog_edit_friend_dialog;
    //사원증
    public GameObject idcard_front;
    public GameObject idcard_back;
    public Animator friendIdcardAni;

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
    //DatabaseReference userListDB;
    DatabaseReference thisUserReference;

    //사원증 컬러
    string[,] colorList = new string[5, 2] {
        { "#FFDC00", "#D79044" }, //노랑:0
        { "#FE944D", "#FE944D" }, //오렌지:1
        { "#06C755", "#06C755" }, //초록:2
        { "#2AC1BC", "#2AC1BC" }, //민트:3
        { "#408BFD", "#408BFD" } }; //블루:4
    #endregion

    #region 시작 시 실행
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
        catch(Exception e)
        {   Debug.LogError("Error: " + e.Message);
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        }
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
        DatabaseReference userListDB = FirebaseDatabase.DefaultInstance.GetReference("userList");
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
    #endregion

    //입사동기 페이지 오픈
    public void OpenFriendsPage()
    {
        friendsPage.SetActive(true);

        //기존 리스트 초기화
        for (int i = 0; i < content_friendsList.transform.childCount; i++)
        { Destroy(content_friendsList.transform.GetChild(i).gameObject); }

        GetFriendsList();
    }
    //친구 리스트 불러오기
    private async void GetFriendsList()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserName);
        print("get this user DB done");
        if (thisUserDB.friendsDictionary!=null&& thisUserDB.friendsDictionary.Count > 0)
        {
            print("this user friend dictionray count: "+thisUserDB.friendsDictionary.Count);
            friendsPage.transform.GetChild(2).gameObject.SetActive(false); //empty area 비활성화
            foreach(string key in thisUserDB.friendsDictionary.Keys)
            {
                UserDefaultInformation newUserInfo = thisUserDB.friendsDictionary[key];
                newFriendProfile = Instantiate(prefabs_singleFriendProfile, content_friendsList.transform);
                newFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[newUserInfo.userProfileImg];
                newFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = newUserInfo.userTitle + " · " + newUserInfo.userJob;
                newFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = newUserInfo.userName;
                newFriendProfile.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(SetDialogEditFriend);
                newFriendProfile.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(OpenIdCard);
            }
        }
        else {
            print("this user friend dictionray is null or count 0");
            friendsPage.transform.GetChild(2).gameObject.SetActive(true); }
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

        //이미 친구인 유저면 버튼 비활성화
        bool alreadyFriend = false;
        foreach(string key in userDB.friendsDictionary.Keys)
        {
            if(key == thisUserName) { alreadyFriend = true; }
        }
        if (alreadyFriend) { searchedFriendProfile.transform.GetChild(3).gameObject.SetActive(false); }

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
    //알림 페이지 닫기
    public void CloseNotificationPage()
    {
        notificationPage.SetActive(false);
        home_icon_notification.sprite = selector_icon_notification[0];
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
                //newAlarmRequestFriend.transform.GetChild(1).GetComponent<TMP_Text>().text = requestFriend.requestDate;
                newAlarmRequestFriend.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = array_profileImg[requestFriend.userInformation.userProfileImg];
                newAlarmRequestFriend.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = requestFriend.userInformation.userJob;
                newAlarmRequestFriend.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = requestFriend.userInformation.userName;
                newAlarmRequestFriend.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(UpdateFriendsList);
                newAlarmRequestFriend.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(DeleteRequestFriend);
            }
        }
    }
    //수락 시 친구 리스트 업데이트
    private async void UpdateFriendsList()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        //string userName = thisObj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text;

        //현재 유저의 DB에 친구 리스트 추가
        int thisObjIndex = thisObj.transform.GetSiblingIndex();
        UserDefaultInformation newFriendInfo = thisUserDB.friendsRequestList[thisObjIndex].userInformation;
        thisUserDB.friendsDictionary.Add(newFriendInfo.userName,newFriendInfo);

        //기존 request list에서 해당 유저 정보 삭제
        thisUserDB.friendsRequestList.RemoveAt(thisObjIndex);

        //현재 유저 DB 업데이트
        UpdateUserDB(thisUserName, thisUserDB);

        //요청한 유저의 DB에 현재 유저 정보 추가
        UserDB friendDB = await GetUserDB(newFriendInfo.userName);
        friendDB.friendsDictionary.Add(thisUserName,thisUserDB.userInformation);
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

    #region 입사동기 목록 편집하기
    //편집화면으로 세팅
    public void SetEditFriendsList()
    {
        //메뉴 버튼 비활성화
        menuArea.SetActive(false);
        //앱바 교체
        friendsPage.transform.GetChild(5).gameObject.SetActive(true);
        //리스트에 - 버튼 활성화
        for(int i=0; i<content_friendsList.transform.childCount; i++)
        {
            content_friendsList.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
            content_friendsList.transform.GetChild(i).GetChild(4).gameObject.SetActive(false);
        }
    }
    //디폴트 화면으로 되돌리기 //편집 완료
    public void ReturnDefaultFriendPage()
    {
        //앱바 교체
        print("return default page : start");
        friendsPage.transform.GetChild(5).gameObject.SetActive(false);
        //리스트에 - 버튼 비활성화
        print("return default page content child count: "+ content_friendsList.transform.childCount);
        for (int i = 0; i < content_friendsList.transform.childCount; i++)
        {
            content_friendsList.transform.GetChild(i).GetChild(3).gameObject.SetActive(false);
            content_friendsList.transform.GetChild(i).GetChild(4).gameObject.SetActive(true);
        }
        print("return default page : childcount check");
        //모든 친구가 삭제되었으면 empty area 활성화
        if (content_friendsList.transform.childCount == 0)
        {
            friendsPage.transform.GetChild(2).gameObject.SetActive(true);
        }
        print("return default page : end");
    }
    //리스트에서 삭제 아이콘 클릭 시 삭제 dialog 오픈
    GameObject selectedUserProfileObj;
    //int selectedUserIndex;
    string selectedUserName;
    private void SetDialogEditFriend()
    {
        selectedUserProfileObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        //selectedUserIndex = selectedUserProfileObj.transform.parent.GetSiblingIndex();
        selectedUserName = selectedUserProfileObj.transform.GetChild(2).GetComponent<TMP_Text>().text;

        dialog_edit_friend_dialog.transform.parent.gameObject.SetActive(true);
        dialog_edit_friend_dialog.transform.GetChild(1).GetComponent<TMP_Text>().text =
            $"'{selectedUserName}'님을 입사동기 목록에서 삭제할까요?";
    }
    //dialog 삭제하기
    public async void DialogDeleteFriend()
    {
        //현재 유저 친구 리스트에서 삭제
        thisUserDB.friendsDictionary.Remove(selectedUserName);
        Destroy(selectedUserProfileObj);
        dialog_edit_friend_dialog.transform.parent.gameObject.SetActive(false);
        UpdateUserDB(thisUserName, thisUserDB);
        print("this user friend dictionary remove done");

        //상대 유저 친구 리스트에서 삭제
        UserDB friendDB = await GetUserDB(selectedUserName);
        friendDB.friendsDictionary.Remove(thisUserName);
        UpdateUserDB(selectedUserName, friendDB);
        print("friend user friend dictionary remove done");
    }
    #endregion

    #region 입사동기 사원증
    //사원증 오픈
    private async void OpenIdCard()
    {
        string friendName = EventSystem.current.currentSelectedGameObject.transform.parent.GetChild(2).GetComponent<TMP_Text>().text;
        idcard_front.transform.parent.gameObject.SetActive(true);
        idcard_front.SetActive(true);
        idcard_back.SetActive(false);

        //친구 데이터 가져오기
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        UserDB friendDB = await GetUserDB(friendName);
        SetIdcardFront(friendDB);
        SetIdcardBack(friendDB);
    }
    //앞면 데이터 세팅
    private void SetIdcardFront(UserDB friendDB)
    {
        UserDefaultInformation friendDefaultInfo = friendDB.userInformation;
        ColorUtility.TryParseHtmlString(colorList[friendDB.idcardColor, 0], out Color cardColor);
        ColorUtility.TryParseHtmlString(colorList[friendDB.idcardColor, 1], out Color companyColor);

        idcard_front.transform.GetChild(0).GetComponent<Image>().color = cardColor;
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = array_profileImg[friendDefaultInfo.userProfileImg];
        idcard_front.transform.GetChild(2).GetComponent<TMP_Text>().text = friendDefaultInfo.userTitle;
        idcard_front.transform.GetChild(3).GetComponent<TMP_Text>().text = friendDefaultInfo.userName;
        idcard_front.transform.GetChild(4).GetComponent<TMP_Text>().text = friendDB.userWishCompany;

        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = true;
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    //뒷면 데이터 세팅
    private void SetIdcardBack(UserDB friendDB)
    {
        //기록 개수
        idcard_back.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = friendDB.totalFolderCount.ToString();
        idcard_back.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = friendDB.contestFolderCount.ToString();
        idcard_back.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>().text = friendDB.projectFolderCount.ToString();
        idcard_back.transform.GetChild(5).GetChild(1).GetComponent<TMP_Text>().text = friendDB.internshipFolderCount.ToString();
        //top3 경험
        idcard_back.transform.GetChild(7).GetChild(0).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeExperiences[0];
        idcard_back.transform.GetChild(7).GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeExperiences[1];
        idcard_back.transform.GetChild(7).GetChild(0).GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeExperiences[2];
        //top3 역량
        idcard_back.transform.GetChild(7).GetChild(2).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeCapabilities[0];
        idcard_back.transform.GetChild(7).GetChild(2).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeCapabilities[1];
        idcard_back.transform.GetChild(7).GetChild(2).GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = friendDB.topThreeCapabilities[2];
    }
    //앞면 클릭 시 뒷면으로 돌아가기
    public void TurnIdCardFrontToBack()
    {
        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = false;
        friendIdcardAni.SetTrigger("frontToBack");
        idcard_back.transform.GetChild(10).GetComponent<Button>().interactable = true;
    }
    //뒷면 클릭 시 앞면으로 돌아가기
    public void TurnIdCardBackToFront()
    {
        idcard_back.transform.GetChild(10).GetComponent<Button>().interactable = false;
        friendIdcardAni.SetTrigger("backToFront");
        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = true;
    }
    #endregion

    #region 데이터 처리
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
    #endregion
}
