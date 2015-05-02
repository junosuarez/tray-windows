var tray = require('./index')

module.exports = function createTray(app, cb) {
  // app isn't used, but kept for api compatibility with [node-tray](https://github.com/brandonhorst/node-tray)
  // for that matter, we don't need this initial callback, since our "specify" is done during init
  var state = {closed: false, map: {}}

  function set(opt) {
    state.app.dropMenu()
    state.map = {}
    opt.menuItems.forEach(function (item) {
      state.map[item.title] = item
      state.app.addMenuItem(item.title)
    }
  }

  cb(null, {
    specify: function (opt) {

      if (!state.app) {
        state.app = tray({
          name: opt.title,
          items: []
        }, function (err, app) {
          if (err) { throw err }
          app.on('click:menuItem', function (menuItem) {
            var item = state.map[menuItem]
            if (item.action) {
              item.action(item.data)
            }
          })
        })
      }

      set(opt)
    },
    close: function () {
      if (!state.closed) {
        if (state.app) {
          state.app.exit()
          delete state.app
        }
        state.closed = true
      } else {
        throw new Error('App closed')
      }
    }
  })
}