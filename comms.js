// Technique used from here: https://github.com/openai/neural-mmo/issues/12#issuecomment-472621615
var ws = new WebSocket(((window.location.protocol === "https:") ? "wss://" : "ws://") + window.location.host + "/ws");
var inbox = [], outbox = [];

/*
var messages = document.createElement('ul');
ws.onmessage = function (event) {
    var messages = document.getElementsByTagName('ul')[0],
        message = document.createElement('li'),
        content = document.createTextNode(event.data);
    message.appendChild(content);
    messages.appendChild(message);
};
document.body.appendChild(messages);
*/

function onMessage(event) {
   msg = event.data;
   inbox.push(msg);
   while (inbox.length > 1) {
      inbox.shift();
   }
}
ws.onmessage = onMessage;


