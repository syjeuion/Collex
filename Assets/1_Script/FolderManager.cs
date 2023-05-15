using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Newtonsoft.Json;

public class FolderManager : MonoBehaviour
{
    #region 페이지
    public GameObject FolderPage;
    public GameObject FilterPage;
    //public GameObject RecordInPage;
    public GameObject FinishUp_1;
    public GameObject FinishUp_2;
    public GameObject FinishUp_3;
    public GameObject SelectedRecordPage;
    public GameObject selfWritingPage;
    public GameObject ReportPage;
    #endregion

    //폴더 페이지 세팅
    public GameObject titleArea;
    public GameObject countAndSorting;
    public GameObject buttonFinishUp;
    public GameObject buttonWriting;
    public GameObject toastPopUp;

    //기록 페이지 세팅
    [SerializeField] TMP_Text recordTitle;
    [SerializeField] TMP_Text recordCapability;
    [SerializeField] TMP_Text recordExperience;
    [SerializeField] TMP_Text recordHashtag;

    //선택된 폴더 정보
    MakeNewProject thisProject;

    public GameObject explanation;
    public GameObject recordListContainer;
    public GameObject recordList;
    GameObject newRecord;
    
    string capas; //역량, 태그 UI에 표시하는 임시 텍스트 저장소
    string outputDate;

    //DateTime startDate;
    //string startDateStr;
    //DateTime endedDate;
    //string endedDateStr;

    #region 마무리하기 오브젝트
    //마무리하기
    public GameObject Contest; //공모전 수상내역

    public TMP_InputField inputMyRole; //역할
    public TMP_InputField inputSummary; //한 줄 요약
    public TMP_InputField inputSelfWriting; //직접 작성하기

    public GameObject finishUp2_EpisodeTypes;
    public GameObject EpisodeTypeGroup;

    public GameObject EpisodeSituation;
    public GameObject EpisodeAction;
    public GameObject EpisodeResult;
    public GameObject EpisodeRealization;

    public TMP_Text selectRecordPageTitle;
    public TMP_Text selectedRecord; //선택된 질문종류 표시 칩

    public GameObject writtenRecordContent; //기록 선택 페이지
    public GameObject singleWrittenRecord; //선택 전
    public GameObject singleSelectedWrittenRecord; //선택 후
    GameObject newWrittenRecord;
    #endregion

    //필터
    public GameObject filterTabs;
    public Sprite unselectFilter;
    public Sprite selectedFilter;
    public Sprite arrowIcon;
    public Sprite closeIcon;

    List<string> defaultTitles = new List<string>();
    List<string> filterTitles = new List<string>();

    //리포트 페이지 세팅
    public GameObject ReportContent;
    public GameObject FolderInfo;
    public GameObject RecordCount;
    public GameObject mainEpisode_none;
    public GameObject MainEpisode;
    public GameObject EpisodeType;
    public GameObject CapabilitesAnalytics;
    public GameObject Experiences;
    public GameObject ExEmpty;
    public GameObject ReportScript;

    //색상
    Color primary3;
    Color gray200;
    Color gray300;
    Color gray600;
    Color gray700;
    Color gray900;
    Color errorColor;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#EBEDEF", out gray200);
        ColorUtility.TryParseHtmlString("#DDE0E3", out gray300);
        ColorUtility.TryParseHtmlString("#6A7381", out gray600);
        ColorUtility.TryParseHtmlString("#575F6B", out gray700);
        ColorUtility.TryParseHtmlString("#1E2024", out gray900);
        ColorUtility.TryParseHtmlString("#FF3E49", out errorColor);
    }

    void Start()
    {
        FolderPage.SetActive(true);
        //RecordInPage.SetActive(false);

        string thisFolderDatas = UserManager.Instance.folders[UserManager.Instance.pushedButton];
        thisProject = JsonConvert.DeserializeObject<MakeNewProject>(thisFolderDatas);

        if (!thisProject.isItOngoing)
        {
            buttonFinishUp.SetActive(false);
            buttonWriting.SetActive(false);
            titleArea.transform.GetChild(3).GetComponent<TMP_Text>().text = thisProject.endedDate.Year+"년 "+ thisProject.endedDate.Month+"월 "+ thisProject.endedDate.Day+"일";
            titleArea.transform.GetChild(3).GetComponent<TMP_Text>().color = gray900;
        }

        foreach (string keys in thisProject.records.Keys)
        {
            defaultTitles.Add(keys);
        }

        //폴더 정보
        titleArea.transform.GetChild(0).GetComponent<TMP_Text>().text = thisProject.projectType;
        titleArea.transform.GetChild(1).GetComponent<TMP_Text>().text = thisProject.projectTitle;

        //startDate = new DateTime(thisProject.startYear, thisProject.startMonth, thisProject.startDay);
        titleArea.transform.GetChild(2).GetComponent<TMP_Text>().text = thisProject.startDate.Year + "년 " + thisProject.startDate.Month + "월 " + thisProject.startDate.Day + "일";
        
        //해당 폴더에 기록이 있으면 기록 리스트 출력
        if (thisProject.records.Count >0)
        {
            countAndSorting.SetActive(true);
            countAndSorting.transform.GetChild(0).GetComponent<TMP_Text>().text = "기록 " + thisProject.records.Count.ToString() + "개";
            StartCoroutine(outputRecords(defaultTitles));
        }

        if (!string.IsNullOrWhiteSpace(UserManager.Instance.pushedRecord)) clickRecord();
    }
    private void Update()
    {
        if (ReportPage.activeSelf)
        {
            ReportContent.GetComponent<VerticalLayoutGroup>().spacing = 15.5f;
            ReportContent.GetComponent<VerticalLayoutGroup>().spacing = 16f;
        }
    }

    #region 필터
    //오픈 필터 페이지
    public void openFilter()
    {
        FilterPage.SetActive(true);
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        if (currentObject.name == "Filter_capability") { filterTabs.transform.GetChild(1).GetComponent<Toggle>().isOn = true; }
        if (currentObject.name == "Filter_hashtag") { filterTabs.transform.GetChild(2).GetComponent<Toggle>().isOn = true; }
    }

    //선택된 필터 리스트들
    List<string> filterCapa = new List<string>();
    List<string> filterHash = new List<string>();
    //필터 업데이트
    public void UpdateFilter()
    {
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        TMP_Text newFilter = currentObject.transform.GetChild(1).GetComponent<TMP_Text>();

        if (currentObject.GetComponent<Toggle>().isOn == true)
        {
            newFilter.color = primary3;
            if (currentObject.transform.parent.name == "Panel_Capability")
            {
                if (!filterCapa.Contains(newFilter.text)) { filterCapa.Add(newFilter.text); }
            }
            if (currentObject.transform.parent.name == "Panel_Hashtag")
            {
                if (!filterHash.Contains(newFilter.text)) { filterHash.Add(newFilter.text); }
            }

        }
        if (currentObject.GetComponent<Toggle>().isOn == false)
        {
            newFilter.color = gray700;

            if (currentObject.transform.parent.name == "Panel_Capability")
            {
                if (filterCapa.Contains(newFilter.text)) { filterCapa.Remove(newFilter.text); }
            }
            if (currentObject.transform.parent.name == "Panel_Hashtag")
            {
                if (filterHash.Contains(newFilter.text)) { filterHash.Remove(newFilter.text); }
            }
        }
    }

    //필터 완료버튼(출력)
    public void outputFilter()
    {
        //선택된 필터가 없는 경우
        if (filterCapa.Count <= 0 && filterHash.Count <= 0)
        {
            StartCoroutine(outputRecords(defaultTitles));
            explanation.GetComponent<Text>().text = "아직 작성된 기록이 없네요!\n나의 직무 경험을 바로 기록해 볼까요?";
        }
        else
        {
            filterTitles = new List<string>();
            foreach (string keys in thisProject.records.Keys)
            {
                string getRecordData = thisProject.records[keys];
                DailyRecord newDailyRecord = JsonConvert.DeserializeObject<DailyRecord>(getRecordData);
                foreach (string capa in filterCapa)
                {
                    if (newDailyRecord.capabilities.Contains(capa) && !filterTitles.Contains(keys))
                    { filterTitles.Add(keys); }
                }
                foreach (string hash in filterHash)
                {
                    if (newDailyRecord.hashtags.Contains(hash) && !filterTitles.Contains(keys))
                    { filterTitles.Add(keys); }
                }
            }

            StartCoroutine(outputRecords(filterTitles));
            explanation.GetComponent<TMP_Text>().text = "조건에 해당하는 기록이 없어요.\n다른 조건으로 다시 설정해 보세요!";
        }
        FilterPage.SetActive(false);
    }
    #endregion

    //기록리스트 출력
    public IEnumerator outputRecords(List<string> titles)
    {
        //리스트 초기화
        for(int i=0; i< recordListContainer.transform.childCount; i++) { Destroy(recordListContainer.transform.GetChild(i).gameObject); }

        for (int i = 0; i < titles.Count; i++)
        {
            string getRecordData = thisProject.records[titles[i]];
            DailyRecord newDailyRecord = JsonConvert.DeserializeObject<DailyRecord>(getRecordData);

            newRecord = Instantiate(recordList, recordListContainer.transform);
            newRecord.transform.SetAsFirstSibling();
            newRecord.transform.GetChild(0).GetComponent<TMP_Text>().text = newDailyRecord.title;
            newRecord.transform.GetChild(1).GetComponent<TMP_Text>().text = newDailyRecord.writings["활동내용"];

            for (int ii = 0; ii < 3; ii++)
            {
                if (newDailyRecord.capabilities[ii] == null) { break; }
                else if (ii != 0) { capas += "・"; }
                capas += newDailyRecord.capabilities[ii];
            }
            newRecord.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = capas;
            capas = "";
            newRecord.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = newDailyRecord.date;
            newRecord.GetComponent<Button>().onClick.AddListener(clickRecord);

            yield return new WaitForEndOfFrame();
            newRecord.GetComponent<VerticalLayoutGroup>().spacing = 7.9f;
            newRecord.GetComponent<VerticalLayoutGroup>().spacing = 8;

            yield return new WaitForEndOfFrame();
            recordListContainer.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
            recordListContainer.GetComponent<VerticalLayoutGroup>().spacing = 20;
        }
        yield return new WaitForEndOfFrame();
        recordListContainer.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        recordListContainer.GetComponent<VerticalLayoutGroup>().spacing = 20;

        yield return new WaitForEndOfFrame();
        if (recordListContainer.transform.childCount > 0)
            explanation.SetActive(false);
        else
            explanation.SetActive(true);
    }
    
    //기록 상세페이지
    public void clickRecord()
    {
        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        if (clickButton != null)
        {
            UserManager.Instance.pushedRecord = clickButton.transform.GetChild(0).GetComponent<TMP_Text>().text;
            DontDestroyCanvas.setRecord();
        }
        
    }
    
    #region 마무리하기
    //마무리하기
    public void FinishUp1Confirm()
    {
        FinishUp_1.SetActive(false);
        FinishUp_2.SetActive(true);
        if (thisProject.projectType == "공모전") { Contest.SetActive(true); }
        else { Contest.SetActive(false); }
    }

    bool episodePositive;
    public void FinishUp2Confirm()
    {
        //역할 인풋 비었는지 확인해야함
        if (string.IsNullOrWhiteSpace(inputMyRole.text))
        {
            inputMyRole.GetComponent<Image>().color = errorColor;
            StartCoroutine(errorMessage(true));
            return;
        }
        //대표에피소드 선택 안했을때
        if (string.IsNullOrWhiteSpace(thisProject.episodeType))
        {
            return;
        }

        if(thisProject.episodeType== "특별한 에피소드가 없었어요")
        {
            FolderPage.SetActive(false);
            //RecordInPage.SetActive(false);
            FinishUp_1.SetActive(false);
            FinishUp_2.SetActive(false);
            ReportPage.SetActive(true);
            return;
        }
        else
        {
            if (thisProject.episodeType == "계속해서 노력했지만 아쉬운 점이 많았어요") episodePositive = false;
            else episodePositive = true;

            FinishUp_3.SetActive(true);
            return;
        }
    }
    //Role & 한줄요약 안썼을때
    GameObject selectedField;
    public GameObject finishUp2NextButton;
    public GameObject finishUp3NextButton;
    public void InputOnSelected()
    {
        selectedField = EventSystem.current.currentSelectedGameObject;
        if (selectedField.GetComponent<TMP_InputField>() != null)
        {
            selectedField.GetComponent<Image>().color = primary3;
        }
        if (selectedField.name == "Input_My Role")
        { StartCoroutine(errorMessage(false)); }
    }
    IEnumerator errorMessage(bool state)
    {
        if(state) inputMyRole.transform.parent.GetChild(3).gameObject.SetActive(true);
        else inputMyRole.transform.parent.GetChild(3).gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        inputMyRole.transform.parent.parent.GetComponent<VerticalLayoutGroup>().spacing = 19.9f;
        inputMyRole.transform.parent.parent.GetComponent<VerticalLayoutGroup>().spacing = 20f;
    }
    public void InputValueChanged()
    {
        if(selectedField.name == "Input_My Role")
        {
            if (!string.IsNullOrWhiteSpace(inputMyRole.text) && !string.IsNullOrWhiteSpace(thisProject.episodeType))
            {
                finishUp2NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray900;
            }
            else finishUp2NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray200;
        }
        else if(selectedField.name == "Input_Summary")
        {
            if (!string.IsNullOrWhiteSpace(inputSummary.text))
            {
                finishUp3NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray900;
            }
            else finishUp3NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray200;
        }
    }
    public void inputRoleDeselected()
    {
        selectedField.GetComponent<Image>().color = gray300;
    }

    //대표 에피소드 선택 시 글씨 색 변경
    Toggle lastToggle;
    public void changeTextColorToggleGroup()
    {
        if(lastToggle!=null)
            lastToggle.transform.GetChild(1).GetComponent<TMP_Text>().color = gray600;

        GameObject thisObj = EventSystem.current.currentSelectedGameObject;
        Toggle thisToggle = thisObj.GetComponent<Toggle>();
        if (thisToggle == null) return;

        if (thisToggle.isOn)
        {
            thisToggle.transform.GetChild(1).GetComponent<TMP_Text>().color = primary3;
            thisProject.episodeType = thisObj.transform.GetChild(1).GetComponent<TMP_Text>().text;
        }
        else
        {
            thisToggle.transform.GetChild(1).GetComponent<TMP_Text>().color = gray600;
            thisProject.episodeType = "";
        }

        if (!string.IsNullOrWhiteSpace(inputMyRole.text) && !string.IsNullOrWhiteSpace(thisProject.episodeType))
        {
            finishUp2NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray900;
        }
        else finishUp2NextButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray200;
        lastToggle = thisToggle;
    }

    #region 에피소드 선택 페이지 세팅
    List<string> selectedQuestion = new List<string>();
    string selectedStep;
    public void selectEpisode()
    {
        selectedQuestion = new List<string>();
        SelectedRecordPage.SetActive(true);
        selectedStep = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        if (selectedStep == "Situation")
        {   selectRecordPageTitle.text = "상황";
            if (episodePositive) { selectedQuestion.Add("문제상황"); selectedQuestion.Add("활동내용"); }
            else { selectedQuestion.Add("문제상황"); selectedQuestion.Add("문제원인"); }
        }
        if (selectedStep == "Action")
        {   selectRecordPageTitle.text = "행동";
            if (episodePositive) { selectedQuestion.Add("해결과정"); selectedQuestion.Add("잘한점"); }
            else { selectedQuestion.Add("활동내용"); selectedQuestion.Add("해결과정"); }
        }
        if (selectedStep == "Result")
        { selectRecordPageTitle.text = "결과";
            if (episodePositive) { selectedQuestion.Add("배운점"); selectedQuestion.Add("잘한점"); }
            else { selectedQuestion.Add("잘한점"); selectedQuestion.Add("부족한점"); }
        }
        if (selectedStep == "Realization")
        { selectRecordPageTitle.text = "느낀점"; selectedQuestion.Add("배운점"); }

        //해당되는 기록 출력
        StartCoroutine(setRecordCard());
    }
    IEnumerator setRecordCard()
    {
        selectedRecord.text = selectedQuestion[0];
        if (selectedQuestion.Count>=2) selectedRecord.text += ", " + selectedQuestion[1];
        if (selectedQuestion.Count>=3) selectedRecord.text += " 외 " + (selectedQuestion.Count - 2).ToString() + "건";
        yield return new WaitForEndOfFrame();
        selectedRecord.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 3.9f;
        selectedRecord.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 4;

        //리셋
        for (int i = 1; i < writtenRecordContent.transform.childCount; i++)
            { Destroy(writtenRecordContent.transform.GetChild(i).gameObject); }

        //출력
        foreach (string value in thisProject.records.Values)
        {
            DailyRecord newDailyRecord = JsonConvert.DeserializeObject<DailyRecord>(value);
            for (int i = 0; i < selectedQuestion.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(newDailyRecord.writings[selectedQuestion[i]]))
                {
                    newWrittenRecord = Instantiate(singleWrittenRecord, writtenRecordContent.transform);
                    newWrittenRecord.transform.GetChild(1).GetComponent<TMP_Text>().text = selectedQuestion[i];
                    newWrittenRecord.transform.GetChild(2).GetComponent<TMP_Text>().text = newDailyRecord.title;
                    newWrittenRecord.transform.GetChild(3).GetComponent<TMP_Text>().text = newDailyRecord.writings[selectedQuestion[i]];
                    newWrittenRecord.transform.GetChild(4).GetComponent<TMP_Text>().text = newDailyRecord.date;
                    newWrittenRecord.GetComponent<Toggle>().onValueChanged.AddListener(changeTextColor);

                }
            }
        }
    }
    //토글 선택될때 글자 색 바꾸기
    public void changeTextColor(bool isOn)
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
        if (selectedObj.name == "NextButton") return;
        if (isOn) selectedObj.transform.GetChild(1).GetComponent<TMP_Text>().color = primary3;
        else selectedObj.transform.GetChild(1).GetComponent<TMP_Text>().color = gray600;
    }
    #endregion

    #region 바텀시트로 질문종류 추가
    //바텀시트 셋팅
    public GameObject selectOtherRecordsContainer;
    public void bottomSheetSetting()
    {
        selectOtherRecordsContainer.transform.parent.gameObject.SetActive(true);
        for(int i=0; i< selectedQuestion.Count; i++)
        {
            for (int ii = 1; ii < 8; ii++)
            {
                if(selectOtherRecordsContainer.transform.GetChild(ii).GetChild(0).GetComponent<TMP_Text>().text == selectedQuestion[i])
                {
                    selectOtherRecordsContainer.transform.GetChild(ii).GetComponent<Toggle>().isOn = true;
                    selectOtherRecordsContainer.transform.GetChild(ii).GetChild(0).GetComponent<TMP_Text>().color = primary3;
                    break;
                }
            }
        }
    }
    //바텀시트 칩 선택 시 색 변경
    public void changeChipColor()
    {
        GameObject selectedChip = EventSystem.current.currentSelectedGameObject;
        
        if (selectedChip.name == "Button_AddInputField"||selectedChip.name=="SelectFilter Button") return;
        if (selectedChip.GetComponent<Toggle>().isOn)
        {
            selectedChip.GetComponent<Image>().color = primary3;
            selectedChip.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
        }
        else
        {
            selectedChip.GetComponent<Image>().color = gray300;
            selectedChip.transform.GetChild(0).GetComponent<TMP_Text>().color = gray700;
        }
    }
    //바텀시트 선택 저장
    public void confirmBottom()
    {
        selectedQuestion = new List<string>();
        for (int i=1;i< 8; i++)
        {
            GameObject thisToggle = selectOtherRecordsContainer.transform.GetChild(i).gameObject;
            if (thisToggle.GetComponent<Toggle>().isOn)
            {
                selectedQuestion.Add(thisToggle.transform.GetChild(0).GetComponent<TMP_Text>().text);
            }
        }
        StartCoroutine(setRecordCard());
        selectOtherRecordsContainer.transform.parent.gameObject.SetActive(false);
    }
    #endregion

    #region 에피소드 선택된 기록 저장
    public void SaveSelectedEpisode()
    {
        if (selectedStep == "Situation")
            StartCoroutine(selectedEpisodeCT(EpisodeSituation.transform.parent.gameObject, thisProject.EpisodeSituation));
        if (selectedStep == "Action")
            StartCoroutine(selectedEpisodeCT(EpisodeAction.transform.parent.gameObject, thisProject.EpisodeAction));
        if (selectedStep == "Result")
            StartCoroutine(selectedEpisodeCT(EpisodeResult.transform.parent.gameObject, thisProject.EpisodeResult));
        if (selectedStep == "Realization")
            StartCoroutine(selectedEpisodeCT(EpisodeRealization.transform.parent.gameObject, thisProject.EpisodeRealization));
    }

    IEnumerator selectedEpisodeCT(GameObject parentObj, List<string> thisList)
    {
        //thisList = new List<string>();
        for(int i=0;i<thisList.Count;i++)
        {
            thisList.Remove(thisList[i]);
        }

        for (int i = 1; i < writtenRecordContent.transform.childCount; i++)
        {
            GameObject selectedRecord = writtenRecordContent.transform.GetChild(i).gameObject;
            if (selectedRecord.GetComponent<Toggle>().isOn)
            {
                if (i == 1)
                {
                    parentObj.transform.GetChild(1).gameObject.SetActive(false);
                    for (int ii = 2; ii < parentObj.transform.childCount; ii++)
                    { Destroy(parentObj.transform.GetChild(ii).gameObject); }
                }

                string type = selectedRecord.transform.GetChild(1).GetComponent<TMP_Text>().text;
                string title;
                if (selectedRecord.transform.GetChild(2).gameObject.activeSelf)
                { title = selectedRecord.transform.GetChild(2).GetComponent<TMP_Text>().text; }
                else { title = ""; }
                string content = selectedRecord.transform.GetChild(3).GetComponent<TMP_Text>().text;
                string date = selectedRecord.transform.GetChild(4).GetComponent<TMP_Text>().text; 

                newWrittenRecord = Instantiate(singleSelectedWrittenRecord, parentObj.transform);
                newWrittenRecord.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = type;
                newWrittenRecord.transform.GetChild(1).GetComponent<TMP_Text>().text = title;
                if (string.IsNullOrWhiteSpace(title)) newWrittenRecord.transform.GetChild(1).gameObject.SetActive(false);

                newWrittenRecord.transform.GetChild(2).GetComponent<TMP_Text>().text = content;
                newWrittenRecord.transform.GetChild(3).GetComponent<TMP_Text>().text = date;
                newWrittenRecord.GetComponent<Button>().onClick.AddListener(openSelectRecord);

                Episode episode = new Episode();
                episode.type = type;
                episode.title = title;
                episode.content = content;
                episode.date = date;

                string newEpisodeData = JsonConvert.SerializeObject(episode);
                thisList.Add(newEpisodeData);
            }
        }
        yield return new WaitForEndOfFrame();
        parentObj.GetComponent<VerticalLayoutGroup>().spacing = 7.9f;
        parentObj.GetComponent<VerticalLayoutGroup>().spacing = 8;
        yield return new WaitForEndOfFrame();
        EpisodeTypeGroup.GetComponent<VerticalLayoutGroup>().spacing = 11.9f;
        EpisodeTypeGroup.GetComponent<VerticalLayoutGroup>().spacing = 12;
        
        SelectedRecordPage.SetActive(false);
    }
    void openSelectRecord()
    {
        SelectedRecordPage.SetActive(true);
    }
    #endregion

    #region 직접 작성하기
    public void setSelfWriting()
    {
        selfWritingPage.SetActive(true);
        inputSelfWriting.text = "";
    }
    public void saveSelfWriting()
    {
        if (!string.IsNullOrWhiteSpace(inputSelfWriting.text))
        {
            StartCoroutine(PrintWrittenRecord());
        }
        selfWritingPage.SetActive(false);
    }
    IEnumerator PrintWrittenRecord()
    {
        newWrittenRecord = Instantiate(singleWrittenRecord, writtenRecordContent.transform);
        newWrittenRecord.transform.GetChild(1).GetComponent<TMP_Text>().text = "직접작성";
        newWrittenRecord.transform.GetChild(1).GetComponent<TMP_Text>().color = primary3;
        newWrittenRecord.transform.GetChild(3).GetComponent<TMP_Text>().text = inputSelfWriting.text;
        newWrittenRecord.transform.GetChild(4).GetComponent<TMP_Text>().text = DateTime.Now.ToString("yyyy년 MM월 dd일");
        newWrittenRecord.GetComponent<Toggle>().onValueChanged.AddListener(changeTextColor);

        newWrittenRecord.transform.GetChild(2).gameObject.SetActive(false);
        newWrittenRecord.transform.SetSiblingIndex(1);

        yield return new WaitForEndOfFrame();
        newWrittenRecord.GetComponent<Toggle>().isOn = true;
    }
    #endregion

    //최종 저장
    public void SaveFinishUp()
    {
        //한줄요약 필수값
        if (string.IsNullOrWhiteSpace(inputSummary.text))
        {
            inputSummary.GetComponent<Image>().color = errorColor;
            return;
        }

        thisProject.myRole = inputMyRole.text; //역할
        thisProject.Summary = inputSummary.text; //한줄요약
        //공모전 수상내역
        if (!string.IsNullOrWhiteSpace(Contest.transform.GetChild(2).GetComponent<TMP_InputField>().text))
            { thisProject.prize = Contest.transform.GetChild(2).GetComponent<TMP_InputField>().text; }

        //종료날짜 저장
        thisProject.endedDate = DateTime.Now;
        //thisProject.endedYear = DateTime.Now.Year;
        //thisProject.endedMonth = DateTime.Now.Month;
        //thisProject.endedDay = DateTime.Now.Day;
        //thisProject.endedDate = DateTime.Now.Year.ToString() + "년 " + DateTime.Now.Month.ToString() + "월 " + DateTime.Now.Day.ToString() + "일";

        thisProject.isItOngoing = false;

        //JSON 변환 후 UserManager에 저장
        string newFolderData = JsonConvert.SerializeObject(thisProject);
        UserManager.Instance.folders[thisProject.projectTitle] = newFolderData;

        //칭호 획득했을때 획득한 칭호 정보 저장하고 획득 페이지 띄워주기
        UserManager.Instance.newUserInformation.titleCheck[5]++; //활동 마무리 count

        UserManager.Instance.checkTitle("앞", 4, 5);
        UserManager.Instance.checkTitle("뒤", 3, 5);
        UserManager.Instance.checkTitle("뒤", 4, 5);
        if (UserManager.Instance.nowGetTitle.Count != 0) UserManager.Instance.getTitle = 1;

        setReport();
    }
    
    public void returnFolder()
    {
        FolderPage.SetActive(true);
        if (!thisProject.isItOngoing)
        {
            buttonFinishUp.SetActive(false);
            buttonWriting.SetActive(false);
        }
        //RecordInPage.SetActive(false);
        FinishUp_1.SetActive(false);
        FinishUp_2.SetActive(false);
        FinishUp_3.SetActive(false);
        //FinishUp_4.SetActive(false);
        SelectedRecordPage.SetActive(false);
        ReportPage.SetActive(false);
    }
    #endregion

    #region 리포트
    //메인에피소드
    public GameObject mainEpisodeFolding;
    public GameObject Episode;
    GameObject newEpisode;
    Episode episode;

    //경험칩
    public GameObject BadgesContainer;
    public GameObject horizontalGroup;
    GameObject newHorizontalGroup;
    public GameObject BadgePrefab;
    GameObject newBadge;
    public void setReport()
    {
        ReportPage.SetActive(true);

        FolderInfo.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = thisProject.projectType;
        FolderInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = thisProject.projectTitle;
        FolderInfo.transform.GetChild(2).GetComponent<TMP_Text>().text = thisProject.startDate+" ~ ";
        if (!thisProject.isItOngoing)
            { FolderInfo.transform.GetChild(2).GetComponent<TMP_Text>().text += thisProject.endedDate; }
        if(!string.IsNullOrWhiteSpace(thisProject.myRole))FolderInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = thisProject.myRole;

        if (thisProject.records.Count > 9) RecordCount.transform.GetChild(2).GetComponent<TMP_Text>().text = "                의 기록을 작성했어요!";
        RecordCount.transform.GetChild(4).GetComponent<TMP_Text>().text = "총 "+thisProject.records.Count.ToString() +"개";

        
        if (thisProject.isItOngoing)
        {
            //날짜 비교
            TimeSpan howManyDays = DateTime.Now - thisProject.startDate;
            int howManyDaysInt = howManyDays.Days;
            if (howManyDaysInt > 9) RecordCount.transform.GetChild(1).GetComponent<TMP_Text>().text = "          동안";
            RecordCount.transform.GetChild(3).GetComponent<TMP_Text>().text = howManyDaysInt.ToString() + "일";

            MainEpisode.SetActive(false);
            mainEpisode_none.SetActive(true);

            CapabilitesAnalytics.transform.GetChild(2).gameObject.SetActive(false);
            
        }
        else
        {
            //날짜 비교
            //endedDate = new DateTime(thisProject.endedYear, thisProject.endedMonth, thisProject.endedDay);
            TimeSpan howManyDays = thisProject.endedDate - thisProject.startDate;
            int howManyDaysInt = howManyDays.Days;
            if (howManyDaysInt > 9) RecordCount.transform.GetChild(1).GetComponent<TMP_Text>().text = "          동안";
            RecordCount.transform.GetChild(3).GetComponent<TMP_Text>().text = howManyDaysInt.ToString() + "일";

            //대표에피소드
            MainEpisode.SetActive(true);
            mainEpisode_none.SetActive(false);
            MainEpisode.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = thisProject.episodeType;
            MainEpisode.transform.GetChild(1).GetComponent<TMP_Text>().text = thisProject.Summary;

            if(thisProject.EpisodeSituation.Count==0&& thisProject.EpisodeAction.Count == 0
                && thisProject.EpisodeResult.Count == 0&& thisProject.EpisodeRealization.Count == 0)
            {
                MainEpisode.transform.GetChild(2).gameObject.SetActive(false);
                MainEpisode.transform.GetChild(3).gameObject.SetActive(false);
                MainEpisode.transform.GetChild(4).gameObject.SetActive(false);
            }
            else
            {
                for(int i=0;i< mainEpisodeFolding.transform.childCount; i++)
                    { Destroy(mainEpisodeFolding.transform.GetChild(i).gameObject); }

                setEpisode(thisProject.EpisodeSituation, "상황");
                setEpisode(thisProject.EpisodeAction, "행동");
                setEpisode(thisProject.EpisodeResult, "결과");
                setEpisode(thisProject.EpisodeRealization, "느낀점");
            }

            //활동 유형
            EpisodeType.transform.GetChild(5).GetComponent<TMP_Text>().text = "이 활동에서 " + UserManager.Instance.newUserInformation.userName + "님은";
            EpisodeType.transform.GetChild(4).gameObject.SetActive(false);
            EpisodeType.transform.GetChild(0).GetComponent<TMP_Text>().text = ReportScript.GetComponent<Report>().setEpisodeType()[1];
            EpisodeType.transform.GetChild(3).GetComponent<TMP_Text>().text = ReportScript.GetComponent<Report>().setEpisodeType()[0];

            //역량 분석
            CapabilitesAnalytics.transform.GetChild(3).gameObject.SetActive(true);
            ReportScript.GetComponent<Report>().setGraph(); //그래프
            CapabilitesAnalytics.transform.GetChild(2).GetComponent<TMP_Text>().text = ReportScript.GetComponent<Report>().MostCapability;

            //글자 크기 예외처리
            if(ReportScript.GetComponent<Report>().MostCapability == "커뮤니케이션능력")
            {
                CapabilitesAnalytics.transform.GetChild(1).GetComponent<TMP_Text>().text = "이 활동에서는                                           이\n특히 두드러지네요!";
                CapabilitesAnalytics.transform.GetChild(1).GetComponent<TMP_Text>().fontSize = 21;
                CapabilitesAnalytics.transform.GetChild(2).GetComponent<TMP_Text>().fontSize = 21;
                CapabilitesAnalytics.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(-24, CapabilitesAnalytics.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y) ;
            }
            else if(ReportScript.GetComponent<Report>().MostCapability == "문제해결능력")
            {
                CapabilitesAnalytics.transform.GetChild(1).GetComponent<TMP_Text>().text = "이 활동에서는                                이\n특히 두드러지네요!";
            }
            CapabilitesAnalytics.transform.GetChild(6).gameObject.SetActive(false);

            //획득 경험
            StartCoroutine(setExperienceBadges());
            Experiences.GetComponent<VerticalLayoutGroup>().padding.bottom = 16;
            Experiences.transform.GetChild(2).gameObject.SetActive(false);
        }

        if (UserManager.Instance.newUserInformation.titleCheck[26] == 0)
        {
            UserManager.Instance.newUserInformation.titleCheck[26]++;
            UserManager.Instance.checkTitle("뒤", 26, 26);
            UserManager.Instance.getTitle = 1;
        }
    }
    //메인 에피소드
    void setEpisode(List<string> episodeList, string step)
    {
        for (int i = 0; i < episodeList.Count; i++)
        {
            episode = JsonConvert.DeserializeObject<Episode>(episodeList[i]);

            newEpisode = Instantiate(Episode, mainEpisodeFolding.transform);
            newEpisode.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = step;
            newEpisode.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = episode.type;

            if (!string.IsNullOrWhiteSpace(episode.title))
                { newEpisode.transform.GetChild(1).GetComponent<TMP_Text>().text = episode.title; }
            else
                newEpisode.transform.GetChild(1).gameObject.SetActive(false);

            newEpisode.transform.GetChild(2).GetComponent<TMP_Text>().text = episode.content;
            newEpisode.transform.GetChild(3).GetComponent<TMP_Text>().text = episode.date;
        }
    }

    #region 리포트 경험
    IEnumerator setExperienceBadges()
    {
        Dictionary<string, int> sortedEx = thisProject.experiences.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        List<string> sortedKeys = new List<string>();
        foreach (string key in sortedEx.Keys) sortedKeys.Add(key);
        if (sortedKeys.Count <= 3)
        {
            Experiences.transform.GetChild(1).gameObject.SetActive(false);
            if (sortedKeys.Count <= 2) Experiences.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
            if (sortedKeys.Count <= 1) Experiences.transform.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
            if (sortedKeys.Count == 0) { Experiences.SetActive(false); ExEmpty.SetActive(true); }
        }
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            if (i < 3)
            {
                Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = sortedKeys[i];
                Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = sortedEx[sortedKeys[i]].ToString() + "회";
                if (sortedEx[sortedKeys[i]] == sortedEx[sortedKeys[0]] && i != 0)
                {
                    ColorUtility.TryParseHtmlString("#408BFD", out Color primary3);
                    ColorUtility.TryParseHtmlString("#0061F4", out Color primary4);
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "1";
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = primary4;
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(1).GetComponent<Image>().color = primary3;
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = primary4;
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(1).GetChild(1).GetComponent<TMP_Text>().color = primary3;
                }
                else if(i==2&&sortedEx[sortedKeys[2]] == sortedEx[sortedKeys[1]])
                    Experiences.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "2";
            }
            else
            {
                if (i == 3)
                {
                    newHorizontalGroup = Instantiate(horizontalGroup, BadgesContainer.transform);
                }
                newBadge = Instantiate(BadgePrefab, newHorizontalGroup.transform);
                newBadge.transform.GetChild(0).GetComponent<TMP_Text>().text = sortedKeys[i];
                yield return new WaitForEndOfFrame();

                if (newHorizontalGroup.GetComponent<RectTransform>().sizeDelta.x >= 350)
                {
                    newHorizontalGroup = Instantiate(horizontalGroup, BadgesContainer.transform);
                    newBadge.transform.SetParent(newHorizontalGroup.transform);
                }
            }
        }
    }
    #endregion
    #endregion

    #region 활동폴더 수정하기
    //활동폴더 수정하기
    public GameObject modifyContainer;
    public TMP_InputField inputFolderTitle;
    //수정 페이지 세팅
    public void openModifyPage()
    {
        inputFolderTitle.text = thisProject.projectTitle;
        for(int i=0; i < 3; i++)
        {
            if (modifyContainer.transform.GetChild(6).GetChild(i).GetChild(1).GetComponent<TMP_Text>().text == thisProject.projectType)
            {
                modifyContainer.transform.GetChild(6).GetChild(i).GetComponent<Toggle>().isOn = true;
            }
        }
        modifyContainer.transform.parent.gameObject.SetActive(true);
    }
    //제목 예외처리
    public void checkFolderTitle()
    {
        ColorUtility.TryParseHtmlString("#FF3E49", out Color colorRed);
        ColorUtility.TryParseHtmlString("#408BFD", out Color colorBlue);
        
        modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().text = inputFolderTitle.text.Length.ToString() + "/20";
        if (inputFolderTitle.text.Length >= 20 || (inputFolderTitle.text != thisProject.projectTitle && UserManager.Instance.folders.ContainsKey(inputFolderTitle.text)))
        {
            inputFolderTitle.GetComponent<Image>().color = colorRed;
            modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().color = colorRed;
        }
        else //(inputFolderTitle.text.Length < 20)
        {
            inputFolderTitle.GetComponent<Image>().color = colorBlue;
            modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().color = colorBlue;
        }
        if (inputFolderTitle.text != thisProject.projectTitle && UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
        {
            modifyContainer.transform.GetChild(3).GetComponent<TMP_Text>().text = "중복되는 이름의 활동폴더가 있어요.";
        }
        else
        {
            modifyContainer.transform.GetChild(3).GetComponent<TMP_Text>().text = "";
        }
    }
    public void inputDeselected()
    {
        ColorUtility.TryParseHtmlString("#E0E0E0", out Color color);
        ColorUtility.TryParseHtmlString("#9E9E9E", out Color colorGray);
        inputFolderTitle.GetComponent<Image>().color = color;
        modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().color = colorGray;
    }
    //폴더 수정사항 저장하기
    public void saveModifyFolder()
    {
        //예외처리
        ColorUtility.TryParseHtmlString("#FF3E49", out Color colorRed);
        if (inputFolderTitle.text==""|| inputFolderTitle.text == null)
        {
            inputFolderTitle.GetComponent<Image>().color = colorRed;
            modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().color = colorRed;
            modifyContainer.transform.GetChild(3).GetComponent<TMP_Text>().text = "활동폴더의 이름을 정해주세요.";
            return;
        }
        if (inputFolderTitle.text != thisProject.projectTitle && UserManager.Instance.folders.ContainsKey(inputFolderTitle.text))
        {
            inputFolderTitle.GetComponent<Image>().color = colorRed;
            modifyContainer.transform.GetChild(4).GetComponent<TMP_Text>().color = colorRed;
            modifyContainer.transform.GetChild(3).GetComponent<TMP_Text>().text = "중복되는 이름의 활동 폴더가 있어요.";
            return;
        }

        //기존 데이터 삭제
        UserManager.Instance.folders.Remove(thisProject.projectTitle);

        thisProject.projectTitle = inputFolderTitle.text;
        for (int i = 0; i < 3; i++)
        {
            if (modifyContainer.transform.GetChild(6).GetChild(i).GetComponent<Toggle>().isOn == true)
            {
                thisProject.projectType = modifyContainer.transform.GetChild(6).GetChild(i).GetChild(1).GetComponent<TMP_Text>().text;
            }
        }

        //JSON으로 저장
        string newFolderData = JsonConvert.SerializeObject(thisProject);
        UserManager.Instance.folders.Add(thisProject.projectTitle, newFolderData);

        //폴더 다시 세팅
        titleArea.transform.GetChild(0).GetComponent<TMP_Text>().text = thisProject.projectType;
        titleArea.transform.GetChild(1).GetComponent<TMP_Text>().text = thisProject.projectTitle;
        modifyContainer.transform.parent.gameObject.SetActive(false);
        FolderPage.SetActive(true);
    }
    #endregion

    //활동폴더 삭제하기
    public GameObject folderRemovePage;
    public void removeFolder()
    {
        UserManager.Instance.folders.Remove(thisProject.projectTitle);
        folderRemovePage.SetActive(false);
        UserManager.Instance.checkFolderDelete = true;
        goHome();
    }

    //씬 전환
    public void goHome() { SceneManager.LoadScene("1_Home"); UserManager.Instance.pushedButton = ""; }
    public void goWriting() { SceneManager.LoadScene("2_Writing"); UserManager.Instance.pushedRecord = ""; }
}
