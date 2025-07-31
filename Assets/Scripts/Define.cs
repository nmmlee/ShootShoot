using UnityEngine;

public static class Define
{
    // 길드 명
    public static string[] guildNames = {"촌뜨기 마을 사냥꾼 길드", "선술집 보안관 길드",
        "명예로운 카우보이 길드", "위대한 카우보이 길드", "카우보이들의 피날레"};

    // 길드 가입 확률
    public static float[] guildRegisterProbability = { 0.9f, 0.8f, 0.7f, 0.65f, 0.4f };

    // 길드 가입 확률 강화 비용(최대 3번까지 강화 가능). 확률 2%씩 올라감.
    public static int[,] guildStrength = {
        {10, 20, 30},
        {20, 30, 40},
        {30, 40, 50},
        {40, 50, 60},
        {50, 60, 70}
    };

    // 길드 가입 비용
    public static int[] guildRegisterCosts = { 30, 50, 70, 90, 110};
}
