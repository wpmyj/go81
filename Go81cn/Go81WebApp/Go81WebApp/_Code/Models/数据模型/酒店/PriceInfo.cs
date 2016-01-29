using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Room
{
    public class PriceInfo
    {
        private List<Price> prices;
        private int stays;
        private string blanceType;
        private string priceType;
        private int breakfast;

        public List<Price> Prices
        {
            get
            {
                return prices;
            }

            set
            {
                prices = value;
            }
        }

        public int Stays
        {
            get
            {
                return stays;
            }

            set
            {
                stays = value;
            }
        }

        public string BlanceType
        {
            get
            {
                return blanceType;
            }

            set
            {
                blanceType = value;
            }
        }

        public string PriceType
        {
            get
            {
                return priceType;
            }

            set
            {
                priceType = value;
            }
        }

        public int Breakfast
        {
            get
            {
                return breakfast;
            }

            set
            {
                breakfast = value;
            }
        }
    }
}
