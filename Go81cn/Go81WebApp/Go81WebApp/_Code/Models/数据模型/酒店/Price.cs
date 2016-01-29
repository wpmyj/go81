using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Room
{
    public class Price
    {
        private int amountBeforeTaxFee;
        private int amountAfterTaxFee;
        private int costAmountBeforeTaxFee;
        private int costAmountAfterTaxFee;
        private int day;

        public int AmountBeforeTaxFee
        {
            get
            {
                return amountBeforeTaxFee;
            }

            set
            {
                amountBeforeTaxFee = value;
            }
        }

        public int AmountAfterTaxFee
        {
            get
            {
                return amountAfterTaxFee;
            }

            set
            {
                amountAfterTaxFee = value;
            }
        }

        public int CostAmountBeforeTaxFee
        {
            get
            {
                return costAmountBeforeTaxFee;
            }

            set
            {
                costAmountBeforeTaxFee = value;
            }
        }

        public int CostAmountAfterTaxFee
        {
            get
            {
                return costAmountAfterTaxFee;
            }

            set
            {
                costAmountAfterTaxFee = value;
            }
        }

        public int Day
        {
            get
            {
                return day;
            }

            set
            {
                day = value;
            }
        }
    }
}
