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
    //public GameObject planners;
    //public GameObject designers;
    //public GameObject frontEnd;
    public GameObject horizontalGroup;
    GameObject newHorizontalGroup;

    public GameObject exChip;
    GameObject newExChip;

    public void SetExChipPage()
    {
        JobContent[0].transform.parent.gameObject.SetActive(true);
        StartCoroutine(SetChip()); }

    GameObject container;
    //List<string> newList = new List<string>();
    string[] newList = new string[17];
    IEnumerator SetChip()
    {
        for(int i = 0; i < 12; i++)
        {
            for(int ii = 0; ii < 17; ii++)
            {if(!string.IsNullOrWhiteSpace(UserManager.Instance.ExperienceChipList[i, ii]))
                newList[ii] = UserManager.Instance.ExperienceChipList[i,ii]; }
            
            if (i == 0) { container = JobContent[0].transform.GetChild(3).gameObject; }
            else if(i<4)
            {
                container = JobContent[0].transform.GetChild(i - 1).gameObject;
            }
            else if (i == 4) { container = JobContent[1].transform.GetChild(4).gameObject; }
            else if (i < 8)
            {
                container = JobContent[1].transform.GetChild(i - 4).gameObject;
            }
            else if( i==8) { container = JobContent[2].transform.GetChild(3).gameObject; }
            else if(i<12) { container = JobContent[2].transform.GetChild(i-9).gameObject; }

            //칩 로드
            for (int ii = 0; ii < 17; ii++)
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

                    if (newHorizontalGroup.GetComponent<RectTransform>().sizeDelta.x >= 350)
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

    public void ChangeJob()
    {
        GameObject thisObj = EventSystem.current.currentSelectedGameObject;
        
        for (int i = 0; i < JobContent.Length; i++)
        {
            if((i+1)== thisObj.transform.GetSiblingIndex()) { JobContent[i].SetActive(true); }
            else JobContent[i].SetActive(false);
        }
        ChangeJobPage.SetActive(false);
    }
}
