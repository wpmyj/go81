using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Basic
{
    public class HotelFacilityInfo
    {
        private string hotelCode;
        private int facilityId;
        private string facilityName;
        private string facilityDescript;

        public string HotelCode
        {
            get
            {
                return hotelCode;
            }

            set
            {
                hotelCode = value;
            }
        }

        public int FacilityId
        {
            get
            {
                return facilityId;
            }

            set
            {
                facilityId = value;
            }
        }

        public string FacilityName
        {
            get
            {
                return facilityName;
            }

            set
            {
                facilityName = value;
            }
        }

        public string FacilityDescript
        {
            get
            {
                return facilityDescript;
            }

            set
            {
                facilityDescript = value;
            }
        }
    }
}
