using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

//[System.Serializable]
//public class testdata
//{
//    public string title;
//}
//폴더 생성
class MakeNewProject
{
    //public bool haveRecord = false;
    public bool isItOngoing = true;

    public string projectTitle;
    public string projectType;
    public DateTime startDate;
    public DateTime endedDate;
    public DateTime lastRecordDate;

    //"내용,날짜,등등"
    public Dictionary<string, string> records = new Dictionary<string, string>(); //기록저장소<제목,내용>

    //DailyRecord data = new DailyRecord();
    //data.title = "testastaset";
    //records.add("test", data);
    //리포트
    //public Dictionary<string,int> jobExList = new Dictionary<string, int>(); //선택된 직무경험 리스트
    public Dictionary<string, int> writings = new Dictionary<string, int>()
    {
        {"문제상황",0},{"문제원인",0},{"해결과정",0},{"배운점",0},{"잘한점",0},{"부족한점",0}
    };
    public Dictionary<string, int> capabilities = new Dictionary<string, int>()
    {
        {"통찰력",0 },{"리더십",0},{"팀워크",0},{"문제해결능력",0},{"커뮤니케이션능력",0}
    };
    public Dictionary<string, int> experiences = new Dictionary<string, int>();

    public string myRole;
    public string prize;
    //public string episodeType;
    public List<string> EpisodeTypes = new List<string>();
    public string Summary;
    public List<string> EpisodeSituation = new List<string>();
    public List<string> EpisodeAction = new List<string>();
    public List<string> EpisodeResult = new List<string>();
    public List<string> EpisodeRealization = new List<string>();
}

//기록 생성
class DailyRecord
{
    public string template;

    public string title;
    //public DateTime date;
    public string date;
    public DateTime dateTime;
    //public string dateDT;
    public List<string> experiences = new List<string>();
    
    public Dictionary<string, string> writings = new Dictionary<string, string>()
    {
        {"활동내용","" },{"문제상황",""},{"문제원인",""},{"해결과정",""},{"배운점",""},{"잘한점",""},{"부족한점",""}
    };

    public string[] capabilities = new string[3];
    public string[] hashtags = new string[3];

}

//대표 에피소드
class Episode
{
    public string type;
    public string title;
    public string content;
    public string date;
}

//유저 DB 데이터 구조
class UserDB
{
    //기본 정보
    public UserDefaultInformation userInformation = new UserDefaultInformation();
    //사원증 정보
    public string userWishCompany;
    public int idcardColor = 3;
    //기록 정보
    public int totalFolderCount;
    public int projectFolderCount;
    public int contestFolderCount;
    public int internshipFolderCount;
    public string[] topThreeExperiences = new string[3];
    public string[] topThreeCapabilities = new string[3];

    //Notification - 받은 응원
    public List<NotiInfo> notiCheerUpList = new List<NotiInfo>();
    //Notification - 입사동기 신청
    public List<NotiInfo> notiApplyFriendList = new List<NotiInfo>();

    //친구 리스트 - {id:info}
    //public List<UserDefaultInformation> friendsList = new List<UserDefaultInformation>();
    public Dictionary<string, UserDefaultInformation> friendsDictionary = new Dictionary<string, UserDefaultInformation>();
}

//친구 리스트 데이터 구조
class UserDefaultInformation
{
    //기본 정보
    public int userProfileImg;
    public string userName;
    public string userTitle;
    public string userJob;
}
//친구 요청 데이터 구조
class NotiInfo
{
    public bool isFirstCheck = true;
    public DateTime date;
    public UserDefaultInformation userInformation = new UserDefaultInformation();
}
////응원하기 알림 리스트
//class CheerUpNotiInfo
//{
//    public bool isFirstCheck = true;
//    public DateTime date;
//    public UserDefaultInformation userInformation = new UserDefaultInformation();
//}

////유저 이름 중복 체크 UserNameList 데이터 구조
//class UserNameList
//{
//    public Dictionary<string, string> userNameList = new Dictionary<string, string>(); 
//}