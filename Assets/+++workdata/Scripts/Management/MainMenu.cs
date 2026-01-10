using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameStateManager.StartGame();
    }
    
    public void GoToMainMenu()
    {
        GameStateManager.GoToMainMenu();
    }
    
    public void Resume()
    {
        PauseManager.Instance.PauseLogic(false);
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
