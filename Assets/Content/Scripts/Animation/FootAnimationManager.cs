using System.Collections;
using System.Linq;
using MyBox;
using UnityEngine;

[ExecuteAlways]
public class FootAnimationManager : MonoBehaviour
{
    [Header("Legs data")]
    public Leg[] Legs = { };
    [MyBox.ReadOnly] public bool IsGrounded = true;
    [MyBox.ReadOnly] public int GroundedLegs = 0;
    [MyBox.ReadOnly] public int MovingLegs = 0;

    [Header("Step Settings")]
    public float StepDistance = 0.5f;   // How far before a step is triggered
    public float SmallStepDistance = 0.1f;
    public float StepHeight = 0.2f;
    public float StepSpeed = 4f;
    public float OverShoot = .5f;

    // TODO when movement complete
    [SerializeField] private bool _isMoving = true;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        foreach (var leg in Legs)
        {
            leg.StartingPos = leg.TargetPos = leg.Positioner.GetGroundPos();
        }
    }

    void FixedUpdate()
    {
        UpdateLegsData();
        TryTriggerStep();

        foreach (var item in Legs)
        {
            MoveFoot(ref item.IsMoving, ref item.Lerp,
                     item.StartingPos, item.TargetPos, item.Target);
        }
    }

    private void UpdateLegsData()
    {
        IsGrounded = false;
        GroundedLegs = 0;

        foreach (var item in Legs)
        {
            item.GroundPos = item.Positioner.GetGroundPos();
            item.Distance = Vector3.Distance(item.Positioner.Effector.position, item.GroundPos);

            if (!item.Positioner.IsGrounded)
                continue;

            IsGrounded = true;
            GroundedLegs++;
        }
    }

    #region Foot movement
    private void TryTriggerStep()
    {
        if (MovingLegs >= Legs.Count() / 2)
            return;

        var targetLeg = Legs.FirstOrDefault(x => x.Distance >= Legs.Max(y => y.Distance));

        if (targetLeg is not { })
            return;

        if (targetLeg.Distance > StepDistance)
        {
            targetLeg.StartingPos = targetLeg.Positioner.Effector.position;

            TriggerStep(ref targetLeg.IsMoving, ref targetLeg.Lerp, targetLeg.StartingPos, targetLeg.GroundPos,
                        out targetLeg.TargetPos);
        }

        else if (!_isMoving)
        {
            foreach (var item in Legs.OrderBy(x => x.Distance))
            {
                if (item.Distance < SmallStepDistance)
                    continue;

                targetLeg.StartingPos = targetLeg.Positioner.Effector.position;

                TriggerStep(ref targetLeg.IsMoving, ref targetLeg.Lerp, targetLeg.StartingPos, targetLeg.GroundPos,
                            out targetLeg.TargetPos, false);

                break;
            }
        }
    }

    private void TriggerStep(ref bool moving, ref float lerp, Vector3 startingPos, Vector3 groundPos,
                             out Vector3 resultPos, bool overShoot = true)
    {
        MovingLegs++;
        moving = true;
        lerp   = 0f;

        resultPos = groundPos - Vector3.right * (startingPos - groundPos).x * (overShoot ? OverShoot : 0);
    }

    private void MoveFoot(ref bool moving, ref float lerp,
                          Vector3 startingPos, Vector3 targetPos, Transform target)
    {
        if (!moving)
        {
            target.position = targetPos;
            return;
        }

        lerp += Time.fixedDeltaTime * StepSpeed;
        Vector3 mid = Vector3.Lerp(startingPos, targetPos, EaseInOutCubic(lerp))
                    + Vector3.up * Mathf.Sin(lerp * Mathf.PI) * StepHeight;
        target.position = mid;

        if (lerp >= 1f)
        {
            moving = false;
            MovingLegs--;
            target.position = targetPos;
        }
    }
    #endregion

    /// <summary>
    /// Smoothly ease in and ease out the input using sigmoid function
    /// </summary>
    private float EaseInOutCubic(float x)
    {
        return 1f / (1 + Mathf.Exp(-10 * (x - 0.5f)));
    }

    [System.Serializable]
    public class Leg
    {
        [Header("References")]
        public FootPositioner Positioner;
        public Transform Target;

        [Header("Movement")]
        public bool UsedInHeightCalculation = true;

        [Header("Ground and distance check")]
        [MyBox.ReadOnly] public Vector3 GroundPos;
        [MyBox.ReadOnly] public float Distance;

        [Header("Moving leg")]
        [MyBox.ReadOnly] public Vector3 StartingPos;
        [MyBox.ReadOnly] public Vector3 TargetPos;
        [MyBox.ReadOnly] public float Lerp;
        [MyBox.ReadOnly] public bool IsMoving;
    }
}
