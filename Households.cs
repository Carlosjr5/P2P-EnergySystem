using ActressMas;
using Microsoft.VisualBasic;
using System.Reflection;
using static EnergySystem23.Program;
using OfficeOpenXml;
using OfficeOpenXml.Style;


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


        //Average Calculation buyer per kWh
        decimal sum;
        decimal average;
        decimal averageRound;

        //Calculating Highs & Lows Buyer
        decimal highestWinRateValueBuyer;
        decimal lowestWinRateValueBuyer;
        decimal secondLowestBuyerSaving;
        decimal thirdLowestBuyerSaving;

        //Calculating Highs & Lows Seller
        decimal highestWinRateValue;
        decimal lowestWinRateValue;
        decimal SellersecondWinRateLowerValue;
        decimal SellerthirdWinRateLowerValue;


        //Deals Prices Around.
        int countBetween11And14;
        int countOverPrized;

        DateTime earliestTime;
        DateTime latestTime;



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
                        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                       

                        HouseholdSetup.dealTimes.Add(currentTime);

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
                    SellerSummarize();
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
            if (deals.Count != 0)
            {
                var successfulDeals = new List<Deal>();

                var sortedBids = deals.OrderByDescending(x => x.Value);
                var sortedOffers = deals.OrderByDescending(x => x.Value);

                // Get the highest and second highest offers
                var highestOffer = sortedOffers.First().Value;
                var secondHighestOffer = sortedOffers.Skip(1).FirstOrDefault().Value;

                foreach (var bid in sortedBids)
                {
                    var buyerName = bid.Key;
                    var buyerBid = bid.Value;
                    var buyerAlreadyMatched = false;

                    foreach (var offer in sortedOffers)
                    {
                        var sellerName = offer.Key;
                        var sellerOffer = offer.Value;

                        if (buyerBid >= sellerOffer)
                        {
                            // Check if the buyer has enough energy to sell
                            if (household_Sells <= 0)
                                break; // Exit the inner loop if the buyer doesn't have energy to sell

                            var finalPrice = secondHighestOffer; // Use the second highest offer as the final price

                            if (buyerAlreadyMatched)
                                break; // Exit the inner loop if the buyer has already been matched

                            Send(buyerName, $"buyerGetsDeal {finalPrice}");
                            Send(sellerName, $"sellerSellsDeal {finalPrice}");

                            successfulDeals.Add(new Deal(buyerName, sellerName, finalPrice));
                            buyerAlreadyMatched = true;

                            // Decrease the available energy for the buyer
                            household_Sells--;
                        }
                    }
                }



                foreach (var deal in successfulDeals)
                {
                    HouseholdSetup.allAverageCostToBuyers.Add(deal.FinalPrice);
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


                    HouseholdSetup.allAverageCostToBuyers.Add(deal.FinalPrice);
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



            }
        }


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


            //3 Min Win Rate Seller above 0.
            if (allWinRateValues.Count > 0)
            {

                // Fisrs seller lowest win rate above 0
                var nonZeroWinRateValues = allWinRateValues.Where(value => value != 0);
                //First Lowest Win Rate Seller
                if (nonZeroWinRateValues.Any())
                {
                    lowestWinRateValue = nonZeroWinRateValues.Min();
                    //Console.WriteLine($"The lowest non-zero win rate value is: {lowestWinRateValue}");
                }
                else
                {
                 //   Console.WriteLine("All win rate values are zero.");
                }


                // Second Lowest Win Rate Seller
                var SellerAboveOneWinRateValues = allWinRateValues.Where(value => value > lowestWinRateValue);
                if (SellerAboveOneWinRateValues.Any())
                {
                    SellersecondWinRateLowerValue = SellerAboveOneWinRateValues.Min();
                    //  Console.WriteLine($"|-- Seller - The higher above 1 win rate value is: {SellersecondWinRateLowerValue}. --|");
                }
                else
                {
                 //   Console.WriteLine("All win rate values are zero.");
                }




                // Third Lowest Win Rate Seller
                var SellerthirdWinRateValues = allWinRateValues.Where(value => value > SellersecondWinRateLowerValue);
                if (SellerthirdWinRateValues.Any())
                {
                    SellerthirdWinRateLowerValue = SellerthirdWinRateValues.Min();
                    // Console.WriteLine($"|-- Seller - The higher above 1 win rate value is: {SellerthirdWinRateLowerValue}. --|");
                }
                else
                {
                  //  Console.WriteLine("All win rate values are zero.");
                }




            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }








            //Average Price per kWh paid in the market deals.
            if (HouseholdSetup.allAverageCostToBuyers.Count > 0)
            {
                sum = HouseholdSetup.allAverageCostToBuyers.Sum();
                average = sum / HouseholdSetup.allAverageCostToBuyers.Count;

                averageRound = Math.Round(average, 2);
                // Console.WriteLine($"BUYER - The average payment after the market is: {averageRound}");


                int count = HouseholdSetup.allAverageCostToBuyers.Count;
                //Console.WriteLine($"Number of elements in the list: {count}");
            }
            else
            {
                Console.WriteLine("No average cost values found.");
            }



            // Count the number of payments between £15 and £20 from Buyers
            foreach (decimal payment in HouseholdSetup.allAverageCostToBuyers)
            {
                if (payment >= 15 && payment <= 20)
                {
                    countOverPrized++;
                }
            }

            // Count the number of payments between £11 and £14
            foreach (decimal payment in HouseholdSetup.allAverageCostToBuyers)
            {
                if (payment >= 11 && payment <= 14)
                {
                    countBetween11And14++;
                }
            }





            // Call the BuyerSummarize method to get all the win rate values
            List<decimal> buyersSavesRateValues = BuyerSummarize();
            //Buyer highest Win Rate
            if (buyersSavesRateValues.Count > 0)
            {
                highestWinRateValueBuyer = buyersSavesRateValues.Max();
                //  Console.WriteLine($"|-- Buyers - The highest win rate value is: {highestWinRateValueBuyer}. --|");
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }



            //Calculating 3 lowest savings from buyer.
            if (buyersSavesRateValues.Count > 0)
            {
                // Filter out the closest values above 0.
                var nonZeroWinRateValues = buyersSavesRateValues.Where(value => value != 0);
                //First Min saving Rate Buyer
                if (nonZeroWinRateValues.Any())
                {
                    lowestWinRateValueBuyer = nonZeroWinRateValues.Min();
                    //  Console.WriteLine($"|-- Buyers - The lowest non-zero win rate value is: {lowestWinRateValueBuyer}. --|");
                }
                else
                {
                  //  Console.WriteLine("All win rate values are zero.");
                }

                // Filter out the values above 1 lowest
                var AboveOneWinRateValues = buyersSavesRateValues.Where(value => value > lowestWinRateValueBuyer);
                //Second lowest min saving rate Buyer
                if (AboveOneWinRateValues.Any())
                {
                    secondLowestBuyerSaving = AboveOneWinRateValues.Min();
                    //Console.WriteLine($"|-- Buyers - The higher above 1 win rate value is: {secondLowestBuyerSaving}. --|");
                }
                else
                {
                    //Console.WriteLine("All win rate values are zero.");
                }

                // Filter out the values above 2 lowest
                var AboveSecondWinRateValues = buyersSavesRateValues.Where(value => value > secondLowestBuyerSaving);
                //Third lowest min saving rate Buyer
                if (AboveSecondWinRateValues.Any())
                {
                    thirdLowestBuyerSaving = AboveSecondWinRateValues.Min();
                    // Console.WriteLine($"|-- Buyers - The higher above 1 win rate value is: {thirdLowestBuyerSaving}. --|");
                }
                else
                {
                   // Console.WriteLine("All win rate values are zero.");
                }



            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }





            //    Console.Clear(); 



            List<string> dealTimes = HouseholdSetup.dealTimes;
            earliestTime = DateTime.Parse(dealTimes.Min());
            latestTime = DateTime.Parse(dealTimes.Max());


            var startAuctionTime = "   |-- Started Auction at: " + earliestTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var endAuctionTime = "   |-- End Auction at: " + latestTime.ToString("yyyy-MM-dd HH:mm:ss.fff");




            int Total = HouseholdSetup.compradores + HouseholdSetup.vendedores + HouseholdSetup.dealsCompleted + HouseholdSetup.buyToMain + (HouseholdSetup.sellToMain);
            var BuyerNumberOfDeals = $"  |--  Buyer Number of Deals: {HouseholdSetup.compradores}.  ";
            var SellerNumberOfDeals = $"  |--  Seller Number of Deals: {HouseholdSetup.vendedores}.  ";
            var BuyMainNumberOfDeals = $"  |--  Buy from Main Source: {HouseholdSetup.buyToMain}.  ";
            var SellMainNumberOfDeals = $"  |--  Sell to Main Source: {HouseholdSetup.sellToMain}.  ";

            var expensivePrice = $"   |-- Number of payments between £15 and £20: {countOverPrized} Deals.\n";
            var cheapPrice = $"   |-- Number of payments between £11 and £14: {countBetween11And14} Deals.\n";

            var PricePerkWh = $"   |-- The average price Per kWh is £{averageRound}.\n";



            var summary = $"|----------------------------------------------------|\n" +
              $"|--                  Auction Ended.                --|\n" +
              $"|-------                Resume:               -------|\n" +
              $"|----------------------------------------------------|\n" +
              $"  |--  Buyer Number of Deals: {HouseholdSetup.compradores}.\n" +
              $"  |--  Seller Number of Deals: {HouseholdSetup.vendedores}.\n" +
              $"  |--  Buy from Main Source: {HouseholdSetup.buyToMain}.\n" +
              $"  |--  Sell to Main Source: {HouseholdSetup.sellToMain}.\n" +
              $"  |--  Total Deals Completed Between Households: {HouseholdSetup.dealsCompleted}.\n" +
              $"|----------------------------------------------------|\n" +
              $"  |--  Total: {Total} messages.\n" +
              $"|----------------------------------------------------|\n" +
              $"|--------------------------------------|\n" +
              $"|--         Savings Summary.         --|\n" +
              $"|--------------------------------------|\n" +
              $"   |-- The average price Per kWh is £{averageRound}.\n" +
              $"|-------------------------------------------------------|\n" +
              $"|---            Buyers.             ---|\n" +
              $"|--------------------------------------|\n" +
              $"   |--  Highest Buyer Savings: £{highestWinRateValueBuyer}.\n" +
              $"   |--  Lowest Buyer Savings: £{lowestWinRateValueBuyer}.\n" +
              $"   |--  Second Lowest Buyer Savings: £{secondLowestBuyerSaving}.\n" +
              $"   |--  Third Lowest Buyer Savings: £{thirdLowestBuyerSaving}.\n" +
              $"|--------------------------------------|\n" +
              $"|---            Sellers.            ---|\n" +
              $"|--------------------------------------|\n" +
              $"   |--  Highest Seller Profit: £{highestWinRateValue}.\n" +
              $"   |--  Lowest Seller Profit: £{lowestWinRateValue}.\n" +
              $"   |--  Second Lowest Seller Profit: £{SellersecondWinRateLowerValue}.\n" +
              $"   |--  Third Lowest Seller Profit: £{SellerthirdWinRateLowerValue}.\n" +
              $"|--------------------------------------|\n" +
              $"|-------------------------------------------------------|\n" +
              $"|---                      Prices.                    ---|\n" +
              $"|-------------------------------------------------------|\n" +
              $"{PricePerkWh}" +
              $"{expensivePrice}" +
              $"{cheapPrice}" +
              $"|-------------------------------------------------------|\n" +
              $"|-------------------------------------------------|\n" +
              $"|--              Auction Period Time.           --|\n" +
              $"|-------------------------------------------------|\n" +
              $"{startAuctionTime}\n" +
              $"{endAuctionTime}\n" +
              $"|-------------------------------------------------|";

            Console.WriteLine(summary);
            saveSummarize(summary);









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
              

                double minPrice = 11.0;


                string bidTimeFormat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");


                HouseholdSetup.dealTimes.Add(bidTimeFormat);


                string buyerDealtxt = ($"\n" + "          |-------------------------|\n" +
                                      $"        --- Household Publish Deal {HouseholdSetup.vendedores++} ---  \n" +
                                      $"          |-> {this.Name}         |\n" +
                                      $"          |-> Energy to sell: {household_Sells}     |\n" +
                                      $"          |-> Max Price: {minPrice}         |\n" +
                                      $"          |-> Min Price: {initialPrice}         |\n" +
                                      $"          |-> Time: {bidTimeFormat}        |\n" +
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
                

                HouseholdSetup.buyMainSource.Add(household_Buys);


                HouseholdSetup.buyToMain++;

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

                HouseholdSetup.sellMainSource.Add(household_Sells);


                HouseholdSetup.sellToMain++;

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





        public void SaveToCSV(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Write the column headers
                writer.WriteLine("Households, Total Number");
                writer.WriteLine("Number Of Household," + HouseholdSetup.totalNumberOfHouseholds);

                // Write the column headers
                writer.WriteLine(" , ");

                // Write the column headers
                writer.WriteLine("Auction Summarize,Messages Count");
                writer.WriteLine("Total Messages," + ((HouseholdSetup.compradores) + (HouseholdSetup.vendedores) + (HouseholdSetup.dealsCompleted) + (HouseholdSetup.sellToMain) + (HouseholdSetup.buyToMain)));
                writer.WriteLine("Buyer Number of Deals," + (HouseholdSetup.compradores));
                writer.WriteLine("Seller Number of Deals," + (HouseholdSetup.vendedores));
                writer.WriteLine("Buy From Main Source ," + (HouseholdSetup.buyToMain));
                writer.WriteLine("Sell From Main Source ," + (HouseholdSetup.sellToMain));
                writer.WriteLine("Total Deals Completed," + (HouseholdSetup.dealsCompleted));

                // Write the column headers
                writer.WriteLine(" , ");

                // Write the column headers
                writer.WriteLine("Saving Summarize,Max&Min");
                writer.WriteLine("Buyer Average Price per kWh," + averageRound);
                writer.WriteLine("Seller - Highest Win Rate," + highestWinRateValue);
                writer.WriteLine("Seller - Lowest Win Rate," + lowestWinRateValue);
                writer.WriteLine("Seller - Second Lowest Win Rate," + SellersecondWinRateLowerValue);
                writer.WriteLine("Seller - Third Lowest Win Rate," + SellerthirdWinRateLowerValue);
                writer.WriteLine("Buyer - Highest Savings," + highestWinRateValueBuyer);
                writer.WriteLine("Buyer - Lowest Savings," + lowestWinRateValueBuyer);
                writer.WriteLine("Buyer - Second Lowest Savings," + secondLowestBuyerSaving);
                writer.WriteLine("Buyer - Third Lowest Savings," + thirdLowestBuyerSaving);
                writer.WriteLine("Deals between 15-20 pounds," + countOverPrized);
                writer.WriteLine("Deals between 11-14 pounds," + countBetween11And14);

                // Write the column headers
                writer.WriteLine("Day, Time");
                writer.WriteLine(earliestTime.ToString("dd/MM/yyyy") + "," + earliestTime.ToString("HH:mm:ss"));
                writer.WriteLine(latestTime.ToString("dd/MM/yyyy") + "," + latestTime.ToString("HH:mm:ss"));
            }
        }



        public void SaveToExcelDouble(string filePath)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            FileInfo file = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(file))
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets["Data"];

                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("Data");
                }


                // Write the data to the specific cells
                worksheet.Cells["A2:B27"].Clear();
                worksheet.Cells["A2"].Value = "Households";
                worksheet.Cells["B2"].Value = "Total Number";
                worksheet.Cells["A3"].Value = "Number Of Household";
                worksheet.Cells["B3"].Value = HouseholdSetup.totalNumberOfHouseholds;

                worksheet.Cells["A5"].Value = "Auction Summarize";
                worksheet.Cells["B5"].Value = "Messages Count";
                worksheet.Cells["A6"].Value = "Total Messages";
                worksheet.Cells["B6"].Value = (HouseholdSetup.compradores + HouseholdSetup.vendedores + HouseholdSetup.dealsCompleted + HouseholdSetup.sellToMain + HouseholdSetup.buyToMain);
                worksheet.Cells["A7"].Value = "Buyer Number of Deals";
                worksheet.Cells["B7"].Value = HouseholdSetup.compradores;
                worksheet.Cells["A8"].Value = "Seller Number of Deals";
                worksheet.Cells["B8"].Value = HouseholdSetup.vendedores;
                worksheet.Cells["A9"].Value = "Buy From Main Source";
                worksheet.Cells["B9"].Value = HouseholdSetup.buyToMain;
                worksheet.Cells["A10"].Value = "Sell From Main Source";
                worksheet.Cells["B10"].Value = HouseholdSetup.sellToMain;
                worksheet.Cells["A11"].Value = "Total Deals Completed";
                worksheet.Cells["B11"].Value = HouseholdSetup.dealsCompleted;

                worksheet.Cells["A13"].Value = "Saving Summarize";
                worksheet.Cells["B13"].Value = "Max&Min";
                worksheet.Cells["A14"].Value = "Buyer Average Price per kWh";
                worksheet.Cells["B14"].Value = averageRound;
                worksheet.Cells["A15"].Value = "Seller - Highest Win Rate";
                worksheet.Cells["B15"].Value = highestWinRateValue;
                worksheet.Cells["A16"].Value = "Seller - Lowest Win Rate";
                worksheet.Cells["B16"].Value = lowestWinRateValue;
                worksheet.Cells["A17"].Value = "Seller - Second Lowest Win Rate";
                worksheet.Cells["B17"].Value = SellersecondWinRateLowerValue;
                worksheet.Cells["A18"].Value = "Seller - Third Lowest Win Rate";
                worksheet.Cells["B18"].Value = SellerthirdWinRateLowerValue;
                worksheet.Cells["A19"].Value = "Buyer - Highest Savings";
                worksheet.Cells["B19"].Value = highestWinRateValueBuyer;
                worksheet.Cells["A20"].Value = "Buyer - Lowest Savings";
                worksheet.Cells["B20"].Value = lowestWinRateValueBuyer;
                worksheet.Cells["A21"].Value = "Buyer - Second Lowest Savings";
                worksheet.Cells["B21"].Value = secondLowestBuyerSaving;
                worksheet.Cells["A22"].Value = "Buyer - Third Lowest Savings";
                worksheet.Cells["B22"].Value = thirdLowestBuyerSaving;
                worksheet.Cells["A23"].Value = "Deals between 15-20 pounds";
                worksheet.Cells["B23"].Value = countOverPrized;
                worksheet.Cells["A24"].Value = "Deals between 11-14 pounds";
                worksheet.Cells["B24"].Value = countBetween11And14;

                worksheet.Cells["A25"].Value = "Day";
                worksheet.Cells["B25"].Value = "Time";
                worksheet.Cells["A26"].Value = earliestTime.ToString("dd/MM/yyyy");
                worksheet.Cells["B26"].Value = earliestTime.ToString("HH:mm:ss");
                worksheet.Cells["A27"].Value = latestTime.ToString("dd/MM/yyyy");
                worksheet.Cells["B27"].Value = latestTime.ToString("HH:mm:ss");

                // Save the changes
                package.Save();

            }
        }



        public void SaveToExcelSecond(string filePath)
      {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            FileInfo file = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(file))
        {

                ExcelWorksheet worksheet = package.Workbook.Worksheets["Data"];

                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("Data");
                }


                // Write the data to the specific cells
                worksheet.Cells["F2:G27"].Clear();



                worksheet.Cells["F2"].Value = "Households";
                worksheet.Cells["G2"].Value = "Total Number";
                worksheet.Cells["F3"].Value = "Number Of Household";
                worksheet.Cells["G3"].Value = HouseholdSetup.totalNumberOfHouseholds;

                worksheet.Cells["F5"].Value = "Auction Summarize";
                worksheet.Cells["G5"].Value = "Messages Count";
                worksheet.Cells["F6"].Value = "Total Messages";
                worksheet.Cells["G6"].Value = (HouseholdSetup.compradores + HouseholdSetup.vendedores + HouseholdSetup.dealsCompleted + HouseholdSetup.sellToMain + HouseholdSetup.buyToMain);
                worksheet.Cells["F7"].Value = "Buyer Number of Deals";
                worksheet.Cells["G7"].Value = HouseholdSetup.compradores;
                worksheet.Cells["F8"].Value = "Seller Number of Deals";
                worksheet.Cells["G8"].Value = HouseholdSetup.vendedores;
                worksheet.Cells["F9"].Value = "Buy From Main Source";
                worksheet.Cells["G9"].Value = HouseholdSetup.buyToMain;
                worksheet.Cells["F10"].Value = "Sell From Main Source";
                worksheet.Cells["G10"].Value = HouseholdSetup.sellToMain;
                worksheet.Cells["F11"].Value = "Total Deals Completed";
                worksheet.Cells["G11"].Value = HouseholdSetup.dealsCompleted;

                worksheet.Cells["F13"].Value = "Saving Summarize";
                worksheet.Cells["G13"].Value = "Max&Min";
                worksheet.Cells["F14"].Value = "Buyer Average Price per kWh";
                worksheet.Cells["G14"].Value = averageRound;
                worksheet.Cells["F15"].Value = "Seller - Highest Win Rate";
                worksheet.Cells["G15"].Value = highestWinRateValue;
                worksheet.Cells["F16"].Value = "Seller - Lowest Win Rate";
                worksheet.Cells["G16"].Value = lowestWinRateValue;
                worksheet.Cells["F17"].Value = "Seller - Second Lowest Win Rate";
                worksheet.Cells["G17"].Value = SellersecondWinRateLowerValue;
                worksheet.Cells["F18"].Value = "Seller - Third Lowest Win Rate";
                worksheet.Cells["G18"].Value = SellerthirdWinRateLowerValue;
                worksheet.Cells["F19"].Value = "Buyer - Highest Savings";
                worksheet.Cells["G19"].Value = highestWinRateValueBuyer;
                worksheet.Cells["F20"].Value = "Buyer - Lowest Savings";
                worksheet.Cells["G20"].Value = lowestWinRateValueBuyer;
                worksheet.Cells["F21"].Value = "Buyer - Second Lowest Savings";
                worksheet.Cells["G21"].Value = secondLowestBuyerSaving;
                worksheet.Cells["F22"].Value = "Buyer - Third Lowest Savings";
                worksheet.Cells["G22"].Value = thirdLowestBuyerSaving;
                worksheet.Cells["F23"].Value = "Deals between 15-20 pounds";
                worksheet.Cells["G23"].Value = countOverPrized;
                worksheet.Cells["F24"].Value = "Deals between 11-14 pounds";
                worksheet.Cells["G24"].Value = countBetween11And14;

                worksheet.Cells["F25"].Value = "Day";
                worksheet.Cells["G25"].Value = "Time";
                worksheet.Cells["F26"].Value = earliestTime.ToString("dd/MM/yyyy");
                worksheet.Cells["G26"].Value = earliestTime.ToString("HH:mm:ss");
                worksheet.Cells["F27"].Value = latestTime.ToString("dd/MM/yyyy");
                worksheet.Cells["G27"].Value = latestTime.ToString("HH:mm:ss");


                // Save the changes
                package.Save();
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
                decimal averageCostToBuyers = amountBought != 0 ? totalCostFromHouseholds / amountBought : 0.0m;

               
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

                string formattedAverageProfit = averageProfit.ToString("0.00");

                string nameofHousehold = $"{this.Name}";
                string wallet = $"{household_Money}";

                int dinero = household_Money;
                decimal winRateValue = Math.Abs(totalSold * mainSourceSellDeal - household_Money);

              
                HouseholdSetup.winRateValues.Add(winRateValue);

                HouseholdSetup.allAverageCostToSellers.Add(averageProfit);

                string sellerSummarize = $"\n|---------------------------------------------------------------|\n" +
                                         $"  |       {nameofHousehold} ->  Credits: {wallet}£               \n" +
                                         $"  |Total kWh Sold: {totalSold}, with an income of: £{household_Money}\n" +
                                         $"  |By dealing with the Main Source would have created an income of £{totalSold * mainSourceSellDeal}\n" +
                                         $"  |Average Profit: {formattedAverageProfit}£ per unit sold\n" +
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
            //Highest win Rate Seller
            if (allWinRateValues.Count > 0)
            {
                highestWinRateValue = allWinRateValues.Max();
                Console.WriteLine($"|-- Sellers - The highest win rate value is: {highestWinRateValue} . --|");
            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }


            //Seller Lowest Above 0.
            if (allWinRateValues.Count > 0)
            {
                // Filter out the values equal to 0
                var nonZeroWinRateValues = allWinRateValues.Where(value => value != 0);

                if (nonZeroWinRateValues.Any())
                {
                    lowestWinRateValue = nonZeroWinRateValues.Min();
                   // Console.WriteLine($"|-- Sellers - The lowest non-zero win rate value is: {lowestWinRateValue} . --|");
                }
                else
                {
                    // Console.WriteLine("All win rate values are zero.");
                }



                //Second Lowest Win Rate
                var AboveOneWinRateValues = allWinRateValues.Where(value => value > lowestWinRateValue);
                if (AboveOneWinRateValues.Any())
                {
                    SellersecondWinRateLowerValue = AboveOneWinRateValues.Min();
                   // Console.WriteLine($"|-- Seller - The higher above 1 win rate value is: {SellersecondWinRateLowerValue}. --|");
                }
                else
                {
                  //  Console.WriteLine("All win rate values are zero.");
                }


                //Second Lowest Win Rate
                // Filter out the values above 1
                var ThirdsLowestWinRateValues = allWinRateValues.Where(value => value > SellersecondWinRateLowerValue);
                if (ThirdsLowestWinRateValues.Any())
                {
                    SellerthirdWinRateLowerValue = ThirdsLowestWinRateValues.Min();
                     Console.WriteLine($"|-- Seller - The higher above 1 win rate value is: {SellerthirdWinRateLowerValue}. --|");
                }
                else
                {
                  //  Console.WriteLine("All win rate values are zero.");
                }



            }
            else
            {
                Console.WriteLine("No win rate values found.");
            }






            //Average Price per kWh
            if (HouseholdSetup.allAverageCostToBuyers.Count > 0)
            {
                 sum = HouseholdSetup.allAverageCostToBuyers.Sum();
                 average = sum / HouseholdSetup.allAverageCostToBuyers.Count;
               //  Console.WriteLine($"The average payment after the market is: {average}");
            }
            else
            {
                Console.WriteLine("No average cost values found.");
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


                string EXCELfilePathSecond = $"{path}/Double&SecondStudiesf.xlsx";


                if (HouseholdSetup.protocol == true)
                {
                    string CSVfilePathDouble = $"{path}/marketSummarizeDouble.csv";
                    
                    SaveToCSV(CSVfilePathDouble);

                    string filePath = $"{path}/marketSummarizeDoubleAuction.txt";

                    SaveToExcelDouble(EXCELfilePathSecond);

                    string[] summarize2 = { summarize };

                    File.AppendAllLines(filePath, summarize2);
                }
                if (HouseholdSetup.protocol == false)
                {
                    string CSVfilePathSecond = $"{path}/marketSummarizeSecond.csv";
                   
                    SaveToCSV(CSVfilePathSecond);

                    SaveToExcelSecond(EXCELfilePathSecond);

                    string filePath = $"{path}/marketSummarizeSecondBidAuction.txt";

                    string[] summarize2 = { summarize };

                    File.AppendAllLines(filePath, summarize2);
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to save the file: " + ex.Message);
            }



        }


        //Making a menu for reload the system, quit or print the summarize of the market.
        public void menu()
        {

         
            DisplayResumeAndTotalMessages();
          

            int option = 0;

            while (option != 1 && option != 2)
            {
                Console.WriteLine("1. Re-run System.");
                Console.WriteLine("2. Quit.");

                // Read the user's input
                Console.Write("Please select an option: ");
                if (!int.TryParse(Console.ReadLine(), out option))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option (1 or 2).");
                }
            }

            // At this point, 'option' will contain a valid input value (1, 2, or 3)
            Console.WriteLine("Selected option: " + option);

            Program program = new Program();

            // Handle the user's input using a switch statement
            switch (option)
            {
                case 1:


                    //restart Highs & Lows Buyer
                     highestWinRateValueBuyer=0;
                     lowestWinRateValueBuyer=0;
                     secondLowestBuyerSaving=0;
                     thirdLowestBuyerSaving = 0;

                    //restart Highs & Lows Seller
                     highestWinRateValue = 0;
                     lowestWinRateValue = 0;
                     SellersecondWinRateLowerValue = 0;
                     SellerthirdWinRateLowerValue = 0;
                    HouseholdSetup.totalNumberOfHouseholds = 0;
                    HouseholdSetup.numberOfDeals = 0;
                    HouseholdSetup.numberOfAnnouncements = 0;
                    HouseholdSetup.protocol = false;
                    HouseholdSetup.compradores = 0;
                    HouseholdSetup.vendedores = 0;
                    HouseholdSetup.buyToMain = 0;
                    HouseholdSetup.sellToMain = 0;
                    HouseholdSetup.dealsCompleted = 0;
                    HouseholdSetup.sprotocol = false;
                    HouseholdSetup.household_Money = 0;
                    HouseholdSetup.startTime = default;

                    HouseholdSetup.closestTime = "";
                    HouseholdSetup.latestTime = "";
                    HouseholdSetup.dealTimes = new List<string>();

                    HouseholdSetup.winRateValues = new List<decimal>();
                    HouseholdSetup.buyersSavesRateValues = new List<decimal>();
                    HouseholdSetup.allAverageCostToBuyers = new List<decimal>();
                    HouseholdSetup.allAverageCostToSellers = new List<decimal>();

                    HouseholdSetup.buyMainSource = new List<decimal>();
                    HouseholdSetup.buyMainSourcePrice = new List<decimal>();
                    HouseholdSetup.sellMainSource = new List<decimal>();

                    HouseholdSetup.numberOfHouseholds = 0;

                    HouseholdSetup.averageCostToBuyers = 0;


                  //  countOverPrized = 0;
                    //countBetween11And14 = 0;
                    

                    Type mainClass = typeof(Program);
                    object[] mainArgs = { };

                    // Call the Main() method using the InvokeMember() method
                    mainClass.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, mainArgs);
                    break;

                case 2:
                    program.stopApp();
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

        }


    }
}