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
  client.addEventListener('receiveLetterAsString', handleReceiveLetterAsString);
  client.addEventListener('receiveNotification', handleReceiveNotification);

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
	
	// idle timer check
	var idleTimer = 0;
	var idleTimeUntilQuit = 30;
	window.setInterval(increaseIdleTimer, 1000);
	
	// Variables and references and stuff
	var canvas = document.getElementById("drawCanvas");
	var bgCanvas = document.getElementById("backgroundCanvas");
	var uiCanvas = document.getElementById("uiCanvas");
	var debugCanvas = document.getElementById("debugCanvas");
	var inputCanvas = document.getElementById("inputCanvas");
	var ctx = canvas.getContext("2d");
	var bgCtx = bgCanvas.getContext("2d");
	var uiCtx = uiCanvas.getContext("2d");
	var debug = debugCanvas.getContext("2d");
	var lastPt = null;
	
	// draw variables & client stuff
	var gameState = "initializing";
	var prevGameState = "";
	var accelerationThreshold = 40;
	ctx.lineWidth="15";
	ctx.strokeStyle="black";
	var drawArrayDivision = 4;
	var defaultAccuracy = 0.5;
	var accuracyThreshold = 0.75;
	var isMouseDown = false;
	
	// letter variables
	var letterScale = 4;
	bgCtx.fillStyle = "black";
	var currentLetter = null;
	var currentLetterWidth = null;
	var currentLetterHeight = null;
	var currentLetterAsString = null;

	// notification variables
	var drawingEnabled = true;
	var notificationDismissable = true;
	var notificationTimeout = 0;
	var notificationTimerFunction = null;
	var notificationShowing = false;
	var notificationText = "";
	//var imgTitlescreen = document.getElementById("titlescreen");

	drawNotification();

	// UI variables
	var uiButtonBackground = "white";
	var uiButtonFontColor = "black";
	var uiButtonFont = "30px Arial";

	// Add event listeners
	window.addEventListener("resize", handleResize);

	inputCanvas.addEventListener("touchstart", inputTouchStart, false);
	inputCanvas.addEventListener("touchend", inputTouchEnd, false);
	inputCanvas.addEventListener("touchmove", inputTouchMove, false);

	inputCanvas.addEventListener("mousedown", inputMouseDown, false);
	inputCanvas.addEventListener("mouseup", inputMouseUp, false);
	inputCanvas.addEventListener("mousemove", inputMouseMove, false);


	// Resize canvases to fit window size
	// Must be a multiple of drawArrayDivision
	canvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
	canvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision
	bgCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
	bgCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision
	uiCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
	uiCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;
	debugCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
	debugCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;
	inputCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
	inputCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;

	// UI buttons
	var buttonClear = button("clear", 0, (uiCanvas.height - uiCanvas.height/10), (uiCanvas.width / 2), uiCanvas.height/10);
	var buttonSend = button("send", (uiCanvas.width / 2), (uiCanvas.height - uiCanvas.height/10), (uiCanvas.width / 2), uiCanvas.height/10);
	var buttonQuit = button('X', (uiCanvas.width - 50), 10, 40, 40);

	// Check for device motion
	if (window.DeviceMotionEvent || window.DeviceMotion){
		// Add the event listener
		window.addEventListener('devicemotion', deviceMotionHandler);
	} else {
		//alert("devicemotion not supported!");
	}
	
	// Check for vibration support
	if ("vibrate" in navigator){
		// enable vibration
		navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;
	}

	// draw UI and all that stuff for the first time
	redrawAll();

	// initialization end
	// +++++++++++++++++++++++++++++++++++
	
	
	// should be called every second
	function increaseIdleTimer(){
		idleTimer += 1;
		
		if (idleTimer >= idleTimeUntilQuit &&
			notificationDismissable === true){
			quitApplication();
		}	
	}
	
	function handleResize(event){
		var lineWidthTmp = ctx.lineWidth;
		var lineCapTmp = ctx.lineCap;
		var strokeStyleTmp = ctx.strokeStyle;
		var bgFillStyleTmp = bgCtx.fillStyle;
		
		canvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
		canvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision
		bgCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
		bgCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision
		uiCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
		uiCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;
		debugCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
		debugCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;
		inputCanvas.width = Math.ceil(window.innerWidth/drawArrayDivision) * drawArrayDivision;
		inputCanvas.height = Math.ceil(window.innerHeight/drawArrayDivision) * drawArrayDivision;
		
		
		ctx.lineWidth = lineWidthTmp;
		ctx.lineCap = lineCapTmp;
		ctx.strokeStyle = strokeStyleTmp;
		bgCtx.fillStyle = bgFillStyleTmp;

		buttonClear = button("clear", 0, (uiCanvas.height - uiCanvas.height/10), (uiCanvas.width / 2), uiCanvas.height/10);
		buttonSend = button("send", (uiCanvas.width / 2), (uiCanvas.height - uiCanvas.height/10), (uiCanvas.width / 2), uiCanvas.height/10);
		buttonQuit = button('X', (uiCanvas.width - 50), 10, 40, 40);

		redrawAll();
	}

	// START
	// communication handler

	function handlePullDrawing(data){
		sendDrawArray();
	}
	
	function handleReceiveLetterAsString(data){
		// TODO: 
		currentLetterAsString = data.letter;
		if (gameState === "playing_word");
		drawLetterOnBgCanvasFromString(currentLetterAsString);
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
		
		/*
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
		*/
		
		// set variables
		currentLetterWidth = newWidth;
		currentLetterHeight = newHeight;
		currentLetter = scaledArray;
		
		drawLetterOnBgCanvas();
		drawUI();
	}

	// Overrides draw options
	function handleDrawOptions(data){
		var drawOptions = data.drawOptions;
		
		prevGameState = gameState;
		gameState = drawOptions.gameState;
		accelerationThreshold = drawOptions.accelerationThreshold;
		ctx.lineWidth = drawOptions.lineWidth;
		ctx.lineCap = drawOptions.lineCap;
		ctx.strokeStyle = drawOptions.strokeStyle;
		drawArrayDivision = drawOptions.drawArrayDivision;
		defaultAccuracy = drawOptions.defaultAccuracy;
		accuracyThreshold = drawOptions.accuracyThreshold;
		
		if (prevGameState === "paused" && gameState === "playing_free"){
			clearCanvas(ctx);
			clearCanvas(bgCtx);
		}
	}

	// Overrides background options
	// Templates for the background canvas
	function handleBackgroundOptions(data){
		var bgOptions = data.backgroundOptions;
		
		letterScale = bgOptions.letterScale;
		bgCtx.fillStyle = bgOptions.fillStyle;
	}

	// This function handles and displays notification data from the game server
	// notifications are prompts that disappear once they are clicked
	// "new round has stared! you are on team read! have fun!" *click* notification disappears 
	// TODO: implement server end
	function handleReceiveNotification(data){
		notificationShowing = true;
		drawingEnabled = false;

		notificationText = data.message;
		notificationDismissable = data.dismissable;
		notificationTimeout = data.timeout;
		
		if (!notificationDismissable){
			clearTimeout(notificationTimerFunction);
			//notificationTimerFunction = setTimeout(dismissNotification(), notificationTimeout * 1000);
		}
		
		drawNotification();
	}

	// communication handler
	// END


	// START
	// misc event handlers

	function deviceMotionHandler(event){
			var accelerationIncludingGravity = event.accelerationIncludingGravity;
			var acceleration = event.acceleration;
			
			/*clearCanvas(debug);
			debug.fillText(event.toString, 0, 20);
			debug.fillText(acceleration.x, 0, 40);
			debug.fillText(acceleration.y, 0, 60);
			debug.fillText(acceleration.z, 0, 80);*/
			
			if (Math.abs(acceleration.x) > accelerationThreshold ||
				Math.abs(acceleration.y) > accelerationThreshold ||
				Math.abs(acceleration.z) > accelerationThreshold ||
				Math.abs(accelerationIncludingGravity.x) > accelerationThreshold ||
				Math.abs(accelerationIncludingGravity.y) > accelerationThreshold ||
				Math.abs(accelerationIncludingGravity.z) > accelerationThreshold){
				
				// temporarily put this here
				//sendDrawArray();
				//alert(acceleration.x +"/"+ acceleration.y +"/"+ acceleration.z);
				
				/*if (gameState === "playing_free"){
					// for showcasing
					sendDrawArray(defaultAccuracy);
				}*/
				clearCanvas(ctx);
				
				/*if (confirm("Clear canvas?")){
					// clear canvas
					clearCanvas();
				} else{
					// do nothing
				}*/
			}
	}

	// misc event handlers
	// END


	// START
	// input canvas handling

	function inputTouchStart(event){
		event.preventDefault();

		if (notificationShowing === true){
			if (notificationDismissable) dismissNotification();
		} else if (buttonCollision(buttonClear, event.touches[0].pageX, event.touches[0].pageY)){
					buttonClearPressed();
				} else if (buttonCollision(buttonSend, event.touches[0].pageX, event.touches[0].pageY)){
					buttonSendPressed();
				} else if (buttonCollision(buttonQuit, event.touches[0].pageX, event.touches[0].pageY)){
					buttonQuitPressed();
				} else{
					if (drawingEnabled) drawTouchStart(event);
				}
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	function inputTouchEnd(event){
		event.preventDefault();
		if (drawingEnabled) drawTouchEnd(event);
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	function inputTouchMove(event){
		event.preventDefault();
		if (drawingEnabled) drawTouchMove(event);
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	function inputMouseDown(event){
		event.preventDefault();

		if (notificationShowing){
			if (notificationDismissable) dismissNotification();
		} else if (buttonCollision(buttonClear, event.pageX, event.pageY)){
					buttonClearPressed();
				} else if (buttonCollision(buttonSend, event.pageX, event.pageY)){
					buttonSendPressed();
				} else if (buttonCollision(buttonQuit, event.pageX, event.pageY)){
					buttonQuitPressed();
				} else{
					if (drawingEnabled) drawMouseDown(event);
				}
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	function inputMouseUp(event){
		event.preventDefault();
		if (drawingEnabled) drawMouseUp(event);
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	function inputMouseMove(event){
		event.preventDefault();
		if (drawingEnabled) drawMouseMove(event);
		
		// reset idleTimer upon input
		idleTimer = 0;
	}

	// input canvas handling
	// END


	// START
	// touch input functions for the draw canvas

	function drawTouchStart(event){
		event.preventDefault();
		
		ctx.beginPath();
		ctx.arc(event.touches[0].pageX, event.touches[0].pageY, ctx.lineWidth/(Math.PI * Math.PI * Math.PI), 0, 2 * Math.PI, false);
		ctx.stroke();
		
		// Store latest point
		lastPt = {x:event.touches[0].pageX, y:event.touches[0].pageY};
	}
	
	function drawTouchMove(event){
		event.preventDefault();

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
	
	function drawTouchEnd(event){
		event.preventDefault();
		
		// Terminate touch path
		lastPt = null;
		
		// If in word mode check for accuracy of drawing and send to game if complete
		if (currentLetter !== null && gameState === "playing_word"){
			
			var accuracy = compareCanvas(bgCtx, ctx);

			if (accuracy >= accuracyThreshold){
				sendDrawArray(accuracy);
				currentLetter = null;
			}
		}
	}

	// touch input functions for the draw canvas
	// END


	// START
	// mouse input functions for the draw canvas

	function drawMouseDown(event){
		event.preventDefault();

		isMouseDown = true;
		
		ctx.beginPath();
		ctx.arc(event.pageX, event.pageY, ctx.lineWidth/(Math.PI * Math.PI * Math.PI), 0, 2 * Math.PI, false);
		ctx.stroke();
		
		// Store latest point
		lastPt = {x:event.pageX, y:event.pageY};
	}
	
	function drawMouseMove(event){
		event.preventDefault();
		if (isMouseDown === true){
			// Draw a continuous path
			if (lastPt != null){			
				ctx.beginPath();
				ctx.moveTo(lastPt.x, lastPt.y);
				//ctx.lineTo(event.touches[0].pageX, event.touches[0].pageY);
				ctx.quadraticCurveTo(lastPt.x, lastPt.y, event.pageX, event.pageY);
				ctx.stroke();
			}

			// Store latest point
			lastPt = {x:event.pageX, y:event.pageY};
		}
	}
	
	function drawMouseUp(event){
		event.preventDefault();
		
		// Terminate touch path
		lastPt = null;
		
		// If in word mode check for accuracy of drawing and send to game if complete
		if (currentLetter !== null && gameState === "playing_word"){
			
			var accuracy = compareCanvas(bgCtx, ctx);

			if (accuracy >= accuracyThreshold){
				sendDrawArray(accuracy);
				currentLetter = null;
			}
		}

		isMouseDown = false;
	}

	// mouse input functions for the draw canvas
	// END	


	// START
	// canvas helper

	function clearCanvas(context){
		context.clearRect(0, 0, context.canvas.width, context.canvas.height);
		/*if (navigator.vibrate) {
			// vibration API supported
			navigator.vibrate(1000);
		}*/
	}

	// redraws everything that should be visible
	function redrawAll(){
		clearCanvas(ctx);
		clearCanvas(bgCtx);
		clearCanvas(uiCtx);
		/*if (currentLetter !== null && gameState == "playing_word"){
			drawLetterOnBgCanvas();
		}*/
		
		if (currentLetterAsString !== null && gameState == "playing_word"){
			drawLetterOnBgCanvasFromString(currentLetterAsString);
		}

		// TODO: draw UI

		drawUI();
	}

	// canvas helper
	// END


	// START
	// UI functions

	// should be called everytime something is drawn on ANY canvas and should be drawn last
	// TODO: deprecated
	function drawUI(){

		if (notificationShowing){
			drawNotification();
		} else {
			drawButton(buttonClear);
			drawButton(buttonSend);
			drawButton(buttonQuit);
		}
	}

	// creates a new button object
	function button(_text, _x, _y, _width, _height){
		var button = {text: _text, x: _x, y: _y, width: _width, height: _height};

		return button;
	}

	// is called when buttonClear is pressed
	function buttonClearPressed(){
		clearCanvas(ctx);
	}

	// is called when buttonSend is pressed
	function buttonSendPressed(){
		if (gameState === "playing_word" &&
		(currentLetter !== null || currentLetterAsString !== null)){
			
			var accuracy = compareCanvas(bgCtx, ctx);

			sendDrawArray(accuracy);
			currentLetter = null;
		} else if (gameState === "playing_free"){
			sendDrawArray(defaultAccuracy);
			clearCanvas(ctx);
		}
	}
	
	// is called when buttonQuit is pressed
	function buttonQuitPressed(){
		quitApplication();
	}
	
	// Quits the application
	function quitApplication(){
		window.open("https://docs.google.com/forms/d/1DUZs3-u70XKN4UzRwLdavks2_PsPnbGpu0_gVTBcvS8/viewform","_self");
	}

	// use this to check if a button was pressed
	function buttonCollision(_button, _x, _y){
		//alert(_x + "/" + _y + " <=> " + _button.x + "/" + _button.y);

		if (_y >= _button.y &&
			_y <= _button.y + _button.height &&
			_x >= _button.x &&
			_x <= _button.x + _button.width) return true;

		return false;
	}

	function drawNotification(){
		// fill background
		uiCtx.fillStyle = "white";
		uiCtx.fillRect(0, 0, uiCtx.canvas.width, uiCtx.canvas.height);

		uiCtx.fillStyle = "black";
		uiCtx.font = "30px arial";
		uiCtx.fontBaseline = "middle";
		uiCtx.textAlign = "center";
		
		//uiCtx.fillText(notificationText, uiCtx.canvas.width/2, uiCtx.canvas.height/2);
		wrapText(uiCtx, notificationText, uiCtx.canvas.width/2, uiCtx.canvas.height*1/3, uiCtx.canvas.width*3/4, 30);
		
		// reset idleTimer
		idleTimer = 0;
	}
	
	// found this at: http://www.html5canvastutorials.com/tutorials/html5-canvas-wrap-text-tutorial/
	function wrapText(context, text, x, y, maxWidth, lineHeight) {
        var words = text.split(' ');
        var line = '';

        for(var n = 0; n < words.length; n++) {
          var testLine = line + words[n] + ' ';
          var metrics = context.measureText(testLine);
          var testWidth = metrics.width;
          if (testWidth > maxWidth && n > 0) {
            context.fillText(line, x, y);
            line = words[n] + ' ';
            y += lineHeight;
          }
          else {
            line = testLine;
          }
        }
        context.fillText(line, x, y);
      }

	function dismissNotification(){
		notificationShowing = false;
		drawingEnabled = true;
		
		notificationText = "";
		notificationDismissable = true;
		notificationTimeout = 0;
		
		clearCanvas(uiCtx);
		drawUI();
	}

	// draws a button on the uiCanvas element
	function drawButton(_button){
		uiCtx.beginPath();

		uiCtx.rect(_button.x, _button.y, _button.width, _button.height);
		uiCtx.fillStyle = uiButtonBackground;
		uiCtx.fill();

		uiCtx.strokeStyle = "black";
		uiCtx.lineWidth = 8;
		uiCtx.stroke();

		uiCtx.fillStyle = uiButtonFontColor;
		uiCtx.font = uiButtonFont;
		uiCtx.textAlign = "center";
		uiCtx.textBaseline = "middle";
		uiCtx.fillText(_button.text, _button.x + _button.width/2, _button.y + _button.height/2, _button.width);
	}

	// UI functions
	// END


	// START
	// array helper functions

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
	
	function scaleDownArrayNew(width, height, targetArray, scaleFactor){
		var newArray = [(width / scaleFactor) * (height / scaleFactor)];
		
		for (var y = 0; y < height/scaleFactor; y++){
			for (var x = 0; x < width/scaleFactor; x++){
				newArray[(width/scaleFactor) * y + x] = targetArray[width * (y * scaleFactor) + (x * scaleFactor)];
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
	
	function createDrawArray(context){
		var width = context.canvas.width;
		var height = context.canvas.height;
		var imageData = context.getImageData(0, 0, width, height);
		
		var drawArray = [];
		for (var i = 3; i < (width*height*4); i += 4){
			drawArray.push(imageData.data[i]);
		}
		return drawArray;
	}

	// array helper functions
	// END


	function compareCanvas(contextTemplate, contextDraw){
		var arrayTemplate = createDrawArray(contextTemplate);
		var arrayDraw = createDrawArray(contextDraw);
		
		//alert(arrayTemplate.length + " & " + arrayDraw.length);
		
		//alert("test");
		
		var arrayLength = arrayTemplate.length || arrayDraw.length;
		
		//alert (arrayLength);
		
		var templatePixels = 0;
		var negativeTemplatePixels = 0;
		var drawnPixels = 0;
		var matchingPixels = 0;
		var mismatchingPixels = 0;
		
		for (var i = 0; i < arrayLength; i++){
			if (arrayTemplate[i] > 0){
				templatePixels++;
			} else {
				negativeTemplatePixels++;
			}
			
			if (arrayDraw[i] > 0){
				drawnPixels++;
			}
			
			if (arrayTemplate[i] > 0 && arrayDraw[i] > 0){
				matchingPixels++;
			}
			else {
				mismatchingPixels++;
			}
		}
		
		//alert(matchingPixels +"||"+ templatePixels +"||"+ drawnPixels +"||"+ matchingPixels +"||"+ negativeTemplatePixels);
		
		var accuracy = (matchingPixels / templatePixels) - (drawnPixels - matchingPixels)/negativeTemplatePixels;

		//alert(accuracy);

		return accuracy;
	}
	
	function drawLetterOnBgCanvas(){
		// draw the received letter on the background canvas
		for (var y = 0; y < bgCanvas.height; y++){
			for (var x = 0; x < bgCanvas.width; x++){
				// check for end of boundaries
				if ((y * currentLetterWidth + x) < currentLetter.length){
					if (currentLetter[y * currentLetterWidth + x] > 0){
						bgCtx.fillRect(x, y, 1, 1);
					}
				}
			}
		}
	}
	
	// no need for big int array transmissions; just render some font onto the canvas ;-)
	function drawLetterOnBgCanvasFromString(_string){
		clearCanvas(ctx);
		clearCanvas(bgCtx);
		
		_string = _string.toUpperCase();
		
		bgCtx.font = bgCtx.canvas.height * 0.75 + "px LetterFont";
		bgCtx.fillStyle = "white";
		bgCtx.textBaseline = "middle";
		bgCtx.textAlign = "center";
		bgCtx.fillText(_string, bgCtx.canvas.width/2, bgCtx.canvas.height/2);
	}
	
	function sendDrawArray(accuracy){
		var width = ctx.canvas.width;
		var height = ctx.canvas.height;
		
		// Get the image data from the drawCanvas
		var imageData = ctx.getImageData(0, 0, width, height);
		
		// Transfer alpha values to new "normal" array
		var drawArray = createDrawArray(ctx);

		var drawAccuracy = accuracy || defaultAccuracy;

		/*
		if (currentLetter != null){
			drawAccuracy = compareArrays(currentLetter, drawArray);
			alert(drawAccuracy);
		} else drawAccuracy = defaultAccuracy;*/
		
		if (currentLetter !== null || currentLetterAsString !== null){
			currentLetter = null;
			currentLetterAsString = null;
			clearCanvas(bgCtx);
			clearCanvas(ctx);
		}

		var sendArray = [];
		
		// Shrink down the array acording to drawArrayDivision parameter
		sendArray = scaleDownArray(width, height, drawArray, drawArrayDivision);
		
		// calculate new array dimensions
		width = width/drawArrayDivision;
		height = height/drawArrayDivision;
		
		// Send the data to the game server
		client.sendCmd('draw', {width: width, height: height, drawArray: sendArray, accuracy: drawAccuracy, gamemode: gameState});
	}

});

