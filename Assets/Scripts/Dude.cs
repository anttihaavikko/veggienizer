using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dude : MonoBehaviour
{
    public Animator anim;
    public Transform veggiePosition;

    private float pullAmount;
    private Veggie veggie;
    private bool pulling;
    private bool carrying;

    // Update is called once per frame
    void Update()
    {
        if (veggie && Input.GetKeyDown(KeyCode.Q) && !carrying)
        {
            if (pulling)
                pullAmount += 0.25f;

            pulling = true;
        }

        anim.SetBool("pulling", pulling);
        anim.SetFloat("pull", pullAmount);

        if(pulling)
        {
            if(pullAmount >= 1f)
            {
                pullAmount = 0;
                anim.SetTrigger("pulled");
                anim.SetBool("pulling", false);
                pulling = false;
                carrying = true;
                anim.SetBool("carrying", true);
            }
        }

        if(veggie && pulling || carrying)
        {
            veggie.transform.position = veggiePosition.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Veggie Pull")
        {
            veggie = collision.gameObject.GetComponentInParent<Veggie>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Veggie Pull")
        {
            veggie = null;
        }
    }
}
