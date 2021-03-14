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

    // Working on a better system
    public float rotationAcceleration;

    public float rotationSpeed;

    public float stepDistance = .1f;
    public int stepCount = 20;



    // Feedback configuration
    public List<GameObject> debugPath;


    private Vector3 lastCameraPosition;

    // Start is called before the first frame update
    void Start() {
        if (!cameraRig) {
            Debug.LogError("Camera Rig was not assigned.");
        }
        if (!playerCamera) {
            Debug.LogError("Player Camera was not assigned.");
        }

        for (int i = 0; i < stepCount; i++) {
            GameObject indicator = CreateIndicator();
            indicator.transform.parent = playerCamera.transform.parent;
            debugPath.Add(indicator);
        }
    }

    // Update is called once per frame
    void Update() {
        if (cameraRig && playerCamera) {
            // Calculating the translation caused by the user moving
            Vector3 newCameraPosition = playerCamera.transform.localPosition;
            newCameraPosition.y = 0;
            Vector3 translation = newCameraPosition - lastCameraPosition;
            translation.y = 0;
            lastCameraPosition = newCameraPosition;

            // Calculate the optimal change to the rotation
            rotationSpeed += CalculateRotationChange(newCameraPosition, translation.normalized, rotationSpeed) * translation.magnitude;
            rotationSpeed = Mathf.Clamp(rotationSpeed, -RadiusToSpeed(minimumCurvature), RadiusToSpeed(minimumCurvature));

            // Update the rotation
            float rotationAmount = rotationSpeed * translation.magnitude;
            // The sign of the rotation changes based on if the user is moving clockwise or counter-clockwise.
            rotationAmount *= Mathf.Sign(Vector3.Cross(translation, newCameraPosition).y);
            // Applied rotation is the opposite of what is desired.
            // The user trying to counter rotation is what gives the desired effect.
            cameraRig.transform.RotateAround(playerCamera.transform.position, Vector3.up, -rotationAmount);
        }
    }

    public float CalculateRotationChange(Vector3 position, Vector3 forward, float angleChange) {
        bool positiveNext = false;
        int bestI = 0;
        float nextScore;
        float bestScore = float.MaxValue;
        for (int i = 0; i <= stepCount; i++) {
            if (i != 0) {
                nextScore = GetPathScore(position, forward, angleChange, true, i, stepCount - i, false);
                if (nextScore < bestScore) {
                    positiveNext = true;
                    bestI = i;
                    bestScore = nextScore;
                }
            }
            if (i != stepCount) {
                nextScore = GetPathScore(position, forward, angleChange, false, i, stepCount - i, false);
                if (nextScore < bestScore) {
                    positiveNext = false;
                    bestI = i;
                    bestScore = nextScore;
                }
            }
        }

        GetPathScore(position, forward, angleChange, positiveNext, bestI, stepCount - bestI, true);

        if (positiveNext) {
            return rotationAcceleration;
        }
        else {
            return -rotationAcceleration;
        }
    }


    public float GetPathScore(Vector3 position, Vector3 forward, float angleChange, bool posFirst, float posSteps, float negSteps, bool showPath) {

        // Cleaning up the input
        Vector3 currentPosition = position;
        currentPosition.y = 0;
        Vector3 currentForward = forward;
        currentForward.y = 0;
        currentForward = currentForward.normalized;
        float currentAngleChange = angleChange;

        float minScore = float.MaxValue;

        //bool aboveTarget = transform.localPosition.magnitude > targetRadius;

        for (int i = 0; i < stepCount; i++) {
            // The angle of rotation changes depending on if going cw or ccw.
            float movementdirrection = Mathf.Sign(Vector3.Cross(currentForward, currentPosition).y);

            if (posFirst) {
                if (i < posSteps) {
                    currentAngleChange += rotationAcceleration * stepDistance;
                }
                else if (i < posSteps + negSteps) {
                    currentAngleChange -= rotationAcceleration * stepDistance;
                }
            }
            else {
                if (i < negSteps) {
                    currentAngleChange -= rotationAcceleration * stepDistance;
                }
                else if (i < posSteps + negSteps) {
                    currentAngleChange += rotationAcceleration * stepDistance;
                }
            }


            currentPosition += currentForward * stepDistance;
            currentAngleChange = Mathf.Clamp(currentAngleChange, -RadiusToSpeed(minimumCurvature), RadiusToSpeed(minimumCurvature));
            currentForward = Quaternion.Euler(0, movementdirrection * currentAngleChange * stepDistance, 0) * currentForward;

            if (showPath) {
                debugPath[i].transform.localPosition = currentPosition + (0.05f + Mathf.Min(0.5f, GetLocationScore(currentPosition, currentForward, currentAngleChange))) * Vector3.up;
            }

            // Prevent over-optimising local conditions by igoning the first few steps.
            if (i > 4) {
                minScore = Mathf.Min(minScore, GetLocationScore(currentPosition, currentForward, currentAngleChange));
            }
            else {
                debugPath[i].transform.localScale = 0.035f * Vector3.one;
            }
        }

        return minScore;
    }

    public float GetLocationScore(Vector3 localPosition, Vector3 forward, float angleChange) {
        float score = 0;
        // Distance from the target radius.
        score += Mathf.Pow(Mathf.Abs(localPosition.magnitude - targetRadius), 2);
        // Difference between curvatures
        score += Mathf.Pow(Mathf.Abs(RadiusToSpeed(angleChange) + targetRadius), 2);
        // Difference between centers
        Vector3 centerPosition = localPosition + Mathf.Sign(Vector3.Cross(localPosition, forward).y) * targetRadius * Vector3.Cross(Vector3.up, forward).normalized;
        //score += Mathf.Pow(centerPosition.magnitude, 2);
        return score;
    }

    private float RadiusToSpeed(float radius) {
        // Prevent returning very large speeds.
        if (Mathf.Abs(radius) < .01f) {
            return 0;
        }
        return 360 / (2 * Mathf.PI * radius);
    }


    /// <summary>
    /// Helper function for creating an indicator without using a prefab.
    /// </summary>
    /// <returns> Returns a colliderless sphere to use as a indicator. </returns>
    public GameObject CreateIndicator() {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(indicator.GetComponent<SphereCollider>());
        indicator.transform.localScale = 0.05f * Vector3.one;
        return indicator;
    }
}
