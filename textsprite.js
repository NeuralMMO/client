function makeTextSprite(message, fontsize) {
            var ctx, texture, sprite, spriteMaterial, 
                canvas = document.createElement('canvas');
            ctx = canvas.getContext('2d');
            ctx.font = fontsize + "px Arial";
            
            // setting canvas width/height before ctx draw, else canvas is empty
            canvas.width = ctx.measureText(message).width;
            canvas.height = fontsize * 6; // fontsize * 1.5
            
            // after setting the canvas width/height we have to re-set font to apply!?! looks like ctx reset
            ctx.font = fontsize + "px Arial";        
            ctx.fillStyle = '#' + (Math.random()*0xFFFFFF<<0).toString(16);
            ctx.fillText(message, 0, fontsize);
    
            texture = new THREE.Texture(canvas);
            texture.minFilter = THREE.LinearFilter; // NearestFilter;
            texture.needsUpdate = true;
    
            spriteMaterial = new THREE.SpriteMaterial({map : texture});
            sprite = new THREE.Sprite(spriteMaterial);
            return sprite;   
        }
