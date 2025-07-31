
const colorPalette1 = ['#FBF8CC', '#A3C4F3', '#FDE4CF', '#90DBF4', '#FFCFD2', '#8EECF5', '#F1C0E8', '#98F5E1', '#CFBAF0', '#B9FBC0'];
const colorPalette2 = ['#EDDCD2', '#DBE7E4', '#FFF1E6', '#F0EFEB', '#FDE2E4', '#D6E2E9', '#FAD2E1', '#BCD4E6', '#C5DEDD', '#99C1DE'];
const colorPalette3 = ['#0077ae', '#5878c3', '#9773c9', '#d06abc', '#fb639e', '#ff6a75', '#ff8447', '#ffa600', '#7eb0d5', '#91b5e0', '#a6b9ea', '#bdbdf0', '#d4c1f4', '#eac4f6', '#ffc8f5', '#ffbee4', '#ffb5cd', '#ffafb2', '#ffad94', '#ffaf76', '#ffb55a']
const colorPalette4 = ['#398f85', '#53a28a', '#70b48e', '#90c692', '#b3d795', '#d8e69b', '#fff5a5', '#ffdc8d', '#ffc27c', '#fca871', '#f68d6c', '#ec736d', '#de5a71'];
const colorPalette5 = ['#F5B7B1', '#D2B4DE', '#AED6F1', '#A2D9CE', '#F9E79F', '#F5CBA7', '#D5DBDB', '#D5DBDB', '#398f85', '#b3d795', '#ffdc8d', '#f68d6c'];

//https://stackoverflow.com/questions/5560248/programmatically-lighten-or-darken-a-hex-color-or-rgb-and-blend-colors
const pSBC = (p, c0, c1, l) => {
    let r, g, b, P, f, t, h, i = parseInt, m = Math.round, a = typeof (c1) == "string";
    if (typeof (p) != "number" || p < -1 || p > 1 || typeof (c0) != "string" || (c0[0] != 'r' && c0[0] != '#') || (c1 && !a)) return null;
    if (!this.pSBCr) this.pSBCr = (d) => {
        let n = d.length, x = {};
        if (n > 9) {
            [r, g, b, a] = d = d.split(","), n = d.length;
            if (n < 3 || n > 4) return null;
            x.r = i(r[3] == "a" ? r.slice(5) : r.slice(4)), x.g = i(g), x.b = i(b), x.a = a ? parseFloat(a) : -1
        } else {
            if (n == 8 || n == 6 || n < 4) return null;
            if (n < 6) d = "#" + d[1] + d[1] + d[2] + d[2] + d[3] + d[3] + (n > 4 ? d[4] + d[4] : "");
            d = i(d.slice(1), 16);
            if (n == 9 || n == 5) x.r = d >> 24 & 255, x.g = d >> 16 & 255, x.b = d >> 8 & 255, x.a = m((d & 255) / 0.255) / 1000;
            else x.r = d >> 16, x.g = d >> 8 & 255, x.b = d & 255, x.a = -1
        } return x
    };
    h = c0.length > 9, h = a ? c1.length > 9 ? true : c1 == "c" ? !h : false : h, f = this.pSBCr(c0), P = p < 0, t = c1 && c1 != "c" ? this.pSBCr(c1) : P ? { r: 0, g: 0, b: 0, a: -1 } : { r: 255, g: 255, b: 255, a: -1 }, p = P ? p * -1 : p, P = 1 - p;
    if (!f || !t) return null;
    if (l) r = m(P * f.r + p * t.r), g = m(P * f.g + p * t.g), b = m(P * f.b + p * t.b);
    else r = m((P * f.r ** 2 + p * t.r ** 2) ** 0.5), g = m((P * f.g ** 2 + p * t.g ** 2) ** 0.5), b = m((P * f.b ** 2 + p * t.b ** 2) ** 0.5);
    a = f.a, t = t.a, f = a >= 0 || t >= 0, a = f ? a < 0 ? t : t < 0 ? a : a * P + t * p : 0;
    if (h) return "rgb" + (f ? "a(" : "(") + r + "," + g + "," + b + (f ? "," + m(a * 1000) / 1000 : "") + ")";
    else return "#" + (4294967296 + r * 16777216 + g * 65536 + b * 256 + (f ? m(a * 255) : 0)).toString(16).slice(1, f ? undefined : -2)
}

class DashboardServerDataProvider {

    dataUrl = '/JobMan/Metrics/Server';
    intervalHandle = null;

    constructor() {
        this.init();
    }


    init() {
        var _this = this;
        this.intervalHandle = setInterval(
            function () {
                _this.update();
            },
            1000);
    }

    update() {

        var _this = this;
        $.ajax(
            {
                url: this.dataUrl,
                type: 'GET',
                traditional: true,
                success: function (result) {
                    _this.dataReceived(result);
                },
                error: function (xhr, status, error) {
                    console.log(xhr);
                    console.log(status);
                    console.log(error);

                    //alert(error);
                }
            }
        );
    }

    dataReceived(result) {
        console.log(result);
        $('#dispServerCount').html(1);
        $('#dispPoolCount').html(result.poolCount.toLocaleString());
        $('#dispWorkerCount').html(result.workerCount.toLocaleString());

        $('#dispWaitingJobCount').html(result.workDataGlobal.waiting.toLocaleString());
        $('#inBufferJobCount').html(result.workDataGlobal.inQueue.toLocaleString());

        $('#dispCompletedJobCount').html(result.workDataGlobal.processed.toLocaleString());
        $('#dispFailedJobCount').html(result.workDataGlobal.fail.toLocaleString());


        var chartData = result.workDataPoolsUi;
        var chartElement = $('#workGraphChart');
        var workGraphPlugin = WorkGraph.getInstance(chartElement);
        workGraphPlugin.update(chartData);

        //WorkDataGlobal

        //dispServerCount
        //dispPoolCount
        //dispWorkerCount
        //dispWaitingJobCount
        //dispCompletedJobCount
        //dispFailedJobCount
    }

    addTestLoad() {

        var _this = this;
        $.ajax(
            {
                url: '/JobMan/Home/AddLoadTest',
                type: 'GET',
                traditional: true,
                success: function (result) {
                    alert('added');
                },
                error: function (xhr, status, error) {
                    console.log(xhr);
                    console.log(status);
                    console.log(error);

                    //alert(error);
                }
            }
        );
    }

}

class WorkGraph {

    chartElement = null;
    chartObject = null;
    data = {};
    chartData = null;
    seriesNames = [];

    static getInstance(parentElement, seriesNames) {
        var _element = $(parentElement);
        var plugin = _element.data('workGraphPlugin');
        if (plugin === undefined) {
            plugin = new WorkGraph(_element, seriesNames);
            _element.data('workGraphPlugin', plugin);
        }

        return plugin;
    }

    constructor(parentElement, seriesNames) {
        console.log("Chart construct");
        this.chartElement = $(parentElement);

        if (seriesNames != undefined && seriesNames != null)
            this.init(seriesNames)
    }

    getRandomColor() {
        //https://stackoverflow.com/questions/43193341/how-to-generate-random-pastel-or-brighter-color-in-javascript#43195379
        var colors = [];
        var hue = Math.random();
        var saturate = Math.random();
        var bright = Math.random();

        colors.fill = "hsl(" + 360 * hue + ',' +
            (25 + 70 * saturate) + '%,' +
            (85 + 10 * bright) + '%)'


        colors.stroke = "hsl(" + 360 * hue + ',' +
            (25 + 10 * saturate) + '%,' +
            (65 + 10 * bright) + '%)'

        return colors;
    }

    getColorFromPalette(index) {
        var indexMod = index % 8;

        var color = {
            fill: colorPalette5[indexMod],
            stroke: null
        }

        color.stroke = pSBC(-0.2, color.fill);

        if (!color.stroke)
            color.stroke = color.fill;

        return color;
    }

    initData(seriesNames) {
        var _datasets = [];

        for (var i = 0; i < seriesNames.length; i++) {
            var name = seriesNames[i];

            var color = this.getColorFromPalette(i);
            var dataset = {
                label: name,
                name: name,
                backgroundColor: color.fill,
                pointBackgroundColor: color.fill,
                borderColor: color.stroke,
                borderWidth: 1,
                pointHighlightStroke: color.fill,
                borderCapStyle: 'butt',
                pointRadius: 1,
                data: []
            }

            _datasets.push(dataset);
        }

        this.chartData = {
            labels: [],
            datasets: _datasets
        };
    }

    init(seriesNames) {
        console.log("Chart init");
        //Chart.defaults.scales.linear.min = 0;
        //Chart.defaults.scales.linear.suggestedMax = 0;

        this.initData(seriesNames);

        var settings = {
            type: 'line',
            data: this.chartData,
            options: {
                responsive: true,
                scales: {
                    yAxes: [{
                        stacked: true,
                        beginAtZero: true,
                        //min: 0,
                        //suggestedMax: 100,
                        ticks: {
                            suggestedMax: 30,
                            beginAtZero: true,
                            stepSize: 1,
                            callback: function (value) { if (value % 1 === 0) { return value; } }
                        },
                        gridLines: {
                            color: '#fbfefb'
                        },
                        grid: {
                            color: '#fbfefb'
                            //drawOnChartArea: false
                        },
                    }],
                    xAxes: [{
                        ticks: {
                            display: false
                        },
                        gridLines: {
                            color: '#fbfefb'
                        },
                        grid: {
                            color: '#fbfefb'
                            //drawOnChartArea: false
                        },
                    }],
                    y: {
                        min: 0,
                        suggestedMax: 100
                    }
                },
                animation: {
                    duration: 0,
                },
                elements: {
                    line: {
                        tension: 0.1
                    }
                },

            }
        };

        this.chartObject = new Chart(this.chartElement, settings);
    }



    updateChartFullData(data) {

        /*
            var data[time] = {
                SeriesName1: value1,
                SeriesName2: value2,
            }
        */

        var seriesTimings = Object.getOwnPropertyNames(data);
        var seriesValues = {};

        this.chartObject.data.labels = seriesTimings;

        for (var i = 0; i < seriesTimings.length; i++) {
            var time = seriesTimings[i];
            var seriesData = data[time];

            for (var k = 0; k < this.seriesNames.length; k++) {
                var serieName = this.seriesNames[k];
                var value = seriesData[serieName];
                if (value === undefined)
                    value = 0;

                var serieValueSet = seriesValues[serieName];
                if (serieValueSet === undefined) {
                    seriesValues[serieName] = serieValueSet = [];
                }

                serieValueSet.push(value);
            }
        }

        for (var i = 0; i < this.chartObject.data.datasets.length; i++) {
            var datasetOnChart = this.chartObject.data.datasets[i];
            var serieValueSet = seriesValues[datasetOnChart.name];
            datasetOnChart.data = serieValueSet;
        }


        this.chartObject.update();
    }

    createWindow() {
        var dataTimings = Object.getOwnPropertyNames(this.data);
        //debugger;
        while (dataTimings.length > 100) {
            var timing = dataTimings[0];
            delete this.data[timing];
            dataTimings = Object.getOwnPropertyNames(this.data);
        }

        //debugger;
        var refTime = null;
        if (dataTimings.length == 0)
            refTime = new Date();
        else
            refTime = new Date(dataTimings[0]);

        var timingCount = dataTimings.length;
        while (timingCount < 100) {
            //new Date().toJSON().slice(0, 19);
            refTime.setSeconds(refTime.getSeconds() - 1);
            var refTimeStr = refTime.toJSON().slice(0, 19);
            var newDataItem = {};

            for (var k = 0; k < this.seriesNames.length; k++) {
                var serieName = this.seriesNames[k];
                newDataItem[serieName] = 0;
            }

            this.data[refTimeStr] = newDataItem;
            timingCount++;
        }
    }

    update(seriesData) {
        this.seriesNames = [];

        var seriesDataParts = Object.getOwnPropertyNames(seriesData);
        for (var i = 0; i < seriesDataParts.length; i++) {
            var serieName = seriesDataParts[i];
            this.seriesNames.push(serieName);
        }

        if (this.chartObject === null)
            this.init(this.seriesNames);
        /*
        var data[time] = {
            SeriesName1: value1,
            SeriesName2: value2,
        }
        */

        for (var i = 0; i < seriesDataParts.length; i++) {
            var serieName = seriesDataParts[i];
            var serieData = seriesData[serieName];

            if (this.data[serieData.time] === undefined) {
                this.data[serieData.time] = {};
            }

            this.data[serieData.time][serieName] = serieData.processed;
        }

        this.createWindow();

        this.updateChartFullData(this.data);

        //https://www.chartjs.org/docs/latest/developers/updates.html




    }

}
