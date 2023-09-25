using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public struct Pair
{
    public int x;
    public int y;
    public Pair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
public struct ProjectType
{
    public string projectType;
    public string projectTypeDescription;
    public ProjectType(string projectType, string projectTypeDescription)
    {
        this.projectType = projectType;
        this.projectTypeDescription = projectTypeDescription;
    }
}

public class Report : MonoBehaviour
{
    public GameObject allGraphs;

    public LineRenderer AllFolderGraph;
    public LineRenderer ThisFolderGraph;

    public Sprite[] episodeTypeArr;
    public Image episodeType;

    MakeNewProject thisProject;
    private string thisUserName;

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

    //글쓰기 필드 별 타입
    Dictionary<string, string> EpisodeDescriptions = new Dictionary<string, string>()
    {
        {"문제상황","" },{"문제원인","" },{"해결과정","" },{"배운점","" },{"잘한점","" },{"부족한점","" }
    };
    private List<ProjectType> projectTypes;
    //    = new List<ProjectType>()
    //{
    //    new ProjectType("문제 탐험가",$"이 활동에서는 <b><color=#1E2024>문제상황</color></b>에 대한 기록을 많이 작성하셨군요. 많은 문제들로 힘든 활동이었지만, 상황을 인지하고 극복했던 경험은 {thisUserName}님의 역량을 증명하는데 큰 도움이 될 거예요. 힘내세요!"),
    //    new ProjectType("전문 분석가",$"이 활동에서는 <b><color=#1E2024>문제원인</color></b>에 대한 기록을 많이 작성하셨군요. 문제원인 분석은 문제해결에 대한 첫 걸음으로, 이렇게 쌓아올린 Name 님의 분석력은 앞으로 일어날 다양한 문제를 해결할 수 있는 밑거름이 될 거예요!"),
    //    new ProjectType("특급 해결사",$"이 활동에서는 <b><color=#1E2024>문제해결과정</color></b>에 대한 기록을 많이 작성하셨군요. 좋은 해결방법을 찾기 위해 노력하는 모습이 정말 멋져요! 자기소개서에서 문제해결 과정이 중요한 만큼 이 활동을 나중에 소재로 활용해보는 건 어때요?"),
    //    new ProjectType("프로 성장러",$"이 활동에서는 <b><color=#1E2024>배운점</color></b>에 대한 기록을 많이 작성하셨군요. 배운 점이 많다는 것은 그만큼 Name 님의 역량이 많이 성장했다는 의미이기도 해요. 배움을 얻은 계기와 함께 좋은 에피소드로 활용해 보아요!"),
    //    new ProjectType("슈퍼 루키",$"이 활동에서는 <b><color=#1E2024>잘한점</color></b>에 대한 기록을 많이 작성하셨군요.  지금까지 많은 잘한 점을 기록한 만큼 Name 님이 가진 역량은 무궁무진할 것입니다. 이를 자신만의 확실한 장점으로 발전시켜 보세요!"),
    //    new ProjectType("대기만성 인재",$"이 활동에서는 <b><color=#1E2024>부족한점</color></b>에 대한 기록을 많이 작성하셨군요. 자신의 부족한 점을 인지하고 보완한다면 Name님의 잠재력을 입증할 수 있는 하나의 방법이 될 수 있어요. Name님은 분명 더 큰 성장을 이룰 수 있을 거에요. 언제나 응원합니다!")
    //};

    private void Start()
    {
        string thisFolderDatas = UserManager.Instance.folders[UserManager.Instance.pushedButton];
        thisProject = JsonConvert.DeserializeObject<MakeNewProject>(thisFolderDatas);
        thisUserName = UserManager.Instance.newUserInformation.userName;

        projectTypes = new List<ProjectType>()
        {
            new ProjectType("문제 탐험가",$"이 활동에서는 <b><color=#1E2024>문제상황</color></b>에 대한 기록을 많이 작성하셨군요. 많은 문제들로 힘든 활동이었지만, 상황을 인지하고 극복했던 경험은 {thisUserName}님의 역량을 증명하는데 큰 도움이 될 거예요. 힘내세요!"),
            new ProjectType("전문 분석가",$"이 활동에서는 <b><color=#1E2024>문제원인</color></b>에 대한 기록을 많이 작성하셨군요. 문제원인 분석은 문제해결에 대한 첫 걸음으로, 이렇게 쌓아올린 {thisUserName}님의 분석력은 앞으로 일어날 다양한 문제를 해결할 수 있는 밑거름이 될 거예요!"),
            new ProjectType("특급 해결사",$"이 활동에서는 <b><color=#1E2024>문제해결과정</color></b>에 대한 기록을 많이 작성하셨군요. 좋은 해결방법을 찾기 위해 노력하는 모습이 정말 멋져요! 자기소개서에서 문제해결 과정이 중요한 만큼 이 활동을 나중에 소재로 활용해보는 건 어때요?"),
            new ProjectType("프로 성장러",$"이 활동에서는 <b><color=#1E2024>배운점</color></b>에 대한 기록을 많이 작성하셨군요. 배운 점이 많다는 것은 그만큼 {thisUserName}님의 역량이 많이 성장했다는 의미이기도 해요. 배움을 얻은 계기와 함께 좋은 에피소드로 활용해 보아요!"),
            new ProjectType("슈퍼 루키",$"이 활동에서는 <b><color=#1E2024>잘한점</color></b>에 대한 기록을 많이 작성하셨군요.  지금까지 많은 잘한 점을 기록한 만큼 {thisUserName}님이 가진 역량은 무궁무진할 것입니다. 이를 자신만의 확실한 장점으로 발전시켜 보세요!"),
            new ProjectType("대기만성 인재",$"이 활동에서는 <b><color=#1E2024>부족한점</color></b>에 대한 기록을 많이 작성하셨군요. 자신의 부족한 점을 인지하고 보완한다면 {thisUserName}님의 잠재력을 입증할 수 있는 하나의 방법이 될 수 있어요. {thisUserName}님은 분명 더 큰 성장을 이룰 수 있을 거에요. 언제나 응원합니다!")
        };
    }

    #region 활동 유형
    int order;
    public string[] setEpisodeType()
    {
        string[] projectType = new string[2];
        Dictionary<string, int> sortedWrtings = thisProject.writings.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        //가장 많은 필드
        List<string> sortedKeys = new List<string>();
        foreach (string key in sortedWrtings.Keys) sortedKeys.Add(key);
        
        if (sortedKeys[0] == "문제상황") order = 0;
        if (sortedKeys[0] == "문제원인") order = 1;
        if (sortedKeys[0] == "해결과정") order = 2;
        if (sortedKeys[0] == "배운점") order = 3;
        if (sortedKeys[0] == "잘한점") order = 4;
        if (sortedKeys[0] == "부족한점") order = 5;
        projectType[0] = projectTypes[order].projectType;
        projectType[1] = projectTypes[order].projectTypeDescription;
        episodeType.sprite = episodeTypeArr[order];
        return projectType;
    }


    #endregion

    #region 그래프
    public void setGraph()
    {
        drowGraph(thisProject.capabilities, ThisFolderGraph);
        if (UserManager.Instance.folders.Count == 1)
        {
            allGraphs.SetActive(false);
            AllFolderGraph.gameObject.SetActive(false);
        }
        else drowGraph(UserManager.Instance.Allcapabilites, AllFolderGraph);
    }
    int check = 0;
    public string MostCapability;
    //public string returnMostCapa() { return MostCapability; }

    void drowGraph(Dictionary<string,int> setDictionary, LineRenderer setLinRenderer)
    {
        //전체 역량 내림차순으로 정렬
        Dictionary<string, int> sortedAllcapabilites = setDictionary.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        //키 리스트
        List<string> sortedKeys = new List<string>();
        foreach (string key in sortedAllcapabilites.Keys) sortedKeys.Add(key);

        if (check == 0) { MostCapability = sortedKeys[0]; check++; }

        //최댓값
        int maxValue = sortedAllcapabilites[sortedKeys[0]];

        //max값 기준으로 나눠서 반올림 후 소숫점 첫째자리 저장(실제 표시될 값)
        List<int> sortedValues = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            if (maxValue != 0)
            {
                if (i == 0) sortedValues.Add(10);
                else
                {
                    double division = (double)sortedAllcapabilites[sortedKeys[i]] / (double)maxValue;
                    sortedValues.Add((int)(Math.Round(division, 1) * 10));
                }
            }
            else sortedValues.Add(0);
        }

        //그래프 그리기
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            if (sortedKeys[i] == "커뮤니케이션능력")
            {
                setLinRenderer.SetPosition(0, new Vector3(0, topPairs[sortedValues[i]], 0));
                setLinRenderer.SetPosition(5, new Vector3(0, topPairs[sortedValues[i]], 0));
            }
            else if (sortedKeys[i] == "리더십")
                setLinRenderer.SetPosition(1, new Vector3(rightPairs[sortedValues[i]].x, rightPairs[sortedValues[i]].y, 0));
            else if (sortedKeys[i] == "문제해결능력")
                setLinRenderer.SetPosition(2, new Vector3(downPairs[sortedValues[i]].x, downPairs[sortedValues[i]].y, 0));
            else if (sortedKeys[i] == "통찰력")
                setLinRenderer.SetPosition(3, new Vector3(-downPairs[sortedValues[i]].x, downPairs[sortedValues[i]].y, 0));
            else
                setLinRenderer.SetPosition(4, new Vector3(-rightPairs[sortedValues[i]].x, rightPairs[sortedValues[i]].y, 0));
        }
    }
    #endregion

    #region 획득 경험


    #endregion
}
