using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Newtonsoft.Json;

public class SearchManager : MonoBehaviour
{
    //Search Bar
    public GameObject SearchBar;
    public TMP_InputField inputSearchKeyword;

    //검색 전 칩 로드
    public GameObject JobExperiences;
    public GameObject HashtagTop5;
    public GameObject horizontalGroup;
    GameObject newHorizontalGroup;
    public GameObject searchChipPrefab;
    GameObject newSearchChip;

    Dictionary<string, int> sortedExperiences;
    Dictionary<string, int> sortedHashtags;
    List<string> sortedEx=new List<string>();
    List<string> sortedTag= new List<string>();

    //검색 후 결과
    public GameObject ScrollView;
    public GameObject SearchResultContent;
    public GameObject SearchResultField;
    GameObject newSearchResult;

    public Sprite searchIcon;
    public Sprite deleteIcon;

    

    //컬러
    Color grayColor_BD = new Color(0.74f, 0.74f, 0.74f); //#BDBDBD
    Color grayColor_42 = new Color(0.26f, 0.26f, 0.26f); //#424242
    Color grayColor_E0 = new Color(0.88f, 0.88f, 0.88f); //#E0E0E0

    private void Start()
    {
        sortedExperiences = UserManager.Instance.AllExperiences.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        sortedHashtags = UserManager.Instance.AllHashtags.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        StartCoroutine(setSearchChips(sortedExperiences, JobExperiences, sortedEx));
    }

    private void Update()
    {
        JobExperiences.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 31.9f;
        JobExperiences.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 32;

        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) goHome();
    }
    #region 검색 전 칩 로드
    int listCount;
    IEnumerator setSearchChips(Dictionary<string,int> thisDictionary,GameObject parent, List<string> sortedKey)
    {
        foreach (string key in thisDictionary.Keys) sortedKey.Add(key);
        if (sortedKey.Count <= 0) { yield break; }
        else
        {
            parent.transform.GetChild(1).gameObject.SetActive(false);
            if (sortedKey.Count >= 5) { listCount = 5; parent.transform.GetChild(0).GetComponent<TMP_Text>().text += " Top 5"; }
            else listCount = sortedKey.Count;
        }


        for (int i = 0; i < listCount; i++)
        {
            if (i == 0) newHorizontalGroup = Instantiate(horizontalGroup, parent.transform);
            
            newSearchChip = Instantiate(searchChipPrefab, newHorizontalGroup.transform);
            newSearchChip.transform.GetChild(0).GetComponent<TMP_Text>().text = sortedKey[i];
            yield return new WaitForEndOfFrame();

            float chipWidth = newSearchChip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            newSearchChip.GetComponent<RectTransform>().sizeDelta = new Vector2(chipWidth + 24f, newSearchChip.GetComponent<RectTransform>().sizeDelta.y);
            yield return new WaitForEndOfFrame();

            if (newHorizontalGroup.GetComponent<RectTransform>().sizeDelta.x >= 350)
            {
                newHorizontalGroup = Instantiate(horizontalGroup, parent.transform);
                newSearchChip.transform.SetParent(newHorizontalGroup.transform);
            }
        }

        if(parent.name == "JobExperiences_Top5") StartCoroutine(setSearchChips(sortedHashtags, HashtagTop5, sortedTag));
    }
    #endregion

    public void backToSearchDefault()
    {
        SearchBar.transform.GetChild(0).gameObject.SetActive(false);
        SearchBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(328, SearchBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta.y);
        SearchBar.GetComponent<HorizontalLayoutGroup>().padding.left = 16;
        inputSearchKeyword.text = "";

        SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite = searchIcon;
        SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = grayColor_BD;
        JobExperiences.transform.parent.gameObject.SetActive(true);
        ScrollView.SetActive(false);
    }

    public void checkInputNull()
    {
        if(SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite != searchIcon)
            SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite = searchIcon;

        if (!string.IsNullOrWhiteSpace(inputSearchKeyword.text))
            SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = grayColor_BD;
        else
            SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = grayColor_E0;
    }

    public void searchAndCloseIcon()
    {
        if (string.IsNullOrWhiteSpace(inputSearchKeyword.text)) return;
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        if(currentObj.name == "SearchButton")
        {
            if (currentObj.transform.GetChild(0).GetComponent<Image>().sprite == searchIcon)
                letsSearch(); 
            else if(currentObj.transform.GetChild(0).GetComponent<Image>().sprite == deleteIcon)
            {
                inputSearchKeyword.text = "";
                SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite = searchIcon;
                SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = grayColor_BD;
            }
        }
    }

    void letsSearch()
    {
        SearchBar.transform.GetChild(0).gameObject.SetActive(true);
        SearchBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(292, SearchBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta.y);
        SearchBar.GetComponent<HorizontalLayoutGroup>().padding.left = 4;
        
        SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite = deleteIcon;
        SearchBar.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = grayColor_42;
        
        JobExperiences.transform.parent.gameObject.SetActive(false);
        ScrollView.SetActive(true);
        StartCoroutine(KeywordSearch());
    }

    #region 검색
    string recordTitle;
    string searchResultWriting;
    string capas;
    
    IEnumerator KeywordSearch()
    {
        SearchResultContent.transform.GetChild(0).gameObject.SetActive(true);

        string pattern = @inputSearchKeyword.text;
        string replacement = "<color=#408BFD>" + inputSearchKeyword.text + "</color>";

        //기존 결과 리셋
        for (int i=2;i<SearchResultContent.transform.childCount;i++)
            Destroy(SearchResultContent.transform.GetChild(i).gameObject);

        //모든 폴더 스캔
        foreach (string value in UserManager.Instance.folders.Values)
        {
            MakeNewProject newProject = JsonConvert.DeserializeObject<MakeNewProject>(value);
            //해당 폴더 내 모든 기록 스캔
            foreach(string records in newProject.records.Values)
            {
                if (records.Contains(inputSearchKeyword.text)) //해당 기록이 검색어를 포함하고 있으면
                {
                    DailyRecord newRecord = JsonConvert.DeserializeObject<DailyRecord>(records);
                    if (newRecord.title.Contains(inputSearchKeyword.text))
                        recordTitle = Regex.Replace(newRecord.title, pattern, replacement);
                    else
                        recordTitle = newRecord.title;

                    //어떤 필드에 있는지 체크
                    List<string> writingKeys = new List<string>();
                    foreach (string key in newRecord.writings.Keys) writingKeys.Add(key);

                    for(int i=0;i<writingKeys.Count;i++)
                    {
                        searchResultWriting = "";//초기화
                        if (newRecord.writings[writingKeys[i]].Contains(inputSearchKeyword.text)) //포함하고 있으면
                        {
                            searchResultWriting = Regex.Replace(newRecord.writings[writingKeys[i]], pattern, replacement);
                            break;
                        }
                        else if(i== writingKeys.Count-1)
                            searchResultWriting = newRecord.writings[writingKeys[0]];
                    }

                    capas = "";//초기화
                    for (int i = 0; i < 3; i++)
                    {
                        if (newRecord.capabilities[i] == null) { break; }
                        else if (i != 0) { capas += " ・ "; }
                        capas += newRecord.capabilities[i];
                    }
                    
                    newSearchResult = Instantiate(SearchResultField, SearchResultContent.transform);
                    newSearchResult.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = newProject.projectTitle;
                    newSearchResult.transform.GetChild(1).GetComponent<TMP_Text>().text = recordTitle;
                    newSearchResult.transform.GetChild(2).GetComponent<TMP_Text>().text = searchResultWriting;
                    newSearchResult.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = capas; 
                    newSearchResult.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = newRecord.date;
                    newSearchResult.GetComponent<Button>().onClick.AddListener(clickRecord);
                    yield return new WaitForEndOfFrame();

                    newSearchResult.GetComponent<VerticalLayoutGroup>().spacing = 3.9f;
                    newSearchResult.GetComponent<VerticalLayoutGroup>().spacing = 4;
                }
                
            }
        }
        int countResult = SearchResultContent.transform.childCount - 2;
        SearchResultContent.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "검색 결과 "+countResult.ToString()+"개";

        yield return new WaitForEndOfFrame();
        if (SearchResultContent.transform.childCount <= 2)
        {
            SearchResultContent.transform.GetChild(0).gameObject.SetActive(false);
            SearchResultContent.transform.GetChild(1).gameObject.SetActive(true);
            SearchResultContent.transform.GetChild(1).GetComponent<TMP_Text>().text = "'" + inputSearchKeyword.text + "’에 해당하는 활동 기록이 없어요.\n다른 기록의 내용으로 검색해보세요!";
        }
        else SearchResultContent.transform.GetChild(1).gameObject.SetActive(false); 
    }
    #endregion

    //기록 상세페이지
    public void clickRecord()
    {
        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        UserManager.Instance.pushedButton = clickButton.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text;
        string pushedRecordTitle = clickButton.transform.GetChild(1).GetComponent<TMP_Text>().text;
        string pattern = "<.*?>";
        UserManager.Instance.pushedRecord = Regex.Replace(pushedRecordTitle, pattern, "");
        DontDestroyCanvas.setRecord();
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goRanking() { SceneManager.LoadScene("5_Ranking"); }
    public void goMypage() { SceneManager.LoadScene("6_Mypage"); }
}
