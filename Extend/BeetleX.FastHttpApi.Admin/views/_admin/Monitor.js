function MonitorPoint() {
    this.x = 0;
    this.y = 0;
    this.value = 0;
}
function MonitorItem(height, width) {
    this.maxValue = 100;
    this.changeMaxValue = false;
    this.label = "";
    this.items = new Array();
    this.strokeStyle = "chartreuse";
    this.height = height;
    this.width = width;
    this.lastValue;
    wlen = width / 200;
    for (i = 0; i <= 200; i++) {
        var p = new MonitorPoint();
        p.x = i * wlen;
        p.y = this.height;
        this.items.push(p);
    }

}
MonitorItem.prototype.push = function (value) {
    var length = this.items.length - 1;
    for (i = 0; i < length; i++) {
        var l = this.items[i];
        var c = this.items[i + 1];
        if (c && l)
            l.y = c.y;
    }
    this.lastValue = value;
    var p = this.items[this.items.length - 1];
    if (value >= this.maxValue) {
        p.value = this.maxValue - 4;
    }
    else {
        p.value = value;
    }
    var pe = this.height / this.maxValue;
    p.y = this.height - (p.value * pe);

}

MonitorItem.prototype.draw = function (context) {
    context.beginPath();
    var first = this.items[0];
    context.moveTo(first.x, first.y);
    for (i = 0; i < this.items.length; i++) {
        var p = this.items[i];
        context.lineTo(p.x, p.y);
    }
    context.fillStyle = ''
    if (this.lastValue >= this.maxValue)
        context.strokeStyle = "orangered";
    else
        context.strokeStyle = this.strokeStyle;
    context.stroke();
}

function Monitor(canvas) {
    this.colors = ["chartreuse", "aqua", "blueviolet", "darkorange", "deepskyblue", "gold"];
    this.canvas = document.getElementById(canvas);
    this.context = this.canvas.getContext('2d');
    this.items = new Array();
}
Monitor.prototype.draw = function () {
    this.clear();
    var _this = this;
    this.items.forEach(function (v, i) {
        v.draw(_this.context);
    });
}
Monitor.prototype.create = function () {
    var item = new MonitorItem(this.canvas.height, this.canvas.width);
    item.strokeStyle = this.colors.shift();
    this.items.push(item);
    return item;
}
Monitor.prototype.clear = function () {
    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    this.context.fillStyle = "#000000";
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
    var y = 0;
    while (y < this.canvas.height) {
        this.context.beginPath();
        this.context.moveTo(0, y);
        this.context.lineTo(this.canvas.width, y);
        this.context.lineWidth = 1;

        // set line color
        this.context.strokeStyle = 'darkgreen';
        this.context.stroke();
        y += 12;
    }
    var x = 0;
    while (x < this.canvas.width) {
        this.context.beginPath();
        this.context.moveTo(x, 0);
        this.context.lineTo(x, this.canvas.height);
        this.context.lineWidth = 1;
        // set line color
        this.context.strokeStyle = 'darkgreen';
        this.context.stroke();
        x += 12;
    }
    var _ths = this;
    this.items.forEach(function (v, i) {

        _ths.context.font = '12px Sans-serif';
        if (v.lastValue > v.maxValue)
            _ths.context.fillStyle = 'orangered';
        else
            _ths.context.fillStyle = v.strokeStyle
        _ths.context.fillText(v.label + " " + v.lastValue + '/' + v.maxValue, 10, i * 14 + 14);
    });

}
