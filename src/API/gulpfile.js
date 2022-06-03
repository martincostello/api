/// <binding Clean='clean' />
'use strict'

var gulp = require('gulp');
var del = require('del');
var concat = require('gulp-concat');
var cssmin = require('gulp-cssmin');
var uglify = require('gulp-uglify');
var csslint = require('gulp-csslint');
var jshint = require('gulp-jshint');
var rename = require('gulp-rename');

var webroot = './wwwroot/assets/';
var assets = './assets/';
var scripts = assets + 'scripts/';
var styles = assets + 'styles/';

var paths = {
    js: scripts + 'js/**/*.js',
    jsDest: webroot + 'js',
    minJs: webroot + 'js/**/*.min.js',
    minJsDest: webroot + 'js/site.min.js',
    css: styles + 'css/**/*.css',
    minCssDest: webroot + 'css/**/site.min.css',
    concatJsDest: webroot + 'js/site.js',
    concatCssDest: webroot + 'css/site.css',
    cssClean: webroot + 'css/**/*.css',
    jsClean: webroot + 'js/**/*.js'
};

gulp.task('clean:js', function () {
    return del([paths.jsClean]);
});

gulp.task('clean:css', function () {
  return del([paths.cssClean]);
});

gulp.task('clean', gulp.parallel('clean:js', 'clean:css'));

gulp.task('lint:css', function () {
  return gulp.src(styles, { allowEmpty: true })
    .pipe(csslint())
    .pipe(csslint.formatter())
    .pipe(csslint.formatter('fail'));
});

gulp.task('lint:js', function () {
  return gulp.src(paths.js, { allowEmpty: true })
    .pipe(jshint())
    .pipe(jshint.reporter('default'))
    .pipe(jshint.reporter('fail'));
});

gulp.task('lint', gulp.parallel('lint:js', 'lint:css'));

gulp.task('min:js', function () {
  return gulp.src([paths.js, '!' + paths.minJs, '!' + paths.concatJsDest])
    .pipe(concat(paths.concatJsDest))
    .pipe(gulp.dest('.'))
    .pipe(uglify())
    .pipe(rename(paths.minJsDest))
    .pipe(gulp.dest('.'));
});

gulp.task('min:css', function () {
  return gulp.src([
      paths.css,
      '!' + paths.minCss,
      '!' + paths.concatCssDest])
    .pipe(concat(paths.concatCssDest))
    .pipe(gulp.dest('.'))
    .pipe(cssmin())
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest('.'));
});

gulp.task('min', gulp.parallel('min:js', 'min:css'));

gulp.task('build', gulp.series('lint', 'min'));
gulp.task('default', gulp.series('build'));
