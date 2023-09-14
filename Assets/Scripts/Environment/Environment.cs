using UnityEngine;

[DisallowMultipleComponent]
public class Environment : MonoBehaviour
{
    

    #region Header 引用
    [Space(10)]
    [Header("引用")]
    #endregion

    public SpriteRenderer spriteRenderer;

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(spriteRenderer), spriteRenderer);
    }

#endif

    #endregion Validation

}