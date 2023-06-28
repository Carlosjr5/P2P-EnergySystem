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

        }
      

        //Main function of the system.
       public  static void Main()
        {
           


            //Sets the environment fot the agents to comunicate.
            var environment = new EnvironmentMas(noTurns: 1000, randomOrder: false, parallel: false);
            //Adding the enviromentAgent class to the system.
            EnvironmentAgent envAgent = new EnvironmentAgent(); environment.Add(envAgent, "environment");


            Console.WriteLine("\n|---------------------------------------------|");
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
            Console.WriteLine("\n|-------------------------------------------|");
            Console.WriteLine("|--- Please enter a number of household: ---|");
            Console.WriteLine("|-------------------------------------------|\n");

            // Get the input as a string
            
            string input = Console.ReadLine();

            // Convert the string to an integer
            int numberOfHouseholds = int.Parse(input);

            // Print the number to the console
            Console.WriteLine($"You entered a number of: {numberOfHouseholds} Households.");

            

            // Prompt the user to enter a number
            Console.WriteLine("\n|--------------------------------------------------------|");
            Console.WriteLine("|--- Which Protocol would you like to use? (1 or 2):    ---|");
            Console.WriteLine("|---         1. Second Bid Auction:                     ---|");
            Console.WriteLine("|---         2. Dutch Auction:                          ---|");
            Console.WriteLine("|----------------------------------------------------------|\n");

            // Get the input as a string
            string inputProtocol = Console.ReadLine();

            // Convert the string to an integer
            int numberProtocol = int.Parse(inputProtocol);
          
            if (numberProtocol == 1) 
            {
                HouseholdSetup.protocol = true;
              
                Console.WriteLine($"You entered: {numberProtocol}, which is the Second Bid Auction.");
               // Console.WriteLine("Protocol: " + HouseholdSetup.protocol);


            }
             
            if(numberProtocol == 2 )
            {
                HouseholdSetup.protocol = false;
                Console.WriteLine($"You entered: {numberProtocol}, which is the Dutch Auction.");
              //  Console.WriteLine("Protocol : " + HouseholdSetup.protocol);
            }


            if(numberProtocol != 1 && numberProtocol != 2) 
            {
                Console.WriteLine("You can only choose between '1' or '2', try again.") ;
                return ;
            }

            //Was failing sometimes so was testing maybe to ask for more number of household than 3.
            if (numberOfHouseholds <= 1)
            {
                Console.WriteLine("The system needs more than 1 Household in order to work, try with higher number than 1.");

                // Prompt the user to enter a number
                Console.WriteLine("Please enter a number of households:");

                // Get the input as a string
                string input2 = Console.ReadLine();

                // Convert the string to an integer
                numberOfHouseholds = int.Parse(input2);
            }


            //Gets the number of households and adds it into the environment.
            for (int totalHouseholds = 0; totalHouseholds < numberOfHouseholds; totalHouseholds++)
            {
                var totalhousehold = new Households();
                environment.Add(totalhousehold, $"Household[{totalHouseholds + 1}]");
            }


            //Calling the Manager to save the data of the buyer and sellers.
            ManagerAgent manager = new ManagerAgent();
            environment.Add(manager, "manager");

            //Counting up the number of households and announcing it.
            HouseholdSetup.numberOfAnnouncements = numberOfHouseholds + 1;

      


            HouseholdSetup.totalNumberOfHouseholds = numberOfHouseholds;

            environment.Start();
            Console.ReadLine();

        

        }


        public void stopApp()
        {
            Environment.Exit(0);
        }

    }






}
