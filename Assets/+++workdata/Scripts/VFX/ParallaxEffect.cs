using MyBox;
using UnityEngine;


public class ParallaxEffect : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float amountOfParallax;
    [SerializeField] Transform camTrans;

    #endregion

    #region private fields
    float startingPos;
    #endregion

    void Start()
    {
        startingPos = transform.position.x;
    }

    void Update()
    {
        float Xpos = camTrans.position.x * amountOfParallax;

        transform.position = transform.position.SetX(Xpos + startingPos);
    }
}
