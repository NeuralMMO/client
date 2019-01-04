//var worldWidth = 256, worldDepth = 256,
//worldHalfWidth = worldWidth / 2, worldHalfDepth = worldDepth / 2;
var width  = 80;
var height = 80;

function terrain(map) {
   this.sz = map.length
   var data = generateHeight(map);

   var geometry = new THREE.PlaneBufferGeometry( 64*sz, 64*sz, this.sz-1, this.sz-1 );
   geometry.rotateX( - Math.PI / 2 );

   var vertices = geometry.attributes.position.array;

   for ( var i = 0, j = 0, l = vertices.length; i < l; i ++, j += 3 ) {
      vertices[ j + 1 ] = data[ i ] * 64;
   }

   texture = new THREE.CanvasTexture( generateTexture( data, this.sz, this.sz) );
   texture.wrapS = THREE.ClampToEdgeWrapping;
   texture.wrapT = THREE.ClampToEdgeWrapping;

   mesh = new THREE.Mesh( geometry, new THREE.MeshBasicMaterial( { map: texture } ) );
   return mesh
}

function tile(val) {
   switch (val) {
      case 0:
         return 0;
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
         if (r==0 || c==0) {
            data[k] = 1;
            k++;
            continue;
         }
         var ll = tile(map[r][c-1]);
         var tt = tile(map[r-1][c]);
         var tl = tile(map[r-1][c-1]);
         var cc = tile(map[r][c]);
         var val = 0.25*(ll + tt + tl + cc);
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

function generateTexture( data, width, height ) {

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

      shade = vector3.dot( sun );

      imageData[ i ] = ( 96 + shade * 128 ) * ( 0.5 + data[ j ] * 0.007 );
      imageData[ i + 1 ] = ( 32 + shade * 96 ) * ( 0.5 + data[ j ] * 0.007 );
      imageData[ i + 2 ] = ( shade * 96 ) * ( 0.5 + data[ j ] * 0.007 );

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

/*
function animate() {

   requestAnimationFrame( animate );

   render();
   stats.update();

}

function render() {

   controls.update( clock.getDelta() );
   renderer.render( scene, camera );

}
*/
