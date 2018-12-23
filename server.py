#!/usr/bin/env python

# WS server that sends messages at random intervals

from pdb import set_trace as T
import numpy as np
import asyncio
import datetime
import random
import websockets
import json

def move(orig, targ):
    ro, co = orig
    rt, ct = targ
    dr = rt - ro
    dc = ct - co
    r = int(ro + np.sign(dr))
    c = int(co + np.sign(dc))
    return r, c

async def time(websocket, path):
    while True:
        #now = datetime.datetime.utcnow().isoformat() + 'Z'
        packet = json.dumps(data)
        print(packet)
        await websocket.send(packet)
        targ = await websocket.recv()
        targ = json.loads(targ)
        data['pos'] = move(data['pos'], targ['pos'])
        await asyncio.sleep(0.6)

data = {'pos':(0, 0)}

start_server = websockets.serve(time, 'localhost', 8001)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()
