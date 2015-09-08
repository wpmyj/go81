﻿var cityInit = ['请选择省份'];
var cityArr = new Array();

cityArr[0] = new Array("四川省", "成都市|自贡市|攀枝花市|泸州市|德阳市|绵阳市|广元市|遂宁市|内江市|乐山市|南充市|眉山市|宜宾市|广安市|达州市|雅安市|巴中市|资阳市|阿坝藏族羌族自治州|甘孜藏族自治州|凉山彝族自治州");
cityArr[1] = new Array("重庆市", "重庆市");
cityArr[2] = new Array("云南省", "昆明市|曲靖市|玉溪市|保山市|昭通市|丽江市|普洱市|临沧市|楚雄彝族自治州|红河哈尼族彝族自治州|文山壮族苗族自治州|西双版纳傣族自治州|大理白族自治州|德宏傣族景颇族自治州|怒江傈僳族自治州|迪庆藏族自治州");
cityArr[3] = new Array("贵州省", "贵阳市|六盘水市|遵义市|安顺市|毕节市|铜仁市|黔西南布依族苗族自治州|黔东南苗族侗族自治州|黔南布依族苗族自治州");
cityArr[4] = new Array("西藏自治区", "拉萨市|昌都地区|山南地区|日喀则地区|那曲地区|阿里地区|林芝地区");
cityArr[5] = new Array("北京市", "北京市");
cityArr[6] = new Array("天津市", "天津市");
cityArr[7] = new Array("河北省", "石家庄市|唐山市|秦皇岛市|邯郸市|邢台市|保定市|张家口市|承德市|沧州市|廊坊市|衡水市");
cityArr[8] = new Array("山西省", "太原市|大同市|阳泉市|长治市|晋城市|朔州市|晋中市|运城市|忻州市|临汾市|吕梁市");
cityArr[9] = new Array("内蒙古自治区", "呼和浩特市|包头市|乌海市|赤峰市|通辽市|鄂尔多斯市|呼伦贝尔市|巴彦淖尔市|乌兰察布市|兴安盟|锡林郭勒盟|阿拉善盟");
cityArr[10] = new Array("辽宁省", "沈阳市|大连市|鞍山市|抚顺市|本溪市|丹东市|锦州市|营口市|阜新市|辽阳市|盘锦市|铁岭市|朝阳市|葫芦岛市");
cityArr[11] = new Array("吉林省", "长春市|吉林市|四平市|辽源市|通化市|白山市|松原市|白城市|延边朝鲜族自治州");
cityArr[12] = new Array("黑龙江省", "哈尔滨市|齐齐哈尔市|鸡西市|鹤岗市|双鸭山市|大庆市|伊春市|佳木斯市|七台河市|牡丹江市|黑河市|绥化市|大兴安岭地区");
cityArr[13] = new Array("上海市", "上海市");
cityArr[14] = new Array("江苏省", "南京市|无锡市|徐州市|常州市|苏州市|南通市|连云港市|淮安市|盐城市|扬州市|镇江市|泰州市|宿迁市");
cityArr[15] = new Array("浙江省", "杭州市|宁波市|温州市|嘉兴市|湖州市|绍兴市|金华市|衢州市|舟山市|台州市|丽水市");
cityArr[16] = new Array("安徽省", "合肥市|芜湖市|蚌埠市|淮南市|马鞍山市|淮北市|铜陵市|安庆市|黄山市|滁州市|阜阳市|宿州市|六安市|亳州市|池州市|宣城市");
cityArr[17] = new Array("福建省", "福州市|厦门市|莆田市|三明市|泉州市|漳州市|南平市|龙岩市|宁德市");
cityArr[18] = new Array("江西省", "南昌市|景德镇市|萍乡市|九江市|新余市|鹰潭市|赣州市|吉安市|宜春市|抚州市|上饶市");
cityArr[19] = new Array("山东省", "济南市|青岛市|淄博市|枣庄市|东营市|烟台市|潍坊市|济宁市|泰安市|威海市|日照市|莱芜市|临沂市|德州市|聊城市|滨州市|菏泽市");
cityArr[20] = new Array("河南省", "郑州市|开封市|洛阳市|平顶山市|安阳市|鹤壁市|新乡市|焦作市|濮阳市|许昌市|漯河市|三门峡市|南阳市|商丘市|信阳市|周口市|驻马店市|省直辖县级行政区划");
cityArr[21] = new Array("湖北省", "武汉市|黄石市|十堰市|宜昌市|襄阳市|鄂州市|荆门市|孝感市|荆州市|黄冈市|咸宁市|随州市|恩施土家族苗族自治州|省直辖县级行政区划");
cityArr[22] = new Array("湖南省", "长沙市|株洲市|湘潭市|衡阳市|邵阳市|岳阳市|常德市|张家界市|益阳市|郴州市|永州市|怀化市|娄底市|湘西土家族苗族自治州");
cityArr[23] = new Array("广东省", "广州市|韶关市|深圳市|珠海市|汕头市|佛山市|江门市|湛江市|茂名市|肇庆市|惠州市|梅州市|汕尾市|河源市|阳江市|清远市|东莞市|中山市|潮州市|揭阳市|云浮市");
cityArr[24] = new Array("广西壮族自治区", "南宁市|柳州市|桂林市|梧州市|北海市|防城港市|钦州市|贵港市|玉林市|百色市|贺州市|河池市|来宾市|崇左市");
cityArr[25] = new Array("海南省", "海口市|三亚市|三沙市|省直辖县级行政区划");
cityArr[26] = new Array("陕西省", "西安市|铜川市|宝鸡市|咸阳市|渭南市|延安市|汉中市|榆林市|安康市|商洛市");
cityArr[27] = new Array("甘肃省", "兰州市|嘉峪关市|金昌市|白银市|天水市|武威市|张掖市|平凉市|酒泉市|庆阳市|定西市|陇南市|临夏回族自治州|甘南藏族自治州");
cityArr[28] = new Array("青海省", "西宁市|海东市|海北藏族自治州|黄南藏族自治州|海南藏族自治州|果洛藏族自治州|玉树藏族自治州|海西蒙古族藏族自治州");
cityArr[29] = new Array("宁夏回族自治区", "银川市|石嘴山市|吴忠市|固原市|中卫市");
cityArr[30] = new Array("新疆维吾尔自治区", "乌鲁木齐市|克拉玛依市|吐鲁番地区|哈密地区|昌吉回族自治州|博尔塔拉蒙古自治州|巴音郭楞蒙古自治州|阿克苏地区|克孜勒苏柯尔克孜自治州|喀什地区|和田地区|伊犁哈萨克自治州|塔城地区|阿勒泰地区|自治区直辖县级行政区划");
cityArr[31] = new Array("台湾省", "台湾省");
cityArr[32] = new Array("香港特别行政区", "香港特别行政区");
cityArr[33] = new Array("澳门特别行政区", "澳门特别行政区");


var Search = function () {
    return {

        initArea: function () {

            var provinceLen = cityArr.length;//共多少个省市自治区  
            var areaArr = [];
            var provinceArr = [];
            areaArr.push('<div class="sw-ui-area-box"><div class="sw-ui-area-bg"></div><div class="sw-ui-area-body"><div class="sw-ui-area-ab-all">');
            areaArr.push('<a class="sw-ui-area-ab-prov-all"  p="" c="" v="" onclick="prosuct_searchbyplace(this);">所有地区</a>');//构造省  
            areaArr.push('<ul class="sw-ui-area-ab-prov">');//构造省  
            for (var i = 0; i < provinceLen; i++) {
                var p = cityArr[i][0];
                var pArr = new Array();
                var csArr = cityArr[i][1].split("|");
                var csLen = csArr.length;

                pArr.push('<li class="sw-ui-area-box-item sw-ui-area-abProv-im">');
                pArr.push('<a class="sw-ui-area-box-link sw-ui-area-ab-prov-itemLink"  p="' + p + '" c="" v="' + p + '" onclick="prosuct_searchbyplace(this);">' + p + '</a>');
                pArr.push('<ul class="sw-ui-area-ab-prov-items">');

                for (var j = 0; j < csLen ; j++) {//构造市  
                    var c = csArr[j];
                    pArr.push('<li class="sw-ui-area-box-item">');
                    pArr.push('<a class="sw-ui-area-box-link sw-ui-area-abProv-itemsubLink" p="' + p + '" c="' + c + '" v="' + c + '" onclick="prosuct_searchbyplace(this);">' + c + '</a>');
                    pArr.push('</li>');
                }

                pArr.push('</ul>');
                var pStr = pArr.join("");
                areaArr.push(pStr);
            }//end for  

            areaArr.push('</ul>');//结束省  
            areaArr.push('</div></div></div>');
            var areaStr = areaArr.join("");
            $(".area .def_box").append(areaStr);

        },

        //选择地区  
        areaEffect: function () {

            //显示全部区域及省份  
            $(".def_box").hover(function () {
                $(this).find(".sw-ui-area-box").show();
            }, function () {
                $(this).find(".sw-ui-area-box").hide();
            });

            //显示省级以下的市级城市  
            $(".sw-ui-area-box-item").hover(function () {
                $(this).css("z-index", "90").find(".sw-ui-area-ab-prov-items").show();
            }, function () {
                $(this).css("z-index", "0").find(".sw-ui-area-ab-prov-items").hide();
            });

        }

    }
}();

$(function () {

    Search.initArea();
    Search.areaEffect();

})
