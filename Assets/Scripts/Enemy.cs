using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    public enum PassiveType
    {
        None,
        HealingAfterDamage,    // 데미지 후 회복
        BulletKilling,         // 특정 탄으로만 죽음
        DoubleOddDamage,       // 홀수 탄 2배 데미지
        DoubleEvenDamage,      // 짝수 탄 2배 데미지
        FirstShotImmunity,     // 첫발 데미지 무시
        OverkillBuff           // 3발 이상 시 HP 2배
    }

    public PassiveType passiveType;
    private int maxHealth;
    private int requiredBulletType; // BulletKilling용 특정 탄 타입
    private bool isFirstShot = true; // FirstShotImmunity용
    private int shotCount = 0; // OverkillBuff용
    private bool shouldHealAfterRound = false; // 라운드 후 회복 플래그

    public int health;
    public int goldReward;
    public int attackRemainTurn;

    private Image enemyImage;
    private Color originalColor;
    private GameManager gameManager;

    void Awake()
    {
        // 기존 초기화
        health = Random.Range(3, 6);
        maxHealth = health;
        goldReward = Random.Range(40, 51);
        attackRemainTurn = Random.Range(1, 3);

        gameManager = FindAnyObjectByType<GameManager>();

        // Enemy 색상 랜덤
        enemyImage = GetComponent<Image>();
        originalColor = new Color(0f, Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        if (enemyImage != null)
        {
            // 원래 색상 저장
            enemyImage.color = originalColor;
        }

        // 랜덤 passive 선택
        passiveType = (PassiveType)Random.Range(1, System.Enum.GetValues(typeof(PassiveType)).Length);
        
        // BulletKilling인 경우 특정 탄 타입 랜덤 선택
        if (passiveType == PassiveType.BulletKilling)
        {
            requiredBulletType = Random.Range(0, 3); // 0: Normal, 1: Gold, 2: Red
        }
        
        UpdatePassiveText();
    }

    // 빨간색으로 변경 (공격받을 때)
    public void SetRedColor()
    {
        if (enemyImage != null)
        {
            enemyImage.color = Color.red;
        }
    }

    // 원래 색상으로 복원 (공격 후)
    public void SetOriginalColor()
    {
        if (enemyImage != null)
        {
            enemyImage.color = originalColor;
        }
    }

    public bool onDamaged(int damage, BulletType bulletType, int bulletIndex)
    {
        int finalDamage = damage;
        
        // Passive 효과 적용
        finalDamage = ApplyPassiveEffect(PassiveTrigger.OnDamaged, finalDamage, bulletType, bulletIndex);

        isFirstShot = false;

        // 데미지가 0이면 무시
        if (finalDamage <= 0)
        {
            Debug.Log("데미지가 무시되었습니다!");
            return false;
        }

        health -= finalDamage;
        shotCount++;

        // HealingAfterDamage passive가 있으면 라운드 후 회복 플래그 설정
        if (passiveType == PassiveType.HealingAfterDamage)
        {
            shouldHealAfterRound = true;
        }

        bool isPerfectKill = false;

        if (health <= 0)
        {
            // BulletKilling 체크
            if (passiveType == PassiveType.BulletKilling)
            {
                if ((int)bulletType != requiredBulletType)
                {
                    health = 1; // 1 HP로 살려놓기
                    Debug.Log($"이 몬스터는 {(BulletType)requiredBulletType} 탄으로만 죽일 수 있습니다!");
                    return false;
                }
            }

            // 정확한 데미지로 죽였는지 확인
            if (health == 0)
            {
                isPerfectKill = true;
            }
            else
            {
                Debug.Log("일반 킬! 골드 1배 보상!");
            }

            Die();
        }
        else
        {
            // HealingAfterDamage 효과
            ApplyPassiveEffect(PassiveTrigger.AfterDamaged, 0, bulletType, bulletIndex);
        }
        return isPerfectKill;
    }

    // 라운드 종료 후 호출되는 함수 (GameManager에서 호출)
    public void OnRoundEnd()
    {
        shotCount = 0;
        isFirstShot = true;

        if (shouldHealAfterRound && health > 0)
        {
            int healAmount = Random.Range(1, 3);
            health = Mathf.Min(health + healAmount, maxHealth);
            Debug.Log($"라운드 종료 후 {healAmount}만큼 회복! 현재 HP: {health}");

            // GameManager UI 업데이트

            if (gameManager != null)
            { 
                gameManager.UpdateEnemyUI();
            }
        }

        shouldHealAfterRound = false; // 플래그 리셋

        if(attackRemainTurn == 0)
        {
            attackRemainTurn = Random.Range(1, 3);
            Debug.Log("[Enemy] : 기다림이 끝났다. 공격");
        }
            
    }

    public void Die()
    {
        Destroy(this.gameObject);
        gameManager.SpawnNewEnemy();
    }

    private int ApplyPassiveEffect(PassiveTrigger trigger, int damage, BulletType bulletType, int bulletIndex)
    {
        switch (passiveType)
        {
            /*
            case PassiveType.HealingAfterDamage:
                if (trigger == PassiveTrigger.AfterDamaged)
                {
                    shouldHealAfterRound = true;
                }
                break;
            */

            case PassiveType.DoubleOddDamage:
                if (trigger == PassiveTrigger.OnDamaged && bulletIndex % 2 == 1) // 홀수 탄
                {
                    damage *= 2;
                    Debug.Log("홀수 탄으로 2배 데미지!");
                }
                break;

            case PassiveType.DoubleEvenDamage:
                if (trigger == PassiveTrigger.OnDamaged && bulletIndex % 2 == 0) // 짝수 탄
                {
                    damage *= 2;
                    Debug.Log("짝수 탄으로 2배 데미지!");
                }
                break;

            case PassiveType.FirstShotImmunity:
                if (trigger == PassiveTrigger.OnDamaged && isFirstShot)
                {
                    damage = 0;
                    Debug.Log("첫발 데미지를 무시했습니다!");
                }
                break;

            case PassiveType.OverkillBuff:
                if (trigger == PassiveTrigger.OnDamaged && shotCount >= 3)
                {
                    health *= 2;
                    Debug.Log("3발 이상 공격받아 HP가 2배가 되었습니다!");
                }
                break;
        }

        return damage;
    }

    public enum PassiveTrigger
    {
        OnDamaged,      // 데미지를 받을 때
        AfterDamaged,   // 데미지를 받은 후
        OnTurnStart,    // 턴 시작 시
        OnAttack        // 공격할 때
    }

    private void UpdatePassiveText()
    {
        string passiveDescription = "";
        
        switch (passiveType)
        {
            case PassiveType.HealingAfterDamage:
                passiveDescription = "데미지 후 2~4 회복";
                break;
            case PassiveType.BulletKilling:
                passiveDescription = $"{(BulletType)requiredBulletType} 탄으로만 죽음";
                break;
            case PassiveType.DoubleOddDamage:
                passiveDescription = "홀수 탄 2배 데미지";
                break;
            case PassiveType.DoubleEvenDamage:
                passiveDescription = "짝수 탄 2배 데미지";
                break;
            case PassiveType.FirstShotImmunity:
                passiveDescription = "첫발 데미지 무시";
                break;
            case PassiveType.OverkillBuff:
                passiveDescription = "3발 이상 시 HP 2배";
                break;
        }

        // GameManager의 UI 텍스트 업데이트
        if (gameManager != null)
        {
            gameManager.UpdateEnemyPassiveText(passiveDescription);
        }
    }
}