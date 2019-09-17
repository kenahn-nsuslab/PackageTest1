"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const { gzip, ungzip } = require('node-gzip');
const int24 = require('int24');
class SocketManager {
    constructor(wss) {
        this.salt = 0;
        this.packetIdSeq = 0;
        this.magicKey = 0xFEFE;
        this.serialMode = 0;
        this.headerSize = 16;
        this.wss = wss;
    }
    ;
    setClient(ws) {
        ws.binaryType = "arrayBuffer";
        ws.onmessage = this.messageHandle;
        ws.on('close', () => {
            console.log("websocket is disconnected");
        });
    }
    messageHandle(event) {
        var header = event.data.slice(0, 16);
        var payload = event.data.slice(16, event.data.length);
        var temp = payload.toString('utf8');
        var headerObject = {
            "salt": header.readUInt16LE(0),
            "protocolId": header.readUInt16LE(6),
            "packetId": int24.readUInt24LE(header, 8)
        };
        console.log("receved data : " + temp);
        console.log("header :" + JSON.stringify(headerObject));
    }
    SendMessage2SocketIO(msg) {
        if (this.user)
            this.user.emit("info", msg);
    }
    setUserSocket(socket) {
        this.user = socket;
        this.user.on("command", (data) => {
            let message = JSON.stringify(data);
            console.log("command received :" + message);
            this.broadcastToClients(message);
        });
        this.user.on("protocol", (data) => {
            let message = JSON.stringify(data);
            console.log("protocol received :" + message);
            this.broadcastProtocolToClients(data);
        });
    }
    broadcastProtocolToClients(data) {
        let databuffer = this.serialize(data.protocolid, data.payload);
        this.wss.clients.forEach(function (socket, index, array) {
            socket.send(databuffer);
        });
    }
    broadcastToClients(message) {
        this.clients.forEach(function (socket, index, array) {
            socket.write(message);
        });
    }
    serialize(protocolId, dto) {
        var payloadString = JSON.stringify(dto);
        ;
        let payloadBuffer = Buffer.from(payloadString);
        let dataBuffer = Buffer.alloc(this.headerSize + payloadBuffer.length);
        dataBuffer.writeUInt16LE(this.salt, 0, true);
        dataBuffer.writeUInt16LE(this.magicKey, 2, true);
        dataBuffer.writeUInt16LE(payloadBuffer.length, 4, true);
        dataBuffer.writeUInt16LE(protocolId, 6, true);
        int24.writeUInt24LE(dataBuffer, 8, this.packetIdSeq++);
        dataBuffer.writeUInt8(this.serialMode, 11, true);
        int24.writeUInt24LE(dataBuffer, 12, 0);
        payloadBuffer.copy(dataBuffer, this.headerSize);
        return dataBuffer;
    }
    binarize(protocolId, jsonString) {
        let payloadBuffer = Buffer.from(jsonString);
        let dataBuffer = Buffer.alloc(this.headerSize + payloadBuffer.length);
        dataBuffer.writeUInt16LE(this.salt, 0, true);
        dataBuffer.writeUInt16LE(this.magicKey, 2, true);
        dataBuffer.writeUInt16LE(payloadBuffer.length, 4, true);
        dataBuffer.writeUInt16LE(protocolId, 6, true);
        int24.writeUInt24LE(dataBuffer, 8, this.packetIdSeq++);
        dataBuffer.writeUInt8(this.serialMode, 11, true);
        int24.writeUInt24LE(dataBuffer, 12, 0);
        payloadBuffer.copy(dataBuffer, this.headerSize);
        return dataBuffer;
    }
    stringToUint8Array(s) {
        let i = 0;
        let bytes = new Uint8Array(s.length * 4);
        for (let ci = 0; ci != s.length; ci++) {
            let c = s.charCodeAt(ci);
            if (c < 128) {
                bytes[i++] = c;
                continue;
            }
            if (c < 2048) {
                bytes[i++] = c >> 6 | 192;
            }
            else {
                if (c > 0xd7ff && c < 0xdc00) {
                    if (++ci == s.length)
                        throw 'UTF-8 encode: incomplete surrogate pair';
                    let c2 = s.charCodeAt(ci);
                    if (c2 < 0xdc00 || c2 > 0xdfff)
                        throw 'UTF-8 encode: second char code 0x' + c2.toString(16) + ' at index ' + ci + ' in surrogate pair out of range';
                    c = 0x10000 + ((c & 0x03ff) << 10) + (c2 & 0x03ff);
                    bytes[i++] = c >> 18 | 240;
                    bytes[i++] = c >> 12 & 63 | 128;
                }
                else {
                    bytes[i++] = c >> 12 | 224;
                }
                bytes[i++] = c >> 6 & 63 | 128;
            }
            bytes[i++] = c & 63 | 128;
        }
        return bytes.subarray(0, i);
    }
    uint8ArrayToString(bytes) {
        let arr = [];
        let i = 0;
        while (i < bytes.length) {
            let c = bytes[i++];
            if (c > 127) {
                if (c > 191 && c < 224) {
                    if (i >= bytes.length)
                        throw 'UTF-8 decode: incomplete 2-byte sequence';
                    c = (c & 31) << 6 | bytes[i] & 63;
                }
                else if (c > 223 && c < 240) {
                    if (i + 1 >= bytes.length)
                        throw 'UTF-8 decode: incomplete 3-byte sequence';
                    c = (c & 15) << 12 | (bytes[i] & 63) << 6 | bytes[++i] & 63;
                }
                else if (c > 239 && c < 248) {
                    if (i + 2 >= bytes.length)
                        throw 'UTF-8 decode: incomplete 4-byte sequence';
                    c = (c & 7) << 18 | (bytes[i] & 63) << 12 | (bytes[++i] & 63) << 6 | bytes[++i] & 63;
                }
                else
                    throw 'UTF-8 decode: unknown multibyte start 0x' + c.toString(16) + ' at index ' + (i - 1);
                ++i;
            }
            if (c <= 0xffff)
                arr.push(String.fromCharCode(c));
            else if (c <= 0x10ffff) {
                c -= 0x10000;
                arr.push(String.fromCharCode(c >> 10 | 0xd800));
                arr.push(String.fromCharCode(c & 0x3FF | 0xdc00));
            }
            else
                throw 'UTF-8 decode: code point 0x' + c.toString(16) + ' exceeds UTF-16 reach';
        }
        return arr.join('');
    }
}
exports.SocketManager = SocketManager;
//# sourceMappingURL=socketManager.js.map