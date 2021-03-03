using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic character controller that uses Unity's built in character controller to detect ground and walls.
/// 
/// As MovementMapping is responcible for horizontal movement, the controller only has to del with vertial movement.
/// </summary>
public class BasicVRMovement : MonoBehaviour {
    public GameObject cameraRig;
    public GameObject playerCamera;

    public CharacterController cc;
    public GameObject headIndicator;

    // Start is called before the first frame update
    void Start() {
        if(!cc) {
            cc = GetComponent<CharacterController>();
        }
        if (!headIndicator) {
            headIndicator = new GameObject("ControllerHeadLocation");
        }
    }

    // Update is called once per frame
    void Update() {
        // Move the character controller.
        // Small downwards speed in place of gravity.
        cc.Move(GetTargetPostion() - transform.position + (3.0f * Time.deltaTime) * Vector3.down);

        // Update the vertical position of the controller.
        Vector3 newPosition = cameraRig.transform.position;
        newPosition.y = transform.position.y - 0.5f * GetHeight();
        cameraRig.transform.position = newPosition;

        // Update the Head indicator.
        headIndicator.transform.position = GetHeadPosition();
    }

    public Vector3 GetTargetPostion() {
        return playerCamera.transform.position + (0.5f * GetHeight() - playerCamera.transform.localPosition.y) * Vector3.up;
    }

    public float GetHeight() {
        return 2.0f;
    }

    public Vector3 GetHeadPosition() {
        Vector3 headPosition = transform.position;
        headPosition.y = playerCamera.transform.position.y;
        return headPosition;
    }
}
