using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MypageUserTitle : MonoBehaviour
{
    //public GameObject idCardScript;
    public GameObject userTitleScript;
    public GameObject changeTitleText;
    public GameObject toastPopUp;
    public TMP_Text usingGuideBannerText;

    public GameObject usingGuidePage;

    //이용가이드 스크롤영역
    public RectTransform usingGuideContent;

    private void Start()
    {
        if (UserManager.Instance.clickHomeBanner)
        {
            usingGuidePage.SetActive(true);
            UserManager.Instance.clickHomeBanner = false;
            UIController.instance.curOpenPageNum = -4;
        }
        usingGuideBannerText.text = UserManager.Instance.newUserInformation.userName + "님을 위한\nCollex 이용 Tip 보러가기!";
    }

    private void Update()
    {
        if(usingGuideContent.anchoredPosition.y >= 2560)
        {
            if (usingGuidePage.transform.GetChild(0).GetChild(1).GetComponent<Toggle>().isOn != true)
                {usingGuidePage.transform.GetChild(0).GetChild(1).GetComponent<Toggle>().isOn = true;}
        }
        else
        {
            if(usingGuidePage.transform.GetChild(0).GetChild(0).GetComponent<Toggle>().isOn != true)
                {usingGuidePage.transform.GetChild(0).GetChild(0).GetComponent<Toggle>().isOn = true;}
        }
    }
    #region 칭호 관련
    public void confirmButton()
    {
        if(userTitleScript.GetComponent<UserTitleManager>().whichTitle == 2)
        {
            userTitleScript.GetComponent<UserTitleManager>().saveTitleCollection();
        }
        if(userTitleScript.GetComponent<UserTitleManager>().whichTitle == 3)
        {
            userTitleScript.GetComponent<UserTitleManager>().saveUserTitle();
            if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedModi))
                changeTitleText.GetComponent<TMP_Text>().text = UserManager.Instance.selectedModi;
            else changeTitleText.GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi;
            if (!string.IsNullOrWhiteSpace(UserManager.Instance.selectedNoun))
                changeTitleText.GetComponent<TMP_Text>().text += " "+UserManager.Instance.selectedNoun;
            else changeTitleText.GetComponent<TMP_Text>().text += " "+UserManager.Instance.newUserInformation.userTitleNoun;
        }
    }

    public void popUpConfirm()
    {
        if(userTitleScript.GetComponent<UserTitleManager>().condition == 1)
        {
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text =
                "'" + userTitleScript.GetComponent<UserTitleManager>().selectedModiData[0] + " " + userTitleScript.GetComponent<UserTitleManager>().selectedNounData[0] + "'을/를 대표 칭호로 설정했어요.";
        }
        else if (userTitleScript.GetComponent<UserTitleManager>().condition == 2)
        {
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text =
                "'" + userTitleScript.GetComponent<UserTitleManager>().selectedModiData[0] + "'을/를 목표 칭호로 설정했어요.";
        }
        else if (userTitleScript.GetComponent<UserTitleManager>().condition == 3)
        {
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text =
                "'" + userTitleScript.GetComponent<UserTitleManager>().selectedNounData[0] + "'을/를 목표 칭호로 설정했어요.";
        }
        else if (userTitleScript.GetComponent<UserTitleManager>().condition == 4)
        {
            toastPopUp.transform.GetChild(0).GetComponent<TMP_Text>().text =
                "'" + userTitleScript.GetComponent<UserTitleManager>().selectedModiData[0] + " " + userTitleScript.GetComponent<UserTitleManager>().selectedNounData[0] + "'을/를 목표 칭호로 설정했어요.";
        }

        toastPopUp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 72);
        StartCoroutine(popUpCoroutine());
    }

    IEnumerator popUpCoroutine()
    {
        toastPopUp.SetActive(true);
        yield return new WaitForSeconds(3f);
        toastPopUp.SetActive(false);
    }
    #endregion

    #region 가이드 페이지
    public void TabWritingTip()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.name == "Tab_writingTip")
                usingGuideContent.anchoredPosition = new Vector2(0, 0);
        }
        
    }
    public void TabUsingTip()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.name == "Tab_usingTip")
                usingGuideContent.anchoredPosition = new Vector2(0, 2560);
        }
            
    }
    #endregion

    #region 환경설정
    public void GoPersonalInformationWeb()
    {
        Application.OpenURL("https://sites.google.com/view/collex/%EA%B0%9C%EC%9D%B8%EC%A0%95%EB%B3%B4-%EC%B2%98%EB%A6%AC%EB%B0%A9%EC%B9%A8?authuser=0");
    }
    #endregion
}
