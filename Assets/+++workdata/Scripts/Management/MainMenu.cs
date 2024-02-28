using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        PauseManager.Instance.PauseLogic(false);
    }
    public void StartGame()
    {
        GameStateManager.StartGame();
    }
    public void OptionsWindow()
    {
        GameStateManager.OptionsWindow();
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    #region Sound
    public void PlaySoundButtonClick()
    {
        SoundManager.Instance.PlaySoundButtonClick();
    }
    public void PlaySoundButtonHover()
    {
        SoundManager.Instance.PlaySoundButtonHover();
    }
    public void PlaySoundButtonClickBack()
    {
        SoundManager.Instance.PlaySoundButtonClickBack();
    }
    #endregion
}
