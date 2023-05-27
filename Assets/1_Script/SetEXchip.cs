using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SetEXchip : MonoBehaviour
{
    public GameObject WritingManager;
    public GameObject ChangeJobPage;
    public GameObject skeleton;

    public GameObject[] JobContent;
    public GameObject horizontalGroup;
    GameObject newHorizontalGroup;

    public GameObject exChip;
    GameObject newExChip;

    private void Start()
    {
        StartCoroutine(SetChip());
    }

    //경험칩추가 버튼 클릭
    bool firstOpen = true;
    public void SetExChipPage()
    {
        JobContent[0].transform.parent.SetSiblingIndex(4);
        if (firstOpen)
        {
            for(int i = 0; i < 5; i++)
            {
                if (i == UserManager.Instance.newUserInformation.kindOfJob)
                {   JobContent[i].transform.SetSiblingIndex(4);
                    int userDetailJob = UserManager.Instance.newUserInformation.detailJob;
                    if (userDetailJob == 5)
                    { JobContent[i].transform.GetChild(0).SetSiblingIndex(3); }
                    else { JobContent[i].transform.GetChild(userDetailJob).SetSiblingIndex(3); }
                    
                    if (i == 1)
                    {
                        //JobContent[i].transform.GetChild(userDetailJob + 1).SetSiblingIndex(4);
                        JobContent[i].transform.GetChild(4).GetChild(0).GetChild(0).GetChild(userDetailJob).GetComponent<Toggle>().isOn = true;
                    }
                    else
                    {
                        //JobContent[i].transform.GetChild(userDetailJob).SetSiblingIndex(3);
                        JobContent[i].transform.GetChild(4).GetChild(userDetailJob).GetComponent<Toggle>().isOn = true;
                    }
                    break;
                }
            }
            firstOpen = false;
        }
    }

    GameObject container;
    //List<string> newList = new List<string>();
    string[] newList = new string[17];
    IEnumerator SetChip()
    {
        for(int i = 0; i < 20; i++)
        {
            int listLengh = 0;
            for (int ii = 0; ii < 17; ii++)
            {   if (!string.IsNullOrEmpty(UserManager.Instance.ExperienceChipList[i, ii]))
                { newList[ii] = UserManager.Instance.ExperienceChipList[i, ii]; listLengh++; }
                else break;
            }
            
            if(i<4) { container = JobContent[0].transform.GetChild(i).gameObject; }
            else if (i < 8) { container = JobContent[1].transform.GetChild(i - 4).gameObject; }
            else if(i<12) { container = JobContent[2].transform.GetChild(i-8).gameObject; }
            else if (i < 16) { container = JobContent[3].transform.GetChild(i - 12).gameObject; }
            else { container = JobContent[4].transform.GetChild(i - 16).gameObject; }

            //칩 로드
            for (int ii = 0; ii < listLengh; ii++)
            {
                if (!string.IsNullOrWhiteSpace(newList[ii]))
                {
                    if (ii == 0)
                    {
                        newHorizontalGroup = Instantiate(horizontalGroup, container.transform);
                        newHorizontalGroup.GetComponent<HorizontalLayoutGroup>().padding.top = 24;
                        newHorizontalGroup.GetComponent<HorizontalLayoutGroup>().padding.left = 16;
                    }
                    newExChip = Instantiate(exChip, newHorizontalGroup.transform);
                    newExChip.transform.GetChild(0).GetComponent<TMP_Text>().text = newList[ii];
                    newExChip.GetComponent<Toggle>().onValueChanged.AddListener(WritingManager.GetComponent<WritingManager>().ExperienceUpdate);
                    yield return new WaitForEndOfFrame();

                    float chipWidth = newExChip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
                    newExChip.GetComponent<RectTransform>().sizeDelta = new Vector2(chipWidth + 24f, newExChip.GetComponent<RectTransform>().sizeDelta.y);
                    yield return new WaitForEndOfFrame();

                    if (newHorizontalGroup.GetComponent<RectTransform>().sizeDelta.x >= 328)
                    {
                        newHorizontalGroup = Instantiate(horizontalGroup, container.transform);
                        newHorizontalGroup.GetComponent<HorizontalLayoutGroup>().padding.top = 24;
                        newHorizontalGroup.GetComponent<HorizontalLayoutGroup>().padding.left = 16;
                        newExChip.transform.SetParent(newHorizontalGroup.transform);
                    }
                }
                
            }
        }   
    }

    //직군 선택 바텀시트
    public void ChangeJob()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject;
        
        for (int i = 0; i < JobContent.Length; i++)
        {
            if((i+1)== thisObj.transform.GetSiblingIndex())
            {   JobContent[i].transform.SetSiblingIndex(4);
                JobContent[i].transform.GetChild(0).SetSiblingIndex(3);
            }
        }
        ChangeJobPage.SetActive(false);
    }

    //탭 선택 시 content 활성화
    public void ChangeTab()
    {
        if (EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>() == null) return;
        //int startNum=0;
        string tabName = EventSystem.current.currentSelectedGameObject.name;
        GameObject nowContent = EventSystem.current.currentSelectedGameObject.transform.parent.parent.gameObject;
        
        if (nowContent.name == "Viewport")
        {   nowContent = nowContent.transform.parent.parent.gameObject;
            //startNum = 1;
        } //디자인
        
        for (int i = 0; i < 4; i++)
        {
            if (nowContent.transform.GetChild(i).name == tabName)
            { nowContent.transform.GetChild(i).SetSiblingIndex(3); break; }
        }

    }
}
