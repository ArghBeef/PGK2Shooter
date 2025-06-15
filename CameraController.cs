using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(AudioListener))]
public class CameraController : NetworkBehaviour
{
    Camera cam;
    AudioListener listener;

    void Awake()
    {
        cam = GetComponent<Camera>();
        listener = GetComponent<AudioListener>();
    }

    public override void OnNetworkSpawn()
    {
        bool isMine = IsOwner;

        cam.enabled = isMine;
        listener.enabled = isMine;

        gameObject.tag = isMine ? "MainCamera" : "Untagged";
    }
}