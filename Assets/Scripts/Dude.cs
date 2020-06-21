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
    public SpeechBubble bubble, vegBubble;
    public TMP_Text scoreText, timeText;
    public Appearer gameOverDisplay;

    private float pullAmount;
    private Veggie veggie, peekVeggie;
    private bool pulling;
    private bool carrying;
    private bool hasReset;

    private int currentValue;
    private float shownScore;
    private int score;
    private float timeLeft;
    private bool ended;
    private bool canRestart;
    private bool autoPull;

    private List<Veggie> exes;

    private void Start()
    {
        timeLeft = 60 * 12;
        exes = new List<Veggie>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.R) || canRestart && Input.anyKeyDown)
        {
            SceneChanger.Instance.ChangeScene("Main");
        }

        if (Application.isEditor)
        {
            Time.timeScale = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;
        }

        if (veggie && pulling || carrying)
        {
            veggie.body.bodyType = RigidbodyType2D.Kinematic;
            veggie.transform.position = veggiePosition.position;
        }

        if (autoPull)
        {
            pullAmount += Time.deltaTime * 5f;
            anim.SetFloat("pull", pullAmount);

            if (pullAmount >= 1f)
            {
                autoPull = false;
            }
            else
                return;
        }

        shownScore = Mathf.MoveTowards(shownScore, score, Time.deltaTime * 10f);
        scoreText.text = Mathf.FloorToInt(shownScore).ToString();

        timeLeft -= Time.deltaTime;

        if(timeLeft > 0)
        {
            var t = System.TimeSpan.FromSeconds(timeLeft);
            timeText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }
        else
        {
            if(!ended)
            {
                DoEnd();
            }
        }

        DoPull();

        anim.SetBool("pulling", pulling);
        anim.SetFloat("pull", pullAmount);

        if (bubble.IsShown() || vegBubble.IsShown())
            return;

        var button = InputMagic.Instance.GetButtonDown(InputMagic.X);

        if (veggie && button && !carrying)
        {
            pc.body.velocity = Vector2.zero;
            pc.body.bodyType = RigidbodyType2D.Kinematic;
            pc.canControl = false;
            pulling = true;

            EffectManager.Instance.AddEffectToParent(1, veggie.transform.position + Vector3.down * 0.5f, veggie.transform);

            if (veggie.IsEx())
            {
                if(veggie.CanPick())
                {
                    autoPull = true;
                    veggie.AllowEx();
                }
                else
                {
                    veggie.DenyEx();
                    inputs.Hide();
                    pulling = false;
                    pc.canControl = true;
                    pc.body.bodyType = RigidbodyType2D.Dynamic;
                    veggie = null;
                }
            }

            if (veggie && veggie.HasFailed())
                autoPull = true;

            if (!autoPull && veggie)
            {
                inputs.SetPosition(veggie.transform.position + Vector3.up * 4.5f);
                var amount = Mathf.Clamp(Mathf.FloorToInt(veggie.GetValue() / 5f) + 3, 3, 9);
                inputs.Initialize(amount);
            }
        }

        if (veggie && button && carrying && !peekVeggie)
            Drop();

        if (veggie && button && carrying && peekVeggie)
            Peek();

        anim.SetBool("pulling", pulling);
        anim.SetFloat("pull", pullAmount);
    }

    void DoPull()
    {
        if (pulling)
        {
            if (pullAmount >= 1f)
            {
                var text = veggie.GetIntro();

                if (!veggie.IsEx())
                {
                    var failed = false;

                    if(veggie.HasFailed())
                    {
                        failed = true;
                    }
                    else
                    {
                        foreach (var v in exes)
                        {
                            if (Random.value < 0.05f)
                            {
                                failed = true;
                                veggie.SetFriend(v);
                                break;
                            }
                        }
                    }

                    if (!failed)
                        bubble.ShowMessage(text);
                }
                else
                {
                    veggie.AllowEx();
                }

                veggie.Increment();

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
                veggie.body.SetRotation(0);

                inputs.Hide();

                pc.canControl = true;

                if (veggie && veggie.HasFriend())
                {
                    veggie.DenyCauseFriend();
                    Drop();
                }

                if(veggie && veggie.HasFailed())
                {
                    veggie.DenyCauseFail();
                    Drop();
                }

                if(veggie)
                {
                    AudioManager.Instance.PlayEffectAt(45, veggie.transform.position, 0.922f);
                    AudioManager.Instance.PlayEffectAt(47, veggie.transform.position, 1.405f);
                    AudioManager.Instance.PlayEffectAt(53, veggie.transform.position, 1.329f);
                }

                if(veggie)
                {
                    EffectManager.Instance.AddEffectToParent(1, veggie.transform.position + Vector3.down * 0.5f, veggie.transform);
                    EffectManager.Instance.AddEffectToParent(4, veggie.transform.position + Vector3.down * 0.5f, veggie.transform);
                }
            }

            float inputX = InputMagic.Instance.GetAxis(InputMagic.STICK_OR_DPAD_X);
            float inputY = InputMagic.Instance.GetAxis(InputMagic.STICK_OR_DPAD_Y);
            var treshold = 0.2f;

            if (Mathf.Abs(inputX) < treshold && Mathf.Abs(inputY) < treshold)
                hasReset = true;

            if (hasReset)
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
    }

    void DoEnd()
    {
        ended = true;
        pc.canControl = false;
        pc.body.velocity = new Vector2(0, pc.body.velocity.y);
        pc.body.bodyType = RigidbodyType2D.Static;
        anim.SetFloat("speed", 0);

        gameOverDisplay.ShowWithText("You scored <color=#99C24D>" + score + "</color>", 1f);

        ScoreManager.Instance.SubmitScore(PlayerPrefs.GetString("PlayerName"), score, score);

        Invoke("EnableRestart", 2f);
    }

    void EnableRestart()
    {
        canRestart = true;
    }

    private void Drop()
    {
        exes.Add(veggie);
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
        {
            pullAmount = amount;
        }
        else
        {
            veggie.Fail();
            veggie.DenyCauseFail();
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
            if(Random.value < 0.1f + score * 0.01f)
            {
                var v = collision.gameObject.GetComponentInParent<Veggie>();
                v.Appear(score);
                v.bubble = vegBubble;
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
                bubble.ShowMessage("Looks like there is some (vegetable) here. I could pick it up with  <sprite=3>  or  <sprite=2>.");
                veggie = veg;
            }
            else if(veg != veggie)
            {
                bubble.ShowMessage("I can only carry one (veggie) at a time but I could still peek at it with\n  <sprite=3>  or  <sprite=2>.");
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
