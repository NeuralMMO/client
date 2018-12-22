#!/usr/bin/env python

# WS server that sends messages at random intervals

from pdb import set_trace as T
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
    return (ro + int(dr>0), co + int(dc>0))

async def time(websocket, path):
    while True:
        #now = datetime.datetime.utcnow().isoformat() + 'Z'
        packet = json.dumps(data)
        await websocket.send(packet)
        targ = await websocket.recv()
        targ = json.loads(targ)
        data['pos'] = move(data['pos'], targ['pos'])
        print(data['pos'])
        await asyncio.sleep(0.6)

data = {'pos':(0, 0)}

start_server = websockets.serve(time, 'localhost', 8001)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()
