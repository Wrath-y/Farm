using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPCSchedule/ScheduleDataList")]
public class ScheduleDataList_SO : ScriptableObject
{
    public List<ScheduleDetails> scheduleList;
}