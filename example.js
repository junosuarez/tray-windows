var tray = require('./.')

tray({
  name: 'jden',
  items: ['A','B', 'C', '-', 'E&xit'],
  icon: require('fs').readFileSync('icon.png')
}, function (err, app) {
  if (err) { return console.log('e', err) }
  console.log('app running')

  app.on('click:menuItem', function (menuItem) {
    console.log('clicked',menuItem)
  })

})