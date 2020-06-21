using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartView : MonoBehaviour
{
    public TMP_Text names, scores, nameText;
    public List<Appearer> hideOnStart, showOnStart;
    public List<GameObject> disableOnStart, enableOnStart;
    public NameInput nameInput;
    public Appearer nameInputAppearer, startHelpAppearer;

    private ScoreManager scoreManager;
    private bool started;
    private bool canStart;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = ScoreManager.Instance;
        scoreManager.LoadLeaderBoards(0);

        nameInput.Ask();
        nameInput.onUpdate += UpdateInputs;
        nameInput.onDone += GotName;
    }

    // Update is called once per frame
    void Update()
    {
        names.text = scoreManager.leaderBoardPositionsString;
        scores.text = scoreManager.leaderBoardScoresString;

        if (!canStart)
            return;

        if (!started && InputMagic.Instance.GetButtonUp(InputMagic.A) ||
            InputMagic.Instance.GetButtonUp(InputMagic.X) ||
            Input.GetKeyUp(KeyCode.Return))
        {
            started = true;
            hideOnStart.ForEach(a => a.Hide());
            showOnStart.ForEach(a => a.Show());
            disableOnStart.ForEach(g => g.SetActive(false));
            enableOnStart.ForEach(g => g.SetActive(true));
        }
    }

    void UpdateInputs(string plrName)
    {
        nameText.text = "Enter your name to start: " + plrName;
    }

    void GotName(string plrName)
    {
        canStart = true;
        nameInputAppearer.Hide();
        startHelpAppearer.Show();
    }

    private void OnDestroy()
    {
        nameInput.onUpdate -= UpdateInputs;
        nameInput.onDone -= GotName;
    }
}
