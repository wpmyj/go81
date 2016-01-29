using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Room
{
    public class RoomPriceRequest : GenericRequest
    {
        private RoomPriceRequestBody body;

        public RoomPriceRequestBody Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }
    }
}
