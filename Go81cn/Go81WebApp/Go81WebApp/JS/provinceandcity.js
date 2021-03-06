﻿//调用方法有两种,一是没有初始化值:new PCAS("deliverprovince","delivercity","deliverarea");
//二是有初始化值:new PCAS("deliverprovince","delivercity","deliverarea", "广东省","广州市","天河区");


SPT = "--请选择省份--";
SCT = "--请选择城市--";
SAT = "--请选择地区--";
ShowT = 1; //提示文字 0:不显示 1:显示

//省市区的格式如下: "A省$AA市,AAA区,AAB区|AB市,ABA区,ABB区#B省$BA市,BAA区,BAB区|BB市,BBA区,BBB区"

PCAD = "重庆市$不限,不限|重庆市,不限,巴县,北碚区,璧山县,长寿县,大渡口区,大足县,合川市,江北区,江北县,江津市,九龙坡区,南岸区,南桐矿区,綦江县,荣昌县,沙坪坝区,市辖区,市中区,双桥区,铜梁县,潼南县,永川市#四川省$不限,不限|成都市,不限,市辖区,锦江区,青羊区,金牛区,武侯区,成华乐,高新区,龙泉驿区,青白江区,新都区,金堂县,双流县,温江县,郫县,大邑县,蒲江县,新津县,都江堰市,彭州市,邛崃市,崇州市|自贡市,不限,市辖区,自流井区,贡井县,大安区,沿滩区,荣县,富顺县|攀枝花市,不限,市辖区,东区,西区,仁和区,米易县,盐边县|泸州市,不限,市辖区,江阳区,纳溪区,龙马潭区,泸县,合江县,叙永县,古蔺县|德阳市,不限,市辖区,旌阳区,中江县,罗江县,广汉市,什邡市,绵竹市|绵阳市,不限,市辖区,培城区,游仙区,三台县,盐亭县,安县,梓潼县,北川县,平武县,江油市|广元市,不限,市辖区,市中区,元坝区,朝天区,旺苍县,青川县,剑阁县,苍溪县|遂宁市,不限,市辖区,市中区,蓬溪县,射洪县,大英县|内江市,不限,市辖区,市中区,东兴区,威远县,资中县,隆昌县|乐山市,不限,市辖区,市中区,沙湾区,五通桥区,金口河区,犍为县,井研县,夹江县,沐川县,峨边彝族自治县,马边彝族自治县,峨眉山市|南充市,不限,市辖区,顺应区,高坪区,嘉陵区,南部县,营山县,蓬安县,仪陇县,西充县,阆中市|眉山市,不限,市辖区,东坡区,仁寿县,彭山县,洪雅县,丹棱县,青神县|宜宾市,不限,市辖区,翠屏区,宜宾县,南溪县,江安县,长宁县,高县,珙县,筠连县,兴文县,屏山县|广安市,不限,市辖区,广安区,岳池县,武胜县,邻水县,华蓥市|达州市,不限,市辖区,通川县,达县,宣汉县,开江县,大竹县,渠县,万源市|雅安市,不限,市辖区,雨城区,名山县,荥经县,汉源县,石棉县,天全县,芦山县,宝山县|巴中市,不限,市辖区,巴州区,通江县,南江县,平昌县|资阳市,不限,市辖区,雁江区,安岳县,乐至县,简阳市|阿坝藏族羌族自治州,不限,汶川县,理县,茂县,松潘县,九寨沟县,金川县,小金县,黑水县,马尔康县,壤塘县,阿坝县,若尔盖县,红原县|甘孜藏族自治州,不限,康定县,泸定县,丹巴县,九龙县,雅江县,道孚县,炉霍县,甘孜县,新龙县,德格县,白玉县,石渠县,色达县,理塘县,巴塘县,乡城县,稻城县,得荣县|凉山彝族自治州,不限,西昌市,木里藏族自治县,盐源县,德昌县,会理县,会东县,宁南县,普格县,布拖县,金阳县,昭觉县,喜德县,冕宁县,越西县,甘洛县,美姑县,雷波县#贵州省$不限,不限|贵阳市,不限,市辖区,南明区,云岩区,花溪区,乌当区,白云区,小河区,开阳县,息烽县,修文县,清镇市|六盘水市,不限,钟山区,六枝特区,水城县,盘县|遵义市,不限,市辖区,红花岗区,遵义县,桐梓,绥阳县,正安县,道真仡佬族苗族自治县,务川仡佬族苗族自治县,凤冈县,湄潭县,余庆县,习水县,赤水市,仁怀市|安顺市,不限,市辖区,西秀区,平坝县,普定县,镇宁布依族苗族自治县,关岭布依族苗族自治县,紫云苗族布依族自治县|铜仁地区,不限,铜仁市,江口县,玉屏侗族自治县,石阡县,思南县,印江土家族苗族自治县,德江县,沿河土家族自治县,松桃苗族自治县,万山特区|黔西南布依族苗族自治州,不限,兴义市,兴仁县,普安县,晴隆县,贞丰县,望谟县,册享县,安龙县|毕节地区,不限,毕节市,大方县,黔西县,金沙县,织金县,纳雍县,威宁彝族回族苏州自治县,赫章县|黔东南苗族侗族自治州,不限,凯里市,黄平县,施秉县,三穗县,镇远县,岑巩县,天柱县,锦屏县,剑河县,台江县,黎平县,榕江县,从江县,雷山县,麻江县,丹寨县|黔南布依族苗族自治州,不限,都匀市,福泉市,荔波县,贵定县,瓮安县,独山县,平塘县,罗甸县,长顺县,龙里县,惠水县,三都水族自治县#云南省$不限,不限|昆明市,不限,市辖区,五华区,盘龙区,官渡区,西山区,东川区,呈贡县,晋宁县,富民县,宜良县,石林彝族自治县,嵩明县,禄劝彝族苗族自治县,寻甸回族彝族自治县,安宁市|曲靖市,不限,市辖区,麒麟区,马龙县,陆良县,师宗县,罗平县,富源县,会泽县,沾益县,宣威市|玉溪市,不限,市辖区,红塔区,江川县,澄江县,海通县,华宁县,易门县,峨山彝族自治县,新平彝族傣族自治县,元江哈尼族彝族傣族自治县|保山市,不限,市辖区,隆阳区,施甸县,腾冲县,龙陵县,昌宁县|昭通市,不限,市辖区,昭阳区,鲁甸县,巧家县,盐津县,大关县,永善县,绥江县,镇雄县,彝良县,威信县,水富县|楚雄彝族自治州,不限,楚雄市,双柏县,牟定县,南华县,姚安县,大姚县,永仁县,元谋县,武定县,禄丰县|红河哈尼族彝族自治州,不限,个旧市,开远市,蒙自县,屏边苗族自治县,建水县,石屏县,弥勒县,泸西县,元阳县,红河县,金平苗族瑶族傣族自治县,绿春县,河口瑶族自治县|文山壮族苗族自治州,不限,文山县,砚山县,西畴县,麻栗坡县,马关县,丘北县,广南县,富宁县|思茅地区,不限,思茅市,普洱哈尼族彝族自治县,墨江哈尼族自治县,景东彝族自治县,景谷傣族彝族自治县,镇沅彝族哈尼族拉祜族自治县,江城哈尼族彝族自治县,孟连傣族拉祜族佤族自治县,澜沧拉祜族自治县,西盟佤族自治县|西双版纳傣族自治州,不限,景洪市,勐海县,勐腊县|大理白族自治州,不限,大理市,漾濞彝族自治县,祥云县,宾川县,弥渡县,南涧彝族自治县,巍山彝族回族自治县,永平县,云龙县,洱源县,剑川县,鹤庆县|德宏傣族景颇族自治州,不限,瑞丽市,潞西市,梁河县,盈江县,陇川县|丽江地区,不限,丽江纳西族自治县,永胜县,华坪县,宁蒗彝族自治县|怒江傈僳族自治州,不限,泸水县,福贡县,贡山独龙族怒族自治县,兰坪白族普米族自治县|迪庆藏族自治州,不限,香格里拉县,德钦县,维西傈僳族自治县|临沧地区,不限,临沧县,凤庆县,云县,永德县,镇康县,双江拉祜族佤族布朗族傣族自治县,耿马傣族佤族自治县,沧源佤族自治县#西藏自治区$不限,不限|拉萨市,不限,市辖区,城关区,林周县,当雄县,尼木县,曲水县,堆龙德庆县,达孜县,墨竹工卡县|昌都地区,不限,昌都县,江达县,贡觉县,类乌齐县,丁青县,察雅县,八宿县,左贡县,芒康县,洛隆县|山南地区,不限,乃东县,扎囊县,贡嘎县,桑日县,琼结县,曲松县,措美县,洛扎县,加查县,隆子县,错那县,浪卡子县|日喀则地区,不限,日喀则市,南木林县,江孜县,定日县,萨迦县,拉孜县,昂仁县,谢通门县,白朗县,仁布县,康马县,定结县,仲巴县,亚东县,吉隆县,聂拉木县,萨嘎县,岗巴县|那曲地区,不限,那曲县,嘉黎县,比如县,聂荣县,安多县,申扎县,索县,班戈县,巴表县,尼玛县|阿里地区,不限,普兰县,札达县,噶尔县,日土县,革吉县,改则县,措勤县|林芝地区,不限,林芝县,工布江达县,米林县,墨脱县,波密县,察隅县,朗县,樟木口岸";

if (ShowT)
    PCAD = SPT + "$" + SCT + "," + SAT + "#" + PCAD;

PCAArea = [];
PCAP = [];
PCAC = [];
PCAA = [];
PCAN = PCAD.split("#");

for (i = 0; i < PCAN.length; i++) {
    PCAA[i] = [];
    TArea = PCAN[i].split("$")[1].split("|");

    for (j = 0; j < TArea.length; j++) {
        PCAA[i][j] = TArea[j].split(",");

        if (PCAA[i][j].length == 1)
            PCAA[i][j][1] = SAT;

        TArea[j] = TArea[j].split(",")[0];
    }

    PCAArea[i] = PCAN[i].split("$")[0] + "," + TArea.join(",");
    PCAP[i] = PCAArea[i].split(",")[0];
    PCAC[i] = PCAArea[i].split(',');
}

//下面开始初始化省市区控件
function PCAS() {
    this.SelP = document.getElementsByName(arguments[0])[0];
    this.SelC = document.getElementsByName(arguments[1])[0];
    this.SelA = document.getElementsByName(arguments[2])[0];
    this.DefP = this.SelA ? arguments[3] : arguments[2];
    this.DefC = this.SelA ? arguments[4] : arguments[3];
    this.DefA = this.SelA ? arguments[5] : arguments[4];
    this.SelP.PCA = this;
    this.SelC.PCA = this;

    this.SelP.onchange = function () {
        PCAS.SetC(this.PCA);
    };
    if (this.SelA)
        this.SelC.onchange = function () {
            PCAS.SetA(this.PCA);
        };
    PCAS.SetP(this);
};

//初始化省
PCAS.SetP = function (PCA) {
    for (var i = 0; i < PCAP.length; i++) {
        var PCAPV;
        var PCAPT = PCAPV = PCAP[i];

        if (PCAPT == SPT)
            PCAPV = "";

        PCA.SelP.options.add(new Option(PCAPT, PCAPV));

        if (PCA.DefP == PCAPV)
            PCA.SelP[i].selected = true;
    }

    PCAS.SetC(PCA);
};

//初始化市
PCAS.SetC = function (PCA) {
    var PI = PCA.SelP.selectedIndex;
    PCA.SelC.length = 0;

    for (var i = 1; i < PCAC[PI].length; i++) {
        var PCACV;
        var PCACT = PCACV = PCAC[PI][i];

        if (PCACT == SCT)
            PCACV = "";

        PCA.SelC.options.add(new Option(PCACT, PCACV));

        if (PCA.DefC == PCACV)
            PCA.SelC[i - 1].selected = true;
    }

    if (PCA.SelA)
        PCAS.SetA(PCA);
};

//初始化区
PCAS.SetA = function (PCA) {
    var PI = PCA.SelP.selectedIndex;
    var CI = PCA.SelC.selectedIndex;
    PCA.SelA.length = 0;

    for (var i = 1; i < PCAA[PI][CI].length; i++) {
        var PCAAV;
        var PCAAT = PCAAV = PCAA[PI][CI][i];

        if (PCAAT == SAT)
            PCAAV = "";

        PCA.SelA.options.add(new Option(PCAAT, PCAAV));

        if (PCA.DefA == PCAAV)
            PCA.SelA[i - 1].selected = true;
    }
};