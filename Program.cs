/*
This application manages transactions at a store check-out line. The
check-out line has a cash register, and the register has a cash till
that is prepared with a number of bills each morning. The till includes
bills of four denominations: $1, $5, $10, and $20. The till is used
to provide the customer with change during the transaction. The item 
cost is a randomly generated number between 2 and 49. The customer 
offers payment based on an algorithm that determines a number of bills
in each denomination. 

Each day, the cash till is loaded at the start of the day. As transactions
occur, the cash till is managed in a method named MakeChange (customer 
payments go in and the change returned to the customer comes out). A 
separate "safety check" calculation that's used to verify the amount of
money in the till is performed in the "main program". This safety check
is used to ensure that logic in the MakeChange method is working as 
expected.
*/


//string? readResult = null;
bool useTestData = false;

Console.Clear();

int[] cashTill = new int[] { 0, 0, 0, 0 };
int registerCheckTillTotal = 0;
int operationCounter = 0;

// registerDailyStartingCash: $1 x 50, $5 x 20, $10 x 10, $20 x 5 => ($350 total)
int[,] registerDailyStartingCash = new int[,] { { 1, 50 }, { 5, 20 }, { 10, 10 }, { 20, 5 } };

int[] testData = new int[] { 6, 10, 17, 20, 31, 36, 40, 41 };
int testCounter = 0;

LoadTillEachMorning(registerDailyStartingCash, cashTill);

registerCheckTillTotal = registerDailyStartingCash[0, 0] * registerDailyStartingCash[0, 1] + registerDailyStartingCash[1, 0] * registerDailyStartingCash[1, 1] + registerDailyStartingCash[2, 0] * registerDailyStartingCash[2, 1] + registerDailyStartingCash[3, 0] * registerDailyStartingCash[3, 1];

// display the number of bills of each denomination currently in the till
LogTillStatus(cashTill);

// display a message showing the amount of cash in the till
Console.WriteLine(TillAmountSummary(cashTill));

// display the expected registerDailyStartingCash total
Console.WriteLine($"Expected till value: {registerCheckTillTotal}");
Console.WriteLine();

var valueGenerator = new Random((int)DateTime.Now.Ticks);

int transactions = 100;

if (useTestData)
{
    transactions = testData.Length;
}

while (transactions > 0)
{
    transactions -= 1;
    int itemCost = valueGenerator.Next(2, 50);
    operationCounter += 1;

    if (useTestData)
    {
        itemCost = testData[testCounter];
        testCounter += 1;
    }

    int paymentOnes = itemCost % 2;                 // value is 1 when itemCost is odd, value is 0 when itemCost is even
    int paymentFives = (itemCost % 10 > 7) ? 1 : 0; // value is 1 when itemCost ends with 8 or 9, otherwise value is 0
    int paymentTens = (itemCost % 20 > 13) ? 1 : 0; // value is 1 when 13 < itemCost < 20 OR 33 < itemCost < 40, otherwise value is 0
    int paymentTwenties = (itemCost < 20) ? 1 : 2;  // value is 1 when itemCost < 20, otherwise value is 2

    // display messages describing the current transaction
    Console.WriteLine($"\nOperation #{operationCounter}");
    Console.WriteLine($"\nCustomer is making a ${itemCost} purchase");
    Console.WriteLine($"\t Using {paymentTwenties} twenty dollar bills");
    Console.WriteLine($"\t Using {paymentTens} ten dollar bills");
    Console.WriteLine($"\t Using {paymentFives} five dollar bills");
    Console.WriteLine($"\t Using {paymentOnes} one dollar bills\n");

    bool transactionSucceeded = false;
    try
    {
        // MakeChange manages the transaction and updates the till 
        MakeChange(itemCost, cashTill, paymentTwenties, paymentTens, paymentFives, paymentOnes);

        //if no exception is thrown, mark the transaction as successful
        transactionSucceeded = true;
        if (!transactionSucceeded)
        {
            Console.WriteLine($"transactionSucceeded: {transactionSucceeded}");
        }
    }
    catch (ApplicationException e)
    {
        Console.WriteLine($"Could not complete transaction (operation #{operationCounter}): {e.Message}");
    }

    if (transactionSucceeded)
    {
        // Backup Calculation - each transaction adds current "itemCost" to the till
        registerCheckTillTotal += itemCost;
    }

    Console.WriteLine(TillAmountSummary(cashTill));
    string tillSummary = TillAmountSummary(cashTill); // Store result in a variable
    Console.WriteLine($"Expected till value: {registerCheckTillTotal}");
    Console.WriteLine();
}

//Console.WriteLine("Press the Enter key to exit");
//do
//{
//    readResult = Console.ReadLine();

//} while (readResult == null);

Console.WriteLine("Press any key to exit");
Console.ReadKey();

static void LoadTillEachMorning(int[,] registerDailyStartingCash, int[] cashTill)
{
    cashTill[0] = registerDailyStartingCash[0, 1];
    cashTill[1] = registerDailyStartingCash[1, 1];
    cashTill[2] = registerDailyStartingCash[2, 1];
    cashTill[3] = registerDailyStartingCash[3, 1];
}


static void MakeChange(int cost, int[] cashTill, int twenties, int tens = 0, int fives = 0, int ones = 0)
{
    int changeNeeded = 0;
    try
    {
        int amountPaid = twenties * 20 + tens * 10 + fives * 5 + ones;
        changeNeeded = amountPaid - cost;

        if (changeNeeded < 0)
            throw new InvalidOperationException("InvalidOperationException: Not enough money provided to complete the transaction.");
        
        //instead of immediately moving bills, I'll first check with the new CanMakeChange method that the till actually can make change
        //PASS 1: Check if we have enough bills without modifying cashTill
        int tempChangeNeeded = changeNeeded;
        int[] tempTill = (int[])cashTill.Clone(); //clone cashTill for a simulation

        if (!CanMakeChange(tempChangeNeeded, tempTill))
            throw new InvalidOperationException("InvalidOperationException: The till is unable to make change for the cash provided.");

        //PASS 2: Actually dispense change
        cashTill[3] += twenties;
        cashTill[2] += tens;
        cashTill[1] += fives;
        cashTill[0] += ones;

        Console.WriteLine("Cashier prepares the following change:");
        while ((changeNeeded > 19) && (cashTill[3] > 0))
        {
            cashTill[3]--;
            changeNeeded -= 20;
            Console.WriteLine("\t A twenty dollar bill");
        }

        while ((changeNeeded > 9) && (cashTill[2] > 0))
        {
            cashTill[2]--;
            changeNeeded -= 10;
            Console.WriteLine("\t A ten dollar bill");
        }

        while ((changeNeeded > 4) && (cashTill[1] > 0))
        {
            cashTill[1]--;
            changeNeeded -= 5;
            Console.WriteLine("\t A five dollar bill");
        }

        while ((changeNeeded > 0) && (cashTill[0] > 0))
        {
            cashTill[0]--;
            changeNeeded -= 1;
            Console.WriteLine("\t A one dollar bill");
        }

        if (changeNeeded > 0)
            throw new InvalidOperationException("InvalidOperationException: The till is unable to make change for the cash provided.");
    }
    catch (InvalidOperationException ex)
    {
        if (ex.StackTrace.Contains("MakeChange"))
        {
            throw new ApplicationException($"Error in MakeChange: {ex.Message}");
        }
        else
        {
            throw new ApplicationException($"Error not in MakeChange: {ex.Message}");
        }
    }
}

static void LogTillStatus(int[] cashTill)
{
    Console.WriteLine("The till currently has:");
    Console.WriteLine($"{cashTill[3] * 20} in twenties");
    Console.WriteLine($"{cashTill[2] * 10} in tens");
    Console.WriteLine($"{cashTill[1] * 5} in fives");
    Console.WriteLine($"{cashTill[0]} in ones");
    Console.WriteLine();
}

static string TillAmountSummary(int[] cashTill)
{
    return $"The till has {cashTill[3] * 20 + cashTill[2] * 10 + cashTill[1] * 5 + cashTill[0]} dollars";
}

//added method to check that the till actually can make change, before bills are moved
static bool CanMakeChange(int changeNeeded, int[] tempTill)
{
    int[] denominations = { 20, 10, 5, 1 };

    for (int i = 0; i < denominations.Length; i++)
    {
        //for i = 0, denominations[0] = 20 and tempTill[3 - 0] = cashTill[3 - 0] = cashTill[3] = 20
        while (changeNeeded >= denominations[i] && tempTill[3 - i] > 0)
        {
            tempTill[3 - i]--;
            changeNeeded -= denominations[i];
        }
    }

    return changeNeeded == 0; // If changeNeeded is 0, we successfully simulated making change.
}
