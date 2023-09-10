using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
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

    //현재 유저 id, DB
    string thisUserId;
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
        myRankingArea.transform.GetChild(4).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName;
    }
    //친구 랭킹 UI 세팅
    private async void setFriendRanking()
    {
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        thisUserDB = await GetUserDB(thisUserId);

        List<string> friendIdList = new List<string>();
        if (thisUserDB.friendsDictionary.Count <= 0) {
            print("친구없음");
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
            return;  } //친구 없으면 종료
        print("친구있음");
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
        print("출력 완료");
        
        //파이어베이스 업데이트
        UpdateUserDB(thisUserId, thisUserDB);
    }
    //친구 랭킹 - 순위 계산
    int index = 2; //직전 랭킹과 같을 때 해당 인덱스
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
            if ( newCount == beforeCount) { newRanking = beforeRanking; }
            else { index = i + 2; }
            beforeRanking = newRanking;
            beforeCount = newCount;
            
            newSortedRanking.Add(id, newRanking);
            print(whichRanking + " - " + id+" : " + i +", index: "+index);

            //해당 유저 정보 가져오기
            UserDefaultInformation friendInfo = await GetUserInformation(id);

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
            if (originalRanking.ContainsKey(id))
            {
                int gap = newRanking - originalRanking[id];

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

            SetFriendRankingUI(content, myRanking, id,
                newRanking.ToString(), gapStr, changeStrColor, friendInfo, icon, newCount.ToString());
        }
        
        if (whichRanking == "record")
        {
            thisUserDB.rankingData.RankingRecord = newSortedRanking;
        }
        else { thisUserDB.rankingData.RankingCheerUp = newSortedRanking; }
        print(whichRanking + " - 실행 완료");
    }
    //친구 랭킹 - UI 출력
    private void SetFriendRankingUI(GameObject content,GameObject myRanking,string id,
        string ranking, string gapStr, Color color, UserDefaultInformation friendInfo,Sprite icon, string count)
    {
        //좌측
        newFriendRanking = Instantiate(prefab_friendRanking, content.transform);
        print("UI출력 - " + id +" : "+ ranking);
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

        //이번 순위가 내 순위라면
        if (id == thisUserId)
        {
            myRanking.transform.GetChild(0).GetComponent<TMP_Text>().text = ranking;
            myRanking.transform.GetChild(1).GetComponent<TMP_Text>().text = gapStr;
            myRanking.transform.GetChild(1).GetComponent<TMP_Text>().color = color;
            myRanking.transform.GetChild(6).GetComponent<TMP_Text>().text = count;

            //내 순위가 동순위일때 젤 위에 뜨게
            newFriendRanking.transform.SetSiblingIndex(index);
            print("my index: " + index);
        }
    }

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
