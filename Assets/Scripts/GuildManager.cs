using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuildManager : MonoBehaviour
{
    public Text guildNameText;
    public Text guildRegisterProbText;
    public Text guildRegisterCostText;
    public Text registerText;
    public Button registerButton;

    [SerializeField]
    private int guildLevel = 0;
    [SerializeField]
    private int maxGuildLevel;
    [SerializeField]
    private GameManager gameManager;

    void Start()
    {
        UpdateGuildUI();
        maxGuildLevel = Define.guildNames.Length - 1;
    }

    public void GuildRegister()
    {
        int remainGold = gameManager.GetGold();

        if(remainGold >= Define.guildRegisterCosts[guildLevel])
        {
            // Random.value < Define.guildRegisterProbability[guildLevel] 라는 좋은 코드도 있음.
            if (Random.Range(0f, 1.0f) >= 1 - Define.guildRegisterProbability[guildLevel])
            {
                Debug.Log("길드 가입에 성공하였습니다!");
                gameManager.MinusGold(Define.guildRegisterCosts[guildLevel]);
                guildLevel += 1;

                // 최고 레벨에 달성하였을 때
                if (guildLevel > maxGuildLevel)
                {
                    registerButton.interactable = false;
                    guildRegisterProbText.text = "";
                    return;
                }

                // 강화 후 길드 UI 업데이트 시도
                UpdateGuildUI();
            }
            else
            {
                gameManager.MinusGold(Define.guildRegisterCosts[guildLevel]);
                Debug.Log("길드 가입에 실패하였습니다..");
            }
        }
        else
        {
            StartCoroutine(CancelRegisterEffect());
        }
        
    }

    void UpdateGuildUI()
    {
        guildNameText.text = Define.guildNames[guildLevel];
        guildRegisterProbText.text = $"가입 확률 : {Define.guildRegisterProbability[guildLevel] * 100}%";
        guildRegisterCostText.text = $"{Define.guildRegisterCosts[guildLevel]}";
    }

    IEnumerator CancelRegisterEffect()
    {
        registerButton.interactable = false;
        Color costTextColor = guildRegisterCostText.color;
        Color registerTextColor = registerText.color;

        guildRegisterCostText.color = new Color(1, 0, 0);
        registerText.color = new Color(1, 0, 0);

        yield return new WaitForSeconds(0.25f);

        guildRegisterCostText.color = costTextColor;
        registerText.color = registerTextColor;
        registerButton.interactable = true;
    }
}
