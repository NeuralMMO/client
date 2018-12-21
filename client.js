var socket = new WebSocket("ws://localhost:8000");
socket.onopen = function () {
    alert("alerting you");
    socket.send('Pingel');
};
