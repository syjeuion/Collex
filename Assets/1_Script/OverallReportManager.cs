using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class OverallReportManager : MonoBehaviour
{
    MakeNewProject newProject;
    DailyRecord newRecord;

    //Home UI
    public GameObject home_capabilityLock;
    public GameObject home_capabilityGroup;
    public TMP_Text home_recordCountThisWeek;
    public TMP_Text home_description;
    [SerializeField] Sprite[] capability_icons;

    //전체 활동 리포트 UI
    public GameObject page_overallReport;
    public GameObject content_overall;

    //내부 변수
    string[] home_descriptions;
    int totalRecordCount;
    int thisWeekRecordCount;
    int dayGap;

    //색상
    Color primary3;
    Color gray900;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#1E2024", out gray900);

        home_descriptions = new string[] {
        "아직 기록하지 않았어요!",
        "2주 이상 기록하지 않았어요!",
        "2주 이상 기록해 보세요!"};

        totalRecordCount = UserManager.Instance.newUserInformation.titleCheck[7];
    }

    void Start()
    {
        //Home UI Setting
        //5개 이상 기록시 open!
        if (totalRecordCount >= 5){
            home_capabilityLock.SetActive(false);
            home_capabilityGroup.SetActive(true);

            string theMostCapability = getMostCapability();
            home_capabilityGroup.transform.GetChild(1).GetComponent<TMP_Text>().text = theMostCapability;
            home_capabilityGroup.transform.GetChild(0).GetComponent<Image>().sprite = capability_icons[capabilityIndex(theMostCapability)];
        }else {
            home_capabilityLock.SetActive(true);
            home_capabilityGroup.SetActive(false);
        }
        //description
        home_description.text = getHomeDescription();
        //이번주 기록
        home_recordCountThisWeek.text = UserManager.Instance.newUserInformation.recordCountThisWeek.ToString()+"개";
    }

    #region 홈 Card UI
    //나의 최고 역량
    private string getMostCapability()
    {
        Dictionary<string, int> Allcapabilites = UserManager.Instance.Allcapabilites.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        //키 리스트
        List<string> sortedKeys = new List<string>();
        foreach (string key in Allcapabilites.Keys) sortedKeys.Add(key);
        return sortedKeys[0];
    }
    //home_description
    private string getHomeDescription()
    {
        int recordCountThisWeek = UserManager.Instance.newUserInformation.recordCountThisWeek;
        int recordCountLastWeek = UserManager.Instance.newUserInformation.recordCountLastWeek;
        int recordCountWeekGap = recordCountThisWeek - recordCountLastWeek;
        int totalRecordCount = UserManager.Instance.newUserInformation.titleCheck[7];

        if (totalRecordCount == 0){
            return home_descriptions[0];
        }
        else if(recordCountThisWeek==0 && recordCountLastWeek == 0)
        {
            return home_descriptions[1];
        }
        else if((DateTime.Now - UserManager.Instance.newUserInformation.userSignUpDate).Days <= 7)
        {
            return home_descriptions[2];
        }
        else if(recordCountWeekGap > 0)
        {
            return $"지난주보다 {recordCountWeekGap}개 많이 기록했어요!";
        }
        else if (recordCountWeekGap < 0)
        {
            return $"지난주보다 {recordCountWeekGap*-1}개 적게 기록했어요!";
        }
        else //recordCountWeekGap = 0
        {
            return "";
        }
        
    }

    private int capabilityIndex(string capability)
    {
        int index = 0;
        switch (capability)
        {
            case "통찰력": index = 0; break;
            case "리더십": index = 1; break;
            case "팀워크": index = 2; break;
            case "문제해결능력": index = 3; break;
            case "커뮤니케이션능력": index = 4; break;
        }
        return index;
    }
    #endregion

    #region 전체 리포트
    public TMP_Text[] folderCounts; //전체 폴더 개수
    public TMP_Text[] totalRecordsTmp; //전체 기록 개수
    //요일 그래프
    public TMP_Text countDayOfWeek; 
    public GameObject[] graphDayOfWeek;
    public TMP_Text[] recordCountThisMonthTmp; //이번달 기록 개수
    //역량
    public TMP_Text[] capabilitiesTmp; 
    public TMP_Text[] capabilityGraphTexts; 
    public LineRenderer AllFolderGraph;
    public GameObject blur_capability;
    //경험
    public GameObject Experiences;
    //public GameObject containerExRanking;
    public GameObject prefabExRanknig;
    GameObject newPrefabExRanking;
    public GameObject blur_experiences;
    GameObject btn_showAllEx;

    public void openOverallReport()
    {
        page_overallReport.SetActive(true);
        content_overall.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);

        //전체 폴더 개수 - Summary - folderCount
        int project = UserManager.Instance.newUserInformation.projectType["프로젝트"];
        int contest = UserManager.Instance.newUserInformation.projectType["공모전"];
        int intern = UserManager.Instance.newUserInformation.projectType["인턴십"];
        folderCounts[0].text = (project + contest + intern).ToString();
        folderCounts[1].text = project.ToString();
        folderCounts[2].text = contest.ToString();
        folderCounts[3].text = intern.ToString();

        //전체 기록 개수 - Total Records Count
        totalRecordsTmp[0].text = totalRecordCount.ToString()+"개";
        if (totalRecordCount < 10){
            totalRecordsTmp[1].text = "총          의 기록을 작성했어요!";
        }
        else if (totalRecordCount < 100){
            totalRecordsTmp[1].text = "총             의 기록을 작성했어요!";
        } else{
            totalRecordsTmp[1].text = "총                 의 기록을 작성했어요!";
        }

        //가장 많이 작성한 요일 - The most day of the week
        countDayOfWeek.text = drawGraphDayOfWeek(UserManager.Instance.newUserInformation.recordDayOfWeek,"요일") +"요일";

        //이번달 기록 개수 - Records Count of this month
        recordCountThisMonthTmp[0].text = $"{DateTime.Now.Month}월 기록 개수";
        int recordCountThisMonth = UserManager.Instance.newUserInformation.recordCountThisMonth;
        recordCountThisMonthTmp[1].text = $"{recordCountThisMonth}개";
        if (recordCountThisMonth < 10) {
            recordCountThisMonthTmp[2].text = "이번 달에는          의 기록을 작성했어요!";
        }else if (recordCountThisMonth < 100)
        {
            recordCountThisMonthTmp[2].text = "이번 달에는             의 기록을 작성했어요!";
        }
        else
        {
            recordCountThisMonthTmp[2].text = "이번 달에는                의 기록을 작성했어요!";
        }
        recordCountThisMonthTmp[3].text = $"그리고 이번 주에는 {UserManager.Instance.newUserInformation.recordCountThisWeek}개의 기록을 작성했네요.";

        //전체 역량 - Capabilities
        if (UserManager.Instance.Allcapabilites.Count <= 0)
        {
            blur_capability.SetActive(true);
            AllFolderGraph.gameObject.SetActive(false);
        }
        else
        {
            blur_capability.SetActive(false);
            AllFolderGraph.gameObject.SetActive(true);

            string mostCapability = drawGraphDayOfWeek(UserManager.Instance.Allcapabilites, "역량");
            capabilitiesTmp[0].text = mostCapability;

            if (mostCapability == "커뮤니케이션능력")
            { capabilitiesTmp[1].text = "                                        이 뛰어나요!"; }
            else if (mostCapability == "문제해결능력")
            { capabilitiesTmp[1].text = "                              이 뛰어나요!"; }
            else
            { capabilitiesTmp[1].text = "               이 뛰어나요!"; }
        }

        //획득 경험 - Experiences
        if (UserManager.Instance.AllExperiences.Count <= 0)
        {
            blur_experiences.SetActive(true);
            Experiences.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            blur_experiences.SetActive(false);
            Experiences.transform.GetChild(1).gameObject.SetActive(true);
            StartCoroutine(setExperiences());
        }
    }

    //그래프 그리기 - 내림차순으로 정렬하기
    private string drawGraphDayOfWeek(Dictionary<string, int> dictionary, string type)
    {
        //Dictionray 내림차순으로 정렬
        Dictionary<string, int> sortedDictionray = dictionary.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        //키 리스트
        List<string> sortedKeys = new List<string>();
        foreach (string key in sortedDictionray.Keys) sortedKeys.Add(key);

        //최댓값
        int maxValue = sortedDictionray[sortedKeys[0]];

        //max값 기준으로 나눠서 반올림 후 소숫점 첫째자리 저장(실제 표시될 값)
        List<int> sortedValues = new List<int>();

        int range;
        if (type == "요일") { range = 7; }
        else { range = 5; }

        for (int i = 0; i < range; i++)
        {
            if (maxValue != 0)
            {
                if (i == 0) sortedValues.Add(10);
                else
                {
                    double division = (double)sortedDictionray[sortedKeys[i]] / (double)maxValue;
                    if (division == 0) { sortedValues.Add(0); }
                    else { sortedValues.Add((int)(Math.Round(division, 1) * 10)); }
                }
            }
            else sortedValues.Add(0);
        }
        if (type == "요일") { drawBarGraph(sortedKeys, sortedValues); }
        else if(type == "역량") { drawPentagonalGraph(sortedKeys, sortedValues); }
        
        return sortedKeys[0];
    }
    #region 막대 그래프 그리기 - 기록 가장 많이 작성한 요일
    private void drawBarGraph(List<string> sortedKeys,List<int> sortedValues)
    {
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            //if (sortedValues[i] == 0) { sortedValues[i] = 10; }
            //else { sortedValues[i] *= 10; }
            sortedValues[i] *= 10;

            if (sortedKeys[i] == "월")
            {
                graphDayOfWeek[0].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(0); }
            }
            else if (sortedKeys[i] == "화")
            {
                graphDayOfWeek[1].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(1); }
            }
            else if (sortedKeys[i] == "수")
            {
                graphDayOfWeek[2].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(2); }
            }
            else if (sortedKeys[i] == "목")
            {
                graphDayOfWeek[3].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(3); }
            }
            else if (sortedKeys[i] == "금")
            {
                graphDayOfWeek[4].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(4); }
            }
            else if (sortedKeys[i] == "토")
            {
                graphDayOfWeek[5].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(5); }
            }
            else //일
            {
                graphDayOfWeek[6].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sortedValues[i]);
                if (i == 0) { graphColorChange(6); }
            }
        }
    }
    private void graphColorChange(int index)
    {
        graphDayOfWeek[index].GetComponent<Image>().color = primary3;
    }
    #endregion

    #region 오각형 그래프 그리기 - 역량 분석
    //리포트 좌표
    private List<int> topPairs = new List<int>()
    { -5,-1,3,9,15,23,32,41,50,59,67};
    private List<Pair> rightPairs = new List<Pair>()
    {
        new Pair(2,-6),new Pair(7,-5),new Pair(10,-4),new Pair(16,-2),new Pair(22,2),new Pair(31,3),
        new Pair(40,5),new Pair(49,8),new Pair(60,12),new Pair(68,13),new Pair(78,16)
    };
    private List<Pair> downPairs = new List<Pair>()
    {
        new Pair(2,-8),new Pair(5,-12),new Pair(8,-15),new Pair(11,-19),new Pair(16,-23),new Pair(20,-31),
        new Pair(26,-40),new Pair(30,-46),new Pair(37,-53),new Pair(41,-61),new Pair(48,-69)
    };
    //그래프 그리기
    private void drawPentagonalGraph(List<string> sortedKeys, List<int> sortedValues)
    {
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            if (sortedKeys[i] == "커뮤니케이션능력")
            {
                if (i == 0) { capabilityGraphTexts[0].color = gray900; }
                AllFolderGraph.SetPosition(0, new Vector3(0, topPairs[sortedValues[i]], 0));
                AllFolderGraph.SetPosition(5, new Vector3(0, topPairs[sortedValues[i]], 0));
            }
            else if (sortedKeys[i] == "리더십")
            {
                if (i == 0) { capabilityGraphTexts[1].color = gray900; }
                AllFolderGraph.SetPosition(1, new Vector3(rightPairs[sortedValues[i]].x, rightPairs[sortedValues[i]].y, 0));
            }
            else if (sortedKeys[i] == "문제해결능력")
            {
                if (i == 0) { capabilityGraphTexts[2].color = gray900; }
                AllFolderGraph.SetPosition(2, new Vector3(downPairs[sortedValues[i]].x, downPairs[sortedValues[i]].y, 0));
            }
            else if (sortedKeys[i] == "통찰력")
            {
                if (i == 0) { capabilityGraphTexts[3].color = gray900; }
                AllFolderGraph.SetPosition(3, new Vector3(-downPairs[sortedValues[i]].x, downPairs[sortedValues[i]].y, 0));
            }
            else
            {
                if (i == 0) { capabilityGraphTexts[4].color = gray900; }
                AllFolderGraph.SetPosition(4, new Vector3(-rightPairs[sortedValues[i]].x, rightPairs[sortedValues[i]].y, 0));
            }
        }
    }
    #endregion

    //획득 경험
    bool firstOpen = true;
    int exTotalRange;
    Dictionary<string, int> sortedExList;
    List<string> sortedKeys;
    IEnumerator setExperiences()
    {
        //리셋
        for (int i=0; i< Experiences.transform.GetChild(1).childCount; i++)
        {
            Destroy(Experiences.transform.GetChild(1).GetChild(i).gameObject);
        }

        //Dictionray 내림차순으로 정렬
        sortedExList = UserManager.Instance.AllExperiences.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        //키 리스트
        sortedKeys = new List<string>();
        foreach (string key in sortedExList.Keys) { sortedKeys.Add(key);}

        btn_showAllEx = Experiences.transform.GetChild(3).gameObject;
        if (sortedKeys.Count <= 5) {
            exTotalRange = sortedKeys.Count;
            btn_showAllEx.SetActive(false);
        }
        else { exTotalRange = 5;
            btn_showAllEx.SetActive(true);
        }

        for (int i = 0; i < exTotalRange; i++)
        {
            SetExRanking(i, Experiences.transform.GetChild(1).gameObject);
        }

        yield return new WaitForEndOfFrame();
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 15.9f;
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 16f;
    }

    //획득 경험 전체 보기 버튼
    bool isBtnOpen = false;
    public void BtnShowAllEx()
    {
        if (isBtnOpen) { StartCoroutine(CloseBtnShowAllEx()); }
        else { StartCoroutine(OpenBtnShowAllEx()); }
    }
    private IEnumerator OpenBtnShowAllEx()
    {
        isBtnOpen = true;
        btn_showAllEx.transform.GetChild(0).GetComponent<TMP_Text>().text = "직무 경험치 줄여서 보기";
        btn_showAllEx.transform.GetChild(1).GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);

        Experiences.transform.GetChild(2).gameObject.SetActive(true);
        if (firstOpen)
        {
            for (int i = 5; i < sortedExList.Count; i++)
            {
                SetExRanking(i, Experiences.transform.GetChild(2).gameObject);
            }
            firstOpen = false;
        }
        yield return new WaitForEndOfFrame();
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 15.9f;
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 16f;
    }
    //획득 경험 전체 보기 닫기
    private IEnumerator CloseBtnShowAllEx()
    {
        isBtnOpen = false;
        btn_showAllEx.transform.GetChild(0).GetComponent<TMP_Text>().text = "모든 직무 경험치 보기";
        btn_showAllEx.transform.GetChild(1).GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
        Experiences.transform.GetChild(2).gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 15.9f;
        content_overall.GetComponent<VerticalLayoutGroup>().spacing = 16f;
    }

    //경험 리스트 출력
    int lastRank;
    int lastCount;
    private void SetExRanking(int order, GameObject content)
    {
        bool isTop =false;
        if (order == 0) { 
            lastRank = order;
            lastCount = sortedExList[sortedKeys[order]];
        }

        if(lastCount == sortedExList[sortedKeys[order]])
        {
            order = lastRank;
            if (order == 0) { isTop = true; }
        }
        else
        {
            lastRank = order;
            lastCount = sortedExList[sortedKeys[order]];
        }

        newPrefabExRanking = Instantiate(prefabExRanknig, content.transform);
        if (isTop)
        {
            newPrefabExRanking.transform.GetChild(0).GetComponent<TMP_Text>().color = primary3;
            newPrefabExRanking.transform.GetChild(1).GetComponent<Image>().color = primary3;
            newPrefabExRanking.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = primary3;
            newPrefabExRanking.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().color = primary3;
        }
        newPrefabExRanking.transform.GetChild(0).GetComponent<TMP_Text>().text = (order + 1).ToString();
        newPrefabExRanking.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = sortedKeys[order];
        newPrefabExRanking.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = $"{sortedExList[sortedKeys[order]]}회";
    }
    #endregion
}
