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

    private void Update()
    {
        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) goHome();
    }

    
    private void Start()
    {
        //랭킹 - 기록순
        setMyRankingRecord();

        //랭킹 - 응원순
        setMyRankingCheerUp();
    }

    //랭킹 - 기록순
    private void setMyRankingRecord()
    {
        recordMyRanking.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];
        recordMyRanking.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        recordMyRanking.transform.GetChild(4).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName;
    }
    //랭킹 - 응원순
    private void setMyRankingCheerUp()
    {
        cheerupMyRanking.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];
        cheerupMyRanking.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        cheerupMyRanking.transform.GetChild(4).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName;
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
    #endregion

    //바텀 네비게이션
    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goMypage() { SceneManager.LoadScene("6_Mypage"); }
}
