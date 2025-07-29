using UnityEngine;
using UnityEngine.UI;

public enum DamageType
{
    OddDamage,
    EvenDamage,
    AllDamage
}

public class Item : MonoBehaviour
{
    [SerializeField]
    private Image buttonImage;

    public DamageType damageType;
    public int cost;
    public int damageUpgrade;
    public bool isActive;
    public GameManager gameManager;

    public Text explainText;
    public Text costText;

    private string itemExplain;
    
    private Sprite currentImage;
    public Sprite dontbulletImage;

    void Start()
    {
        switch(damageType)
        {
            case DamageType.OddDamage:
                itemExplain = "Ȧ����°";
                break;
            case DamageType.EvenDamage:
                itemExplain = "¦����°";
                break;
            case DamageType.AllDamage:
                itemExplain = "���";
                break;
        }
        explainText.text = $"{itemExplain} źȯ ������ +{damageUpgrade}";
        costText.text = $"{cost}��";
        currentImage = buttonImage.sprite;
    }

    public void Toggle()
    {
        isActive = !isActive;
        ChangeSprite();
    }

    public void ChangeSprite()
    {
        buttonImage.sprite = isActive ? dontbulletImage : currentImage;
    }

    public void ApplyEffect()
    {
        switch(damageType)
        {
            case DamageType.OddDamage:
                gameManager.IncreaseOddDamageUp();
                break;
            case DamageType.EvenDamage:
                gameManager.IncreaseEvenDamageUp();
                break;
            case DamageType.AllDamage:
                gameManager.IncreaseAllDamageUp();
                break;
        }
    }



}
