var edge = require('edge')
var path = require('path')
var extend = require('xtend')
var EventEmitter = require('events').EventEmitter

var raw = edge.func({source: 'lib/tray.cs', references: ['System.Drawing.dll','System.Windows.Forms.dll']})

function tray(opt, cb) {
  var ctx = new EventEmitter
  var args = extend(opt, {
    eventHandler: eventHandler(ctx)
  })

  ctx.on('start', function (send) {
    console.log(send)
    send('hi', function (e, r) {
      console.log('<-', arguments)
    })
    send('hi2', function (e, r) {
      console.log('<-', arguments)
    })
    // setInterval(function () {
    //   send({a:'asds'}, function () {
    //     console.log(arguments)
    //   })
    // }, 500)
    cb(null, ctx)
  })

  try {
    raw(args, function (err, r) {
      if (err) {
        ctx.emit('error', err)
      }
      console.log('ret', r)
    })

  } catch (e) {
    e.message = 'Startup Error: ' + e.message
    cb(e)
  }
}

function eventHandler(ctx) {
  return function (obj, cb) {
    console.log('ev', obj)
    cb(null, "ok")
    ctx.emit(obj.e, obj.data)
  }
}

module.exports = tray
