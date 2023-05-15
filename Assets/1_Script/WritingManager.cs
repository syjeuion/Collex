using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Newtonsoft.Json;

public class WritingManager : MonoBehaviour
{
    MakeNewProject thisProject = new MakeNewProject();
    DailyRecord newDailyRecord = new DailyRecord();

    #region 게임 오브젝트
    public GameObject selectedCapabilitiesPage;
    public GameObject writingPage;
    public GameObject selectFolderPage;
    public GameObject experiencePage;
    public GameObject cancelWritingPage;
    public GameObject templatesPage;

    public RectTransform writingContent;
    public GameObject writingArea;
    public GameObject writing_Problem;
    public GameObject writing_Cause;
    public GameObject writing_Solution;
    public GameObject writing_Learning;
    public GameObject writing_Goodpoint;
    public GameObject writing_Badpoint;

    [SerializeField] TMP_Text todayDate;

    public TMP_InputField inputTitle;
    public TMP_InputField inputPractice;
    public TMP_InputField inputProblem;
    public TMP_InputField inputCause;
    public TMP_InputField inputSolution;
    public TMP_InputField inputLearning;
    public TMP_InputField inputGoodpoint;
    public TMP_InputField inputBadpoint;

    public GameObject Capabilities;
    public GameObject Hashtags;
    int forcount;

    public GameObject experiencePageConfirmButton;
    public Sprite button_disable;
    public Sprite button_enable;

    public GameObject writingNextButton;

    public GameObject snackBar;

    //bool whenModify;
    //DailyRecord modifyRecord;
    #endregion

    //사용 컬러
    Color primary1;
    Color primary3;
    Color gray_200;
    Color gray_300;
    Color gray_700;
    Color gray_500;
    Color gray_900;
    Color errorColor;

    //활동폴더 불러오기
    private void Start()
    {
        //컬러 지정
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#EBEDEF", out gray_200);
        ColorUtility.TryParseHtmlString("#DDE0E3", out gray_300);
        ColorUtility.TryParseHtmlString("#575F6B", out gray_700);
        ColorUtility.TryParseHtmlString("#949CA8", out gray_500);
        ColorUtility.TryParseHtmlString("#1E2024", out gray_900);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);


        if (UserManager.Instance.pushedButton !=null&& UserManager.Instance.pushedButton != "")
        {
            string thisFolder = UserManager.Instance.pushedButton;
            string thisFolderDatas = UserManager.Instance.folders[thisFolder];
            thisProject = JsonConvert.DeserializeObject<MakeNewProject>(thisFolderDatas);
        }
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.pushedRecord)) setModify();
    }

    #region 기록 수정하기
    public void setModify()
    {
        templatesPage.SetActive(false);
        writingPage.transform.GetChild(2).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "저장";
        writingPage.transform.GetChild(2).GetChild(3).GetChild(0).GetComponent<TMP_Text>().color = gray_900;

        writingContent.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;

        string clickedRecordSTR = thisProject.records[UserManager.Instance.pushedRecord];
        newDailyRecord = JsonConvert.DeserializeObject<DailyRecord>(clickedRecordSTR);

        selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().text = thisProject.projectTitle;
        inputTitle.text = newDailyRecord.title;
        //todayDate.text = newDailyRecord.date.Year+"년 "+ newDailyRecord.date.Month+"월 "+ newDailyRecord.date.Day+"일";
        todayDate.text = newDailyRecord.date;
        if (newDailyRecord.experiences.Count > 0) startCoroutine();

        inputPractice.text = newDailyRecord.writings["활동내용"]; 
        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["문제상황"])){ writing_Problem.SetActive(true);inputProblem.text = newDailyRecord.writings["문제상황"]; }
        else writing_Problem.SetActive(false);

        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["문제원인"])) { writing_Cause.SetActive(true); inputCause.text = newDailyRecord.writings["문제원인"]; }
        else writing_Cause.SetActive(false);

        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["해결과정"])) { writing_Solution.SetActive(true); inputSolution.text = newDailyRecord.writings["해결과정"]; }
        else writing_Solution.SetActive(false);

        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["잘한점"])) { writing_Goodpoint.SetActive(true); inputGoodpoint.text = newDailyRecord.writings["잘한점"]; }
        else writing_Goodpoint.SetActive(false);

        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["부족한점"])) { writing_Badpoint.SetActive(true); inputBadpoint.text = newDailyRecord.writings["부족한점"]; }
        else writing_Badpoint.SetActive(false);

        if (!string.IsNullOrWhiteSpace(newDailyRecord.writings["배운점"])) { writing_Learning.SetActive(true); inputLearning.text = newDailyRecord.writings["배운점"]; }
        else writing_Learning.SetActive(false);
    }

    public void backButton()
    {
        if (!string.IsNullOrWhiteSpace(UserManager.Instance.pushedRecord)) cancelWritingPage.SetActive(true);
        else templatesPage.SetActive(true);
    }

    public void nextButton()
    {
        if (string.IsNullOrWhiteSpace(inputTitle.text))
        {
            inputTitle.GetComponent<Image>().color = errorColor;
            inputTitle.transform.GetChild(1).GetComponent<TMP_Text>().text = "기록문서의 이름을 정해주세요.";
            return;
        }
        if (string.IsNullOrWhiteSpace(inputPractice.text))
        {
            inputPractice.GetComponent<Image>().color = errorColor;
            return;
        }
        if(!string.IsNullOrWhiteSpace(inputTitle.text)&& !string.IsNullOrWhiteSpace(inputPractice.text))
        {
            if (!string.IsNullOrWhiteSpace(UserManager.Instance.pushedRecord)) SaveDailyRecord();
            else { writingPage.SetActive(false); selectedCapabilitiesPage.SetActive(true); }//checkTitle();
        }
        //checkTitle();
    }

    #endregion

    //int clickCount = 0;
    private void Update()
    {
        //spacing 맞추기
        //writingArea.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        //writingArea.GetComponent<VerticalLayoutGroup>().spacing = 20f;
        //writingContent.GetComponent<VerticalLayoutGroup>().spacing = 20f;

        if (Input.GetKey(KeyCode.KeypadEnter)|| Input.GetKey(KeyCode.Delete)) { checkHeight(); }

        //안드로이드 뒤로가기
        if (Input.GetKey(KeyCode.Escape))
        {
            cancelWritingPage.SetActive(true);
        }
        //if (clickCount == 2) { Application.Quit(); }두번 눌렀을때 종료?
    }
    //void doubleClick() { clickCount = 0; } 종료..인데 어떻게 하는건지 모르겠다

    //글쓰기에서 나갈때
    public void escWritingPage()
    {
        if (UserManager.Instance.pushedButton == null || UserManager.Instance.pushedButton == "")
        { SceneManager.LoadScene("1_Home"); }
        else { goFolder(); }
    }

    #region 템플릿 선택
    //템플릿 저장
    string temporaryTemplate;
    public void NextOfTemplate()
    {
        StartCoroutine(templateSetting());

        if(UserManager.Instance.pushedButton == null || UserManager.Instance.pushedButton == "")
        {
            selectFolderPage.SetActive(true);
            setSelectFolderBottom();
        }
        else
        {
            newDailyRecord.template = EventSystem.current.currentSelectedGameObject.name;
            selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().text = UserManager.Instance.pushedButton;
            selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_900;
        }
    }

    IEnumerator templateSetting()
    {
        temporaryTemplate = EventSystem.current.currentSelectedGameObject.name;
        if (temporaryTemplate == "기본형")
        {
            writingArea.SetActive(false);
        }
        else if (temporaryTemplate == "문제기록형")
        {
            writing_Problem.SetActive(true);
            writing_Cause.SetActive(true);
            writing_Solution.SetActive(true);
            writing_Learning.SetActive(true);
            writing_Goodpoint.SetActive(false);
            writing_Badpoint.SetActive(false);
        }
        else if (temporaryTemplate == "자기칭찬형")
        {
            writing_Problem.SetActive(false);
            writing_Cause.SetActive(false);
            writing_Solution.SetActive(false);
            writing_Goodpoint.SetActive(true);
            writing_Learning.SetActive(true);
            writing_Badpoint.SetActive(false);
        }
        else if (temporaryTemplate == "자기반성형")
        {
            writing_Problem.SetActive(true);
            writing_Cause.SetActive(true);
            writing_Solution.SetActive(false);
            writing_Badpoint.SetActive(true);
            writing_Learning.SetActive(true);
            writing_Badpoint.SetActive(false);
        }
        yield return new WaitForEndOfFrame();
        writingContent.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        //writingContent.GetComponent<VerticalLayoutGroup>().spacing = 20;
    }
    #endregion

    //폴더 선택
    #region 폴더 선택
    public GameObject selectFolderButton;
    public GameObject selectFolderContent;
    public GameObject selectFolderPrefab;
    GameObject newSelectFolder;
    //바텀시트 폴더 리스트 세팅
    public void setSelectFolderBottom()
    {
        for(int i=0;i< selectFolderContent.transform.childCount; i++)
        {
            Destroy(selectFolderContent.transform.GetChild(i).gameObject);
        }

        int ii = 0;
        foreach(string key in UserManager.Instance.folders.Keys)
        {
            //진행중인 폴더인지 확인
            thisProject = JsonConvert.DeserializeObject<MakeNewProject>(UserManager.Instance.folders[key]);
            if (thisProject.isItOngoing)
            {
                ii += 1;
                newSelectFolder = Instantiate(selectFolderPrefab, selectFolderContent.transform);
                newSelectFolder.transform.SetAsFirstSibling();
                newSelectFolder.transform.GetChild(0).GetComponent<TMP_Text>().text = key;
                newSelectFolder.GetComponent<Button>().onClick.AddListener(selectFolder);
            }
            RectTransform contentRect = selectFolderContent.transform.parent.GetComponent<RectTransform>();
            if (ii > 3) contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 56);
        }
        selectFolderPage.SetActive(true);
    }
    //바텀시트 선택된 폴더 정보 저장
    public void selectFolder()
    {
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        UserManager.Instance.pushedButton = currentObject.transform.GetChild(0).GetComponent<TMP_Text>().text;

        string thisFolder = UserManager.Instance.pushedButton;
        string thisFolderDatas = UserManager.Instance.folders[thisFolder];
        thisProject = JsonConvert.DeserializeObject<MakeNewProject>(thisFolderDatas);

        selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().text = thisFolder;
        selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_900;

        selectFolderPage.SetActive(false);
    }
    #endregion

    //글쓰기 다음 버튼 누를때 제목 체크
    #region 제목 예외처리
    /*public void checkTitle()
    {
        if (string.IsNullOrWhiteSpace(inputTitle.text))
        {
            inputTitle.GetComponent<Image>().color = errorColor;
            inputTitle.transform.GetChild(1).GetComponent<TMP_Text>().text = "기록문서의 이름을 정해주세요.";
            return;
        }
        //if (string.IsNullOrWhiteSpace(selectFolderButton.transform.GetChild(0).GetComponent<TMP_Text>().text)) { print("folder name"); return; }
        
        writingPage.SetActive(false);
    }*/
    //제목 실시간 체크
    public void inputValueChanged()
    {
        if (inputTitle.text== UserManager.Instance.pushedRecord) return;
        
        if (thisProject.records.ContainsKey(inputTitle.text))
        {
            inputTitle.GetComponent<Image>().color = errorColor;
            inputTitle.transform.GetChild(1).GetComponent<TMP_Text>().text = "중복되는 이름의 기록문서가 있어요.";
            //writingNextButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            inputTitle.GetComponent<Image>().color = primary3;
            inputTitle.transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            //writingNextButton.GetComponent<Button>().interactable = true;
        }

        if (!string.IsNullOrWhiteSpace(inputTitle.text) && !string.IsNullOrWhiteSpace(inputPractice.text))
        {
            //writingNextButton.GetComponent<Button>().interactable = true;
            writingNextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_900;
        }
        else writingNextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_200;
    }
    public void inputDeselected()
    {
        if (inputTitle.text == UserManager.Instance.pushedRecord) return;
        if (thisProject.records.ContainsKey(inputTitle.text))
        {
            inputTitle.GetComponent<Image>().color = errorColor;
        }
        else
        {
            inputTitle.GetComponent<Image>().color = gray_300;
        }
    }
    #endregion

    #region Writing InputField
    //인풋필드 클릭 시 상단에 위치 고정시키기
    float height;
    RectTransform parentRect;
    GameObject thisField;
    //인풋 OnSelect
    public void OnSelectWritingInput()
    {
        height = 0;
        parentRect = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<RectTransform>();
        thisField = EventSystem.current.currentSelectedGameObject;
        StartCoroutine(OnSelectInput());
    }
    IEnumerator OnSelectInput()
    {
        writingContent.GetComponent<VerticalLayoutGroup>().padding.bottom = 454;
        yield return new WaitForEndOfFrame();
        if (thisField.name == "InputField_Practice")
        { writingContent.anchoredPosition = new Vector2(writingContent.anchoredPosition.x, 373f); }
        else
        {
            for (int i = 0; i < thisField.transform.parent.GetSiblingIndex(); i++)
            {
                if (writingArea.transform.GetChild(i).gameObject.activeSelf)
                { height += writingArea.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y+20; }
            }
            yield return new WaitForEndOfFrame();
            writingContent.anchoredPosition = new Vector2(writingContent.anchoredPosition.x, (393+inputPractice.transform.parent.GetComponent<RectTransform>().sizeDelta.y + height));
        }
    }
    //인풋 Deselect
    public void OnDeselectWritingInput()
    {
        writingContent.GetComponent<VerticalLayoutGroup>().padding.bottom = 44;
    }
    //글쓰기 실시간 높이 값 반영
    public void checkHeight()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
        StartCoroutine(writingAreaSpacing());

        if (!string.IsNullOrWhiteSpace(inputTitle.text) && !string.IsNullOrWhiteSpace(inputPractice.text))
        {
            //writingNextButton.GetComponent<Button>().interactable = true;
            writingNextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_900;
        }
        else writingNextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_200;
    }
    float inputTextHeight;
    IEnumerator writingAreaSpacing()
    {
        if (thisField.GetComponent<TMP_InputField>() != null)
        {
            thisField.GetComponent<Image>().color = gray_300;

            parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, 30f + thisField.GetComponent<RectTransform>().sizeDelta.y);
            print(thisField.GetComponent<RectTransform>().sizeDelta.y);
            print(parentRect.sizeDelta.y);
            /*if (selectedField.name == "Input_Practice")
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 76f + (float)selectedField.GetComponent<RectTransform>().sizeDelta.y);
            else
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 16f + (float)selectedField.GetComponent<RectTransform>().sizeDelta.y);*/
            yield return new WaitForEndOfFrame();
        }
        //yield return new WaitForEndOfFrame();
        //writingArea.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        //writingArea.GetComponent<VerticalLayoutGroup>().spacing = 20;
    }
    #endregion

    #region 기록문서 저장
    public void SaveDailyRecord()
    {
        newDailyRecord.title = inputTitle.text;
        newDailyRecord.date = todayDate.text;
        //newDailyRecord.date = UserManager.Instance.calendarDate;
        newDailyRecord.writings["활동내용"] = inputPractice.text;

        if (!string.IsNullOrWhiteSpace(inputProblem.text))
        {
            newDailyRecord.writings["문제상황"] = inputProblem.text; thisProject.writings["문제상황"]++;
            UserManager.Instance.newUserInformation.titleCheck[17]++;
            UserManager.Instance.checkTitle("앞", 16, 17);
            UserManager.Instance.checkTitle("뒤", 17, 17);
        }
        if (!string.IsNullOrWhiteSpace(inputCause.text))
        {
            newDailyRecord.writings["문제원인"] = inputCause.text; thisProject.writings["문제원인"]++;
            UserManager.Instance.newUserInformation.titleCheck[19]++;
            UserManager.Instance.checkTitle("앞", 18, 19);
            UserManager.Instance.checkTitle("뒤", 19, 19);
        }
        if (!string.IsNullOrWhiteSpace(inputSolution.text))
        {
            newDailyRecord.writings["해결과정"] = inputSolution.text; thisProject.writings["해결과정"]++;
            UserManager.Instance.newUserInformation.titleCheck[18]++;
            UserManager.Instance.checkTitle("앞", 17, 18);
            UserManager.Instance.checkTitle("뒤", 18, 18);
        }
        if (!string.IsNullOrWhiteSpace(inputLearning.text))
        {
            newDailyRecord.writings["배운점"] = inputLearning.text; thisProject.writings["배운점"]++;
            UserManager.Instance.newUserInformation.titleCheck[22]++;
            UserManager.Instance.checkTitle("앞", 21, 22);
            UserManager.Instance.checkTitle("뒤", 22, 22);
        }
        if (!string.IsNullOrWhiteSpace(inputGoodpoint.text))
        {
            newDailyRecord.writings["잘한점"] = inputGoodpoint.text; thisProject.writings["잘한점"]++;
            UserManager.Instance.newUserInformation.titleCheck[20]++;
            UserManager.Instance.checkTitle("앞", 19, 20);
            UserManager.Instance.checkTitle("뒤", 20, 20);
        }
        if (!string.IsNullOrWhiteSpace(inputBadpoint.text))
        {
            newDailyRecord.writings["부족한점"] = inputBadpoint.text; thisProject.writings["부족한점"]++;
            UserManager.Instance.newUserInformation.titleCheck[21]++;
            UserManager.Instance.checkTitle("앞", 20, 21);
            UserManager.Instance.checkTitle("뒤", 21, 21);
        }

        forcount = 5; saveToggle(Capabilities);
        forcount = 12; saveToggle(Hashtags);

        //선택된 경험 폴더 딕셔너리에 저장
        if (newDailyRecord.experiences.Count != 0)
        {
            for (int i = 0; i < newDailyRecord.experiences.Count; i++)
            {
                saveToDctionary(0, newDailyRecord.experiences[i]);
                saveToDctionary(1, newDailyRecord.experiences[i]);
            }
            UserManager.Instance.newUserInformation.titleCheck[14]++;
            UserManager.Instance.checkTitle("앞", 11, 14);
            UserManager.Instance.checkTitle("앞", 12, 14);
            UserManager.Instance.checkTitle("앞", 13, 14);
        }
        

        //폴더에 기록 저장
        string newDailyRecordJSON = JsonConvert.SerializeObject(newDailyRecord);

        if (newDailyRecord.title == UserManager.Instance.pushedRecord) //기록 수정하기에서 제목 안바꿨을때
            thisProject.records[newDailyRecord.title] = newDailyRecordJSON;
        else if (!string.IsNullOrWhiteSpace(UserManager.Instance.pushedRecord)) //기록 수정하기에서 제목 바꿨을때
        {
            thisProject.records.Remove(UserManager.Instance.pushedRecord);
            UserManager.Instance.pushedRecord = newDailyRecord.title;
            thisProject.records.Add(newDailyRecord.title, newDailyRecordJSON);
        }
        else //일반 기록
            thisProject.records.Add(newDailyRecord.title, newDailyRecordJSON);

        //칭호 획득 확인
        if(thisProject.records.Count == UserManager.Instance.newUserInformation.titleCheck[6] + 1)
        {
            UserManager.Instance.newUserInformation.titleCheck[6]++; //한 폴더 내 기록 count

            UserManager.Instance.checkTitle("앞", 5, 6);
            UserManager.Instance.checkTitle("앞", 6, 6);
            UserManager.Instance.checkTitle("뒤", 5, 6);
            UserManager.Instance.checkTitle("뒤", 6, 6);
        }
        UserManager.Instance.newUserInformation.titleCheck[7]++; //기록 작성 count
        UserManager.Instance.checkTitle("앞", 7, 7);
        UserManager.Instance.checkTitle("앞", 8, 7);
        for(int i = 7; i < 11; i++)
        {
            UserManager.Instance.checkTitle("뒤", i, 7);
        }
        if (UserManager.Instance.nowGetTitle.Count != 0) UserManager.Instance.getTitle = 1;

        //최종 작성일 저장
        thisProject.lastRecordDate = DateTime.Now;
        //thisProject.lastRecordYear = DateTime.Now.Year;
        //thisProject.lastRecordMonth = DateTime.Now.Month;
        //thisProject.lastRecordDay = DateTime.Now.Day;
        //폴더 다시 JSON으로 변환
        string newFolderData = JsonConvert.SerializeObject(thisProject);
        UserManager.Instance.folders[thisProject.projectTitle] = newFolderData;
        goFolder();
    }

    //딕셔너리에 저장하는 함수
    void saveToDctionary(int check, string key)
    {
        Dictionary<string, int> thisDictionary;
        if (check == 0)
        { thisDictionary = thisProject.experiences; }
        else if(check==1)
        { thisDictionary = UserManager.Instance.AllExperiences; }
        else //if(check==2)
        { thisDictionary = UserManager.Instance.AllHashtags; }

        if (thisDictionary.ContainsKey(key))
            thisDictionary[key]++;
        else
            thisDictionary.Add(key, 1);

        if (check == 1)
        {
            if(thisDictionary[key]== UserManager.Instance.newUserInformation.titleCheck[15] + 1)
            {
                UserManager.Instance.newUserInformation.titleCheck[15]++;
                UserManager.Instance.checkTitle("앞", 15, 15);
            }
            if (thisDictionary.Count == 12 && UserManager.Instance.newUserInformation.titleCheck[16]==0)
            {
                UserManager.Instance.newUserInformation.titleCheck[16]++;
                UserManager.Instance.checkTitle("뒤", 16, 16);
            }
        }
    }
    #endregion

    //토글 클릭할때 실행되는 함수
    #region 토글
    public void clickCapa()
    {
        forcount = 5;
        checkingToggle(Capabilities);
    }

    public void clickHash()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        if (currentObj.GetComponent<Toggle>().isOn)
        {
            currentObj.transform.GetChild(1).GetComponent<TMP_Text>().color = primary3;
        }
        else
        {
            currentObj.transform.GetChild(1).GetComponent<TMP_Text>().color = gray_500;
        }
        forcount = 12;
        checkingToggle(Hashtags);
    }

    //토글 3회 제한하는 함수
    public void checkingToggle(GameObject toggles)
    {
        int countToggle = 0;
        for (int i = 0; i < forcount; i++)
        {
            if (toggles.transform.GetChild(i).GetComponent<Toggle>().isOn == true)
            {
                countToggle++;
                if (countToggle > 3)
                {
                    EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>().isOn = false;
                    StartCoroutine(setSnackBar());
                    return;
                }
            }
        }
    }

    IEnumerator setSnackBar()
    {
        if (selectedCapabilitiesPage.activeSelf)
        {
            snackBar.transform.GetChild(0).GetComponent<TMP_Text>().text = "역량은 최대 3개까지만 선택이 가능합니다.";
        }
        else { snackBar.transform.GetChild(0).GetComponent<TMP_Text>().text = "해시태그는 최대 3개까지만 선택이 가능합니다."; }
        snackBar.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        snackBar.SetActive(false);
    }

    //토글 저장
    int thisType;
    public void saveToggle(GameObject toggles)
    {
        int n = 0;
        for (int i = 0; i < forcount; i++)
        {
            if (toggles.transform.GetChild(i).GetComponent<Toggle>().isOn == true)
            {
                string thisName = toggles.transform.GetChild(i).name;
                if (toggles.name == "Capabilities")
                {
                    thisProject.capabilities[thisName]++;
                    UserManager.Instance.Allcapabilites[thisName]++;
                    newDailyRecord.capabilities[n] = thisName;

                    if (thisName == "통찰력") { thisType = 9; }
                    if (thisName == "리더십") thisType = 10;
                    if (thisName == "팀워크") thisType = 11;
                    if (thisName == "문제해결능력") thisType = 12;
                    if (thisName == "커뮤니케이션능력") thisType = 13;
                    UserManager.Instance.newUserInformation.titleCheck[thisType]++;
                    UserManager.Instance.checkTitle("뒤", thisType+2, thisType);

                    if (UserManager.Instance.newUserInformation.titleCheck[8] == 0)
                    {
                        UserManager.Instance.newUserInformation.titleCheck[8]++;
                        UserManager.Instance.checkTitle("앞", 9, 8);
                    }
                    if (n == 2)
                    {
                        if (UserManager.Instance.newUserInformation.titleCheck[29] == 0)
                        {
                            UserManager.Instance.newUserInformation.titleCheck[29]++;
                            UserManager.Instance.checkTitle("앞", 10, 29);
                        }
                    }
                    n++;
                }
                if (toggles.name == "Hashtags")
                {
                    saveToDctionary(2, thisName);
                    newDailyRecord.hashtags[n] = thisName;

                    if (UserManager.Instance.newUserInformation.titleCheck[15] == 0)
                    {
                        UserManager.Instance.newUserInformation.titleCheck[15]++;
                        UserManager.Instance.checkTitle("앞", 14, 15);
                    }

                    n++;
                }
            }
        }
    }
    #endregion

    //경험칩 저장
    #region 경험칩
    List<string> deletedEXchip = new List<string>();
    //List<GameObject> selectedExChip = new List<GameObject>();
    /*public void setEXchips()
    {
        experiencePage.SetActive(true);
        GameObject selectedEX;
        for (int i = 0; i < deletedEXchip.Count; i++)
        {
            selectedEX = experiencePage.transform.Find("planners/서비스기획/" + deletedEXchip[i]).gameObject;
            selectedEX.GetComponent<Toggle>().isOn = false;
        }
    }*/

    public void ExperienceUpdate(bool value)
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        if (currentObj.name != "SelectExperience_Button" && currentObj.name != "Image")
        {
            if (currentObj.GetComponent<Toggle>().isOn == true && !newDailyRecord.experiences.Contains(currentObj.transform.GetChild(0).GetComponent<TMP_Text>().text))
            {
                //selectedExChip.Add(currentObj);
                newDailyRecord.experiences.Add(currentObj.transform.GetChild(0).GetComponent<TMP_Text>().text);
                currentObj.GetComponent<Image>().color = primary3;
                currentObj.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
                if (newDailyRecord.experiences.Count != 0)
                {
                    experiencePageConfirmButton.GetComponent<Image>().sprite = button_enable;
                    experiencePageConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = primary1;
                }
            }
            if (currentObj.GetComponent<Toggle>().isOn == false && newDailyRecord.experiences.Contains(currentObj.transform.GetChild(0).GetComponent<TMP_Text>().text))
            {
                //selectedExChip.Remove(currentObj);
                newDailyRecord.experiences.Remove(currentObj.transform.GetChild(0).GetComponent<TMP_Text>().text);
                currentObj.GetComponent<Image>().color = gray_300;
                currentObj.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_700;
                if (newDailyRecord.experiences.Count == 0)
                {
                    experiencePageConfirmButton.GetComponent<Image>().sprite = button_disable;
                    experiencePageConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_500;
                }
            }
        }
    }

    //경험 칩 로드
    public GameObject Experiences;
    public GameObject selectedEX;
    GameObject newExperience;

    public GameObject selectExButton;
    public GameObject HorizontalGroupPrefab;
    GameObject newHorizontalGroupObj;
    public void startCoroutine()
    {
        StartCoroutine(saveExperience());
    }
    
    IEnumerator saveExperience()
    {
        //기존 칩 제거
        for (int i = 1; i < Experiences.transform.childCount-1; i++)
        {
            Destroy(Experiences.transform.GetChild(i).gameObject);
        }

        //새 칩 로드
        for (int i = 0; i < newDailyRecord.experiences.Count; i++)
        {
            if (i == 0) newHorizontalGroupObj = Instantiate(HorizontalGroupPrefab, Experiences.transform);
            
            newExperience = Instantiate(selectedEX, newHorizontalGroupObj.transform);
            //newExperience.name = i + "_";
            newExperience.transform.GetChild(0).GetComponent<TMP_Text>().text = newDailyRecord.experiences[i];
            newExperience.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(removeEXchip);
            yield return new WaitForEndOfFrame();

            if (newHorizontalGroupObj.GetComponent<RectTransform>().sizeDelta.x >= 195)
            {
                newHorizontalGroupObj = Instantiate(HorizontalGroupPrefab, Experiences.transform);
                newExperience.transform.SetParent(newHorizontalGroupObj.transform);
            }
            
        }
        selectExButton.transform.SetAsLastSibling();
        yield return new WaitForEndOfFrame();

        if (newHorizontalGroupObj == null) { yield break; }
        newHorizontalGroupObj.GetComponent<HorizontalLayoutGroup>().spacing = 7.9f;
        newHorizontalGroupObj.GetComponent<HorizontalLayoutGroup>().spacing = 8;
        Experiences.GetComponent<VerticalLayoutGroup>().spacing = 7.9f;
        Experiences.GetComponent<VerticalLayoutGroup>().spacing = 8;

        experiencePage.SetActive(false);
    }
    //경험칩 제거 함수
    void removeEXchip()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        newDailyRecord.experiences.Remove(button.transform.parent.GetChild(0).GetComponent<TMP_Text>().text);
        deletedEXchip.Add(button.transform.parent.GetChild(0).GetComponent<TMP_Text>().text);
        /*for(int i = 0; i < selectedExChip.Count; i++)
        {
            if(selectedExChip[i].transform.GetChild(0).GetComponent<TMP_Text>().text == button.transform.parent.GetChild(0).GetComponent<TMP_Text>().text)
            {
                selectedExChip[i].GetComponent<Toggle>().isOn = false;
            }
        }*/
        if (button.transform.parent.parent.childCount == 1) { Destroy(button.transform.parent.parent.gameObject); }
        else { Destroy(button.transform.parent.gameObject); }
    }
    #endregion

    //inputField 편집
    #region 인풋 필드 편집
    public GameObject addInputFieldContainer;
    GameObject activeObject;

    public Sprite plusIcon;
    public Sprite deleteIcon;
    //이미 활성화된 토글 on
    public void openAddInput()
    {
        addInputFieldContainer.transform.parent.gameObject.SetActive(true);
        for(int i = 0; i < 6; i++)
        {
            activeObject = addInputFieldContainer.transform.GetChild(i + 1).gameObject;
            if (writingArea.transform.GetChild(i).gameObject.activeSelf)
            {
                activeObject.GetComponent<Toggle>().isOn = true;
                activeObject.GetComponent<Image>().color = primary3;
                activeObject.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
                activeObject.transform.GetChild(1).GetComponent<Image>().sprite = deleteIcon;
            }
            else
            {
                activeObject.GetComponent<Toggle>().isOn = false;
                activeObject.GetComponent<Image>().color = gray_300;
                activeObject.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_700;
                activeObject.transform.GetChild(1).GetComponent<Image>().sprite = plusIcon;
            }
        }
    }
    //칩 선택 시 반영
    public void changeChipColor(bool check)
    {
        GameObject selectedChip = EventSystem.current.currentSelectedGameObject;
        //print(selectedChip.name);
        if(selectedChip.name != "Button_AddInputField")
        {
            if (selectedChip.GetComponent<Toggle>().isOn ==true)
            {
                selectedChip.GetComponent<Image>().color = primary3;
                selectedChip.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
                selectedChip.transform.GetChild(1).GetComponent<Image>().sprite = deleteIcon;
            }
            if (selectedChip.GetComponent<Toggle>().isOn ==false)
            {
                selectedChip.GetComponent<Image>().color = gray_300;
                selectedChip.transform.GetChild(0).GetComponent<TMP_Text>().color = gray_700;
                selectedChip.transform.GetChild(1).GetComponent<Image>().sprite = plusIcon;
            }
            
        }
    }
    //추가완료 버튼
    int count;
    public void comfirmAddInput()
    {
        StartCoroutine(AddInput());
    }
    IEnumerator AddInput()
    {
        count = 0;
        for (int i = 0; i < 6; i++)
        {
            GameObject activeToggle = addInputFieldContainer.transform.GetChild(i + 1).gameObject;
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "문제상황")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Problem.SetActive(true); count++; }
                else { writing_Problem.SetActive(false); inputProblem.text = ""; }
            }
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "문제원인")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Cause.SetActive(true); count++; }
                else { writing_Cause.SetActive(false); inputCause.text = ""; }
            }
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "해결과정")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Solution.SetActive(true); count++; }
                else { writing_Solution.SetActive(false); inputSolution.text = ""; }
            }
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "잘한점")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Goodpoint.SetActive(true); count++; }
                else { writing_Goodpoint.SetActive(false); inputGoodpoint.text = ""; }
            }
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "부족한점")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Badpoint.SetActive(true); count++; }
                else { writing_Badpoint.SetActive(false); inputBadpoint.text = ""; }
            }
            if (activeToggle.transform.GetChild(0).GetComponent<TMP_Text>().text == "배운점")
            {
                if (activeToggle.GetComponent<Toggle>().isOn) { writing_Learning.SetActive(true); count++; }
                else { writing_Learning.SetActive(false); inputLearning.text = ""; }
            }
        }
        if (count == 0) writingArea.SetActive(false);
        else writingArea.SetActive(true);
        addInputFieldContainer.transform.parent.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();
        writingArea.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        writingContent.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
    }
    #endregion#

    public void goFolder() { SceneManager.LoadScene("3_Folder");  }

}
