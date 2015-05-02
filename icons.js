var file = function (path) { return function () { return require('fs').readFileSync(path); } }
module.exports = {
  red: file(__dirname + '/icons/red.png'),
  yellow: file(__dirname + '/icons/yellow.png'),
  green: file(__dirname + '/icons/green.png'),
  black: file(__dirname + '/icons/black.png'),
  white: file(__dirname + '/icons/white.png'),
  grey: file(__dirname + '/icons/grey.png'),
}