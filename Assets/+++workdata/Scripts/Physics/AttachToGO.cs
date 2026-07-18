using MyBox;
using UnityEngine;

public class AttachToGO : MonoBehaviour
{
    [ConditionalField(nameof(attachToMainCam), true), SerializeField] Transform attachTo;
    [SerializeField] bool attachToMainCam;

    Transform transToAttachTo;

    void Start()
    {
        RefreshAttachment();
    }

    void RefreshAttachment()
    {
        if (transToAttachTo != null) return;

        if (!attachToMainCam)
        {
            transToAttachTo = attachTo;
        }
        else
        {
            transToAttachTo = Camera.main.transform;
        }
    }

    void Update()
    {
        transform.position = transToAttachTo.position.RemoveZ();
    }
}