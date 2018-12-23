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

async def time(websocket, path):
    socket = SerialSocket(websocket)
    while True:
        print(data)
        await socket.send(data)
        targ = await socket.recv()
        data['pos'] = move(data['pos'], targ['pos'])
        await asyncio.sleep(0.6)

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

data = {'pos':(0, 0)}
socket = WebSocketServer(time, 'localhost', 8001)
