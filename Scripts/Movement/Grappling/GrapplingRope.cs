using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("Grappling Rope")]
    [SerializeField] private int ropeQuality;
    [SerializeField] private float damper;
    [SerializeField] private float strength;
    [SerializeField] private float velocity;
    [SerializeField] private float waveCount;
    [SerializeField] private float waveHeight;
    [SerializeField] private AnimationCurve affectCurve;
    private Vector3 currentGrapplePosition;

    [Header("References")]
    [SerializeField] private Grappling grappling;
    [SerializeField] private Swinging swinging;
    [SerializeField] private Transform gunTip;
    [SerializeField] private PlayerMovementAdvanced pm;
    private LineRenderer lr;
    private Spring spring;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawGrapplingRope();
        DrawSwingingRope();
    }

    private void DrawGrapplingRope()
    {
        // if swinging, don't draw grappling rope
        if (swinging.swinging) return;

        // if not grappling, don't draw rope
        if (!grappling.grappling)
        {
            currentGrapplePosition = gunTip.position;
            spring.Reset();
            if(lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }
        
        if(lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = ropeQuality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var grapplePoint = grappling.GetGrapplePoint();
        var gunTipPosition = gunTip.position;
        var up = Quaternion.LookRotation(grapplePoint - gunTipPosition).normalized * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for(var i = 0; i < ropeQuality + 1; i++) {
            var delta = i / (float)ropeQuality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }

    private void DrawSwingingRope()
    {
        // if grappling, don't draw swinging rope
        if (grappling.grappling) return;

        // if not swinging, don't draw rope
        if (!swinging.swinging)
        {
            currentGrapplePosition = gunTip.position;
            spring.Reset();
            if (lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = ropeQuality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var swingPoint = swinging.GetSwingPoint();
        var gunTipPosition = gunTip.position;
        var up = Quaternion.LookRotation(swingPoint - gunTipPosition).normalized * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 12f);

        for (var i = 0; i < ropeQuality + 1; i++)
        {
            var delta = i / (float)ropeQuality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
