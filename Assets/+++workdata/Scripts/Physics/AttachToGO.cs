using MyBox;
using UnityEngine;

public class AttachToGO : MonoBehaviour
{
    [ConditionalField(nameof(attachToMainCam), true), SerializeField]
    Transform attachTo;

    [SerializeField] bool attachToMainCam;

    Transform transToAttachTo;

    private void Start()
    {
        RefreshAttachment();
    }

    private void RefreshAttachment()
    {
        if (transToAttachTo != null) return;

        if (!attachToMainCam) transToAttachTo = attachTo;
        else transToAttachTo = Camera.main.transform;
    }

    void Update()
    {
        transform.position = transToAttachTo.position.RemoveZ();
    }
}