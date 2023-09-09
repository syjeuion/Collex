using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;
using Firebase.Database;

public class Onboarding : MonoBehaviour
{
    //UserId
    string userId;
    //페이지
    public GameObject SignInPage;
    public GameObject QuitAlertPage;
    public GameObject onboardingUserPage;
    public GameObject onboardingGuidePage;
    public GameObject progressBar;
    public Image backButtonIcon;

    //약관동의
    public GameObject AgreementsPage;
    public GameObject agreeConfirmButton;
    public Sprite buttonDisabled;
    public Sprite buttonEnabled;

    //유저 페이지 오브젝트
    public GameObject getUserName;
    public GameObject inputUserName;
    public GameObject getUserJob;
    public GameObject getUserJobDetails;

    //가이드 페이지 오브젝트
    public TMP_Text guideMessage;
    public Image daJeongImg;
    public Sprite[] DaJeongFaces;

    //직군별 직무 리스트
    private string[,] JobList = new string[5, 4]
    {   {"서비스 기획","UX 기획","PM/PO","사업 기획"},
        { "UX/UI 디자인", "영상 디자인", "브랜드 디자인", "그래픽 디자인" },
        { "앱 개발", "웹 개발", "웹 퍼블리싱", "시스템 엔지니어" },
        { "서버 개발", "IT 클라우드", "기술지원", "QA 엔지니어" },
        { "데이터 분석", "데이터 엔지니어", "인공지능", "DBA" } };

    
    int countStep; //단계 체크
    string UserName; //유저 이름
    int UserJobIndex=7; //직군
    int UserDetailJobIndex=5; //직무

    //유저 이름 중복 방지 리스트 UserNameList
    List<string> userNameList = new List<string>();

    //
    bool isOnboarding;

    //사용 컬러
    Color primary1;
    Color primary3;
    Color errorColor;
    Color gray_200;
    Color gray_400;
    Color gray_500;
    Color gray_800;

    public static Action ActionPlayerPrefs;

    #region 앱 시작 세팅
    private void Awake()
    {
        TouchScreenKeyboard.hideInput=true;

        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);
        ColorUtility.TryParseHtmlString("#EBEDEF", out gray_200);
        ColorUtility.TryParseHtmlString("#B6BBC3", out gray_400);
        ColorUtility.TryParseHtmlString("#949CA8", out gray_500);
        ColorUtility.TryParseHtmlString("#3C4149", out gray_800);
        ActionPlayerPrefs = () => { GetPlayerPrefs(); };

        Screen.fullScreen = false;

        if (UserManager.Instance.firstOpen) {  GetPlayerPrefs(); }

    }
    private void Start()
    {
        StartCoroutine(waitSplash());
        
        //checkOnboarding();
    }
    
    //스플래시 애니메이션이 끝나기 전 홈으로 넘어가는 것 방지
    IEnumerator waitSplash()
    {
        yield return new WaitForSeconds(0.1f);
        yield return checkOnboarding();
        
        //yield return new WaitForSeconds(0.1f);
        ////yield return CheckResetDate(); //이달 첫 실행 유저인지 체크
        //yield return new WaitForSeconds(0.1f);
    }

    //bool homeCheck =false;
    private async Task checkOnboarding()
    {
        //로그인 체크
        userId = UserManager.Instance.newUserInformation.userId;
        print("userId: " + userId);
        
        //string userName = "";
        //Id가 있으면 
        if (!string.IsNullOrEmpty(userId))
        {
            SignInPage.SetActive(false);

            //Id가 DB에도 저장 되어있는지 확인 == 온보딩 했다는 뜻
            //bool isUserIdInDB = await GetUserIdlList(userId);
            List<string> userIdList = await getIdOrNameList("id");

            print("userIdList[0]: "+userIdList[0]);
            //print("isUserIdInDB: " + isUserIdInDB);

            if (userIdList.Contains(userId))
            {
                goHome();
                //isOnboarding = true;
            }
            else {
                //온보딩 시작 시 userDB 불러오기
                userNameList = await getIdOrNameList("name");
            }
        }
        else { SignInPage.SetActive(true); }

        //약관동의 체크
        if (UserManager.Instance.newUserInformation.agreementForApp) { AgreementsPage.SetActive(false); }
        else { AgreementsPage.SetActive(true); }

        CheckResetDate(); //이달 첫 실행 유저인지 체크
    }

    //PlayerPrefs 불러오는 함수
    public void GetPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("UserInformation"))
        {
            string newUserInformationData = PlayerPrefs.GetString("UserInformation");
            if (!string.IsNullOrWhiteSpace(newUserInformationData))
                UserManager.Instance.newUserInformation = JsonConvert.DeserializeObject<UserInformation>(newUserInformationData);
        }

        //기록 정보
        string folderJsonData = PlayerPrefs.GetString("FolderData", "");
        if (!string.IsNullOrWhiteSpace(folderJsonData))
            UserManager.Instance.folders = JsonConvert.DeserializeObject<Dictionary<string, string>>(folderJsonData);

        string capabilitesData = PlayerPrefs.GetString("CapabilitesData", "");
        if (!string.IsNullOrWhiteSpace(capabilitesData))
            UserManager.Instance.Allcapabilites = JsonConvert.DeserializeObject<Dictionary<string, int>>(capabilitesData);

        string experiencesData = PlayerPrefs.GetString("ExperiencesData", "");
        if (!string.IsNullOrWhiteSpace(experiencesData))
            UserManager.Instance.AllExperiences = JsonConvert.DeserializeObject<Dictionary<string, int>>(experiencesData);

        string hashtagData = PlayerPrefs.GetString("HashtagData", "");
        if (!string.IsNullOrWhiteSpace(hashtagData))
            UserManager.Instance.AllHashtags = JsonConvert.DeserializeObject<Dictionary<string, int>>(hashtagData);

        string bookmarkData = PlayerPrefs.GetString("BookmarkData", "");
        if (!string.IsNullOrWhiteSpace(bookmarkData))
            UserManager.Instance.bookmarks = JsonConvert.DeserializeObject<List<string>>(bookmarkData);
        UserManager.Instance.firstOpen = false;
    }
    #endregion


    #region 약관동의 관련
    //체크 클릭 시 버튼 활성화
    public void AgreeToggleCheck()
    {
        Toggle agreeToggle = EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();
        if (agreeToggle.isOn)
        {
            agreeConfirmButton.GetComponent<Button>().interactable = true;
            agreeConfirmButton.GetComponent<Image>().sprite = buttonEnabled;
            agreeConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = primary1;
        }
        else
        {
            agreeConfirmButton.GetComponent<Button>().interactable = false;
            agreeConfirmButton.GetComponent<Image>().sprite = buttonDisabled;
            agreeConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_400;
        }
    }
    //시작하기 버튼 클릭
    public void AgreeConfirmButton()
    {
        UserManager.Instance.newUserInformation.agreementForApp = true;
        AgreementsPage.SetActive(false);
    }
    //개인정보처리약관 웹페이지 이동
    public void AgreeGoWeb()
    {
        Application.OpenURL("https://sites.google.com/view/collex/%EA%B0%9C%EC%9D%B8%EC%A0%95%EB%B3%B4-%EC%B2%98%EB%A6%AC%EB%B0%A9%EC%B9%A8?authuser=0");
    }
    #endregion

    //디바이스 뒤로가기
    bool escapeCheck = true;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)&&escapeCheck)
        {
            escapeCheck = false;
            StartCoroutine(EscapeBackButton());
        }
    }
    IEnumerator EscapeBackButton()
    {
        if (countStep == 0) { QuitAlertPage.SetActive(true); }
        BackButton();
        yield return new WaitForSeconds(0.1f);
        escapeCheck = true;
    }

    #region 다음/이전
    public void OnClickPage()
    {
        if (countStep == 0) //첫인사>앱설명
        {
            daJeongImg.sprite = DaJeongFaces[0];
            guideMessage.text = "저희 <color=#408BFD>Collex</color>에서는 직무 관련 경험을 하면서 \n무엇을 했고, 배웠고, 느꼈는지를\n템플릿으로 기록할 수 있어요.";
            backButtonIcon.color = gray_800;
            //StartCoroutine(ChangeProgressBar(true, 40, 40*2));
            ChangeProgressBar(true, 40, 40 * 2);
            countStep =1;
        }
        else if (countStep == 1) //앱설명>이름묻기
        {
            guideMessage.text = "아! 혹시 우리 회사에서 \n사용하고 싶은 닉네임이 있나요?";
            //StartCoroutine(ChangeProgressBar(true, 40*2, 40*3));
            ChangeProgressBar(true, 40 * 2, 40 * 3);
            countStep =2;
        }
        else if (countStep == 2) //이름묻기>이름입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            getUserName.SetActive(true);
            getUserJob.SetActive(false);
            getUserJobDetails.SetActive(false);
            //StartCoroutine(ChangeProgressBar(true, 40*3, 40*4));ChangeProgressBar(true, 40*3, 40*4)
            ChangeProgressBar(true, 40 * 3, 40 * 4);
            countStep = 3;
        }
        else if(countStep==4) //직군묻기>직군입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            getUserName.SetActive(false);
            getUserJob.SetActive(true);
            getUserJobDetails.SetActive(false);
            //StartCoroutine(ChangeProgressBar(true, 40 * 5, 40 * 6));
            ChangeProgressBar(true, 40 * 5, 40 * 6);
            countStep = 5;
        }
        else if(countStep==7|| countStep == 8) //직무선택후반응>마무리
        {
            daJeongImg.sprite = DaJeongFaces[2];
            guideMessage.text = "앗, 갑자기 팀장님이 부르셔서...\n일단 활동 폴더 추가하고,\n기록을 작성해 보시겠어요?";
            //StartCoroutine(ChangeProgressBar(true, 40*8, 360));
            ChangeProgressBar(true, 40 * 8, 360);
            countStep = 9;
        }
        else //countStep==9 //마무리>홈
        {
            UserManager.Instance.newUserInformation.userName = UserName;
            UserManager.Instance.newUserInformation.kindOfJob = UserJobIndex;
            UserManager.Instance.newUserInformation.detailJob = UserDetailJobIndex;
            GetFirstTitles();
        }
    }
    public void BackButton()
    {
        if (countStep == 0) return;
        else if(countStep ==1) //앱설명>첫인사
        {
            daJeongImg.sprite = DaJeongFaces[1];
            guideMessage.text = "<color=#408BFD>Collex</color>에 입사한 걸 환영해요!\n저는 사수 한다정이라고 해요.\n잘 부탁해요.…";
            backButtonIcon.color = gray_400;
            //StartCoroutine(ChangeProgressBar(false, 40 * 2, 40));
            ChangeProgressBar(false, 40 * 2, 40);
            countStep = 0;
        }
        else if (countStep == 2) //이름묻기>앱설명
        {
            guideMessage.text = "저희 <color=#408BFD>Collex</color>에서는 직무 관련 경험을 하면서 \n무엇을 했고, 배웠고, 느꼈는지를\n템플릿으로 기록할 수 있어요.";
            //StartCoroutine(ChangeProgressBar(false, 40 * 3, 40 * 2));
            ChangeProgressBar(false, 40 * 3, 40 * 2);
            countStep = 1;
        }
        else if (countStep == 3) //이름입력>이름묻기
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "아! 혹시 우리 회사에서 \n사용하고 싶은 닉네임이 있나요?";
            //StartCoroutine(ChangeProgressBar(false, 40 * 4, 40 * 3));
            ChangeProgressBar(false, 40 * 4, 40 * 3);
            countStep = 2;
        }
        else if (countStep == 4) //직군묻기>이름입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            getUserName.SetActive(true);
            getUserJob.SetActive(false);
            getUserJobDetails.SetActive(false);
            //StartCoroutine(ChangeProgressBar(false, 40 * 5, 40 * 4));
            ChangeProgressBar(false, 40 * 5, 40 * 4);
            countStep = 3;
        }
        else if (countStep == 5) //직군입력>직군묻기
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "좋아요, " + UserName+ "님!\n저희 팀에서 어떤 업무를 담당할지 아시죠?";
            //StartCoroutine(ChangeProgressBar(false, 40 * 6, 40 * 5));
            ChangeProgressBar(false, 40 * 6, 40 * 5);
            daJeongImg.sprite = DaJeongFaces[0];
            countStep = 4;
        }
        else if (countStep == 6) //직무입력>직군입력
        {
            getUserJobDetails.SetActive(false);
            getUserJob.SetActive(true);
            //StartCoroutine(ChangeProgressBar(false, 40 * 7, 40 * 6));
            ChangeProgressBar(false, 40 * 7, 40 * 6);
            countStep = 5;
        }
        else if (countStep == 7 || countStep==8) //직무선택후반응>직무입력||직군입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            
            if (UserManager.Instance.newUserInformation.kindOfJob==5)
            {   countStep = 5;
                //StartCoroutine(ChangeProgressBar(false, 40 * 8, 40 * 6));
                ChangeProgressBar(false, 40 * 8, 40 * 6);
            }
            else {
                //countStep = 6; StartCoroutine(ChangeProgressBar(false, 40 * 8, 40 * 7));
                ChangeProgressBar(false, 40 * 8, 40 * 7);
            }
        }
        else //countStep ==9 //마무리>직무선택후반응
        {
            if (UserManager.Instance.newUserInformation.detailJob!=5)
            {   guideMessage.text = "맞아요!";
                daJeongImg.sprite = DaJeongFaces[1];
                countStep = 7;
            }
            else //직무 없음
            {   guideMessage.text = "아, 그랬나요?";
                countStep = 8;
                daJeongImg.sprite = DaJeongFaces[0];
            }
            //StartCoroutine(ChangeProgressBar(false, 360, 40 * 8));
            ChangeProgressBar(false, 360, 40 * 8);
        }
    }
    #endregion

    #region 유저 이름
    //이름 입력
    int inputLength;
    public void OnSelectInputName()
    {
        inputUserName.transform.GetChild(1).gameObject.SetActive(true);
        inputUserName.transform.GetChild(3).gameObject.SetActive(false);
        inputUserName.transform.GetChild(1).GetComponent<Image>().color = primary3;
        inputUserName.transform.GetChild(2).GetComponent<TMP_Text>().color = primary3;
    }
    public void OnValueChangedInputName()
    {
        inputLength = inputUserName.GetComponent<TMP_InputField>().text.Length;

        inputUserName.transform.GetChild(2).GetComponent<TMP_Text>().text =
            inputLength.ToString() + "/10";

        GameObject confirmButton = getUserName.transform.GetChild(2).gameObject;
        if (inputLength <= 0)
        {
            confirmButton.GetComponent<Button>().interactable = false;
            confirmButton.GetComponent<Image>().color = gray_200;
            confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_500;
        }
        else if (inputLength <= 10)
        {
            confirmButton.GetComponent<Button>().interactable = true;
            confirmButton.GetComponent<Image>().color = primary3;
            confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = primary1;
        }
        else
        {
            confirmButton.GetComponent<Button>().interactable = false;
            inputUserName.transform.GetChild(1).GetComponent<Image>().color = errorColor;
            inputUserName.transform.GetChild(2).GetComponent<TMP_Text>().color = errorColor;
        }


    }
    public void OnDeselectInputName()
    {
        inputUserName.transform.GetChild(1).gameObject.SetActive(false);
        inputUserName.transform.GetChild(2).GetComponent<TMP_Text>().color = gray_500;
    }
    //이름 저장
    private void SaveUserName()
    {
        onboardingGuidePage.SetActive(true);
        onboardingUserPage.SetActive(false);
        guideMessage.text = "좋아요, "+UserName+"님!\n저희 팀에서 어떤 업무를 담당할지 아시죠?";
        //StartCoroutine(ChangeProgressBar(true, 40*4, 40*5));
        ChangeProgressBar(true, 40 * 4, 40 * 5);
        countStep = 4;
    }

    
    //이름 중복 체크 후 이름 저장
    public void checkNameOverlap()
    {
        //getUserNameList();
        UserName = inputUserName.GetComponent<TMP_InputField>().text;
        if (userNameList.Contains(UserName))
        {
            inputUserName.transform.GetChild(1).gameObject.SetActive(true);
            inputUserName.transform.GetChild(1).GetComponent<Image>().color = errorColor;
            inputUserName.transform.GetChild(3).gameObject.SetActive(true);
        }
        else
        {
            SaveUserName(); //이름 저장
            
        }
    }
    #endregion

    #region 직군/직무 선택
    //직군 선택
    bool jobSameCheck = true;
    GameObject selectedJobObj;
    public void SaveUserJob()
    {
        if (selectedJobObj != null)
        { selectedJobObj.transform.GetChild(1).gameObject.SetActive(false); }

        selectedJobObj = EventSystem.current.currentSelectedGameObject;
        selectedJobObj.transform.GetChild(1).gameObject.SetActive(true);
        if (UserJobIndex == selectedJobObj.transform.GetSiblingIndex()) jobSameCheck = true;
        else jobSameCheck = false;
        UserJobIndex = selectedJobObj.transform.GetSiblingIndex();
        
        if (UserJobIndex == 5)
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "아, 그랬나요?";
            daJeongImg.sprite = DaJeongFaces[0];
            //StartCoroutine(ChangeProgressBar(true, 40 * 6, 40 * 8));
            ChangeProgressBar(true, 40 * 6, 40 * 8);
            countStep = 8;
        }
        else
        {
            getUserJob.SetActive(false);
            getUserJobDetails.SetActive(true);
            SetJobDetails();
            //StartCoroutine(ChangeProgressBar(true, 40 * 6, 40 * 7));
            ChangeProgressBar(true, 40 * 6, 40 * 7);
            countStep = 6;
        }
        
    }
    //직무 선택 페이지 세팅
    void SetJobDetails()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!jobSameCheck)
            {
                if (getUserJobDetails.transform.GetChild(i).GetChild(1).gameObject.activeSelf)
                { getUserJobDetails.transform.GetChild(i).GetChild(1).gameObject.SetActive(false); }
            }
            
            getUserJobDetails.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text
            = "<color=#408BFD>" + JobList[UserJobIndex, i]+"</color>";
            string lastStr = JobList[UserJobIndex, i].Substring(JobList[UserJobIndex, i].Length - 1, 1);
            if (lastStr=="A"|| lastStr == "O"|| lastStr == "어"|| lastStr == "드")
                { getUserJobDetails.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text += "요!"; }
            else
                { getUserJobDetails.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text += "이요!"; }
        }
        
    }
    //직무 선택
    GameObject selectedDetailJobObj;
    public void SaveUserJobDetail()
    {
        if (selectedDetailJobObj != null)
        { selectedDetailJobObj.transform.GetChild(1).gameObject.SetActive(false); }

        selectedDetailJobObj = EventSystem.current.currentSelectedGameObject;
        selectedDetailJobObj.transform.GetChild(1).gameObject.SetActive(true);

        UserDetailJobIndex = selectedDetailJobObj.transform.GetSiblingIndex();
        if (UserDetailJobIndex >= 4)
        {
            guideMessage.text = "아, 그랬나요?";
            daJeongImg.sprite = DaJeongFaces[0];
            countStep = 8;
        }
        else
        {
            guideMessage.text = "맞아요!";
            daJeongImg.sprite = DaJeongFaces[1];
            countStep = 7;
        }
        
        onboardingUserPage.SetActive(false);
        onboardingGuidePage.SetActive(true);

        //StartCoroutine(ChangeProgressBar(true, 40*7, 40*8));
        ChangeProgressBar(true, 40 * 7, 40 * 8);
    }
    #endregion


    //온보딩 완료 후 칭호 획득
    void GetFirstTitles()
    {
        UserManager.Instance.newUserInformation.titleCheck[0]++;
        UserManager.Instance.newUserInformation.userTitleModi = "신입";
        if (UserJobIndex == 0)
        {   UserManager.Instance.newUserInformation.titleCheck[1]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "기획자";
        }
        else if (UserJobIndex == 1)
        {   UserManager.Instance.newUserInformation.titleCheck[2]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "디자이너";
        }
        else if(UserJobIndex==5) //직무 선택 안함
        {
            UserManager.Instance.newUserInformation.userTitleNoun = "사원";
        }
        else //2,3,4
        { UserManager.Instance.newUserInformation.titleCheck[3]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "개발자";
        }

        //온보딩 정보 파이어베이스에 저장
        UserDB newUserData = new UserDB();
        newUserData.userInformation.userName = UserName;
        newUserData.userInformation.userJob = JobList[UserManager.Instance.newUserInformation.kindOfJob, UserManager.Instance.newUserInformation.detailJob];
        newUserData.userInformation.userTitle = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;

        //UserDB 저장
        UpdateUserDB(userId, newUserData);
        //유저 UserIdList 저장
        UpdateUserIdList();

        //칭호 획득창 띄우기
        UserManager.Instance.getTitle = 2;
    }

    //ProgressBar 움직이게
    //IEnumerator ChangeProgressBar(bool next, int startWidth, int finishWidth)
    void ChangeProgressBar(bool next, int startWidth, int finishWidth)
    {
        if (next)
        {
            for (int i = startWidth; i < finishWidth + 1; i++)
            {
                progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(i, 4);
                //yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (int i = startWidth; i > finishWidth + 1; i--)
            {
                progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(i, 4);
                //yield return new WaitForEndOfFrame();
            }
        }
    }

    #region 파이어베이스
    //DB에서 유저 data 가져오기
    private async Task<string> GetUserDB(string userId)
    {
        UserDB userDB = new UserDB();
        string userName;
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        try
        {
            var taskResult = await dataReference.GetValueAsync();
            userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
            userName = userDB.userInformation.userName;
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
            userName = "";
            //DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        }
        print("userName: " + userName);
        return userName;
    }

    //DB에 유저 data 저장하기
    private async void UpdateUserDB(string userId, UserDB userDB)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        string userDBstr = JsonConvert.SerializeObject(userDB);
        await dataReference.SetRawJsonValueAsync(userDBstr);
    }
    //UserIdList 저장하기
    private async void UpdateUserIdList()
    {
        //UserEmail List 저장
        Dictionary<string, string> userIdDic;

        DatabaseReference userIdList = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        var taskResult = await userIdList.GetValueAsync();
        userIdDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(taskResult.GetRawJsonValue());
        userIdDic.Add(UserName, userId);
        
        string userIdListStr = JsonConvert.SerializeObject(userIdDic);
        await userIdList.SetRawJsonValueAsync(userIdListStr);
    }
    //유저 Id 리스트 가져오기
    private async Task<bool> GetUserIdlList(string userId)
    {
        bool isThereUserId = false;

        DatabaseReference userList = FirebaseDatabase.DefaultInstance.GetReference("userList");
        try
        {
            var taskResult = await userList.GetValueAsync();
            foreach (var user in taskResult.Children)
            {
                if (user.Key == userId) { isThereUserId = true; }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        }
        return isThereUserId;
    }
    //유저 이름 리스트 가져오기
    private async Task<List<string>> getIdOrNameList(string type)
    {
        print("getIdOrNameList type: " + type);
        List<string> list = new List<string>();
        DatabaseReference userIdReference = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        Dictionary<string, string> userIdList = new Dictionary<string, string>();
        
        var taskResult = await userIdReference.GetValueAsync();
        try
        {
            userIdList = JsonConvert.DeserializeObject<Dictionary<string, string>>(taskResult.GetRawJsonValue());
            foreach (string key in userIdList.Keys)
            {
                if (type == "id")
                { list.Add(userIdList[key]); }
                else if (type == "name")
                { list.Add(key); }
            }
        }
        catch(Exception e) { Debug.LogError("Error deserializing JSON: " + e.Message); }
        return list;
    }

    //이번달 첫 유저면 랭킹 리셋 시키는 함수 실행시키기
    private async void CheckResetDate()
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("resetDate");
        var taskResult = await dataReference.GetValueAsync();
        string resetDate = JsonConvert.DeserializeObject<string>(taskResult.GetRawJsonValue());
        print("taskResult.Value.ToString(): " + resetDate);
        print("DateTime.Now.ToString(yyyy.MM): " + DateTime.Now.ToString("yyyy.MM"));
        if (resetDate != DateTime.Now.ToString("yyyy.MM"))
        {
            //리셋 함수 실행
            print("ResetRanking(idList);");
            List<string> idList = await getIdOrNameList("id");
            print("idList[0]: "+idList[0]);
            ResetRanking(idList);

            //resetDate 업데이트
            resetDate = DateTime.Now.ToString("yyyy.MM");
            await dataReference.SetValueAsync(resetDate);
        }
        //else {
        //    print("isOnboarding: " + isOnboarding);
        //    if (isOnboarding) { goHome(); } }
    }
    private async void ResetRanking(List<string> idList)
    {
        foreach(string userId in idList)
        {
            try
            {
                //print("try userId: " + userId);
                DatabaseReference userReference =
                    FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId).Child("rankingData");
                RankingData rankingData = new RankingData();
                string rankingDataStr = JsonConvert.SerializeObject(rankingData);
                await userReference.SetRawJsonValueAsync(rankingDataStr);
            }
            catch
            {
                //print("catch userId: " + userId);
                continue;
            }
        }
        //if (isOnboarding) { goHome(); }
    }
    #endregion

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void quitApplication() { Application.Quit(); }
}
