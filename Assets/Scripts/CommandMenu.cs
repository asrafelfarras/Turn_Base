using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public Button strikeButton;
    public Button skillButton;
    public Button switchButton;

    public BattleSystem battleSystem;

    void Start()
    {
        // Add listeners
        strikeButton.onClick.AddListener(() => battleSystem.OnCommandSelected("Strike"));
        skillButton.onClick.AddListener(() => battleSystem.OnCommandSelected("Skill"));
        switchButton.onClick.AddListener(() => battleSystem.OnCommandSelected("Switch"));
    }

    public void ShowMenu(bool show)
    {
        gameObject.SetActive(show);
    }
}
