using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Dude : MonoBehaviour
{
    public Animator anim;
    public Transform veggiePosition;
    public InputDisplay inputs;
    public PlatformerController pc;
    public SpeechBubble bubble;
    public TMP_Text scoreText, timeText;

    private float pullAmount;
    private Veggie veggie, peekVeggie;
    private bool pulling;
    private bool carrying;
    private bool hasReset;

    private int currentValue;
    private float shownScore;
    private int score;
    private float timeLeft;

    private void Start()
    {
        timeLeft = 60 * 12;
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.isEditor)
        {
            Time.timeScale = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;
        }

        if (veggie && pulling || carrying)
        {
            veggie.body.bodyType = RigidbodyType2D.Kinematic;
            veggie.transform.position = veggiePosition.position;
        }

        shownScore = Mathf.MoveTowards(shownScore, score, Time.deltaTime * 10f);
        scoreText.text = Mathf.FloorToInt(shownScore).ToString();

        if (bubble.IsShown())
            return;

        timeLeft -= Time.deltaTime;

        if(timeLeft > 0)
        {
            var t = System.TimeSpan.FromSeconds(timeLeft);
            timeText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }
        else
        {
            // TODO: end
            pc.canControl = false;
            pc.body.velocity = Vector2.zero;
            anim.SetFloat("speed", 0);
        }

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

            if (veggie.IsEx())
            {
                if(veggie.CanPick())
                {
                    pullAmount = 1f;
                }
                else
                {
                    bubble.ShowMessage("Nope...");
                    inputs.appearer.Hide();
                    pulling = false;
                    pc.canControl = true;
                    veggie = null;
                }
            }
        }

        if (veggie && button && carrying && !peekVeggie)
            Drop();

        if (veggie && button && carrying && peekVeggie)
            Peek();

        anim.SetBool("pulling", pulling);
        anim.SetFloat("pull", pullAmount);

        if(pulling)
        {
            if(pullAmount >= 1f)
            {
                var text = veggie.GetIntro();
                bubble.ShowMessage(text);
                currentValue = veggie.GetValue();
                score = currentValue;

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

        if(Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
    }

    private void Drop()
    {
        score = 0;
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
            if(Random.value < 0.1f + currentValue * 0.01f)
            {
                collision.gameObject.GetComponentInParent<Veggie>().Appear(currentValue);
            }
        }

        if(collision.gameObject.tag == "Veggie Pull")
        {
            var veg = collision.gameObject.GetComponentInParent<Veggie>();

            if(veg.IsEx())
            {
                bubble.ShowMessage(veg.GetExInfo());

                if (!veggie)
                    veggie = veg;

                return;
            }

            if (!veggie)
            {
                bubble.ShowMessage("Looks like there is some (vegetable) here. I could pick it up with <sprite=5>.");
                veggie = veg;
            }
            else if(veg != veggie)
            {
                bubble.ShowMessage("I can only carry one (veggie) at a time but I could still peek at it with <sprite=5>.");
                peekVeggie = veg;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Veggie Pull")
        {
            var veg = collision.gameObject.GetComponentInParent<Veggie>();

            if (veg == peekVeggie)
                peekVeggie = null;

            if (veg == veggie)
                veggie = null;
        }

        if (collision.gameObject.tag == "Area")
        {
            collision.gameObject.GetComponentInParent<Area>().Toggle(false);
        }
    }

    void Peek()
    {
        var amount = peekVeggie.GetValue();
        var estimation = Mathf.CeilToInt(Random.Range(0.5f, 1.5f) * amount);
        bubble.ShowMessage("This vegetable looks like it could be worth something around ("+ estimation + ").");
        peekVeggie = null;
    }
}
