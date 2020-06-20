using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Veggie : MonoBehaviour
{
    public Rigidbody2D body;
    public List<GameObject> objects;
    public bool isHidden = true;
    public GameObject activator;

    private void Start()
    {
        if(isHidden)
            objects.ForEach(g => g.SetActive(false));
    }

    public void Appear()
    {
        objects.ForEach(g => g.SetActive(true));
        activator.SetActive(false);
    }
}
