using TMPro;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    #region serialized fields
    [field: SerializeField] public Transform Puk { get; private set; }
    [field: SerializeField] public Transform ArenaMiddle { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosLeft { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosRight { get; private set; }

    [SerializeField] Rigidbody2D pukRB;
    [SerializeField] Transform ballResetLeft;
    [SerializeField] Transform ballResetRight;
    [SerializeField] TextMeshProUGUI leftCounterTxt;
    [SerializeField] TextMeshProUGUI rightCounterTxt;
    [SerializeField] Vector2 pointCounter;
    #endregion

    #region private fields

    #endregion


    public void Goal(int goalID)
    {
        if (goalID == 0)
        {
            pointCounter.y += 1;
            rightCounterTxt.text = pointCounter.y.ToString();
        }
        else
        {
            pointCounter.x += 1;
            leftCounterTxt.text = pointCounter.x.ToString();
        }

        pukRB.velocity = Vector2.zero;
        pukRB.transform.position = goalID == 0 ? ballResetLeft.position : ballResetRight.position;

    }
}