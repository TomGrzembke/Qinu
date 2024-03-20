using Cinemachine;
using UnityEngine;

public class SwitchCamCollider : MonoBehaviour
{
    #region serialized fields
    [SerializeField] CinemachineVirtualCamera leftCam;
    [SerializeField] CinemachineVirtualCamera rightCam;
    #endregion

    #region private fields
    Collider2D playerCol;
    bool comesFromRight;
    #endregion
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerCol = collision;
        comesFromRight = (transform.position.x - collision.transform.position.x) < 0;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        bool goesToRight = (transform.position.x - collision.transform.position.x) < 0;

        if (playerCol && comesFromRight != goesToRight)
        {
            if (goesToRight)
            {
                leftCam.Priority -= 1;
                rightCam.Priority += 1;
            }
            else
            {
                leftCam.Priority += 1;
                rightCam.Priority -= 1;
            }
            playerCol = null;
        }
    }
}