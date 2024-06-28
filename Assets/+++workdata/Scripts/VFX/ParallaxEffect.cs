using MyBox;
using UnityEngine;


public class ParallaxEffect : MonoBehaviour
{
    #region Serialized
    [SerializeField] float amountOfParallax;
    [SerializeField] Transform camTrans;
    #endregion

    #region Non Serialized
    float startingPos;
    float xPos;
    #endregion

    void Start()
    {
        startingPos = transform.position.x;
    }

    void Update()
    {
        xPos = camTrans.position.x * amountOfParallax;

        transform.position = transform.position.SetX(xPos + startingPos);
    }
}
