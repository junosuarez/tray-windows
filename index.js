var edge = require('edge')
var extend = require('xtend')
var EventEmitter = require('events').EventEmitter
var icons = require('./icons')
var debug = require('debug')('tray-windows')
var path = require('path')

var raw = edge.func({source: path.resolve(__dirname, 'lib/tray.cs'), references: ['System.Drawing.dll','System.Windows.Forms.dll']})

function tray(opt, cb) {
  var ctx = new EventEmitter
  opt = opt || {}
  opt.icon = opt.icon || icons.grey()

  ctx.addMenuItem = function (item) {
    messages.push({
      e: 'add:menuItem',
      data: item
    })
    return this
  }

  ctx.delMenuItem = function (item) {
    messages.push({
      e: 'del:menuItem',
      data: item
    })
    return this
  }

  ctx.delMenuItemAt = function (index) {
    messages.push({
      e: 'del:menuItem:at',
      data: index
    })
    return this
  }

  ctx.exit = function () {
    messages.push({
      e: 'exit',
      data: null
    })
    // stop additional messages from being queued
    messages.push = function () {}
    return this
  }

  ctx.dropMenu = function () {
    messages.push({
      e: 'drop:menu',
      data: null
    })
    return this
  }

  ctx.updateIcon = function (buffer) {
    messages.push({
      e: 'updateIcon',
      data: buffer
    })
  }

  var messages = []

  var args = extend(opt, {
    eventHandler: eventHandler(ctx),
    checkMessage: check(messages)
  })

  ctx.on('start', function (send) {
    cb(null, ctx)
  })

  try {
    raw(args, function (err, r) {
      if (err) {
        ctx.emit('error', err)
      }
    })

  } catch (e) {
    e.message = 'Startup Error: ' + e.message
    cb(e)
  }
}

function check(messages) {
  return function (obj, cb) {
    var message = messages.shift()
    // var message = messages[0]
    cb(null, message)
  }
}

function eventHandler(ctx) {
  return function (obj, cb) {
    debug('event', obj)
    cb(null, "ok")
    ctx.emit(obj.e, obj.data)
  }
}

module.exports = tray
module.exports.icons = icons