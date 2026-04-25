using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour 
{
    Transform RespawnPosition;
    [SerializeField] float ZOffset;
    [SerializeField] float YOffset;
    [SerializeField] float XOffset;
    public void RespawnAtPosition(GameObject PlayerObject)
    {
        PlayerObject.transform.position = new Vector3(RespawnPosition.position.x + XOffset, RespawnPosition.position.y + YOffset, RespawnPosition.position.z + ZOffset);
    }
    public void SetNewRespawnPosition(Transform RespawnTransform = null)
    {
        RespawnPosition = RespawnTransform;
    }


}
