using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    public TextMeshProUGUI deathText;
    public GameObject gui;

    public void Display(string reason)
    {
        deathText.text = $"You died of {reason}";
        gameObject.SetActive(true);
        gui.SetActive(false);
        MouseLook.disableLook = true;
        MouseLook.lockCursor = false;
        Movement.disableMovement = true;
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        gui.SetActive(true);
        // Get player script
        PlayerScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        player.Spawn();
        MouseLook.disableLook = false;
        MouseLook.lockCursor = true;
        Movement.disableMovement = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
