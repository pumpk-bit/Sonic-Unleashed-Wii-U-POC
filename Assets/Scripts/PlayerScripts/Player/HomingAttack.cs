using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HomingAttack : MonoBehaviour
{
    [SerializeField] public bool CanScriptRun;

    [Header("Homing Attack")]
    [SerializeField] private float HomingAttackRadius;
    [SerializeField] private float HomingAttackSpeed;
    [SerializeField] private string HomingTagName;

    [SerializeField] public bool CanHome;

    [SerializeField] public bool IsHoming;

    [SerializeField] private bool HomeingMakesPlayerUnableToMove;
    [SerializeField] private bool CanHomeAfterHoming;
    [SerializeField] private float WaitTimeForHoming;

    [SerializeField] private AudioManager AudioManager;

    [SerializeField] private PlayerMoving PlayerMovingScript;

    [SerializeField] private Rigidbody Rigidbody;
    [SerializeField] private GameObject HomingAttackSphere;

    [SerializeField] private string LockOnSfx;

    [Header("Boost")]
    [SerializeField] private bool BoostIfNone;


    private  Transform TempTransform;
    private  Transform TempTransform2;
    public  Coroutine homingCoroutine;
    public void Start()
    {
        CheckIfAllIsAssigned();
    }

    private void CheckIfAllIsAssigned()
    {
        if (PlayerMovingScript == null)
            Debug.LogError("PlayerMovingScript not assigned in HomingAttack. Fixing for now."); PlayerMovingScript = GetComponent<PlayerMoving>();

        if (Rigidbody == null)
            Debug.LogError("Rigidbody not assigned in HomingAttack. Fixing for now."); Rigidbody = GetComponent<Rigidbody>();

        if (AudioManager == null)
            Debug.LogError("AudioManager not assigned in HomingAttack. Fixing for now."); AudioManager = FindObjectOfType<AudioManager>();

        if (HomingAttackSphere == null)
            Debug.LogError("HomingAttackSphere not assigned in HomingAttack. Cannot fix.");
    }
    public void HomingAttackManagerJump()
    {
        if (CanScriptRun == false) return;

        if (HomingAttackSphere.activeSelf == false)
        {
            if (BoostIfNone == true)
            {
                PlayerMovingScript.Boost();
                return;
            }
            else
            {
                return;
            }
        }
        else
        {
            homingCoroutine = StartCoroutine(RunToPointRoutine(HomingAttackSphere.transform.position));
            if (!CanHomeAfterHoming) CanHome = false;
        }


    }
    float TempTimeHM;
    public void HomingAttackManagerSphere()
    {
        if (CanScriptRun == false) return;

        if (CanHome == false && CanHomeAfterHoming == false)
        {
            TempTimeHM += Time.deltaTime;
            if (TempTimeHM >= WaitTimeForHoming)
            {
                CanHome = true;
                TempTimeHM = 0f;
            }
            else
            {
                return;
            }

        }
        TempTransform = CheckForHomingAttackNear();
        if (TempTransform == null)
        {
            TempTransform2 = null;
            SetSphereNonActive();
            
            return;
        }
        SetSphereActive();
        HomingAttackSphere.transform.position = CheckForHomingAttackNear().transform.position;

        if (TempTransform != TempTransform2)
        {
            TempTransform2 = TempTransform;
            AudioManager.Play(LockOnSfx);
        }
    }


    private IEnumerator RunToPointRoutine(Vector3 target)
    {
        float startTime = Time.time;
        float maxHomingTime = 2.0f;
        if (HomeingMakesPlayerUnableToMove == true)
        {
            PlayerMovingScript.CanMove = false;
        }
        IsHoming = true;
        while (Time.time - startTime < maxHomingTime)
        {
            float dist = Vector3.Distance(Rigidbody.position, target);

            if (dist < 0.5f || PlayerMovingScript.grounded)
            {
                IsHoming = false;
                SetVelToZero();
                break;
            }

            Vector3 dir = (target - Rigidbody.position).normalized;
            Rigidbody.velocity = dir * HomingAttackSpeed;

            yield return new WaitForFixedUpdate();
        }

        if (HomeingMakesPlayerUnableToMove == true)
        {
            PlayerMovingScript.CanMove = true;
        }
    }


    private readonly Collider[] overlapResults = new Collider[10];

    private Transform CheckForHomingAttackNear()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, HomingAttackRadius, overlapResults);

        Transform nearest = null;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            if (overlapResults[i].CompareTag(HomingTagName))
            {
                if (overlapResults[i].transform.root == transform.root) continue;

                float dist = (overlapResults[i].transform.position - transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = overlapResults[i].transform;
                }
            }
        }
        return nearest;
    }

    private void SetVelToZero()
    {
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
    }

    public void SetSphereActive()
    {
        HomingAttackSphere.SetActive(true);
    }
    public void SetSphereNonActive()
    {
        HomingAttackSphere.SetActive(false);
    }

    public void StopHomingCoroutine()
    {
        StopCoroutine(homingCoroutine);
        PlayerMovingScript.CanMove = true;
    }
}
