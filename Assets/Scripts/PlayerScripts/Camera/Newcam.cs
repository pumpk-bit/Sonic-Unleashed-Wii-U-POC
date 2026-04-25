using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.WiiU;
using WiiU = UnityEngine.WiiU;
using UnityEngine.UI;

public class Newcam : MonoBehaviour
{
    [Header("Victory")]
    [SerializeField] public bool IsMovable = true;

    [Header("Player")]
    [SerializeField] public PlayerMoving PlayerMoving;
    [Header("Camera")]
    [SerializeField] public bool CanPlayerMove = true;

    [SerializeField] private float Sensitivity = 3.0f;
    [SerializeField] private float SmoothingTime = 0.2f;


    [SerializeField] public Transform Target;

    [SerializeField] private Transform TargetLookTowards;
    [SerializeField] private float SpeedUntilFlipCam;
    [SerializeField] public bool LookTowardReverse;
    [SerializeField] private bool WhenReverseFlip;

    [SerializeField] public float DistanceFromTarget;
    [SerializeField] public float DistanceFromTargetUp;

    [SerializeField] bool IsXInverted;
    [SerializeField] bool IsYInverted;

    private float _rotationY;
    private float _rotationX;
    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    private Vector3 nextRotation;

    float mouseX;
    float mouseY;

    [Header("3D")]
    [SerializeField] public bool ifIs3d;
    [SerializeField] public float CamDistance = 0.5f;
    [SerializeField] public Slider ApartSlider;
    [SerializeField] public Text Slidertext;

    [SerializeField] GameObject RightCam;
    [SerializeField] GameObject LeftCam;



    WiiU.GamePad gp = WiiU.GamePad.access;




    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        TargetLookTowards = Target;


        CalcRamS(); //Debug
    }
    void Update()
    {

        float start = Time.realtimeSinceStartup; //Debug

        if (!IsMovable) { VictoryCam(); return; }
        PlayerInput();
        DampeningAndRotation();
        // don't set transform position here; do it in LateUpdate

        CamUpdate = (Time.realtimeSinceStartup - start) * 1000f;

    }
    void LateUpdate()
    {
        float start = Time.realtimeSinceStartup; //Debug

        if (ifIs3d) Cam3D();
        else NormalCamera();

        lateUpdateTime = (Time.realtimeSinceStartup - start) * 1000f;

    }
    private void VictoryCam()
    {
        transform.position = Target.position - transform.forward;

        if (ifIs3d)
        {
            CamDistance = ApartSlider.value;
            Slidertext.text = CamDistance.ToString();

            RightCam.transform.position = new Vector3(transform.position.x + CamDistance, transform.position.y, transform.position.z);

            LeftCam.transform.position = new Vector3(transform.position.x - CamDistance, transform.position.y, transform.position.z);

        }
    }


    private Quaternion nexRotation;
    private Quaternion targetRotation;
    private void NormalCamera()
    {
        float start = Time.realtimeSinceStartup; //Debug

        //Positioning behind the target
        transform.position = Target.position - transform.forward * DistanceFromTarget + transform.up * DistanceFromTargetUp;

        CamFollowTime = (Time.realtimeSinceStartup - start) * 1000f;

    }

    private void PlayerInput()
    {
        float start = Time.realtimeSinceStartup; //Debug

        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            mouseX = state.rStick.x * Sensitivity * Time.deltaTime;
            mouseY = state.rStick.y * Sensitivity * Time.deltaTime;
            if (state.IsTriggered(WiiU.GamePadButton.Up))
            {
                Sensitivity++;
            }
            if (state.IsTriggered(WiiU.GamePadButton.Down))
            {
                Sensitivity--;
            }


        }

#if UNITY_EDITOR
        mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;
#endif

        _rotationY += mouseX;
        _rotationX += mouseY;

        //clamping x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);
        if (IsXInverted)
        {
            nextRotation = new Vector3(_rotationX, -_rotationY, Target.rotation.eulerAngles.z);
        }
        if (IsYInverted)
        {
            nextRotation = new Vector3(-_rotationX, _rotationY, Target.rotation.eulerAngles.z);
        }
        else
        {
            nextRotation = new Vector3(_rotationX, _rotationY, Target.rotation.eulerAngles.z);
        }

        InputTime = (Time.realtimeSinceStartup - start) * 1000f;

    }

    private void DampeningAndRotation()
    {
        float start = Time.realtimeSinceStartup; //Debug

        //Dampening and the rotation
        if (CanPlayerMove == true)
        {
            nexRotation = Quaternion.Euler(nextRotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, nexRotation, SmoothingTime * Time.deltaTime);
        }
        else
        {

            _rotationY = transform.rotation.eulerAngles.y;
            _rotationX = transform.rotation.eulerAngles.x;

            if (TargetLookTowards == Target)
            {
                Quaternion targetRotation = TargetLookTowards.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SmoothingTime * Time.deltaTime);
            }
            else
            {

                if (LookTowardReverse == true) targetRotation = Quaternion.LookRotation(TargetLookTowards.forward, TargetLookTowards.up);
                else targetRotation = Quaternion.LookRotation(-TargetLookTowards.forward, TargetLookTowards.up);

                if (WhenReverseFlip) CheckAndFlipCamera();
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SmoothingTime * Time.deltaTime);
            }

        }

        CamSmootheTime = (Time.realtimeSinceStartup - start) * 1000f;

    }

    private void Cam3D()
    {
        //Positioning behind the target
        transform.position = Target.position - transform.forward * DistanceFromTarget + transform.up * DistanceFromTargetUp;

        CamDistance = ApartSlider.value;
        Slidertext.text = CamDistance.ToString();

        RightCam.transform.position = new Vector3(transform.position.x + CamDistance, transform.position.y, transform.position.z);
        LeftCam.transform.position = new Vector3(transform.position.x - CamDistance, transform.position.y, transform.position.z);

    }

    private void CheckAndFlipCamera()
    {
        if (PlayerMoving.currentSpeed < SpeedUntilFlipCam) return;
        Vector3 playerDir = Target.forward.normalized;
        Vector3 lookDir = TargetLookTowards.forward.normalized;

        float dot = Vector3.Dot(playerDir, lookDir);
        LookTowardReverse = dot > 0f;
    }

    //Switch modes from other scripts
    public void SetCamToFollow(bool Follow)
    {
        _rotationY = transform.rotation.eulerAngles.y;
        _rotationX = transform.rotation.eulerAngles.x;
        CanPlayerMove = Follow;
        TargetLookTowards = Target;

    }
    public void SetCamToTowards(Transform Targetlook)
    {
        nexRotation = Target.rotation;
        nextRotation = Target.rotation.eulerAngles;
        CanPlayerMove = false;

        TargetLookTowards = Targetlook;
    }

    float lateUpdateTime;
    float CamFollowTime;
    float CamSmootheTime;
    float InputTime;
    float CamUpdate;


    private float _uiTimerAcc;
    private const float UI_UPDATE_INTERVAL = 0.05f;

    private string _cachedLateUpdateLabel;
    private string _cachedUpdateLabel;
    private string _cachedInputTime;
    private string _cachedFollowTime;
    private string _cachedSmoothTime;
    private string _cachedRenderTime;
    private float ms;
    private float fps;
    private float timeSpent;

    void OnGUI()
    {
        _uiTimerAcc += Time.deltaTime;
        if (_uiTimerAcc >= UI_UPDATE_INTERVAL)
        {
            _cachedLateUpdateLabel = string.Format("NewCam:LateUpdate: {0:F2} ms", lateUpdateTime);
            _cachedRenderTime = string.Format("NewCamCamRenderTime: {0:F2} ms", renderTime);
            _cachedUpdateLabel = string.Format("NewCam: Update: {0:F2} ms", CamUpdate);
            _cachedInputTime = string.Format("NewCam: Input: {0:F2} ms", InputTime);
            _cachedFollowTime = string.Format("NewCam: Following: {0:F2} ms", CamFollowTime);
            _cachedSmoothTime = string.Format("NewCam: Smooth: {0:F2} ms", CamSmootheTime);

            ms = Time.deltaTime * 1000f;
            fps = 1f / Time.deltaTime;
            timeSpent = Time.realtimeSinceStartup; //Debug

            _uiTimerAcc = 0f;
        }

        GUI.Label(new Rect(10, 200, 300, 20), _cachedLateUpdateLabel);
        GUI.Label(new Rect(10, 230, 300, 20), _cachedRenderTime);
        GUI.Label(new Rect(10, 260, 300, 20), _cachedUpdateLabel);
        GUI.Label(new Rect(10, 290, 300, 20), _cachedInputTime);
        GUI.Label(new Rect(10, 320, 300, 20), _cachedFollowTime);
        GUI.Label(new Rect(10, 350, 300, 20), _cachedSmoothTime);


        //        GUI.Label(new Rect(10, 170, 300, 20), _Renderes);

        // GUI.Label(new Rect(10, 400, 300, 20), ("Estimated texture memory: " + (totalBytesT / 1024f / 1024f) + " MB. Static")); // Expensive to calculate, so only do it once and display the cached value
        // GUI.Label(new Rect(10, 430, 300, 20), ("Estimated mesh memory: " + (totalBytesM / 1024f / 1024f) + " MB. Static"));

        GUI.Label(new Rect(10, 430, 300, 20), ("MS: " + ms));
        GUI.Label(new Rect(10, 480, 300, 20), ("FPS: " + fps));
        GUI.color = Color.white; // reset

        GUI.Label(new Rect(10, 500, 300, 20), ("Runtime: " + timeSpent));
        GUI.Label(new Rect(10, 530, 300, 20), ("Height: " + _cachedHeight  + " Width: " + _cachedWidth));
        GUI.Label(new Rect(10, 560, 300, 20), ("USR: " + _cachedAcc));
        GUI.Label(new Rect(10, 590, 300, 20), ("Time Before PowerDown: " + Core.secondsBeforeAPD));
        GUI.Label(new Rect(10, 620, 300, 20), ("Internet: " + AutoConnection.applicationConnected));
        GUI.Label(new Rect(10, 660, 300, 20), ("Internet address: " + AutoConnection.address));
        GUI.Label(new Rect(10, 690, 300, 20), ("Internet subnet: " + AutoConnection.subnet));
    }


    float renderStart;
    float renderTime;

    void OnPreRender()
    {
        renderStart = Time.realtimeSinceStartup;
    }

    void OnPostRender()
    {
        renderTime = (Time.realtimeSinceStartup - renderStart) * 1000f;
    }

    private string _cachedHeight;
    private string _cachedWidth;
    private string _cachedAcc;

    void CalcRamS()
    {
        _cachedHeight = Core.GetScreenHeight((UnityEngine.WiiU.DisplayIndex.TV)).ToString();
        _cachedWidth = Core.GetScreenWidth((UnityEngine.WiiU.DisplayIndex.TV)).ToString();
        _cachedAcc = Core.accountName;
        AutoConnection.ConnectAsync(); //Internet Test

        TextureCalc();
        MeshCalc();
    }
    long totalBytesT = 0;
    long totalBytesM = 0;
    private void TextureCalc()
    {
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
         totalBytesT = 0;

        foreach (var tex in textures)
        {
            if (tex == null) continue;

            int width = tex.width;
            int height = tex.height;

            // Very rough fallback estimate: assume 4 bytes per pixel
            long bytes = (long)width * height * 4;

            totalBytesT += bytes;

            //Debug.Log(tex.name + " ~ " + (bytes / 1024f / 1024f) + " MB");
        }

        Debug.Log("Estimated texture memory: " + (totalBytesT / 1024f / 1024f) + " MB");
    }


    private void MeshCalc()
    {
        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
        SkinnedMeshRenderer[] skinned = FindObjectsOfType<SkinnedMeshRenderer>();

        HashSet<Mesh> countedMeshes = new HashSet<Mesh>();

        foreach (var mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;
            if (!countedMeshes.Add(mesh)) continue;

            long meshBytes = EstimateMesh(mesh);
            totalBytesM += meshBytes;

            //Debug.Log(mesh.name + " ~ " + (meshBytes / 1024f / 1024f) + " MB");
        }

        foreach (var smr in skinned)
        {
            Mesh mesh = smr.sharedMesh;
            if (mesh == null) continue;
            if (!countedMeshes.Add(mesh)) continue;

            long meshBytes = EstimateMesh(mesh);
            totalBytesM += meshBytes;

            // Debug.Log(mesh.name + " ~ " + (meshBytes / 1024f / 1024f) + " MB");
        }

        Debug.Log("Estimated UNIQUE mesh memory: " + (totalBytesM / 1024f / 1024f) + " MB");
        Debug.Log("Unique meshes counted: " + countedMeshes.Count);
    }

    private long EstimateMesh(Mesh mesh)
    {
        long vertexCount = mesh.vertexCount;

        // Lower and safer rough estimate for static meshes
        long vertexBytes = vertexCount * 32;

        long indexBytes;

        if (mesh.isReadable)
        {
            try
            {
                long indexCount = 0;
                for (int i = 0; i < mesh.subMeshCount; i++)
                    indexCount += mesh.GetIndices(i).Length;

                indexBytes = indexCount * 2;
            }
            catch
            {
                indexBytes = vertexCount * 3;
            }
        }
        else
        {
            indexBytes = vertexCount * 3;
        }

        return vertexBytes + indexBytes;
    }
}
