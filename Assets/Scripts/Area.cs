using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Area : MonoBehaviour
{
    public GameObject cam;
    public bool showMessageOnVisited;
    public List<string> messages;

    private bool visited;

    public void Toggle(bool state)
    {
        cam.SetActive(state);
        visited = true;
    }

    public bool CanShow()
    {
        return messages.Any() && !visited || showMessageOnVisited;
    }
}
