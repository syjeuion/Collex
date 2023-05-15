using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GNBManager : Singleton<GNBManager>
{
    public void GoHome()
    {
        SceneManager.LoadScene("1_Home");
    }

    public void GoWriting()
    {
        SceneManager.LoadScene("2_Writing");
    }

}
