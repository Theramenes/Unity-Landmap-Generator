using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Int Variable")]
public class IntVariable : ScriptableObject
{
    [SerializeField]
    private int value;

    public int Value
    {
        get { return value; }
        set { this.value = value; }
    }
}
