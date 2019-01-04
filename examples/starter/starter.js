class Engine {
   constructor() {
      this.scene = new THREE.Scene();
      this.camera = new THREE.PerspectiveCamera( 75, window.innerWidth/window.innerHeight, 0.1, 1000 );
      this.renderer = new THREE.WebGLRenderer();
      this.renderer.setSize( window.innerWidth, window.innerHeight );
      document.body.appendChild( this.renderer.domElement );
      this.camera.position.z = 5;
   }
}

class App {
   constructor() {
      this.engine = new Engine();
      this.setupScene();
      this.t = 0;
      this.animate();
   }

   setupScene() {
      this.cube = new Cube();
      this.engine.scene.add(this.cube);
   }

   animate() {
         this.cube.rotation.x += 0.01;
         this.cube.rotation.y += 0.01;
         this.t += .01;
         this.cube.position.x = 2*Math.cos(this.t);
         this.cube.position.y = 2*Math.sin(this.t);
         this.engine.renderer.render( this.engine.scene, this.engine.camera );
         requestAnimationFrame(()=>this.animate());
   }
}

var Cube = function() {
   var geometry = new THREE.BoxGeometry(1, 1, 1);
   var material = new THREE.MeshBasicMaterial({color: 0xff0000});
   return new THREE.Mesh(geometry, material);
}


var app = new App()
