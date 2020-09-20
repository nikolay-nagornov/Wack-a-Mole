using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SphereCollider))]
public class EnemiesArmor : MonoBehaviour
{
    public bool IsActive { get; private set; }

    [SerializeField] private float dropForceMin = 300f;
    [SerializeField] private float dropForceMax = 400f;
    [SerializeField] private Vector3 dropDir;

    private Vector3 startPos;
    private Quaternion startRot;
    private string defaultTag;

    private Transform tr;
    private Transform parent;
    private Rigidbody rb;
    private BoxCollider boxCol;
    private SphereCollider sphereCol;
    private MeshRenderer rend;
    private AudioManager audioMng;

    private void Start()
    {
        tr = transform;
        parent = tr.parent;
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
        sphereCol = GetComponent<SphereCollider>();
        rend = GetComponent<MeshRenderer>();
        audioMng = AudioManager.instance;

        startPos = tr.position;
        startRot = tr.rotation;
        defaultTag = tr.tag;

        //dropDir = tr.TransformDirection(dropDir);

        Deactivate();
    }

    public void Init()
    {
        tr.position = startPos;
        tr.rotation = startRot;
        tr.SetParent(parent);
        sphereCol.enabled = true;
        rend.enabled = true;
        IsActive = true;
    }

    public void Deactivate()
    {
        rend.enabled = false;
        rb.isKinematic = true;
        boxCol.enabled = false;
        sphereCol.enabled = false;
        tr.tag = defaultTag;
        IsActive = false;
    }

    public void Defeate()
    {
        float randomAxisValue = Random.Range(1f, 1.2f);

        tr.SetParent(null);
        rb.isKinematic = false;
        tr.tag = "Untagged";
        sphereCol.enabled = false;
        boxCol.enabled = true;

        rb.AddForce(dropDir * randomAxisValue * Random.Range(dropForceMin, dropForceMax));
        //Handheld.Vibrate();
        audioMng.PlaySound("armor_defeat", true);

        Invoke("Deactivate", 1.5f);
    }
}
