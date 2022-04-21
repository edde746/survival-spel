using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum SnapType
{
    Wall = 1 << 0,
    Floor = 1 << 1,
    Foundation = 1 << 2,
    Center = 1 << 3,
}

public class BuildingSnap : MonoBehaviour
{
    public SnapType type;
}
