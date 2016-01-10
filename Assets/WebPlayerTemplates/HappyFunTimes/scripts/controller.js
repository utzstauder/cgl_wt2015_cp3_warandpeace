/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
"use strict";

// Start the main app logic.
requirejs([
    'hft/commonui',
    'hft/gameclient',
    'hft/misc/input',
    'hft/misc/misc',
    'hft/misc/mobilehacks',
    'hft/misc/strings',
    'hft/misc/touch',
    '../3rdparty/chroma.min',
    './bower_components/hft-utils/dist/audio',
//    '../3rdparty/gyronorm.complete.min',
  ], function(
    commonUI,
    GameClient,
    input,
    misc,
    mobileHacks,
    strings,
    touch,
    chroma,
    AudioManager /*,
    GyroNorm */) {

  var $ = document.getElementById.bind(document);
  var globals = {
    debug: false,
    // orientation: "landscape-primary",
    provideOrientation: false,
    provideMotion: false,
    provideRotationRate: false,
  };
  misc.applyUrlSettings(globals);
  mobileHacks.disableContextMenu();
  mobileHacks.fixHeightHack();
  mobileHacks.adjustCSSBasedOnPhone([
    {
      test: mobileHacks.isIOS8OrNewerAndiPhone4OrIPhone5,
      styles: {
        ".button": {
          bottom: "40%",
        },
      },
    },
  ]);

  var needOrientationData = false;
  var startOrientationData = function() {
    needOrientationData = true;
  };
  var stopOrientationData = function() {
  };


  var fullElem = $("full");
  var client = new GameClient();
  var audioManager = new AudioManager();

  var layouts = {
    "1button": {
      orientation: "none",
      buttons: true,
    },
    "2button": {
      orientation: "none",
      buttons: true,
    },
    "1dpad-1button": {
      orientation: "landscape",
      buttons: true,
      dpads: true,
    },
    "1dpad-2button": {
      orientation: "landscape",
      buttons: true,
      dpads: true,
    },
    "1dpad": {
      orientation: "none",
      dpads: true,
    },
    "2dpad": {
      orientation: "landscape",
      dpads: true,
    },
    "1lrpad-1button": {
      orientation: "landscape",
      buttons: true,
      lrpads: true,
    },
    "1lrpad-2button": {
      orientation: "landscape",
      buttons: true,
      lrpads: true,
    },
    "1lrpad": {
      orientation: "none",
      lrpads: true,
    },
    "touch": {
      orientation: "portrait",
      orientationOptional: true,
    },
    "orient": {
      orientation: "portrait",
      orientationOptional: true,
    },
	// custom controller type by me
	"draw":{
	  orientation: "portrait",
	  orientationOptional: true,
	},
  };

  function to255(v) {
    return v * 255 | 0;
  }

  function handleColor(data) {
    // the color arrives in data.color.
    // we use chroma.js to darken the color
    // then we get our style from a template in controller.html
    // sub in our colors, remove extra whitespace and attach to body.
    var color = "rgb(" + to255(data.color.r) + "," + to255(data.color.g) + "," + to255(data.color.b) + ")";
    var subs = {
      light: color,
      dark: chroma(color).darken().hex(),
    };
    var style = $("background-style").text;
    style = strings.replaceParams(style, subs).replace(/[\n ]+/g, ' ').trim();
    document.body.style.background = style;
  }

  function notLayout(name) {
    return name.substr(0, 7) !== "layout-";
  }

  function handleOptions(data) {
    data = data || {};
    var controllerOptions = data.controllerOptions || data;
    var controllerType = controllerOptions.controllerType;
    // names in C# are c_pads_buttons but names here are pads-buttons.
    // that's basically because I wrote the JS first. I could fix all the JS and CSS to match the C# but I'm lazy.
    controllerType = (controllerType || "").replace(/s/g, "").replace("c_", "").replace(/_/g, '-').toLowerCase();  // remove 's' so buttons -> button, dpads -> dpad
    if (!(controllerType in layouts)) {
      if (controllerType) {
        client.error("unknown controller type: " + controllerType);
        client.error("valid types are:\n" + Object.keys(layouts).join("\n"));
      }
      controllerType = "1dpad-2button";
    }
    var elem = $("buttons");
    var classes = elem.className.split(/[ \t\n]+/);
    classes = classes.filter(notLayout);
    classes.unshift("layout-" + controllerType);
    elem.className = classes.join(" ");

    var layout = layouts[controllerType];
    commonUI.setOrientation(layout.orientation, layout.orientationOptional);

    globals.provideOrientation  = controllerOptions.provideOrientation;
    globals.provideAcceleration = controllerOptions.provideAcceleration;
    globals.provideRotationRate = controllerOptions.provideRotationRate;

    if (globals.provideOrientation ||
        globals.provideAcceleration ||
        globals.provideRotationRate) {
      startOrientationData(); // eslint-disable-line
    } else {
      stopOrientationData();  // eslint-disable-line
    }
  }

  function handleFull() {
    fullElem.style.display = "block";
  }

  function handlePlay() {
    fullElem.style.display = "none";
  }

  function handleLoadSounds(data) {
    var sounds = data.sounds;
    Object.keys(sounds).forEach(function(name) {
      var sound = sounds[name];
      if (sound.generator) {
        sound.jsfx = [sound.generator].concat(sound.parameters);
      }
    });
    audioManager.loadSounds(sounds);
  }

  var playingSounds = {};

  function handlePlaySound(data) {
    var sound = audioManager.playSound(data.name, 0, data.loop);
    if (data.remember) {
      handleStopSound(data);
      playingSounds[data.name] = sound;
    }
  }

  client.addEventListener('color', handleColor);
  client.addEventListener('options', handleOptions);
  client.addEventListener('full', handleFull);
  client.addEventListener('play', handlePlay);
  client.addEventListener('loadSounds', handleLoadSounds);
  client.addEventListener('playSound', handlePlaySound);
  
  // custom eventlisteners
  client.addEventListener('drawOptions', handleDrawOptions);
  client.addEventListener('backgroundOptions', handleBackgroundOptions);
  client.addEventListener('receiveLetter', handleReceiveLetter);
  client.addEventListener('pullDrawing', handlePullDrawing);

  // This way of making buttons probably looks complicated but
  // it lets us easily make more buttons.
  //
  // It's actually pretty simple. We embed 2 svg files
  // in the HTML in a script tag. We could load them but
  // loading is ASYNC
  //
  // We put in substitutions in the form of %(nameOfValue)s
  // so we can easily replace the colors. We could have done
  // that by looking up nodes or using CSS but this was easiest.
  //
  // We then insert that text into a div by id, look up
  // the 2 svg files and hook up some functions, press(), and
  // isPressed() that we can use check the state of the button
  // and to change which svg shows.
  var Button = function() {
    var svgSrc = $("button-img").text + $("button-pressed").text;

    return function Button(id, options) {
      var element = $(id);
      var pressed = false;
      element.innerHTML = strings.replaceParams(svgSrc, options);
      var buttonSvg  = element.querySelector(".button-img");
      var pressedSvg = element.querySelector(".button-pressed");

      this.press = function(press) {
        pressed = press;
        buttonSvg.style.display  =  pressed ? "none" : "inline-block";
        pressedSvg.style.display = !pressed ? "none" : "inline-block";
      };

      this.isPressed = function() {
        return pressed;
      };

      this.press(false);
    };
  }();

  // Make 2 buttons
  var buttons = [
    new Button("buttonA", { surfaceColor: "#F64B83", edgeColor: "#76385E" }),
    new Button("buttonB", { surfaceColor: "#1C97FA", edgeColor: "#1C436A" }),
  ];

  var LRButton = function() {
    var svgSrc = $("lr-button-00").text +
                 $("lr-button-01").text +
                 $("lr-button-10").text +
                 $("lr-button-11").text;

    return function LRButton(id, options) {
      options = options || {};
      var element = $(id);
      var state = 1;
      element.innerHTML = strings.replaceParams(svgSrc, options);
      var svgs = [];
      var zeroOne = function(v) {
        return v ? "1" : "0";
      };
      for (var ii = 0; ii < 4; ++ii) {
        var svgId = ".lr-button-" + zeroOne(ii & 1) + zeroOne(ii & 2);
        var svgElement = element.querySelector(svgId);
        svgs.push(svgElement);
        svgElement.style.display = "none";
      }

      this.setState = function(bits) {
        bits &= 0x3;
        if (state !== bits) {
          svgs[state].style.display = "none";
          state = bits;
          svgs[state].style.display = "inline-block";
          return true;
        }
      };

      this.getState = function() {
        return state;
      };

      this.setState(0);
    };
  }();

  var lrButton = new LRButton("lrpad");  // eslint-disable-line

  var DPad = function(id) {
    var element = $(id);
    element.innerHTML = $("dpad-image").text;
  };
  // TODO: animate dpads
  var dpads = [  // eslint-disable-line
    new DPad("dpad1"),
    new DPad("dpad2"),
  ];

  commonUI.setupStandardControllerUI(client, globals);

  // Since we take input touch, mouse, and keyboard
  // we only send the button to the game when it's state
  // changes.
  function handleButton(pressed, id) {
    var button = buttons[id];
    if (pressed !== button.isPressed()) {
      button.press(pressed);
      client.sendCmd('button', { id: id, pressed: pressed });
    }
  }

  function handleDPad(e) {
    // lrpad is just dpad0
    var pad = e.pad;
    if (pad === 2) {
      pad = 0;
    }
    if (pad === 0) {
      lrButton.setState(e.info.bits);
    }
    client.sendCmd('dpad', { pad: pad, dir: e.info.direction });
  }

  // Setup some keys so we can more easily test on desktop
  var keys = { };
  keys["Z"]                     = function(e) { handleButton(e.pressed,  0); };  // eslint-disable-line
  keys["X"]                     = function(e) { handleButton(e.pressed,  1); };  // eslint-disable-line
  input.setupKeys(keys);
  input.setupKeyboardDPadKeys(handleDPad, {
    pads: [
     { keys: input.kCursorKeys, },
     { keys: input.kASWDKeys,   },
    ],
  });

  // Setup the touch areas for buttons.
  touch.setupButtons({
    inputElement: $("buttons"),
    buttons: [
      { element: $("buttonA"), callback: function(e) { handleButton(e.pressed, 0); }, },  // eslint-disable-line
      { element: $("buttonB"), callback: function(e) { handleButton(e.pressed, 1); }, },  // eslint-disable-line
    ],
  });

  // should I look this up? I can't actually know it until the CSS is set.
  touch.setupVirtualDPads({
    inputElement: $("dpads"),
    callback: handleDPad,
    fixedCenter: true,
    pads: [
      { referenceElement: $("dpad1"), },
      { referenceElement: $("dpad2"), },
      { referenceElement: $("lrpad"), },
    ],
  });

  var baseButtonNdx = 18
  var maxIndex = 10;
  var usedIndices = [];
  var pointerIdToIndex = {};
  function getPointerIndex(e, start) {
    var index = pointerIdToIndex[e.pointerId];
    if (index === undefined) {
      for (var ii = 0; ii < maxIndex; ++ii) {
        if (!usedIndices[ii]) {
          usedIndices[ii] = start;
          index = ii;
          break;
        }
      }
      if (index === undefined) {
        throw "what";
      }
      pointerIdToIndex[e.pointerId] = index;
    }
    if (!start) {
      delete pointerIdToIndex[e.pointerId];
      usedIndices[index] = undefined;
    }
    return index;
  }

  // Setup the touch area
  $("touch").addEventListener('pointermove', function(event) {
    var target = event.target;
    var position = input.getRelativeCoordinates(target, event);
    client.sendCmd('touch', {
      id: getPointerIndex(event, true),
      x: position.x / target.clientWidth  * 1000 | 0,
      y: position.y / target.clientHeight * 1000 | 0,
    });
    event.preventDefault();
  });

  var isTouched = {};
  function handleTouchDown(e) {
    isTouched[e.pointerId] = true;
    client.sendCmd('button', { id: getPointerIndex(e, true) + baseButtonNdx, pressed: true });
  }

  function handleTouchUp(e) {
    isTouched[e.pointerId] = false;
    client.sendCmd('button', { id: getPointerIndex(e, false) + baseButtonNdx, pressed: false });
  }

  function handleTouchOut(e) {
    client.sendCmd('button', { id: getPointerIndex(e, false) + baseButtonNdx, pressed: false });
  }

  function handleTouchLeave(e) {
    client.sendCmd('button', { id: getPointerIndex(e, false) + baseButtonNdx, pressed: false });
  }

  function handleTouchCancel(e) {
    isTouched[e.pointerId] = false;
    client.sendCmd('button', { id: getPointerIndex(e, false) + baseButtonNdx, pressed: false });
  }

  function handleTouchEnter(e) {
    if (isTouched[e.pointerId]) {
      client.sendCmd('button', { id: getPointerIndex(e, true) + baseButtonNdx, pressed: true });
    }
  }

  $("touch").addEventListener('pointerdown', handleTouchDown);
  $("touch").addEventListener('pointerup', handleTouchUp);
  $("touch").addEventListener('pointerout', handleTouchOut);
  $("touch").addEventListener('pointerenter', handleTouchEnter);
  $("touch").addEventListener('pointerleave', handleTouchLeave);
  $("touch").addEventListener('pointercancel', handleTouchCancel);
  $("orient").addEventListener('pointerdown', handleTouchDown);
  $("orient").addEventListener('pointerup', handleTouchUp);
  $("orient").addEventListener('pointerout', handleTouchOut);
  $("orient").addEventListener('pointerenter', handleTouchEnter);
  $("orient").addEventListener('pointerleave', handleTouchLeave);
  $("orient").addEventListener('pointercancel', handleTouchCancel);

  var gn = new GyroNorm();

  function handleOrientationData(data) {
    if (globals.provideOrientation) {
      // data.do.alpha    ( deviceorientation event alpha value )
      // data.do.beta     ( deviceorientation event beta value )
      // data.do.gamma    ( deviceorientation event gamma value )
      // data.do.absolute ( deviceorientation event absolute value )
      client.sendCmd('orient', { a: data.do.alpha, b: data.do.beta, g: data.do.gamma, abs: data.do.absolute });
    }

    if (globals.provideAcceleration) {
      // data.dm.x        ( devicemotion event acceleration x value )
      // data.dm.y        ( devicemotion event acceleration y value )
      // data.dm.z        ( devicemotion event acceleration z value )
      client.sendCmd('accel', { x: data.dm.x, y: data.dm.y, z: data.dm.z });
    }

    // data.dm.gx       ( devicemotion event accelerationIncludingGravity x value )
    // data.dm.gy       ( devicemotion event accelerationIncludingGravity y value )
    // data.dm.gz       ( devicemotion event accelerationIncludingGravity z value )


    if (globals.provideRotationRate) {
      // data.dm.alpha    ( devicemotion event rotationRate alpha value )
      // data.dm.beta     ( devicemotion event rotationRate beta value )
      // data.dm.gamma    ( devicemotion event rotationRate gamma value )
      client.sendCmd('rot', { a: data.dm.alpha, b: data.dm.beta, g: data.dm.gamma });
    }
  }

  function setupDeviceOrientation() {
    startOrientationData = function() {
      if (!gn.isRunning()) {
        gn.start(handleOrientationData);
      }
    };
    stopOrientationData = function() {
      if (gn.isRunning()) {
        gn.stop(handleOrientationData);
      }
    };
    // We need this because gn.init might
    // not have returned before we start
    // asking for data
    if (needOrientationData) {
      startOrientationData();
    }
  }

  gn.init().then(setupDeviceOrientation);

	// +++++++++++++++++++++++++++++++++++
	
	// Custom controller stuff starts here
	// Collaborative project 3 - Drawnes
	
	// +++++++++++++++++++++++++++++++++++
	// initialization
	
	// Variables and references and stuff
	var canvas = document.getElementById("drawCanvas");
	var bgCanvas = document.getElementById("backgroundCanvas");
	var ctx = canvas.getContext("2d");
	var bgCtx = bgCanvas.getContext("2d");
	var lastPt = null;
	
	// draw variables & client stuff
	var accelerationThreshold = 40;
	ctx.lineWidth="15";
	ctx.strokeStyle="black";
	var drawArrayDivision = 1;
	var defaultAccuracy = 0.5;
	
	// letter variables
	var letterScale = 4;
	bgCtx.fillStyle ="black";
	
	var currentLetter = null;
	var currentLetterWidth = null;
	var currentLetterHeight = null;
	
	// Add event listeners
	canvas.addEventListener("touchmove", draw, false);
	canvas.addEventListener("touchend", drawEnd, false);
	
	// Resize canvases to fit window size
	canvas.width = window.innerWidth;
	canvas.height = window.innerHeight;
	bgCanvas.width = window.innerWidth;
	bgCanvas.height = window.innerHeight;

	// Check for device motion
	if (window.DeviceMotionEvent){
		// Add the event listener
		window.addEventListener('devicemotion', deviceMotionHandler);
	}
	
	// Check for vibration support
	if ("vibrate" in navigator){
		// enable vibration
		navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;
	}

	// initialization end
	// +++++++++++++++++++++++++++++++++++
	
	// Overrides draw options
	function handleDrawOptions(data){
		var drawOptions = data.drawOptions;
		
		accelerationThreshold = drawOptions.accelerationThreshold;
		ctx.lineWidth = drawOptions.lineWidth;
		ctx.lineCap = drawOptions.lineCap;
		ctx.strokeStyle = drawOptions.strokeStyle;
		drawArrayDivision = drawOptions.drawArrayDivision;
		defaultAccuracy = drawOptions.defaultAccuracy;
	}
	
	function handleBackgroundOptions(data){
		var bgOptions = data.backgroundOptions;
		
		letterScale = bgOptions.letterScale;
		bgCtx.fillStyle = bgOptions.fillStyle;
	}
	
	function draw(event){
		event.preventDefault();
		
		// Draw a small rectangle in the canvas at the touch position
		// ctx.fillRect(event.touches[0].pageX, event.touches[0].pageY, 5, 5);
		
		// Draw a continuous path
		if (lastPt != null){			
			ctx.beginPath();
			ctx.moveTo(lastPt.x, lastPt.y);
			//ctx.lineTo(event.touches[0].pageX, event.touches[0].pageY);
			ctx.quadraticCurveTo(lastPt.x, lastPt.y, event.touches[0].pageX, event.touches[0].pageY);
			ctx.stroke();
		}
		
		
		// Store latest point
		lastPt = {x:event.touches[0].pageX, y:event.touches[0].pageY};
	}
	
	function drawEnd(event){
		event.preventDefault();
		
		// tmp
		//sendDrawArray();
		
		// Terminate touch path
		lastPt = null;
	}
	
	function clearCanvas(context){
		context.clearRect(0, 0, context.canvas.width, context.canvas.height);
		if (navigator.vibrate) {
			// vibration API supported
			navigator.vibrate(1000);
		}
	}
	
	function deviceMotionHandler(event){
			var acceleration = event.acceleration;
			
			if (Math.abs(acceleration.x) > accelerationThreshold ||
				Math.abs(acceleration.y) > accelerationThreshold ||
				Math.abs(acceleration.z) > accelerationThreshold){
				
				// temporarily put this here
				//sendDrawArray();
				
				
				if (currentLetter == null){
					// for showcasing
					sendDrawArray();
				}
				clearCanvas(ctx);
				
				/*if (confirm("Clear canvas?")){
					// clear canvas
					clearCanvas();
				} else{
					// do nothing
				}*/
			}
	}
	
	function flipArrayHorizontally(width, height, targetArray){
		var newArray = [];
		
		for (var y = height-1; y >= 0; y--){
			for (var x = 0; x < width; x++){
				newArray.push(targetArray[y * width + x]);
			}
		}
		
		return newArray;
	}
	
	function scaleUpArray(width, height, targetArray, scaleX, scaleY){
		var newArray = [];
		
		for (var y = 0; y < height; y++){
			for (var yStretch = 0; yStretch < scaleY; yStretch++){
				for (var x = 0; x < width; x++){
					for (var xStretch = 0; xStretch < scaleX; xStretch++){
						newArray.push(targetArray[y * width + x]);
					}
				}
			}
		}
		
		return newArray;
	}
	
	function scaleDownArray(width, height, targetArray, scaleFactor){
		var newArray = [];
		
		for (var y = 0; y < height; y += scaleFactor){
			for (var x = 0; x < width; x += scaleFactor){
				newArray.push(targetArray[y * width + x]);
			}
		}
		
		return newArray;	
	}
	
	function compareArrays(templateArray, drawArray){
		// scale drawArray to template size
		//var scaleFactor = ctx.canvas.width / currentLetterWidth;
		//var scaledTemplateArray = scaleUpArray(currentLetterWidth, currentLetterHeight, templateArray, scaleFactor, scaleFactor);

		//alert("scaleFactor: " + scaleFactor + ", " + scaledTemplateArray.length + " & " + drawArray.length);

		var templatePixels = 0;
		var matchingPixels = 0;
		var accuracy = 0.0;
		
		for (var i = 0; i < drawArray.length; i++){
			if (templateArray[i] > 1){
				templatePixels++;
				if (drawArray[i] > 1) matchingPixels++;
			}
			
		}
		
		accuracy = matchingPixels / templatePixels;
		
		return accuracy;
	}
	
	function sendDrawArray(){
		var width = ctx.canvas.width;
		var height = ctx.canvas.height;
		
		// Create ImageData object from canvas
		//var imageData = ctx.createImageData(width, height);
		
		// Get the image data from the drawCanvas
		var imageData = ctx.getImageData(0, 0, width, height);
		
		/*
		// Convert to "normal" int array
		var drawArray = Array.prototype.slice.call(imageData.data);
		*/
		
		// Transfer alpha values to new "normal" array
		var drawArray = [];
		for (var i = 3; i < (width*height*4); i += 4){
			drawArray.push(imageData.data[i]);
		}

		var drawAccuracy;
		
		if (currentLetter != null){
			drawAccuracy = compareArrays(currentLetter, drawArray);
			alert(drawAccuracy);
		} else drawAccuracy = defaultAccuracy;

		var sendArray = [];
		
		// Shrink down the array acording to drawArrayDivision parameter
		sendArray = scaleDownArray(width, height, drawArray, drawArrayDivision);
		
		// calculate new array dimensions
		width = width/drawArrayDivision;
		height = height/drawArrayDivision;
		
		// Send the data to the game server
		client.sendCmd('draw', {width: width, height: height, drawArray: sendArray, accuracy: drawAccuracy});
	}
	
	function handlePullDrawing(data){
		sendDrawArray();
	}
	
	function handleReceiveLetter(data){
		//alert(data.width + " x " + data.height + " = " + data.drawArray.length);
		
		//clear canvas first
		clearCanvas(bgCtx);
		clearCanvas(ctx);
		
		// array is upside down
		var flippedArray = flipArrayHorizontally(data.width, data.height, data.drawArray);
		// array is smaller than canvas
		var scaledArray = scaleUpArray(data.width, data.height, flippedArray, letterScale, letterScale);
	
		// calculate scaled dimensions
		var newWidth = data.width * letterScale;
		var newHeight = data.height * letterScale;
		
		// draw the received letter on the background canvas
		for (var y = 0; y < bgCanvas.height; y++){
			for (var x = 0; x < bgCanvas.width; x++){
				// check for end of boundaries
				if ((y * newWidth + x) < scaledArray.length){
					if (scaledArray[y * newWidth + x] > 0){
						bgCtx.fillRect(x, y, 1, 1);
					}
				}
			}
		}
		
		// set variables
		currentLetterWidth = newWidth;
		currentLetterHeight = newHeight;
		currentLetter = scaledArray;
		
	}

});

