using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CustomerCreater), typeof(DialogueHandler))]
public class CustomerHandler : MonoBehaviour
{
    private CustomerCreater creater;
    private DialogueHandler dialogueHandler;
    [SerializeField] private List<Customer> customers = new();

    private Ingredients currentIngredients;
    private int currentAmountOfIngredients = 0;
    private bool awaitingIngredients = false;

    private SerialPort port;

    private const string arduinoPort = "COM4";

    private Action<Customer, bool> continuationAction;
    private Action outOfPotionsAction;

    private void Awake()
    {
        creater = GetComponent<CustomerCreater>();
        dialogueHandler = GetComponent<DialogueHandler>();
    }

    private void OnDisable()
    {
        port.Close();
    }

    void Start()
    {
        continuationAction += Test;
        outOfPotionsAction += (() => Debug.Log("Out Of Potions."));
        outOfPotionsAction += NextCustomer;
        Setup();
    }

    private void Test(Customer customer, bool isCorrect)
    {
        currentIngredients ^= currentIngredients;

        Debug.Log(isCorrect);
        Debug.Log(customer.CurrentPotionIndex);
    }

    private void Setup()
    {
        port = new SerialPort(arduinoPort, 9600);
        port.Open();

        continuationAction += dialogueHandler.ContinueDialogue;

        NextCustomer();
    }

    public async void NextCustomer()
    {
        var customer = creater.CreateCustomer();
        customers.Add(customer);

        dialogueHandler.PlayDialogue(customer);

        customer.SetOutOfPotionsAction(outOfPotionsAction);

        await Awaitable.BackgroundThreadAsync();

        for (int i = 0; i < customer.DesiredPotion.Count; i++)
        {
            currentIngredients ^= currentIngredients;
            currentAmountOfIngredients = 0;

            awaitingIngredients = true;

            await AwaitInput(InputIngredients, customer, i);

            customer.AreIngredientsCorrect(currentIngredients, continuationAction);
        }
    }

    public async Task AwaitInput(Action<string> callBack, Customer customer, int potionIndex)
    {
        await Awaitable.BackgroundThreadAsync();

        while (awaitingIngredients)
        {
            string data = null;
            try
            {
                data = port.ReadLine();
            }
            catch (TimeoutException)
            {
                Debug.LogWarning("Timed out.");
            }

            if (data != null && int.TryParse(data, out int result))
            {
                var currentIngredient = (Ingredients)(1 << result);
                if (!((currentIngredients & currentIngredient) != 0))
                {
                    Debug.Log((Ingredients)(1 << result));
                    Debug.Log(data);
                    callBack?.Invoke(data);
                }
            }

            if (currentAmountOfIngredients >= customer.DesiredPotion[potionIndex].AmountOfIngredients)
            {
                DisableAwaitingInput();
            }
        }
    }

    //For externally disabling the waiting. 

    public void DisableAwaitingInput()
    {
        awaitingIngredients = false;
    }

    public void InputIngredients(string input)
    {
        if (int.TryParse(input, out int result))
        {
            var ingredient = (Ingredients)(1 << result);
            currentIngredients |= ingredient;
            currentAmountOfIngredients++;
        }
    }

    //Something which is to be determined;
    private async Task PerformCustomerTask(Customer customer)
    {
        await Awaitable.BackgroundThreadAsync();

        //bool performingTask = true;

        //while (performingTask)
        //{
        //    performingTask = false;
        //}

        //dialogueHandler.PlayDialogue(customer);
    }
}