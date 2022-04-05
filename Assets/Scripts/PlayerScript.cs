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
    bool showPickableText = false;

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
        showPickableText = false;
        hunger -= Time.deltaTime * 0.09f;
        thirst -= Time.deltaTime * 0.1f;

        // Starving
        if (hunger < 5f)
            health -= Time.deltaTime * 0.7f;

        // Dehydrated
        if (thirst < 7f)
            health -= Time.deltaTime * 0.6f;


        // Interactivity
        if (itemBusy) return;

        ref var activeItem = ref Inventory.Instance.GetActiveItem();
        if (activeItem.item != null && activeItem.item.flags.HasFlag(ItemFlags.Food) && Input.GetButtonDown("Fire1"))
        {
            hunger += activeItem.item.GetStat("hunger");
            activeItem.Consume(1);
            StartCoroutine(BusyFor(1f));
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, ~(1 << 2)))
        {
            if (hit.transform.gameObject.CompareTag("Interactable"))
            {
                var pickable = hit.transform.GetComponent<Pickable>();
                if (pickable != null)
                {
                    showPickableText = true;
                    if (Input.GetKeyDown(KeyCode.E))
                        pickable.Pick();
                }
            }

            if (Input.GetButton("Fire1"))
            {
                if (activeItem.count == 0 || activeItem.item == null) return;

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

    IEnumerator BusyFor(float time)
    {
        itemBusy = true;
        yield return new WaitForSeconds(time);
        itemBusy = false;
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

    GUIStyle textStyle, textCenterStyle;
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
        if (textCenterStyle == null)
            textCenterStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState { textColor = Color.white } };

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

        if (showPickableText)
            GUI.Label(new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 10f, 200f, 20f), "Press [E] to pick", textCenterStyle);
    }
}
