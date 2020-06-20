using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dude : MonoBehaviour
{
    public Animator anim;
    public Transform veggiePosition;
    public InputDisplay inputs;
    public PlatformerController pc;
    public SpeechBubble bubble;

    private float pullAmount;
    private Veggie veggie;
    private bool pulling;
    private bool carrying;
    private bool hasReset;

    // Update is called once per frame
    void Update()
    {
        var button = InputMagic.Instance.GetButtonDown(InputMagic.X);

        if (veggie && button && !carrying)
        {
            pc.body.velocity = Vector2.zero;
            pc.body.bodyType = RigidbodyType2D.Kinematic;
            pc.canControl = false;

            if (!pulling)
            {
                inputs.transform.position = veggie.transform.position + Vector3.up * 4.5f;
                inputs.Initialize(Random.Range(3, 10));
            }

            pulling = true;
        }

        if (veggie && button && carrying)
            Drop();

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
                pc.body.bodyType = RigidbodyType2D.Dynamic;

                veggie.body.velocity = Vector2.zero;
                veggie.body.angularVelocity = 0;
                veggie.transform.rotation = Quaternion.Euler(Vector3.zero);

                inputs.appearer.HideWithDelay();

                pc.canControl = true;
            }

            float inputX = InputMagic.Instance.GetAxis(InputMagic.STICK_OR_DPAD_X);
            float inputY = InputMagic.Instance.GetAxis(InputMagic.STICK_OR_DPAD_Y);
            var treshold = 0.2f;

            if (Mathf.Abs(inputX) < treshold && Mathf.Abs(inputY) < treshold)
                hasReset = true;

            if(hasReset)
            {
                if (inputY > treshold)
                    SendInput(0);

                if (inputY < -treshold)
                    SendInput(2);

                if (inputX > treshold)
                    SendInput(3);

                if (inputX < -treshold)
                    SendInput(1);
            }
        }

        if(veggie && pulling || carrying)
        {
            veggie.body.bodyType = RigidbodyType2D.Kinematic;
            veggie.transform.position = veggiePosition.position;
        }

        if(Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
    }

    private void Drop()
    {
        carrying = false;
        veggie.body.bodyType = RigidbodyType2D.Dynamic;
        anim.SetBool("carrying", false);
        veggie.body.velocity = pc.body.velocity * 1.1f;
        veggie = null;
    }

    private void SendInput(int dir)
    {
        var amount = inputs.Input(dir);
        if(amount >= 0)
            pullAmount = amount;
        else
        {
            veggie = null;
            pulling = false;
            pc.body.bodyType = RigidbodyType2D.Dynamic;
            Invoke("ReturnControl", 0.3f);
        }
        hasReset = false;
    }

    void ReturnControl()
    {
        pc.canControl = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Area")
        {
            var area = collision.gameObject.GetComponentInParent<Area>();

            if(area.CanShow())
            {
                area.messages.ForEach(bubble.QueMessage);
                bubble.CheckQueuedMessages();
            }

            area.Toggle(true);
        }

        if (collision.gameObject.tag == "Veggie Activation")
        {
            if(Random.value < 0.1f)
            {
                collision.gameObject.GetComponentInParent<Veggie>().Appear();
            }
        }

        if (veggie)
            return;

        if(collision.gameObject.tag == "Veggie Pull")
        {
            veggie = collision.gameObject.GetComponentInParent<Veggie>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == veggie)
        {
            veggie = null;
        }

        if (collision.gameObject.tag == "Area")
        {
            collision.gameObject.GetComponentInParent<Area>().Toggle(false);
        }
    }
}
