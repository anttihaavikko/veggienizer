using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Veggie : MonoBehaviour
{
    public Rigidbody2D body;
    public List<GameObject> objects;
    public bool isHidden = true;
    public GameObject activator;
    public SpriteRenderer sprite;
    public List<Sprite> sprites;
    public List<GameObject> types;
    public SpeechBubble bubble;
    public Transform speechPoint;

    private int value;
    private int times;
    private bool mad;

    private int type;
    private bool failed;

    private Veggie friend;

    private void Start()
    {
        if(isHidden)
            objects.ForEach(g => g.SetActive(false));

        sprite.sprite = sprites.OrderBy(s => Random.value).First();

        type = Random.Range(0, types.Count);
        types[type].SetActive(true);
    }

    public string GetName()
    {
        return types[type].name;
    }

    public void Appear(int val)
    {
        value = Random.Range(1 + val / 2, 5 + val * 2);
        objects.ForEach(g => g.SetActive(true));
        activator.SetActive(false);
    }

    public string GetIntro()
    {
        switch(type)
        {
            // yam
            case 0:
                return GetRandom(new string[]{
                    "What a specimen",
                    "Ohh, sweet sweet (yam)",
                    "What a thick (yam)"
                }, new string[]{
                    "I could eat these all day!"
                });
            // carrot
            case 1:
                return GetRandom(new string[]{
                    "What a specimen",
                    "What a long (carrot)",
                    "Sweet, a (carrot)",
                    "Oh my, that's a damn long (carrot)"
                }, new string[]{
                    "I could eat these all day!",
                    "Whats up doc?",
                    "Its vibrant color matches my hair!"
                });
            // ginger
            case 2:
                return GetRandom(new string[]{
                    "That would be a (ginger)",
                    "The spicy and versatile (ginger)"
                }, new string[]{
                    "So damn fresh!",
                    "Fresh like an ocean breeze.",
                    "I love (gingers)! I have a feeling it originates from Jessica Rabbit.",
                    "Some people think they have no souls..."
                });
            // potato
            case 3:
                return GetRandom(new string[]{
                    "What a specimen",
                    "The humble (potato)",
                    "What a classic! A (potato)"
                }, new string[]{
                    "I could eat these all day!"
                });
            // turnip
            case 4:
                return GetRandom(new string[]{
                    "What a specimen",
                    "Ah, a (turnip)",
                    "Oh my, it's a (turnip)"
                }, new string[]{
                    "Now I feel like Mario.",
                    "Some people throw these at shy guys..."
                });
            // beet
            case 5:
                return GetRandom(new string[]{
                    "What a specimen",
                    "What a vibrant color! (Beets) are always great"
                }, new string[]{
                    "I could eat these all day!"
                });
            default:
                return "Nice, this (" + GetName() + ") is worth (" + value + ").";
        }
    }

    private string GetValueText()
    {
        var options = new string[] {
             " and it's worth (VALUE). ",
             " worth (VALUE). ",
             " valued at (VALUE). ",
             " coming strong with a value of (VALUE). ",
             " coming in hot with a value of (VALUE). "
        };

        var text = options.OrderBy(s => Random.value).First();
        return text.Replace("VALUE", value.ToString());
    }

    private string GetRandom(string[] starts, string[] ends)
    {
        var start = starts.OrderBy(s => Random.value).First();
        var end = ends.OrderBy(s => Random.value).First();
        var mid = GetValueText();
        var text = start + mid + end;
        text = text.Replace("NAME", GetName());
        text = text.Replace("VALUE", value.ToString());
        return text;
    }

    public int GetValue()
    {
        return value;
    }

    public bool IsEx()
    {
        return times > 0;
    }

    public bool CanPick()
    {
        if (mad)
            return false;

        mad = Random.value < Mathf.Pow(0.6f, times);

        return !mad;
    }

    public string GetExInfo()
    {
         return "This (" + GetName() +") was worth (" + value + ").";
    }

    public void Speak(string message)
    {
        bubble.followTarget = transform;
        bubble.ShowMessage(message);
    }

    public void DenyEx()
    {
        var starts = new string[] {
             "No way!",
             "Go (screw) yourself!",
             "Eww, (no)!",
             "What! Are you (kidding)?",
             "Ew! (Just ew)!"
        };

        var ends = new string[]
        {
            "Not after how (you've treated) me.",
            "Not after all we've (been through).",
            "You treated me like (garbage).",
            "You treated me like a (day old produce)."
        };

        var text = starts.OrderBy(s => Random.value).First() + "\n" + ends.OrderBy(s => Random.value).First();
        Speak(text);
    }

    public void DenyCauseFriend()
    {
        times = 0;

        var starts = new string[] {
             "No way!",
             "Get off me!",
             "Hands off!",
             "Oh hell no!"
        };

        var ends = new string[]
        {
            "I was a good friend of (" + friend.GetName() + ") valued (" + friend.GetValue() + ").",
            "I'm a friend of (" + friend.GetName() + ") valued (" + friend.GetValue() + ") and you treated them like crap.",
            "I'm a friend of (" + friend.GetName() + ") valued (" + friend.GetValue() + ") and you treated them awfully.",
            "After how you treated (" + friend.GetName() + ") valued (" + friend.GetValue() + "), now way (hose)."
        };

        var text = starts.OrderBy(s => Random.value).First() + "\n" + ends.OrderBy(s => Random.value).First();
        Speak(text);
    }

    public void DenyCauseFail()
    {
        times = 0;

        var starts = new string[] {
             "No way!",
             "Get off me!",
             "Hands off!",
             "Oh hell no!"
        };

        var ends = new string[]
        {
            "You (missed) your chance.",
            "I'm way out of your (league)!",
            "Do I need a (restraining order)?"
        };

        var text = starts.OrderBy(s => Random.value).First() + "\n" + ends.OrderBy(s => Random.value).First();
        Speak(text);
    }

    public void AllowEx()
    {
        var starts = new string[] {
             "Yes!",
             "Of course (dear)!",
             "I was (here waiting) all this time!",
             "I knew you'd be back.",
             "Yes dear!",
             "Yes honey!",
             "Oui (mon cher)!",
             "Always (my love)!"
        };

        var ends = new string[]
        {
            "Never (leave me) again.",
            "I still have (feelings) for you.",
            "Lets give it (another try)...",
            "(Please) never leave me again...",
            "I was here (waiting) for you."
        };

        var text = starts.OrderBy(s => Random.value).First() + "\n" + ends.OrderBy(s => Random.value).First();
        Speak(text);
    }

    public void Increment()
    {
        times++;
    }

    public void SetFriend(Veggie veg)
    {
        friend = veg;
    }

    public bool HasFriend()
    {
        return friend != null;
    }

    public bool HasFailed()
    {
        return failed;
    }

    public void Fail()
    {
        failed = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.relativeVelocity.magnitude > 3f)
        {
            var vol = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.1f, 0.1f, 0.8f);
            AudioManager.Instance.PlayEffectAt(31, transform.position, 0.793f * vol);
            AudioManager.Instance.PlayEffectAt(32, transform.position, 0.147f * vol);
            AudioManager.Instance.PlayEffectAt(33, transform.position, 0.362f * vol);
            AudioManager.Instance.PlayEffectAt(40, transform.position, 1.414f * vol);
            AudioManager.Instance.PlayEffectAt(45, transform.position, 0.483f * vol);
        }
    }
}
