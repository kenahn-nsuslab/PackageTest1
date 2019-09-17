"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const http_1 = require("http");
const express = require("express");
const socketIo = require("socket.io");
const socketManager_1 = require("./socketManager");
const WebSocket = require("ws");
class AppServer {
    constructor() {
        this.config();
        this.createServer();
        this.listen();
    }
    createApp() {
        this.app = express();
    }
    createServer() {
        this.ioWebServer = http_1.createServer(this.app);
        this.wsWebServer = http_1.createServer(express());
        this.io = socketIo(this.ioWebServer);
        const server = this.wsWebServer;
        this.wsServer = new WebSocket.Server({ server });
        this.manager = new socketManager_1.SocketManager(this.wsServer);
    }
    config() {
        this.port = process.env.PORT || AppServer.PORT;
        this.wsPort = process.env.WSPORT || AppServer.WSPORT;
    }
    listen() {
        this.io.on('connect', (socket) => {
            console.log('Connected socket.io client on port %s.', this.port);
            socket.on('disconnect', () => {
                console.log('socket.io Client disconnected');
            });
            if (this.manager.user == null || this.manager.user.disconnected) {
                this.manager.setUserSocket(socket);
                socket.emit("info", "you are the controller");
            }
            else {
                socket.emit("info", "already connected by other user");
                socket.disconnect();
            }
        });
        this.wsServer.on('error', function (error) {
            console.error(JSON.stringify(error));
        });
        this.wsServer.on('connection', (ws) => {
            console.log('websocket client is connected');
            this.manager.setClient(ws);
        });
        this.ioWebServer.listen(this.port, () => {
            console.log('Running socket.io server on port %s', this.port);
        });
        this.wsWebServer.listen(this.wsPort, () => {
            console.log('Running websocket server on port %s', this.wsPort);
        });
    }
    getApp() {
        return this.app;
    }
}
AppServer.PORT = 3000;
AppServer.WSPORT = 3001;
exports.AppServer = AppServer;
//# sourceMappingURL=appServer.js.map