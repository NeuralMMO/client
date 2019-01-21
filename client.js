import * as engineM from './engine.js';
import * as playerM from './player.js';
import * as terrainM from './terrain.js';
import * as countsM from './counts.js';
import * as valuesM from './values.js';
import * as entityM from './entitybox.js';

var client, counts, values, stats;


class Client {
   constructor (client_container) {
      this.engine = new engineM.Engine(modes.ADMIN, client_container);
      this.handler = new playerM.PlayerHandler(this.engine);
      this.init = true;
      this.packet = null;
   }

   // hook up signals
   setupSignals() {
      client_container.innerHTML = ""; // get rid of the text after loading
      client_container.appendChild( this.engine.renderer.domElement );

      var scope = this; // javascript quirk... don't touch this
      function onMouseDown( event ) { scope.onMouseDown( event ); }
      function onWindowResize() { scope.onWindowResize(); }

      client_container.addEventListener( 'click', onMouseDown, false );
      window.addEventListener( 'resize', onWindowResize, false );
   }

   updatePacket() {
      if (inbox.length > 0) {
         this.packet = inbox.pop();
      } else {
         this.packet = null;
      }
   }

   update() {
      var delta = this.engine.clock.getDelta();
      this.updatePacket();

      if (this.packet) {
         // Receive packet, begin translating based on the received position
         var packet = JSON.parse(this.packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.terrain = new terrainM.Terrain(map, this.engine);
         }
         this.terrain.update(packet['map']);
      }
      //this.terrain.updateFast();
      this.engine.update(delta);
   }

   onWindowResize () {
      this.engine.onWindowResize();
   }

   onMouseDown(event) {
      var entityBox = document.getElementById( 'entityBox' );
      var box = new entityM.EntityBox(color);

      // hook up signals
      entityBox.innerHTML = "";
      entityBox.appendChild( box.renderer.domElement );

      //var pos = this.engine.raycast(event.clientX, event.clientY);
      //this.engine.controls.target.set(pos[0], pos[1], pos[2]);
   }
}

class Counts {
   constructor (client, viewer_container) {
      this.engine = new engineM.Engine(modes.ADMIN, viewer_container);
      viewer_container.innerHTML = ""; // get rid of the text after loading
      viewer_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;
   }

   update() {
      var delta = this.engine.clock.getDelta();
      if (this.client.packet) {
         // Receive packet, begin translating based on the received position
         var packet = this.client.packet;
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.counts = new countsM.Counts(
                  packet['map'], packet['counts'], this.engine);
 
         }
         this.counts.update(packet['map'], packet['counts']);
      }
      this.counts.updateFast();
      this.engine.update(delta);
   }
}

class Values{
   constructor (client, viewer_container) {
      this.engine = new engineM.Engine(modes.ADMIN, viewer_container);
      viewer_container.innerHTML = ""; // get rid of the text after loading
      viewer_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;
   }

   update() {
      var delta = this.engine.clock.getDelta();
      if (this.client.packet) {
         // Receive packet, begin translating based on the received position
         var packet = this.client.packet;
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.values = new valuesM.Values(
                  packet['map'], packet['values'], this.engine);
         }
         this.values.update(packet['map'], packet['values']);
      }
      this.values.updateFast();
      this.engine.update(delta);
   }
}


function webglError() {
   if ( WEBGL.isWebGLAvailable() === false ) {
      document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   }
}

function init() {
   webglError();
   var client_container = document.getElementById("client_container");
   var viewer_container = document.getElementById("viewer_container");

   client = new Client(client_container);
   counts = new Counts(client, counts_container);
   values = new Values(client, values_container);
   stats  = new Stats();
   client.setupSignals();
   client_container.appendChild(stats.dom);

   var blocker = document.getElementById("blocker");
   var instructions = document.getElementById("instructions");
   instructions.addEventListener("click", function() {
	   client.engine.controls.enabled = true;
	   client.engine.controls.update();
	   counts.engine.controls.enabled = true;
	   counts.engine.controls.update();
	   values.engine.controls.enabled = true;
	   values.engine.controls.update();
	   instructions.style.display = "none";
      blocker.style.display = "none";
   }, false);

   animate();
}

function animate() {
   requestAnimationFrame( animate );
   client.update();
   counts.update();
   values.update();
   stats.update();
}


// Main
init();
