using UnityEngine;

public class MiscManager : MonoBehaviour
{

    public static MiscManager Instance;

    [SerializeField] GameObject _pauseMenuPanel;
    [SerializeField] GameObject _gearButton;

    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = 60;
    }

    public void PauseGame()
    {
        _gearButton.SetActive(false);
        _pauseMenuPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        _gearButton.SetActive(true);
        _pauseMenuPanel.SetActive(false);
        Time.timeScale = 1;
    }
}
