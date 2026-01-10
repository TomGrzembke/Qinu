using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameStateManager.StartGame();
    }
    public void OptionsWindow()
    {
        GameStateManager.OptionsWindow();
    }
    
    public void GoToMainMenu()
    {
        GameStateManager.GoToMainMenu();
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
