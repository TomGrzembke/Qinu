using UnityEngine;

public class EndManager : MonoBehaviour
{
    #region serialized fields
    [SerializeField] DialogueTutorial winDialogue;
    [SerializeField] DialogueTutorial looseDialogue;
    [SerializeField] Transform qinu;
    #endregion

    #region private fields

    #endregion

    void Start()
    {
        if(EndLoader.Instance.WonGame)
            winDialogue.gameObject.SetActive(true);
        else
            looseDialogue.gameObject.SetActive(true);

        qinu.position = EndLoader.Instance.QinuEndPos;
    }


    public void ToMainMenu()
    {
        SceneLoader.Instance.LoadSceneViaIndex(Scenes.MainMenu);
        SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.End);
    }
}