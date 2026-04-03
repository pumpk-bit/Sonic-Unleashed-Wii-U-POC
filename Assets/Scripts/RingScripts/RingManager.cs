using UnityEngine;
using UnityEngine.UI;

public class RingManager : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] public GameObject SonicCharather;

    [Header("Scripts")]
    [SerializeField] public Scenemanager SceneManager;
    [SerializeField] public PlayerMoving PlayerMoving;
    [SerializeField] public AudioManager AudioManager;

    [Header("Current States")]
    [SerializeField] public float AllRingAmmount;
    [SerializeField] public float DisplayRingAmmount;
    [SerializeField] private float PrivateRingAmmount;
    [SerializeField] public float RedRingAmmount;

    [Header("Text")]
    [SerializeField] public Text RingText;
    [SerializeField] public Text RedRingText;

    [Header("Audio")]
    [SerializeField] string OneUpSFX;

    void Start()
    {
        CheckIfAllIsAssigned();
    }


    private void CheckIfAllIsAssigned()
   {
        if (SceneManager == null)
            Debug.LogError("SceneManager not assigned in RingManager. Fixing for now."); SceneManager = FindObjectOfType<Scenemanager>();
        if (PlayerMoving == null)
            Debug.LogError("PlayerMoving not assigned in RingManager. Fixing for now."); PlayerMoving = FindObjectOfType<PlayerMoving>();
        if (AudioManager == null)
            Debug.LogError("AudioManager not assigned in RingManager. Fixing for now."); AudioManager = FindObjectOfType<AudioManager>();
        if (SonicCharather == null)
            Debug.LogError("SonicCharather game object not assigned in RingManager. Cannot fix.");
    }
    public void AddRing(float RingValue)
    {
        DisplayRingAmmount += RingValue;
        AllRingAmmount += RingValue;
        PrivateRingAmmount += RingValue;

        if (PrivateRingAmmount >= 100f )
        {
            PrivateRingAmmount = 0f;
            AudioManager.Play(OneUpSFX);
        }

        RingText.text = "Ring: " + DisplayRingAmmount.ToString("0");
    }

    public void AddRedRing()
    {
        RedRingAmmount++;
    }

}



