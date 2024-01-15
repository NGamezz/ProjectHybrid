using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CustomerCreater), typeof(DialogueHandler))]
public class CustomerHandler : MonoBehaviour
{
    private CustomerCreater creater;
    private DialogueHandler dialogueHandler;
    [SerializeField] private List<Customer> customers = new();

    [SerializeField] private float customerMoveSpeed = 5.0f;

    [SerializeField] private UnityEvent<int> potionEvent;

    private Ingredients currentIngredients;
    private int currentAmountOfIngredients = 0;
    private bool awaitingIngredients = false;

    private Vector3 ownPosition;

    private SerialPort port;

    [SerializeField] private string arduinoPort = "COM8";

    private Action<Customer, bool> continuationAction;
    private Action outOfPotionsAction;
    private bool endAwaitInput = false;

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
        ownPosition = transform.position;
        continuationAction += Test;
        outOfPotionsAction += (() => Debug.Log("Out Of Potions."));
        Setup();
    }

    private void Test(Customer customer, bool isCorrect)
    {
        if (!isCorrect)
        {
            endAwaitInput = true;
            awaitingIngredients = false;
        }
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
        Debug.Log("Create Customer");
        var customer = creater.CreateCustomer();
        customers.Add(customer);

        await PerformCustomerTask(customer);

        dialogueHandler.PlayDialogue(customer);

        customer.SetOutOfPotionsAction(outOfPotionsAction);

        await Awaitable.BackgroundThreadAsync();

        for (int i = 0; i < customer.DesiredPotion.Count; i++)
        {
            if (endAwaitInput)
            {
                goto CustomerLeave;
            }

            currentIngredients ^= currentIngredients;
            currentAmountOfIngredients = 0;

            awaitingIngredients = true;

            await AwaitInput(InputIngredients, customer, i);

            if (endAwaitInput)
            {
                goto CustomerLeave;
            }

            customer.AreIngredientsCorrect(currentIngredients, continuationAction);
        }
    CustomerLeave:

        endAwaitInput = false;
        await CustomerLeave(customer);
        NextCustomer();
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
                    potionEvent?.Invoke(result);
                }
            }

            if (currentAmountOfIngredients >= customer.DesiredPotion[potionIndex].AmountOfIngredients || endAwaitInput)
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

    private async Task CustomerLeave(Customer customer)
    {
        await Awaitable.MainThreadAsync();

        Vector3 targetPosition = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(10, 15);

        targetPosition.z = 0;
        var distance = targetPosition - customer.meshObject.transform.position;
        distance.z = 0;

        while (distance.magnitude > 1.0f)
        {
            distance = targetPosition - customer.meshObject.transform.position;
            distance.z = 0;
            customer.meshObject.transform.Translate(customerMoveSpeed * Time.deltaTime * distance.normalized, Space.World);
            await Task.Delay(10);
        }
    }

    private async Task PerformCustomerTask(Customer customer)
    {
        await Awaitable.MainThreadAsync();

        var distance = ownPosition - customer.meshObject.transform.position;
        distance.z = 0;

        while (distance.magnitude > 1.0f)
        {
            distance = ownPosition - customer.meshObject.transform.position;
            distance.z = 0;
            customer.meshObject.transform.Translate(customerMoveSpeed * Time.deltaTime * distance.normalized, Space.World);
            await Task.Delay(10);
        }
    }
}