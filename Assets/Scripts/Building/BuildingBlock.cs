using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum BlockFlag
{
    CanPlaceOnGround = 1 << 0,
    ForgoeCollision = 1 << 1,
    CanRotate = 1 << 2,
}

[CreateAssetMenu(fileName = "New BuildingBlock", menuName = "Assets/BuildingBlock")]
public class BuildingBlock : ScriptableObject
{
    public int id = -1;
    public GameObject block;
    public Sprite sprite;
    public BlockFlag flags;
    public SnapType snapsTo;
    public int cost = 100;
    // Would be nice to have different upgrade levels, with different costs and items
}
