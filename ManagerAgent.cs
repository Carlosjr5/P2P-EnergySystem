using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using ActressMas;
using static EnergySystem23.Program;

namespace EnergySystem23
{
    class ManagerAgent : Agent
    {
        //Declaring the list with the buyers and sellers data and deals.
        private List<string> listOfSellers;
        private List<string> listOfBuyers;
        private Dictionary<string, int> deals;
        //Counting the messages received from household.
        private int sellerMessagesReceived = 0;
        private int marketFinished;
        

        public string MarketSummarizePath;

        //Setup of the agent declaring the varibles needed.
        public override void Setup()
        {
            listOfSellers = new List<string>();
            listOfBuyers = new List<string>();
            deals = new Dictionary<string, int>();
 
        }

        //Agent action deoending on the case requested.
        public override void Act(Message message)
        {

            message.Parse(out string action, out List<String> parameters);
            switch (action)
            {

                //Received when a seller tells the manager their status
                case "Seller":
                    listOfSellers.Add(message.Sender);
                
                    if (listOfSellers.Count + listOfBuyers.Count == HouseholdSetup.totalNumberOfHouseholds)
                    {
                        SellerDeals();
                   
                      
                    }
                    break;

                //Received when a seller tells the manager their status
                case "Buyer":
                    listOfBuyers.Add(message.Sender);
                    if (listOfSellers.Count + listOfBuyers.Count == HouseholdSetup.totalNumberOfHouseholds)
                    {
                        SellerDeals();
                       
                    }
                    break;

                //Received when a seller submits a proposal
                case "sellerDeals":
                    sellerMessagesReceived++;
                    deals.Add(message.Sender, Int32.Parse(parameters[0]));
                    if (sellerMessagesReceived == listOfSellers.Count)
                    {
                        EvaluateDeals();
                   
                    }
                    break;

                //Received when a seller submits a rejection
                case "seller_decline":
                    sellerMessagesReceived++;
                    if (sellerMessagesReceived == listOfSellers.Count)
                    {
                        EvaluateDeals();
                    }
                    break;

                //Received when seller requests the BuyerList
                case "getListOfBuyers":
                    string buyerliststring = "";
                    foreach (string buyer in listOfBuyers)
                    {
                        buyerliststring += (" " + buyer);

                    }
                    //Sending th list of buyers to the agent.
                    Send(message.Sender, "listOfBuyers" + buyerliststring);
                    HouseholdSetup.numberOfDeals++;
                    break;

                //Received once an auction has ended and next seller is to be chosen
                case "NextDeal":
               
                   
                    deals.Clear();
                    //Gets next seller deal.
                    SellerDeals();
                    sellerMessagesReceived = 0;
                    break;

                case "maketFinal":
                    marketFinished++;
                        if (marketFinished == HouseholdSetup.totalNumberOfHouseholds) 
                        { 
                       
                        Console.WriteLine("Work on proccess...");
                       
                        }
                        else 
                        {
                        Broadcast("fin");

              

                    }

                    break;

            }
        }

        //Printing seller deals.
        private void SellerDeals()
        {
            

            //Calling the class Household in case there are no remaing sellers or buyers.
            Households house = new Households();

            //When there are no more sellers, the energy will be bought to main source.
            if (listOfBuyers.Count() == 0)
            {
                Console.WriteLine("\n No remaining Buyers\n");
                house.SellMain();
                Broadcast("SellMain");
                house.BuyMain();
                Broadcast("BuyMain");

                Console.WriteLine("finished");
            }
          
        

            if (listOfSellers.Count() == 0)
            {
                
                Console.WriteLine("\nNo remaining Sellers\n");
                house.BuyMain();
                Broadcast("BuyMain");
                house.SellMain();
                Broadcast("SellMain");
                Console.WriteLine("finished");
            
            }
          

            //Geeting all sellers from list.
            foreach (string sellers in listOfSellers)
            {
                //Sending the data of the seller to know about seller amount of energy to sell.
                Send(sellers, "sellers_deals");
                HouseholdSetup.numberOfDeals++;

            }



        }

  

        //Evaluates the deal of the seller so its added into the environment.
        private void EvaluateDeals()
        {
            //while there are deals, decision of higher value to pay is made.
            if (deals.Count != 0)
            {

                //Variable to store the higher value on the deals from the top to bottom, returning the first element on the sequence.
                var highest = deals.OrderByDescending(x => x.Value).FirstOrDefault();

                //Gets all the sellers from the list.
                foreach (string seller in listOfSellers)
                {
                    //Once all sellers are set, gets the highest number of the deal and gets accepted.
                    if (seller == highest.Key)
                    {
                        //Gives the deal ton the manager
                        Send(seller, "SellerdealCompleted");

                        //One more deal done.
                        HouseholdSetup.numberOfDeals++;

                    }
                }
            }
            else
            {
                //Once there is no more sellers in the list the maket closes and Broadcast the data with the summarize of the household.


            

                saveSummarize("\n            |--Market Finished--| \n");

                Console.WriteLine("             |---------------------|");
                Console.WriteLine("             |--  Market Resume  --|");
                Console.WriteLine("             |---------------------| \n");


                int Total = (HouseholdSetup.compradores - 1) + (HouseholdSetup.vendedores - 1) + (HouseholdSetup.dealsCompleted - 1);
                Console.WriteLine($"|- Buyer Number of Deals: {HouseholdSetup.compradores - 1} --|");
                Console.WriteLine($"|- Seller Number of Deals: {HouseholdSetup.vendedores - 1} --|");
                Console.WriteLine($"|- Total Deals Completed: {HouseholdSetup.dealsCompleted - 1} --|\n");
                Console.WriteLine("                |-- Total: " + Total + " messages. --|");

                saveSummarize($"|- Buyer Number of Deals: {HouseholdSetup.compradores - 1} --|");
                saveSummarize($"|- Seller Number of Deals: {HouseholdSetup.vendedores - 1} --|");
                saveSummarize($"|- Total Deals Completed: {HouseholdSetup.dealsCompleted - 1} --|\n");
                saveSummarize("         |-- Total: " + Total + " messages. --|");



                //Broadcasting the case which has the data of the summarize and the movements between the peers and main source.
                Broadcast("SellMainSource");
                Broadcast("BuyMainSource");

                Broadcast("BuyersSummarize");
                Broadcast("SellersSummarize");


            }

          

         

        }

        private void saveSummarize(string summarize)
        {
           

                try
                {
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



    }
}
