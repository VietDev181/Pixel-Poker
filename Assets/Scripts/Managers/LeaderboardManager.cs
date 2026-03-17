using UnityEngine;
using System.Collections.Generic;

public static class LeaderboardManager
{
    private const int MAX_RANK = 3;
    private const string KEY = "LEADERBOARD_TIME_";

    // Lấy danh sách top
    public static List<float> GetTimes()
    {
        List<float> times = new();

        for (int i = 0; i < MAX_RANK; i++)
        {
            if (PlayerPrefs.HasKey(KEY + i))
                times.Add(PlayerPrefs.GetFloat(KEY + i));
        }

        return times;
    }

    // Thêm thời gian mới
    public static int AddTime(float newTime)
    {
        List<float> times = GetTimes();
        times.Add(newTime);

        times.Sort();

        int rank = times.IndexOf(newTime) + 1;

        if (times.Count > MAX_RANK)
            times.RemoveRange(MAX_RANK, times.Count - MAX_RANK);

        for (int i = 0; i < times.Count; i++)
            PlayerPrefs.SetFloat(KEY + i, times[i]);

        PlayerPrefs.Save();

        return rank;
    }
}
