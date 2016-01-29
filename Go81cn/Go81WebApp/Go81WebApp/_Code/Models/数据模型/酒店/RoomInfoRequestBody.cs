using System.Collections.Generic;

namespace DTAPI.Room
{
    public class RoomInfoRequestBody
    {
        private string hotelCode;
        private string startDate;
        private string endDate;
        private string editer;
        private string roomID;
        private List<RoomInfo> roomInfoList;

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

        public string Editer
        {
            get
            {
                return editer;
            }

            set
            {
                editer = value;
            }
        }

        public string RoomID
        {
            get
            {
                return roomID;
            }

            set
            {
                roomID = value;
            }
        }

        public List<RoomInfo> RoomInfoList
        {
            get
            {
                return roomInfoList;
            }

            set
            {
                roomInfoList = value;
            }
        }
    }
}
