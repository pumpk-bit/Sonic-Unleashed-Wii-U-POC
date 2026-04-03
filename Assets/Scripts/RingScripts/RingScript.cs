using UnityEngine;

public class RingScript : MonoBehaviour 
{
    [Header("Sound")]
    [SerializeField] string NormalRingSFX;
    [SerializeField] string TenRingSFX;
    [SerializeField] string RedRingSFX;

    [SerializeField] string MonitorBreakSFX;

    [Header("Value")]
    [SerializeField] float RingValue;

    [Header("AudioManager")]
    [SerializeField] public AudioManager AudioManager;

    [Header("If Monitor")]
    [SerializeField] bool IsMonitor;
    [SerializeField] bool RemovesSpeed = true;

    [SerializeField] bool NeedToBeBallToBreak;
    [SerializeField] bool BouncesPlayer;
    [SerializeField] float BounceHeight;
    [SerializeField] bool DoesMonitorMakeRingSound;

    [SerializeField] bool DestroyParent;


    [Header("If Ring")]
    [SerializeField] bool IsNormalRing;
    [SerializeField] bool IsTenRing;
    [SerializeField] bool IsRedRing;

    [Header("If End Ring")]
    [SerializeField] bool IsEndRing;

    [Header("Respawns?")]
    [SerializeField] bool CanRespawn;
    [SerializeField] float RespawnDelay;

    [Header("SceneManager")]
    [SerializeField] Scenemanager Scenemanager;



    private void Start()
    {
        CheckIfAllIsAssigned();
    }
    private void CheckIfAllIsAssigned()
    {
        if (AudioManager == null)
          // Debug.LogError("AudioManager not assigned in RingScript. Fixing for now. IF you're using loading diffrent map type then ignore this.");
          AudioManager = FindObjectOfType<AudioManager>();
       // if (Scenemanager == null && IsEndRing)
          //  Debug.LogError("Scenemanager not assigned in RingScript. Fixing for now. IF you're using loading diffrent map type then ignore this."); Scenemanager = FindObjectOfType<Scenemanager>();
    }



    GameObject player;
    PlayerMoving PlayerMovingScript;
    RingManager RingManagerScript;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            PlayerMovingScript = player.gameObject.GetComponentInParent<PlayerMoving>();
            RingManagerScript = player.gameObject.GetComponentInParent<RingManager>();


            if (IsNormalRing) NormalRingDealer(RingManagerScript);
            if (IsTenRing) TenRingDealer(RingManagerScript);
            if (IsRedRing) RedRingDealer(RingManagerScript);

            if (IsMonitor) MonitorDealer(PlayerMovingScript, RingManagerScript);

            if (IsEndRing) EndRingDealer();

        }

    }
    
    private void MonitorDealer(PlayerMoving PlayerMovingScript, RingManager RingManagerScript)
    {
        if (NeedToBeBallToBreak)
        {
            if (PlayerMovingScript.BallorNot == "Ball")
            {
                MonitorBreakOpen();
            }
        }
        else
        {
            MonitorBreakOpen();
        }
    }

    private void MonitorBreakOpen()
    {
        if (DoesMonitorMakeRingSound == true) AudioManager.Play(NormalRingSFX);
        if (BouncesPlayer == true) BouncePlayer(BounceHeight, MonitorBreakSFX, PlayerMovingScript);
        RingManagerScript.AddRing(RingValue);

        if (!CanRespawn)
        {
            if (DestroyParent) DestroryParentObject();
            else DestroryObject();
        }
        else
        {
            RespawnManager();
        }

    }
    private void BouncePlayer(float BounceHeight, string SFX, PlayerMoving PlayerMovingScript)
    {
        PlayerMovingScript.Spring(BounceHeight, SFX, transform.up, RemovesSpeed);
    }



    private void NormalRingDealer(RingManager RingManagerScript)
    {
        RingManagerScript.AddRing(RingValue);
        AudioManager.Play(NormalRingSFX);
        if (!CanRespawn) DestroryObject();
        else RespawnManager();
    }
    private void TenRingDealer(RingManager RingManagerScript)
    {
        RingManagerScript.AddRing(RingValue);
        AudioManager.Play(TenRingSFX);
        if (!CanRespawn) DestroryObject();
        else RespawnManager();
    }
    private void RedRingDealer(RingManager RingManagerScript)
    {
        RingManagerScript.AddRedRing();
        AudioManager.Play(RedRingSFX);
        if (!CanRespawn) DestroryObject();
        else RespawnManager();
    }

    private void EndRingDealer()
    {
        Scenemanager.LevelEnd();
    }

    private void DestroryParentObject()
    {
        gameObject.SetActive(false);
        //Destroy(transform.parent.gameObject);
    }
    private void DestroryObject()
    {
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    private void RespawnManager()
    {
        gameObject.SetActive(false);
        Invoke("ReAppear", RespawnDelay);
    }

    private void ReAppear()
    {
        gameObject.SetActive(true);
    }


}
