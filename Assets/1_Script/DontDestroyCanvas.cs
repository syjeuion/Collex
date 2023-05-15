using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;


public class DontDestroyCanvas : MonoBehaviour
{
    MakeNewProject thisProject;
    DailyRecord newRecord;

    public GameObject toastPopUp;
    public GameObject recordTitleContainer;
    public GameObject recordsContainer;
    public GameObject prefab;
    GameObject newWriting;

    public static Action setRecord; //페이지 세팅 함수
    public static DontDestroyCanvas Instance;

    //칭호 획득 시
    public GameObject ObtainTitlePage;
    public GameObject titleConfirmButton;

    public Animator fireCracker;

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        setRecord = () => { setRecordPage(); };
    }

    //칭호 획득했는지 체크
    int getTitleCount;
    private void Update()
    {
        //if(recordsContainer.transform.parent.GetComponent<VerticalLayoutGroup>().spacing != 24)
        //    recordsContainer.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 24;

        if (UserManager.Instance.getTitle == 1 || UserManager.Instance.getTitle == 2)
        {
            ObtainTitlePage.SetActive(true);
            setObtainTitlePage();
            getTitleCount = UserManager.Instance.nowGetTitle.Count;
            if(UserManager.Instance.getTitle==1) UserManager.Instance.getTitle = 0;
            if (UserManager.Instance.getTitle == 2) UserManager.Instance.getTitle = 3;

        }
    }

    #region 페이지 세팅
    string clickedRecordTitle;
    GameObject folderPage;
    //GameObject bookmarkPage;
    //GameObject toastPopUp;

    private void setRecordPage()
    {
        if (SceneManager.GetActiveScene().name == "3_Folder")
        {
            folderPage = GameObject.Find("FolderPage");
        }
        //else if (SceneManager.GetActiveScene().name == "6_Mypage")
        //{
        //    bookmarkPage = GameObject.Find("BookmarkPage");
        //}

        this.transform.GetChild(0).gameObject.SetActive(true);
        clickedRecordTitle = UserManager.Instance.pushedRecord;

        string thisFolderDatas = UserManager.Instance.folders[UserManager.Instance.pushedButton];
        thisProject = JsonConvert.DeserializeObject<MakeNewProject>(thisFolderDatas);
        string thisRecordData = thisProject.records[clickedRecordTitle];
        newRecord = JsonConvert.DeserializeObject<DailyRecord>(thisRecordData);

        StartCoroutine(SetPage());
    }

    string capas;
    IEnumerator SetPage()
    {
        if (UserManager.Instance.bookmarks.Contains(thisProject.projectTitle + "\t" + newRecord.title))
            transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Toggle>().isOn = true;
        else
            transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Toggle>().isOn = false;

        recordTitleContainer.transform.GetChild(0).GetComponent<TMP_Text>().text = newRecord.title;
        recordTitleContainer.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = newRecord.date;

        if (string.IsNullOrWhiteSpace(newRecord.capabilities[0]))
        {
            recordTitleContainer.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < newRecord.capabilities.Length; i++)
            {
                if (newRecord.capabilities[i] == null) { break; }
                else if (i != 0) { capas += " ・ "; }
                capas += newRecord.capabilities[i];
            }
            recordTitleContainer.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = capas;
            capas = "";
        }

        if (newRecord.experiences.Count == 0)
        {
            recordTitleContainer.transform.GetChild(3).GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < newRecord.experiences.Count; i++)
            {
                if (newRecord.experiences[i] == null) { break; }
                else if (i != 0) { capas += " ・ "; }
                capas += newRecord.experiences[i];
            }
            recordTitleContainer.transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = capas;
            capas = "";
        }

        if (string.IsNullOrWhiteSpace(newRecord.hashtags[0]))
        {
            recordTitleContainer.transform.GetChild(3).GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < newRecord.hashtags.Length; i++)
            {
                if (newRecord.hashtags[i] == null) { break; }
                else if (i != 0) { capas += "  "; }
                capas += "#" + newRecord.hashtags[i];
            }
            recordTitleContainer.transform.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text = capas;
            capas = "";
        }
        yield return new WaitForEndOfFrame();
        if (!recordTitleContainer.transform.GetChild(3).GetChild(0).gameObject.activeSelf &&
            !recordTitleContainer.transform.GetChild(3).GetChild(1).gameObject.activeSelf &&
            !recordTitleContainer.transform.GetChild(3).GetChild(2).gameObject.activeSelf)
        {
            recordTitleContainer.transform.GetChild(3).GetChild(3).gameObject.SetActive(true);
            recordTitleContainer.transform.GetChild(3).GetChild(3).GetComponent<LayoutElement>().ignoreLayout = false;
        }

        for (int i = 0; i < recordsContainer.transform.childCount; i++)
        {
            Destroy(recordsContainer.transform.GetChild(i).gameObject);
        }

        if (newRecord.writings.Keys != null)
        {
            foreach (string key in newRecord.writings.Keys)
            {
                if (newRecord.writings[key] != "")
                {
                    newWriting = Instantiate(prefab, recordsContainer.transform);
                    newWriting.transform.GetChild(0).GetComponent<TMP_Text>().text = key;
                    newWriting.transform.GetChild(1).GetComponent<TMP_Text>().text = newRecord.writings[key];
                    yield return new WaitForEndOfFrame();
                    recordsContainer.GetComponent<VerticalLayoutGroup>().spacing = 15.9f;
                    recordsContainer.GetComponent<VerticalLayoutGroup>().spacing = 16;
                }
            }
        }
        yield return new WaitForEndOfFrame();
        recordsContainer.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 23.9f;
        recordsContainer.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 24f;
    }
    #endregion

    public void RecordInfoToggle()
    {
        StartCoroutine(ToggleCT());
    }
    IEnumerator ToggleCT()
    {
        Toggle toggle = EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();
        if (toggle.isOn)
            recordTitleContainer.transform.GetChild(3).gameObject.SetActive(true);
        else
            recordTitleContainer.transform.GetChild(3).gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        recordTitleContainer.GetComponent<VerticalLayoutGroup>().spacing = 11.9f;
        recordTitleContainer.GetComponent<VerticalLayoutGroup>().spacing = 12f;
    }

    #region 기록문서 삭제, 수정
    //기록문서 삭제하기

    GameObject bookmarkPageContent;
    public void removeRecord()
    {
        thisProject.records.Remove(clickedRecordTitle);

        //JSON으로 저장
        string newFolderData = JsonConvert.SerializeObject(thisProject);
        UserManager.Instance.folders[thisProject.projectTitle] = newFolderData;

        //활동폴더 페이지에서 해당 기록 삭제
        if (SceneManager.GetActiveScene().name == "3_Folder")
        {
            for (int i = 0; i < folderPage.transform.GetChild(1).childCount; i++)
            {
                if (folderPage.transform.GetChild(1).GetChild(i).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == clickedRecordTitle)
                    Destroy(folderPage.transform.GetChild(1).GetChild(i).gameObject);
            }

            int recordCount = thisProject.records.Count;
            folderPage.transform.GetChild(2).GetComponent<TMP_Text>().text = "기록 " + recordCount.ToString() + "개";
            if (recordCount <= 0) folderPage.transform.GetChild(4).gameObject.SetActive(true);

            StartCoroutine(toastPopUpCoroutine());
        }
        if (SceneManager.GetActiveScene().name == "6_Mypage")
        {
            destroyBookmark();
        }

        transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);

        if (UserManager.Instance.bookmarks.Contains(thisProject.projectTitle + "\t" + clickedRecordTitle))
        { UserManager.Instance.bookmarks.Remove(thisProject.projectTitle + "\t" + clickedRecordTitle); }
    }
    IEnumerator toastPopUpCoroutine()
    {
        int childCount = folderPage.transform.childCount;
        GameObject folderToastPopUp = folderPage.transform.GetChild(childCount - 1).gameObject;
        folderToastPopUp.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        folderToastPopUp.SetActive(false);
    }
    //기록문서 수정하기
    public void modifyRecord()
    {
        UserManager.Instance.pushedRecord = clickedRecordTitle;
        goWriting();
        transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    #endregion

    #region 북마크
    public void checkBookmark()
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        if (!currentObj.GetComponent<Toggle>()) return;
        if (currentObj.GetComponent<Toggle>().isOn)
        {
            if (!UserManager.Instance.bookmarks.Contains(thisProject.projectTitle + "\t" + newRecord.title))
            {
                UserManager.Instance.bookmarks.Add(thisProject.projectTitle + "\t" + newRecord.title);
                if (UserManager.Instance.newUserInformation.titleCheck[25] == 0)
                {
                    UserManager.Instance.newUserInformation.titleCheck[25]++;
                    UserManager.Instance.checkTitle("앞", 24, 25);
                    UserManager.Instance.getTitle = 1;
                }
                StartCoroutine(addRemoveToast(1));
            }
            else return;
        }
        else
        {
            if (UserManager.Instance.bookmarks.Contains(thisProject.projectTitle + "\t" + newRecord.title))
            {
                UserManager.Instance.bookmarks.Remove(thisProject.projectTitle + "\t" + newRecord.title);
                StartCoroutine(addRemoveToast(0));
            }
        }
    }

    IEnumerator addRemoveToast(int check)
    {
        toastPopUp.SetActive(true);
        if (check == 1) //on
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text = "북마크 목록에 추가했어요.";
        else //off
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text = "북마크 목록에서 삭제했어요.";
        yield return new WaitForSeconds(2.5f);
        toastPopUp.SetActive(false);
    }
    //북마크에서 뒤로가기
    public void bookmarkBackButton()
    {
        if (SceneManager.GetActiveScene().name == "6_Mypage")
        {
            if (!UserManager.Instance.bookmarks.Contains(thisProject.projectTitle + "\t" + newRecord.title))
            {
                StartCoroutine(destroyBookmark());
            }
        }
        else return;
    }
    //북마크에서 지우기
    IEnumerator destroyBookmark()
    {
        bookmarkPageContent = GameObject.Find("BookmarkContent");
        for (int i = 1; i < bookmarkPageContent.transform.childCount; i++)
        {
            GameObject thisObj = bookmarkPageContent.transform.GetChild(i).gameObject;
            if (thisObj.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text == thisProject.projectTitle)
            {
                if (thisObj.transform.GetChild(1).GetComponent<TMP_Text>().text == clickedRecordTitle)
                {
                    Destroy(thisObj);
                }
            }
        }
        yield return new WaitForEndOfFrame();
        if (bookmarkPageContent.transform.childCount <= 1)
        {
            bookmarkPageContent.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    #endregion

    #region 칭호 획득 페이지
    //GameObject canvas;
    string[] nowTitle = new string[3];
    int order;
    string userJob;
    public void setObtainTitlePage()
    {
        //애니메이션 실행
        fireCracker.SetTrigger("startAni");

        //GameObject canvas = GameObject.Find("Canvas");
        //canvas.GetComponent<Canvas>().enabled = false; //중복 터치 방지
        UIController.instance.SetEnableCanvasState(false);

        if (UserManager.Instance.getTitle == 2)
        {
            ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = "Collex 입사 완료로";
            ObtainTitlePage.transform.GetChild(3).GetComponent<TMP_Text>().text =
                "‘주니어 "+ UserManager.Instance.newUserInformation.userTitleNoun + "' 칭호를 획득했어요!";

            titleConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "홈으로 가기";
            ObtainTitlePage.transform.GetChild(4).gameObject.SetActive(false);
            ObtainTitlePage.transform.GetChild(5).gameObject.SetActive(true);
        }
        else
        {
            nowTitle[0] = UserManager.Instance.nowGetTitle[order][0];
            nowTitle[1] = UserManager.Instance.nowGetTitle[order][1];
            nowTitle[2] = UserManager.Instance.nowGetTitle[order][2];

            ObtainTitlePage.transform.GetChild(3).GetComponent<TMP_Text>().text = "'" + nowTitle[1] + "' 칭호를 획득했어요!";
            string description = nowTitle[2];

            if (description == "Collex 입사 완료" || description == "경험 첫 추가" || description == "입사동기 첫 추가" || description == "활동 첫 마무리")
                ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = description + "로";
            else
                ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = description + "으로";

            titleConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "대표 칭호로 설정하기";
            ObtainTitlePage.transform.GetChild(4).gameObject.SetActive(true);
            ObtainTitlePage.transform.GetChild(5).gameObject.SetActive(false);
        }
    }
    public void titleCancel()
    {
        if (order >= getTitleCount - 1)
        {
            ObtainTitlePage.SetActive(false);
            //GameObject canvas = GameObject.Find("Canvas");
            //canvas.GetComponent<Canvas>().enabled = true;
            UIController.instance.SetEnableCanvasState(true);
            UserManager.Instance.nowGetTitle = new List<string[]>();
            order = 0;
        }
        else
        {
            order++;
            setObtainTitlePage();
        }
    }
    public void titleConfirm()
    {
        if(UserManager.Instance.getTitle == 3)
        {
            goHome();
            ObtainTitlePage.SetActive(false);
            UIController.instance.SetEnableCanvasState(true);
        }
        else
        {
            if (nowTitle[0] == "앞")
                UserManager.Instance.newUserInformation.userTitleModi = nowTitle[1];
            else
                UserManager.Instance.newUserInformation.userTitleNoun = nowTitle[1];

            if (order >= getTitleCount - 1)
            {
                ObtainTitlePage.SetActive(false);
                //GameObject canvas = GameObject.Find("Canvas");
                //canvas.GetComponent<Canvas>().enabled = true;
                UIController.instance.SetEnableCanvasState(true);
                UserManager.Instance.nowGetTitle = new List<string[]>();
                order = 0;
            }
            else
            {
                order++;
                setObtainTitlePage();
            }
        }
        
    }

    #endregion

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goWriting() { SceneManager.LoadScene("2_Writing"); }
}
