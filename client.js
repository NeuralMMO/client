if ( WEBGL.isWebGLAvailable() === false ) {

   document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   document.getElementById( 'container' ).innerHTML = "";

}

var container, stats;

var camera, controls, scene, renderer;

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

init();
animate();

function init() {
   container = document.getElementById( 'container' );

   //camera = new THREE.OrthographicCamera( window.innerWidth / - 2, window.innerWidth / 2, window.innerHeight / 2, window.innerHeight / - 2, 1, 20000 );
   camera = new THREE.PerspectiveCamera( 60, window.innerWidth / window.innerHeight, 1, 20000 );
   camera.position.y = getY( worldHalfWidth, worldHalfDepth ) * 100 + 100;

   scene = new THREE.Scene();
   scene.background = new THREE.Color( 0x006666 );

   var geometry = new THREE.CubeGeometry(sz, 1, sz);
   var material = new THREE.MeshBasicMaterial( {color: 0xff0000} );
   player = new THREE.Mesh(geometry, material);
   scene.add(player);
   //player.add(camera)

   //var target = new THREE.Object3D();

   controls = new THREE.OrbitControls(camera, container);
   controls.mouseButtons = {
      LEFT: THREE.MOUSE.RIGHT,
      MIDDLE: THREE.MOUSE.MIDDLE,
      RIGHT: THREE.MOUSE.RIGHT
   }
   controls.target.set( 0, 0, 0 )
   //controls.target = player
   controls.enablePan = false
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
   scene.add( mesh );

   var ambientLight = new THREE.AmbientLight( 0xcccccc );
   scene.add( ambientLight );

   var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
   directionalLight.position.set( 1, 1, 0.5 ).normalize();
   scene.add( directionalLight );

   renderer = new THREE.WebGLRenderer( { antialias: true } );
   renderer.setPixelRatio( window.devicePixelRatio );
   renderer.setSize( window.innerWidth, window.innerHeight );

   container.innerHTML = "";

   container.appendChild( renderer.domElement );

   stats = new Stats();
   container.appendChild( stats.dom );
   container.addEventListener( 'click', onMouseDown, false );

   window.addEventListener( 'resize', onWindowResize, false );

}

function onWindowResize() {

   camera.aspect = window.innerWidth / window.innerHeight;
   camera.updateProjectionMatrix();

   renderer.setSize( window.innerWidth, window.innerHeight );

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
      console.log(pos);

      var xx = player.position.x;
      var zz = player.position.z;

      xx = Math.floor(xx/sz);
      zz = Math.floor(zz/sz);

      var pos = [xx, zz];
      var packet = JSON.stringify({'pos': pos});
      console.log(packet);
      ws.send(packet);
   }

}

function render() {

   controls.update( clock.getDelta() );
   renderer.render( scene, camera );

}

function onMouseDown( event ) {

   mouse.x = ( event.clientX / renderer.domElement.clientWidth ) * 2 - 1;
   mouse.y = - ( event.clientY / renderer.domElement.clientHeight ) * 2 + 1;
   raycaster.setFromCamera( mouse, camera );

   // See if the ray from the camera into the world hits one of our meshes
   var intersects = raycaster.intersectObject( mesh );

   // Toggle rotation bool for meshes that we clicked
   if ( intersects.length > 0 ) {
      var x = intersects[ 0 ].point.x;
      var z = intersects[ 0 ].point.z;

      x = Math.floor(x/sz);
      z = Math.floor(z/sz);

      player.position.x = x*sz;
      player.position.z = z*sz;
      player.position.y = sz/2;

      //controls.target.copy(player)
      //camera.target.position.copy(player)
      //controls.update()
      //controls = new THREE.OrbitControls(camera, container);
      controls.target.copy(player.position)
      //controls.target0(player.position)
      //controls.target.set( player.position.x, player.position.y, player.position.z)
      //controls.target0.set( player.position.x, player.position.y, player.position.z)
      //controls.target.position.copy( player);
      //camera.target.position.copy( player);
   }

}

