using System.Collections;
using UnityEngine;

/// <summary> Additional function for the end scene </summary>
public class EndManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] DialogueTutorial winDialogue;
    [SerializeField] DialogueTutorial looseDialogue;
    [SerializeField] Transform qinu;

    [Header("SuckIn")]
    [SerializeField] float suckInTime = 2;
    [SerializeField] AnimationCurve suckInCurve;
    [SerializeField] Transform capsuleTrans;
    #endregion

    void Start()
    {
        if (EndLoader.Instance.WonGame)
            winDialogue.gameObject.SetActive(true);
        else
            looseDialogue.gameObject.SetActive(true);

        qinu.position = EndLoader.Instance.QinuEndPos;
    }


    public void ToMainMenu()
    {
        SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.End);
        SceneLoader.Instance.LoadSceneViaIndex(Scenes.MainMenu);
    }

    public void SuckQinuIn()
    {
        StartCoroutine(SuckQinuInCor());
    }

    IEnumerator SuckQinuInCor()
    {
        float timeWentBy = 0;
        Vector3 originalPos = qinu.position;

        while (timeWentBy < suckInTime)
        {
            timeWentBy += Time.deltaTime;
            qinu.position = Vector3.Lerp(originalPos, capsuleTrans.position, suckInCurve.Evaluate(timeWentBy / suckInTime));
            yield return null;
        }

        qinu.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
    }

    public void DeactivateQinu()
    {
        qinu.gameObject.SetActive(false);
    }
}