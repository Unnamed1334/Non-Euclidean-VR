using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Movement Mapping acts as the input class for the VR controller.
/// 
/// The class converts the limited movement in the playspace to normal movement in the world.
/// The character contoller can then react as if the player was moving arround normally.
/// </summary>
public class MovementMapping : MonoBehaviour {

    // Required objects
    public GameObject cameraRig;
    public GameObject playerCamera;

    // Playspace configuation
    public float maxRadius = 1.0f;

    // Feedback configuration

    private Vector3 lastCameraPosition;

    // Start is called before the first frame update
    void Start() {
        if (!cameraRig) {
            Debug.LogError("Camera Rig was not assigned.");
        }
        if (!playerCamera) {
            Debug.LogError("Player Camera was not assigned.");
        }
    }

    // Update is called once per frame
    void Update() {
        if (cameraRig && playerCamera) {
            // Get the movement vector of the camera.
            Vector3 newCameraPosition = playerCamera.transform.localPosition;
            newCameraPosition.y = 0;
            Vector3 translation = newCameraPosition - lastCameraPosition;
            translation.y = 0;
            // Distance is stored to make later code cleaner.
            float dist = translation.magnitude;


            // Calculating the new position in p
            float tangentRadius = Mathf.Abs(newCameraPosition.x * translation.normalized.z - newCameraPosition.z * translation.normalized.x);
            tangentRadius = Mathf.Clamp(tangentRadius, -1, 1);
            float parallelDistance = Vector3.Dot(translation.normalized, newCameraPosition);
            tangentRadius = Mathf.Clamp(tangentRadius, 0, 1);

            float rotationRadius = 0;
            float idealradius = 0;
            // Cases:
            // -- Moveing outwards, in front of target
            if (parallelDistance > 0) {
                if (newCameraPosition.magnitude > 1) {
                    rotationRadius = .3f;
                }
                else {
                    idealradius = Mathf.Max(0.3f, 0.3f + Mathf.Pow(1 - tangentRadius, 0.0625f) * ((tangentRadius + 0.5f) / 2 - 0.3f));
                    rotationRadius = 0.3f + (idealradius - 0.3f) * Mathf.Pow(16, -new Vector3(parallelDistance, 0, tangentRadius - .5f).magnitude);
                }
            }
            // -- Moving inwards, behind target
            // ---- Above target
            // ---- below target
            else {
                idealradius = Mathf.Max(0.3f, 0.3f + Mathf.Pow(1 - tangentRadius, 0.0625f) * ((tangentRadius + 0.5f) / 2 - 0.3f));
                // Strange hackery near the center to help dirrect the player away
                if (newCameraPosition.magnitude < .2f) {
                    idealradius -= Mathf.Min(3, 3 * (2 - 10 * newCameraPosition.magnitude));
                }
                rotationRadius = 1 / ((1 / idealradius) * (1 + parallelDistance) - parallelDistance * (-2 + 4 * tangentRadius));
            }
            // Convert to speed
            float rotAmmount = RadiusToSpeed(rotationRadius);
            // Deadzone near center
            if (newCameraPosition.magnitude < .3f) {
                rotAmmount *= Mathf.Max(0, 10 * newCameraPosition.magnitude - 2);
            }

            // Use dirrection and displacement to get the rotation dirrection
            rotAmmount *= Mathf.Sign(Vector3.Cross(translation, newCameraPosition).y) * dist;

            // Use dirrection and displacement to get the rotation
            cameraRig.transform.RotateAround(playerCamera.transform.position, Vector3.up, rotAmmount);


            // Trying to remove some of the parameters so the only thing the player enters is radius.
            //float inputRadius = maxRadius;

            lastCameraPosition = newCameraPosition;
        }
    }

    private float RadiusToSpeed(float radius) {
        // Prevent returning very large speeds.
        if (Mathf.Abs(radius) < .01f) {
            return 0;
        }
        return 360 / (2 * Mathf.PI * radius);
    }
}
