using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System;
using UnityEngine.EventSystems;
using Newtonsoft.Json;


public class HomeManager : MonoBehaviour
{
    //페이지
    public GameObject HomeDefault;
    public GameObject NewFolderPopUp;
    public GameObject QuitAlert;

    public GameObject Home_MainContent;
    RectTransform wholeContentRect;

    //유저 정보
    public GameObject userProfiles;
    //[SerializeField] TMP_Text userName;
    //public Image profileimg;

    //칭호 관리
    public GameObject modiContainer;
    public GameObject nounContainer;

    //폴더 관리
    public TMP_InputField inputFolderTitle;

    public GameObject Types;

    public GameObject explanation;
    public GameObject explanation_end;

    public GameObject FoldersOngoing;
    public GameObject FoldersEnded;
    public GameObject singleFolder;
    GameObject newFolder;

    public GameObject SnackBar;
    public GameObject SnackBar2line;


    public Sprite[] myProfileImgs;

    //응원문구
    string[] cheerUpMessage = new string[]
    { "Collex에 입사한 걸 환영해요!","꾸준한 기록으로 목표를 이루어 볼까요?", "오늘도 기록을 해볼까요?", "끝까지 최선을 다해봐요!", "열심히 잘하고 있어요!" };

    
    #region start_홈화면 셋팅
    private void Start()
    {
        userProfiles.transform.GetChild(0).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];

        /*if (!string.IsNullOrWhiteSpace(UserManager.Instance.newUserInformation.userTitleModi))
        {
            userProfiles.transform.GetChild(1).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi;
            if (!string.IsNullOrWhiteSpace(UserManager.Instance.newUserInformation.userTitleNoun))
            { userProfiles.transform.GetChild(1).GetComponent<TMP_Text>().text += " "+UserManager.Instance.newUserInformation.userTitleNoun; }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(UserManager.Instance.newUserInformation.userTitleNoun))
            { userProfiles.transform.GetChild(1).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleNoun; }
            else { userProfiles.transform.GetChild(1).GetComponent<TMP_Text>().text = "반가워요"; }
        }*/
        //홈 스크롤 시 앱바 색상 변경용도
        wholeContentRect = Home_MainContent.GetComponent<RectTransform>();

        UserTitleManager.ActionUserTitle();
        //UIController.instance.ReloadUserTitleUI();
        userProfiles.transform.GetChild(2).GetComponent<TMP_Text>().text = "\n"+UserManager.Instance.newUserInformation.userName + "님!";
        
        //칭호 세팅
        if (UserManager.Instance.newUserInformation.isItFirstTargetTitle != 0)
        {
            Destroy(modiContainer.transform.parent.GetChild(0).gameObject);
            Destroy(modiContainer.transform.parent.GetChild(1).gameObject);
            setTargetTitle(modiContainer, UserManager.Instance.newUserInformation.targetTitleModi);
            setTargetTitle(nounContainer, UserManager.Instance.newUserInformation.targetTitleNoun);
        }

        //폴더 세팅
        if (UserManager.Instance.newUserInformation.isItFirst == 0)
        {
            explanation.SetActive(true);
            userProfiles.transform.GetChild(3).GetComponent<TMP_Text>().text = cheerUpMessage[0];
            //UserManager.Instance.isItFirst = 1;
        }
        else
        {
            System.Random random = new System.Random();
            int randomNumber = random.Next(1, 5);
            userProfiles.transform.GetChild(3).GetComponent<TMP_Text>().text = cheerUpMessage[randomNumber];
            explanation.SetActive(false);
            FoldersOngoing.SetActive(true);
            //FoldersEnded.SetActive(false);
            singleFolder.SetActive(true);

            folderSetting();
            if (FoldersOngoing.transform.childCount <= 0)
            {
                explanation.SetActive(true);
                explanation.GetComponent<TMP_Text>().text = "지금 진행 중인 활동이 없어요.\n새 활동을 시작해서 역량을 쌓아보세요!";
            }
        }

        //폴더 삭제로 홈으로 넘어왔을때
        if (UserManager.Instance.checkFolderDelete) StartCoroutine(setSnackBar(SnackBar));

    }
    //폴더 세팅 함수
    void folderSetting()
    {
        //리스트 초기화
        for(int i=0; i<FoldersOngoing.transform.childCount; i++) { Destroy(FoldersOngoing.transform.GetChild(i).gameObject); }
        for (int i = 0; i < FoldersEnded.transform.childCount; i++) { Destroy(FoldersEnded.transform.GetChild(i).gameObject); }

        int ongoingCount = 0;
        int endedCount = 0;
        foreach (string value in UserManager.Instance.folders.Values)
        {
            string getFolderData = value;
            MakeNewProject newProject = JsonConvert.DeserializeObject<MakeNewProject>(getFolderData);


            if (newProject.isItOngoing)
            {
                newFolder = Instantiate(singleFolder, FoldersOngoing.transform);

                if (newProject.records.Count == 0) { newFolder.transform.GetChild(4).gameObject.SetActive(false); }
                else
                {
                    TimeSpan howManyDays = DateTime.Now - newProject.lastRecordDate;
                    int howDays = howManyDays.Days;
                    newFolder.transform.GetChild(4).GetComponent<TMP_Text>().text = "마지막 작성 " + howDays.ToString() + "일 전";
                }
               
                ongoingCount += 1;
            }
            if (!newProject.isItOngoing)
            {
                explanation_end.SetActive(false);
                newFolder = Instantiate(singleFolder, FoldersEnded.transform);
                newFolder.transform.GetChild(4).GetComponent<TMP_Text>().text
                    = newProject.startDate.Year+"년 "+newProject.startDate.Month+"월 - "+ newProject.endedDate.Year+"년 "+newProject.endedDate.Month+"월";
                endedCount += 1;
            }

            newFolder.transform.SetAsFirstSibling();
            newFolder.transform.GetChild(0).GetComponent<TMP_Text>().text = newProject.projectType;
            newFolder.transform.GetChild(1).GetComponent<TMP_Text>().text = newProject.projectTitle;
            newFolder.transform.GetChild(2).GetComponent<TMP_Text>().text = newProject.records.Count.ToString();
            newFolder.transform.GetChild(3).GetComponent<TMP_Text>().text = newProject.experiences.Count.ToString();
            newFolder.GetComponent<Button>().onClick.AddListener(goFolder);
        }
        singleFolder.SetActive(false);

        //스크롤 뷰 크기 조정
        if (ongoingCount >= endedCount)
            wholeContentRect.sizeDelta = new Vector2(wholeContentRect.sizeDelta.x, 454 + (ongoingCount * 82));
        else
            wholeContentRect.sizeDelta = new Vector2(wholeContentRect.sizeDelta.x, 454 + (endedCount * 82));
    }
    #endregion

    private void Update()
    {
        //앱바 컬러 조절
        if (wholeContentRect.anchoredPosition.y >= 298)
            HomeDefault.transform.GetChild(3).GetComponent<Image>().color = Color.white;
        else
            {ColorUtility.TryParseHtmlString("#EFF5FF", out Color color);
            HomeDefault.transform.GetChild(3).GetComponent<Image>().color = color;}

        //홈에서 칭호 획득 했을 때
        if (UserManager.Instance.checkHomeTargetTitle)
        {
            setTargetTitle(modiContainer, UserManager.Instance.newUserInformation.targetTitleModi);
            setTargetTitle(nounContainer, UserManager.Instance.newUserInformation.targetTitleNoun);
            UserManager.Instance.checkHomeTargetTitle = false;
        }

        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) QuitAlert.SetActive(true);
    }

    #region 폴더 생성
    public void pushMakeFolder()
    {
        NewFolderPopUp.SetActive(true);
        ColorUtility.TryParseHtmlString("#E0E0E0", out Color color);
        ColorUtility.TryParseHtmlString("#9E9E9E", out Color color3);
        inputFolderTitle.GetComponent<Image>().color = color;
        inputFolderTitle.transform.GetChild(1).GetComponent<TMP_Text>().color = color3;
        inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().color = color3;
        inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
    }
    #region 제목 예외처리
    public void inputValueChanged()
    {
        ColorUtility.TryParseHtmlString("#FF3E49", out Color colorRed); //error
        ColorUtility.TryParseHtmlString("#408BFD", out Color colorBlue); //selected
        ColorUtility.TryParseHtmlString("#9E9E9E", out Color colorGray); //default
        inputFolderTitle.transform.GetChild(1).GetComponent<TMP_Text>().text = inputFolderTitle.text.Length.ToString() + "/20";

        //글자 수 체크
        if (inputFolderTitle.text.Length >= 20 || UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
        {
            inputFolderTitle.GetComponent<Image>().color = colorRed;
            inputFolderTitle.transform.GetChild(1).GetComponent<TMP_Text>().color = colorRed;
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().color = colorRed;
        }
        else //(inputFolderTitle.text.Length < 20)
        {
            inputFolderTitle.GetComponent<Image>().color = colorBlue;
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().color = colorBlue;
            inputFolderTitle.transform.GetChild(1).GetComponent<TMP_Text>().color = colorGray;
        }

        //중복 체크
        if (UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
        {
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().text = "중복되는 이름의 활동폴더가 있어요.";
        }
        else {
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
        }
    }
    public void inputDeselected()
    {
        ColorUtility.TryParseHtmlString("#E0E0E0", out Color color);
        inputFolderTitle.GetComponent<Image>().color = color;
    }
    #endregion
    #region 폴더 저장
    //string projectType;
    public void saveFolder()
    {
        //제목 안썼을 때
        if(inputFolderTitle.text.Length <= 0|| UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
        {
            ColorUtility.TryParseHtmlString("#FF3E49", out Color color1);
            inputFolderTitle.GetComponent<Image>().color = color1;
            inputFolderTitle.transform.GetChild(1).GetComponent<TMP_Text>().color = color1;
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().color = color1;
            inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().text = "활동폴더의 이름을 정해주세요.";
            if (UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
            {
                inputFolderTitle.transform.GetChild(2).GetComponent<TMP_Text>().text = "중복되는 이름의 활동폴더가 있어요.";
            }
            return;
        }
        NewFolderPopUp.SetActive(false);
        //설명 글 없애기
        explanation.SetActive(false);

        //데이터 저장
        MakeNewProject newProject = new MakeNewProject();

        newProject.startDate = DateTime.Now;
        //newProject.startYear = DateTime.Now.Year;
        //newProject.startMonth = DateTime.Now.Month;
        //newProject.startDay = DateTime.Now.Day;
        //newProject.startDate = DateTime.Now.Year.ToString()+"년 "+ DateTime.Now.Month.ToString() + "월 "+ DateTime.Now.Day.ToString() + "일";

        //newProject.lastRecordYear = DateTime.Now.Year;
        //newProject.lastRecordMonth = DateTime.Now.Month;
        //newProject.lastRecordDay = DateTime.Now.Day;

        newProject.projectTitle = inputFolderTitle.text;
        inputFolderTitle.text = "";
        
        UserManager.Instance.newUserInformation.isItFirst = 1; //더이상 폴더 첫 생성이 아님(디스크립션 차이)

        //칭호 획득했을때 획득한 칭호 정보 저장하고 획득 페이지 띄워주기
        UserManager.Instance.newUserInformation.titleCheck[4]++; //폴더 생성 count
        for(int i = 1; i < 4; i++)
        {
            UserManager.Instance.checkTitle("앞", i, 4);
        }
        if (UserManager.Instance.nowGetTitle.Count != 0) { UserManager.Instance.getTitle = 1; }

        //목표칭호 있으면 다시 세팅
        if (UserManager.Instance.newUserInformation.isItFirstTargetTitle != 0)
        {
            setTargetTitle(modiContainer, UserManager.Instance.newUserInformation.targetTitleModi);
        }

        //활동 종류 저장
        for (int i = 0; i < 3; i++)
        {
            if (Types.transform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                UserManager.Instance.projectType[Types.transform.GetChild(i).name] += 1;
                newProject.projectType = Types.transform.GetChild(i).name;
            }
        }

        string newFolderData = JsonConvert.SerializeObject(newProject);
        UserManager.Instance.folders.Add(newProject.projectTitle, newFolderData);

        //데이터 출력
        FoldersOngoing.SetActive(true);
        singleFolder.SetActive(true);

        folderSetting(); //리스트 출력

    }
    #endregion
    #endregion

    #region 목표칭호 설정
    string newTargetTitle;
    public void saveTargetTitle()
    {
        newTargetTitle = "";
        
        if (modiContainer.transform.parent.childCount > 4)
        {
            Destroy(modiContainer.transform.parent.GetChild(0).gameObject);
            Destroy(modiContainer.transform.parent.GetChild(1).gameObject);
        }
        UserTitleManager.TargetTitle(); //저장하는 함수 호출
        
        setTargetTitle(modiContainer, UserManager.Instance.newUserInformation.targetTitleModi);
        setTargetTitle(nounContainer, UserManager.Instance.newUserInformation.targetTitleNoun);

        UserManager.Instance.newUserInformation.isItFirstTargetTitle = 1;

        SnackBar.transform.GetChild(0).GetComponent<TMP_Text>().text = "'"+newTargetTitle + "'을/를 목표 칭호로 설정했어요.";
        StartCoroutine(setSnackBar(SnackBar));
    }
    //칭호 세팅
    void setTargetTitle(GameObject container, string[] titleArray)
    {
        container.SetActive(true);
        if (!string.IsNullOrWhiteSpace(titleArray[0]))
        {
            container.transform.GetChild(1).gameObject.SetActive(true);
            container.transform.GetChild(2).gameObject.SetActive(true);
            container.transform.GetChild(3).gameObject.SetActive(false);

            //데이터 세팅
            container.transform.GetChild(0).GetComponent<TMP_Text>().text = titleArray[0];
            container.transform.GetChild(1).GetComponent<TMP_Text>().text = titleArray[1];
            
            int userCount = UserManager.Instance.newUserInformation.titleCheck[int.Parse(titleArray[4])];
            int maxCount = int.Parse(titleArray[3]);
            if (userCount >= maxCount) userCount = maxCount;

            float gauge = (float)((double)userCount / (double)maxCount);
            container.transform.GetChild(2).GetChild(1).GetComponent<Image>().fillAmount = gauge;
            container.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = userCount.ToString() + "/" + maxCount.ToString();

            ColorUtility.TryParseHtmlString("#FFFFFF", out Color strokeColor);
            container.GetComponent<Image>().color = strokeColor;
            if (!string.IsNullOrWhiteSpace(newTargetTitle)) { newTargetTitle += " "; }
            newTargetTitle += titleArray[0];
        }
        else
        {
            if(container.name == "Selected Modif Container")
                container.transform.GetChild(0).GetComponent<TMP_Text>().text = "앞 칭호";
            else
                container.transform.GetChild(0).GetComponent<TMP_Text>().text = "뒤 칭호";
            container.transform.GetChild(1).gameObject.SetActive(false);
            container.transform.GetChild(2).gameObject.SetActive(false);
            container.transform.GetChild(3).gameObject.SetActive(true);
            ColorUtility.TryParseHtmlString("#E0E0E0", out Color strokeColor);
            container.GetComponent<Image>().color = strokeColor;
        }
    }
    #endregion

    //프로필 이미지 클릭 시 사원증 수정 페이지로 이동
    public void ClickProfileImg()
    {
        UserManager.Instance.editProfileInHome = true;
        goMypage();
    }

    //홈화면 fab버튼
    public void pushMakeNewRecord()
    {
        if (FoldersOngoing.transform.childCount <= 0)
        {
            StartCoroutine(setSnackBar(SnackBar2line));
            return;
        }
        else
        {
            UserManager.Instance.pushedButton = "";
            UserManager.Instance.pushedRecord = "";
            SceneManager.LoadScene("2_Writing");
        }
    }

    //토스트 팝업
    IEnumerator setSnackBar(GameObject toastPopUp)
    {
        toastPopUp.SetActive(true);
        yield return new WaitForSeconds(3f);
        toastPopUp.SetActive(false);
    }

    //씬 관리
    public void goFolder()
    {
        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        UserManager.Instance.pushedButton = clickButton.transform.GetChild(1).GetComponent<TMP_Text>().text;
        SceneManager.LoadScene("3_Folder");
    }

    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goRanking() { SceneManager.LoadScene("5_Ranking"); }
    public void goMypage() { SceneManager.LoadScene("6_Mypage"); }

    //앱 종료
    public void quitApplication() { Application.Quit(); }
}