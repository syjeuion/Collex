using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using TMPro;

public class SetEXchip : MonoBehaviour
{
    public GameObject WritingManager;
    public GameObject skeleton;

    public GameObject planners;
    public GameObject designers;
    public GameObject horizontalGroup;
    GameObject newHorizontalGroup;

    public GameObject exChip;
    GameObject newExChip;

    const string ExURL = "https://docs.google.com/spreadsheets/d/1eeIYaIaWKZZdIQ_uJALBBgKEpr23Ak3Zfwe8Dv80NTI/export?format=tsv&gid=198678164&range=A2:H18";

    bool isItFirst = true;
    public void setExStart()
    {
        planners.transform.parent.gameObject.SetActive(true);
        if (isItFirst)
        {
            skeleton.SetActive(true);
            StartCoroutine(DownloadPlannerEx());
            isItFirst = false;
        }
    }

    IEnumerator DownloadPlannerEx()
    {
        //경험 데이터 받아오기
        UnityWebRequest Plannerwww = UnityWebRequest.Get(ExURL);
        yield return Plannerwww.SendWebRequest();
        string PlannerData = Plannerwww.downloadHandler.text;

        skeleton.SetActive(false);
        setData(PlannerData);
    }

    //기획
    List<string> ServicePlanning = new List<string>();
    List<string> UXPlanning = new List<string>();
    List<string> PmPo = new List<string>();
    List<string> BusinessPlanning = new List<string>();
    //디자인
    List<string> UxUiDesign = new List<string>();
    List<string> ImageDesign = new List<string>();
    List<string> BrandDesign = new List<string>();
    List<string> GraphicsDesign = new List<string>();

    void setData(string tsv)
    {
        string[] row = tsv.Split('\n');
        int rowSize = row.Length;
        int columnSize = row[0].Split('\t').Length;

        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split('\t');
            //기획
            ServicePlanning.Add(column[0]);
            if (!string.IsNullOrWhiteSpace(column[1])) UXPlanning.Add(column[1]);
            if (!string.IsNullOrWhiteSpace(column[2])) PmPo.Add(column[2]);
            if (!string.IsNullOrWhiteSpace(column[3])) BusinessPlanning.Add(column[3]);
            //디자인
            if (!string.IsNullOrWhiteSpace(column[4])) UxUiDesign.Add(column[4]);
            if (!string.IsNullOrWhiteSpace(column[5])) ImageDesign.Add(column[5]);
            if (!string.IsNullOrWhiteSpace(column[6])) BrandDesign.Add(column[6]);
            if (!string.IsNullOrWhiteSpace(column[7])) GraphicsDesign.Add(column[7]);
        }
        StartCoroutine(setChip());
    }

    GameObject container;
    List<string> newList = new List<string>();
    IEnumerator setChip()
    {
        for(int i = 0; i < 8; i++)
        {
            
            if (i == 0) { newList = ServicePlanning; container = planners.transform.GetChild(3).gameObject; }
            else if(i<4)
            {
                container = planners.transform.GetChild(i - 1).gameObject;
                if (i == 1) newList = UXPlanning;
                if (i == 2) newList = PmPo;
                if (i == 3) newList = BusinessPlanning;
            }
            else if (i == 4) { newList = UxUiDesign; container = designers.transform.GetChild(4).gameObject; }
            else if (i < 8)
            {
                container = designers.transform.GetChild(i - 4).gameObject;
                if (i == 5) newList = ImageDesign;
                if (i == 6) newList = BrandDesign;
                if (i == 7) newList = GraphicsDesign;
            }

            //칩 로드
            for (int ii = 0; ii < newList.Count; ii++)
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
