export {EntityBox}


class EntityBox {

   constructor() {
      // HTML stuff
      this.container = document.createElement("div");
      this.container.id = "entity_container";
      this.container.style.cssText =  // opacity:0.5. z-index puts this in front
		   'position:fixed;left:0;top:66%;z-index:10000';

      this.panels = [];

		//var rp = new RenderedPanel();
		var fp = new FlatPanel("#000000");

		this.addPanel(fp);
		//this.addPanel(rp);

      this.dom = this.container;
      this.mode = -1;
		document.body.appendChild(this.dom);

      var scope = this;
      function onMouseDown( event ) {scope.onClick(event);}
      this.container.addEventListener( "click", onMouseDown, false);
   }

   onClick (event) {
      this.container.style.display = "none";
   }

   addPanel (panel) {
      this.container.appendChild(panel.dom);
		this.panels.push(panel);
   }

   showAll() {
		this.container.style.display = 'block';
   }

   showPanel( id ) {
		for ( var i = 0; i < this.container.children.length; i ++ ) {
			this.container.children[i].style.display = i === id ? 'block' : 'none';
		}
		this.mode = id;
	}

   changeColor(color) {
      for (var i = 0; i < this.panels.length; i++) {
         this.panels[i].changeColor(color);
      }
   }

   setText(text) {
      for (var i = 0; i < this.panels.length; i++) {
         this.panels[i].setText(text);
      }
   }

   update(delta) {
      // This will be replaced with something more complicated later
      for (var i = 0; i < this.panels.length; i++) {
         this.panels[i].update(delta);
      }
   }
}


class Panel {

   changeColor(color) {
   }

   setText(text) {
   }

   update(delta) {
      // Do nothing
   }
}

class TilePanel extends Panel {

   constructor(i, j) {
		this.TILESZ = 80;
		var canvas = document.createElement( 'canvas' );
      canvas.id = "tile_panel";
		canvas.width = this.TILESZ;
		canvas.height = this.TILESZ;
		canvas.style.cssText = 'position:fixed;left:0;top:66%;width:'
         + canvas.width + 'px;height:' + canvas.height + 'px';
		this.canvas = canvas;
	   this.context = this.canvas.getContext( '2d' );
		this.dom = canvas;
	}

	drawTiles( tileMap ) {
      // tileMap is array of R, G, B = n x 3 x d
		for (var c = 0; c < map.cols; c++) {
			for (var r = 0; r < map.rows; r++) {
				var tile = map.getTile(c, r);

				if (tile !== 0) { // 0 => empty tile
					this.context.fillStyle = tileToColorMap[tile];
					this.context.fillRect(
						  c * map.tsize, // target x
						  r * map.tsize, // target y
						  map.tsize, // target width
						  map.tsize // target height
					);
				}
			}
		}
   }

}

class FlatPanel extends Panel {

   constructor(fgColor) {
      /* fgColor will be the color of the text.
       * bgColor will be the color of the background, and possibly the color
       * of the graph.
       */
      super();

      this.fgColor = fgColor;  // this shouldn't change

		var min = Infinity, max = 0, round = Math.round;
		var PR = round( window.devicePixelRatio || 1 );

		this.WIDTH = window.innerWidth / 4 * PR;
      this.HEIGHT = 200 * PR;
		this.TEXT_X = 20 * PR;
      this.TEXT_Y = 50 * PR;
		var GRAPH_X = 300 * PR, GRAPH_Y = 150 * PR,
				GRAPH_WIDTH = 74 * PR, GRAPH_HEIGHT = 30 * PR;

		var canvas = document.createElement( 'canvas' );
      canvas.id = "flat_panel";
		canvas.width = this.WIDTH;
		canvas.height = this.HEIGHT;
		canvas.style.cssText = 'position:fixed;left:0;top:66%;width:'
         + this.WIDTH/PR + 'px;height:' + this.HEIGHT/PR + 'px';
		this.canvas = canvas;

		var context = this.canvas.getContext( '2d' );
		context.font = 'bold ' + ( 50 * PR )
            + 'px DragonSlapper';
		context.textBaseline = 'top';
      this.context = context;

		//context.fillStyle = bgColor;
		//context.globalAlpha = 0.9;
		//context.fillRect( GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT );

		this.dom = canvas;

	}

   setText( text ) {
      this.text = text;
      this.fillText();
   }

   fillText() {
		this.context.fillStyle = this.fgColor;
		this.context.fillText( this.text, this.TEXT_X, this.TEXT_Y );
		// this.context.fillRect( GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT );
   }

	changeColor( color ) {
      this.bgColor = color;
      this.context.fillStyle = this.bgColor;
		this.context.fillRect( 0, 0, this.WIDTH, this.HEIGHT );
      this.fillText();
		// currently pass, maybe change FG color?
	}

   update ( delta ) {
		// currently we do nothing
		// this.context.drawImage(
		//		canvas, GRAPH_X + PR, GRAPH_Y, GRAPH_WIDTH - PR, GRAPH_HEIGHT,
		//		GRAPH_X, GRAPH_Y, GRAPH_WIDTH - PR, GRAPH_HEIGHT );
   }
}

class RenderedPanel extends Panel {

   constructor() {
      super();
      // engine stuff in a panel
      this.scene = new THREE.Scene();

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000);

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth/3, window.innerHeight/3 );
      this.renderer.shadowMap.enabled = true;

      var pointLight = new THREE.PointLight( 0xffffff, 1.5, 0, 2);
      pointLight.position.set(64*40, 1500, 64*40);
      pointLight.caseShadow = true;
      pointLight.shadow.camera.far = 0;
      this.scene.add(pointLight);

		this.dom = this.renderer.domElement;
   }

   changeColor(color) {
      this.scene.background = new THREE.Color( color );
   }

   update(delta) {
      this.renderer.render( this.scene, this.camera );
   }
}
