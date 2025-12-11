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

    private bool waitingForNextLine = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueText.text = "";
        StartCoroutine(Typing());
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogue != null && dialogue.Length > 0 && !waitingForNextLine && index < dialogue.Length && dialogueText.text == dialogue[index])
        {
            StartCoroutine(NextLine());
        }
    }

    public void RemoveText()
    {
        dialogueText.text = "";
        index = 0;
    }

    // Public API used by Timeline Playable: start typing a specific line of text
    public void PlayTextLine(string text, float speed, float clipDuration)
    {
        PlayTextLineWithHold(text, speed, 2f); // default 2 second hold
    }

    // Public API variant that accepts custom hold duration
    public void PlayTextLineWithHold(string text, float speed, float holdAfterTyping)
    {
        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(TypeLineRoutine(text, speed, holdAfterTyping));
    }

    private IEnumerator TypeLineRoutine(string text, float speed, float holdAfterTyping)
    {
        isTyping = true;
        dialogueText.text = "";

        float perChar = 1f / Mathf.Max(0.001f, speed);
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(perChar);
        }

        // Hold text for the specified duration
        if (holdAfterTyping > 0f)
            yield return new WaitForSeconds(holdAfterTyping);

        // Optionally clear text when done (you can change this behavior)
        // dialogueText.text = "";

        isTyping = false;
        typingCoroutine = null;
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
