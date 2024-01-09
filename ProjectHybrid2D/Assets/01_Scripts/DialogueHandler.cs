using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [Tooltip("Amount of seconds, delay.")]
    [SerializeField] private float writeSpeed = 0.05f;

    //Seperate
    [SerializeField] private string[] possibleCorrectDialogue;
    [SerializeField] private string[] possibleInCorrectDialogue;

    [SerializeField] private TMP_Text dialogueText;
    private bool continueDialogue;

    public async void ContinueDialogue(Customer customer, bool correct)
    {
        Debug.Log(correct);

        var text = correct ? Utility.GetRandomElementFromArray(possibleCorrectDialogue) :
            Utility.GetRandomElementFromArray(possibleInCorrectDialogue);
        Debug.Log(text);
        await TypeWriterPlay(text);

        continueDialogue = true;
    }

    public void EndDialogue()
    {

    }

    public async void PlayDialogue(Customer customer)
    {
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();

        var waitingCount = 0;
        await Awaitable.BackgroundThreadAsync();
        foreach (var _ in customer.DesiredPotion)
        {
            while (!continueDialogue)
            {
                waitingCount++;
            }
            waitingCount = 0;
            continueDialogue = false;

            await TypeWriterPlay(customer.CurrentDialogue());
            customer.NextDialogue();
        }
    }

    private async Task TypeWriterPlay(string text)
    {
        if (text == "") { return; }

        await Awaitable.MainThreadAsync();

        Debug.Log("Starting Write.");
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            await Awaitable.WaitForSecondsAsync(writeSpeed);
        }

        await Awaitable.BackgroundThreadAsync();
    }
}