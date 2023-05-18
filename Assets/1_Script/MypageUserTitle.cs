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

    //이용가이드 스크롤영역
    public RectTransform usingGuideContent;

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

    //가이드 페이지
    public void TabWritingTip()
    {
        usingGuideContent.anchoredPosition = new Vector2(0, 0);
    }
    public void TabUsingTip()
    {
        usingGuideContent.anchoredPosition = new Vector2(0, 2260);
    }
}
