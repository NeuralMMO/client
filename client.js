import * as engineM from './engine.js';
import * as playerM from './player.js';
import * as terrainM from './terrain.js';
import * as countsM from './counts.js';
import * as valuesM from './values.js';

var client, viewer, stats

class Client {
   constructor () {
      this.engine = new engineM.Engine(client_container);
      client_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.init = true;
      this.dom();
   }

   // hook up signals
   dom() {
      function onMouseDown( event ) { this.onMouseDown( event ); }
      client_container.addEventListener( 'click', onMouseDown, false );

      function onWindowResize() { this.engine.onWindowResize(); }
      window.addEventListener( 'resize', onWindowResize, false );
   }
 
   update() {
      var delta = this.engine.clock.getDelta();
      if (inbox.length > 0) {
         // Receive packet, begin translating based on the received position
         var packet = inbox[0];
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.terrain = new terrainM.Terrain(map, this.engine);
         }
         this.terrain.update(packet['map']);
      }
      this.terrain.updateFast();
      this.engine.update(delta);
   }

   onMouseDown(event) {
      //player.moveTarg = engine.raycast(event.clientX, event.clientY);
      //player.sendMove();

      //var pos = this.engine.raycast(event.clientX, event.clientY);
      //this.engine.controls.target.set(pos[0], pos[1], pos[2]);
   }
}

class Viewer {
   constructor (client) {
      this.engine = new engineM.Engine(viewer_container);
      viewer_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;
   }

   update() {
      var delta = this.engine.clock.getDelta();
      if (inbox.length > 0) {
         // Receive packet, begin translating based on the received position
         var packet = inbox[0];
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.counts = new countsM.Counts(
                  packet['map'], packet['counts'], this.engine);
            this.values = new valuesM.Values(
                  packet['map'], packet['values'], this.engine);
            this.terrain = new terrainM.Terrain(map, this.engine);
 
         }
         this.counts.update(packet['map'], packet['counts']);
         this.values.update(packet['map'], packet['values']);
         this.terrain.update(packet['map']);
      }
      this.counts.updateFast();
      this.values.updateFast();
      this.terrain.updateFast();
      this.engine.update(delta);
   }
}

function webglError() {
   if ( WEBGL.isWebGLAvailable() === false ) {
      document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   }
}

function init() {
   webglError()
   client = new Client();
   viewer = new Viewer(client);
   stats  = new Stats();
   client_container.appendChild(stats.dom);
   animate();
}

function animate() {
   requestAnimationFrame( animate );
   client.update();
   viewer.update();
   stats.update();
}


// Main
init();
