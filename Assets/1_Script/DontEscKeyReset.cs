using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DontEscKeyReset : MonoBehaviour
{
    [SerializeField] TMP_InputField myInput;
    public string myData;

    public void CheckData()
    {
        if(!string.IsNullOrEmpty(myInput.text))
            myData = myInput.text;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            myInput.text = myData;
            myData = "";
        }
    }
}
