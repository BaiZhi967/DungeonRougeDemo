using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    #region Header 武器基础信息
    [Space(10)]
    [Header("武器基础信息")]
    #endregion Header 武器基础信息
    #region Tooltip
    [Tooltip("武器名称")]
    #endregion Tooltip
    public string weaponName;
    public Sprite weaponSprite;

    #region Header 武器配置
    [Space(10)]
    [Header("武器配置")]
    #endregion Header 武器配置
    #region Tooltip
    [Tooltip("武器开火位置")]
    #endregion Tooltip
    public Vector3 weaponShootPosition;
    #region Tooltip
    [Tooltip("当前武器弹药")]
    #endregion Tooltip
    public AmmoDetailsSO weaponCurrentAmmo;
    #region Tooltip
    [Tooltip("武器开火特效SO")]
    #endregion Tooltip
    public WeaponShootEffectSO weaponShootEffect;
    #region Tooltip
    [Tooltip("开火音效SO")]
    #endregion Tooltip
    public SoundEffectSO weaponFiringSoundEffect;
    #region Tooltip
    [Tooltip("换弹音效SO")]
    #endregion Tooltip
    public SoundEffectSO weaponReloadingSoundEffect;
    #region Header 武器属性
    [Space(10)]
    [Header("武器属性")]
    #endregion Header 武器属性
    #region Tooltip
    [Tooltip("无限弹药")]
    #endregion Tooltip
    public bool hasInfiniteAmmo = false;
    #region Tooltip
    [Tooltip("无限弹夹")]
    #endregion Tooltip
    public bool hasInfiniteClipCapacity = false;
    #region Tooltip
    [Tooltip("弹夹容量")]
    #endregion Tooltip
    public int weaponClipAmmoCapacity = 6;
    #region Tooltip
    [Tooltip("总弹夹")]
    #endregion Tooltip
    public int weaponAmmoCapacity = 100;
    #region Tooltip
    [Tooltip("发射间隔")]
    #endregion Tooltip
    public float weaponFireRate = 0.2f;
    #region Tooltip
    [Tooltip("充能时间 - 充能武器独有")]
    #endregion Tooltip
    public float weaponPrechargeTime = 0f;
    #region Tooltip
    [Tooltip("换弹时间")]
    #endregion Tooltip
    public float weaponReloadTime = 0f;

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentAmmo), weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);

        if (!hasInfiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);
        }

        if (!hasInfiniteClipCapacity)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
        }
    }

#endif
    #endregion Validation
}
