using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MypageUserTitle : MonoBehaviour
{
    //public GameObject idCardScript;
    public GameObject userTitleScript;
    public GameObject changeTitleText;
    public GameObject toastPopUp;

    public void confirmButton()
    {
        if(userTitleScript.GetComponent<UserTitleManager>().whichTitle == 2)
        {
            userTitleScript.GetComponent<UserTitleManager>().saveTitleCollection();
        }
        if(userTitleScript.GetComponent<UserTitleManager>().whichTitle == 3)
        {
            userTitleScript.GetComponent<UserTitleManager>().saveUserTitle();
            changeTitleText.GetComponent<TMP_Text>().text = UserManager.Instance.selectedModi + " " + UserManager.Instance.selectedNoun;
            //idCardScript.GetComponent<IdCard>().setTitle(changeTitleText, UserManager.Instance.selectedModi, UserManager.Instance.selectedNoun);
        }
    }

    public void popUpConfirm()
    {
        print(userTitleScript.GetComponent<UserTitleManager>().condition);
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
        yield return new WaitForSeconds(2.5f);
        toastPopUp.SetActive(false);
    }
}
