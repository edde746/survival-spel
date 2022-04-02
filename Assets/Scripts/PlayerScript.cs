using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle player specific behaviour
public class PlayerScript : MonoBehaviour
{
    public Vector3 spawnPoint;
    Camera playerCamera;
    public float spawnRadius = 300f;
    bool itemBusy = false;
    float health = 65f, hunger = 75f, thirst = 45f;
    GUIStyle healthStyle, hungerStyle, thirstStyle, emptyStyle, alertStyle;

    void Start()
    {
        // Set spawnpoint
        var point = new Vector3(spawnPoint.x + Random.Range(spawnRadius / -2f, spawnRadius / 2f), 200f, spawnPoint.z + Random.Range(spawnRadius / -2f, spawnRadius / 2f));
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
            point.y = hit.point.y + 3f;

        transform.position = point;

        // Get camera
        playerCamera = GetComponentInChildren<Camera>();

        GUIStyle CreateStyle(Color color)
        {
            return new GUIStyle { normal = new GUIStyleState { background = Globals.SolidColorTexture(color) } };
        }

        alertStyle = CreateStyle(Color.Lerp(Color.red, Color.black, 0.2f));
        healthStyle = CreateStyle(Color.Lerp(Color.red, Color.white, 0.3f));
        hungerStyle = CreateStyle(Color.Lerp(Color.green, Color.white, 0.5f));
        thirstStyle = CreateStyle(Color.Lerp(Color.blue, Color.white, 0.5f));
        emptyStyle = CreateStyle(new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    void Update()
    {
        hunger -= Time.deltaTime * 0.09f;
        thirst -= Time.deltaTime * 0.1f;

        // Starving
        if (hunger < 5f)
            health -= Time.deltaTime * 0.7f;

        // Dehydrated
        if (thirst < 7f)
            health -= Time.deltaTime * 0.6f;

        if (Input.GetButton("Fire1") && !itemBusy)
        {
            ref var activeItem = ref Inventory.Instance.GetActiveItem();
            if (activeItem.count == 0 || activeItem.item == null) return;

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, ~(1 << 2)))
            {
                if (activeItem.item.flags.HasFlag(ItemFlags.Tool))
                {
                    var farmable = hit.transform.gameObject.GetComponent<Farmable>();
                    if (farmable != null && (farmable.toolType == 0 || activeItem.item.flags.HasFlag(farmable.toolType)))
                    {
                        StartCoroutine(Farm(farmable, activeItem.item));
                    }
                }
                else if (activeItem.item.flags.HasFlag(ItemFlags.Placeable))
                {
                    StartCoroutine(Place(activeItem.item.model, hit.point));
                    activeItem.Consume(1);
                }
            }
        }
    }

    IEnumerator Farm(Farmable farmable, Item activeItem)
    {
        itemBusy = true;
        // Apply damage & start tool animation
        farmable.ApplyDamage(activeItem.GetStat("damage"));
        var animator = Inventory.Instance.activeItemModel?.GetComponent<Animator>() ?? null;
        if (animator != null)
            animator.SetTrigger("Using");
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        // Award items to player
        var giveAmount = (int)Mathf.Floor(farmable.itemAmount * (1f - farmable.health / farmable.maxHealth));
        farmable.itemAmount -= giveAmount;
        Inventory.Instance.GiveItem(farmable.item, (int)Mathf.Floor(giveAmount * activeItem?.GetStat("gather") ?? 1f));
        // Wait then stop animation
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        itemBusy = false;
    }

    IEnumerator Place(GameObject model, Vector3 point)
    {
        itemBusy = true;
        Instantiate(model, point, Quaternion.identity);
        // TODO: Sound FX & PTFX
        yield return new WaitForSeconds(.5f);
        itemBusy = false;
    }

    Vector2 vitalsBarSize = new Vector2(150f, 17.5f);
    void DrawBar(float progress, GUIStyle filled, ref float drawY)
    {
        var barPosition = new Vector2(10f, drawY);
        GUI.Box(new Rect(barPosition, vitalsBarSize), GUIContent.none, emptyStyle);
        GUI.Box(new Rect(barPosition, new Vector2(vitalsBarSize.x * progress, vitalsBarSize.y)), GUIContent.none, filled);
        drawY -= vitalsBarSize.y + 5f;
    }

    GUIStyle textStyle;
    void DrawBar(string text, GUIStyle style, ref float drawY)
    {
        if (textStyle == null)
            textStyle = new GUIStyle { alignment = TextAnchor.MiddleLeft, normal = new GUIStyleState { textColor = Color.white } };

        GUI.Box(new Rect(new Vector2(10f, drawY), vitalsBarSize), GUIContent.none, style);
        GUI.Label(new Rect(new Vector2(15f, drawY + 5f), vitalsBarSize - new Vector2(5f, 10f)), text, textStyle);
        drawY -= vitalsBarSize.y + 5f;
    }

    void OnGUI()
    {
        var drawY = Screen.height - (vitalsBarSize.y + 5f);

        DrawBar(thirst / 100f, thirstStyle, ref drawY);
        DrawBar(hunger / 100f, hungerStyle, ref drawY);
        DrawBar(health / 100f, healthStyle, ref drawY);

        // Display to the user that they are starving
        if (hunger < 5f)
            DrawBar("Starving", alertStyle, ref drawY);

        // Display to the user that they are dehydrated
        if (thirst < 7f)
            DrawBar("Dehydrated", alertStyle, ref drawY);
    }
}
