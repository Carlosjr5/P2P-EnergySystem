using ActressMas;
using System.Diagnostics;
using System.Reflection;
using static EnergySystem23.Program;
using System.Threading;
using System;
using Google.Api.Ads.AdWords.v201809;
using Microsoft.VisualBasic;


namespace EnergySystem23
{

    //Class running on top of the AcctressMas as an Agent using its environment.
    class Households : Agent
    {
        private Dictionary<string, DateTime> buyerBids;


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
        public Dictionary<string, int> deals;
        //Counting the messages of deals recieved by the buyers and messages of sellers.
        private int countBuyerDeals = 0;

       


        private Timer timer;


        List<int> BuyerDealsList = new List<int>();

        private decimal storedHighestWinRate;
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
            buyerBids = new Dictionary<string, DateTime>();

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
                        string currentTime = DateTime.Now.ToString("HH:mm:ss");
                        //Saves the deals of the buyer(household) withing the amount able to pay and total energy left needed to buy.
                        string buyerDealsData = ($"\n" + "          |----------------------------------|\n" +
                                              $"                --- Buyers Deals {HouseholdSetup.compradores++} ---  \n" +
                                              $"          |-> {this.Name}, at '{currentTime}'   |\n" +
                                              $"          |-> Offers £{mainSourceBuyDeal - 1}/kWh                 |\n" +
                                              $"          |-> Total energy needed: {household_Buys}kWh.     |\n" +
                                              $"          |----------------------------------|\n");




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
                            //Double Auction Validation.
                            ValidateDoubleAuctionDeal();
                            break;
                        }
                        if (HouseholdSetup.protocol == false)
                        {
                            //Dutch Auction Validation.
                            ValidateDeal();
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
                            //Double Auction.
                            ValidateDoubleAuctionDeal();
                            break;
                        }

                        if (HouseholdSetup.protocol == false)
                        {
                            //Second-Bid Auction
                            ValidateDeal();
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
                                        $"       |     Deal {HouseholdSetup.dealsCompleted++} Completed -> {firstHighestValue.Key} buys 1kWh for a total of £{FinalPay.Value}.  |\n" +
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

                Broadcast("SellMainSource");
                Broadcast("BuyMainSource");
                Broadcast("BuyersSummarize");
                Broadcast("SellersSummarize");

                DisplayResumeAndTotalMessages();

            }

        }



        // Double Auction Validation Functionality
        public void ValidateDoubleAuctionDeal()
        {
            if (deals.Count >= 1)
            {
                var successfulDeals = new List<Deal>();

                
                var reservePrice = 11m; // Use decimal literal by appending 'm' to the value

                var sortedBids = deals.OrderByDescending(x => x.Value);
                var sortedOffers = deals.OrderBy(x => x.Value);

                foreach (var bid in sortedBids)
                {
                    var buyerName = bid.Key;
                    var buyerBid = bid.Value;
                    var buyerAlreadyMatched = false;

                    foreach (var offer in sortedOffers)
                    {
                        var sellerName = offer.Key;
                        var sellerOffer = offer.Value;

                        if (buyerBid >= sellerOffer && buyerBid >= reservePrice && sellerOffer >= reservePrice)
                        {
                            // Check if the buyer has enough energy to sell
                            if (household_Sells <= 0)
                                break; // Exit the inner loop if the buyer doesn't have energy to sell

                            // Average price of the highest bid and lowest offer
                            var finalPrice = (int)((buyerBid + sellerOffer) / 2m);

                            if (buyerAlreadyMatched)
                                break; // Exit the inner loop if the buyer has already been matched

                            Send(buyerName, $"buyerGetsDeal {finalPrice}");
                            Send(sellerName, $"sellerSellsDeal {finalPrice}");

                            successfulDeals.Add(new Deal(buyerName, sellerName, finalPrice));
                            buyerAlreadyMatched = true; // Mark the buyer as already matched

                            // Decrease the available energy for the buyer
                            household_Sells--;
                        }

                    }
                }

                foreach (var deal in successfulDeals)
                {
                    // Update relevant variables and statistics
                    numberOfenergySold++;
                    household_Money += (int)deal.FinalPrice;
                    HouseholdSetup.numberOfDeals++;


                    string dealCompleted = ($"      |--------------------------------------------------------------------------------------------|\n" +
                                            $"              ->  Deal {HouseholdSetup.dealsCompleted++} Completed:\n" +
                                            $"              => {deal.BuyerName} buys 1kWh for a total of £{deal.FinalPrice}.  \n" +
                                            $"      |--------------------------------------------------------------------------------------------|\n");

                    Console.WriteLine(dealCompleted);
                    saveSummarize(dealCompleted);


                }

                Send("manager", "NextDeal");
                NextDeal();
            }
            else
            {
                Broadcast("SellMainSource");
                Broadcast("BuyMainSource");
                Broadcast("BuyersSummarize");
                Broadcast("SellersSummarize");

                DisplayResumeAndTotalMessages();
                          
            }
        }


        decimal highestWinRateValue;
        decimal lowestWinRateValue;
        decimal highestWinRateValueBuyer;
        decimal lowestWinRateValueBuyer;
        public void DisplayResumeAndTotalMessages()
        {

           

            // Call the SellerSummarize method to get all the win rate values
            List<decimal> allWinRateValues = SellerSummarize();

            //Max Win Rate Seller.
            if (allWinRateValues.Count > 0)
            {
                highestWinRateValue = allWinRateValues.Max();
               // Console.WriteLine($"The highest win rate value is: {highestWinRateValue}");

            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


            //Min Win Rate Seller.
            if (allWinRateValues.Count > 0)
            {
                // Filter out the values equal to 0
                var nonZeroWinRateValues = allWinRateValues.Where(value => value != 0);

                if (nonZeroWinRateValues.Any())
                {
                    lowestWinRateValue = nonZeroWinRateValues.Min();
                    //Console.WriteLine($"The lowest non-zero win rate value is: {lowestWinRateValue}");
                }
                else
                {
                    Console.WriteLine("All win rate values are zero.");
                }
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }





            // Call the BuyerSummarize method to get all the win rate values
            List<decimal> buyersSavesRateValues = BuyerSummarize();

            if (buyersSavesRateValues.Count > 0)
            {
                highestWinRateValueBuyer = buyersSavesRateValues.Max();
              //  Console.WriteLine($"|-- Buyers - The highest win rate value is: {highestWinRateValueBuyer}. --|");
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


            if (buyersSavesRateValues.Count > 0)
            {
                // Filter out the values equal to 0
                var nonZeroWinRateValues = buyersSavesRateValues.Where(value => value != 0);

                if (nonZeroWinRateValues.Any())
                {
                    lowestWinRateValueBuyer = nonZeroWinRateValues.Min();
                   // Console.WriteLine($"|-- Buyers - The lowest non-zero win rate value is: {lowestWinRateValueBuyer}. --|");
                }
                else
                {
                    Console.WriteLine("All win rate values are zero.");
                }
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }

       


          //  Console.Clear(); // Clear the console before displaying the resume and total messages

                    Console.WriteLine("|----------------------------------|");
                    Console.WriteLine("|--         Auction Ended.       --|");
                    Console.WriteLine("|-------       Resume:      -------| ");
                    Console.WriteLine("|----------------------------------| ");

                    int Total = (HouseholdSetup.compradores - 1) + (HouseholdSetup.vendedores - 1) + (HouseholdSetup.dealsCompleted - 1);
                    var BuyerNumberOfDeals = $"  |--  Buyer Number of Deals: {HouseholdSetup.compradores - 1}.  ";
                    var SellerNumberOfDeals = $"  |--  Seller Number of Deals: {HouseholdSetup.vendedores - 1}.  ";
                    var TotalDealsCompleted = $"  |--  Total Deals Completed: {HouseholdSetup.dealsCompleted - 1}. "; 
                    var TotalMessages = "|----------------------------------|" +
                                        "\n  |--  Total: " + Total + " messages.    \n" +
                                        "|----------------------------------|\n";

                    var savingSummary = "|--------------------------------------|\n"
                                      + "|--         Savings Summary.         --| \n"
                                      + "|--------------------------------------|";
                      var LowerHigher = "|-----------               ------------|\n" +
                                        "|--------------------------------------|\n"
                                      + "|---            Buyers.             ---| \n"
                                      + "|--------------------------------------|\n" +
                                     "   |--  Highest Buyer Savings: £" + highestWinRateValueBuyer +   ".   \n" +
                                     "   |--  Lowest Buyer Savings: £" + lowestWinRateValueBuyer +  ".   \n" +
                                        "|--------------------------------------|\n"
                                      + "|---            Sellers.            ---| \n"
                                      + "|--------------------------------------|\n" +
                                     "   |--  Highest Seller Savings: £" + highestWinRateValue + ".    \n" +
                                     "   |--  Lowest Seller Savings: £"  + lowestWinRateValue + ".   \n" +
                                        "|--------------------------------------|\n";


             
                



                    Console.WriteLine(BuyerNumberOfDeals);
                    Console.WriteLine(SellerNumberOfDeals);
                    Console.WriteLine(TotalDealsCompleted);
                    Console.WriteLine(TotalMessages);
                    Console.WriteLine(savingSummary);
                    Console.WriteLine(LowerHigher);


                    saveSummarize(BuyerNumberOfDeals);
                    saveSummarize(SellerNumberOfDeals);
                    saveSummarize(TotalDealsCompleted);
                    saveSummarize(TotalMessages);

                    saveSummarize(savingSummary);
                    saveSummarize(LowerHigher);




        }













        //Function to set seller deal when called.
        public void sellerDeals()
        {
            //Sending the mesasge to the manager about the amount of energy the 
            //household has to sell, so it sums up a deal.
            Send("manager", $"sellerDeals {household_Sells}");

            HouseholdSetup.numberOfDeals++;
        }

        private int minPrice;
        private int maxPrice;
        private int currentPrice;



        // private DateTime startTime = DateTime.Now; // Store the start time


      


        public void buyer_setDeal()
        {

            if (listOfBuyers.Count == 0)
            {
                // Sends a message to the manager and gets a list of buyers
                Send("manager", "getListOfBuyers");
                HouseholdSetup.numberOfDeals++;
            }
            else
            {
                double initialPrice = 21.0; // Replace this with your desired initial price
                string currentTime = DateTime.Now.ToString("HH:mm:ss");

                double minPrice = 11.0;

               




                string buyerDealtxt = ($"\n" + "          |-------------------------|\n" +
                                      $"        --- Household Publish Deal {HouseholdSetup.vendedores++} ---  \n" +
                                      $"          |-> {this.Name}         |\n" +
                                      $"          |-> Energy to sell: {household_Sells}     |\n" +
                                      $"          |-> Max Price: {minPrice}         |\n" +
                                      $"          |-> Min Price: {initialPrice}         |\n" +
                                      $"          |-> Time: {currentTime}        |\n" +
                                      $"          |-> Looking for buyer...  |\n" +
                                      $"          |-------------------------|\n");

                // Printing it to console
                Console.WriteLine(buyerDealtxt);

                // Saving data to txt document
                saveSummarize(buyerDealtxt);

                // Print all the buyers that are saved on the list within the buyer deal called in the action case of the Manager Agent
                foreach (string buyer in listOfBuyers)
                {
                    // Sending to the manager the deal of the household and summing up the count of number of deals
                    Send(buyer, "buyersDeal");
                    // Each buyer on the list has a deal to offer, so increment the number of deals
                    HouseholdSetup.numberOfDeals++;

                    // Record the time of the buyer's bid
                    buyerBids[buyer] = DateTime.Now;
                }
            }
        }






        private int CalculateCurrentPrice(int initialPrice)
        {
            // Implement your logic here to update the price based on time or any other criteria
            // For example, you can decrease the price over time or based on market conditions

            // Update the minPrice and maxPrice variables based on the initial price
            minPrice = Math.Min(minPrice, initialPrice);
            maxPrice = Math.Max(maxPrice, initialPrice);

            // Placeholder logic: Reduce the price by 1 for every 10 deals completed
            int numberOfDealsCompleted = HouseholdSetup.dealsCompleted;
            int priceReductionFactor = numberOfDealsCompleted / 10; // Adjust the factor based on your desired rate of price reduction

            int currentPrice = initialPrice - priceReductionFactor;

            // Make sure the current price does not go below a certain threshold
            int minimumPrice = 10; // Set your desired minimum price
            currentPrice = Math.Max(currentPrice, minimumPrice);

            return currentPrice;
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
        public List<decimal> BuyerSummarize()
        {
            if (household == "Buyer")
            {
                string nameofHouseholdBuyer = $"{this.Name}";
                int amountBought = boughtFromSeller + priceBoughMainSource;

                decimal totalCostFromMainSource = amountBought * mainSourceBuyDeal;
                decimal totalCostFromHouseholds = Math.Abs(household_Money);
                decimal savings = totalCostFromMainSource - totalCostFromHouseholds;
                decimal averageCostToBuyers = amountBought != 0 ? totalCostFromMainSource / amountBought : 0.0m;

                // Add winRateValue to the list
                HouseholdSetup.buyersSavesRateValues.Add(savings);

                // Round the average cost to two decimal places
                averageCostToBuyers = Math.Round(averageCostToBuyers, 2);

                string buyerSumtxt = $"\n|----------------------------------------------|\n" +
                                     $"  |        {nameofHouseholdBuyer} -> Credits: {household_Money}£              \n" +
                                     $"  |Bought: {amountBought} kWh for a total cost of £{totalCostFromHouseholds}\n" +
                                     $"  |Price to buy from Main Source: £{totalCostFromMainSource} \n" +
                                     $"  |Saved:  {savings}£\n" +
                                     $"  |Average Cost to Buyers: {averageCostToBuyers}£ per unit\n" +
                                     $"|----------------------------------------------|\n";

                Console.WriteLine(buyerSumtxt);
                saveSummarize(buyerSumtxt);
            }
            return HouseholdSetup.buyersSavesRateValues;
        }



       

        public List<decimal> SellerSummarize()
        {
            if (household == "Seller")
            {
                int totalSold = numberOfenergySold + priceSoldto_mainSource;

                decimal averageProfit = totalSold != 0 ? household_Money / (decimal)totalSold : 0.0m;

                string nameofHousehold = $"{this.Name}";
                string wallet = $"{household_Money}";

                decimal winRateValue = Math.Abs(totalSold * mainSourceSellDeal - household_Money);

                // Add winRateValue to the list
                HouseholdSetup.winRateValues.Add(winRateValue);

                string sellerSummarize = $"\n|---------------------------------------------------------------|\n" +
                                         $"  |       {nameofHousehold} ->  Credits: {wallet}£               \n" +
                                         $"  |Total kWh Sold: {totalSold}, with an income of: £{household_Money}\n" +
                                         $"  |By dealing with the Main Source would have created an income of £{totalSold * mainSourceSellDeal}\n" +
                                         $"  |Average Profit: {averageProfit}£ per unit sold\n" +
                                         $"  |Win Rate: £{winRateValue}\n" +
                                         $"|---------------------------------------------------------------|\n";

                // Print it to the console
                Console.WriteLine(sellerSummarize);
                // Save it into the text document.
                saveSummarize(sellerSummarize);
            }



            return HouseholdSetup.winRateValues;


        }

        public void CalculateHighestWinRateValue()
        {
            // Call the SellerSummarize method to get all the win rate values
            List<decimal> allWinRateValues = SellerSummarize();

            if (allWinRateValues.Count > 0)
            {
                decimal highestWinRateValue = allWinRateValues.Max();
                Console.WriteLine($"|-- Sellers - The highest win rate value is: {highestWinRateValue} . --|");
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


            if (allWinRateValues.Count > 0)
            {
                // Filter out the values equal to 0
                var nonZeroWinRateValues = allWinRateValues.Where(value => value != 0);

                if (nonZeroWinRateValues.Any())
                {
                    decimal lowestWinRateValue = nonZeroWinRateValues.Min();
                    Console.WriteLine($"|-- Sellers - The lowest non-zero win rate value is: {lowestWinRateValue} . --|");
                }
                else
                {
                    Console.WriteLine("All win rate values are zero.");
                }
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


        }


        public void CalculateHighestWinRateValueBuyers()
        {
            // Call the SellerSummarize method to get all the win rate values
            List<decimal> buyersSavesRateValues = BuyerSummarize();

            if (buyersSavesRateValues.Count > 0)
            {
                decimal highestWinRateValue = buyersSavesRateValues.Max();
                Console.WriteLine($"|-- Buyers - The highest win rate value is: {highestWinRateValue}. --|");
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


            if (buyersSavesRateValues.Count > 0)
            {
                // Filter out the values equal to 0
                var nonZeroWinRateValues = buyersSavesRateValues.Where(value => value != 0);

                if (nonZeroWinRateValues.Any())
                {
                    decimal lowestWinRateValue = nonZeroWinRateValues.Min();
                    Console.WriteLine($"|-- Buyers - The lowest non-zero win rate value is: {lowestWinRateValue}. --|");
                }
                else
                {
                    Console.WriteLine("All win rate values are zero.");
                }
            }
            else
            {
                Console.WriteLine("No win rate values found.");
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

            int option = 0;

            while (option != 1 && option != 2 && option != 3)
            {
                Console.WriteLine("1. Re-run System.");
                Console.WriteLine("2. Display Number of Messages & Savings Summary.");
                Console.WriteLine("3. Quit.");

                // Read the user's input
                Console.Write("Please select an option: ");
                if (!int.TryParse(Console.ReadLine(), out option))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option (1, 2, or 3).");
                }
            }

            // At this point, 'option' will contain a valid input value (1, 2, or 3)
            Console.WriteLine("Selected option: " + option);

            Program program = new Program();

            // Handle the user's input using a switch statement
            switch (option)
            {

                case 1:
                    HouseholdSetup.compradores = 0;
                    HouseholdSetup.vendedores = 0;
                    HouseholdSetup.dealsCompleted = 0;
                    HouseholdSetup.numberOfHouseholds = 0;
                    Type mainClass = typeof(Program);

                    object[] mainArgs = { };

                    // Call the Main() method using the InvokeMember() method
                    mainClass.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, mainArgs);
                    break;


                case 2:
                    DisplayResumeAndTotalMessages();
                    break;

              

     

                case 3:
                    program.stopApp();
                  
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }


    }
}