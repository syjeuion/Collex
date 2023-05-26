using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

public class Onboarding : MonoBehaviour
{
    //페이지
    public GameObject onboardingUserPage;
    public GameObject onboardingGuidePage;
    public GameObject progressBar;
    public Image backButtonIcon;

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
    int UserJobIndex; //직군
    int UserDetailJobIndex=5; //직무

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

        StartCoroutine(WaitSplash());
    }
    //온보딩 했으면 홈으로 바로넘어가기
    IEnumerator WaitSplash()
    {
        yield return new WaitForSeconds(1.5f);
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.newUserInformation.userName)) { goHome(); }
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

    #region 다음/이전
    public void OnClickPage()
    {
        if (countStep == 0) //첫인사>앱설명
        {
            daJeongImg.sprite = DaJeongFaces[0];
            guideMessage.text = "저희 <color=#408BFD>Collex</color>에서는 직무 관련 경험을 하면서 \n무엇을 했고, 배웠고, 느꼈는지를\n템플릿으로 기록할 수 있어요.";
            backButtonIcon.color = gray_800;
            StartCoroutine(ChangeProgressBar(true, 40, 40*2));
            countStep=1;
        }
        else if (countStep == 1) //앱설명>이름묻기
        {
            guideMessage.text = "아! 혹시 우리 회사에서 \n사용하고 싶은 닉네임이 있나요?";
            StartCoroutine(ChangeProgressBar(true, 40*2, 40*3));
            countStep=2;
        }
        else if (countStep == 2) //이름묻기>이름입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            StartCoroutine(ChangeProgressBar(true, 40*3, 40*4));
            countStep=3;
        }
        else if(countStep==4) //직군묻기>직군입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            getUserName.SetActive(false);
            getUserJob.SetActive(true);
            StartCoroutine(ChangeProgressBar(true, 40 * 5, 40 * 6));
            countStep = 5;
        }
        else if(countStep==7|| countStep == 8) //직무선택후반응>마무리
        {
            daJeongImg.sprite = DaJeongFaces[2];
            guideMessage.text = "앗, 갑자기 팀장님이 부르셔서...\n일단 활동 폴더 추가하고,\n기록을 작성해 보시겠어요?";
            StartCoroutine(ChangeProgressBar(true, 40*8, 360));
            countStep = 9;
        }
        else //countStep==9 //마무리>홈
        {
            UserManager.Instance.newUserInformation.userName = UserName;
            UserManager.Instance.newUserInformation.kindOfJob = UserJobIndex;
            if (UserDetailJobIndex < 4)
            { UserManager.Instance.newUserInformation.detailJob = JobList[UserJobIndex, UserDetailJobIndex]; }
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
            StartCoroutine(ChangeProgressBar(false, 40 * 2, 40));
            countStep = 0;
        }
        else if (countStep == 2) //이름묻기>앱설명
        {
            guideMessage.text = "저희 <color=#408BFD>Collex</color>에서는 직무 관련 경험을 하면서 \n무엇을 했고, 배웠고, 느꼈는지를\n템플릿으로 기록할 수 있어요.";
            StartCoroutine(ChangeProgressBar(false, 40 * 3, 40 * 2));
            countStep = 1;
        }
        else if (countStep == 3) //이름입력>이름묻기
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "아! 혹시 우리 회사에서 \n사용하고 싶은 닉네임이 있나요?";
            StartCoroutine(ChangeProgressBar(false, 40 * 4, 40 * 3));
            countStep = 2;
        }
        else if (countStep == 4) //직군묻기>이름입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            StartCoroutine(ChangeProgressBar(false, 40 * 5, 40 * 4));
            countStep = 3;
        }
        else if (countStep == 5) //직군입력>직군묻기
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "좋아요, " + UserName+ "님!\n저희 팀에서 어떤 업무를 담당할지 아시죠?";
            StartCoroutine(ChangeProgressBar(false, 40 * 6, 40 * 5));
            daJeongImg.sprite = DaJeongFaces[0];
            countStep = 4;
        }
        else if (countStep == 6) //직무입력>직군입력
        {
            getUserJobDetails.SetActive(false);
            getUserJob.SetActive(true);
            StartCoroutine(ChangeProgressBar(false, 40 * 7, 40 * 6));
            countStep = 5;
        }
        else if (countStep == 7 || countStep==8) //직무선택후반응>직무입력||직군입력
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            
            if (UserManager.Instance.newUserInformation.kindOfJob==5)
            {   countStep = 5;
                StartCoroutine(ChangeProgressBar(false, 40 * 8, 40 * 6));
            }
            else { countStep = 6; StartCoroutine(ChangeProgressBar(false, 40 * 8, 40 * 7)); }
        }
        else //countStep ==9 //마무리>직무선택후반응
        {
            if (!string.IsNullOrEmpty(UserManager.Instance.newUserInformation.detailJob))
            {   guideMessage.text = "맞아요!";
                daJeongImg.sprite = DaJeongFaces[1];
                countStep = 7;
            }
            else //직무 없음
            {   guideMessage.text = "아, 그랬나요?";
                countStep = 8;
                daJeongImg.sprite = DaJeongFaces[0];
            }
            StartCoroutine(ChangeProgressBar(false, 360, 40 * 8));
        }
    }
    #endregion

    #region 유저 이름
    //이름 입력
    int inputLength;
    public void OnSelectInputName()
    {
        inputUserName.transform.GetChild(1).gameObject.SetActive(true);
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
        else if(inputLength<=10)
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
    public void SaveUserName()
    {
        UserName = inputUserName.GetComponent<TMP_InputField>().text;
        onboardingGuidePage.SetActive(true);
        onboardingUserPage.SetActive(false);
        guideMessage.text = "좋아요, "+UserName+"님!\n저희 팀에서 어떤 업무를 담당할지 아시죠?";
        StartCoroutine(ChangeProgressBar(true, 40*4, 40*5));
        countStep = 4;
    }
    #endregion

    #region 직군/직무 선택
    //직군 선택
    
    GameObject selectedJobObj;
    public void SaveUserJob()
    {
        if (selectedJobObj != null)
        { selectedJobObj.transform.GetChild(1).gameObject.SetActive(false); }

        selectedJobObj = EventSystem.current.currentSelectedGameObject;
        selectedJobObj.transform.GetChild(1).gameObject.SetActive(true);
        UserJobIndex = selectedJobObj.transform.GetSiblingIndex();
        
        if (UserJobIndex == 5)
        {
            onboardingGuidePage.SetActive(true);
            onboardingUserPage.SetActive(false);
            guideMessage.text = "아, 그랬나요?";
            daJeongImg.sprite = DaJeongFaces[0];
            StartCoroutine(ChangeProgressBar(true, 40 * 6, 40 * 8));
            countStep = 8;
        }
        else
        {
            getUserJob.SetActive(false);
            getUserJobDetails.SetActive(true);
            SetJobDetails();
            StartCoroutine(ChangeProgressBar(true, 40 * 6, 40 * 7));
            countStep = 6;
        }
        
    }
    //직무 선택 페이지 세팅
    void SetJobDetails()
    {
        for (int i = 0; i < 4; i++)
        {
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

        StartCoroutine(ChangeProgressBar(true, 40*7, 40*8));
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

        UserManager.Instance.getTitle = 2;
    }

    //ProgressBar 움직이게
    IEnumerator ChangeProgressBar(bool next, int startWidth, int finishWidth)
    {
        if (next)
        {
            for (int i = startWidth; i < finishWidth + 1; i++)
            {
                progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(i, 4);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (int i = startWidth; i > finishWidth + 1; i--)
            {
                progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(i, 4);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
}
