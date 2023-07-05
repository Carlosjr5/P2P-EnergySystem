using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using ActressMas;

namespace EnergySystem23
{
    class Program
    {
       

        //Setup of the agent, which gets the number of households, deals and announcements.
        public static class HouseholdSetup
        {
            public static int totalNumberOfHouseholds;
            public static int numberOfDeals = 0;
            public static int numberOfAnnouncements = 0;
            public static bool protocol;
            public static int compradores = 0;
            public static int vendedores = 0;
            public static int dealsCompleted = 0;
            public static bool sprotocol;
            internal static decimal household_Money;
            internal static DateTime startTime;

            // Define a list to store the win rate values
          public static List<decimal> winRateValues = new List<decimal>();
            public static List<decimal> buyersSavesRateValues = new List<decimal>();


            public static int numberOfHouseholds;

        }
      

        //Main function of the system.
       public  static void Main()
        {
           


            //Sets the environment fot the agents to comunicate.
            var environment = new EnvironmentMas(noTurns: 1000, randomOrder: false, parallel: false);
            //Adding the enviromentAgent class to the system.
            EnvironmentAgent envAgent = new EnvironmentAgent(); environment.Add(envAgent, "environment");


            Console.WriteLine("\n|-----------------------------------------------|");
            Console.WriteLine("|---   Welcome to Carlos P2P Energy System   ---|");
            Console.WriteLine("|---        40452913 -- SET10111             ---|");
            Console.WriteLine("|-----------------------------------------------|\n");
            /*
            Console.WriteLine("\n|---------------------------------------------------|");
            Console.WriteLine("|---   Enter a path to save the market summarize   ---|");
            Console.WriteLine("|---           E.g   'C:/User/Desktop/' for windows:           ---|");
            Console.WriteLine("|---           E.g   '/Users/carlosjimenezrodriguez/Desktop/test' for mac:           ---|");
            Console.WriteLine("|---       File will be created on Desktop.         ---|");
            Console.WriteLine("|-----------------------------------------------------|\n");





           // / Users / carlosjimenezrodriguez / Desktop / test
            // Get the input as a string
            string MarketSummarizePath = Console.ReadLine();
            string inputedPath = MarketSummarizePath;

            Console.WriteLine($"Path:" + inputedPath);
            string path = Directory.GetCurrentDirectory();
            getPath( path );
            */

            // Prompt the user to enter a number
            Console.WriteLine("\n  |--------------------------------------------|");
            Console.WriteLine("  |--- Please enter a number of households: ---|");
            Console.WriteLine("  |--------------------------------------------|\n");

          

            bool isValidInputa = false;

            while (!isValidInputa)
            {
                // Get the input as a string
                string input = Console.ReadLine();

                // Convert the string to an integer
                if (int.TryParse(input, out HouseholdSetup.numberOfHouseholds))
                {
                    Console.WriteLine($"You entered a number of {HouseholdSetup.numberOfHouseholds} households.");
                    isValidInputa = true;
                }
                else
                {
                    // Invalid input: input is not a valid number
                    Console.WriteLine("Invalid input. Please enter a valid number of households.");
                }
            }


            // Prompt the user to enter a number
            Console.WriteLine("\n|----------------------------------------------------------|");
            Console.WriteLine("|--- Which Protocol would you like to use? (1 or 2):    ---|");
            Console.WriteLine("|---         1. Double Auction.                         ---|");
            Console.WriteLine("|---         2. Second-Bid Auction.                     ---|");
            Console.WriteLine("|----------------------------------------------------------|\n");

            int numberProtocol;
            bool isValidInput = false;

            while (!isValidInput)
            {
                // Get the input as a string
                string inputProtocol = Console.ReadLine();

                // Convert the string to an integer
                if (int.TryParse(inputProtocol, out numberProtocol))
                {
                    // Check if the number is either 1 or 2
                    if (numberProtocol == 1)
                    {
                        HouseholdSetup.protocol = true;
                        Console.WriteLine($"You entered: {numberProtocol}, which is the Double Auction.");
                        isValidInput = true;
                    }
                    else if (numberProtocol == 2)
                    {
                        HouseholdSetup.protocol = false;
                        Console.WriteLine($"You entered: {numberProtocol}, which is the Second-Bid Auction.");
                        isValidInput = true;
                    }
                    else
                    {
                        // Invalid input: number is neither 1 nor 2
                        Console.WriteLine("Invalid input. Please enter a valid number (1 or 2).");
                    }
                }
                else
                {
                    // Invalid input: input is not a valid number
                    Console.WriteLine("Invalid input. Please enter a valid number (1 or 2).");
                }
            }


            //Was failing sometimes so was testing maybe to ask for more number of household than 3.
            if (HouseholdSetup.numberOfHouseholds <= 1)
            {
                Console.WriteLine("The system needs more than 1 Household in order to work, try with higher number than 1.");

                // Prompt the user to enter a number
                Console.WriteLine("Please enter a number of households:");

                // Get the input as a string
                string input2 = Console.ReadLine();

                // Convert the string to an integer
                HouseholdSetup.numberOfHouseholds = int.Parse(input2);
            }


            //Gets the number of households and adds it into the environment.
            for (int totalHouseholds = 0; totalHouseholds < HouseholdSetup.numberOfHouseholds; totalHouseholds++)
            {
                var totalhousehold = new Households();
                environment.Add(totalhousehold, $"Household[{totalHouseholds + 1}]");
            }


            //Calling the Manager to save the data of the buyer and sellers.
            ManagerAgent manager = new ManagerAgent();
            environment.Add(manager, "manager");

            //Counting up the number of households and announcing it.
            HouseholdSetup.numberOfAnnouncements = HouseholdSetup.numberOfHouseholds + 1;

      


            HouseholdSetup.totalNumberOfHouseholds = HouseholdSetup.numberOfHouseholds;

            environment.Start();
            Console.ReadLine();

        

        }


        public void stopApp()
        {
            Environment.Exit(0);
        }

    }






}
