using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float firePreChargeTimer = 0f;
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        // 加载组件
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
    }

    private void OnEnable()
    {
        // 开火事件
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        // 冷却计时器
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 开火事件
    /// </summary>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    /// <summary>
    /// 开火
    /// </summary>
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // 充能武器
        WeaponPreCharge(fireWeaponEventArgs);

        // 开火
        if (fireWeaponEventArgs.fire)
        {
            // 开火判断
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);

                ResetCoolDownTimer();

                ResetPrechargeTimer();
            }
        }
    }

    /// <summary>
    /// 充能武器事件
    /// </summary>
    private void WeaponPreCharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // 是否为充能武器
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            // 计时器
            firePreChargeTimer -= Time.deltaTime;
        }
        else
        {
            // 重置计时器
            ResetPrechargeTimer();
        }
    }

    /// <summary>
    /// 是否准备好开火
    /// </summary>
    private bool IsWeaponReadyToFire()
    {
        // 弹药检查
        if (activeWeapon.GetCurrentWeapon().weaponRemainingAmmo <= 0 && !activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteAmmo)
            return false;

        // 换弹检测
        if (activeWeapon.GetCurrentWeapon().isWeaponReloading)
            return false;

        // 充能检测
        if (firePreChargeTimer > 0f || fireRateCoolDownTimer > 0f)
            return false;

        // 弹夹检测
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity && activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
        {
            // 触发换弹
            reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentWeapon(), 0);

            return false;
        }

        // 开火
        return true;
    }

    /// <summary>
    /// 设置子弹
    /// </summary>
    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            // 发射协程
            StartCoroutine(FireAmmoRoutine(currentAmmo, aimAngle, weaponAimAngle, weaponAimDirectionVector));
        }
    }

    /// <summary>
    /// 发射弹药
    /// </summary>
    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        int ammoCounter = 0;

        // 数量
        int ammoPerShot = Random.Range(currentAmmo.ammoSpawnAmountMin, currentAmmo.ammoSpawnAmountMax + 1);

        // 间隔
        float ammoSpawnInterval;

        if (ammoPerShot > 1)
        {
            ammoSpawnInterval = Random.Range(currentAmmo.ammoSpawnIntervalMin, currentAmmo.ammoSpawnIntervalMax);
        }
        else
        {
            ammoSpawnInterval = 0f;
        }

        // 循环获取每次射击的弹药数量
        while (ammoCounter < ammoPerShot)
        {
            ammoCounter++;

            // 获取预制体
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[Random.Range(0, currentAmmo.ammoPrefabArray.Length)];

            // 随机速度
            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            // 获取开火组件
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);

            // 初始化子弹
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            // 等待射击间隔
            yield return new WaitForSeconds(ammoSpawnInterval);
        }

        // 如果不是无限子弹 减少弹药数量
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity)
        {
            activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
            activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
        }

        // 呼叫开火事件
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWeapon());

        // 特效
        WeaponShootEffect(aimAngle);

        // 音效
        WeaponSoundEffect();
    }

    /// <summary>
    /// 重置冷却时间
    /// </summary>
    private void ResetCoolDownTimer()
    {
        // 重置冷却时间
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }

    /// <summary>
    /// 重置充能时间
    /// </summary>
    private void ResetPrechargeTimer()
    {
        // 重置充能时间
        firePreChargeTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponPrechargeTime;
    }


    /// <summary>
    /// 特效
    /// </summary>
    private void WeaponShootEffect(float aimAngle)
    {
        
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect != null && activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null)
        {
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect)PoolManager.Instance.ReuseComponent(activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab, activeWeapon.GetShootEffectPosition(), Quaternion.identity);

            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect, aimAngle);

            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 音效
    /// </summary>
    private void WeaponSoundEffect()
    {
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect);
        }
    }
}