using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject tab_applyFriend;
    public GameObject content_noti_applyFriend;
    public GameObject prefabs_noti_applyFriend;
    GameObject newNotiApplyFriend;
    public GameObject content_noti_cheerUp;
    public GameObject prefabs_noti_cheerUp;
    GameObject newNotiCheerUp;

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
    public Button idcard_cheerUp;

    //입사동기 추가하기(검색)
    public GameObject friendSearchPage;
    public GameObject SearchButtonGroup;
    public GameObject SearchEmptyArea;
    public TMP_InputField searchText;
    public GameObject searchedFriendProfile;
    public GameObject snackBar;
    GameObject btnSendApplyFriend;

    //프로필이미지
    public Sprite[] array_profileImg;

    //유저 이름 리스트
    private List<string> userNameList = new List<string>();

    //현재 유저 이름
    string thisUserId;
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

    //컬러코드
    Color primary1;
    Color primary3;
    Color gray400;
    #endregion

    #region 시작 시 실행
    //시작 시 firebase 초기화
    private void Start()
    {
        //컬러
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#B6BBC3", out gray400);

        btnSendApplyFriend = searchedFriendProfile.transform.GetChild(3).gameObject;

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
        thisUserId = UserManager.Instance.newUserInformation.userId;
        thisUserName = UserManager.Instance.newUserInformation.userName;
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        getThisUserDB();
        
    }
    //현재 유저의 정보 받아오기
    private async void getThisUserDB()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.1f));
        try
        {
            thisUserDB = await GetUserDB(thisUserId);
            //친구 신청 있으면 알람 띄워주기
            SelectorNotificationIcon();
            //전체 유저 이름 리스트 불러와서 저장
            GetUserNameList();
        }
        catch(Exception e) {
            Debug.LogError("error: " + e.Message);
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
        }
        
    }
    //알림 아이콘
    private void SelectorNotificationIcon()
    {
        if (thisUserDB.isNewNotiCheerUp) //받은 응원 있는지 체크
        {
            home_icon_notification.sprite = selector_icon_notification[1];
            //입사동기신청도 있으면 redDot 활성화
            if (thisUserDB.isNewNotiApplyFriend)
            { tab_applyFriend.transform.GetChild(1).gameObject.SetActive(true); }
        }
        else if (thisUserDB.isNewNotiApplyFriend) //입사동기 신청 있는지 체크
        {
            home_icon_notification.sprite = selector_icon_notification[1];
            //입사동기 신청 탭 활성화
            tab_applyFriend.GetComponent<Toggle>().isOn = true;
        }
        else { home_icon_notification.sprite = selector_icon_notification[0];}

        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    //전체 유저 이름 리스트 불러와서 저장
    private void GetUserNameList()
    {
        DatabaseReference userIdList = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        userIdList.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted) { Debug.LogError("getUserNameList Error"); }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var userName in snapshot.Children)
                {
                    //print(userName.Key);
                    userNameList.Add(userName.Key);
                }
            }
        });
    }
    #endregion

    #region 입사동기 페이지 오픈
    //입사동기 페이지 오픈
    public void OpenFriendsPage()
    {
        UIController.instance.curOpenPageNum = 4;
        friendsPage.SetActive(true);
        friendSearchPage.SetActive(false);

        //기존 리스트 초기화
        for (int i = 0; i < content_friendsList.transform.childCount; i++)
        { Destroy(content_friendsList.transform.GetChild(i).gameObject); }

        GetFriendsList();
    }
    //친구 리스트 불러오기
    private async void GetFriendsList()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserId);

        if (thisUserDB.friendsDictionary!=null&& thisUserDB.friendsDictionary.Count > 0)
        {
            friendsPage.transform.GetChild(2).gameObject.SetActive(false); //empty area 비활성화
            foreach(string key in thisUserDB.friendsDictionary.Keys)
            {
                //UserDefaultInformation newUserInfo = thisUserDB.friendsDictionary[key].userInformation;
                UserDefaultInformation friendInfo = await GetUserInformationAsync(key);
                newFriendProfile = Instantiate(prefabs_singleFriendProfile, content_friendsList.transform);
                newFriendProfile.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[friendInfo.userProfileImg];
                newFriendProfile.transform.GetChild(1).GetComponent<TMP_Text>().text = friendInfo.userTitle + " · " + friendInfo.userJob;
                newFriendProfile.transform.GetChild(2).GetComponent<TMP_Text>().text = friendInfo.userName;
                newFriendProfile.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(SetDialogEditFriend);
                newFriendProfile.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(OpenIdCard);
            }
        }
        else {
            friendsPage.transform.GetChild(2).gameObject.SetActive(true); }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    public void CloseFriendsPage()
    {
        UIController.instance.curOpenPageNum = -1;
        friendsPage.SetActive(false);
    }
    #endregion

    #region 친구 이름 검색하기
    //검색 페이지 오픈
    public void OpenSearchPage()
    {
        UIController.instance.curOpenPageNum = 6;
        friendSearchPage.SetActive(true);
        SearchEmptyArea.SetActive(false);
        //검색바 리셋
        searchText.text = "";
        //검색 결과 리셋
        searchedFriendProfile.SetActive(false);
    }
    //검색바 - 검색 버튼 클릭
    public void SearchButton()
    {
        //검색 실행
        SearchUserName();

        //x버튼 활성화, 검색 버튼 비활성화
        SearchButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
        SearchButtonGroup.transform.GetChild(1).gameObject.SetActive(false);
    }
    //검색바 - x버튼 클릭
    public void SearchDeleteButton()
    {
        searchText.text = "";
        //x버튼 비활성화, 검색 버튼 활성화
        SearchButtonGroup.transform.GetChild(0).gameObject.SetActive(false);
        SearchButtonGroup.transform.GetChild(1).gameObject.SetActive(true);
        SearchEmptyArea.SetActive(false);
    }
    //검색바 - 검색어 입력 도중 상태 체크
    public void CheckInputFieldOnChanged()
    {
        if (!string.IsNullOrWhiteSpace(searchText.text))
        {
            //x버튼 활성화
            SearchButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
            SearchButtonGroup.transform.GetChild(1).gameObject.SetActive(true);
        }
        else { SearchButtonGroup.transform.GetChild(0).gameObject.SetActive(false); }
    }
    //검색바 - 친구 이름으로 검색하기
    string searchedName;
    private async void SearchUserName()
    {
        searchedName = searchText.text;
        if (userNameList.Contains(searchedName) && searchedName!=UserManager.Instance.newUserInformation.userName)
        {
            SearchEmptyArea.SetActive(false);
            DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
            try
            {
                string searchedId = await GetUserId(searchedName);
                UserDB userDB = await GetUserDB(searchedId);

                SetSearchedUserUI(userDB); //받아오면 UI 출력하기
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); } //예외처리
        }
        else {
            //검색 결과 없을 때
            searchedFriendProfile.SetActive(false);
            SearchEmptyArea.SetActive(true);
        }
    }
    //UI처리
    private void SetSearchedUserUI(UserDB userDB)
    {
        searchedFriendProfile.SetActive(true);
        btnSendApplyFriend.GetComponent<Button>().interactable = true;
        btnSendApplyFriend.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
        btnSendApplyFriend.transform.GetChild(1).gameObject.SetActive(false);

        //이미 요청 보낸 유저면 취소 버튼 뜨게
        bool alreadyRequest = false;

        for(int i=0; i<userDB.notiApplyFriendList.Count; i++)
        {
            if (userDB.notiApplyFriendList[i].userId == thisUserId)
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
            if(key == thisUserId) { alreadyFriend = true; }
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
    public async void FriendAddOrCancel()
    {
        TMP_Text btnText = btnSendApplyFriend.transform.GetChild(0).GetComponent<TMP_Text>();
        print(btnText.text);
        if(btnText.text == "추가")
        {
            string searchedId = await GetUserId(searchedName);
            CheckAlreadyFriend(searchedId, btnText);
        }
        else
        {
            CancelRequestFriend();
            btnText.text = "추가";
        }
    }
    //이미 나한테 신청 보낸 친구일 경우 바로 추가
    bool isFromNoti = true;
    bool isAlreadyFriend = false;
    private void CheckAlreadyFriend(string searchedId, TMP_Text btnText)
    {
        isAlreadyFriend = false;
        if (thisUserDB.notiApplyFriendList.Count > 0)
        {
            for(int i=0; i< thisUserDB.notiApplyFriendList.Count; i++)
            {
                NotiInfo info = thisUserDB.notiApplyFriendList[i];
                if(info.userId == searchedId)
                {
                    isAlreadyFriend = true;
                    thisUserDB.notiApplyFriendList.RemoveAt(i);

                    isFromNoti = false;
                    friendId = info.userId;
                    UpdateFriendsList();
                }
            }
        }

        if (isAlreadyFriend)
        {
            btnSendApplyFriend.GetComponent<Button>().interactable = false;
            btnText.color = gray400;
            btnSendApplyFriend.transform.GetChild(1).gameObject.SetActive(true);

            StartCoroutine(SetSnackBar("님과 입사동기가 되었어요!", 70));
        }
        else
        {
            SendRequestFriend(searchedId);
            btnText.text = "취소";
        }
    }
    //추가 버튼
    private async void SendRequestFriend(string searchedId)
    {
        //snackBar 띄우기
        StartCoroutine(SetSnackBar("님에게 입사동기를 신청했어요!\n상대가 수락하면 랭킹에서 볼 수 있어요.", 98));

        //검색된 유저의 DB 받아오기
        
        UserDB userDB = await GetUserDB(searchedId);

        //현재 유저 정보 넣기
        NotiInfo newRequestFriendInfo = new NotiInfo();
        newRequestFriendInfo.userId = thisUserId;
        newRequestFriendInfo.date = DateTime.Now;
        //newRequestFriendInfo.userInformation = thisUserDB.userInformation;

        userDB.notiApplyFriendList.Add(newRequestFriendInfo);
        userDB.isNewNotiApplyFriend = true;

        //다시 검색된 유저 DB 업데이트 하기
        UpdateUserDB(searchedId, userDB);
    }
    IEnumerator SetSnackBar(string text, int height)
    {
        snackBar.GetComponent<RectTransform>().sizeDelta = new Vector2(350, height);
        snackBar.transform.GetChild(0).GetComponent<TMP_Text>().text =
            searchedName + text;

        snackBar.SetActive(true);
        yield return new WaitForSeconds(3f);
        snackBar.SetActive(false);
    }
    //취소 버튼
    private async void CancelRequestFriend()
    {
        //검색된 유저의 DB 받아오기
        string searchedId = await GetUserId(searchedName);
        UserDB userDB = await GetUserDB(searchedId);

        //현재 유저 정보 빼기
        for(int i=0; i<userDB.notiApplyFriendList.Count; i++)
        {
            if (userDB.notiApplyFriendList[i].userId == thisUserId)
            { userDB.notiApplyFriendList.RemoveAt(i); }
        }
        userDB.isNewNotiApplyFriend = false;

        //다시 검색된 유저 DB 업데이트 하기
        UpdateUserDB(searchedId, userDB);
    }
    //검색에서 백버튼
    public void BackFromApplyFriend()
    {
        UIController.instance.curOpenPageNum = 4;
        if (isAlreadyFriend)
        {
            OpenFriendsPage();
        }
        else { friendSearchPage.SetActive(false); }
    }
    #endregion

    #region 알림 관련
    //알림 페이지 오픈
    public async void OpenNotificationPage()
    {
        UIController.instance.curOpenPageNum = 3;
        notificationPage.SetActive(true);
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작

        thisUserDB = await GetUserDB(thisUserId);
        thisUserDB.isNewNotiApplyFriend = false;
        thisUserDB.isNewNotiCheerUp = false;

        CheckNoti("받은응원", thisUserDB.notiCheerUpList, content_noti_cheerUp, prefabs_noti_cheerUp, newNotiCheerUp);
        CheckNoti("입사동기신청", thisUserDB.notiApplyFriendList, content_noti_applyFriend, prefabs_noti_applyFriend, newNotiApplyFriend);
        UpdateUserDB(thisUserId, thisUserDB);
    }
    //알림 페이지 닫기
    public void CloseNotificationPage()
    {
        notificationPage.SetActive(false);
        home_icon_notification.sprite = selector_icon_notification[0];
        UIController.instance.curOpenPageNum = -1;
    }
    //알림 체크
    private async void CheckNoti(string whichNoti, List<NotiInfo> notiList, GameObject content, GameObject prefab, GameObject newObj)
    {
        //기존 리스트 초기화
        for (int i = 2; i < content.transform.childCount; i++)
        { Destroy(content.transform.GetChild(i).gameObject); }
        //알림 있는지 체크
        if(notiList.Count > 0)
        {
            content.transform.GetChild(0).gameObject.SetActive(false); //empty area 비활성화
            content.transform.GetChild(1).gameObject.SetActive(true); //description 활성화

            foreach(NotiInfo newNotiInfo in notiList)
            {
                //날짜 체크
                TimeSpan howManyDays = DateTime.Now - newNotiInfo.date;
                int howManyDaysInt = howManyDays.Days;
                string requestDate;
                if (howManyDaysInt == 0) { requestDate = "오늘"; }
                else if (howManyDaysInt == 1) { requestDate = "어제"; }
                else if (howManyDaysInt <= 60) { requestDate = newNotiInfo.date.ToString("MM월 dd일"); }
                else
                {   //60일 지나면 리스트에서 삭제하고 다음으로 넘기기
                    notiList.Remove(newNotiInfo); continue;
                }

                //instance 생성
                newObj = Instantiate(prefab, content.transform);
                //첫 확인이면 파란색 표시
                if (newNotiInfo.isFirstCheck)
                {
                    newObj.GetComponent<Image>().color = primary1;
                    newNotiInfo.isFirstCheck = false;
                }

                GameObject userInfoObj;
                if(whichNoti == "입사동기신청")
                {
                    userInfoObj = newObj.transform.GetChild(0).gameObject;
                    //버튼에 함수 입력
                    newObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(UpdateFriendsList);
                    newObj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(DeleteRequestFriend);
                }
                else {
                    userInfoObj = newObj;
                    //버튼에 함수 입력
                    newObj.GetComponent<Button>().onClick.AddListener(MoveToIdCardBack);
                }
                //기본 정보 입력
                UserDefaultInformation newNotiUserInfo = await GetUserInformationAsync(newNotiInfo.userId);
                userInfoObj.transform.GetChild(0).GetComponent<Image>().sprite = array_profileImg[newNotiUserInfo.userProfileImg];
                userInfoObj.transform.GetChild(1).GetComponent<TMP_Text>().text = newNotiUserInfo.userTitle + " · " + newNotiUserInfo.userJob;
                userInfoObj.transform.GetChild(2).GetComponent<TMP_Text>().text = newNotiUserInfo.userName;
                userInfoObj.transform.GetChild(3).GetComponent<TMP_Text>().text = requestDate;
            }
            UpdateUserDB(thisUserId, thisUserDB);
        }
        else
        {
            content.transform.GetChild(0).gameObject.SetActive(true); //empty area 활성화
            content.transform.GetChild(1).gameObject.SetActive(false); //description 비활성화
        }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    #region 알림 - 입사동기 신청
    //알림 - 입사동기 신청 - 수락 시 친구 리스트 업데이트
    private async void UpdateFriendsList()
    {
        if (isFromNoti)
        {
            GameObject thisObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
            string userName = thisObj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text;

            //현재 유저의 DB에 친구 리스트 추가
            int thisObjIndex = thisObj.transform.GetSiblingIndex() - 2;
            friendId = await GetUserId(userName);
            print("userName: " + userName);
            print("friendId: " + friendId);

            //기존 request list에서 해당 유저 정보 삭제
            thisUserDB.notiApplyFriendList.RemoveAt(thisObjIndex);

            Destroy(thisObj); //알람 삭제

            CheckNotiEmpty(thisUserDB.notiApplyFriendList, content_noti_applyFriend);
        }
        isFromNoti = true;

        dateTimeClass dateTime = new dateTimeClass();
        thisUserDB.friendsDictionary.Add(friendId, dateTime);

        //현재 유저 DB 업데이트
        UpdateUserDB(thisUserId, thisUserDB);

        //요청한 유저의 DB에 현재 유저 정보 추가
        UserDB friendDB = await GetUserDB(friendId);
        dateTimeClass dateTimefriend = new dateTimeClass();
        
        friendDB.friendsDictionary.Add(thisUserId, dateTimefriend);
        UpdateUserDB(friendId, friendDB);
    }
    //알림 - 입사동기 신청 - 거절 시 친구 요청 리스트에서 삭제
    private async void DeleteRequestFriend()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string userName = thisObj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text;

        //기존 request list에서 해당 유저 정보 삭제
        int thisObjIndex = thisObj.transform.GetSiblingIndex()-2;
        string friendId = await GetUserId(userName);
        thisUserDB.notiApplyFriendList.RemoveAt(thisObjIndex);

        //현재 유저 DB 업데이트
        UpdateUserDB(thisUserId, thisUserDB);

        Destroy(thisObj); //알람 삭제
        CheckNotiEmpty(thisUserDB.notiApplyFriendList, content_noti_applyFriend);
    }
    #endregion
    //알림 비워져 있으면 empty area 띄우기
    private void CheckNotiEmpty(List<NotiInfo> notiList,GameObject content)
    {
        if (notiList.Count == 0)
        {
            content.transform.GetChild(0).gameObject.SetActive(true);
            content.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    //알림 - 받은응원 - 리스트 클릭 시 해당 유저 사원증 뒷면으로 이동
    private async void MoveToIdCardBack()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject;
        string thisFriendName = thisObj.transform.GetChild(2).GetComponent<TMP_Text>().text;

        //알림페이지 닫기
        notificationPage.SetActive(false);
        //입사동기 페이지 열기
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        OpenFriendsPage();
        //친구 데이터 가져오기
        friendId = await GetUserId(thisFriendName);
        friendDB = await GetUserDB(friendId);

        idcard_front.transform.parent.gameObject.SetActive(true);
        SetIdcardFront(friendDB);
        SetIdcardBack(friendDB);
        //idcard_front.SetActive(false);
        TurnIdCardFrontToBack();
        print("move to card back : done");
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
        friendsPage.transform.GetChild(5).gameObject.SetActive(false);
        //리스트에 - 버튼 비활성화
        for (int i = 0; i < content_friendsList.transform.childCount; i++)
        {
            content_friendsList.transform.GetChild(i).GetChild(3).gameObject.SetActive(false);
            content_friendsList.transform.GetChild(i).GetChild(4).gameObject.SetActive(true);
        }
        //모든 친구가 삭제되었으면 empty area 활성화
        if (content_friendsList.transform.childCount == 0)
        {
            friendsPage.transform.GetChild(2).gameObject.SetActive(true);
        }
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
        string selectedUserId = await GetUserId(selectedUserName);
        thisUserDB.friendsDictionary.Remove(selectedUserId);

        Destroy(selectedUserProfileObj);
        dialog_edit_friend_dialog.transform.parent.gameObject.SetActive(false);

        UpdateUserDB(thisUserId, thisUserDB);
        print("this user friend dictionary remove done");

        //상대 유저 친구 리스트에서 삭제
        UserDB friendDB = await GetUserDB(selectedUserId);
        friendDB.friendsDictionary.Remove(thisUserId);
        UpdateUserDB(selectedUserId, friendDB);
        print("friend user friend dictionary remove done");
    }
    #endregion

    #region 입사동기 사원증
    //사원증 오픈
    string friendId;
    UserDB friendDB;
    private async void OpenIdCard()
    {
        string friendName = EventSystem.current.currentSelectedGameObject.transform.parent.GetChild(2).GetComponent<TMP_Text>().text;
        idcard_front.transform.parent.gameObject.SetActive(true);

        //온보딩 체크
        if (!UserManager.Instance.newUserInformation.idcard_onboarding)
        { idcard_front.transform.parent.GetChild(2).gameObject.SetActive(true);
            UserManager.Instance.newUserInformation.idcard_onboarding = true;
        }

        //친구 데이터 가져오기
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        friendId = await GetUserId(friendName);
        friendDB = await GetUserDB(friendId);

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
        idcard_front.transform.GetChild(4).GetComponent<TMP_Text>().color = companyColor;

        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = true;
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }
    //뒷면 데이터 세팅
    private void SetIdcardBack(UserDB friendDB)
    {
        //이름
        idcard_back.transform.GetChild(0).GetComponent<TMP_Text>().text = friendDB.userInformation.userName+ "님의 활동 내역";
        //기록 개수
        idcard_back.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = friendDB.totalFolderCount.ToString();
        idcard_back.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = friendDB.contestFolderCount.ToString();
        idcard_back.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>().text = friendDB.projectFolderCount.ToString();
        idcard_back.transform.GetChild(5).GetChild(1).GetComponent<TMP_Text>().text = friendDB.internshipFolderCount.ToString();

        //top3 경험
        SetTopThree(friendDB.topThreeExperiences, idcard_back.transform.GetChild(7).GetChild(0).gameObject);
        //top3 역량
        SetTopThree(friendDB.topThreeCapabilities, idcard_back.transform.GetChild(7).GetChild(2).gameObject);

        //응원하기 버튼 활성화 여부
        TimeSpan howManyDays = DateTime.Now - thisUserDB.friendsDictionary[friendId].sendCheerUpDate;
        int howManyDaysInt = howManyDays.Days;
        if (howManyDaysInt == 0)
        {
            print("cheerUp done");
            idcard_cheerUp.interactable = false;
        }
        else { idcard_cheerUp.interactable = true; print("cheerUp yet"); }
    }
    //Top3 비었는지 체크 후 UI 세팅
    private void SetTopThree(string[] topThreeArray, GameObject topThreeContainer)
    {
        int isEmptyExCount = 0;
        foreach (string value in topThreeArray)
        {
            if (string.IsNullOrEmpty(value) || value == "-") { isEmptyExCount++; }
        }
        if (isEmptyExCount >= 3)
        {
            topThreeContainer.transform.GetChild(3).gameObject.SetActive(true);
        }
        else
        {
            topThreeContainer.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = topThreeArray[0];
            topThreeContainer.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = topThreeArray[1];
            topThreeContainer.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = topThreeArray[2];
            topThreeContainer.transform.GetChild(3).gameObject.SetActive(false);
        }
    }
    //앞면 클릭 시 뒷면으로 돌아가기
    public void TurnIdCardFrontToBack()
    {
        idcard_front.transform.parent.GetChild(2).gameObject.SetActive(false);
        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = false; //중복 터치 방지
        friendIdcardAni.SetTrigger("frontToBack");
        idcard_back.transform.GetChild(10).GetComponent<Button>().interactable = true;
    }
    //뒷면 클릭 시 앞면으로 돌아가기
    public void TurnIdCardBackToFront()
    {
        idcard_back.transform.GetChild(10).GetComponent<Button>().interactable = false; //중복 터치 방지
        friendIdcardAni.SetTrigger("backToFront");
        idcard_front.transform.GetChild(6).GetComponent<Button>().interactable = true;
    }
    //사원증 닫기버튼
    public void CloseIdCard()
    {
        idcard_front.transform.parent.gameObject.SetActive(false);
        idcard_back.SetActive(true);
        idcard_front.SetActive(true);
        idcard_front.transform.parent.GetChild(2).gameObject.SetActive(false);

        //사원증 앞면 리셋
        idcard_front.transform.GetChild(0).GetComponent<Image>().color = Color.white;
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = array_profileImg[0];
        idcard_front.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(3).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(4).GetComponent<TMP_Text>().text = "";
    }
    #endregion

    #region 입사동기 - 응원하기 - 보내기
    public void SendCheerUp()
    {
        thisUserDB.friendsDictionary[friendId].sendCheerUpDate = DateTime.Now;
        idcard_cheerUp.interactable = false; //응원하기 버튼 비활성화

        NotiInfo notiInfo = new NotiInfo();
        notiInfo.userId = thisUserId;
        notiInfo.date = DateTime.Now;
        //notiInfo.userInformation = thisUserDB.userInformation;

        friendDB.notiCheerUpList.Add(notiInfo);
        friendDB.isNewNotiCheerUp = true;

        friendDB.rankingData.countCheerUp++; //랭킹 - 응원수

        UpdateUserDB(friendId, friendDB);
        UpdateUserDB(thisUserId, thisUserDB);
    }
    #endregion

    #region 데이터 처리
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
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
        }
        return userDB;
    }

    //DB에 유저 data 저장하기
    private async void UpdateUserDB(string userId, UserDB userDB)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        string userDBstr = JsonConvert.SerializeObject(userDB);
        await dataReference.SetRawJsonValueAsync(userDBstr);
    }
    //유저 이름으로 유저 Id 가져오기
    private async Task<string> GetUserId(string userName)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        var taskResult = await dataReference.GetValueAsync();

        Dictionary<string, string> userIdList = JsonConvert.DeserializeObject<Dictionary<string, string>>(taskResult.GetRawJsonValue());
        string userId;
        if (userIdList.Keys.Contains(userName))
        {
            userId = userIdList[userName];
        }
        else { userId = "1234"; }
        return userId;
    }
    //DB에서 유저 기본 정보만 가져오기
    private async Task<UserDefaultInformation> GetUserInformationAsync(string userId)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId).Child("userInformation");
        var taskResult = await dataReference.GetValueAsync();
        UserDefaultInformation friendInfo = JsonConvert.DeserializeObject<UserDefaultInformation>(taskResult.GetRawJsonValue());
        return friendInfo;
    }
    #endregion
}
