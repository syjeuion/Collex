using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Firebase.Database;

public class RankingManager : MonoBehaviour
{
    //기록
    public GameObject recordMyRanking; //나의 랭킹
    public GameObject content_record;
    //응원
    public GameObject cheerupMyRanking; //나의 랭킹
    public GameObject content_cheerUp;

    //프리팹
    public GameObject prefab_friendRanking;
    GameObject newFriendRanking;

    //기록/응원 아이콘
    public Sprite[] rankingIcons; //0:기록, 1:응원
    //프로필 이미지
    public Sprite[] myProfileImgs;

    //사원증
    public GameObject idcard_front;
    public GameObject idcard_back;
    public Animator friendIdcardAni;
    public Button idcard_cheerUp;
    //사원증 컬러
    string[,] colorList = new string[5, 2] {
        { "#FFDC00", "#D79044" }, //노랑:0
        { "#FE944D", "#FE944D" }, //오렌지:1
        { "#06C755", "#06C755" }, //초록:2
        { "#2AC1BC", "#2AC1BC" }, //민트:3
        { "#408BFD", "#408BFD" } }; //블루:4

    //현재 유저 id, DB
    string thisUserId;
    string thisUserName;
    UserDB thisUserDB;

    //Color
    Color primary3;
    Color red;
    Color gray700;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#FF3E49", out red);
        ColorUtility.TryParseHtmlString("#575F6B", out gray700);
    }

    private void Update()
    {
        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) goHome();
    }

    private void Start()
    {
        //현재 유저 id, DB
        thisUserId = UserManager.Instance.newUserInformation.userId;
        thisUserName = UserManager.Instance.newUserInformation.userName;
        setFriendRanking();

        ////내 랭킹 UI 세팅
        setMyRanking(recordMyRanking);
        setMyRanking(cheerupMyRanking);

    }
    //내 랭킹 UI 세팅
    private void setMyRanking(GameObject myRankingArea)
    {
        myRankingArea.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];
        myRankingArea.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        myRankingArea.transform.GetChild(4).GetComponent<TMP_Text>().text = thisUserName;   
    }
    private void MyRankingWithoutFriend(GameObject myRankingArea, int count)
    {
        myRankingArea.transform.GetChild(0).GetComponent<TMP_Text>().text = "1";
        myRankingArea.transform.GetChild(6).GetComponent<TMP_Text>().text = count.ToString();
    }

    #region 친구 랭킹 UI 세팅
    //친구 랭킹 UI 세팅
    private async void setFriendRanking()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserId);

        List<string> friendIdList = new List<string>();

        //친구 없으면 종료
        if (thisUserDB.friendsDictionary.Count <= 0)
        {
            //empty area 활성화
            content_record.transform.GetChild(1).gameObject.SetActive(true);
            content_cheerUp.transform.GetChild(1).gameObject.SetActive(true);

            MyRankingWithoutFriend(recordMyRanking, thisUserDB.rankingData.countRecord);
            MyRankingWithoutFriend(cheerupMyRanking, thisUserDB.rankingData.countCheerUp);

            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
            return;
        }
        
        //empty area 제거
        content_record.transform.GetChild(1).gameObject.SetActive(false);
        content_cheerUp.transform.GetChild(1).gameObject.SetActive(false);

        foreach (string id in thisUserDB.friendsDictionary.Keys)
        {
            friendIdList.Add(id);
        }

        await celculateRanking("record", friendIdList);
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        await celculateRanking("cheerUp", friendIdList);
        
        //파이어베이스 업데이트
        UpdateUserDB(thisUserId, thisUserDB);
    }
    //친구 랭킹 - 순위 계산
    int index = 2; //직전 랭킹과 같을 때 해당 인덱스
    bool isRankingSame;
    private async Task celculateRanking(string whichRanking, List<string> friendIdList)
    {
        index = 2;
        //현재 친구 대상으로 count 값 가져와서 디셔너리에 저장
        Dictionary<string, int> newRankingDic = new Dictionary<string, int>();
        for (int i = 0; i < friendIdList.Count; i++)
        {
            string id = friendIdList[i];
            int count = await GetUserRankingCount(id, whichRanking);
            newRankingDic.Add(id, count);
        }
        //내 순위도 dictionary에 저장
        if(whichRanking == "record")
            { newRankingDic.Add(thisUserId, thisUserDB.rankingData.countRecord); }
        else { newRankingDic.Add(thisUserId, thisUserDB.rankingData.countCheerUp); }

        //내림차순으로 딕셔너리 정렬
        Dictionary<string, int> sortedRankingDic = newRankingDic.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        List<string> sortedIds = new List<string>();
        foreach (string key in sortedRankingDic.Keys) sortedIds.Add(key);

        //순서대로 UI 출력
        int beforeRanking=1; //직전 랭킹
        int beforeCount=0; //직전 count
        
        Dictionary<string, int> newSortedRanking = new Dictionary<string, int>();
        for (int i = 0; i < sortedIds.Count; i++)
        {
            string id = sortedIds[i];
            int newRanking = i + 1;
            int newCount = sortedRankingDic[id];

            //동순위 처리
            if (i == 0) { beforeCount = newCount; }
            if (newCount == beforeCount) { newRanking = beforeRanking; }
            else { index = i + 2; }
            beforeRanking = newRanking;
            beforeCount = newCount;

            newSortedRanking.Add(id, newRanking);
        }
        //기존 순위와 동일한지 체크
        if (whichRanking == "record")
        {
            isRankingSame = true;
            for (int i=0; i<newSortedRanking.Count; i++)
            {
                string id = sortedIds[i];
                if(thisUserDB.rankingData.RankingRecord.ContainsKey(id)
                    && thisUserDB.rankingData.RankingRecord[id] != newSortedRanking[id])
                { isRankingSame = false; break; }
            }
        }
        else {
            isRankingSame = true;
            for (int i = 0; i < newSortedRanking.Count; i++)
            {
                string id = sortedIds[i];
                if (thisUserDB.rankingData.RankingCheerUp.ContainsKey(id)
                    && thisUserDB.rankingData.RankingCheerUp[id] != newSortedRanking[id])
                { isRankingSame = false; break; }
            }
        }
        print(whichRanking+" - isRankingSame: " + isRankingSame);

        for (int i = 0; i < sortedIds.Count; i++)
        {
            //해당 유저 정보 가져오기
            string userId = sortedIds[i];
            UserDefaultInformation friendInfo = await GetUserInformation(userId);

            

            //기록/응원 content 세팅
            GameObject myRanking;
            GameObject content;
            Dictionary<string, int> originalRanking = new Dictionary<string, int>();
            Sprite icon;
            if (whichRanking == "record")
            {
                content = content_record;
                originalRanking = thisUserDB.rankingData.RankingRecord;
                icon = rankingIcons[0];
                myRanking = recordMyRanking;
            }
            else
            {
                content = content_cheerUp;
                originalRanking = thisUserDB.rankingData.RankingCheerUp;
                icon = rankingIcons[1];
                myRanking = cheerupMyRanking;
            }

            Color changeStrColor;
            //기존 순위와 비교
            string gapStr;

            if (isRankingSame)
            {
                RankingGap rankingGapBefore = new RankingGap() ;
                if (whichRanking == "record"&& thisUserDB.rankingData.gapDic_record.ContainsKey(userId))
                {
                    rankingGapBefore = thisUserDB.rankingData.gapDic_record[userId];
                }
                else if(whichRanking == "cheerUp" && thisUserDB.rankingData.gapDic_cheerUp.ContainsKey(userId))
                {
                    rankingGapBefore = thisUserDB.rankingData.gapDic_cheerUp[userId];
                }
                else { rankingGapBefore.rankingStr = "-";
                    rankingGapBefore.rankingColor = "#"+ColorUtility.ToHtmlStringRGB(gray700);
                }
                gapStr = rankingGapBefore.rankingStr;
                ColorUtility.TryParseHtmlString(rankingGapBefore.rankingColor, out changeStrColor);
                //changeStrColor = rankingGapBefore.rankingColor;
            }
            else
            {
                if (originalRanking.ContainsKey(userId))
                {
                    int gap = newSortedRanking[userId] - originalRanking[userId];

                    if (gap < 0) //현재 순위가 더 낮음
                    {
                        gapStr = (gap * -1).ToString() + "↑";
                        changeStrColor = red;
                    }
                    else if (gap > 0) //현재 순위가 더 높음
                    {
                        gapStr = (gap).ToString() + "↓";
                        changeStrColor = primary3;
                    }
                    else //동일
                    {
                        gapStr = "-";
                        changeStrColor = gray700;
                    }
                }
                else
                {
                    gapStr = "-";
                    changeStrColor = gray700;
                }
            }

            SetFriendRankingUI(content, myRanking, userId,
                newSortedRanking[userId].ToString(), gapStr, changeStrColor, friendInfo, icon, sortedRankingDic[userId].ToString());

            //기존 갭 저장
            RankingGap rankingGap = new RankingGap();
            rankingGap.rankingStr = gapStr;
            rankingGap.rankingColor = "#" + ColorUtility.ToHtmlStringRGB(changeStrColor);
            if (whichRanking == "record")
            {
                if (thisUserDB.rankingData.gapDic_record.ContainsKey(userId))
                    { thisUserDB.rankingData.gapDic_record[userId] = rankingGap; }
                else { thisUserDB.rankingData.gapDic_record.Add(userId, rankingGap); }
            }
            else
            {
                if (thisUserDB.rankingData.gapDic_cheerUp.ContainsKey(userId))
                    { thisUserDB.rankingData.gapDic_cheerUp[userId] = rankingGap; }
                else { thisUserDB.rankingData.gapDic_cheerUp.Add(userId, rankingGap); }
            }
        }
        
        if (whichRanking == "record")
        {
            thisUserDB.rankingData.RankingRecord = newSortedRanking;
        }
        else { thisUserDB.rankingData.RankingCheerUp = newSortedRanking; }

    }
    //친구 랭킹 - UI 출력
    private void SetFriendRankingUI(GameObject content,GameObject myRanking,string id,
        string ranking, string gapStr, Color color, UserDefaultInformation friendInfo,Sprite icon, string count)
    {
        //좌측
        newFriendRanking = Instantiate(prefab_friendRanking, content.transform);
        newFriendRanking.transform.GetChild(0).GetComponent<TMP_Text>().text = ranking;
        newFriendRanking.transform.GetChild(1).GetComponent<TMP_Text>().text = gapStr;
        newFriendRanking.transform.GetChild(1).GetComponent<TMP_Text>().color = color;
        //유저 정보
        newFriendRanking.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[friendInfo.userProfileImg];
        newFriendRanking.transform.GetChild(3).GetComponent<TMP_Text>().text = friendInfo.userTitle;
        newFriendRanking.transform.GetChild(4).GetComponent<TMP_Text>().text = friendInfo.userName;
        //우측
        newFriendRanking.transform.GetChild(5).GetComponent<Image>().sprite = icon;
        newFriendRanking.transform.GetChild(6).GetComponent<TMP_Text>().text = count;
        //버튼
        newFriendRanking.GetComponent<Button>().onClick.AddListener(OpenIdCard);

        //이번 순위가 내 순위라면
        if (id == thisUserId)
        {
            myRanking.transform.GetChild(0).GetComponent<TMP_Text>().text = ranking;
            myRanking.transform.GetChild(1).GetComponent<TMP_Text>().text = gapStr;
            myRanking.transform.GetChild(1).GetComponent<TMP_Text>().color = color;
            myRanking.transform.GetChild(6).GetComponent<TMP_Text>().text = count;

            //내 순위가 동순위일때 젤 위에 뜨게
            newFriendRanking.transform.SetSiblingIndex(index);
        }
    }
    #endregion

    #region 사원증 세팅
    #region 입사동기 사원증
    //사원증 오픈
    string friendId;
    string friendName;
    UserDB friendDB;
    private async void OpenIdCard()
    {
        friendName = EventSystem.current.currentSelectedGameObject.transform.GetChild(4).GetComponent<TMP_Text>().text;
        idcard_front.transform.parent.gameObject.SetActive(true);

        //친구 데이터 가져오기
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작

        //온보딩 체크
        if (!UserManager.Instance.newUserInformation.idcard_onboarding)
        {
            idcard_front.transform.parent.GetChild(2).gameObject.SetActive(true);
            UserManager.Instance.newUserInformation.idcard_onboarding = true;
        }

        //현재 유저 랭킹 클릭
        if (friendName == thisUserName)
        {
            friendDB = thisUserDB;
            idcard_cheerUp.gameObject.SetActive(false);
        }
        else
        {
            friendId = await GetUserId(friendName);
            friendDB = await GetUserDB(friendId);
            idcard_cheerUp.gameObject.SetActive(true);
        }

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
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = myProfileImgs[friendDefaultInfo.userProfileImg];
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
        idcard_back.transform.GetChild(0).GetComponent<TMP_Text>().text = friendDB.userInformation.userName + "님의 활동 내역";
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
        if (friendName != thisUserName)
        {
            TimeSpan howManyDays = DateTime.Now - thisUserDB.friendsDictionary[friendId].sendCheerUpDate;
            int howManyDaysInt = howManyDays.Days;
            if (howManyDaysInt == 0)
            {
                idcard_cheerUp.interactable = false;
            }
            else { idcard_cheerUp.interactable = true; }
        }
        
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

        //사원증 앞면 리셋
        idcard_front.transform.GetChild(0).GetComponent<Image>().color = Color.white;
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = myProfileImgs[0];
        idcard_front.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(3).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(4).GetComponent<TMP_Text>().text = "";

        idcard_front.transform.parent.GetChild(2).gameObject.SetActive(false);
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
    #endregion

    #region 파이어베이스 realTimeDB
    //DB에서 유저 data 가져오기
    private async Task<UserDB> GetUserDB(string userId)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        var taskResult = await dataReference.GetValueAsync();
        UserDB userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
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
    //해당 유저 기록/응원 수 가져오기
    private async Task<int> GetUserRankingCount(string userId, string whichRanking)
    {
        int count;
        try
        {
            DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId).Child("rankingData");
            var taskResult = await dataReference.GetValueAsync();
            RankingData rankingData = JsonConvert.DeserializeObject<RankingData>(taskResult.GetRawJsonValue());

            if(whichRanking == "record") { count = rankingData.countRecord; }
            else { count = rankingData.countCheerUp; }
        }
        catch{
            count = -1; }
        return count;
    }
    //유저 정보 가져오기
    private async Task<UserDefaultInformation> GetUserInformation(string userId)
    {
        UserDefaultInformation userInfo = new UserDefaultInformation();
        try
        {
            DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId).Child("userInformation");
            var taskResult = await dataReference.GetValueAsync();
            userInfo = JsonConvert.DeserializeObject<UserDefaultInformation>(taskResult.GetRawJsonValue());
        }
        catch
        {
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
        }
        
        return userInfo;
    }
    #endregion

    //바텀 네비게이션
    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goMypage() { SceneManager.LoadScene("6_Mypage"); }
}
