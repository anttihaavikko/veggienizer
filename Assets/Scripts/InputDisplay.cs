﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDisplay : MonoBehaviour
{
    public List<Image> icons;
    public Color okColor, failColor;

    private int position;
    private List<int> directions;

    public void Initialize(int count)
    {
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
            }
        }
    }

    public float Input(int direction)
    {
        var cur = icons[position];

        if (directions[position] == direction)
        {
            cur.color = okColor;
            position++;
            return 1f * position / directions.Count; 
        }
        else
        {
            cur.color = failColor;
            return -1f;
        }
    }
}