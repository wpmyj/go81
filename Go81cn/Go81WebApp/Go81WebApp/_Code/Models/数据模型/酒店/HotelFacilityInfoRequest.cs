using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Basic
{
    public class HotelFacilityInfoRequest : GenericRequest
    {
        private List<HotelFacilityInfo> body;

        public List<HotelFacilityInfo> Body
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
