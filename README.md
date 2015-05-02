# tray-windows
npm module to create system tray apps on Windows

## Installation
```
> npm install --save tray-windows
```


## Usage
See `example.js` for more
```js
var tray = require('tray-windows')

tray({
  name: 'Tooltip title',
  items: ['A','B','C'],
  icon: tray.icons.green()
}, function (err, app) {
  if (err) { throw err }

  app.on('click:menuItem', function (item) {
    console.log(item)
    // {index: 0, text: 'A'}
  })

  app.addMenuItem('D')
  app.delMenuItem('B')
  app.delMenuItemAt(1) // 0-based index
  app.dropMenu() // remove all menu items
  app.exit() // close the tray app; node still running

  app.updateIcon(tray.icons.red())
})
```


## Icons
Icons should be Buffers of 20x20 PNGs (alpha ok).
Basic icons are included for the following colors:

Tray.icons.red()
Tray.icons.yellow()
Tray.icons.green()
Tray.icons.black()
Tray.icons.white()
Tray.icons.grey()

The PSD used to produce these icons is included in the git repo in `icons/icon.psd`


## Compatibility
This module was inspired by [tray](https://github.com/brandonhorst/node-tray) for OS X. If you're trying to
write something that will work cross-platform, you can try the compatibility layer:

```js
var createTray = require('tray-windows/compat')
var createApp = function (cb) {
  // since `native-app`is only for OSX, we don't depend on it-
  // this function isn't needed, but shown here mocked out
  // to use the example code
  cb(null, null)
}

// sample taken verbatim from `tray` readme:

createApp(function (err, app) {
  createTray(app, function (err, tray) {
    tray.specify({
      title: 'Hello, world!',
      menuItems: [
        {title: 'Informational'},
        {
          title: 'Do something',
          shortcut: 'x',
          action: function () { console.log('You pressed a menuItem!') }
        }
      ]
    })
  })
})

```

## Requirements

- Node
- Windows
- .Net 4.5+

Tested on Windows 8 x64. If you try another configuration, please let me know in an issue if it worked or not.


## Contributing

Contributions welcome! Please see CONTRIBUTING.md

- jden <jason@denizac.org>

## License

(c) MMXV jden. ISC License. See LICENSE.md.