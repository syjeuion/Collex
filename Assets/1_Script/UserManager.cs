using System.Collections;
using System.Collections.Generic;
using System;
//using System.IO;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
//using TMPro;



public class UserInformation
{
    //public string userEmail; //유저 Email
    public string userId; //유저 Id

    public bool agreementForApp; //약관동의 체크

    public int isItFirst; //폴더 생성이 처음
    public int isItFirstTargetTitle; //목표칭호 설정이 처음
    public int isItFirstTargetTitlePage; //목표칭호 페이지 처음인지
    public int isItFirstTitleCollection; //칭호컬렉션 처음인지
    public int isItFirstUserTitle; //대표칭호 처음인지
    public int folderPageCount; //폴더페이지 몇번 들어갔는지-툴팁

    public bool homeBanner; //홈 이용팁 배너 클릭 유무

    public string userName; //유저 이름
    public int kindOfJob; //직군
    public int detailJob; //직무
    public DateTime userSignUpDate; //가입 날짜

    public int userProfileImgNumber; //설정된 프로필 사진 뭔지
    //public string companyName; //목표 회사
    public int idCardColorNumber;

    public string userTitleModi; //대표칭호 수식
    public string userTitleNoun; //대표칭호 명사

    public string[] targetTitleModi = new string[5]; //목표칭호 수식 - 0:이름,1:설명,2:게이지,3:획득개수,4:타입
    public string[] targetTitleNoun = new string[5];

    public int[] titleCheck = new int[30]; //type별 획득 개수

    public int recordCountedMonth; //저장하고 있는 '이번달'이 언젠지
    public int recordCountThisMonth;//이번달 기록 개수
    public int recordCountThisWeek;//이번주 기록 개수
    public int recordCountLastWeek;//저번주 기록 개수

    public Dictionary<string, int> projectType = new Dictionary<string, int>() //폴더 타입
        { {"프로젝트",0 },{"인턴십",0 },{"공모전",0 } };

    public Dictionary<string, int> recordDayOfWeek= new Dictionary<string, int>() //기록 작성 요일
        { {"월",0},{"화",0},{"수",0},{"목",0},{"금",0},{"토",0},{"일",0} };
}

public class UserManager : Singleton<UserManager>
{
    public bool firstOpen = true; //처음 앱 실행 체크
    public bool checkFolderDelete = false; //폴더 삭제하고 홈으로 돌아왔을때 체크
    public bool checkHomeTargetTitle = false; //홈에서 목표칭호 획득했을 때
    public bool editProfileInHome = false; //홈에서 프로필 이미지 클릭했을 때
    public bool clickHomeBanner = false; //홈에서 이용팁 배너 클릭했을 때
    public int getTitle = 0; //칭호 조건 달성했을 때 체크하고 칭호획득 페이지에서 해제

    //경험칩
    public string[,] ExperienceChipList = new string[20, 17];

    //경험칩 저장
    const string ExURL = "https://docs.google.com/spreadsheets/d/1eeIYaIaWKZZdIQ_uJALBBgKEpr23Ak3Zfwe8Dv80NTI/export?format=tsv&gid=198678164&range=B3:R22";

    IEnumerator DownloadPlannerEx()
    {
        //경험 데이터 받아오기
        UnityWebRequest Exwww = UnityWebRequest.Get(ExURL);
        yield return Exwww.SendWebRequest();
        ExSetData(Exwww.downloadHandler.text);
    }
    void ExSetData(string tsv)
    {
        string[] row = tsv.Split('\n');
        int rowSize = row.Length;
        
        for (int i = 0; i < rowSize; i++) //칭호 개수
        {
            string[] column = row[i].Split('\t');
            int columnSize = column.Length;

            for (int ii = 0; ii < columnSize; ii++)
            {
                ExperienceChipList[i, ii] = column[ii];
            }
        }
    }

    //칭호 배열
    public string[,] modiTitleList = new string[27,4];
    public string[,] nounTitleList = new string[27,4];
    //const string[] labelArr = new string["커뮤니케이션능","",""]; //급한 문제는 아니지만 이렇게 처리하는게 더 좋음(오류 방지)

    //칭호 리스트 - 구글 스프레드 시트에서 가져옴
    const string ModificationURL = "https://docs.google.com/spreadsheets/d/1eeIYaIaWKZZdIQ_uJALBBgKEpr23Ak3Zfwe8Dv80NTI/export?format=tsv&range=B2:E28";
    const string NounURL = "https://docs.google.com/spreadsheets/d/1eeIYaIaWKZZdIQ_uJALBBgKEpr23Ak3Zfwe8Dv80NTI/export?format=tsv&range=H2:K28";

    //public string[] nowGetTitle = new string[3]; //현재 획득 칭호 - 칭호 획득 페이지
    public List<string[]> nowGetTitle = new List<string[]>(); //현재 획득 칭호
    public string selectedModi;
    public string selectedNoun;

    public UserInformation newUserInformation = new UserInformation();

    #region 기록 정보
    public Dictionary<string, string> folders = new Dictionary<string, string>(); //폴더저장소<제목,내용>
    
    public Dictionary<string, int> Allcapabilites = new Dictionary<string, int>() //전체 역량 수 - 리포트
        { {"커뮤니케이션능력",0},{"리더십",0},{"문제해결능력",0},{"통찰력",0},{"팀워크",0} };
    public Dictionary<string, int> AllExperiences = new Dictionary<string, int>(); //전체 경험 수 - 검색
    public Dictionary<string, int> AllHashtags = new Dictionary<string, int>(); //전체 해시태그 수 - 검색

    public List<string> bookmarks = new List<string>(); //북마크 제목 "폴더제목 \t 기록제목" 형태로 저장

    public string pushedButton; //선택된 폴더 제목 저장
    public string pushedRecord; //선택된 기록 제목 저장 //기록 수정하기

    //public GameObject recordDetailPage;
    #endregion

    //public System.DateTime calendarDate;

    private void Start()
    {
        StartCoroutine(DownloadTitles());
        StartCoroutine(DownloadPlannerEx());
    }

    #region 칭호 세팅
    string ModifData;
    string NounData;
    IEnumerator DownloadTitles()
    {

        //수식형 칭호 데이터 받아오기
        UnityWebRequest Modifwww = UnityWebRequest.Get(ModificationURL);
        yield return Modifwww.SendWebRequest();
        ModifData = Modifwww.downloadHandler.text;

        //명사형 칭호 데이터 받아오기
        UnityWebRequest Nounwww = UnityWebRequest.Get(NounURL);
        yield return Nounwww.SendWebRequest();
        NounData = Nounwww.downloadHandler.text;

        //칭호 리스트
        setData(ModifData, modiTitleList);
        setData(NounData, nounTitleList);
    }
    void setData(string tsv, string[,] titleList)
    {
        string[] row = tsv.Split('\n');
        int rowSize = row.Length;
        //int columnSize = row[0].Split('\t').Length;

        for (int i = 0; i < rowSize; i++) //칭호 개수
        {
            string[] column = row[i].Split('\t');
            titleList[i, 0] = column[0];
            titleList[i, 1] = column[1];
            titleList[i, 2] = column[2];
            titleList[i, 3] = column[3];
        }
    }
    #endregion

    //칭호 획득했는지 체크하고 획득시 획득 페이지 활성화
    public void checkTitle(string orderCheck, int index, int type) //앞/뒤, 리스트순서, type
    {
        string[,] titleList; //칭호 테이블 - index:number
        if (orderCheck == "앞") titleList = modiTitleList;
        else titleList = nounTitleList;

        if (int.Parse(titleList[index, 2]) == newUserInformation.titleCheck[type])
        {
            nowGetTitle.Add(new string[3] { orderCheck, titleList[index, 0], titleList[index, 1] }); //앞/뒤,칭호,설명
        }
    }


    private void Update()
    {
        //if (Input.GetKey(KeyCode.Q)) Reset();

        /*if (Input.GetKey(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "2_Writing") return;
            Application.Quit();
        }*/
    }


    public void goHome() { SceneManager.LoadScene("1_Home"); }

    #region 앱 종료/데이터 저장/리셋
    //앱이 종료될 때
    private void OnApplicationQuit()
    {
        saveFoldersData();
    }
    //bool isFocus = false;
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            saveFoldersData();
            //isFocus = true;
        }
        /*else
        {
            if (isFocus) { isFocus = false; Onboarding.ActionPlayerPrefs(); }
        }*/
    }

    //데이터 저장
    void saveFoldersData()
    {
        string newUserInformationData = JsonConvert.SerializeObject(newUserInformation);
        PlayerPrefs.SetString("UserInformation", newUserInformationData);

        //폴더
        string folderJsonData = JsonConvert.SerializeObject(folders);
        PlayerPrefs.SetString("FolderData", folderJsonData);
        //역량
        string capabilitesData = JsonConvert.SerializeObject(Allcapabilites);
        PlayerPrefs.SetString("CapabilitesData", capabilitesData);
        //경험
        string experiencesData = JsonConvert.SerializeObject(AllExperiences);
        PlayerPrefs.SetString("ExperiencesData", experiencesData);
        //해시태그
        string hashtagData = JsonConvert.SerializeObject(AllHashtags);
        PlayerPrefs.SetString("HashtagData", hashtagData);
        //북마크
        string bookmarkData = JsonConvert.SerializeObject(bookmarks);
        PlayerPrefs.SetString("BookmarkData", bookmarkData);
    }
    #endregion
}
