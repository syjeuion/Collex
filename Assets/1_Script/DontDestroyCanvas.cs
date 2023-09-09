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

    //기록 상세 페이지
    public GameObject toastPopUp;
    public GameObject recordTitleContainer;
    public GameObject recordsContainer;
    public GameObject prefab;
    GameObject newWriting;

    public static Action setRecord; //페이지 세팅 함수
    public static Action<bool> controlProgressIndicator; //프로그래스 제어
    public static Action openQuitAlert;
    public static DontDestroyCanvas Instance;

    //칭호 획득 시
    public GameObject ObtainTitlePage;
    public GameObject titleConfirmButton;
    public GameObject TitleListPage; //여러개 동시 획득
    public GameObject titleListContent;
    public GameObject titleListPrefab;
    GameObject newTitleList;

    public Animator fireCracker;
    public Animator fireCrackerList;

    //ProgressIndicator
    public GameObject progressIndicatorPage;
    //Quit alert
    public GameObject quitAlertPage;

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
        controlProgressIndicator = (bool check) => { ControlProgressIndicator(check); };
        openQuitAlert = () => { OpenQuitAlert(); };
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
            getTitleCount = UserManager.Instance.nowGetTitle.Count;
            setObtainTitlePage();
            if(UserManager.Instance.getTitle==1) UserManager.Instance.getTitle = 0;
            if (UserManager.Instance.getTitle == 2) UserManager.Instance.getTitle = 3; //온보딩 후

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

        transform.GetChild(0).gameObject.SetActive(true);
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
            transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<Toggle>().isOn = true;
        else
            transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<Toggle>().isOn = false;

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
            recordTitleContainer.transform.GetChild(3).GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            //recordTitleContainer.transform.GetChild(3).GetChild(3).GetComponent<LayoutElement>().ignoreLayout = false;
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
        UIController.instance.SetEnableCanvasState(false);

        if (UserManager.Instance.getTitle == 2)
        {   
            fireCracker.SetTrigger("startAni"); //애니메이션 실행

            ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = "Collex 입사 완료로";
            ObtainTitlePage.transform.GetChild(3).GetComponent<TMP_Text>().text =
                "‘신입 " + UserManager.Instance.newUserInformation.userTitleNoun + "’ 칭호를 획득했어요!";

            titleConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "홈으로 가기";
            ObtainTitlePage.transform.GetChild(4).gameObject.SetActive(false);
            ObtainTitlePage.transform.GetChild(5).gameObject.SetActive(true);
        }
        else
        {
            titleConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "확인";
            ObtainTitlePage.transform.GetChild(5).gameObject.SetActive(false);
            
            if (getTitleCount == 1)
            {
                fireCracker.SetTrigger("startAni"); //애니메이션 실행

                nowTitle[0] = UserManager.Instance.nowGetTitle[0][0];
                nowTitle[1] = UserManager.Instance.nowGetTitle[0][1];
                nowTitle[2] = UserManager.Instance.nowGetTitle[0][2];

                ObtainTitlePage.transform.GetChild(3).GetComponent<TMP_Text>().text = "‘" + nowTitle[1] + "’ 칭호를 획득했어요!";
                string description = nowTitle[2];

                if (description == "경험 첫 추가" || description == "입사동기 첫 추가" || description == "활동 첫 마무리")
                    ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = description + "로";
                else
                    ObtainTitlePage.transform.GetChild(2).GetComponent<TMP_Text>().text = description + "으로";

                TitleListPage.SetActive(false);

                //if (nowTitle[0] == "앞" && nowTitle[1] == UserManager.Instance.newUserInformation.targetTitleModi[0])
                //{ UserManager.Instance.newUserInformation.targetTitleModi = new string[5]; }
                //if (nowTitle[0] == "뒤" && nowTitle[1] == UserManager.Instance.newUserInformation.targetTitleNoun[0])
                //{ UserManager.Instance.newUserInformation.targetTitleNoun = new string[5]; }

                if (SceneManager.GetActiveScene().name == "1_Home" && UserManager.Instance.newUserInformation.isItFirstTargetTitle != 0)
                { UserManager.Instance.checkHomeTargetTitle = true; }
            }
            else //칭호 여러개 동시 획득 시
            {
                TitleListPage.SetActive(true);

                for(int i=0;i< titleListContent.transform.childCount; i++)
                { Destroy(titleListContent.transform.GetChild(i).gameObject); }

                fireCrackerList.SetTrigger("ObtainTitleList"); //애니메이션 실행

                TitleListPage.transform.GetChild(1).GetComponent<TMP_Text>().text
                    = "'" + UserManager.Instance.nowGetTitle[0][1] + "' 외 " + (getTitleCount - 1) + "개의 칭호를 획득했어요!";
                for (int i = 0; i < getTitleCount; i++)
                {
                    newTitleList = Instantiate(titleListPrefab, titleListContent.transform);
                    newTitleList.transform.GetChild(1).GetComponent<TMP_Text>().text = UserManager.Instance.nowGetTitle[i][1];
                    newTitleList.transform.GetChild(2).GetComponent<TMP_Text>().text = "- "+UserManager.Instance.nowGetTitle[i][2];
                }
            }
        }
    }
    public void TitleConfirm()
    {
        if (UserManager.Instance.getTitle == 3)
        {
            StartCoroutine(TitleToHome());
        }
        else
        {
            ObtainTitlePage.SetActive(false);
            UIController.instance.SetEnableCanvasState(true);
            UserManager.Instance.nowGetTitle = new List<string[]>();
        }
    }
    /*public void titleCancel()
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
            StartCoroutine(TitleToHome());
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
        if(SceneManager.GetActiveScene().name == "1_Home"|| SceneManager.GetActiveScene().name == "6_Mypage")
        { UserTitleManager.ActionUserTitle(); }
    }*/
    IEnumerator TitleToHome()
    {
        goHome();
        yield return new WaitForEndOfFrame();
        UIController.instance.SetEnableCanvasState(true);
        ObtainTitlePage.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion
    public void AppBarHomeButton()
    {
        StartCoroutine(TitleToHome());
    }

    //프로그래스 인디케이터 제어
    public void ControlProgressIndicator(bool check)
    {
        if (check)
        {
            progressIndicatorPage.SetActive(true);
        }
        else
        {
            progressIndicatorPage.SetActive(false);
        }
    }

    //서버 오류 시 앱 강제 종료
    public void OpenQuitAlert()
    {
        quitAlertPage.SetActive(true);
    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goWriting() { SceneManager.LoadScene("2_Writing"); }

    public void quitApplication() { Application.Quit(); }
}
