using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using Firebase.Database;
using Newtonsoft.Json;

public class UserTitleManager : MonoBehaviour
{
    //UserId
    string userId;

    //화면
    public GameObject UserTitlePage;
    public GameObject ConfirmButton;
    public GameObject toolTip1;
    public GameObject toolTip2;

    public Sprite skeleton1;
    public Sprite skeleton2;

    //버튼
    public Sprite Button_Disabled;
    public Sprite Button_Enabled;

    //탭
    public GameObject tabModi;
    public GameObject tabNoun;

    //출력 오브젝트
    public GameObject ModiContent; //container
    public GameObject NounContent;
    public GameObject titlePrefab_Gauge; //Prefab 목표칭호=1, 칭호컬렉션=2
    public GameObject titlePrefab_list; //Prefab 대표칭호=3
    GameObject newTitleObj; //instantiate

    //선택된 칭호
    public GameObject selectedModi;
    public GameObject selectedNoun;

    //대표칭호 로드
    public TMP_Text userTitleUI;

    public void ReloadUserTitleUI()
    {
        userTitleUI.text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
    }

    public static Action TargetTitle;
    public static Action ActionUserTitle;
    private void Awake()
    {
        TargetTitle = () => { saveTargetTitle(whichTitle); };
        ActionUserTitle = () => { ReloadUserTitleUI(); };
    }

    string nowTargetTitleModi;
    string nowTargetTitleNoun;
    string nowUserTitleModi;
    string nowUserTitleNoun;

    //Color
    Color primary1;
    Color gray300;
    Color gray500;
    Color gray900;
    private void Start()
    {
        ColorUtility.TryParseHtmlString("#EFF5FF", out primary1);
        ColorUtility.TryParseHtmlString("#DDE0E3", out gray300);
        ColorUtility.TryParseHtmlString("#949CA8", out gray500);
        ColorUtility.TryParseHtmlString("#1E2024", out gray900);
        nowUserTitleModi = UserManager.Instance.newUserInformation.userTitleModi;
        nowUserTitleNoun = UserManager.Instance.newUserInformation.userTitleNoun;

        userId = UserManager.Instance.newUserInformation.userId;
    }

    #region 페이지 세팅
    //목표칭호=1
    public int whichTitle;
    public void setTargetTitle()
    {
        whichTitle = 1;
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        UIController.instance.curOpenPageNum = 2;
        UIController.instance.PageObjArr[2].SetActive(true);
        //목표칭호 앞 뒤 선택에 따른 탭 활성화
        if (currentObj.name == "Selected Noun Container")
        {
            tabModi.GetComponent<Toggle>().isOn = false;
            tabNoun.GetComponent<Toggle>().isOn = true;
        }
        else 
        {
            tabModi.GetComponent<Toggle>().isOn = true;
            tabNoun.GetComponent<Toggle>().isOn = false;
        }

        //기존 목표칭호 있으면 띄워주기
        nowTargetTitleModi = UserManager.Instance.newUserInformation.targetTitleModi[0];
        nowTargetTitleNoun = UserManager.Instance.newUserInformation.targetTitleNoun[0];

        //UserTitlePage.SetActive(true);
        if (!string.IsNullOrWhiteSpace(nowTargetTitleModi))
            selectedModi.GetComponent<TMP_Text>().text = nowTargetTitleModi;
        else selectedModi.GetComponent<TMP_Text>().text = nowUserTitleModi;

        if (!string.IsNullOrWhiteSpace(nowTargetTitleNoun))
            selectedNoun.GetComponent<TMP_Text>().text = nowTargetTitleNoun;
        else selectedNoun.GetComponent<TMP_Text>().text = nowUserTitleNoun;

        StartCoroutine(DownloadTitles(1));

        //처음 들어갔을때만 툴팁 띄워줌
        if (UserManager.Instance.newUserInformation.isItFirstTargetTitlePage == 0)
        {
            StartCoroutine(toolTipCoroutine(toolTip1));
            //UserManager.Instance.newUserInformation.isItFirstTargetTitlePage=1;
        }
    }
    //칭호컬렉션=2
    public void setTitleCollection() 
    {
        whichTitle = 2;

        ConfirmButton.GetComponent<Image>().sprite = Button_Disabled;
        ConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray500;
        ConfirmButton.GetComponent<Button>().interactable = false;

        titlePageInMy();

        if (UserManager.Instance.newUserInformation.isItFirstTitleCollection == 0)
        {
            StartCoroutine(toolTipCoroutine(toolTip1));
            //UserManager.Instance.newUserInformation.isItFirstTitleCollection = 1;
        }
    }
    //대표칭호=3
    public void setUserTitle()
    {
        whichTitle = 3;
        UserTitlePage.transform.GetChild(0).GetComponent<TMP_Text>().text = "나를 나타낼 수 있는 나만의 칭호를 설정해 보세요!";
        UserTitlePage.transform.GetChild(6).GetChild(1).GetComponent<TMP_Text>().text = "대표 칭호 선택하기";
        ConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "대표 칭호로 설정하기";

        ModiContent.transform.GetChild(0).GetComponent<Image>().sprite = skeleton2;
        NounContent.transform.GetChild(0).GetComponent<Image>().sprite = skeleton2;

        titlePageInMy();

        if (UserManager.Instance.newUserInformation.isItFirstUserTitle == 0)
        {
            StartCoroutine(toolTipCoroutine(toolTip1));
            UserManager.Instance.newUserInformation.isItFirstUserTitle = 1;
        }

        ConfirmButton.GetComponent<Image>().sprite = Button_Enabled;
        ConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = primary1;
        ConfirmButton.GetComponent<Button>().interactable = true;
    }

    void titlePageInMy()
    {
        UserTitlePage.SetActive(true);
        tabModi.GetComponent<Toggle>().isOn = true;
        tabNoun.GetComponent<Toggle>().isOn = false;

        nowUserTitleModi = UserManager.Instance.newUserInformation.userTitleModi;
        nowUserTitleNoun = UserManager.Instance.newUserInformation.userTitleNoun;
        selectedModi.GetComponent<TMP_Text>().text = nowUserTitleModi;
        selectedNoun.GetComponent<TMP_Text>().text = nowUserTitleNoun;

        StartCoroutine(DownloadTitles(whichTitle));
    }
    #endregion

    #region 데이터 불러오기
    //string ModifData;
    //string NounData;
    IEnumerator DownloadTitles(int whichTitle)
    {
        //기존 리스트 삭제
        for (int i = 1; i < ModiContent.transform.childCount; i++)
            Destroy(ModiContent.transform.GetChild(i).gameObject);
        for (int i = 1; i < NounContent.transform.childCount; i++)
            Destroy(NounContent.transform.GetChild(i).gameObject);

        //스켈레톤 활성화
        ModiContent.transform.GetChild(0).gameObject.SetActive(true);
        ModiContent.GetComponent<VerticalLayoutGroup>().padding.left = 16;
        ModiContent.GetComponent<VerticalLayoutGroup>().padding.top = 14;
        NounContent.transform.GetChild(0).gameObject.SetActive(true);
        NounContent.GetComponent<VerticalLayoutGroup>().padding.left = 16;
        NounContent.GetComponent<VerticalLayoutGroup>().padding.top = 14;

        if(string.IsNullOrWhiteSpace(UserManager.Instance.nounTitleList[26, 3]))
        {
            yield return new WaitForSeconds(3f);
        }

        //스켈레톤 지우기
        ModiContent.transform.GetChild(0).gameObject.SetActive(false);
        ModiContent.GetComponent<VerticalLayoutGroup>().padding.left = 0;
        ModiContent.GetComponent<VerticalLayoutGroup>().padding.top = 4;
        NounContent.transform.GetChild(0).gameObject.SetActive(false);
        NounContent.GetComponent<VerticalLayoutGroup>().padding.left = 0;
        NounContent.GetComponent<VerticalLayoutGroup>().padding.top = 4;

        //칭호 리스트
        setData("앞", whichTitle, false);
        setData("뒤", whichTitle, false);
    }

    
    void setData(string orderCheck, int whichTitle, bool targetCheck)
    {
        if (whichTitle == 2&& orderCheck=="뒤") setGaugePrefab("뒤", "사원", "Collex 입사 완료", 1);
        for (int i=0; i<27; i++) //칭호 개수
        {
            string[] column = new string[4];
            if (orderCheck == "앞")
            {
                column[0] = UserManager.Instance.modiTitleList[i, 0];
                column[1] = UserManager.Instance.modiTitleList[i, 1];
                column[2] = UserManager.Instance.modiTitleList[i, 2];
                column[3] = UserManager.Instance.modiTitleList[i, 3];
            }
            else
            {
                column[0] = UserManager.Instance.nounTitleList[i, 0];
                column[1] = UserManager.Instance.nounTitleList[i, 1];
                column[2] = UserManager.Instance.nounTitleList[i, 2];
                column[3] = UserManager.Instance.nounTitleList[i, 3];
            }

            if (targetCheck) //목표칭호 저장
            {
                if (orderCheck == "앞")
                {
                    if (column[0] == selectedModiData[0])
                    {
                        UserManager.Instance.newUserInformation.targetTitleModi[0] = selectedModiData[0];
                        UserManager.Instance.newUserInformation.targetTitleModi[2] = selectedModiData[2];
                        UserManager.Instance.newUserInformation.targetTitleModi[1] = column[1];
                        UserManager.Instance.newUserInformation.targetTitleModi[3] = column[2];
                        UserManager.Instance.newUserInformation.targetTitleModi[4] = column[3];
                    }
                }
                if (orderCheck == "뒤")
                {
                    if (column[0] == selectedNounData[0])
                    {
                        UserManager.Instance.newUserInformation.targetTitleNoun[0] = selectedNounData[0];
                        UserManager.Instance.newUserInformation.targetTitleNoun[2] = selectedNounData[2];
                        UserManager.Instance.newUserInformation.targetTitleNoun[1] = column[1];
                        UserManager.Instance.newUserInformation.targetTitleNoun[3] = column[2];
                        UserManager.Instance.newUserInformation.targetTitleNoun[4] = column[3];
                    }
                }
            }
            else //리스트 세팅
            {
                //획득 여부 확인
                int titleCount = int.Parse(column[2]); //필요 횟수
                int titleType = int.Parse(column[3]); //인덱스 type

                int userCount = UserManager.Instance.newUserInformation.titleCheck[titleType];

                if (userCount >= titleCount)
                {
                    int gaugeValue = 1;
                    if (whichTitle == 2) setGaugePrefab(orderCheck, column[0], column[1], gaugeValue);
                    if (whichTitle == 3) setListPrefab(orderCheck, column[0]);
                }
                else
                {
                    double gauge = (double)userCount / (double)titleCount;
                    if (whichTitle == 1 || whichTitle == 2)
                    {
                        setGaugePrefab(orderCheck, column[0], column[1], (float)gauge);
                    }
                }
            }
        }
    }
    #endregion

    #region 칭호 UI리스트 출력
    //출력하는 prefab 종류 구분
    //목표칭호, 칭호 컬렉션
    void setGaugePrefab(string orderCheck, string title, string description, float gaugeValue)
    {
        if (orderCheck == "앞")
        {
            newTitleObj = Instantiate(titlePrefab_Gauge, ModiContent.transform);
            newTitleObj.GetComponent<Toggle>().group = ModiContent.GetComponent<ToggleGroup>();

            if (whichTitle == 1)
            {
                if (!string.IsNullOrWhiteSpace(nowTargetTitleModi))
                {   if (title == nowTargetTitleModi)
                    { newTitleObj.GetComponent<Toggle>().isOn = true; } }
            }
            else
            {   if (title == nowUserTitleModi)
                { newTitleObj.GetComponent<Toggle>().isOn = true; } }
            
        }
        if (orderCheck == "뒤")
        {
            newTitleObj = Instantiate(titlePrefab_Gauge, NounContent.transform);
            newTitleObj.GetComponent<Toggle>().group = NounContent.GetComponent<ToggleGroup>();

            if (whichTitle == 1)
            {
                if (!string.IsNullOrWhiteSpace(nowTargetTitleNoun))
                {   if (title == nowTargetTitleNoun)
                    { newTitleObj.GetComponent<Toggle>().isOn = true; } }
            }
            else
            {   if (title == nowUserTitleNoun)
                { newTitleObj.GetComponent<Toggle>().isOn = true;}
            }
        }

        newTitleObj.transform.GetChild(2).GetComponent<TMP_Text>().text = title;
        
        newTitleObj.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = gaugeValue;
        if (gaugeValue == 1)
        {
            newTitleObj.transform.GetChild(3).GetComponent<TMP_Text>().text = "[획득 완료] "+description;
            newTitleObj.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
            newTitleObj.transform.GetChild(1).GetChild(3).gameObject.SetActive(true);
            newTitleObj.transform.SetSiblingIndex(1);
        }
        else
        {
            newTitleObj.transform.GetChild(3).GetComponent<TMP_Text>().text = "- " + description;
            newTitleObj.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = (Mathf.RoundToInt(gaugeValue * 100)).ToString() + "%"; }

        newTitleObj.GetComponent<Toggle>().onValueChanged.AddListener(UpdateTitle);
    }
    //대표칭호
    void setListPrefab(string orderCheck, string title)
    {
        if (orderCheck == "앞")
        {
            newTitleObj = Instantiate(titlePrefab_list, ModiContent.transform);
            newTitleObj.GetComponent<Toggle>().group = ModiContent.GetComponent<ToggleGroup>();
            if (title == nowUserTitleModi) newTitleObj.GetComponent<Toggle>().isOn = true;
        }
        if (orderCheck == "뒤")
        {
            newTitleObj = Instantiate(titlePrefab_list, NounContent.transform);
            newTitleObj.GetComponent<Toggle>().group = NounContent.GetComponent<ToggleGroup>();
            if (title == nowUserTitleNoun) newTitleObj.GetComponent<Toggle>().isOn = true;
        }
        newTitleObj.GetComponent<Toggle>().onValueChanged.AddListener(UpdateTitle);

        newTitleObj.transform.GetChild(1).GetComponent<TMP_Text>().text = title;
    }
    #endregion

    #region 선택한 칭호 UI에 반영
    public string[] selectedModiData = new string[5]; //0:이름,1:설명,2:게이지,3:획득개수,4:타입
    public string[] selectedNounData = new string[5];
    //선택한 칭호 UI에 반영
    public void UpdateTitle(bool check)
    {
        StartCoroutine(titleContainerSpacing(check));
    }
    IEnumerator titleContainerSpacing(bool check)
    {
        GameObject currentObj = EventSystem.current.currentSelectedGameObject;
        if (check == true)
        {
            if (currentObj.transform.parent.name == "ModiContent")
            {
                if (whichTitle == 3) //대표칭호에서는 칭호만 저장
                {
                    selectedModiData[0] = currentObj.transform.GetChild(1).GetComponent<TMP_Text>().text;
                }
                else
                {
                    selectedModiData[0] = currentObj.transform.GetChild(2).GetComponent<TMP_Text>().text;
                    selectedModiData[2] = currentObj.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount.ToString();
                }
                selectedModi.GetComponent<TMP_Text>().text = selectedModiData[0];

                if (whichTitle == 1)
                {
                    if (UserManager.Instance.newUserInformation.isItFirstTargetTitlePage == 0)
                    {
                        toolTip1.SetActive(false);
                        StartCoroutine(toolTipCoroutine(toolTip2));
                        UserManager.Instance.newUserInformation.isItFirstTargetTitlePage = 1;
                    }
                }
                if (whichTitle == 2)
                {
                    if (UserManager.Instance.newUserInformation.isItFirstTitleCollection == 0)
                    {
                        toolTip1.SetActive(false);
                        StartCoroutine(toolTipCoroutine(toolTip2));
                        UserManager.Instance.newUserInformation.isItFirstTitleCollection = 1;
                    }
                }

            }
            if (currentObj.transform.parent.name == "NounContent")
            {
                if (whichTitle == 3) //대표칭호에서는 칭호만 저장
                {
                    selectedNounData[0] = currentObj.transform.GetChild(1).GetComponent<TMP_Text>().text;
                }
                else
                {
                    selectedNounData[0] = currentObj.transform.GetChild(2).GetComponent<TMP_Text>().text;
                    selectedNounData[2] = currentObj.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount.ToString();
                }
                selectedNoun.GetComponent<TMP_Text>().text = selectedNounData[0];
            }

            //칭호컬렉션에서 버튼 문구 변경
            /*if (whichTitle == 2)
            {
                if ((!string.IsNullOrWhiteSpace(selectedModiData[2]) && int.Parse(selectedModiData[2]) == 1) ||
                        (!string.IsNullOrWhiteSpace(selectedNounData[2]) && int.Parse(selectedNounData[2]) == 1))
                {
                    ConfirmButton.GetComponent<Image>().sprite = Button_Enabled;

                }
                else
                { ConfirmButton.GetComponent<Image>().sprite = Button_Disabled; }
            }*/
        }
        if (check == false) //선택 해제
        {
            if (currentObj.transform.parent.name == "ModiContent")
            {
                selectedModiData = new string[5];
                selectedModi.GetComponent<TMP_Text>().text = nowTargetTitleModi;
            }
            if (currentObj.transform.parent.name == "NounContent")
            {
                selectedNounData = new string[5];
                selectedNoun.GetComponent<TMP_Text>().text = nowUserTitleNoun;
            }
        }
        yield return new WaitForEndOfFrame();
        selectedModi.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 3.9f;
        selectedModi.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 4f;

        //버튼 활성화
        if((whichTitle==1&&(!string.IsNullOrWhiteSpace(selectedModiData[0]) || !string.IsNullOrWhiteSpace(selectedNounData[0])))||
            (whichTitle == 2 && (selectedModiData[2] == "1" || selectedNounData[2] == "1")))
        {
            ConfirmButton.GetComponent<Image>().sprite = Button_Enabled;
            ConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = primary1;
            ConfirmButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            ConfirmButton.GetComponent<Image>().sprite = Button_Disabled;
            ConfirmButton.transform.GetChild(0).GetComponent<TMP_Text>().color = gray500;
            ConfirmButton.GetComponent<Button>().interactable = false;
        }
    }
    //미리보기 탭 시 칭호 탭 변경
    public void TouchTitleTab()
    {
        string tabName = EventSystem.current.currentSelectedGameObject.name;
        if (tabName == "TitleModi") tabModi.GetComponent<Toggle>().isOn = true;
        if (tabName == "TitleNoun") tabNoun.GetComponent<Toggle>().isOn = true;
    }
    //탭 선택에 따라 컬러 변경
    public void ChangeTab()
    {
        if (tabModi.GetComponent<Toggle>().isOn == true)
        {
            selectedModi.GetComponent<TMP_Text>().color = gray900;
            selectedNoun.GetComponent<TMP_Text>().color = gray300;
        }
        if (tabNoun.GetComponent<Toggle>().isOn == true)
        {
            selectedModi.GetComponent<TMP_Text>().color = gray300;
            selectedNoun.GetComponent<TMP_Text>().color = gray900;
        }
    }
    #endregion

    //1.목표칭호 저장
    public void saveTargetTitle(int whichTitle)
    {
        //UserManager에 저장
        if(selectedModiData[0] != null)
        {
            setData( "앞", whichTitle, true);
        }
        if (selectedNounData[0] != null)
        {
            setData( "뒤", whichTitle, true);
        }
        //UserTitlePage.SetActive(false);
        UIController.instance.PageObjArr[2].SetActive(false);
    }

    #region 2.칭호컬렉션 저장
    public int condition;
    public GameObject popUp;
    public void saveTitleCollection()
    {
        if ((!string.IsNullOrWhiteSpace(selectedModiData[0])&& selectedModiData[2] == "1")
            && (!string.IsNullOrWhiteSpace(selectedNounData[0])&& int.Parse(selectedNounData[2]) == 1)) //둘다 선택된 상태일 때
        {
            if (selectedModiData[0] == UserManager.Instance.newUserInformation.userTitleModi
                && selectedNounData[0] == UserManager.Instance.newUserInformation.userTitleNoun)
            { UserTitlePage.SetActive(false); return; }

            popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                    = "'" + selectedModiData[0] + " " + selectedNounData[0] + "'을/를 대표 칭호로\n설정할까요?";
            popUp.GetComponent<RectTransform>().sizeDelta = new Vector2(312, 156);
            condition = 1;
            /*else if (int.Parse(selectedModiData[2]) != 1 && int.Parse(selectedNounData[2]) == 1) //앞칭호 목표칭호
            {
                popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                    = "'" + selectedModiData[0] + "'을/를 목표 칭호로 설정할까요?";
                condition = 2;
            }

            else if (int.Parse(selectedModiData[2]) == 1 && int.Parse(selectedNounData[2]) != 1) //뒷칭호 목표칭호
            {
                popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                    = "'" + selectedNounData[0] + "'을/를 목표 칭호로 설정할까요?";
                condition = 3;
            }

            else //int.Parse(selectedModiData[2]) != 1 && int.Parse(selectedNounData[2]) != 1 //둘 다 목표칭호
            {
                popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                    = "'" + selectedModiData[0] + " " + selectedNounData[0] + "'을/를 목표 칭호로\n설정할까요?";
                popUp.GetComponent<RectTransform>().sizeDelta = new Vector2(312, 156);
                condition = 4;
            }*/
        }
        else if(!string.IsNullOrWhiteSpace(selectedModiData[0])&& selectedModiData[2] == "1") //앞 선택
        {
            if (selectedModiData[0]==UserManager.Instance.newUserInformation.userTitleModi)
            { UserTitlePage.SetActive(false); return; }
            popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                    = "'" + selectedModiData[0] + "'을/를 대표 칭호로 설정할까요?";
            condition = 1;
        }
        else if (!string.IsNullOrWhiteSpace(selectedNounData[0])&& selectedNounData[2] == "1")//뒤 선택
        {
            if (selectedNounData[0] == UserManager.Instance.newUserInformation.userTitleNoun)
            { UserTitlePage.SetActive(false); return; }
            popUp.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text
                        = "'" + selectedNounData[0] + "'을/를 대표 칭호로 설정할까요?";
            condition = 1;
        }
        else { UserTitlePage.SetActive(false); return; }
        popUp.SetActive(true);
    }
    public void popUpConfirmButton()
    {
        if (condition == 1)
        {
            //saveUserTitle();
            if (!string.IsNullOrWhiteSpace(selectedModiData[0])&& selectedModiData[2]=="1")
            {
                UserManager.Instance.newUserInformation.userTitleModi = selectedModiData[0];
            }
            if (!string.IsNullOrWhiteSpace(selectedNounData[0]) && selectedNounData[2] == "1")
            {
                UserManager.Instance.newUserInformation.userTitleNoun = selectedNounData[0];
            }
            ReloadUserTitleUI();
            //UserTitlePage.SetActive(false);
        }
        /*else if(condition==2)
            setData( "앞", whichTitle, true);
        else if(condition==3)
            setData("뒤", whichTitle, true);
        else
            saveTargetTitle(2);*/

        popUp.SetActive(false);
        UserTitlePage.SetActive(false);
    }
    #endregion

    //3.대표칭호 저장
    public void saveUserTitle()
    {
        //UserManager에 저장
        if (!string.IsNullOrWhiteSpace(selectedModiData[0]))
        {
            UserManager.Instance.selectedModi = selectedModiData[0];
        }
        if (!string.IsNullOrWhiteSpace(selectedNounData[0]))
        {
            UserManager.Instance.selectedNoun = selectedNounData[0];
        }
        UserTitlePage.SetActive(false);

        //파이어베이스 업데이트
        UpdateUserTitle(selectedModiData[0], selectedNounData[0]);
    }
    private async void UpdateUserTitle(string modi, string noun)
    {
        string userName = UserManager.Instance.newUserInformation.userName;
        UserDB userDB = await GetUserDB(userId);

        string userTitle = modi;
        print("userTitle: "+userTitle);
        if (!string.IsNullOrEmpty(noun)) { userTitle += " " + noun; }
        print("userTitle: " + userTitle);
        userDB.userInformation.userTitle = userTitle;

        UpdateUserDB(userId, userDB);
    }

    #region 파이어베이스 realTimeDB
    //DB에서 유저 data 가져오기
    private async Task<UserDB> GetUserDB(string userId)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        var taskResult = await dataReference.GetValueAsync();
        UserDB userDB = JsonConvert.DeserializeObject<UserDB>(taskResult.GetRawJsonValue());
        return userDB;
    }

    //DB에 유저 data 저장하기
    private async void UpdateUserDB(string userId, UserDB userDB)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("userList").Child(userId);
        string userDBstr = JsonConvert.SerializeObject(userDB);
        await dataReference.SetRawJsonValueAsync(userDBstr);
    }
    #endregion


    //툴팁
    IEnumerator toolTipCoroutine(GameObject toolTip)
    {
        toolTip.SetActive(true);
        yield return new WaitForSeconds(3f);
        toolTip.SetActive(false);
    }
}
