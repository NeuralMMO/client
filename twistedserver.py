from pdb import set_trace as T
import numpy as np

import sys
import json

from twisted.internet import reactor
from twisted.internet.task import LoopingCall
from twisted.python import log
from twisted.web.server import Site
from twisted.web.static import File

from autobahn.twisted.websocket import WebSocketServerFactory, \
    WebSocketServerProtocol

from autobahn.twisted.resource import WebSocketResource

def sign(x):
    return int(np.sign(x))

def move(orig, targ):
    ro, co = orig
    rt, ct = targ
    dr = rt - ro
    dc = ct - co
    if abs(dr) > abs(dc):
        return ro + sign(dr), co
    elif abs(dc) > abs(dr):
        return ro, co + sign(dc)
    else:
        return ro + sign(dr), co + sign(dc)

class EchoServerProtocol(WebSocketServerProtocol):
    def __init__(self):
        super().__init__()
        print("CREATED A SERVER")
        self.frame = 0
        self.packet = {}

    def onOpen(self):
        print("Opened connection to server")
        self.realm = self.factory.realm
        self.frame += 1

        realm = self.realm.envs[0]
        gameMap = realm.env
        ent = {}
        for id, e in realm.desciples.items():
           ent[id] = {'pos': e.client.pos}
        ent[id] = {'pos': (0, 0)}
         
        self.packet['map'] = gameMap.tolist()
        self.packet['ent'] = ent

        data = self.packet
        packet = json.dumps(data).encode('utf8')
        self.sendMessage(packet, False)

        #packet = json.dumps(data).encode('utf8')
        #self.sendMessage(packet, True)

    def onClose(self, wasClean, code=None, reason=None):
        print('Connection closed')

    def onConnect(self, request):
        print("WebSocket connection request: {}".format(request))

    def onMessage(self, packet, isBinary):
        print("Message", packet)

        packet = json.loads(packet)
        #self.sendMessage(payload, isBinary)
        #packet = packet['0']
        realm = self.realm.envs[0]

        pos = packet['pos']
        ent = self.packet['ent']
        ent['0'] = {'pos': move(ent['0']['pos'], pos)}

        ent = {}
        for id, e in realm.desciples.items():
           ent[id] = {'pos': e.client.pos}
        self.packet['ent'] = ent
 

    def sendUpdate(self):
        packet = json.dumps(self.packet).encode('utf8')
        self.sendMessage(packet, False)

    def connectionMade(self):
        super().connectionMade()
        self.factory.clientConnectionMade(self)

    def connectionLost(self, reason):
        super().connectionLost(reason)
        self.factory.clientConnectionLost(self)

class WSServerFactory(WebSocketServerFactory):
    def __init__(self, ip, realm, step):
        super().__init__(ip)
        self.realm, self.step = realm, step
        self.clients = []

        lc = LoopingCall(self.announce)
        lc.start(0.6)

    def announce(self):
        self.step()
        for client in self.clients:
            client.sendUpdate()

    def clientConnectionMade(self, client):
        self.clients.append(client)

    def clientConnectionLost(self, client):
        self.clients.remove(client)

class Application:
    def __init__(self, realm, step):
        self.realm = realm
        #log.startLogging(sys.stdout)
        port = 8080

        #factory = WSServerFactory(u'ws://localhost:'+str(port), realm)
        factory = WSServerFactory(u"ws://127.0.0.1:8080", realm, step)
        #factory = WebSocketServerFactory(u"ws://127.0.0.1:8080")
        factory.protocol = EchoServerProtocol

        resource = WebSocketResource(factory)

        # we server static files under "/" ..
        root = File(".")

        # and our WebSocket server under "/ws" (note that Twisted uses
        # bytes for URIs)
        root.putChild(b"ws", resource)

        # both under one Twisted Web Site
        site = Site(root)

        reactor.listenTCP(port, site)
        reactor.run()


