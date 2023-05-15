using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action OnStartClicked = () => { };
    //모바일 스크롤
    public ScrollRect writingScene;

    private void Start()
    {
        //OnStartClicked = OnBeginDrag() + OnDrag + OnEndDrag;
    }

    public void OnBeginDrag(PointerEventData e) { writingScene.OnBeginDrag(e); }
    public void OnDrag(PointerEventData e) { writingScene.OnDrag(e); }
    public void OnEndDrag(PointerEventData e) { writingScene.OnEndDrag(e); }
}
