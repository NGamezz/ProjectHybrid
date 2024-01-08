using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [Tooltip("Amount of seconds, delay.")]
    [SerializeField] private float writeSpeed = 2.0f;

    //Seperate
    [SerializeField] private string[] possibleCorrectDialogue;
    [SerializeField] private string[] possibleInCorrectDialogue;

    [SerializeField] private TMP_Text dialogueText;
    private bool continueDialogue;

    public void SetContinuationAction ( Action<Customer, bool> action )
    {
        action += ContinueDialogue;
    }

    private async void ContinueDialogue ( Customer customer, bool correct )
    {
        await TypeWriterPlay(correct ? Utility.GetRandomElementFromArray(possibleCorrectDialogue) :
            Utility.GetRandomElementFromArray(possibleInCorrectDialogue));

        continueDialogue = true;
    }

    public void EndDialogue ()
    {

    }

    public async void PlayDialogue ( Customer customer )
    {
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();
        await TypeWriterPlay(customer.CurrentDialogue());

        await Awaitable.BackgroundThreadAsync();
        foreach ( var _ in customer.DesiredPotion )
        {
            while ( !continueDialogue )
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
            continueDialogue = false;

            await TypeWriterPlay(customer.CurrentDialogue());
            customer.NextDialogue();
        }
    }

    private async Task TypeWriterPlay ( string text )
    {
        dialogueText.text = "";

        foreach ( char c in text )
        {
            dialogueText.text += c;
            await Awaitable.WaitForSecondsAsync(writeSpeed);
        }
    }
}