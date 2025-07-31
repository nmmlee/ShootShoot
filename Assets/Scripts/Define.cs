using UnityEngine;

public static class Define
{
    // ��� ��
    public static string[] guildNames = {"�̶߱� ���� ��ɲ� ���", "������ ���Ȱ� ���",
        "���ο� ī�캸�� ���", "������ ī�캸�� ���", "ī�캸�̵��� �ǳ���"};

    // ��� ���� Ȯ��
    public static float[] guildRegisterProbability = { 0.9f, 0.8f, 0.7f, 0.65f, 0.4f };

    // ��� ���� Ȯ�� ��ȭ ���(�ִ� 3������ ��ȭ ����). Ȯ�� 2%�� �ö�.
    public static int[,] guildStrength = {
        {10, 20, 30},
        {20, 30, 40},
        {30, 40, 50},
        {40, 50, 60},
        {50, 60, 70}
    };

    // ��� ���� ���
    public static int[] guildRegisterCosts = { 30, 50, 70, 90, 110};
}
