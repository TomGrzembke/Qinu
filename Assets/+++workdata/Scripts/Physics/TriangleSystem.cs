using System.Collections;
using UnityEngine;

/// <summary> Intends to monitor specific areas and activate objects to spice up the gameplay </summary>
public class TriangleSystem : MonoBehaviour
{
    [SerializeField] string pukString = "Puk";
    [SerializeField] float triangleActivateSeconds = 4;
    [SerializeField] float triangleDeactivateSeconds = 9;
    [SerializeField] GameObject triangles;

    Coroutine activationObserverRoutine = null;
    Coroutine triangleRoutine = null;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(pukString)) return;

        if (activationObserverRoutine != null) return;

        activationObserverRoutine = StartCoroutine(StucknessObserverRoutine());
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(pukString)) return;

        StopRoutine(activationObserverRoutine);
        activationObserverRoutine = null;
    }

    IEnumerator StucknessObserverRoutine()
    {
        yield return new WaitForSecondsRealtime(triangleActivateSeconds);

        activationObserverRoutine = null;

        StopRoutine(triangleRoutine);

        triangleRoutine = StartCoroutine(TriangleRoutine());
    }

    IEnumerator TriangleRoutine()
    {
        triangles.SetActive(true);
        yield return new WaitForSecondsRealtime(triangleDeactivateSeconds);


        triangles.SetActive(false);
        triangleRoutine = null;
    }

    void StopRoutine(Coroutine routine)
    {
        if (routine == null) return;

        StopCoroutine(routine);
    }
}
