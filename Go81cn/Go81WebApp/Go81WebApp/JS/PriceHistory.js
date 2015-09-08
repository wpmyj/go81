$(function () {
    String.prototype.evalAsJSON = function () {
        return eval('(' + this + ')');
    }

    function showTooltip(x, y, contents) {
        $('<div id="historyPricesTooltip">' + contents + '</div>').css({
            position: 'absolute',
            display: 'none',
            top: y - 40,
            left: x + 20,
            border: '1px solid #ddf',
            padding: '2px',
            'background-color': '#eef',
            opacity: 0.80
        }).appendTo('body').fadeIn(200);
    }

    var hpsData = $('#historyPrices').attr('pdata').evalAsJSON();
    var hpsColor = [];
    var i = 0;
    $.each(hpsData, function (key, val) {
        hpsColor[key] = i;
        ++i;
    });

    $('#historyPrices').css('padding', '10px')
        .append('<select id="priceSelector"></select>')
        .append('<div id="chart" style="width:800px;height:400px"></div>');

    var previousPoint = null;
    $('#historyPrices #chart')
    .bind('plothover', function (event, pos, item) {
        if (item) {
            if (previousPoint != item.dataIndex) {
                previousPoint = item.dataIndex;

                $('#historyPricesTooltip').remove();
                var x = $.plot.formatDate(item.series.data[item.dataIndex][0], '%Y年%m月%d日');
                y = item.datapoint[1].toFixed(2);

                showTooltip(item.pageX, item.pageY,
                            (item.series.label + '，于' + x + '的价格为' + y).replace(/\，/g, '<br />'));
            }
        }
        else {
            $('#historyPricesTooltip').remove();
            previousPoint = null;
        }
    });

    var choiceContainer = $('#historyPrices #priceSelector');
    $.each(hpsData, function (key, val) {
        choiceContainer.append("<option value='" + key + "'>" + key);
    });
    choiceContainer.prop('selectedIndex', 0);
    choiceContainer.change(plotAccordingToChoices);

    function plotAccordingToChoices() {
        var data = [];
        var key = $("#historyPrices #priceSelector option:selected").val();
        if (key && hpsData[key]) {
            var pd = {};
            pd.label = key;
            pd.color = hpsColor[key];
            pd.data = hpsData[key];
            data.push(pd);
        }
        if (data.length > 0) {
            $('#historyPrices #chart').plot(data, {
                legend: {
                    show: false
                },
                xaxis: {
                    mode: 'time',
                    ticks: 8,
                    timeformat: '%Y.%m.%d',
                    timezone: 'browser'
                },
                yaxis: {
                    position: "right"
                },
                series: {
                    lines: { show: true, fill: false },
                    points: { show: true, fill: false }
                },
                grid: {
                    hoverable: true
                }
            });
        } else {
            $('#historyPrices #chart').html("无数据");
        }
    }

    plotAccordingToChoices();
});
