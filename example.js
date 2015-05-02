var tray = require('./.')

tray({
  name: 'jden',
  items: ['+','--', 'C', '-', 'Exit'],
  icon: tray.icons.grey()
}, function (err, app) {
  if (err) { return console.log('e', err) }
  console.log('app running')

  app.on('click:menuItem', function (menuItem) {
    console.log('clicked',menuItem)
  

    if (menuItem.text === '+') {
      app.addMenuItem('bob')
    }
    if (menuItem.text === '--') {
      app.delMenuItem('bob')
    }
    if (menuItem.text === 'Exit') {
      app.exit()
    }

    // app.delMenuItemAt(menuItem.index)
    if (menuItem.text === 'C') {
      app.dropMenu()
    }

  })


var white = tray.icons.white()
var black = tray.icons.black()
var i = 0
setInterval(function () {
  if (i++ % 2) {
    app.updateIcon(white)
  } else {
    app.updateIcon(black)
  }
}, 1000)




})