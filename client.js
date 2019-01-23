import * as engineM from './engine.js';
import * as playerM from './player.js';
import * as terrainM from './terrain.js';
import * as countsM from './counts.js';
import * as valuesM from './values.js';
import * as entityM from './entitybox.js';
import * as textsprite from './textsprite.js';

var client, counts, values, stats, box;
var CURRENT_VIEW = views.CLIENT;

class AbstractClient {
   // interface for client, viewer, and counts
   constructor (client, my_container) {
      this.engine = new engineM.Engine(modes.ADMIN, my_container);
      my_container.innerHTML = ""; // get rid of the text after loading
      my_container.appendChild( this.engine.renderer.domElement );

      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;

      var scope = this; // javascript quirk... don't touch this
      function onMouseDown( event ) { scope.onMouseDown( event ); }
      my_container.addEventListener( 'click', onMouseDown, false );
   }

   onMouseDown(event) {
      // optional
   }

   update () {
      throw new Error("Must override abstract update method of AbstractClient.");
   }

   onWindowResize () {
      this.engine.onWindowResize();
   }

}


class Client extends AbstractClient {
   constructor (my_container) {
      super(null, my_container);
      this.packet = null;
      this.frame = 0;
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
         this.frame += 1;
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.terrain = new terrainM.Terrain(map, this.engine);

            /*
            var pkt1 = {
               'pos': [18, 11],
               'entID': 394,
               'color':'#ff8000',
               'name': 'Neural_394',
               'food':25, 
               'water':12,
               'health':16,
               'maxFood':32,
               'maxWater':32,
               'maxHealth':32,
               'damage':2,
            }

            var pkt2 = {
               'pos': [19, 13],
               'entID': 383,
               'color':'#8000ff',
               'name': 'Neural_383',
               'food':8, 
               'water':28,
               'health':23,
               'maxFood':32,
               'maxWater':32,
               'maxHealth':32,
               'damage':0,
               'attack':'Range',
               'target':'394',
            }
        
            var player1 = new playerM.Player(this.handler, 0, pkt1)
            this.engine.scene.add(player1);
            player1.updateData(this.engine, pkt1, {})
            this.player1 = player1

            var player2 = new playerM.Player(this.handler, 0, pkt2)
            this.engine.scene.add(player2);
            player2.updateData(this.engine, pkt2, {394: player1})
            this.player2 = player2

            var attkGeom = new THREE.IcosahedronGeometry(10);
            var attkMatl = new THREE.MeshBasicMaterial({color: '#0000ff'});
            var attkMesh = new THREE.Mesh(attkGeom, attkMatl);

            var p2 = player2.obj.position
            var p1 = player1.obj.position
            var moveFrac = 0.75
            var y = 96;

            var x = p1.x + moveFrac * (p2.x - p1.x) + 16;
            var z = p1.z + moveFrac * (p2.z - p1.z) + 16;
            var pos = new THREE.Vector3(x, y, z)
            attkMesh.position.x = x
            attkMesh.position.y = y
            attkMesh.position.z = z
            this.attkMesh = attkMesh
            this.engine.scene.add(attkMesh);

            var dmg = textsprite.makeTextSprite(2, "200", '#ff0000');
            dmg.scale.set( 10, 30, 1 );
            dmg.position.x = p2.x
            dmg.position.y = p2.y + 128
            dmg.position.z = p2.z
            this.dmg = dmg
            this.engine.scene.add(dmg);
            */

         }
         this.terrain.update(packet['map']);
      }
      if (this.terrain) {
         this.terrain.updateFast();
      }
      this.handler.updateFast();
      this.engine.update(delta);
   }

   onMouseDown(event) {
      // handle player event first
      var minDistance = 1000000; // large number
      var minPlayer = null;

      for (var id in this.handler.players) {
         // Player subclasses Object3D
         var player = this.handler.players[id];
         var coords = this.engine.raycast(event.clientX, event.clientY,
                 player);
         if (coords) {
            var distance = this.engine.camera.position.distanceTo(coords);
            if (distance < minDistance) {
               minDistance = distance;
               minPlayer = player;
            }
         }
      }

      if (minPlayer) {
         // now we've identified the closest player
         console.log("Clicked player", minPlayer.clientId);
         if (!box) {
            box = new entityM.EntityBox();
         }
         box.setText("Info: Player #" + minPlayer.clientId);
         box.changeColor(minPlayer.color);

         if (this.engine.mode == modes.SPECTATOR) {
            // follow this player
         }
      }

      // then handle translate event (if self is player)
      if (this.engine.mode == modes.PLAYER) {
         //var pos = this.engine.raycast(event.clientX, event.clientY);
         //this.engine.controls.target.set(pos);
      }
   }
}


class Counts extends AbstractClient {
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
      if (this.counts) {
         this.counts.updateFast();
      }
      this.engine.update(delta);
   }
}


class Values extends AbstractClient {
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
      if (this.values) {
         this.values.updateFast();
      }
      this.engine.update(delta);
   }
}


function webglError() {
   if ( WEBGL.isWebGLAvailable() === false ) {
      document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   }
}

function toggleVisualizers() {
   console.log(CURRENT_VIEW);
   CURRENT_VIEW = (CURRENT_VIEW + 1) % 3;
   displayOnlyCurrent();
}

function displayOnlyCurrent() {
   client_container.style.display = "none";
   values_container.style.display = "none";
   counts_container.style.display = "none";

   // now update the current view
   switch ( CURRENT_VIEW ) {
      case views.CLIENT:
         client_container.style.display = "block";
         break;
      case views.COUNTS:
         counts_container.style.display = "block";
         break;
      case views.VALUES:
         values_container.style.display = "block";
         break;
   }
}

function onKeyDown(event) {
   // TODO: this isn't working
   switch ( event.keyCode ) {
      case 84: // T
         toggleVisualizers();
         break;
   }
}

function onWindowResize() {
   client.onWindowResize();
   if (counts) {counts.onWindowResize();}
   if (values) {values.onWindowResize();}
}

function init() {
   webglError();
   var client_container = document.getElementById("client_container");
   var values_container = document.getElementById("values_container");

   client = new Client(client_container);
   counts = new Counts(client, counts_container);
   values = new Values(client, values_container);

   stats  = new Stats();
   client_container.appendChild(stats.dom);

   // Start by setting these to none
   displayOnlyCurrent();

   var blocker = document.getElementById("blocker");
   var instructions = document.getElementById("instructions");

   instructions.addEventListener("click", function() {
	   client.engine.controls.enabled = true;
	   client.engine.controls.update();

      if (counts) {
	      counts.engine.controls.enabled = true;
	      counts.engine.controls.update();
      }
	   if (values) {
         values.engine.controls.enabled = true;
	      values.engine.controls.update();
      }
	   instructions.style.display = "none";
      blocker.style.display = "none";
   }, false);

   window.addEventListener( 'resize', onWindowResize, false );
   client_container.addEventListener( 'keyDown', onKeyDown, false );

   animate();
}

function animate() {
   requestAnimationFrame( animate );
   client.update();
   if (counts && counts_container.style.display != "none") { counts.update();}
   if (values && values_container.style.display != "none" ) { values.update();}
   if (stats) { stats.update();}
   if (box) { box.update();}
}


// Main
init();
