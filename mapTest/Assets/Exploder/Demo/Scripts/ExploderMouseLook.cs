using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
public class ExploderMouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;
    private float kickTimeout = 0.0f;
    private float kickBackRot = 0;
    private bool kickBack;
    private float rotationTarget;

    public Camera mainCamera;

	void Update()
	{
        if (Screen.lockCursor == false)
        {
            return;
        }

	    var rotX = 0.0f;

		if (axes == RotationAxes.MouseXAndY)
		{
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

		    rotX = rotationX;
            mainCamera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);

            if (kickTimeout > 0.0f)
            {
                rotationTarget += Input.GetAxis("Mouse Y")*sensitivityY;
            }
		}
		else if (axes == RotationAxes.MouseX)
		{
		    rotX = Input.GetAxis("Mouse X")*sensitivityX;
            mainCamera.transform.Rotate(0, 0, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

            if (kickTimeout > 0.0f)
            {
                rotationTarget += Input.GetAxis("Mouse Y") * sensitivityY;
            }

		    rotX = transform.localEulerAngles.y;
            mainCamera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
		}

	    kickTimeout -= Time.deltaTime;

        if (kickTimeout > 0.0f)
        {
            rotationY = Mathf.Lerp(rotationY, rotationTarget, Time.deltaTime*10);
        }
        else
        {
            if (kickBack)
            {
                kickBack = false;
                kickTimeout = 0.5f;
                rotationTarget = kickBackRot;
            }
        }

	    gameObject.transform.rotation = Quaternion.Euler(0, rotX, 0);
	}

	void Start()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
	}

    public void Kick()
    {
        kickTimeout = 0.1f;
        rotationTarget = rotationY + (UnityEngine.Random.Range(15, 20));
        kickBackRot = rotationY;
        kickBack = true;
    }
}
