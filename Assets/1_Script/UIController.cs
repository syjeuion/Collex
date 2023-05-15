using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController instance = null;
    [SerializeField] Canvas myCanves;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;

    }
    public void SetEnableCanvasState(bool isflag)
    {
        myCanves.enabled = isflag;
    }
}
