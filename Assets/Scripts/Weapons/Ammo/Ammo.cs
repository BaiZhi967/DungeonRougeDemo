using UnityEngine;

[DisallowMultipleComponent]
public class Ammo : MonoBehaviour, IFireable
{

    [SerializeField] private TrailRenderer trailRenderer;

    private float ammoRange = 0f; // 子弹范围
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private float fireDirectionAngle;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;
    private bool isAmmoMaterialSet = false;
    private bool overrideAmmoMovement;
    private bool isColliding = false;

    private void Awake()
    {
        // 获取Sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 装弹
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isAmmoMaterialSet)
        {
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        if (!overrideAmmoMovement)
        {
            // 计算移动向量
            Vector3 distanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;

            transform.position += distanceVector;

            // 到达最大范围后禁用
            ammoRange -= distanceVector.magnitude;

            if (ammoRange < 0f)
            {
                if (ammoDetails.isPlayerAmmo)
                {
                    StaticEventHandler.CallMultiplierEvent(false);
                }

                DisableAmmo();
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果已经被碰撞过则直接返回
        if (isColliding) return;

        // 造成伤害
        DealDamage(collision);

        // 特效
        AmmoHitEffect();

        DisableAmmo();
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        bool enemyHit = false;

        if (health != null)
        {
            // 将碰撞状态改为已经被碰撞了
            isColliding = true;

            health.TakeDamage(ammoDetails.ammoDamage);

            // 敌人受击
            if (health.enemy != null)
            {
                enemyHit = true;
            }
        }


        // 乘胜追击
        if (ammoDetails.isPlayerAmmo)
        {
            if (enemyHit)
            {
                StaticEventHandler.CallMultiplierEvent(true);
            }
            else
            {
                StaticEventHandler.CallMultiplierEvent(false);
            }
        }

    }


    /// <summary>
    /// 初始化弹药
    /// </summary>
    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        #region Ammo

        this.ammoDetails = ammoDetails;

        // 初始化碰撞情况
        isColliding = false;

        // 设置发射距离
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // 设置材质
        spriteRenderer.sprite = ammoDetails.ammoSprite;

        // 充能武器
        if (ammoDetails.ammoChargeTime > 0f)
        {
            // 充能时间
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            SetAmmoMaterial(ammoDetails.ammoChargeMaterial);
            isAmmoMaterialSet = false;
        }
        else
        {
            ammoChargeTimer = 0f;
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        // 设置弹药范围
        ammoRange = ammoDetails.ammoRange;

        // 设置弹药速度
        this.ammoSpeed = ammoSpeed;

        // 弹药移动
        this.overrideAmmoMovement = overrideAmmoMovement;

        // 激活物品
        gameObject.SetActive(true);

        #endregion Ammo


        #region Trail 踪迹

        if (ammoDetails.isAmmoTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = ammoDetails.ammoTrailMaterial;
            trailRenderer.startWidth = ammoDetails.ammoTrailStartWidth;
            trailRenderer.endWidth = ammoDetails.ammoTrailEndWidth;
            trailRenderer.time = ammoDetails.ammoTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }

        #endregion Trail 踪迹

    }

    /// <summary>
    /// 根据输入角度和方向调整弹药发射方向和角度
    /// 随机分布
    /// </summary>
    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // 计算最小和最大之间的随机扩展角
        float randomSpread = Random.Range(ammoDetails.ammoSpreadMin, ammoDetails.ammoSpreadMax);

        // -1 - 1获取随机数
        int spreadToggle = Random.Range(0, 2) * 2 - 1;

        if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
        {
            fireDirectionAngle = aimAngle;
        }
        else
        {
            fireDirectionAngle = weaponAimAngle;
        }

        // 调整弹药发射角度
        fireDirectionAngle += spreadToggle * randomSpread;

        // 设置角度
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        // 设置发射位置
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);

    }

    /// <summary>
    /// 禁用子弹
    /// </summary>
    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示击中特效
    /// </summary>
    private void AmmoHitEffect()
    {
        // 击中特效处理
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.ammoHitEffectPrefab != null)
        {
            // 从对象池获取特效
            AmmoHitEffect ammoHitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent(ammoDetails.ammoHitEffect.ammoHitEffectPrefab, transform.position, Quaternion.identity);

            // 设置特效
            ammoHitEffect.SetHitEffect(ammoDetails.ammoHitEffect);

            // 显示特效
            ammoHitEffect.gameObject.SetActive(true);
        }
    }


    public void SetAmmoMaterial(Material material)
    {
        spriteRenderer.material = material;
    }


    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }

#endif
    #endregion Validation

}