using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        InputManager.Instance.ShowCursor();
    }

    public void StartGame()
    {
        GameStateManager.Instance.StartGame();
    }
    
    public void GoToMainMenu()
    {
        GameStateManager.Instance.GoToMainMenu();
    }
    
    public void Resume()
    {
        PauseManager.Instance.PauseLogic(false);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

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
}
