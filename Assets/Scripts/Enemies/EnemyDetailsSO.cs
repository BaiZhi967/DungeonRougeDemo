using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header 敌人信息
    [Space(10)]
    [Header("敌人信息")]
    #endregion

    #region Tooltip
    [Tooltip("敌人名称")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("敌人预制体")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("唤醒距离")]
    #endregion
    public float chaseDistance = 50f;

    #region Header 敌人材质
    [Space(10)]
    [Header("敌人材质")]
    #endregion
    public Material enemyStandardMaterial;
    public float enemyMaterializeTime;
    public Shader enemyMaterializeShader;
    [ColorUsage(true, true)]
    public Color enemyMaterializeColor;

    #region Header 敌人武器
    [Space(10)]
    [Header("敌人武器")]
    #endregion

    public WeaponDetailsSO enemyWeapon;
    public float firingIntervalMin = 0.1f;
    public float firingIntervalMax = 1f;
    public float firingDurationMin = 1f;
    public float firingDurationMax = 2f;
    public bool firingLineOfSightRequired;

    #region Header 敌人生命
    [Space(10)]
    [Header("敌人生命")]
    #endregion
    public EnemyHealthDetails[] enemyHealthDetailsArray;
    public bool isImmuneAfterHit = false;
    public float hitImmunityTime;
    public bool isHealthBarDisplayed = false;



    #region Validation
#if UNITY_EDITOR
    
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyStandardMaterial), enemyStandardMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyMaterializeShader), enemyMaterializeShader);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingIntervalMin), firingIntervalMin, nameof(firingIntervalMax), firingIntervalMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingDurationMin), firingDurationMin, nameof(firingDurationMax), firingDurationMax, false);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyHealthDetailsArray), enemyHealthDetailsArray);
        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }

#endif
    #endregion

}