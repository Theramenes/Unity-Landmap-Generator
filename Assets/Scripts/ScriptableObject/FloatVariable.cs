using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Float Variable")]
public class FloatVariable : ScriptableObject
{
    [SerializeField]
    private float value;

    public float Value
    {
        get { return value; }
        set { this.value = value; }
    }
}
