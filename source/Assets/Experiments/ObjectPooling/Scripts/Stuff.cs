using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Stuff : PooledObject
{
    public Rigidbody Body { get; private set; }
    public MeshRenderer[] MeshRenderers;

    // Use this for initialization
	void Awake ()
	{
	    Body = GetComponent<Rigidbody>();
	    MeshRenderers = GetComponentsInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetMaterial(Material m)
    {
        for (int i = 0; i < MeshRenderers.Length; i++)
        {
            MeshRenderers[i].material = m;
        }
    }

    void OnTriggerEnter(Collider enteredCollider)
    {
        if (enteredCollider.CompareTag("Kill Zone"))
        {
            ReturnToPool();
        }
    }

    void OnLevelWasLoaded()
    {
        ReturnToPool();
    }
}
