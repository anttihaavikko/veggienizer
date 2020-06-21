using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDisplay : MonoBehaviour
{
    public List<Image> icons;
    public Color okColor, failColor;
    public Appearer appearer, bar;
    public Transform barFilling;

    private int position;
    private List<int> directions;

    private float max, cur;
    private bool isOn;

    public void Initialize(int count)
    {
        max = cur = count * 0.5f;

        directions = new List<int>();
        for (var i = 0; i < count; i++)
            directions.Add(Random.Range(0, 4));

        position = 0;

        for (var i = 0; i < icons.Count; i++)
        {
            var active = i < directions.Count;
            icons[i].gameObject.SetActive(active);

            if(active)
            {
                icons[i].transform.rotation = Quaternion.Euler(0, 0, 90 * directions[i]);
                icons[i].color = Color.white;
            }
        }

        appearer.Show();
        bar.Show();
    }

    private void Update()
    {
        if(isOn)
        {
            var size = cur / max;
            barFilling.transform.localScale = new Vector3(size, 1f, 1f);
            cur = Mathf.Max(cur - Time.deltaTime, 0f);
        }
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        bar.transform.position = pos + Vector3.up * 1f;
        isOn = true;
    }

    public float Input(int direction)
    {
        var c = icons[position];

        if (directions[position] == direction && cur > 0)
        {
            c.color = okColor;
            position++;

            if(position == directions.Count)
            {
                isOn = false;
            }

            return 1f * position / directions.Count; 
        }
        else
        {
            c.color = failColor;
            appearer.HideWithDelay();
            bar.HideWithDelay();
            isOn = false;
            return -1f;
        }
    }

    public void Hide()
    {
        appearer.HideWithDelay();
        bar.HideWithDelay();
    }
}
