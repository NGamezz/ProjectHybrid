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
    [SerializeField] private List<Customer> customers = new List<Customer>();

    private Ingredients currentIngredients;
    private int currentAmountOfIngredients = 0;
    private bool awaitingIngredients = false;

    private SerialPort port;

    private Action<Customer, bool> continuationAction;

    private void Awake ()
    {
        creater = GetComponent<CustomerCreater>();
        dialogueHandler = GetComponent<DialogueHandler>();
    }

    void Start ()
    {
        //Setup();

        var customer = creater.CreateCustomer();
        customers.Add(customer);
    }

    private void Setup ()
    {
        port = new SerialPort("COM4", 9600)
        {
            ReadTimeout = 50
        };
        port.Open();

        dialogueHandler.SetContinuationAction(continuationAction);
    }

    public async void NextCustomer ()
    {
        await Awaitable.BackgroundThreadAsync();

        var customer = creater.CreateCustomer();
        customers.Add(customer);

        await PerformCustomerTask(customer);

        for ( int i = 0; i < customer.DesiredPotion.Count; i++ )
        {
            currentIngredients ^= currentIngredients;

            await AwaitInput(InputIngredients, customer, i);

            customer.AreIngredientsCorrect(currentIngredients, continuationAction);
        }
    }

    public async Task AwaitInput ( Action<string> callBack, Customer customer, int potionIndex )
    {
        await Awaitable.BackgroundThreadAsync();

        while ( awaitingIngredients )
        {
            string data = null;
            try
            {
                data = port.ReadLine();
            }
            catch ( TimeoutException )
            {
                Debug.LogWarning("Timed out.");
            }

            if ( data != null )
            {
                callBack?.Invoke(data);
            }

            if ( currentAmountOfIngredients >= customer.DesiredPotion[potionIndex].AmountOfIngredients )
            {
                DisableAwaitingInput();
            }
        }
    }

    //For externally disabling the waiting.
    public void DisableAwaitingInput ()
    {
        awaitingIngredients = false;
    }

    public void InputIngredients ( string input )
    {
        if ( int.TryParse(input, out int result) )
        {
            var ingredient = (Ingredients)(1 << result);
            currentIngredients |= ingredient;
            currentAmountOfIngredients++;
        }
    }

    //Something which is to be determined;
    private async Task PerformCustomerTask ( Customer customer )
    {
        await Awaitable.BackgroundThreadAsync();

        bool performingTask = true;

        while ( performingTask )
        {
            performingTask = false;
        }

        dialogueHandler.PlayDialogue(customer);

        awaitingIngredients = true;
    }
}