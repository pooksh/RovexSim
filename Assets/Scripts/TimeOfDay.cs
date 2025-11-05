using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeOfDay : IComparable<TimeOfDay> {
    public int hour;
    public int minute;

    public TimeOfDay(int hr, int min)
    {
        hour = hr;
        minute = min;
    }
    
    public TimeOfDay(string timecode)
    {
        // must be in format 00:00
        List<string> timeParts = new List<string>();
        timeParts.AddRange(timecode.Split(":"));
        
        hour = Int32.Parse(timeParts[0]);
        minute = Int32.Parse(timeParts[1]);
    }

    public TimeOfDay(TimeOfDay time) {
        hour = time.hour;
        minute = time.minute;
    }

    public int TotalMinutesSinceDawn()
    {
        return hour * 60 + minute;
    }

    public void IncrementTimeByMinutues(int mins)
    {
        minute += mins;
        while (minute >= 60) {
            minute -= 60;
            hour += 1;
            while (hour >= 24) {
                hour -= 24;
            }
        }
    }

    public void IncrementTimeByHours(int hrs)
    {
        hour += hrs;
        while (hour >= 24) {
            hour -= 24;
        }
    }

    // 24 hour clock; e.g. 16:00 being 4:00 pm
    public string StringTime()
    {
        string minString = "" + minute;
        string hourString = "" + hour;
        if (minute < 10) {
            minString = "0" + minString;
        }

        if (hour < 10) {
            hourString = "0" + hourString;
        }

        return hourString + ":" + minString;
    }

    public string StringTimeAMPM()
    {

        string minString = "" + minute;
        if (minute < 10) {
            minString = "0" + minString;
        }

        if (hour <= 12) {
            return hour + ":" + minString + " AM";
        }
        else {
            return hour - 12 + ":" + minString + " PM"; 
        }
    }

    public int CompareTo(TimeOfDay that)
    {
        return this.TotalMinutesSinceDawn().CompareTo(that.TotalMinutesSinceDawn());
    }

    public override int GetHashCode()
    {
        return TotalMinutesSinceDawn();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        TimeOfDay other = (TimeOfDay)obj;
        return this.TotalMinutesSinceDawn() == other.TotalMinutesSinceDawn();
    }

    public static bool operator ==(TimeOfDay left, TimeOfDay right)
    {
        if (ReferenceEquals(left, right)) {
            return true;
        }
        if (left is null || right is null) {
            return false;
        }
        return left.TotalMinutesSinceDawn() == right.TotalMinutesSinceDawn();
    }

    public static bool operator !=(TimeOfDay left, TimeOfDay right)
    {
        return !(left == right);
    }

    public static bool operator <(TimeOfDay left, TimeOfDay right)
    {
        return left.CompareTo(right) < 0;
    }


    public static bool operator >(TimeOfDay left, TimeOfDay right)
    {
        return left.CompareTo(right) > 0;
    }
    
    public static bool operator <=(TimeOfDay left, TimeOfDay right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(TimeOfDay left, TimeOfDay right)
    {
        return left.CompareTo(right) >= 0;
    }
}