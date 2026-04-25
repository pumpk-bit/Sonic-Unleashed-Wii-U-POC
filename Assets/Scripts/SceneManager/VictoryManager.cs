using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] public GameObject GoalRing;

    [SerializeField] public GameObject VictoryCamPos;
    [SerializeField] public GameObject VictoryPlayerPos;

	[SerializeField] public GameObject VictoryCamLookAt;
	[SerializeField] public GameObject VictoryPlayerLookAt;

	[SerializeField] public GameObject PlayerGameObject;
	[SerializeField] public GameObject NewCamObject;

	[SerializeField] public Newcam Cam3D;


	[SerializeField] public MusicManager MusicManager;
	[SerializeField] public Newcam Newcam;
	[SerializeField] public CameraHolderScript CameraHolderScript;
	[SerializeField] public PlayerMoving PlayerMoving;

    [SerializeField] string VictoryTheme;
    [SerializeField] string BadVictoryTheme;


    public void VictoryScreenSetUp()
	{
		CheckIfAllIsAvailable();
		
        CameraHolderScript.CanMove = false;
        Newcam.IsMovable = false;
        Newcam.Target = VictoryCamPos.transform;



        PlayerMoving.VictoryMode();


        GoalRing.SetActive(false);

        PlayerGameObject.transform.position = VictoryPlayerPos.transform.position;
        NewCamObject.transform.position = VictoryCamPos.transform.position;

        Cam3D.transform.position = VictoryCamPos.transform.position;


        PlayerGameObject.transform.LookAt(VictoryPlayerLookAt.transform);
        NewCamObject.transform.LookAt(VictoryCamLookAt.transform);

        Cam3D.transform.LookAt(VictoryCamLookAt.transform);

		//if (PlayerMoving.timer >= 450) MusicManager.Play(BadVictoryTheme);
        //else MusicManager.Play(VictoryTheme);
    }

	private void CheckIfAllIsAvailable()
	{
        MusicManager = FindObjectOfType<MusicManager>();
        PlayerMoving = FindObjectOfType<PlayerMoving>();
        CameraHolderScript = FindObjectOfType<CameraHolderScript>();

    }
	
}
	
