using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

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

    //버튼
    public Sprite Button_Disabled;
    public Sprite Button_Enabled;

    //사원증 이미지
    public GameObject idCard;
    public Image userProfileImg;
    public Sprite[] ProfileImgs;
    int profileNumber;

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
    string userDetailJob;

    //사용 컬러
    Color primary1;
    Color primary3;
    Color gray300;
    Color gray500;
    Color errorColor;

    private void Start()
    {
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#DDE0E3", out gray300);
        ColorUtility.TryParseHtmlString("#949CA8", out gray500);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);
        setIdCard();
        colorNumber = UserManager.Instance.newUserInformation.idCardColorNumber;
        profileNumber = UserManager.Instance.newUserInformation.userProfileImgNumber;
        changeCardColor();

        userJob = UserManager.Instance.newUserInformation.kindOfJob;
        userDetailJob = UserManager.Instance.newUserInformation.detailJob;
    }

    private void Update()
    {
        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) goHome();
    }

    //사원증 세팅
    public void setIdCard()
    {
        //GameObject idCard = IdCardSection.transform.GetChild(1).gameObject;

        //프로필 이미지
        userProfileImg.sprite = ProfileImgs[profileNumber];

        //목표 회사
        idCard.transform.GetChild(1).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.companyName;

        //setTitle(idCard.transform.GetChild(2).gameObject, UserManager.Instance.newUserInformation.userTitleModi, UserManager.Instance.newUserInformation.userTitleNoun);//칭호
        idCard.transform.GetChild(2).GetComponent<TMP_Text>().text =
            UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        idCard.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName; //유저 이름
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
    }
    #endregion

    #region 사원증 수정
    public GameObject jobContent;
    //사원증 수정 페이지 세팅
    public void setEditIdCard()
    {
        EditIdCardPage.SetActive(true);
        editIdCardAlert.SetActive(false);
        //사원증 프로필 사진 수정
        EditIdCardPage.transform.GetChild(0).GetComponent<Image>().sprite = ProfileImgs[profileNumber];

        //이름
        EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = UserManager.Instance.newUserInformation.userName;
        EditIdCardPage.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text.Length.ToString() + "/10";
        //목표기업명
        EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text = UserManager.Instance.newUserInformation.companyName;
        EditIdCardPage.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text =
            EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text.Length.ToString() + "/10";
        //직군직무
        EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text =
            Jobs[userJob] + " · " + UserManager.Instance.newUserInformation.detailJob;

        //setTitle(EditIdCardPage.transform.GetChild(4).GetChild(1).GetChild(0).gameObject, UserManager.Instance.newUserInformation.userTitleModi, UserManager.Instance.newUserInformation.userTitleNoun);
        EditIdCardPage.transform.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text =
            UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
    }

    #region 사원증 프로필 이미지 선택
    public void setEditProfileImg()
    {
        editProfileImg.SetActive(true);
        if (profileNumber == 0) { return; }
        else
        {
            editProfileImg.transform.GetChild(profileNumber-1).GetChild(0).gameObject.SetActive(true);
        }
    }
    public void ChangeProfileImg()
    {
        Image nowProfile = EventSystem.current.currentSelectedGameObject.GetComponent<Image>();
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
        //if(jobContent.transform.FindChild(userJob)) //find로 찾는 방법 - 한글을 써야함
        for (int i = 0; i < 5; i++)
        {
            for (int ii = 1; ii < 5; ii++)
            {
                if (jobContent.transform.GetChild(i).GetChild(ii).GetChild(1).GetComponent<TMP_Text>().text == UserManager.Instance.newUserInformation.detailJob)
                {
                    jobContent.transform.GetChild(i).GetChild(ii).GetComponent<Toggle>().isOn = true;
                    return;
                }
            }
        }
    }
    //직군/직무 선택할때 반영
    public void ChangeJob()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;

        if (currentObj.GetComponent<Toggle>() == null) return;
        currentObj.GetComponent<Toggle>().isOn = true;

        userDetailJob = currentObj.transform.GetChild(1).GetComponent<TMP_Text>().text;
        string jobGroup = currentObj.transform.parent.name;
        if (jobGroup == "PlannerGroup") userJob = 0;
        else if (jobGroup == "DesignerGroup") userJob = 1;
        else if (jobGroup == "FrontEndGroup") userJob = 2;
        else if (jobGroup == "BackEndGroup") userJob = 3;
        else if (jobGroup == "DataGroup") userJob = 4;

        if (userDetailJob != UserManager.Instance.newUserInformation.detailJob)
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
        }
    }
    //직군/직무 저장
    public void SaveChangeJob()
    {
        EditIdCardPage.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = Jobs[userJob] + " · " + userDetailJob;
        SetUserJobPage.SetActive(false);
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
    }
    public void checkInput()
    {
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
        if(UserManager.Instance.newUserInformation.userProfileImgNumber != profileNumber||
            UserManager.Instance.newUserInformation.userName != EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text||
            UserManager.Instance.newUserInformation.companyName != EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text||
            UserManager.Instance.newUserInformation.detailJob != userDetailJob||
            !string.IsNullOrWhiteSpace(UserManager.Instance.selectedModi)||
            !string.IsNullOrWhiteSpace(UserManager.Instance.selectedNoun))
        {
            editIdCardAlert.SetActive(true);
        }
        else { EditIdCardPage.SetActive(false); }

    }
    //사원증 수정 내역 저장
    public void SaveIdCard()
    {
        if (string.IsNullOrWhiteSpace(EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text))
        {
            return;
        }
        UserManager.Instance.newUserInformation.userProfileImgNumber = profileNumber;

        UserManager.Instance.newUserInformation.userName= EditIdCardPage.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text;
        UserManager.Instance.newUserInformation.companyName= EditIdCardPage.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>().text;

        UserManager.Instance.newUserInformation.kindOfJob = userJob;
        UserManager.Instance.newUserInformation.detailJob = userDetailJob;

        if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedModi))
        { UserManager.Instance.newUserInformation.userTitleModi = UserManager.Instance.selectedModi; }
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedNoun))
        { UserManager.Instance.newUserInformation.userTitleNoun = UserManager.Instance.selectedNoun; }

        if (UserManager.Instance.newUserInformation.titleCheck[28] == 0)
        {
            UserManager.Instance.newUserInformation.titleCheck[28]++;
            UserManager.Instance.checkTitle("앞", 26, 28);
            UserManager.Instance.getTitle = 1;
        }

        EditIdCardPage.SetActive(false);
        setIdCard();
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

    //기록 상세페이지
    public void clickRecord()
    {
        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        UserManager.Instance.pushedButton = clickButton.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text;
        UserManager.Instance.pushedRecord = clickButton.transform.GetChild(1).GetComponent<TMP_Text>().text;
        DontDestroyCanvas.setRecord();
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goRanking() { SceneManager.LoadScene("5_Ranking"); }
}