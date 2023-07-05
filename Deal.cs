using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergySystem23
{
    public class Deal
    {
        public string BuyerName { get; }
        public string SellerName { get; }
        public decimal FinalPrice { get; }

        public Deal(string buyerName, string sellerName, decimal finalPrice)
        {
            BuyerName = buyerName;
            SellerName = sellerName;
            FinalPrice = finalPrice;
        }
    }



}
