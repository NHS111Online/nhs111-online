const path = require('path'),
    UglifyJSPlugin = require('uglifyjs-webpack-plugin'),
    Visualizer = require('webpack-visualizer-plugin')


module.exports = {
    entry: {
        'bundle-head': './src/js/entry-head.js',
        'bundle': './src/js/entry-main.js',
        'bundle-polyfills': './src/js/entry-polyfills.js'
    },
    output: {
        filename: '[name].js'
    },
    plugins: [
        new UglifyJSPlugin({ sourceMap: true }),
        new Visualizer({
            filename: '../../src/codebase/components/_jschart/jschart.njk'
        })
    ],
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /(node_modules|bower_components)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['env'],
                    }
                }
            }
        ]
    }
}
