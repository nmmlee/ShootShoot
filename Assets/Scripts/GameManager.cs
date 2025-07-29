using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 탄환 타입 정의
public enum BulletType
{
    Normal,     // 기본 탄환
    Gold,       // 골드 탄환
    Red         // 레드 탄환
}

public class GameManager : MonoBehaviour
{
    public GameObject playerGunObject;

    [SerializeField]
    private Image sliderImage;
    [SerializeField]
    private Button shootBtn;
    [SerializeField]
    private Text shootBtnText;
    [SerializeField]
    private Image[] bulletImages = new Image[5]; // 5개의 탄환 이미지

    [SerializeField]
    private Text enemyGoldRewardTxt;
    [SerializeField]
    private Text enemyHpTxt;
    [SerializeField]
    private Text enemyPassiveTxt;
    [SerializeField]
    private Text enemyRemainTurnTxt;

    [SerializeField]
    private GameObject enemyPrefab; // Enemy 프리팹 (Inspector에서 설정)
    [SerializeField]
    private Transform enemySpawnPoint; // Enemy 스폰 위치 (Inspector에서 설정)

    // 탄환 타입별 이미지 Dictionary
    private Dictionary<BulletType, Sprite> bulletTypeSprites = new Dictionary<BulletType, Sprite>();

    // 탄환 타입별 이미지 (Inspector에서 설정)
    [SerializeField]
    private Sprite normalBulletSprite;
    [SerializeField]
    private Sprite goldBulletSprite;
    [SerializeField]
    private Sprite redBulletSprite;

    private float time;
    private int bulletCount;
    private int totalGold;

    // 탄환 순환 시스템
    private List<BulletType> bulletQueue = new List<BulletType>();
    private int currentBulletIndex = 0;
    private bool isEnemyRespawning;

    private Enemy currentEnemy;

    void Start()
    {
        shootBtn.interactable = false;
        InitializeBulletQueue();
        InitializeBulletSprites();
        UpdateBulletImages();
        SpawnNewEnemy();
    }

    // 탄환 큐 초기화
    void InitializeBulletQueue()
    {
        bulletQueue.Clear();
        bulletQueue.Add(BulletType.Normal);  // 1번 탄환: 기본 탄환
        bulletQueue.Add(BulletType.Normal);  // 2번 탄환: 기본 탄환
        bulletQueue.Add(BulletType.Gold);    // 3번 탄환: 골드 탄환
        bulletQueue.Add(BulletType.Normal);  // 4번 탄환: 기본 탄환
        bulletQueue.Add(BulletType.Red);     // 5번 탄환: 레드 탄환
    }

    void InitializeBulletSprites()
    {
        bulletTypeSprites.Clear();
        bulletTypeSprites.Add(BulletType.Normal, normalBulletSprite);
        bulletTypeSprites.Add(BulletType.Gold, goldBulletSprite);
        bulletTypeSprites.Add(BulletType.Red, redBulletSprite);
    }

    void UpdateBulletImages()
    {
        for(int i = 0; i < bulletImages.Length; i++)
        {
            if(bulletImages[i] != null && bulletTypeSprites.ContainsKey(bulletQueue[i]))
            {
                bulletImages[i].sprite = bulletTypeSprites[bulletQueue[i]];
            }
        }
    }

    // 골드 추가
    public void AddGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"골드를 {amount} 획득했습니다! 총 골드: {totalGold}");
        // 여기에 골드 UI 업데이트 로직을 추가할 수 있습니다
    }

    // 새로운 Enemy 생성
    public void SpawnNewEnemy()
    {
        if (enemyPrefab != null && enemySpawnPoint != null)
        {
            // SpawnPoint의 자식으로 Enemy 생성
            GameObject newEnemy = Instantiate(enemyPrefab, enemySpawnPoint);

            // 로컬 위치를 (0,0,0)으로 설정 (부모 기준)
            newEnemy.transform.localPosition = Vector3.zero;
            newEnemy.transform.localRotation = Quaternion.identity;
            newEnemy.transform.localScale = Vector3.one;

            Debug.Log("새로운 Enemy가 생성되었습니다!");

            currentEnemy = newEnemy.GetComponent<Enemy>();

            UpdateEnemyUI();
        }
        else
        {
            Debug.LogError("Enemy 프리팹이나 스폰 포인트가 설정되지 않았습니다!");
        }
    }

    public void UpdateEnemyUI()
    {
        if(currentEnemy != null)
        {
            enemyGoldRewardTxt.text = $"처치보상 : {currentEnemy.goldReward}";
            enemyHpTxt.text = $"적의 HP : {currentEnemy.health}";
            enemyRemainTurnTxt.text = $"공격까지 남은 턴 : {currentEnemy.attackRemainTurn}";
        }
        else
        {
            Debug.Log("아직 enemy가 생성되지 않았어요. UI 업데이트 실패");
        }

    }

    // GameManager.cs에 추가
    public void UpdateEnemyPassiveText(string passiveText)
    {
        if (enemyPassiveTxt != null)
        {
            enemyPassiveTxt.text = $"패시브: {passiveText}";
        }
    }

    void Update()
    {
        moveSlider();
        setAttackCount();
    }

    void moveSlider()
    {
        if (Input.GetButton("Jump"))
        {
            time += Time.deltaTime;
            sliderImage.fillAmount = Mathf.PingPong(time, 1f);
        }
    }

    void setAttackCount()
    {
        if (Input.GetButtonUp("Jump"))
        {
            // Debug.Log(sliderImage.fillAmount);

            if (sliderImage.fillAmount < 0.175)
                bulletCount = 1;
            else if (sliderImage.fillAmount < 0.375)
                bulletCount = 2;
            else if (sliderImage.fillAmount < 0.625)
                bulletCount = 3;
            else if (sliderImage.fillAmount < 0.875)
                bulletCount = 4;
            else if (sliderImage.fillAmount < 1)
                bulletCount = 5;
        }
        shootBtn.interactable = true;
        shootBtnText.text = $"{bulletCount}발 발사";
    }

    public void Shoot()
    {
        shootBtn.interactable = false;
        StartCoroutine(ShootBullets());
    }

    private IEnumerator ShootBullets()
    {
        Enemy enemy = currentEnemy;

        // 발사할 탄환 개수만큼 반복
        for (int i = 0; i < bulletCount; i++)
        {
            // Enemy가 없으면 발사 중단
            if (enemy == null)
            {
                Debug.Log("Enemy가 없어서 발사를 중단합니다.");
                break;
            }

            // 현재 탄환 타입 가져오기
            BulletType currentBullet = bulletQueue[0];

            // 총알 회전 시작할 때 Enemy 빨간색으로 변경
            enemy.SetRedColor();

            // 사용한 탄환을 맨 뒤로 순환
            bulletQueue.Add(bulletQueue[0]);
            bulletQueue.RemoveAt(0);

            UpdateBulletImages();

            // 0.25초 동안 총알 회전
            yield return playerGunObject.transform.DORotate(
                new Vector3(0, 0, -360f), 0.25f, RotateMode.FastBeyond360
            ).SetEase(Ease.Linear).WaitForCompletion();

            // 회전 완료 후 Enemy 원래 색상으로 복원
            enemy.SetOriginalColor();

            // 홀수/짝수에 따른 데미지 계산
            int damage = CalculateBulletDamage(i + 1);

            if (enemy != null)
            {
                bool isPerfectKill = enemy.onDamaged(damage, currentBullet, i + 1);
                Debug.Log("[GameManager] : 맞을 때마다 UI Update");
                UpdateEnemyUI();

                if (isPerfectKill)
                {
                    // 정확한 데미지로 죽였을 때 골드 2배
                    Debug.Log("[GameManager] : 딱데미지!");
                }

                if (enemy == null || isEnemyRespawning)
                {
                    Debug.Log("[GameManager] : 적이 죽었습니다! 공격 중단");
                    break; // 루프 종료
                }
                yield return new WaitForSeconds(0.1f);

            }
            else
            {
                // 적이 없으면 공격 중단
                Debug.Log("[GameManager] : 적이 없습니다! 공격 중단");
                break;
            }

            // 탄환 효과 처리
            // ProcessBulletEffect(currentBullet);
        }
        // 모든 탄환 발사 완료 후 라운드 종료 처리
        if (enemy != null)
        {
            enemy.OnRoundEnd(); // 라운드 후 회복 처리
            enemy.attackRemainTurn--;
        }

        shootBtn.interactable = true;
    }

    // 적 부활 시작 (적이 죽을 때 호출)
    public void StartEnemyRespawn()
    {
        isEnemyRespawning = true;
        shootBtn.interactable = false; // 공격 버튼 비활성화
        currentEnemy = null;
        Debug.Log("적 부활 중... 공격 불가");

        // 0.25초 후 부활 완료
        StartCoroutine(CompleteEnemyRespawn());
    }

    private IEnumerator CompleteEnemyRespawn()
    {
        yield return new WaitForSeconds(0.25f);

        isEnemyRespawning = false;
        Debug.Log("적 부활 완료! 공격 가능");

        // 새로운 적 생성
        SpawnNewEnemy();
    }

    private int CalculateBulletDamage(int index)
    {
        int baseDamage = 1;
        return baseDamage; // + (index % 2 == 0 ? 1 : 0);
    }

    // 탄환 효과 처리
    private void ProcessBulletEffect(BulletType bulletType)
    {

        switch (bulletType)
        {
            case BulletType.Normal:
                Debug.Log("기본 탄환 발사!");
                break;
            case BulletType.Gold:
                Debug.Log("골드 탄환 발사!");
                // 골드 탄환 특별 효과
                break;
            case BulletType.Red:
                Debug.Log("레드 탄환 발사!");
                // 레드 탄환 특별 효과
                break;
        }
    }

    // 디버그용: 현재 탄환 큐 상태 출력
    public void PrintBulletQueue()
    {
        string queueStatus = "현재 탄환 큐: ";
        for (int i = 0; i < bulletQueue.Count; i++)
        {
            queueStatus += $"{i + 1}번: {bulletQueue[i]}, ";
        }
        Debug.Log(queueStatus);
    }
}
