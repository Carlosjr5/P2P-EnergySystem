using ActressMas;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text.RegularExpressions;
using static EnergySystem23.Program;


namespace EnergySystem23
{

    //Class running on top of the AcctressMas as an Agent using its environment.
    class Households : Agent
    {
        //Amount of energy household neeeds.
        private int household_Needs;
        //Ammount of energy household generates.
        private int household_Generates;
        //Price of the main source to buy.
        private int mainSourceBuyDeal;
        //Price of the main source to sell.
        private int mainSourceSellDeal;
        //Declaring houselhold, for later use as 'buyer or seller'.
        private string household;
        //Amount of energy household needs to buy.
        private int household_Buys = 0;
        //Amount of energy the household has for selling.
        private int household_Sells = 0;
        //Declaring variable for the household_Money which depends on the prize of the main source.
        private int household_Money;
        //Amount of energy the household bought from seller.
        private int boughtFromSeller = 0;
        //Amount of household_Money seller generates.
        private int sellerIncomes = 0;
        //Price bought in main source
        private int priceBoughMainSource = 0;
        //Integer to get the money received by the seller.
        private int seller_money_sold = 0;
        //Declaring the number of energy sold in total.
        private int numberOfenergySold = 0;
        //Number of energy sold to ulitity.
        private int priceSoldto_mainSource = 0;

        //List with the households who became buyers.
        private List<string> listOfBuyers;
        //List to store the deals offered by buyers.
        private Dictionary<string, int> deals;
        //Counting the messages of deals recieved by the buyers and messages of sellers.
        private int countBuyerDeals = 0;




        List<int> BuyerDealsList = new List<int>();


        private int cuenta = 1;
        private int total = 0;

        //Once the class is called, the environment starts and sets number of deals up as many number of households.
        public override void Setup()
        {
            //Start the enviroment.
            Send("environment", "start");
            //declare string of the list for buyers and deals with the houselhold and price.
            listOfBuyers = new List<string>();
            deals = new Dictionary<string, int>();
            //Set number of deals as number of households.
            HouseholdSetup.numberOfDeals++;

           

        }

        Program program = new Program();

        //Action of the agent with messages and parameters using ActressMas.
        public override void Act(Message message)
        {


            message.Parse(out string action, out List<String> parameters);
            switch (action)
            {
                //Giving the data of each household with their demand and sending it to the manager.
                case "informando":

                    //Declaring the parameters of each variable saved into the list FROM EnvironmentAgent content.
                    household_Needs = Int32.Parse(parameters[0]);
                    household_Generates = Int32.Parse(parameters[1]);
                    mainSourceBuyDeal = Int32.Parse(parameters[2]);
                    mainSourceSellDeal = Int32.Parse(parameters[3]);

                    //Checks if the households needs more than it generates.
                    if (household_Generates > household_Needs)
                    {

                        household = "Seller";
                        //Amount to know how much energy it can sell.
                        household_Sells = household_Generates - household_Needs;


                    }
                    else
                    {

                        household = "Buyer";
                        //Amount to know how much energy needs buy.
                        household_Buys = household_Needs - household_Generates;

                    }

                    //Once the household is declared, save its information to string.
                    string householdData = $"{household} {household_Sells} {household_Buys}";


                    //Sending it to manager in order to proceed with the transaction.
                    Send("manager", householdData);

                    //Printing the data of each housesold.
                    string householdInformacion = (($"{this.Name} -> {household}.  \r\n -> Generates {household_Generates} kWh. \r\n -> Needs {household_Needs} kWh. \r\n -> Buys from main source for £{mainSourceBuyDeal}. \r\n -> Sells to main source for £{mainSourceSellDeal}. \r\n -> Amount of energy to sell: {household_Sells}kWh.  \r\n -> Amount of energy to buy: {household_Buys}kWh. \r\n"));
                    //Saving records into a txt document.
                    saveSummarize(householdInformacion);


                    Console.WriteLine(householdInformacion);


                    //Sum number of deals to check for it.
                    HouseholdSetup.numberOfDeals++;

                    break;


                case "sellers_deals":
                    //Checks if the household has energy to sell.
                    if (household_Sells > 0)
                    {
                        //Printing how much energy does the household has for selling.
                        sellerDeals();
                        

                    }
                    else
                    {
                        //Sending the message case to the managerAgent class as seller_decline.
                        Send("manager", "seller_decline");
                        HouseholdSetup.numberOfDeals++;
                    }
                break;


                //Printing the list of buyers in order to set a deal for each of them, once its requested by ManagerAgent class.
                case "listOfBuyers":


                    foreach (string buyer in parameters)
                    {

                        //For each buyer add the buyer parameters into the list.
                        listOfBuyers.Add(buyer);

                    

                    }
                    //Function to add the seller household name and set the list of buyers.
                    buyer_setDeal();


                    break;

                //Used for when the manager decides who gets the deal.
                //Depending on higher and second price.
                case "buyerGetsDeal":

                    //Setting the household wallet to negative depending on amount paid.
                    household_Money -= Int32.Parse(parameters[0]);
                    //Amount of energy bought sums 1.
                    boughtFromSeller++;

                    //seller household gets one energy less to sell.
                    sellerIncomes += Int32.Parse(parameters[0]);
                    //Setting to one less the amount of energy needed to buy.
                    household_Buys--;
                    break;

                //Case for printing the deals of each buyer.
                case "buyersDeal":




             

                    //Checks if the amount of energy the household needs to buy is more than 0.
                    if (household_Buys != 0)
                    {

                      

                        //while it needs to buy more energy, will send a deal with an amount of 1 pounds less of what it pays to main source.
                        Send(message.Sender, $"buyerOffers {mainSourceBuyDeal - 1}");
                        HouseholdSetup.numberOfDeals++;
                        //Saves the deals of the buyer(household) withing the amount able to pay and total energy left needed to buy.
                        string buyerDealsData = ($"\n" + "          |----------------------------------|\n" +
                                              $"                --- Buyers Deals {HouseholdSetup.compradores++} ---  \n " +
                                              $"\n          |-> {this.Name}                     |\n" +
                                              $"          |-> Offers £{mainSourceBuyDeal - 1}/kWh             |\n" +
                                              $"          |-> Total energy needed: {household_Buys}kWh. |\n" +
                                              $"          |-----------------------------------|\n");

               
                    
                      
                        //Prints data into system.
                        Console.WriteLine(buyerDealsData);
                        //Saving records into the txt document.
                        saveSummarize(buyerDealsData);








                    }
                    if (household_Buys == 0)
                    {

                        //No need of buying more energy, send a decline message.
                        Send(message.Sender, "buyerDeclines");
                        //Counting until last deal.
                        HouseholdSetup.numberOfDeals++;
                    }
                    break;

                //Sellers receive this when a buyer submits a proposal
                case "buyerOffers":
                    //One more offer added to the system, 
                    countBuyerDeals++;

                   
                    deals.Add(message.Sender, Int32.Parse(parameters[0]));
                    //While the number of deals and the number of buyers are the same, there will be deal open.
                    //so has to be validated with the method for ValidateDeal().
                    if (countBuyerDeals == listOfBuyers.Count)
                    {


                     



                        //The transactions are validated using this function
                        //which checks for first and second higher prices in the market.

                        if (HouseholdSetup.protocol == true) 
                        {
                            ValidateDeal();
                            break;
                        }
                        if (HouseholdSetup.protocol == false)
                        {
                            ValidateDutchDeal();
                            break;
                        }
                    }
                    break;

                //Sellers receive this when a buyer submits a rejection
                case "buyerDeclines":
                    countBuyerDeals++;



                    if (countBuyerDeals == listOfBuyers.Count)
                    {
                        if (HouseholdSetup.protocol == true)
                        {
                            ValidateDeal();
                            break;
                        }

                        if (HouseholdSetup.protocol == false)
                        {
                            ValidateDutchDeal();
                            break;
                        }
                    }
                    break;


                //When the deal is evaluated the manager receives this case and after setting seller deals, sets deals of buyers.
                case "SellerdealCompleted":
                    //Function to add the seller household name and set the list of buyers.
                    buyer_setDeal();
                    break;


                //When there are no more buyers or sellers the housholds will buy from main source.
                case "BuyMainSource":
                    BuyFromMainSource();
                    break;

                case "SellMainSource":
                    SellToMainSource();
                    break;

                case "BuyMain":
                    BuyMain();
                    BuyerSummarize();
                    break;

                case "SellMain":
                    SellMain();
                    //  SellerSummarize();
                    break;

                //Received when it's time to total up costs at end
                case "BuyersSummarize":
                    BuyerSummarize();
                
                    break;

                //Received when it's time to total up costs at end
                case "SellersSummarize":
                    SellerSummarize();
                  
                    break;

                case "fin":
                    menu();
                    break;
            }
        }


        //Function to validate the deals and candidate to take the kWh.
        //The household to take it would be the one that has to pay more to the main source,
        //but it will not be paying the price he offers, it would be the seconnd highest price.
        public void ValidateDeal()
        {
            //While there are deals left.
            if (deals.Count != 0)
            {
              

                //Getting the 1st highest value and the second in order to the first pay what the second offers.
                var firstHighestValue = deals.OrderByDescending(x => x.Value).FirstOrDefault();

                var secondHighestValue = deals.OrderByDescending(x => x.Value).ElementAtOrDefault(1);

                //Declaring to pay the second highest value.
                var FinalPay = secondHighestValue;
                //Checks if the value is smaller or equal to 0 in that case will pay they amount the first offered.
                if (secondHighestValue.Value <= 0)
                {
                    FinalPay = firstHighestValue;
                }

                Send(firstHighestValue.Key, $"buyerGetsDeal {FinalPay.Value}");
                HouseholdSetup.numberOfDeals++;

                household_Sells--;
                //Set that the buyer has now less to sell.
                numberOfenergySold++;
                seller_money_sold += FinalPay.Value;
                household_Money += FinalPay.Value;
                string dealCompleted = ($"      |----------------------------------------------------------------------|\n" +
                                        $"       |     Deal Completed -> {firstHighestValue.Key} buys 1kWh for a total of £{FinalPay.Value}.  |\n" +
                                        $"      |----------------------------------------------------------------------|\n");
    
                Console.WriteLine(dealCompleted);
                //Saving the data into the txt document.
                saveSummarize(dealCompleted);
                //Calls the manager to get the next deal by the seller.
                Send("manager", "NextDeal");
                HouseholdSetup.numberOfDeals++;
           

                NextDeal();
            }

            //When there are no more deals, make summarize
            else
            {
                Console.WriteLine("\n   |-----No remaining buyers-----| \n");

                Console.WriteLine("\n       |--Market Finished--| \n");
                //Broadcasting the case which has the data of the summarize and the movements between the peers and main source.
                BuyFromMainSource();
                SellToMainSource();

                BuyerSummarize();
                SellerSummarize();


                Broadcast("SellMainSource");
                Broadcast("BuyMainSource");
                Broadcast("BuyersSummarize");
                Broadcast("SellersSummarize");


            }

        }


        //Dutch Auction Validation Function.
        public void ValidateDutchDeal()
        {

     


            if (deals.Count != 0)
            {


               


                // Starts with the highest offer
                var highestOffer = deals.OrderByDescending(x => x.Value).FirstOrDefault();

                // Predetermined reserve price (the lowest acceptable price)
                var reservePrice = 11; // Replace this with your actual reserve price

                var finalPrice = highestOffer.Value;
                string buyerName = highestOffer.Key;

                // The auctioneer lowers the price until a buyer accepts
                while (true)
                {
                    // If there's a buyer who accepts this price
                    if (deals.Any(deal => deal.Value >= finalPrice))
                    {
                        var buyerDeal = deals.FirstOrDefault(deal => deal.Value >= finalPrice);
                        buyerName = buyerDeal.Key;
                        break;
                    }


                    // If it reaches the reserve price
                    if (finalPrice <= reservePrice)
                    {
                        break;
                    }

                    // Lower the price
                    finalPrice--;
                    //  Console.WriteLine("Bid Latest Price: " + finalPrice);
                }

                Send(buyerName, $"buyerGetsDeal {finalPrice}");
                HouseholdSetup.numberOfDeals++;

                household_Sells--;
                //Set that the buyer has now less to sell.
                numberOfenergySold++;
                seller_money_sold += finalPrice;
                household_Money += finalPrice;
                string dealCompleted = ($"      |----------------------------------------------------------------------|\n" +
                                        $"       |     Deal Completed -> {buyerName} buys 1kWh for a total of £{finalPrice}.  |\n" +
                                        $"      |----------------------------------------------------------------------|\n");

                Console.WriteLine(dealCompleted);
                //Saving the data into the txt document.
                saveSummarize(dealCompleted);
                //Calls the manager to get the next deal by the seller.
                Send("manager", "NextDeal");
                HouseholdSetup.numberOfDeals++;
              

                NextDeal();
            }
            else
            {


                Console.WriteLine("\n |----- No remaining Deals -----| \n");


                Console.WriteLine("\n       |--Market Finished--| \n");
                //Broadcasting the case which has the data of the summarize and the movements between the peers and main source.
                BuyFromMainSource();
                SellToMainSource();

                BuyerSummarize();
                SellerSummarize();


                Broadcast("SellMainSource");
                Broadcast("BuyMainSource");
                Broadcast("BuyersSummarize");
                Broadcast("SellersSummarize");


            }
        }






        //Function to set seller deal when called.
        public void sellerDeals()
        {
            //Sending the mesasge to the manager about the amount of energy the 
            //household has to sell, so it sums up a deal.
            Send("manager", $"sellerDeals {household_Sells}");

            HouseholdSetup.numberOfDeals++;
        }

      
       

        //Function to set buyer deal when called.
        public void buyer_setDeal()
        {
            // int tempEnergy = household_Sells--;
            if (listOfBuyers.Count == 0)
            {
                //sends a message to manager and gets a list of buyers 
                Send("manager", "getListOfBuyers");
                HouseholdSetup.numberOfDeals++;
            }
            else
            {


                //Prints details of the seller.
                string buyerDealtxt = ($"\n" + "          |-------------------------|\n" +
                                      $"         --- Household Publish Deal {HouseholdSetup.vendedores++} ---  \n " +
                                      $"         |-> {this.Name}           |\n" +
                                      $"          |-> Energy to sell: {household_Sells}     |\n" +
                                      $"          |-> Looking for buyer...  |\n" +
                                      $"          |-------------------------|\n");



                //Printing it to console.
                Console.WriteLine(buyerDealtxt);

                //saving data to txt document
                saveSummarize(buyerDealtxt);


                //Prints all the buyers that are saved on the list within the buyer deal called in the action case of the Manager Agent.
                foreach (string buyer in listOfBuyers)
                {
                    //Sending to the manager the deal of the household and suming up the count of number of deals.
                    Send(buyer, "buyersDeal");
                    //Each buyer on the list has a deal to offer so counts up.
                    HouseholdSetup.numberOfDeals++;



                }
            }
 
            

        }

        
        
        //Used to get next deal by the seller, setting the count to 0.
        public void NextDeal()
        {
            countBuyerDeals = 0;
            deals.Clear();
        }

        ManagerAgent manager = new ManagerAgent();
        //Sends to manager the data of which household bought an amount oof kWh from main source.
        public void BuyFromMainSource()
        {
            //if agent is a buyer still to buy energy
            if (household_Buys > 0)
            {
                string credits = $"{household_Buys * mainSourceBuyDeal}£";
                string buyfromMainSOurcetxt = $"{this.Name} bought {household_Buys}kWh from the main source for £{mainSourceBuyDeal}/kWh, Total of: {credits} \n";

                //Printing to console
                Console.WriteLine(buyfromMainSOurcetxt);
                //Writting it into the txt document.
                saveSummarize(buyfromMainSOurcetxt);

                //Substracting the money of  the household wallet depending
                //on the calculation of the energy to sell and the prize the main source pays for .
                household_Money -= (household_Buys * mainSourceBuyDeal);
                priceBoughMainSource = household_Buys;
                //Sell all energy left and set to 0 the amount of energy.
                household_Buys = 0;
                Send("manager", "maketFinal");
            
            }

        }



        //Sends to manager the data of which household sold an amount of kWh to main source.
        public void SellToMainSource()
        {
            //if agent is a seller still to sell energy
            if (household_Sells > 0)
            {

                string credits = $"{household_Sells * mainSourceSellDeal}£";
                string sellmainSourcetxt = $"{this.Name} sold {household_Sells}kWh to the main source for £{mainSourceSellDeal}/kWh, Total of: {credits} \n";

                //Printing it to the system.
                Console.WriteLine(sellmainSourcetxt);
                //Saving data into the txt document.
                saveSummarize(sellmainSourcetxt);

                //Summing the money of  the household wallet depending
                //on the calculation of the energy to sell and the prize the main source pays for .
                household_Money += (household_Sells * mainSourceSellDeal);
                priceSoldto_mainSource = household_Sells;
                //Set energy to sell to 0.
                household_Sells = 0;

                Send("manager", "maketFinal");
            }


        }


        //Sends to manager the data of which household bought an amount oof kWh from main source.
        public void BuyMain()
        {
            if (household_Buys > 0)
            {

                //Substracting the money of  the household wallet depending
                //on the calculation of the energy to sell and the prize the main source pays for .
                household_Money -= (household_Buys * mainSourceBuyDeal);
                priceBoughMainSource = household_Buys;
                //Sell all energy left and set to 0 the amount of energy.
                household_Buys = 0;

                string buyfromMainSOurcetxt = ($"{this.Name} bought all the kWh needed from the main source for £{mainSourceBuyDeal}/kWh.");
                //Printing to console
                Console.WriteLine(buyfromMainSOurcetxt);
                //Writting it into the txt document.
                saveSummarize(buyfromMainSOurcetxt);


                Send("manager", "maketFinal");
            }

        }

        //When household are in the same situation and there is not buyers.
        public void SellMain()
        {

            //if agent is a seller still to sell energy sell it to main.
            if (household_Sells > 0)
            {

                //Summing the money of  the household wallet depending
                //on the calculation of the energy to sell and the prize the main source pays for .
                household_Money += (household_Sells * mainSourceSellDeal);
                priceSoldto_mainSource = household_Sells;
                //Set energy to sell to 0.
                household_Sells = 0;

                string sellmainSourcetxt = ($"{this.Name} sold all the kWh to the main source for £{mainSourceSellDeal}/kWh.");
                //Printing it to the system
                Console.WriteLine(sellmainSourcetxt);
                //Saving data into the txt document.
                saveSummarize(sellmainSourcetxt);
                Send("manager", "maketFinal");
            }

          

        }


        //Summarize if the market for the buyer.
        public void BuyerSummarize()
        {
            if (household == "Buyer")
            {

                string nameofHouseholdBuyer = $"{this.Name}";
                int amountBought = boughtFromSeller + priceBoughMainSource;

                // string savedBuyer = $"{Math.Abs(household_Money - amountBought * mainSourceBuyDeal)} ";

                //   string winRate = $" {System.Math.Abs(household_Money) - amountBought - household_Money)}";
                string buyerSumtxt = ("\n|----------------------------------------------|\n"
                                   + $"  |        {nameofHouseholdBuyer} -> Credits: {household_Money}£              \n"
                                   + $"  |Bought: {amountBought} kWh for a total cost of £{(Math.Abs(household_Money))}\n"
                                   + $"  |Price to buy from Main Source: £{amountBought * mainSourceBuyDeal} \n"
                                   + $"  |Saved:  {Math.Abs(household_Money) - amountBought * mainSourceBuyDeal}£\n"
                                   + "|----------------------------------------------|\n");
                //Print to console
                Console.WriteLine(buyerSumtxt);
                //Save to txt document.              
                saveSummarize(buyerSumtxt);

                

            }
          
        }

        //Summarize the market records of the seller
        public void SellerSummarize()
        {
            if (household == "Seller")
            {

                int totalSold = numberOfenergySold + priceSoldto_mainSource;

                string nameofHousehold = $"{this.Name}";
                string wallet = $"{household_Money}";

                //   nameofHousehold.FirstOrDefault();

                string winRate = $" {System.Math.Abs(totalSold * mainSourceSellDeal - household_Money)}";


                string SellerSummarize = ("\n|---------------------------------------------------------------|\n" +
                                        $"  |       {nameofHousehold} ->  Credits: {wallet}£               \n" +
                                        $"  |Total kWh Sold: {totalSold}, with an income of: £{household_Money}\n" +
                                        $"  |By dealing with the Main Source would have created an income of £{totalSold * mainSourceSellDeal}\n" +
                                        $"  |Win Rate: £{winRate}\n" +
                                           "|---------------------------------------------------------------|\n");
                //Print it to console
                Console.WriteLine(SellerSummarize);
                //Saved into the txt document.
                saveSummarize(SellerSummarize);

              


            }

        }

        private void saveSummarize(string summarize)
        {
           
            try
            {
               // File.Create("marketSummarize.txt");
                string path = Directory.GetCurrentDirectory();
                string filePath = $"{path}/marketSummarize.txt"; 

                    string[] summarize2 = { summarize };

                File.AppendAllLines(filePath, summarize2);

              
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to save the file: " + ex.Message);
            }

         

        }


        //Making a menu for reload the system, quit or print the summarize of the market.
        private void menu()
        {
            int Total = (HouseholdSetup.compradores - 1) + (HouseholdSetup.vendedores - 1);
            Console.WriteLine($"Buyer Number of Deals: {HouseholdSetup.compradores-1}");
            Console.WriteLine($"Seller Number of Deals: {HouseholdSetup.vendedores - 1}");
            Console.WriteLine("Total: " + Total);

            Console.WriteLine("1. Re-run System.");
            Console.WriteLine("2. Quit.");

            // Read the user's input
            Console.Write("Please select an option: \n");
            int option = int.Parse(Console.ReadLine());
            Program program = new Program();
            // Handle the user's input using a switch statement
            switch (option)
            {

            
                //Reruns the system.
                case 1: // If the user chose option 2, reload the program
                        // Define the class where the Main() method is located

                    HouseholdSetup.compradores = 0;
                    HouseholdSetup.vendedores = 0;
                    Type mainClass = typeof(Program);


                    // Define the parameters for the Main() method (if any)
                    object[] mainArgs = { /* Insert any arguments for the Main() method here */ };

                    // Call the Main() method using the InvokeMember() method
                    mainClass.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, mainArgs);
                    break;


                case 2:
                    //Stops the app calling the function created for it.

                    program.stopApp();
                    break;

                //case 4:
                //    Console.WriteLine("1. Re-run System.");
                //    Console.WriteLine("2. Quit.");
                //    // Read the user's input
                //    Console.Write("Please select an option: \n");
                //    int option2 = int.Parse(Console.ReadLine());

                //    switch (option2)
                //    {
                //        case 1:
                //            // Define the class where the Main() method is located
                //            Type mainClass1 = typeof(Program);

                //            // Define the parameters for the Main() method (if any)
                //            object[] mainArgs1 = { /* Insert any arguments for the Main() method here */ };

                //            // Call the Main() method using the InvokeMember() method
                //            mainClass1.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, mainArgs1);

                //            break;

                //        case 2:
                //            program.stopApp();
                //            break;

                //        default:
                //            break;
                //    }
                //    break;


                default: // If the user entered an invalid option, display an error message
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

        }

    }
}