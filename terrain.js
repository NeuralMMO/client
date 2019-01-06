//var worldWidth = 256, worldDepth = 256,
//worldHalfWidth = worldWidth / 2, worldHalfDepth = worldDepth / 2;
var width  = 80;
var height = 80;

function terrain(map) {
   this.sz = map.length
   var data = generateHeight(map);

   var geometry = new THREE.PlaneBufferGeometry(
           64*sz, 64*sz, this.sz-1, this.sz-1 );
   geometry.rotateX( - Math.PI / 2 );

   var vertices = geometry.attributes.position.array;

   for ( var i = 0, j = 0, l = vertices.length; i < l; i ++, j += 3 ) {
      vertices[ j + 1 ] = data[ i ] * 64;
   }

   texture = new THREE.CanvasTexture(
           generateTexture( data, this.sz, this.sz) );
   texture.wrapS = THREE.ClampToEdgeWrapping;
   texture.wrapT = THREE.ClampToEdgeWrapping;

   mesh = new THREE.Mesh( geometry,
           new THREE.MeshBasicMaterial( { map: texture } ) );
   return mesh
}

function tile(val) {
   switch (val) {
      case 0:
         return 1.01;
         break;
      case 1:
         return 0;
         break;
      case 2:
         return 1;
         break;
      case 3:
         return 1;
         break;
      case 4:
         return 1;
         break;
      case 5:
         return 2;
         break;
      case 6:
         return 1;
         break;
   }
}

function generateHeight(map) {
   this.sz = map.length;
   var data = new Uint8Array( this.sz*this.sz );
   var k = 0;
   for ( var r = 0; r < this.sz; r ++ ) {
      for ( var c = 0; c < this.sz; c ++ ) {

         var ll = 0.0;
         var tt = 0.0;
         var tl = 0.0;
         var cc = 0.0;

         cc = tile(map[r][c]);
         if (c != 0) {
            ll = tile(map[r][c-1]);
         }
         if (r != 0) {
            tt = tile(map[r-1][c]);
         }
         if (r != 0 && c != 0) {
            tl = tile(map[r-1][c-1]);
         }
         //var val = 0.25*(ll + tt + tl + cc);
         var val = Math.max(ll, tt, tl, cc);
         var mag = 2;
         data[k] = mag * val;
         /*
         data[k] = 1;
         if (ll != 1) {
            data[k] = mag * ll;
         }
         if (tt != 1) {
            data[k] = mag * tt;
         }
         if (cc != 1) {
            data[k] = mag * cc;
         }
         */
         k++;
      }
   }
   return data;
}

function generateFlat(map) {
   this.sz = map.length;
   var data = new Uint8Array( this.sz*this.sz );
   var k = 0;
   for ( var r = 0; r < this.sz; r ++ ) {
      for ( var c = 0; c < this.sz; c ++ ) {
         data[k] = map[r][c];
         k++;
      }
   }
   return data;
}


function generateTexture( data, map, width, height ) {

   var canvas, canvasScaled, context, image, imageData, vector3, sun, shade;

   vector3 = new THREE.Vector3( 0, 0, 0 );

   sun = new THREE.Vector3( 1, 1, 1 );
   sun.normalize();

   canvas = document.createElement( 'canvas' );
   canvas.width = width;
   canvas.height = height;

   context = canvas.getContext( '2d' );
   context.fillStyle = '#000';
   context.fillRect( 0, 0, width, height );

   image = context.getImageData( 0, 0, canvas.width, canvas.height );
   imageData = image.data;

   for ( var i = 0, j = 0, l = imageData.length; i < l; i += 4, j ++ ) {

      vector3.x = data[ j - 2 ] - data[ j + 2 ];
      vector3.y = 2;
      vector3.z = data[ j - width * 2 ] - data[ j + width * 2 ];
      vector3.normalize();
      //if (isnan(vector3)) {
      //   continue;
      //}
      //var tx = Math.floor(vector3.x / 64);
      //var tz = Math.floor(vector3.z / 64);
      //var val = tile(map[tx][tz])
      var val = 0;


      shade = vector3.dot( sun );

      imageData[ i ] = ( 96 + shade * 128 ) * ( 0.5 + val * 0.007 );
      imageData[ i + 1 ] = ( 32 + shade * 96 ) * ( 0.5 + val * 0.007 );
      imageData[ i + 2 ] = ( shade * 96 ) * ( 0.5 + val * 0.007 );

      /*
      imageData[ i ] = ( 96 + shade * 128 ) * ( 0.5 + data[ j ] * 0.007 );
      imageData[ i + 1 ] = ( 32 + shade * 96 ) * ( 0.5 + data[ j ] * 0.007 );
      imageData[ i + 2 ] = ( shade * 96 ) * ( 0.5 + data[ j ] * 0.007 );
      */


   }

   context.putImageData( image, 0, 0 );

   // Scaled 4x

   canvasScaled = document.createElement( 'canvas' );
   canvasScaled.width = width * 4;
   canvasScaled.height = height * 4;

   context = canvasScaled.getContext( '2d' );
   context.scale( 4, 4 );
   context.drawImage( canvas, 0, 0 );

   image = context.getImageData( 0, 0, canvasScaled.width, canvasScaled.height );
   imageData = image.data;

   for ( var i = 0, l = imageData.length; i < l; i += 4 ) {

      var v = ~ ~ ( Math.random() * 5 );

      imageData[ i ] += v;
      imageData[ i + 1 ] += v;
      imageData[ i + 2 ] += v;

   }

   context.putImageData( image, 0, 0 );

   return canvasScaled;

}


function addTerrain(map) {
    /*
     * Adds terrain which shades water, grass, dirt, and mountains
     * based on a heightmap given by the server.
     */
   var tileSz = 64;
   var nTiles = map.length;

   // LIGHT
	var light = new THREE.PointLight(0xffffff);
	light.position.set(100,250,100);
	engine.scene.add(light);

    var loader = new THREE.TextureLoader();

	// SKYBOX
	var skyBoxGeometry = new THREE.CubeGeometry( 20000, 20000, 10000 );
	var skyBoxMaterial = new THREE.MeshBasicMaterial( {
        color: 0x9999ff, side: THREE.BackSide } );
	var skyBox = new THREE.Mesh( skyBoxGeometry, skyBoxMaterial );
	engine.scene.add(skyBox);

	// texture used to generate "bumpiness"
    // We're going to use a DataTexture instead
    var heights = generateHeight(map);
    var flats = generateFlat(map);
    var bumpMap = new Uint8Array( 3 * heights.length );
    var tileMap = new Uint8Array( 3 * heights.length );

    const heightScale = 45;
    for (var i = 0; i < heights.length; i++) {
        // only R channel is updated for now as that's what the vertex
        // shader will care about. Change this to RGB later
        bumpMap[i*3]   = heightScale * heights[i];
        bumpMap[i*3+1] = heightScale * heights[i];
        bumpMap[i*3+2] = heightScale * heights[i];

        tileMap[i*3]   = flats[i];
        tileMap[i*3+1] = flats[i];
        tileMap[i*3+2] = flats[i];
    }

	var bumpTexture = new THREE.DataTexture(
            bumpMap, width, height, THREE.RGBFormat);
	bumpTexture.wrapS = bumpTexture.wrapT = THREE.ClampToEdgeWrapping;
   bumpTexture.needsUpdate = true;

	var tileTexture = new THREE.DataTexture(
            tileMap, width, height, THREE.RGBFormat);
	tileTexture.wrapS = tileTexture.wrapT = THREE.ClampToEdgeWrapping;
   tileTexture.needsUpdate = true;


	// magnitude of normal displacement
	var bumpScale   = 200.0;

	var oceanTexture = loader.load('resources/images/dirt-512.jpg' );
	oceanTexture.wrapS = oceanTexture.wrapT = THREE.RepeatWrapping;

	var sandyTexture = loader.load('resources/images/sand-512.jpg' );
	sandyTexture.wrapS = sandyTexture.wrapT = THREE.RepeatWrapping;

	var rockyTexture = loader.load('resources/images/rock-512.jpg' );
	rockyTexture.wrapS = rockyTexture.wrapT = THREE.RepeatWrapping;

	var snowyTexture = loader.load('resources/images/snow-512.jpg' );
	snowyTexture.wrapS = snowyTexture.wrapT = THREE.RepeatWrapping;

	var scrubTexture = loader.load('resources/tiles/scrub.png' );
	scrubTexture.wrapS = scrubTexture.wrapT = THREE.RepeatWrapping;

	var forestTexture = loader.load('resources/tiles/forest.png' );
	forestTexture.wrapS = forestTexture.wrapT = THREE.RepeatWrapping;

	var lavaTexture = loader.load('resources/tiles/lava.png' );
	lavaTexture.wrapS = lavaTexture.wrapT = THREE.RepeatWrapping;

	var stoneTexture = loader.load('resources/tiles/stone.png' );
	stoneTexture.wrapS = stoneTexture.wrapT = THREE.RepeatWrapping;

	var grassTexture = loader.load('resources/tiles/grass.png' );
	grassTexture.wrapS = grassTexture.wrapT = THREE.RepeatWrapping;


	// use "this." to create global object
	this.customUniforms = {
		bumpTexture:	{ type: "t", value: bumpTexture },
		bumpScale:	   { type: "f", value: bumpScale },
		oceanTexture:	{ type: "t", value: oceanTexture },
		sandyTexture:	{ type: "t", value: sandyTexture },
		grassTexture:	{ type: "t", value: grassTexture },
		rockyTexture:	{ type: "t", value: rockyTexture },
		snowyTexture:	{ type: "t", value: snowyTexture },
		forestTexture: { type: "t", value: forestTexture},
		lavaTexture:   { type: "t", value: lavaTexture},
	   tileTexture:	{ type: "t", value: tileTexture },
		scrubTexture:  { type: "t", value: scrubTexture},
		stoneTexture:  { type: "t", value: stoneTexture},
	};

	// create custom material from the shader code above
	//   that is within specially labelled script tags
	var customMaterial = new THREE.ShaderMaterial(
	{
	    uniforms: customUniforms,
		vertexShader:   document.getElementById('vertexShader').textContent,
		fragmentShader: document.getElementById('fragmentShader').textContent,
		// side: THREE.DoubleSide
	}   );

   var mapSz = nTiles*tileSz;
   var planeGeo = new THREE.PlaneGeometry(
         mapSz, mapSz, tileSz, tileSz);

   var usingPositive = false;
   // Only use first left quadrant
   if (usingPositive) {
      planeGeo.translate(mapSz/2, -mapSz/2, 125);
   } else {
      planeGeo.translate(mapSz/2, mapSz/2, 125);
   }

	var plane = new THREE.Mesh(	planeGeo, customMaterial );
	plane.rotation.x = -Math.PI / 2;
	plane.position.y = -100;
	engine.scene.add( plane );
   engine.mesh = plane;

	var waterGeo = new THREE.PlaneGeometry( 1000, 1000, 1, 1 );
	var waterTex = loader.load( 'resources/images/water512.jpg' );
	waterTex.wrapS = waterTex.wrapT = THREE.RepeatWrapping;
	waterTex.repeat.set(5,5);
	var waterMat = new THREE.MeshBasicMaterial( {
        map: waterTex, transparent:true, opacity:0.40} );
	var water = new THREE.Mesh(	planeGeo, waterMat );
	water.rotation.x = -Math.PI / 2;
	water.position.y = -50;
	engine.scene.add( water);
}
