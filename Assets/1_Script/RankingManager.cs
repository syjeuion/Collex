using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RankingManager : MonoBehaviour
{
    public GameObject recordMyRanking;
    public GameObject cheerupMyRanking;

    public Sprite[] myProfileImgs;

    private void Update()
    {
        //안드로이드 디바이스 뒤로가기 클릭 시
        if (Input.GetKey(KeyCode.Escape)) goHome();
    }

    
    private void Start()
    {
        //Record MyRanking 정보 로드
        recordMyRanking.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];
        recordMyRanking.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        recordMyRanking.transform.GetChild(4).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName;

        //Cheerup MyRanking
        cheerupMyRanking.transform.GetChild(2).GetComponent<Image>().sprite = myProfileImgs[UserManager.Instance.newUserInformation.userProfileImgNumber];
        cheerupMyRanking.transform.GetChild(3).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userTitleModi + " " + UserManager.Instance.newUserInformation.userTitleNoun;
        cheerupMyRanking.transform.GetChild(4).GetComponent<TMP_Text>().text = UserManager.Instance.newUserInformation.userName;

    }

    public void goHome() { SceneManager.LoadScene("1_Home"); }
    public void goSearch() { SceneManager.LoadScene("4_Search"); }
    public void goMypage() { SceneManager.LoadScene("6_Mypage"); }
}
