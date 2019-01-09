class PlayerHandler {
   /*
    * The PlayerHandler receives packets from the server containing player
    * information (other players' movements, interactions with our player)
    * and disperses the signals appropriately.
    */

   constructor() {
      this.players = {};
   }

   addPlayer(id) {
      var obj = loadObj( "resources/nn.obj", "resources/nn.mtl" );
      this.players[id] = new Player(obj, id);
      engine.scene.add(obj);
      //this.players.push(player);
   }

   removePlayer( playerIndex ) {
      this.players[playerIndex].remove()
      delete this.players[playerIndex];
   }

   updateData(ents) {
      for (var id in this.players) {
         if (!(id in ents)) {
            this.removePlayer(id);
         }
      }

      for (var id in ents) {
         if (!(id in this.players)) {
            this.addPlayer(id)
         }
         this.players[id].updateData(ents[id]);
      }
   }

   update( delta ) {
      for (var id in this.players) {
         this.players[id].update(delta);
      }
      /*
      for (var i = 1; i < this.numPlayers; i++) {
         this.players[i].moveTo([Math.random() * worldWidth,
               Math.random() * worldDepth]);
      }
      */
   }
}

class Player {
   constructor( obj, index )  {
      this.translateState = false;
      this.translateDir = new THREE.Vector3(0.0, 0.0, 0.0);
      this.moveTarg = [0, 0];
      this.index = index;
      this.height = sz;    // above grass, below mountains
      this.obj = obj

      this.initObj(obj);
      this.overhead = new Overhead( this.obj.position, name);
   }

   initObj(obj) {
      this.obj = obj;
      this.obj.position.y = this.height;
      this.target = obj.position.clone();
   }

   remove() {
      engine.scene.remove(this.obj);
      this.overhead.remove();
   }

   setPos(x, y, z) {
      var pos = new THREE.Vector3(x*sz, this.height, z*sz);
      this.obj.position.copy(pos);
   }

   updateData (packet) {
      var move = packet['pos'];
      console.log("Move: ", move)
      this.moveTo(move);
   }

   update(delta) {
      this.translate( delta );
   }

   //Initialize a translation for the player, send current pos to server
   moveTo( pos ) {
      /*
       */
      var x = pos[0];
      var z = pos[1];

      this.target = new THREE.Vector3(x*sz+sz/2, this.height, z*sz+sz/2);

      // Signal for begin translation
      this.translateState = true;
      this.translateDir = this.target.clone();
      this.translateDir.sub(this.obj.position);

      if (this.index == 0) {
         this.sendMove();
      }
   }

   sendMove() {
      var packet = JSON.stringify({
         "pos" : this.moveTarg
      });
      ws.send(packet);
   }

   translate(delta) {
      if (this.translateState) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);
         this.obj.position.add(movement);
         this.overhead.move(movement);

         var eps = 0.0000001;
         if (this.obj.position.distanceToSquared(this.target) <= eps) {
            // Finish animating, reset
            this.translateState = false;
            this.obj.position.copy(this.target);
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }
}

class TargetPlayer extends Player {

   //Deprecated
   //Resets the camera on me.
   focus() {
      engine.camera.position.add(this.translateDir);
      engine.controls.target.copy(this.obj.position.clone());
   }

   //Translate, but also move the camera at the same time.
   translate(delta) {
      if (this.translateState) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);

         // Move player, then camera
         this.obj.position.add(movement);
         this.overhead.move(movement);
         engine.camera.position.add(movement);

         // Turn the target into the new position of the player
         engine.controls.target.copy(this.obj.position);

         var eps = 0.0000001;
         if (this.obj.position.distanceToSquared(this.target) <= eps) {
            // Finish animating, reset
            this.translateState = false;
            this.obj.position.copy(this.target);
            engine.controls.target.copy(this.obj.position);
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }
}

class Overhead {
   constructor(pos, name) {
      this.position = pos.clone();
      // Health: red
      this.health = this.initSprite(0xff0000, pos.y + 1.1 * sz);
      // Food: gold
      this.food = this.initSprite(0xd4af37, pos.y + 1.2 * sz);
      // Water: blue
      this.water = this.initSprite(0x0000ff, pos.y + 1.3 * sz);

      engine.scene.add(this.health);
      engine.scene.add(this.food);
      engine.scene.add(this.water);

      this.name = this.initName(pos.y + 1.4 * sz, name);
      engine.scene.add(this.name);
   }

   remove() {
      engine.scene.remove(this.health);
      engine.scene.remove(this.food);
      engine.scene.remove(this.water);
      engine.scene.remove(this.name);
   }

   initSprite( colorRGB, height) {
      var sprite = new THREE.Sprite( new THREE.SpriteMaterial( {
         color: colorRGB
      } ) );
      sprite.scale.set( 64, 8, 1 );
      sprite.position.copy(this.position.clone());
      sprite.position.y = height;
      return sprite;
   }

   initName(height, name) {
      var sprite = makeTextSprite(name);
      //sprite.scale.set( 64, 8, 1 );
      sprite.position.copy(this.position.clone());
      sprite.position.y = height;
      return sprite;
   }

   move( movement ) {
      this.position.add(movement);
      this.health.position.add(movement);
      this.food.position.add(movement);
      this.water.position.add(movement);
      this.name.position.add(movement);
   }
}
