if ( WEBGL.isWebGLAvailable() === false ) {

   document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   document.getElementById( 'container' ).innerHTML = "";

}

var container, stats;

var camera, controls, scene, renderer;

var tick = 0.6;
var translateState = -1;
var translateDir = new THREE.Vector3(0.0, 0.0, 0.0);

var worldWidth = 64, worldDepth = 64;
var worldHalfWidth = worldWidth / 2;
var worldHalfDepth = worldDepth / 2;
var data = generateHeight( worldWidth, worldDepth );

var clock = new THREE.Clock();
var player, mesh, target;

var raycaster = new THREE.Raycaster();
var mouse = new THREE.Vector2();
var sz = 100;
var frame = 0;
var moveTarg = [0, 0];
var engine;

class Engine {
   constructor() {
      this.camera = new THREE.PerspectiveCamera( 60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = getY( worldHalfWidth, worldHalfDepth ) * 100 + 100;
      this.camera.position.z = 5;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );

      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x006666 );

      document.body.appendChild( this.renderer.domElement );
   }

   onWindowResize() {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();

      this.renderer.setSize( window.innerWidth, window.innerHeight );
   }
}

init();
animate();


function init() {
   container = document.getElementById( 'container' );
   engine = new Engine();

   var geometry = new THREE.CubeGeometry(sz, 1, sz);
   var material = new THREE.MeshBasicMaterial( {color: 0xff0000} );
   player = new THREE.Mesh(geometry, material);
   engine.scene.add(player);
   //player.add(camera)

   //var target = new THREE.Object3D();

   controls = new THREE.OrbitControls(engine.camera, container);
   controls.mouseButtons = {
      LEFT: THREE.MOUSE.RIGHT,
      MIDDLE: THREE.MOUSE.MIDDLE,
      RIGHT: THREE.MOUSE.RIGHT
   }
   controls.target.set( 0, 0, 0 );
   //controls.target = player
   controls.enablePan = false;
   controls.minPolarAngle = 0.0001;
   controls.maxPolarAngle = Math.PI / 2.0 - 0.1;
   //controls.target = target
   controls.movementSpeed = 1000;
   controls.lookSpeed = 0.125;
   controls.lookVertical = true;


   // sides
   var matrix = new THREE.Matrix4();

   var pxGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pxGeometry.attributes.uv.array[ 1 ] = 0.5;
   pxGeometry.attributes.uv.array[ 3 ] = 0.5;
   pxGeometry.rotateY( Math.PI / 2 );
   pxGeometry.translate( 50, 0, 0 );

   var nxGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   nxGeometry.attributes.uv.array[ 1 ] = 0.5;
   nxGeometry.attributes.uv.array[ 3 ] = 0.5;
   nxGeometry.rotateY( - Math.PI / 2 );
   nxGeometry.translate( - 50, 0, 0 );

   var pyGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pyGeometry.attributes.uv.array[ 5 ] = 0.5;
   pyGeometry.attributes.uv.array[ 7 ] = 0.5;
   pyGeometry.rotateX( - Math.PI / 2 );
   pyGeometry.translate( 0, 50, 0 );

   var pzGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pzGeometry.attributes.uv.array[ 1 ] = 0.5;
   pzGeometry.attributes.uv.array[ 3 ] = 0.5;
   pzGeometry.translate( 0, 0, 50 );

   var nzGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   nzGeometry.attributes.uv.array[ 1 ] = 0.5;
   nzGeometry.attributes.uv.array[ 3 ] = 0.5;
   nzGeometry.rotateY( Math.PI );
   nzGeometry.translate( 0, 0, - 50 );

   //

   var geometries = [];

   for ( var z = 0; z < worldDepth; z ++ ) {

      for ( var x = 0; x < worldWidth; x ++ ) {

         var h = getY( x, z );

         matrix.makeTranslation(
            x * 100,
            h * 100,
            z * 100,
         );

         var px = getY( x + 1, z );
         var nx = getY( x - 1, z );
         var pz = getY( x, z + 1 );
         var nz = getY( x, z - 1 );

         geometries.push( pyGeometry.clone().applyMatrix( matrix ) );

         if ( ( px !== h && px !== h + 1 ) || x === 0 ) {

            geometries.push( pxGeometry.clone().applyMatrix( matrix ) );

         }

         if ( ( nx !== h && nx !== h + 1 ) || x === worldWidth - 1 ) {

            geometries.push( nxGeometry.clone().applyMatrix( matrix ) );

         }

         if ( ( pz !== h && pz !== h + 1 ) || z === worldDepth - 1 ) {

            geometries.push( pzGeometry.clone().applyMatrix( matrix ) );

         }

         if ( ( nz !== h && nz !== h + 1 ) || z === 0 ) {

            geometries.push( nzGeometry.clone().applyMatrix( matrix ) );

         }

      }

   }

   var geometry = THREE.BufferGeometryUtils.mergeBufferGeometries( geometries );
   geometry.computeBoundingSphere();
   geometry.computeFaceNormals()

   var texture = new THREE.TextureLoader().load( 'three.js/examples/textures/minecraft/atlas.png' );
   texture.magFilter = THREE.NearestFilter;

   mesh = new THREE.Mesh( geometry, new THREE.MeshLambertMaterial( { map: texture, side: THREE.DoubleSide } ) );
   engine.scene.add( mesh );

   var ambientLight = new THREE.AmbientLight( 0xcccccc );
   engine.scene.add( ambientLight );

   var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
   directionalLight.position.set( 1, 1, 0.5 ).normalize();
   engine.scene.add( directionalLight );

   container.innerHTML = "";

   container.appendChild( engine.renderer.domElement );

   stats = new Stats();
   container.appendChild( stats.dom );
   container.addEventListener( 'click', onMouseDown, false );

   window.addEventListener( 'resize', onWindowResize, false );

}



function generateHeight( width, height ) {

   var data = [], perlin = new ImprovedNoise(),
      size = width * height, quality = 2, z = Math.random() * 100;

   for ( var i = 0; i < size; i ++ ) {
      data[i] = 0;
   }

   return data;

}

function getY( x, z ) {

   return ( data[ x + z * worldWidth ] * 0.2 ) | 0;

}

//

function animate() {

   requestAnimationFrame( animate );

   render();
   stats.update();

   while (inbox.length > 0) {
      var packet = inbox.shift();
      packet = JSON.parse(packet);
      var pos = packet.pos;
      console.log('Inbox: ', pos);

      //var x = player.position.x;
      //var z = player.position.z;

      //x = Math.floor(x/sz);
      //z = Math.floor(z/sz);
      var x = pos[0];
      var z = pos[1];
      var clickedSquare = new THREE.Vector3(x*sz, sz/2+0.1, z*sz);
      translateDir = clickedSquare.clone();

      translateState = 0.0;
      translateDir.sub(controls.target0);
      //controls.saveState();
      controls.target0.copy(clickedSquare);

      var x = moveTarg[0];
      var z = moveTarg[1];
      setMove(x, z);

      /*
      var pos = [x, z];
      var packet = JSON.stringify({'pos': pos});
      console.log('Outbox: ', packet);
      ws.send(packet);
      */
   }
}

function setMove(x, z) {
   moveTarg = [x, z];
   var packet = JSON.stringify({'pos': moveTarg});
   console.log("Set Move:", packet);
   ws.send(packet);
}

function translate(delta) {
   if (translateState != -1) {
      var movement = translateDir.clone();
      movement.multiplyScalar(delta / tick);
      player.position.add(movement);
      // this *should* be equivalent
      // controls.object.position.add(movement);
      engine.camera.position.add(movement);
      controls.target.copy(player.position);

      translateState += delta;

      var eps = 0.0000001;
      if (player.position.distanceToSquared(controls.target0) <= eps ||
              translateState >= tick) {
         // Finish animating, reset
         translateState = -1;
         player.position.copy(controls.target0);
         controls.target.copy(player.position);
         translateDir.set(0.0, 0.0, 0.0);
      }
   }
}

function render() {
   /*
    * TODO: fix what happens when another square is clicked before the current
    * animation is finished, or when the camera is rotated/zoomed before it's
    * finished
    * TODO: sometimes the camera rotates itself?
    */
   var delta = clock.getDelta();
   translate(delta)
   controls.update( delta );
   engine.renderer.render( engine.scene, engine.camera );
}

function onMouseDown( event ) {
   /*
    * Sets the translation direction based on the clicked square and toggles
    * state to translate.
    */

   mouse.x = ( event.clientX / engine.renderer.domElement.clientWidth ) * 2 - 1;
   mouse.y = - ( event.clientY / engine.renderer.domElement.clientHeight ) * 2 + 1;
   raycaster.setFromCamera( mouse, engine.camera );

   // See if the ray from the camera into the world hits one of our meshes
   var intersects = raycaster.intersectObject( mesh );

   // Toggle rotation bool for meshes that we clicked
   if ( intersects.length > 0 ) {
      var x = intersects[ 0 ].point.x;
      var z = intersects[ 0 ].point.z;

      x = Math.floor(x/sz);
      z = Math.floor(z/sz);

      setMove(x, z, controls);
   }
}

function onWindowResize() {
   engine.onWindowResize()
  }

