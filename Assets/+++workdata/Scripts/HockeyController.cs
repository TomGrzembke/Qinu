using TMPro;
using UnityEngine;

public class HockeyController : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Rigidbody2D pukRB;
    [SerializeField] Transform ballResetLeft;
    [SerializeField] Transform ballResetRight;
    [SerializeField] TextMeshProUGUI leftCounterTxt;
    [SerializeField] TextMeshProUGUI rightCounterTxt;
    [SerializeField] Vector2 pointCounter;
    #endregion

    #region private fields

    #endregion

    public void Goal(bool leftSide)
    {
        if (leftSide)
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
        pukRB.transform.position = leftSide ? ballResetLeft.position : ballResetRight.position;

    }
}