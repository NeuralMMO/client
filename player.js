import * as textsprite from "./textsprite.js";
import * as OBJ from "./obj.js";
import * as Animation from "./animation.js";
import * as Sprite from "./sprite.js";

export {Player, PlayerHandler};


class PlayerHandler {
   /*
    * The PlayerHandler receives packets from the server containing player
    * information (other players' movements, interactions with our player)
    * and disperses the signals appropriately.
    */

   constructor(engine) {
      this.players = {};
      this.engine = engine;
      this.load();
      this.finishedLoading = false;
   }

   load() {
      this.nnObjs = {}
      var promises = [];
      var scope = this;

      for (var name in Neon) {
         var color = Neon[name];
         var loadedPromise = OBJ.loadNN(color);

         loadedPromise.then( function (result) {
            scope.nnObjs[result.myColor] = result.obj;
         });

         promises.push(loadedPromise);
      }

      Promise.all(promises).then(function () {
         scope.finishedLoading = true;
         console.log("PlayerHandler: Finished loading all meshes.");
      });
   }

   addPlayer(id, params) {
      var player = new Player(this, id, params)
      this.players[id] = player;
      this.engine.scene.add(player);
   }

   removePlayer( playerIndex ) {
      this.engine.scene.remove(this.players[playerIndex])
      delete this.players[playerIndex];
   }

   updateFast() {
      for (var id in this.players) {
         this.players[id].updateFast();
      }
   }

   updateData(ents) {
      if (!this.finishedLoading) {
         return;
      }

      for (var id in this.players) {
         if (!(id in ents)) {
            this.removePlayer(id);
         }
      }

      for (var id in ents) {
         if (!(id in this.players)) {
            this.addPlayer(id, ents[id])
         }
         this.players[id].updateData(this.engine, ents[id], this.players);
      }
   }
}

class Player extends THREE.Object3D {

   constructor(handler, params)  {
      super();
      this.userData = {
         translateState : false,
         translateDir : new THREE.Vector3(0.0, 0.0, 0.0),
         moveTarg : [0, 0],
         height : sz,  // above grass, below mountains
         entID : params['entID'],
         engine : handler.engine,
         target : null,
         color : null,
         obj : null,
         anims : [],
         handler : handler,
         params : params
      }
      this.initObj(params, handler);
      this.initOverhead(params);
   }

   clone() {
      var copy = super.clone();
      return new Player(this.userData.handler, this.userData.params);
   }

   initObj(params, handler) {
      var pos = params['pos'];
      this.userData.color = params['color']
      this.userData.obj = handler.nnObjs[this.userData.color].clone();
      this.userData.obj.position.y = this.userData.height;
      this.userData.obj.position.copy(this.coords(pos));
      this.userData.target = this.userData.obj.position.clone();
      this.add(this.userData.obj)
   }

   initOverhead(params) {
      this.userData.overhead = new Sprite.Overhead(params, this.userData.engine);
      this.userData.obj.add(this.userData.overhead)
      this.userData.overhead.position.y = sz;
   }

   //Format: pos = (r, c)
   coords(pos) {
      return new THREE.Vector3(
            pos[1]*sz+sz+sz/2, this.userData.height, pos[0]*sz+sz+sz/2);
   }

   cancelAnims() {
      for (var anim in this.userData.anims) {
         this.userData.anims[anim].cancel()
      }
   }

   updateData(engine, packet, players) {
      this.cancelAnims();

      var move = packet['pos'];
      //console.log("Move: ", move)
      this.userData.anims.push(new Animation.Move(this, move));

      var damage = packet['damage'];
      if (damage != null) {
         this.userData.anims.push(new Animation.Damage(this, packet['damage']));
      }

      this.userData.overhead.update(packet)

      var targ = packet['target'];
      if (targ != null) {
         var targID = parseInt(targ, 10);
         if (this.userData.entID != targID && targID in players) {
            var attk;
            switch (packet['attack']) {
               case 'Melee':
                  attk = new Animation.Melee(
                        engine.scene, this, players[targID]);
                  break;
               case 'Range':
                  attk = new Animation.Range(
                        engine.scene, this, players[targID]);
                  break;
               case 'Mage':
                  attk = new Animation.Mage(
                        engine.scene, this, players[targID]);
                  break;
            }
            this.userData.anims.push(attk);
         }
      }
   }

   updateFast() {
      this.userData.overhead.updateFast()
   }

   sendMove() {
      var packet = JSON.stringify({
         "pos" : this.userData.moveTarg
      });
      ws.send(packet);
   }
}

