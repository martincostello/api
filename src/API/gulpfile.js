/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    del = require("del"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    csslint = require("gulp-csslint"),
    jshint = require("gulp-jshint"),
    karmaServer = require("karma").Server,
    less = require("gulp-less"),
    lesshint = require("gulp-lesshint"),
    rename = require("gulp-rename"),
    sass = require("gulp-sass"),
    sassLint = require("gulp-sass-lint");

var webroot = "./wwwroot/Assets/";
var assets = "./Assets/";
var scripts = assets + "Scripts/";
var styles = assets + "Styles/";

var paths = {
    js: scripts + "js/**/*.js",
    jsDest: webroot + "js",
    minJs: webroot + "js/**/*.min.js",
    minJsDest: webroot + "js/site.min.js",
    testsJs: scripts + "js/**/*.spec.js",
    css: styles + "css/**/*.css",
    minCssDest: webroot + "css/**/site.min.css",
    concatJsDest: webroot + "js/site.js",
    concatCssDest: webroot + "css/site.css",
    less: styles + "/less/site.less",
    lessDest: webroot + "css",
    sass: styles + "/sass/site.scss",
    sassDest: webroot + "css",
    cssClean: webroot + "css/**/*.css",
    jsClean: webroot + "js/**/*.js"
};

gulp.task("clean:js", function () {
    return del([paths.jsClean]);
});

gulp.task("clean:css", function () {
    return del([paths.cssClean]);
});

gulp.task("clean", gulp.parallel("clean:js", "clean:css"));

gulp.task("css:less", function () {
    return gulp.src(paths.less, { allowEmpty: true })
      .pipe(less())
      .pipe(rename("less.css"))
      .pipe(gulp.dest(paths.lessDest));
});

gulp.task("css:sass", function () {
    return gulp.src(paths.sass, { allowEmpty: true })
      .pipe(sass().on("error", sass.logError))
      .pipe(rename("sass.css"))
      .pipe(gulp.dest(paths.sassDest));
});

gulp.task("css", gulp.series("css:less", "css:sass"));

gulp.task("lint:css", function () {
    return gulp.src(styles, { allowEmpty: true })
      .pipe(csslint())
      .pipe(csslint.formatter())
      .pipe(csslint.formatter('fail'));
});

gulp.task("lint:js", function () {
    return gulp.src(paths.js, { allowEmpty: true })
      .pipe(jshint())
      .pipe(jshint.reporter("default"))
      .pipe(jshint.reporter("fail"));
});

gulp.task("lint:less", function () {
    return gulp.src(paths.less, { allowEmpty: true })
        .pipe(lesshint())
        .pipe(lesshint.reporter());
});

gulp.task("lint:sass", function () {
    return gulp.src(paths.sass, { allowEmpty: true })
        .pipe(sassLint())
        .pipe(sassLint.format())
        .pipe(sassLint.failOnError());
});

gulp.task("lint", gulp.parallel("lint:js", "lint:less", "lint:sass", "lint:css"));

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs, "!" + paths.concatJsDest, "!" + paths.testsJs])
        .pipe(concat(paths.concatJsDest))
        .pipe(gulp.dest("."))
        .pipe(uglify())
        .pipe(rename(paths.minJsDest))
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([
            paths.lessDest + "/less.css",
            paths.sassDest + "/sass.css",
            paths.css,
            "!" + paths.minCss,
            "!" + paths.concatCssDest])
        .pipe(concat(paths.concatCssDest))
        .pipe(gulp.dest("."))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest("."));
});

gulp.task("min", gulp.parallel("min:js", "min:css"));

gulp.task("test:js:karma", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        singleRun: true
    }, done).start();
});

gulp.task("test:js:chrome", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        browsers: ["Chrome"]
    }, done).start();
});

gulp.task("test:js", gulp.series("test:js:karma"));
gulp.task("test", gulp.series("test:js"));

gulp.task("build", gulp.series("lint", "min"));
gulp.task("publish", gulp.series("build", "test"));

gulp.task("default", gulp.series("build"));
