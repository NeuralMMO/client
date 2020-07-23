const kAddress = "127.0.0.1";

export class ChannelClient {
    route;
    ws;
    isBinary;
    messageHandler;
    isOpened;
    clientId;

    constructor(route, isBinary, port, messageHandler) {
        this.route = route;
        this.isBinary = isBinary;
        this.messageHandler = messageHandler;
        this.isOpened = false;

        let connectTo = `ws://${kAddress}:${port}/${route}`;
        this.ws = new WebSocket(connectTo);
        this.ws.addEventListener("open", this.onOpen.bind(this));
        this.ws.addEventListener("close", this.onClose.bind(this));
        this.ws.addEventListener("error", this.onError.bind(this));
        this.ws.addEventListener("message", this.onMessage.bind(this));

        if (this.isBinary)
            this.ws.binaryType = "arraybuffer";
    }

    onOpen(ev) {
        console.log(`[${this.route}] Connected ${this.binaryStatus()}`);
    }

    onClose(ev) {
        console.log(`[${this.route}] Closed ${this.binaryStatus()}`);
        this.close();
    }

    onError(ev) {
        console.error(`[${this.route}] Error: ${ev}`);
        this.close();
    }

    onMessage(ev) {
        if (this.isConnected() && this.messageHandler) {
            this.messageHandler(ev)
        } else {
            // The server sends us our connectionId in plain text
            let data = ev["data"];
            this.clientId = parseInt(data);
            this.isOpened = true;
        }
    }

    binaryStatus() {
        return this.isBinary ? "binary" : "";
    }

    send(data) {
        if (!this.isOpened)
            return;
        this.ws.send(data)
    }

    close() {
        this.isOpened = false;
        this.ws.close();
    }

    isConnected() {
        return this.isOpened;
    }
}
