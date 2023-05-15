using System.Collections;
using System.Collections.Generic;
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

    //단계 체크
    int countStep;

    //사용 컬러
    Color primary1;
    Color primary3;
    Color errorColor;
    Color gray_200;
    Color gray_500;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);
        ColorUtility.TryParseHtmlString("#EBEDEF", out gray_200);
        ColorUtility.TryParseHtmlString("#949CA8", out gray_500);

        Screen.fullScreen = false;

        if (UserManager.Instance.firstOpen)
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

        StartCoroutine(WaitSplash());
    }
    //온보딩 했으면 홈으로 바로넘어가기
    IEnumerator WaitSplash()
    {
        yield return new WaitForSeconds(1.5f);
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.newUserInformation.userName)) { goHome(); }
    }

    //화면 터치 시 페이지 전환
    public void OnClickPage()
    {
        if (countStep == 0)
        {
            daJeongImg.sprite = DaJeongFaces[0];
            guideMessage.text = "아, 이름이 뭐라고 하셨죠?";
            StartCoroutine(ChangeProgressBar(45, 90));
            countStep++;
        }
        else if (countStep == 1)
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            StartCoroutine(ChangeProgressBar(90, 135));
            countStep++;
        }
        else if (countStep == 2)
        {
            onboardingGuidePage.SetActive(false);
            onboardingUserPage.SetActive(true);
            getUserName.SetActive(false);
            getUserJob.SetActive(true);
            StartCoroutine(ChangeProgressBar(180, 225));
            countStep++;
        }
        else if(countStep==3)
        {
            StartCoroutine(ChangeProgressBar(315, 360));
            daJeongImg.sprite = DaJeongFaces[2];
            guideMessage.text = "앗, 갑자기 팀장님이 부르셔서...\n일단 활동 폴더 추가하고,\n기록을 작성해 보시겠어요?";
            countStep++;
        }
        else
        {
            GetFirstTitles();
        }
    }

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
        string UserName = inputUserName.GetComponent<TMP_InputField>().text;
        UserManager.Instance.newUserInformation.userName = UserName;
        
        onboardingGuidePage.SetActive(true);
        onboardingUserPage.SetActive(false);
        guideMessage.text = "맞다, "+UserName+"님이었죠.\n"+ UserName + "님, 저희 팀에서\n어떤 업무를 담당할지 아시죠?";
        StartCoroutine(ChangeProgressBar(135, 180));
    }
    #endregion

    //직군 선택
    int UserJobIndex;
    public void SaveUserJob()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        UserJobIndex = currentObj.transform.GetSiblingIndex();
        UserManager.Instance.newUserInformation.kindOfJob = UserJobIndex;

        getUserJob.SetActive(false);
        getUserJobDetails.SetActive(true);

        SetJobDetails();
    }
    //직무 선택 페이지 세팅
    void SetJobDetails()
    {
        StartCoroutine(ChangeProgressBar(225, 270));
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
    public void SaveUserJobDetail()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;

        if (currentObj.transform.GetSiblingIndex() >= 4)
        {
            guideMessage.text = "아, 그랬나요?";
            daJeongImg.sprite = DaJeongFaces[0];
        }
        else
        {
            UserManager.Instance.newUserInformation.detailJob = JobList[UserJobIndex, currentObj.transform.GetSiblingIndex()];

            guideMessage.text = "맞아요!";
            daJeongImg.sprite = DaJeongFaces[1];
        }
        
        onboardingUserPage.SetActive(false);
        onboardingGuidePage.SetActive(true);

        StartCoroutine(ChangeProgressBar(270, 315));
    }

    //온보딩 완료 후 칭호 획득
    void GetFirstTitles()
    {
        UserManager.Instance.newUserInformation.titleCheck[0]++;
        UserManager.Instance.newUserInformation.userTitleModi = "주니어";
        if (UserJobIndex == 0)
        {   UserManager.Instance.newUserInformation.titleCheck[1]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "기획자";
        }
        else if (UserJobIndex == 1)
        {   UserManager.Instance.newUserInformation.titleCheck[2]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "디자이너";
        }
        else
        { UserManager.Instance.newUserInformation.titleCheck[3]++;
            UserManager.Instance.newUserInformation.userTitleNoun = "개발자";
        }

        UserManager.Instance.getTitle = 2;
    }

    //ProgressBar 움직이게
    IEnumerator ChangeProgressBar(int startWidth, int finishWidth)
    {
        for(int i=startWidth; i < finishWidth+1; i++)
        {
            progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(i, 4);
            yield return new WaitForEndOfFrame();
        }
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
}
