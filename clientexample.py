from twisted.internet import reactor, protocol, task

class MyProtocol(protocol.Protocol):
    def connectionMade(self):
        self.factory.clientConnectionMade(self)
    def connectionLost(self, reason):
        self.factory.clientConnectionLost(self)

class MyFactory(protocol.Factory):
    protocol = MyProtocol
    def __init__(self):
        self.clients = []
        self.lc = task.LoopingCall(self.announce)
        self.lc.start(10)

    def announce(self):
        for client in self.clients:
            client.transport.write("10 seconds has passed\n")

    def clientConnectionMade(self, client):
        self.clients.append(client)

    def clientConnectionLost(self, client):
        self.clients.remove(client)

myfactory = MyFactory()
reactor.listenTCP(9000, myfactory)
reactor.run()
