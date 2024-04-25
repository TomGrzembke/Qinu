using System.Collections.Generic;
using UnityEngine;

public class SwitchNavBehavior : MonoBehaviour
{
    #region serialized fields
    [SerializeField] bool right;
    #endregion

    #region private fields
    Dictionary<CharNav, bool> nPCsTracked = new();
    #endregion

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("NPC")) return;

        CharNav charNav = collision.transform.parent.GetComponent<CharNav>();
        if (!nPCsTracked.ContainsKey(charNav))
            nPCsTracked.Add(charNav, (transform.position.x - collision.transform.position.x) < 0);

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("NPC")) return;

        CharNav charNav = collision.transform.parent.GetComponent<CharNav>();
        bool goesToRight = (transform.position.x - collision.transform.position.x) < 0;


        if (!nPCsTracked.TryGetValue(charNav, out bool comesFromRight)) return;
        if (comesFromRight == goesToRight) return;

        if (!right) //Inverts if comesFromLeft
            goesToRight = !goesToRight;

        if (goesToRight)
            charNav.ActivateNavCalc();
        else
            charNav.ChangeToArena(comesFromRight == goesToRight ? 0 : 1);

        nPCsTracked.Remove(charNav);
    }
}
