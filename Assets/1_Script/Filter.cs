using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using TMPro;

public class Filter : MonoBehaviour
{
    /*
    public GameObject filterTabs;
    public GameObject FilterPage;
    public GameObject FolderManager;
    public GameObject explanation;

    //필터
    List<string> defaultTitles = new List<string>();
    List<string> filterTitles = new List<string>();

    //컬러
    Color primary3;
    Color gray700;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#408BFD", out primary3);
        ColorUtility.TryParseHtmlString("#575F6B", out gray700);
    }

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
            StartCoroutine(FolderManager.GetComponent<FolderManager>().outputRecords(defaultTitles));
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
                    if (newDailyRecord.capabilities.Contains(capa) && !filterTitles.Contains(keys)) { filterTitles.Add(keys); }
                }
                foreach (string hash in filterHash)
                {
                    if (newDailyRecord.hashtags.Contains(hash) && !filterTitles.Contains(keys)) { filterTitles.Add(keys); }
                }
            }

            StartCoroutine(FolderManager.GetComponent<FolderManager>().outputRecords(filterTitles));
            explanation.GetComponent<Text>().text = "조건에 해당하는 기록이 없어요.\n다른 조건으로 다시 설정해 보세요!";
        }
        FilterPage.SetActive(false);
    }*/
}
