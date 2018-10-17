var _uploadID = 0;
function readFileHandler(file, blockSize) {
    this.file = file;
    this.size = file.size;
    this.readBytes = 0;
    this.id = ++_uploadID;
    this.index = 0;
    this.name = file.name;
    this.blockSize = 1024 * 8;
    if (blockSize)
        this.blockSize = blockSize;
    this.pages = parseInt(file.size / this.blockSize);
    if (file.size % this.blockSize > 0)
        this.pages++;
    this.reader = null;
    this.percent = 0;
}

readFileHandler.prototype.completed = function () {
    return this.pages == this.index;
};
readFileHandler.prototype.read = function () {
    var _this = this;
  
    return new Promise(resolve => {
        var length = _this.size - this.readBytes;
        if (length > _this.blockSize)
            length = _this.blockSize;
        if (!_this.reader)
            _this.reader = new FileReader();
        var result;
        _this.reader.onload = function (evt) {
            if (evt.target.readyState == FileReader.DONE) {
                var str = _this.toBase64(evt.target.result);
                result = { Eof: _this.completed(), Data: str, Name: _this.name };
                resolve(result);
            }
            else {
                result = { errCode: 500, name: "load file error!" };
                resolve(result);
            }
        };
        _this.reader.onerror = function (evt) {
            result = { errCode: evt.target.error.errCode, name: evt.target.error.name };
            resolve(result);
        };

        var start = _this.index * _this.blockSize;
        var end = start + length;
        _this.index++;
        _this.readBytes += length;
        var blob = _this.file.slice(start, end);
        _this.reader.readAsArrayBuffer(blob);
        var p = this.index / this.pages * 100;
        this.percent = parseInt(p);
    });
};
readFileHandler.prototype.toBase64 = function (buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
};

