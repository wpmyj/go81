

using DTAPI.Basic;
using System.Collections.Generic;

namespace DTAPI
{
    /// <summary>
    /// 基础信息类
    /// </summary>
    public class BasicUtil
    {				
        public static string GetRequestData(List<HotelInfo> hotelInfoList)
        {
            string requestData;
            GenericRequest request = HTTPHandler.GenerateRequetData("GetHotelInfo");

            //GetHotelInfo Header Body
            HotelInfoRequest hotelInfoRequest = new HotelInfoRequest();
            hotelInfoRequest.Head = request.Head;
            hotelInfoRequest.Body = hotelInfoList;

            //Json数据
            requestData = JsonUtil.GetJsonFromObject(hotelInfoRequest);

            return requestData;
        }

        /// <summary>
        /// 获取酒店信息
        /// </summary>
        /// <param name="hotelInfoList"></param>
        /// <returns></returns>
        public static string GetHotelInfo(List<HotelInfo> hotelInfoList)
        {
            string responseData = string.Empty;
            string requestData = GetRequestData(hotelInfoList);

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }

        /// <summary>
        /// 获取房型信息
        /// </summary>
        /// <param name="roomTypeInfoList"></param>
        /// <returns></returns>
        public static string GetRoomTypeInfo(List<RoomTypeInfo> roomTypeInfoList)
        {
            string responseData = string.Empty;
            string requestData;

            GenericRequest request = HTTPHandler.GenerateRequetData("GetRoomTypeInfo");

            //GetRoomTypeInfo Header Body
            RoomTypeInfoRequest roomTypeInfoRequest = new RoomTypeInfoRequest();
            roomTypeInfoRequest.Head = request.Head;
            roomTypeInfoRequest.Body = roomTypeInfoList;

            //Json数据
            requestData = JsonUtil.GetJsonFromObject(roomTypeInfoRequest);

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }

        /// <summary>
        /// 获取设施设备
        /// </summary>
        /// <param name="hotelFacilityInfoList"></param>
        /// <returns></returns>
        public static string GetHotelFacilityInfo(List<HotelFacilityInfo> hotelFacilityInfoList)
        {
            string responseData = string.Empty;
            string requestData;

            GenericRequest request = HTTPHandler.GenerateRequetData("GetHotelFacilityInfo");

            //GetHotelFacilityInfo Header Body
            HotelFacilityInfoRequest hotelFacilityInfoRequest = new HotelFacilityInfoRequest();
            hotelFacilityInfoRequest.Head = request.Head;
            hotelFacilityInfoRequest.Body = hotelFacilityInfoList;

            //Json数据
            requestData = JsonUtil.GetJsonFromObject(hotelFacilityInfoRequest);

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="hotelImageInfoList"></param>
        /// <returns></returns>
        public static string GetHotelImageInfo(List<HotelImageInfo> hotelImageInfoList)
        {
            string responseData = string.Empty;
            string requestData;

            GenericRequest request = HTTPHandler.GenerateRequetData("GetHotelImageInfo");

            //GetHotelFacilityInfo Header Body
            HotelImageInfoRequest hotelImageInfoRequest = new HotelImageInfoRequest();
            hotelImageInfoRequest.Head = request.Head;
            hotelImageInfoRequest.Body = hotelImageInfoList;

            //Json数据
            requestData = JsonUtil.GetJsonFromObject(hotelImageInfoRequest);

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }
				
				/// <summary>
        /// 另外一种获取基础信息的方式
        /// </summary>
        /// <param name="serviceName">接口名称</param>
        /// <returns></returns>
        public static string GetBasicResponseData(string serviceName)
        {
            string responseData = string.Empty;
            string requestData=string.Empty;
            if (!serviceName.Equals(string.Empty))
            {
                // 获取酒店信息
                if (serviceName.Equals("GetHotelInfo"))
                {
                    //示例JSON字符串
                    responseData = JsonUtil.LoadJson("GetHotelInfo.json");
                }
                // 获取房型信息
                else if (serviceName.Equals("GetRoomTypeInfo"))
                {
                    responseData = JsonUtil.LoadJson("GetRoomTypeInfo.json");
                }
                // 获取设施设备
                else if (serviceName.Equals("GetHotelFacilityInfo"))
                {
                    responseData = JsonUtil.LoadJson("GetHotelFacilityInfo.json");
                }
                // 获取图片
                else if (serviceName.Equals("GetHotelImageInfo"))
                {
                    responseData = JsonUtil.LoadJson("GetHotelImageInfo.json");
                }								
						}

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }	
    }
}
