using System.Collections;
using System.Collections.Generic;
using Farm.Transition;
using UnityEngine;

namespace Farm.Save
{
    public class DataSlot
    {
        // 进度条，String是GUID
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();


        // UI显示进度详情
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "年/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "月/" + timeData.timeDict["gameDay"] + "日/";
                }
                return string.Empty;
            }
        }

        public string DataScene
        {
            get
            {
                var key = TransitionManager.Instance.GUID;
                if (dataDict.ContainsKey(key))
                {
                    var transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "00.Start" => "海边",
                        "01.Field" => "农场",
                        "02.Home" => "小木屋",
                        _ => string.Empty
                    };
                }
                else return string.Empty;
            }
        }
    }
}
