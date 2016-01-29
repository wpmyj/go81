using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Room
{
    public class SetRoomPriceItem
    {
        private List<PriceInfo> priceInfos;
        private string roomId;
        private string startDate;
        private string endDate;
        private string currency;
        private string hotelId;

        public List<PriceInfo> PriceInfos
        {
            get
            {
                return priceInfos;
            }

            set
            {
                priceInfos = value;
            }
        }

        public string RoomId
        {
            get
            {
                return roomId;
            }

            set
            {
                roomId = value;
            }
        }

        public string StartDate
        {
            get
            {
                return startDate;
            }

            set
            {
                startDate = value;
            }
        }

        public string EndDate
        {
            get
            {
                return endDate;
            }

            set
            {
                endDate = value;
            }
        }

        public string Currency
        {
            get
            {
                return currency;
            }

            set
            {
                currency = value;
            }
        }

        public string HotelId
        {
            get
            {
                return hotelId;
            }

            set
            {
                hotelId = value;
            }
        }
    }
}
