#!/usr/bin/env python

# WS server that sends messages at random intervals

from pdb import set_trace as T
import numpy as np
import asyncio
import datetime
import random
import websockets
import json

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

class SerialSocket:
    def __init__(self, socket):
        self.socket = socket

    async def send(self, packet):
        packet = json.dumps(packet)
        await self.socket.send(packet)

    async def recv(self):
        packet = await self.socket.recv()
        return json.loads(packet)

class WebSocketServer:
    def __init__(self, func, host, port):
        socket = websockets.serve(func, host, port)
        asyncio.get_event_loop().run_until_complete(socket)
        asyncio.get_event_loop().run_forever()

class Tick:
    def __init__(self, realm):
        self.realm = realm
        self.frame = 0
        self.socket = None
 
    async def __call__(self, websocket, path):
        self.frame += 1
        print('Here')
        if self.socket is None:
           self.socket = SerialSocket(websocket)

        env = self.realm.envs[0]
        data = {'pos': (0, 0)}
        if len(env.desciples) > 0:
            idx = min(int(e) for e in env.desciples.keys())
            ent = env.desciples[str(idx)]
            pos = ent.client.pos
            data = {'pos': pos}

        socket = self.socket
        await socket.send(data)
        #targ = await socket.recv()
        #data['pos'] = move(data['pos'], targ['pos'])
        await asyncio.sleep(0.6)

class Application:
    def __init__(self, realm):
        #app = server.Application((2048+256, 1024+256),
        #        self, step, self.config)
        self.socket = WebSocketServer(Tick(realm), 'localhost', 8001)
        #        Tick.__call__, 'localhost', 8001)

data = {'pos':(0, 0)}
#socket = WebSocketServer(
#        Tick(None, None, None, None), 'localhost', 8001)


