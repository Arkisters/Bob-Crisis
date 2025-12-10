using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneTxt : MonoBehaviour
{

    public float wordSpeed;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogue;
    private int index = 0;

    private bool isTyping = false;
    private bool waitingForNextLine = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueText.text = "";
        StartCoroutine(Typing());
    }

    // Update is called once per frame
    void Update()
    {
        if (!waitingForNextLine && dialogueText.text == dialogue[index])
        {
            StartCoroutine(NextLine());
        }
    }

    public void RemoveText()
    {
        dialogueText.text = "";
        index = 0;
        isTyping = false;
    }

    IEnumerator Typing()
    {
        isTyping = true;
        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / wordSpeed);
        }
    }

    IEnumerator NextLine()
    {
        waitingForNextLine = true;
        yield return new WaitForSeconds(2f);
        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            RemoveText();
        }
        waitingForNextLine = false;
    }

}
