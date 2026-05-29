using System.Reflection;
using UnityEngine;
public class PlayerRailGrinding : MonoBehaviour
{
    [SerializeField] GameObject Root;
    [SerializeField] PlayerMoving PlayerMoving;
    [SerializeField] AudioManager AudioManager;

    public Rail rail;

    public float DefSpeedForGrind = 10f;
    private float Savedspeed;

    public bool grinding;
    public bool cangrind = true;

    float distanceOnRail;
    float directionSign = 1f;

    Vector3 velocity;

    float maxSpeed;
    void Start()
    {
        grinding = false;
        Savedspeed = DefSpeedForGrind;
        maxSpeed = PlayerMoving.maxSpeed;
    }

    void Update()
    {
        if (!cangrind) return;
        if (!grinding || rail == null) return;

        distanceOnRail += DefSpeedForGrind * directionSign * Time.deltaTime;

        // Stop at rail ends
        if (!rail.loop)
        {
            if (distanceOnRail >= rail.Length || distanceOnRail <= 0f)
            {
                StopGrinding();
                return;
            }
        }

        Vector3 pos = rail.GetPoint(distanceOnRail);
        Vector3 dir = rail.GetDirection(distanceOnRail);

        Vector3 travelDir = dir * directionSign;

        Root.transform.position = pos;
        Root.transform.rotation = Quaternion.LookRotation(travelDir);

        //PlayerMoving.currentSpeed = DefSpeedForGrind;

        float speedNormalized = maxSpeed > 0f ? DefSpeedForGrind / maxSpeed : 0f;
        if (speedNormalized <= 0.7f) speedNormalized = 0.7f;

        AudioManager.ChangePitch("Grind", speedNormalized);

    }

    public void StartGrinding(Rail newRail, Vector3 playerVelocity)
    {
        if (grinding == true  && rail == newRail) return;

        PlayerMoving.playerRigidbody.isKinematic = true;

        AudioManager.Play("StartGrind");

        rail = newRail;

        distanceOnRail = rail.GetClosestDistance(Root.transform.position);

        Vector3 railDir = rail.GetDirection(distanceOnRail);

        Vector3 playerForward = Vector3.ProjectOnPlane(Root.transform.forward, Vector3.up).normalized;

        directionSign = Mathf.Sign(Vector3.Dot(playerForward, railDir));

        if (directionSign == 0f) directionSign = 1f;

        grinding = true;

        DefSpeedForGrind = Mathf.Max(PlayerMoving.currentSpeed, Savedspeed);

        PlayerMoving.GrindingManager(grinding);

        AudioManager.Play("Grind");

    }

    public void StopGrinding()
    {
        if (rail != null)
        {
            Vector3 dir = rail.GetDirection(distanceOnRail) * directionSign;
            PlayerMoving.playerRigidbody.isKinematic = false;
            PlayerMoving.playerRigidbody.velocity = dir * DefSpeedForGrind;
        }

        grinding = false;
        rail = null;

        PlayerMoving.GrindingManager(false);

        AudioManager.StopPlaying("Grind");

    }


}
