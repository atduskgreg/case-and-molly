#!/usr/bin/env node
// var http = require('http');

//HTTP part
// var server = http.createServer(function(req, res){
//   res.writeHead(200,{ 'Content-Type': 'text/html' });
//   console.log("web request");
//   res.end('<h1>Try the same on socket!</h1>');
// });
// server.listen(8080);

// Socket part
var WebSocketServer = require('ws').Server
  , wss = new WebSocketServer({port: 80});

var on = false;

var clients = [];

wss.on('connection', function(ws) {
	console.log("socket connection");
    ws.on('message', function(message) {
    	if(message == "join"){
    		clients.push(ws);
    	}
    	if(message == "write"){
        	on = !on;
        	console.log('t: %s', on);
        	for(var i = 0; i< clients.length; i++){
        		clients[i].send("{\"t\":"+ on+ "}");
        	}
    	} 
    	
    });
});
