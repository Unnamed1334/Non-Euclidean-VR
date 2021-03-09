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
    // Maximum size of the playspace. 
    public float maxRadius = 1.0f;
    // The maximum speed the mapping will attempt to correct.
    // Stored as curvature as it is easier to visualize than angle per meter
    // Too small of a value will make the camera spin wildly. Too large of a value will result in walking out of the play area.
    public float minimumCurvature = .3f;
    // The radius of the circle that the mappling will follow given a long period of straight movement.
    // Too small makes movement difficult while not using most of the play area.
    // Too large makes it easy to exit the play area with sudden dirrection changes.
    public float targetRadius = .5f;

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
            float parallelDistance = Vector3.Dot(translation.normalized, newCameraPosition);
            // Do some cleanup to get good numbers. Apply max radius here as the equations are based on unit distances.
            tangentRadius = 1 / maxRadius * tangentRadius;
            parallelDistance = 1 / maxRadius * parallelDistance;
            tangentRadius = Mathf.Clamp(tangentRadius, -1, 1);
            parallelDistance = Mathf.Clamp(parallelDistance, -1, 1);


            float rotationRadius;
            float idealradius;
            // Cases:
            // -- Moveing outwards, in front of target
            if (parallelDistance > 0) {
                // Fallback case if the player manages to exit the play area.
                // Use the minimum radius to try to correct.
                if (newCameraPosition.magnitude > maxRadius) {
                    rotationRadius = minimumCurvature;
                }
                else {
                    idealradius = minimumCurvature + Mathf.Pow(1 - tangentRadius, 0.0625f) * ((tangentRadius + 0.5f) / 2 - minimumCurvature);
                    // Minimum radius;
                    idealradius = Mathf.Max(idealradius, minimumCurvature);
                    rotationRadius = minimumCurvature + (idealradius - minimumCurvature) * Mathf.Pow(16, -new Vector3(parallelDistance, 0, tangentRadius - .5f).magnitude);
                }
            }
            // -- Moving inwards, behind target
            // ---- Above target
            // ---- below target
            else {
                idealradius = Mathf.Max(minimumCurvature, minimumCurvature + Mathf.Pow(1 - tangentRadius, 0.0625f) * ((tangentRadius + 0.5f) / 2 - minimumCurvature));
                // Strange hackery near the center to help dirrect the player away
                if (newCameraPosition.magnitude < .2f) {
                    idealradius -= Mathf.Min(3, 3 * (2 - 10 * newCameraPosition.magnitude));
                }
                rotationRadius = 1 / ((1 / idealradius) * (1 + parallelDistance) - parallelDistance * (-2 + 4 * tangentRadius));
            }
            // Convert to speed but clamp to the bounds
            float rotAmmount = RadiusToSpeed(Mathf.Max(maxRadius * rotationRadius, minimumCurvature));
            // Deadzone near center to remove the central singularity
            if (newCameraPosition.magnitude < minimumCurvature) {
                rotAmmount *= Mathf.Clamp01(10 * newCameraPosition.magnitude - 2);
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
