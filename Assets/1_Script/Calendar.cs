using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Calendar : MonoBehaviour
{
    [SerializeField] TMP_Text todayDate;

    public GameObject days;

    [SerializeField] TMP_Text dayMMDDTitle;
    [SerializeField] TMP_Text dayYYYYMM;

    int newYear;
    int newMonth;
    int newDay;

    int firstDay;

    private void Start()
    {
        newYear = DateTime.Now.Year;
        newMonth = DateTime.Now.Month;
        newDay = DateTime.Now.Day;
        todayDate.text = newYear + "년 " + newMonth + "월 " + newDay +"일";

        dayMMDDTitle.text = newMonth + "월"+ newDay + "일"+GetDay(DateTime.Now);
        dayYYYYMM.text = newYear + "년" + newMonth + "월";

        changeMonth(0);
    }

    public void changeMonth(int changeCount)
    {
        newMonth += changeCount;
        if(newMonth <= 0) { newMonth = 12; newYear -= 1; }
        if (newMonth > 12) { newMonth = 1; newYear += 1; }
        dayYYYYMM.text = newYear + "년" + newMonth + "월";

        DateTime dateValue = new DateTime(newYear, newMonth, 1);
        firstDay = (int)dateValue.DayOfWeek;

        for (int i =0; i<35; i++)
        {
            Color color = days.transform.GetChild(i).GetComponent<Image>().color;
            color.a = 0f;
            days.transform.GetChild(i).GetComponent<Image>().color = color;

            if (i < firstDay)
            {
                days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "";
            }
            else if(i< DateTime.DaysInMonth(newYear, newMonth)+firstDay)
            {
                newDay = i - firstDay+1;
                DateTime newDate = new DateTime(newYear, newMonth, newDay);

                days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = newDay.ToString();
                if (newDate<DateTime.Today) {
                    ColorUtility.TryParseHtmlString("#757575", out Color color1);
                    days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = color1;
                }
                else if (newDate == DateTime.Today) {
                    ColorUtility.TryParseHtmlString("#408BFD", out Color color1);
                    days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = color1;
                    Color newcolor = days.transform.GetChild(i).GetComponent<Image>().color;
                    newcolor.a = 255f;
                    days.transform.GetChild(i).GetComponent<Image>().color = newcolor;
                }
                else if (newDate > DateTime.Today) {
                    ColorUtility.TryParseHtmlString("#212121", out Color color1);
                    days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = color1;
                }
            }
            else
            {
                days.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "";
            }
        }
    }

    public void selectDate()
    {
        changeMonth(0);
        GameObject selectedDay = EventSystem.current.currentSelectedGameObject;
        newDay = int.Parse(selectedDay.transform.GetChild(0).GetComponent<TMP_Text>().text);
        dayMMDDTitle.text = newMonth + "월" + newDay + "일" + GetDay(new DateTime(newYear,newMonth,newDay));
        //UserManager.Instance.calendarDate = new DateTime(newYear, newMonth, newDay);

        Color newcolor = selectedDay.GetComponent<Image>().color;
        newcolor.a = 255f;
        selectedDay.GetComponent<Image>().color = newcolor;

        ColorUtility.TryParseHtmlString("#EFF5FF", out Color color1);
        selectedDay.transform.GetChild(0).GetComponent<TMP_Text>().color = color1;
    }

    public void confirmCalendar()
    {
        todayDate.text = newYear+"년 "+newMonth+"월 "+newDay+"일";
        ColorUtility.TryParseHtmlString("#212121", out Color color1);
        todayDate.color = color1;
    }

    private string GetDay(DateTime dt)
    {
        string strDay = "";

        switch (dt.DayOfWeek)
        {
            case DayOfWeek.Monday:
                strDay = "(월)";
                break;
            case DayOfWeek.Tuesday:
                strDay = "(화)";
                break;
            case DayOfWeek.Wednesday:
                strDay = "(수)";
                break;
            case DayOfWeek.Thursday:
                strDay = "(목)";
                break;
            case DayOfWeek.Friday:
                strDay = "(금)";
                break;
            case DayOfWeek.Saturday:
                strDay = "(토)";
                break;
            case DayOfWeek.Sunday:
                strDay = "(일)";
                break;
        }

        return strDay;
    }
}
