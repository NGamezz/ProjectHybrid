using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [Tooltip("Amount of seconds, delay.")]
    [SerializeField] private float writeSpeed = 0.05f;

    [Tooltip("Linger Time In Seconds.")]
    [SerializeField] private float lingerTime = 2.0f;

    [SerializeField] private string[] possibleCorrectDialogue;
    [SerializeField] private string[] possibleInCorrectDialogue;

    [SerializeField] private GameObject textBubbleGameObject;

    [SerializeField] private float customerTimeFrame = 30.0f;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private TMP_Text dialogueText;
    private bool continueDialogue;

    public Action OnFail;

    private Custom.Utility utility = new();
    private bool cancel = false;

    private double timer = 0;
    private double Timer
    {
        get
        {
            return timer;
        }
        set
        {
            if (timer != value)
            {
                timerText.text = $"Time Remaining = {timer:0.0}";
                timer = value;
            }
        }
    }

    private void Awake()
    {
        utility.Setup();
        SetBubble(false);
        OnFail += EndDialogue;
    }

    public void SetDialogueEvent(ref Action<Customer> action)
    {
        action += PlayDialogue;
    }

    public async void SetBubble(bool state)
    {
        await Awaitable.MainThreadAsync();

        timerText.gameObject.SetActive(state);
        textBubbleGameObject.SetActive(state);
    }

    public async void ContinueDialogue(Customer customer, bool correct)
    {
        var text = correct ? utility.GetRandomElementFromArray(possibleCorrectDialogue) :
            utility.GetRandomElementFromArray(possibleInCorrectDialogue);

        await TypeWriterPlay(text);

        if (!correct)
        {
            cancel = true;
        }
        else
        {
            continueDialogue = true;
        }
    }

    public void EndDialogue()
    {
        cancel = true;
    }

    public async void PlayDialogue(Customer customer)
    {
        await Awaitable.MainThreadAsync();

        Debug.Log("Play Dialogue.");
        cancel = false;
        Timer = customerTimeFrame;
        SetBubble(true);
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();

        foreach (var _ in customer.DesiredPotion)
        {
            Timer = customerTimeFrame;

            while (!continueDialogue && !cancel)
            {
                Timer -= 0.1f;
                if (Timer <= 0)
                {
                    OnFail?.Invoke();
                }
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
            continueDialogue = false;

            await TypeWriterPlay(customer.CurrentDialogue());
            customer.NextDialogue();

            if (cancel)
            {
                Debug.Log("Cancel");
                SetBubble(false);
                return;
            }
        }

        SetBubble(false);
    }

    private async Task TypeWriterPlay(string text)
    {
        if (text == "" || cancel)
        { return; }

        await Awaitable.MainThreadAsync();

        dialogueText.text = "";

        foreach (char c in text)
        {
            if (cancel)
            { return; }

            dialogueText.text += c;
            await Awaitable.WaitForSecondsAsync(writeSpeed);
        }

        await Awaitable.WaitForSecondsAsync(lingerTime);
    }
}