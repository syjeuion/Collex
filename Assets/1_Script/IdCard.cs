using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;
using System.Threading.Tasks;
using Firebase.Database;

public class IdCard : MonoBehaviour
{
    MakeNewProject newProject;
    DailyRecord newRecord;
    //page
    public GameObject EditIdCardPage;
    public GameObject SetUserJobPage;
    public GameObject BookmarkPage;

    public GameObject IdCardSection;
    public GameObject colorOption;
    public GameObject editProfileImg;
    public GameObject editIdCardAlert;

    //로그아웃
    public GameObject Dialog_Logout;

    public GameObject editIdCard_userName;

    //버튼
    public Sprite Button_Disabled;
    public Sprite Button_Enabled;

    //사원증 이미지
    public GameObject idCard;
    public Image userProfileImg;
    public Sprite[] ProfileImgs;
    int profileNumber;

    //사원증
    public GameObject idcard_front;
    public GameObject idcard_back;
    public Animator friendIdcardAni;

    //사원증 컬러
    string[,] colorList = new string[5, 3] {
        { "#FFDC00", "#FEFAE0", "#D79044" }, //노랑:0
        { "#FE944D", "#FFF4E4", "#FE944D" }, //오렌지:1
        { "#06C755", "#EEF8EA", "#06C755" }, //초록:2
        { "#2AC1BC", "#E6F4F4", "#2AC1BC" }, //민트:3
        { "#408BFD", "#EFF5FF", "#408BFD" } }; //블루:4
    int colorNumber; //선택된 컬러 저장

    //유저 직군
    string[] Jobs = new string[5] { "기획","디자인","프론트엔드","백엔드","데이터"};
    int userJob;
    //int selectedJob;
    int userDetailJob;
    private string[,] JobList = new string[5, 4]
    {   {"서비스 기획","UX 기획","PM/PO","사업 기획"},
        { "UX/UI 디자인", "영상 디자인", "브랜드 디자인", "그래픽 디자인" },
        { "앱 개발", "웹 개발", "웹 퍼블리싱", "시스템 엔지니어" },
        { "서버 개발", "IT 클라우드", "기술지원", "QA 엔지니어" },
        { "데이터 분석", "데이터 엔지니어", "인공지능", "DBA" } };

    //사용 컬러
    Color primary1;
    Color primary3;
    Color gray300;
    Color gray500;
    Color errorColor;

    string userName;
    string userId;
    string userWishCompany;
    int profileImgNum;
    UserDB thisUserDB;

    //이름 리스트
    List<string> userNameList;
    private async void Start()
    {
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#DDE0E3", out gray300);
        ColorUtility.TryParseHtmlString("#949CA8", out gray500);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);
        
        if (UserManager.Instance.newUserInformation.titleCheck[27] == 0)
        { colorNumber = 3; }
        else
        { colorNumber = UserManager.Instance.newUserInformation.idCardColorNumber; }

        //profileNumber = UserManager.Instance.newUserInformation.userProfileImgNumber;
        //setIdCard();

        userJob = UserManager.Instance.newUserInformation.kindOfJob;
        userDetailJob = UserManager.Instance.newUserInformation.detailJob;

        //userName = UserManager.Instance.newUserInformation.userName;
        userId = UserManager.Instance.newUserInformation.userId;

        changeCardColor();
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        GetThisUserDB();

        if (UserManager.Instance.editProfileInHome) { setEditIdCard(); }

        //이름 리스트 가져오기
        userNameList = await GetNameList();
    }
    //UserDB 가져오기
    private async void GetThisUserDB()
    {
        try
        {
            thisUserDB = await GetUserDB(userId);
            userName = thisUserDB.userInformation.userName;
            userWishCompany = thisUserDB.userWishCompany;
            profileImgNum = thisUserDB.userInformation.userProfileImg;
            setIdCard(profileImgNum, userWishCompany, userName);
        }
        catch(Exception e) {
            Debug.LogError("Error: " + e.Message);
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
        }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
    }

    bool EscCheck = true;
    private void Update()
    {
        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape) && EscCheck)
        {
            EscCheck = false;
            StartCoroutine(OnClickEsc());
        }
    }
    IEnumerator OnClickEsc()
    {
        int num = UIController.instance.curOpenPageNum;
        if (num == -2)
        {   DontDestroyCanvas.setRecord(false);
            UIController.instance.curOpenPageNum = 3;
        }
        else if(num == 1)
        {
            CheckBackEditIDcard();
        }
        else if(num == 2)
        {
            BackFunc(1);
        }
        else if(num == -1 || num == -4)
        {
            goHome();
        }
        else if(num!=-3)
        {
            BackFunc(-1);
        }
        yield return new WaitForSeconds(0.1f);
        EscCheck = true;
    }

    //사원증 세팅
    public void setIdCard(int profileNum, string company, string name)
    {
        //GameObject idCard = IdCardSection.transform.GetChild(1).gameObject;

        //프로필 이미지
        userProfileImg.sprite = ProfileImgs[profileNum];

        //목표 회사
        idCard.transform.GetChild(1).GetComponent<TMP_Text>().text = company;

        //setTitle(idCard.transform.GetChild(2).gameObject, UserManager.Instance.newUserInformation.userTitleModi, UserManager.Instance.newUserInformation.userTitleNoun);//칭호
        //UIController.instance.ReloadUserTitleUI();
        UserTitleManager.ActionUserTitle();
        idCard.transform.GetChild(3).GetComponent<TMP_Text>().text = name; //유저 이름
    }

    #region 사원증 컬러 변경
    GameObject selectedObj;
    int first;
    public void changeCardColor()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            selectedObj = EventSystem.current.currentSelectedGameObject;
            if (selectedObj.name == "ColorYellow") colorNumber = 0;
            else if (selectedObj.name == "ColorOrange") colorNumber = 1;
            else if (selectedObj.name == "ColorGreen") colorNumber = 2;
            else if (selectedObj.name == "ColorMint") colorNumber = 3;
            else if (selectedObj.name == "ColorBlue") colorNumber = 4;
        }

        ColorUtility.TryParseHtmlString(colorList[colorNumber, 0], out Color cardColor);
        ColorUtility.TryParseHtmlString(colorList[colorNumber, 1], out Color backgroundColor);
        ColorUtility.TryParseHtmlString(colorList[colorNumber, 2], out Color companyColor);

        IdCardSection.GetComponent<Image>().color = backgroundColor;
        IdCardSection.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = cardColor;
        IdCardSection.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = companyColor;

        IdCardSection.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = cardColor;
        for (int i = 0; i < colorOption.transform.childCount; i++)
        {
            if (!colorOption.transform.GetChild(i).gameObject.activeSelf)
                colorOption.transform.GetChild(i).gameObject.SetActive(true);
        }

        if (first != 0)
        {
            if (UserManager.Instance.newUserInformation.titleCheck[27] == 0)
            {
                UserManager.Instance.newUserInformation.titleCheck[27]++;
                UserManager.Instance.checkTitle("앞", 25, 27);
                UserManager.Instance.getTitle = 1;
            }
        }
        first++;

        UserManager.Instance.newUserInformation.idCardColorNumber = colorNumber;
        colorOption.transform.GetChild(colorNumber).gameObject.SetActive(false);
        IdCardSection.transform.GetChild(1).GetComponent<Toggle>().isOn = false;
        colorOption.SetActive(false);

        //파이어베이스 업데이트
        UpdateIdcardColor();
    }
    private async void UpdateIdcardColor()
    {
        thisUserDB = await GetUserDB(userId);
        thisUserDB.idcardColor = colorNumber;
        UpdateUserDB(userId, thisUserDB);
    }
    #endregion

    #region 사원증 수정
    public GameObject jobContent;
    //사원증 수정 페이지 세팅
    public void setEditIdCard()
    {
        EditIdCardPage.SetActive(true);
        editIdCardAlert.SetActive(false);
        UIController.instance.curOpenPageNum = 1;
        //사원증 프로필 사진 수정
        EditIdCardPage.transform.GetChild(0).GetComponent<Image>().sprite = ProfileImgs[profileImgNum];

        //이름
        EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = userName;
        EditIdCardPage.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text.Length.ToString() + "/10";
        editIdCard_userName.transform.GetChild(3).gameObject.SetActive(false);

        //목표기업명
        if (!string.IsNullOrWhiteSpace(userWishCompany))
        {
            EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text = userWishCompany;
            EditIdCardPage.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text =
                EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text.Length.ToString() + "/10";
        }

        //직군직무
        SetJobTextUI(UserManager.Instance.newUserInformation.kindOfJob, UserManager.Instance.newUserInformation.detailJob);

        //setTitle(EditIdCardPage.transform.GetChild(4).GetChild(1).GetChild(0).gameObject, UserManager.Instance.newUserInformation.userTitleModi, UserManager.Instance.newUserInformation.userTitleNoun);
        EditIdCardPage.transform.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text =
            UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
    }
    void SetJobTextUI(int Job, int detailJob)
    {
        if (Job == 5)
        { EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "직군/직무 미선택"; }
        else
        {
            EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text =
            Jobs[Job] + " · ";
            if (detailJob >= 4)
            { EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += "직무 미선택"; }
            else
            { EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += JobList[Job, detailJob]; }
        }
    }

    #region 사원증 프로필 이미지 선택
    GameObject lastSelectedProfileImg;
    public void setEditProfileImg()
    {
        editProfileImg.SetActive(true);
        if (profileImgNum == 0) { return; }
        else
        {
            for(int i=0; i<8; i++)
            {
                if (i == (profileImgNum - 1))
                { editProfileImg.transform.GetChild(i).GetChild(0).gameObject.SetActive(true); }
                else { editProfileImg.transform.GetChild(i).GetChild(0).gameObject.SetActive(false); }
            }
        }
    }
    public void ChangeProfileImg()
    {
        if (lastSelectedProfileImg != null)
        { lastSelectedProfileImg.transform.GetChild(0).gameObject.SetActive(false); }

        lastSelectedProfileImg = EventSystem.current.currentSelectedGameObject;
        Image nowProfile = lastSelectedProfileImg.GetComponent<Image>();
        for (int i = 0; i < ProfileImgs.Length; i++)
        {
            if (nowProfile.sprite == ProfileImgs[i])
            {
                EditIdCardPage.transform.GetChild(0).GetComponent<Image>().sprite = nowProfile.sprite;
                profileNumber = i;
                nowProfile.gameObject.transform.parent.gameObject.SetActive(false);
                return;
            }
        }
    }
    #endregion

    #region 직군/직무 수정
    public void setJobPage()
    {
        SetUserJobPage.SetActive(true);
        UIController.instance.curOpenPageNum = 2;
        //if(jobContent.transform.FindChild(userJob)) //find로 찾는 방법 - 한글을 써야함

        if (userJob == 5) return;
        if (userDetailJob >= 4) return;
        
        jobContent.transform.GetChild(userJob).GetChild(userDetailJob+1).GetComponent<Toggle>().isOn = true;
    }
    //직군/직무 선택할때 반영
    public void ChangeJob()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        
        if (currentObj.GetComponent<Toggle>() == null) return;
        
        userDetailJob = currentObj.transform.GetSiblingIndex()-1;
        string jobGroup = currentObj.transform.parent.name;
        if (jobGroup == "PlannerGroup") userJob = 0;
        else if (jobGroup == "DesignerGroup") userJob = 1;
        else if (jobGroup == "FrontEndGroup") userJob = 2;
        else if (jobGroup == "BackEndGroup") userJob = 3;
        else if (jobGroup == "DataGroup") userJob = 4;

        /*if (userDetailJob != UserManager.Instance.newUserInformation.detailJob)
        {
            SetUserJobPage.transform.GetChild(2).GetComponent<Button>().interactable = true;
            SetUserJobPage.transform.GetChild(2).GetComponent<Image>().sprite = Button_Enabled;
            SetUserJobPage.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().color = primary1;
        }
        else
        {
            SetUserJobPage.transform.GetChild(2).GetComponent<Button>().interactable = false;
            SetUserJobPage.transform.GetChild(2).GetComponent<Image>().sprite = Button_Disabled;
            SetUserJobPage.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().color = gray500;
        }*/
    }
    //직군/직무 저장
    public void SaveChangeJob()
    {
        SetJobTextUI(userJob, userDetailJob);
        //EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = Jobs[userJob] + " · " + JobList[userJob,userDetailJob];
        SetUserJobPage.SetActive(false);
        UIController.instance.curOpenPageNum = 1;
    }
    #endregion

    #region 인풋필드 예외처리
    GameObject currentInputObj;
    public void inputOnselected()
    {
        currentInputObj = EventSystem.current.currentSelectedGameObject;
        if (currentInputObj.GetComponent<TMP_InputField>() == null) return;
        currentInputObj.GetComponent<Image>().color = primary3;
        currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().color = primary3;
        editIdCard_userName.transform.GetChild(3).gameObject.SetActive(false);
    }
    public void checkInput()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
        currentInputObj = EventSystem.current.currentSelectedGameObject;
        if (currentInputObj.GetComponent<TMP_InputField>() == null) return;
        if (currentInputObj.GetComponent<TMP_InputField>().text.Length >= 10)
        {
            currentInputObj.GetComponent<Image>().color = errorColor;
            currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().color = errorColor;
            //currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().color = errorColor;
        }
        else
        {
            currentInputObj.GetComponent<Image>().color = primary3;
            currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().color = primary3;
        }

        currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().text = currentInputObj.GetComponent<TMP_InputField>().text.Length.ToString() + "/10";
    }
    public void inputDeselected()
    {
        currentInputObj.GetComponent<Image>().color = gray300;
        currentInputObj.transform.parent.GetChild(2).GetComponent<TMP_Text>().color = gray500;
    }
    #endregion

    //사원증 수정에서 뒤로가기 눌렀을때 변경사항 체크
    public void CheckBackEditIDcard()
    {
        if(profileImgNum != profileNumber||
            userName != EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text||
            userWishCompany != EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text||
            UserManager.Instance.newUserInformation.detailJob != userDetailJob||
            !string.IsNullOrWhiteSpace(UserManager.Instance.selectedModi)||
            !string.IsNullOrWhiteSpace(UserManager.Instance.selectedNoun))
        {
            editIdCardAlert.SetActive(true);
        }
        else if (UserManager.Instance.editProfileInHome)
            { goHome(); UserManager.Instance.editProfileInHome = false; }
        else { EditIdCardPage.SetActive(false); }
        UIController.instance.curOpenPageNum = -1;
    }

    //사원증 수정 내역 저장
    public void SaveIdCard()
    {
        string newName = EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text;

        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }
        else if (userNameList.Contains(newName)&& newName != userName)
        {
            editIdCard_userName.transform.GetChild(1).GetComponent<Image>().color = errorColor;
            editIdCard_userName.transform.GetChild(3).gameObject.SetActive(true);
            return;
        }

        profileImgNum = profileNumber;
        UserManager.Instance.newUserInformation.userProfileImgNumber = profileImgNum;

        UserManager.Instance.newUserInformation.userName = newName;
        //UserManager.Instance.newUserInformation.userName = userName;

        userWishCompany = EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text;

        UserManager.Instance.newUserInformation.kindOfJob = userJob;
        UserManager.Instance.newUserInformation.detailJob = userDetailJob;
        if(userJob == 0)
        {
            UserManager.Instance.newUserInformation.titleCheck[1]++;
            UserManager.Instance.checkTitle("뒤", 0, 1);
        }
        else if (userJob == 1)
        {
            UserManager.Instance.newUserInformation.titleCheck[2]++;
            UserManager.Instance.checkTitle("뒤", 1, 2);
        }
        else if(userJob <= 4)
        {
            UserManager.Instance.newUserInformation.titleCheck[3]++;
            UserManager.Instance.checkTitle("뒤", 2, 3);
        }


        if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedModi))
        { UserManager.Instance.newUserInformation.userTitleModi = UserManager.Instance.selectedModi; }
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedNoun))
        { UserManager.Instance.newUserInformation.userTitleNoun = UserManager.Instance.selectedNoun; }

        if (UserManager.Instance.newUserInformation.titleCheck[28] == 0)
        {
            UserManager.Instance.newUserInformation.titleCheck[28]++;
            UserManager.Instance.checkTitle("앞", 26, 28);
        }
        UserManager.Instance.getTitle = 1;

        if (UserManager.Instance.editProfileInHome) { goHome(); UserManager.Instance.editProfileInHome = false; }
        else { EditIdCardPage.SetActive(false);
            UIController.instance.curOpenPageNum = -1;
            setIdCard(profileNumber, userWishCompany, UserManager.Instance.newUserInformation.userName); }

        //파이어베이스 업데이트
        DontDestroyCanvas.controlProgressIndicator(true); //인디케이터 시작
        UpdateUserInfo();
    }
    private async void UpdateUserInfo()
    {
        thisUserDB = await GetUserDB(userId);
        thisUserDB.userWishCompany = userWishCompany;
        thisUserDB.userInformation.userProfileImg = profileImgNum;

        UserDefaultInformation userInfo = thisUserDB.userInformation;
        userInfo.userProfileImg = profileNumber;
        userInfo.userName = UserManager.Instance.newUserInformation.userName;
        userInfo.userJob = JobList[userJob, userDetailJob];

        UpdateUserDB(userId, thisUserDB);

        if(UserManager.Instance.newUserInformation.userName != userName)
        {
            UpdateUserIdList(userName, UserManager.Instance.newUserInformation.userName);
            userName = UserManager.Instance.newUserInformation.userName;
        }
    }
    #endregion

    #region 북마크
    public GameObject bookmarkContent;
    public GameObject bookmarkRecord;
    GameObject newbookmarkRecord;

    string capas;
    public void setBookmarkPage()
    {
        BookmarkPage.SetActive(true);
        UIController.instance.curOpenPageNum = 3;
        StartCoroutine(setBookMarkList());
    }
    IEnumerator setBookMarkList()
    {
        for (int i = 1; i < bookmarkContent.transform.childCount; i++)
        {
            Destroy(bookmarkContent.transform.GetChild(i).gameObject);
        }

        List<string> bookmarkList = UserManager.Instance.bookmarks;
        for (int i = 0; i < bookmarkList.Count; i++)
        {
            string bookmark = bookmarkList[i];
            string[] bookmarkTitles = bookmark.Split('\t');

            if (UserManager.Instance.folders.ContainsKey(bookmarkTitles[0]))
            {
                string bookmarkRecordData = UserManager.Instance.folders[bookmarkTitles[0]];
                newProject = JsonConvert.DeserializeObject<MakeNewProject>(bookmarkRecordData);

                if (newProject.records.ContainsKey(bookmarkTitles[1]))
                {
                    newRecord = JsonConvert.DeserializeObject<DailyRecord>(newProject.records[bookmarkTitles[1]]);

                    newbookmarkRecord = Instantiate(bookmarkRecord, bookmarkContent.transform);
                    newbookmarkRecord.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = bookmarkTitles[0];
                    newbookmarkRecord.transform.GetChild(1).GetComponent<TMP_Text>().text = newRecord.title;
                    newbookmarkRecord.transform.GetChild(2).GetComponent<TMP_Text>().text = newRecord.writings["활동내용"];

                    capas = "";//초기화
                    for (int ii = 0; ii < 3; ii++)
                    {
                        if (newRecord.capabilities[ii] == null) { break; }
                        else if (ii != 0) { capas += " ・ "; }
                        capas += newRecord.capabilities[ii];
                    }
                    newbookmarkRecord.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = capas;
                    newbookmarkRecord.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = newRecord.date;
                    newbookmarkRecord.GetComponent<Button>().onClick.AddListener(clickRecord);
                }
            }
        }
        yield return new WaitForEndOfFrame();
        bookmarkContent.GetComponent<VerticalLayoutGroup>().spacing = -7.9f;
        bookmarkContent.GetComponent<VerticalLayoutGroup>().spacing = -8;

        if (bookmarkContent.transform.childCount != 1)
            bookmarkContent.transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion

    #region 사원증 상세보기
    //사원증 오픈
    public void OpenIdCard()
    {
        idcard_front.transform.parent.gameObject.SetActive(true);
        UIController.instance.curOpenPageNum = -3;

        //온보딩 체크
        if (!UserManager.Instance.newUserInformation.idcard_onboarding)
        {
            idcard_front.transform.parent.GetChild(2).gameObject.SetActive(true);
            UserManager.Instance.newUserInformation.idcard_onboarding = true;
        }

        SetIdcardFront(thisUserDB);
        SetIdcardBack(thisUserDB);
    }
    //앞면 데이터 세팅
    private void SetIdcardFront(UserDB friendDB)
    {
        UserDefaultInformation friendDefaultInfo = friendDB.userInformation;
        ColorUtility.TryParseHtmlString(colorList[friendDB.idcardColor, 0], out Color cardColor);
        ColorUtility.TryParseHtmlString(colorList[friendDB.idcardColor, 1], out Color companyColor);

        idcard_front.transform.GetChild(0).GetComponent<Image>().color = cardColor;
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = ProfileImgs[friendDefaultInfo.userProfileImg];
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
        idcard_front.transform.GetChild(1).GetComponent<Image>().sprite = ProfileImgs[0];
        idcard_front.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(3).GetComponent<TMP_Text>().text = "";
        idcard_front.transform.GetChild(4).GetComponent<TMP_Text>().text = "";

        idcard_front.transform.parent.GetChild(2).gameObject.SetActive(false);
        UIController.instance.curOpenPageNum = -1;
    }
    #endregion

    #region 파이어베이스 realTimeDB
    //DB에서 유저 data 가져오기
    private async Task<UserDB> GetUserDB(string id)
    {
        UserDB userDB = new UserDB();
        try
        {
            DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(id);
            var taskResult = await dataReference.GetValueAsync();
            userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
        }
        catch(Exception e)
        {
            Debug.LogError("error: " + e.Message);
            DontDestroyCanvas.openQuitAlert(); //강제 종료 알랏
        }
        DontDestroyCanvas.controlProgressIndicator(false); //인디케이터 종료
        return userDB;
    }
    //DB에 유저 data 저장하기
    private async void UpdateUserDB(string id, UserDB userDB)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(id);
        string userDBstr = JsonConvert.SerializeObject(userDB);
        await dataReference.SetRawJsonValueAsync(userDBstr);
    }
    //유저 이름으로 유저 Id 가져오기
    private async void UpdateUserIdList(string userName, string newUserName)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        var taskResult = await dataReference.GetValueAsync();

        Dictionary<string, string> userIdDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(taskResult.GetRawJsonValue());

        userIdDic.Remove(userName);
        userIdDic.Add(newUserName, userId);

        string userIdDicStr = JsonConvert.SerializeObject(userIdDic);
        await dataReference.SetRawJsonValueAsync(userIdDicStr);
    }
    //유저 이름 리스트 가져오기
    private async Task<List<string>> GetNameList()
    {
        List<string> list = new List<string>();
        DatabaseReference userIdReference = FirebaseDatabase.DefaultInstance.GetReference("userIdList");
        Dictionary<string, string> userIdList = new Dictionary<string, string>();

        var taskResult = await userIdReference.GetValueAsync();
        try
        {
            userIdList = JsonConvert.DeserializeObject<Dictionary<string, string>>(taskResult.GetRawJsonValue());
            foreach (string key in userIdList.Keys)
            {
                list.Add(key);
            }
        }
        catch (Exception e) { Debug.LogError("Error deserializing JSON: " + e.Message); }
        return list;
    }
    #endregion

    //기록 상세페이지
    public void clickRecord()
    {
        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        UserManager.Instance.pushedButton = clickButton.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text;
        UserManager.Instance.pushedRecord = clickButton.transform.GetChild(1).GetComponent<TMP_Text>().text;
        DontDestroyCanvas.setRecord(true);
        UIController.instance.curOpenPageNum = -2;
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goRanking() { SceneManager.LoadScene("5_Ranking"); }


    //페이지 열기
    public void OpenPage(int num)
    {
        UIController.instance.PageObjArr[num].SetActive(true);
        UIController.instance.curOpenPageNum = num;
    }

    //뒤로가기
    public void BackFunc(int num)
    {
        if (UIController.instance.curOpenPageNum == -4)
        {
            goHome();
        }
        else
        {
            UIController.instance.PageObjArr[UIController.instance.curOpenPageNum].SetActive(false);
            UIController.instance.curOpenPageNum = num;
        }
    }
    public void CloseTitlePopUp()
    {
        UIController.instance.curOpenPageNum = 4;
    }

    //로그아웃
    public void OpenDialogLogout()
    {
        Dialog_Logout.SetActive(true);
        UIController.instance.curOpenPageNum = -3;
    }
    public void CloseDialogLogout()
    {
        Dialog_Logout.SetActive(false);
        UIController.instance.curOpenPageNum = 0;
    }
    public void LetSignOut()
    {
        GoogleSigninManager.SignOut();
    }
}