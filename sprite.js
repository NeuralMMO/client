import * as textsprite from "./textsprite.js";
export {Overhead}

class Overhead extends THREE.Object3D {
   constructor(params) {
      super()
      this.initName(params);
      this.initStats(params);
   }

   update(params) {
      this.stats.update(params);
   }

   initStats(params) {
      this.stats = new Stats(params);
      this.add(this.stats);
   }

   initName(params) {
      var sprite = textsprite.makeTextSprite(params['name'], "200", params['color']);
      sprite.scale.set( 64, 30, 1 );
      sprite.position.y = 48;
      this.add(sprite);
   }
}

class Stats extends THREE.Object3D {
   constructor(params) {
      super();
      this.barHeight = 8;

      this.health = this.initBar(0x00ff00, params['maxHealth'], 0)
      this.water  = this.initBar(0x0000ff, params['maxWater'], 8)
      this.food   = this.initBar(0xd4af37, params['maxFood'], 16)
   }

   update(params) {
      this.health.update(params['health']);
      this.water.update(params['water']);
      this.food.update(params['food']);
   }

   initBar(color, width, height){
      var bar = new StatBar(color, width, this.barHeight);
      bar.position.y = height
      this.add(bar)
      return bar
   }
}


class StatBar extends THREE.Object3D {
   constructor(color, width, height) {
      super();
      this.valBar = this.initSprite(color);
      this.valBar.center = new THREE.Vector2(1, 0);

      this.redBar = this.initSprite(0xff0000);
      this.redBar.center = new THREE.Vector2(0, 0);

      this.offset = 64;
      this.height = height;
      this.width = width;
      this.update(width);
   }

   initSprite(hexColor) {
      var material = new THREE.SpriteMaterial({color: hexColor});
      var sprite = new THREE.Sprite(material)
      this.add(sprite)
      return sprite
   }

   update(val) {
      this.valBar.scale.set(val, this.height, 1);
      this.redBar.scale.set(this.width - val, this.height, 1);
   }
}

