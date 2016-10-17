using UnityEngine;

public class StuffSpawner : MonoBehaviour
{
    public float Velocity;
    public FloatRange timeBetweenSpawns, scale, randomVelocity, angularVelocity;
    public Stuff[] stuffPrefabs;
    public Material StuffMaterial;

    private float timeSinceLastSpawn;
    private float currentSpawnDelay;

    [System.Serializable]
    public struct FloatRange
    {
        public float min, max;

        public float RandomInRange
        {
            get { return Random.Range(min, max); }
        }
    }


    // Update is called once per frame
    void FixedUpdate ()
	{
	    timeSinceLastSpawn += Time.deltaTime;
	    if (timeSinceLastSpawn >= currentSpawnDelay)
	    {
	        timeSinceLastSpawn -= currentSpawnDelay;
	        currentSpawnDelay = timeBetweenSpawns.RandomInRange;

            SpawnStuff();
	    }
	}

    void SpawnStuff()
    {
        Stuff prefab = stuffPrefabs[Random.Range(0, stuffPrefabs.Length)];
        Stuff spawn = prefab.GetPooledInstance<Stuff>();
        if (spawn == null) return;
        spawn.transform.localPosition = transform.position;
        spawn.transform.localScale = Vector3.one*scale.RandomInRange;
        spawn.transform.localRotation = Random.rotation;
        spawn.Body.velocity = transform.up*Velocity + Random.onUnitSphere * randomVelocity.RandomInRange;
        spawn.Body.angularVelocity = Random.onUnitSphere*angularVelocity.RandomInRange;
//        spawn.GetComponent<MeshRenderer>().material = StuffMaterial;
        spawn.SetMaterial(StuffMaterial);
    }
}
