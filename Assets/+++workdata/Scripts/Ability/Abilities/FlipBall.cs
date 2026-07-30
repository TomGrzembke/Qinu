using UnityEngine;

public class FlipBall : Ability
{
     AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
     BallController ballController;

     protected override void OnInitializedInternal()
     {
          ballController = SlotManager.Puk.GetComponent<BallController>();
     }

     protected override void CleanupInternal()
     {
          ballController = null;
          Clear();
     }

     protected override void ExecuteInternal()
     {
          ballController.FlipVelocity();
     }

}
