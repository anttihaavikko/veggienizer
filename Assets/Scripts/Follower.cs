using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform target;
	public bool followRotation = true;
    public bool freezeX, freezeY, freezeZ;

    // Update is called once per frame
    void LateUpdate()
    {
        if(target)
		{
            var x = freezeX ? transform.position.x : target.position.x;
            var y = freezeY ? transform.position.y : target.position.y;
            var z = freezeZ ? transform.position.z : target.position.z;

            transform.position = new Vector3(x, y, z);

            if(followRotation)
			    transform.rotation = target.rotation;
		}
    }
}
