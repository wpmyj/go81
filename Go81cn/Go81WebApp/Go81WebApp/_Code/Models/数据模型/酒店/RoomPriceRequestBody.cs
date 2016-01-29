using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Room
{
    public class RoomPriceRequestBody
    {
        private List<SetRoomPriceItem> setRoomPriceItems;

        public List<SetRoomPriceItem> SetRoomPriceItems
        {
            get
            {
                return setRoomPriceItems;
            }

            set
            {
                setRoomPriceItems = value;
            }
        }
    }
}
