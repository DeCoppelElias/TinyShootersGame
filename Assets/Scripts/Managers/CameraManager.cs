using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public static CameraManager Instance { get; private set; }

	[Header("Camera State")]
	[SerializeField] private CameraState cameraState = CameraState.Idle;
	private enum CameraState { Idle, Shaking, Transition }
	private Camera cam;

	[Header("Camera Shake Settings")]
	[SerializeField] private float shakeDuration = 0.5f;
	[SerializeField] private float shakeAmount = 0.7f;
	private float shakeStart = 0;
	private Vector3 shakeOriginalPos;

	[Header("Camera Transition Settings")]
	[SerializeField] private float transitionDuration = 0.5f;
	private Vector3 transitionTarget;
	private float transitionSpeed = 1;

    private void Awake()
    {
		Instance = this;
    }

    private void Start()
    {
		this.cam = Camera.main;
	}
    private void Update()
    {
		if (cameraState == CameraState.Shaking && Time.timeScale > 0)
		{
			if (Time.time - shakeStart <= shakeDuration)
            {
				Vector2 shakeOffset = UnityEngine.Random.insideUnitCircle * shakeAmount;
				cam.transform.position = shakeOriginalPos + new Vector3(shakeOffset.x, shakeOffset.y, 0);
			}
            else
            {
				cam.transform.position = shakeOriginalPos;
				cameraState = CameraState.Idle;
            }
		}
		else if (cameraState == CameraState.Transition)
        {
			Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, transitionTarget, transitionSpeed * Time.deltaTime);

			if (Vector3.Distance(Camera.main.transform.position, transitionTarget) < 0.01f)
            {
				Camera.main.transform.position = transitionTarget;
				cameraState = CameraState.Idle;
			}
		}
	}
	public void ShakeScreen()
	{
		if (cameraState != CameraState.Idle) return;
		cameraState = CameraState.Shaking;

		shakeStart = Time.time;
		shakeOriginalPos = Camera.main.transform.position;
	}

	public void TransitionCamera(Vector3 targetPosition)
    {
		if (cameraState != CameraState.Idle) return;
		cameraState = CameraState.Transition;

		this.transitionTarget = targetPosition;
		float distance = Vector3.Distance(Camera.main.transform.position, transitionTarget);
		this.transitionSpeed = distance / transitionDuration;
    }
}
